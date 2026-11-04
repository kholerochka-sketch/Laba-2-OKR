const API_URL = "/api";
const API_BASE_URL = API_URL;

document.addEventListener('DOMContentLoaded', () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilters();
    setupModal();
    setupReviewForm();
    setupAdminPanel(); // Ініціалізація адмін-панелі (Фіча F5)
    loadAllReviews();
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

// Завантаження списку місць (F1 + F2) з кнопкою видалення для адміна (F5)
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

        const adminKey = document.getElementById('admin-key-input')?.value || '';

        placesList.innerHTML = places.map(p => {
            const placeId = p.id || p._id;
            return `
            <div class="card" style="position: relative;">
                <h3>${p.name}</h3>
                <p><strong>Категорія:</strong> ${p.category}</p>
                <p>${p.description ? p.description.substring(0, 100) + '...' : ''}</p>
                <button class="details-btn" onclick='openModal(${JSON.stringify(p)})'>Детальніше</button>
                ${adminKey ? `<button onclick="deletePlace('${placeId}')" style="margin-top: 10px; background: #c0392b; color: white; border: none; padding: 5px 10px; border-radius: 4px; cursor: pointer; display: block;">Видалити місце 🗑️</button>` : ''}
            </div>
        `;
        }).join('');
    } catch (error) {
        console.error(error);
        placesList.innerHTML = '<p style="color: red;">Не вдалося завантажити місця.</p>';
    }
}

// Завантаження порад (F3)
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
                <p>${t.content || t.text}</p>
            </div>
        `).join('');
    } catch (error) {
        tipsList.innerHTML = '<p style="color: #666;">Поради тимчасово недоступні.</p>';
    }
}

// Фільтри (F2)
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

// Модальне вікно
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

window.openModal = function (place) {
    const modal = document.getElementById('placeModal');
    const modalDetails = document.getElementById('modalDetails');
    if (!modal || !modalDetails) return;

    window.currentPlaceName = place.name;

    modalDetails.innerHTML = `
        <h2>${place.name}</h2>
        <p><strong>Категорія:</strong> ${place.category}</p>
        <p>${place.description}</p>
        ${place.mapUrl ? `<p><a href="${place.mapUrl}" target="_blank">Відкрити на карті 🗺️</a></p>` : ''}
    `;

    modal.style.display = 'block';
};

// Завантаження всіх відгуків (F4)
async function loadAllReviews() {
    const container = document.getElementById('reviews-container');
    if (!container) return;

    container.innerHTML = '<p style="color: #666;">Завантаження відгуків...</p>';
    try {
        const response = await fetch('/api/feedback');
        if (!response.ok) throw new Error('Помилка завантаження відгуків');

        const reviews = await response.json();

        if (reviews.length === 0) {
            container.innerHTML = '<p style="color: #666;">Ще немає відгуків. Оберіть місце на карті та залиште перший!</p>';
            return;
        }

        container.innerHTML = reviews.map(r => `
            <div style="background: #f9f9f9; padding: 14px; border-radius: 6px; margin-bottom: 12px; border: 1px solid #e0e0e0;">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px;">
                    <strong>${r.name}</strong>
                    <span style="color: #f39c12; font-size: 14px;">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</span>
                </div>
                <p style="margin: 6px 0; color: #333; font-size: 15px;">${r.message}</p>
                <small style="color: #999; font-size: 11px;">${new Date(r.createdAt).toLocaleDateString()}</small>
            </div>
        `).join('');
    } catch (error) {
        console.error(error);
        container.innerHTML = '<p style="color: red;">Не вдалося завантажити відгуки.</p>';
    }
}

// Відправка відгуку (F4)
function setupReviewForm() {
    const form = document.getElementById('review-form');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const placeName = window.currentPlaceName || "Загальне місце";
        const userMessage = document.getElementById('review-text').value;
        const formattedMessage = `📍 [Місце: ${placeName}] — ${userMessage}`;

        const newReview = {
            name: document.getElementById('reviewer-name').value,
            email: document.getElementById('reviewer-email').value,
            rating: parseInt(document.getElementById('review-rating').value),
            message: formattedMessage
        };

        try {
            const response = await fetch('/api/feedback', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newReview)
            });

            if (response.ok) {
                form.reset();
                const modal = document.getElementById('placeModal');
                if (modal) modal.style.display = 'none';
                loadAllReviews();
                alert('Дякуємо! Ваш відгук успішно додано.');
            } else {
                alert('Помилка при відправці відгуку.');
            }
        } catch (error) {
            console.error(error);
            alert('Помилка мережі.');
        }
    });
}

// АДМІН-ПАНЕЛЬ: Додавання та видалення місць (Фіча F5)
function setupAdminPanel() {
    const form = document.getElementById('add-place-form');
    const adminKeyInput = document.getElementById('admin-key-input');

    if (adminKeyInput) {
        adminKeyInput.addEventListener('input', () => {
            loadPlaces(); // Перезавантажуємо список місць, щоб з'явилися кнопки видалення, якщо ключ введено вірно
        });
    }

    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const adminKey = adminKeyInput ? adminKeyInput.value.trim() : '';
        if (!adminKey) {
            alert('Будь ласка, введіть адмін-ключ вище!');
            return;
        }

        const newPlace = {
            name: document.getElementById('place-name').value,
            category: document.getElementById('place-category').value,
            description: document.getElementById('place-desc').value,
            mapUrl: document.getElementById('place-map').value || null
        };

        try {
            const response = await fetch('/api/places', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Admin-Key': adminKey
                },
                body: JSON.stringify(newPlace)
            });

            if (response.ok) {
                form.reset();
                loadPlaces();
                alert('Місце успішно додано через адмін-панель! 🎉');
            } else if (response.status === 401) {
                alert('Неправильний адмін-ключ (X-Admin-Key)!');
            } else {
                alert('Помилка при додаванні місця.');
            }
        } catch (error) {
            console.error(error);
            alert('Помилка мережі.');
        }
    });
}

// Функція видалення місця для адміна (F5)
window.deletePlace = async function (id) {
    const adminKey = document.getElementById('admin-key-input')?.value.trim() || '';
    if (!adminKey) {
        alert('Введіть адмін-ключ у полі зверху!');
        return;
    }

    if (!confirm('Ви впевнені, що хочете видалити це місце?')) return;

    try {
        const response = await fetch(`/api/places/${id}`, {
            method: 'DELETE',
            headers: {
                'X-Admin-Key': adminKey
            }
        });

        if (response.ok) {
            loadPlaces();
            alert('Місце успішно видалено!');
        } else if (response.status === 401) {
            alert('Неправильний адмін-ключ!');
        } else {
            alert('Не вдалося видалити місце.');
        }
    } catch (error) {
        console.error(error);
        alert('Помилка мережі.');
    }
};