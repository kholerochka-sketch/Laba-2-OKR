const API_URL = "/api";
const API_BASE_URL = API_URL; // Щоб уникнути помилок у старих функціях

document.addEventListener('DOMContentLoaded', () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilters();
    setupModal();
    setupReviewForm();
});

// Перевірка стану системи (F0)
async function checkHealth() {
    const statusBox = document.getElementById('health-status');
    if (!statusBox) return;
    try {
        const response = await fetch('/api/health');
        if (response.ok) {
            statusBox.textContent = 'API працює стабільно ✅';
            statusBox.style.backgroundColor = '#d4edda';
            statusBox.style.color = '#155724';
        } else {
            statusBox.textContent = 'Помилка сервера ❌';
        }
    } catch (error) {
        statusBox.textContent = 'Сервер не відповідає ❌';
        statusBox.style.backgroundColor = '#f8d7da';
        statusBox.style.color = '#721c24';
    }
}

// Завантаження списку місць з урахуванням фільтрів (F1 + F2)
async function loadPlaces(category = '') {
    const placesList = document.getElementById('places-list');
    if (!placesList) return;

    placesList.innerHTML = '<p class="placeholder">Дані завантажуються...</p>';
    try {
        let url = '/api/places';
        if (category) {
            url += `?category=${category}`;
        }
        const response = await fetch(url);
        if (!response.ok) throw new Error('Помилка завантаження місць');
        const places = await response.json();

        if (places.length === 0) {
            placesList.innerHTML = '<p>Місць у цій категорії поки немає.</p>';
            return;
        }

        placesList.innerHTML = places.map(p => `
            <div class="card">
                <h3>${p.name}</h3>
                <p><strong>Категорія:</strong> ${p.category}</p>
                <p>${p.description ? p.description.substring(0, 100) + '...' : ''}</p>
                <button class="details-btn" onclick='openModal(${JSON.stringify(p)})'>Детальніше</button>
            </div>
        `).join('');
    } catch (error) {
        console.error(error);
        placesList.innerHTML = '<p style="color: red;">Не вдалося завантажити місця.</p>';
    }
}

// Завантаження порад для походу (F3)
async function loadTips() {
    const tipsList = document.getElementById('tips-list');
    if (!tipsList) return;
    try {
        const response = await fetch('/api/tips');
        if (!response.ok) return;
        const tips = await response.json();
        tipsList.innerHTML = tips.map(t => `
            <div class="card">
                <h3>${t.title}</h3>
                <p>${t.content}</p>
            </div>
        `).join('');
    } catch (error) {
        tipsList.innerHTML = '<p style="color: #666;">Поради тимчасово недоступні.</p>';
    }
}

// Налаштування кнопок фільтрації (F2)
function setupFilters() {
    const buttons = document.querySelectorAll('.filter-btn');
    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const category = btn.getAttribute('data-category');
            loadPlaces(category);
        });
    });
}

// Управління модальним вікном
function setupModal() {
    const modal = document.getElementById('placeModal');
    const closeBtn = document.querySelector('.close-btn');
    if (!modal) return;

    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            modal.style.display = 'none';
        });
    }

    window.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.style.display = 'none';
        }
    });
}

// Відкриття модального вікна з деталями місця та завантаженням відгуків (F2 + F4)
window.openModal = function (place) {
    const modal = document.getElementById('placeModal');
    const modalDetails = document.getElementById('modalDetails');
    if (!modal || !modalDetails) return;

    window.currentPlaceId = place.id || place._id;

    modalDetails.innerHTML = `
        <h2>${place.name}</h2>
        <p><strong>Категорія:</strong> ${place.category}</p>
        <p>${place.description}</p>
        ${place.mapUrl ? `<p><a href="${place.mapUrl}" target="_blank">Відкрити на карті 🗺️</a></p>` : ''}
    `;

    modal.style.display = 'block';

    // Завантажуємо відгуки для цього місця (Фіча F4)
    loadReviews(window.currentPlaceId);
};

// Завантаження відгуків для конкретного місця (Фіча F4)
async function loadReviews(placeId) {
    const container = document.getElementById('reviews-container');
    if (!container) return;

    container.innerHTML = '<p style="color: #666;">Завантаження відгуків...</p>';
    try {
        const response = await fetch(`/api/places/${placeId}/reviews`);
        if (!response.ok) throw new Error('Помилка завантаження відгуків');

        const reviews = await response.json();

        if (reviews.length === 0) {
            container.innerHTML = '<p style="color: #666;">Ще немає відгуків. Будьте першим!</p>';
            return;
        }

        container.innerHTML = reviews.map(r => `
            <div style="background: #f9f9f9; padding: 10px; border-radius: 6px; margin-bottom: 10px;">
                <strong>${r.userName}</strong> — <span style="color: #f39c12;">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</span>
                <p style="margin: 5px 0 0 0; color: #333;">${r.comment}</p>
                <small style="color: #999; font-size: 11px;">${new Date(r.createdAt).toLocaleDateString()}</small>
            </div>
        `).join('');
    } catch (error) {
        console.error(error);
        container.innerHTML = '<p style="color: red;">Не вдалося завантажити відгуки.</p>';
    }
}

// Відправка нового відгуку на бекенд (Фіча F4)
function setupReviewForm() {
    const form = document.getElementById('review-form');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const placeId = window.currentPlaceId;
        if (!placeId) return;

        const newReview = {
            userName: document.getElementById('reviewer-name').value,
            rating: parseInt(document.getElementById('review-rating').value),
            comment: document.getElementById('review-text').value
        };

        try {
            const response = await fetch(`/api/places/${placeId}/reviews`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newReview)
            });

            if (response.ok) {
                form.reset();
                loadReviews(placeId); // Оновлюємо список відгуків на екрані
            } else {
                alert('Помилка при відправці відгуку.');
            }
        } catch (error) {
            console.error('Помилка мережі:', error);
        }
    });
}