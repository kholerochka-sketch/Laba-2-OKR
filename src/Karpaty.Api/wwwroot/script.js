
const API_URL = "/api";

document.addEventListener('DOMContentLoaded', () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilters();
    setupModal();
    setupReviewForm();
    loadAllReviews();
});

// Перевірка стану API
async function checkHealth() {
    const statusBox = document.getElementById('health-status');
    if (!statusBox) return;

    try {
        const response = await fetch(`${API_URL}/health`);

        if (response.ok) {
            statusBox.textContent = 'API працює стабільно ✅';
            statusBox.style.backgroundColor = '#d4edda';
            statusBox.style.color = '#155724';
        } else {
            statusBox.textContent = 'Помилка сервера ❌';
        }
    } catch (error) {
        console.error(error);
        statusBox.textContent = 'Сервер не відповідає ❌';
        statusBox.style.backgroundColor = '#f8d7da';
        statusBox.style.color = '#721c24';
    }
}

// Завантаження місць
async function loadPlaces(category = '') {
    const placesList = document.getElementById('places-list');
    if (!placesList) return;

    placesList.innerHTML = '<p class="placeholder">Дані завантажуються...</p>';

    try {
        let url = `${API_URL}/places`;

        if (category) {
            url += `?category=${encodeURIComponent(category)}`;
        }

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error('Помилка завантаження місць');
        }

        const places = await response.json();

        if (!places.length) {
            placesList.innerHTML = '<p>Місць у цій категорії поки немає.</p>';
            return;
        }

        placesList.innerHTML = places.map(p => `
            <div class="card">
                <h3>${p.name}</h3>
                <p><strong>Категорія:</strong> ${p.category}</p>
                <p>${p.description ? p.description.substring(0, 100) + '...' : ''}</p>
                <button class="details-btn"
                    onclick='openModal(${JSON.stringify(p)})'>
                    Детальніше
                </button>
            </div>
        `).join('');

    } catch (error) {
        console.error(error);
        placesList.innerHTML =
            '<p style="color:red;">Не вдалося завантажити місця.</p>';
    }
}

// Завантаження порад
async function loadTips() {
    const tipsList = document.getElementById('tips-list');
    if (!tipsList) return;

    try {
        const response = await fetch(`${API_URL}/tips`);

        if (!response.ok) {
            throw new Error('Помилка завантаження порад');
        }

        const tips = await response.json();

        tipsList.innerHTML = tips.map(t => `
            <div class="card">
                <h3>${t.title}</h3>
                <p>${t.content || t.text || ''}</p>
            </div>
        `).join('');

    } catch (error) {
        console.error(error);
        tipsList.innerHTML =
            '<p style="color:#666;">Поради тимчасово недоступні.</p>';
    }
}

// Фільтрація місць
function setupFilters() {
    const buttons = document.querySelectorAll('.filter-btn');

    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            loadPlaces(btn.getAttribute('data-category') || '');
        });
    });
}

// Налаштування модального вікна
function setupModal() {
    const modal = document.getElementById('place-modal');
    const closeBtn = document.getElementById('close-modal');

    if (!modal) return;

    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            modal.style.display = 'none';
        });
    }

    window.addEventListener('click', event => {
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    });
}

// Відкриття деталей місця
window.openModal = function(place) {
    const modal = document.getElementById('place-modal');
    const modalDetails = document.getElementById('modalDetails');

    if (!modal || !modalDetails) return;

    window.currentPlaceName = place.name;

    modalDetails.innerHTML = `
        <h2>${place.name}</h2>
        <p><strong>Категорія:</strong> ${place.category}</p>
        <p>${place.description || ''}</p>
        ${place.mapUrl
            ? `<p><a href="${place.mapUrl}" target="_blank" rel="noopener noreferrer">Відкрити на карті 🗺️</a></p>`
            : ''}
    `;

    modal.style.display = 'block';
};

// Завантаження всіх відгуків
async function loadAllReviews() {
    const container = document.getElementById('reviews-container');
    if (!container) return;

    container.innerHTML = '<p>Завантаження відгуків...</p>';

    try {
        const response = await fetch(`${API_URL}/feedback`);

        if (!response.ok) {
            throw new Error('Помилка завантаження відгуків');
        }

        const reviews = await response.json();

        if (!reviews.length) {
            container.innerHTML =
                '<p>Ще немає відгуків. Оберіть місце та залиште перший!</p>';
            return;
        }

        container.innerHTML = reviews.map(r => `
            <div class="card">
                <strong>${r.name || 'Мандрівник'}</strong>
                <span style="color:#f39c12;">
                    ${'★'.repeat(Math.max(0, Math.min(5, Number(r.rating) || 0)))}
                </span>
                <p>${r.message || ''}</p>
                <small>${r.createdAt
                    ? new Date(r.createdAt).toLocaleString('uk-UA')
                    : ''}</small>
            </div>
        `).join('');

    } catch (error) {
        console.error(error);
        container.innerHTML =
            '<p style="color:red;">Не вдалося завантажити відгуки.</p>';
    }
}

// Налаштування форми відгуків
function setupReviewForm() {
    const form = document.getElementById('review-form');
    if (!form) return;

    form.addEventListener('submit', async function(event) {
        event.preventDefault();

        const placeName = window.currentPlaceName || 'Загальне місце';
        const userMessage = document.getElementById('review-text').value.trim();

        const newReview = {
            name: document.getElementById('reviewer-name').value.trim(),
            email: document.getElementById('reviewer-email').value.trim(),
            rating: parseInt(
                document.getElementById('review-rating').value,
                10
            ),
            message: `📍 [Місце: ${placeName}] — ${userMessage}`
        };

        try {
            const response = await fetch(`${API_URL}/feedback`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newReview)
            });

            if (response.ok) {
                form.reset();

                const modal = document.getElementById('place-modal');
                if (modal) {
                    modal.style.display = 'none';
                }

                window.currentPlaceName = null;

                await loadAllReviews();

                alert('Дякуємо! Ваш відгук успішно додано.');

            } else if (response.status === 429) {
                alert('Забагато запитів! Зачекайте хвилину.');

            } else {
                const errorData = await response.json().catch(() => ({}));
                alert(
                    'Помилка при відправці відгуку: ' +
                    (errorData.error || 'невірні дані')
                );
            }

        } catch (error) {
            console.error('Помилка мережі:', error);
            alert('Сталася помилка мережі при надсиланні відгуку.');
        }
    });
}
