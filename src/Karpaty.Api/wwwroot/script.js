const API_URL = "/api";
let allPlacesData = [];

document.addEventListener('DOMContentLoaded', () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilters();
    setupModal();
    setupReviewForm();
    setupAdminPanel();
    loadAllReviews();
});

const categoryTranslations = {
    'waterfall': 'Водоспад',
    'viewpoint': 'Оглядовий майданчик',
    'meadow': 'Полонина'
};

const difficultyTranslations = {
    'easy': 'Легкий',
    'medium': 'Середній',
    'hard': 'Важкий'
};

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

// Залізобетонна функція для різних та унікальних картинок
function getPlaceImage(place) {
    // Якщо користувач ввів справжнє посилання через адмінку — беремо його
    if (place.imageUrl && typeof place.imageUrl === 'string' && place.imageUrl.trim() !== "" && place.imageUrl.startsWith('http')) {
        return place.imageUrl.trim();
    }

    const nameLower = (place.name || "").toLowerCase();

    // Унікальні фото під конкретні місця з високою стабільністю завантаження
    if (nameLower.includes('довбуш')) {
        return 'https://images.unsplash.com/photo-1599813233827-0ffb8214f475?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('піп іван') || nameLower.includes('піпіван') || nameLower.includes('чорног')) {
        return 'https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('женек') || nameLower.includes('гук')) {
        return 'https://images.unsplash.com/photo-1432821596592-e2c18b78144f?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('боржав')) {
        return 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('петрос')) {
        return 'https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('руна') || nameLower.includes('рівна')) {
        return 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('синевир')) {
        return 'https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('шипіт')) {
        return 'https://images.unsplash.com/photo-1546182990-dffeafbe841d?auto=format&fit=crop&w=600&q=80';
    } else if (nameLower.includes('говерла')) {
        return 'https://images.unsplash.com/photo-1454496522488-7a8e488e8606?auto=format&fit=crop&w=600&q=80';
    }

    // Якщо назва не розпізнана, підбираємо унікальну картинку за хешем назви (щоб кожне нове місце мало гарантовано свою унікальну фотографію, а не однакову!)
    const collectionPool = [
        'https://images.unsplash.com/photo-1511593358241-7eea1f3c84e5?auto=format&fit=crop&w=600&q=80',
        'https://images.unsplash.com/photo-1470071459604-3b5ec3a7fe05?auto=format&fit=crop&w=600&q=80',
        'https://images.unsplash.com/photo-1426604966848-d7adacbd02bff?auto=format&fit=crop&w=600&q=80',
        'https://images.unsplash.com/photo-1501785888041-af3ef285b470?auto=format&fit=crop&w=600&q=80',
        'https://images.unsplash.com/photo-1448375240586-882707db888b?auto=format&fit=crop&w=600&q=80'
    ];

    let hash = 0;
    for (let i = 0; i < (place.name || "").length; i++) {
        hash = place.name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % collectionPool.length;
    return collectionPool[index];
}

async function loadPlaces(category = '') {
    const placesList = document.getElementById('places-list');
    if (!placesList) return;

    placesList.innerHTML = '<p class="placeholder" role="status">Дані завантажуються...</p>';
    try {
        let url = '/api/places';
        if (category) {
            url += `?category=${category}`;
        }
        const response = await fetch(url);
        if (response.status === 404) {
            placesList.innerHTML = `
                <div class="not-found-container" role="alert">
                    <h3 class="not-found-title">🔍 Нічого не знайдено</h3>
                    <p class="not-found-text">Категорію не знайдено або ресурс відсутній.</p>
                </div>`;
            return;
        }
        if (!response.ok) throw new Error('Помилка завантаження місць');

        const places = await response.json();
        allPlacesData = places;

        if (!Array.isArray(places) || places.length === 0) {
            placesList.innerHTML = `
                <div class="not-found-container" role="alert">
                    <h3 class="not-found-title">⛰️ Порожньо</h3>
                    <p class="not-found-text">Місць у цій категорії поки немає.</p>
                </div>`;
            return;
        }

        const adminKey = document.getElementById('admin-key-input')?.value || '';

        placesList.innerHTML = places.map(p => {
            const placeId = p.id || p._id;
            const safeName = escapeHtml(p.name);
            const safeCategory = categoryTranslations[p.category] || escapeHtml(p.category);
            const rawRegion = p.region || "";
            const formattedRegion = escapeHtml(rawRegion.replace(/_/g, ' '));
            const safeDesc = p.description ? escapeHtml(p.description.substring(0, 95)) + '...' : '';
            const imageUrl = getPlaceImage(p);

            return `
            <div class="card">
                <div class="card-img-container">
                    <img src="${imageUrl}" alt="${safeName}" onerror="this.onerror=null; this.src='https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=600&q=80'">
                    <span class="category-tag">${safeCategory}</span>
                </div>
                <div class="card-body">
                    <h3>${safeName}</h3>
                    <p class="region-info"><strong>Регіон:</strong> ${formattedRegion}</p>
                    <p class="desc-info">${safeDesc}</p>
                    <div class="card-footer">
                        <button class="details-btn" onclick="openModalById('${placeId}')">Детальніше</button>
                        ${adminKey ? `<button class="delete-btn" onclick="deletePlace('${placeId}')" title="Видалити">🗑️</button>` : ''}
                    </div>
                </div>
            </div>
        `;
        }).join('');
    } catch (error) {
        console.error(error);
        placesList.innerHTML = `
            <div class="error-container" role="alert">
                <h3 class="not-found-title">⚠️ Помилка</h3>
                <p class="not-found-text">Не вдалося завантажити місця. Перевірте з'єднання з сервером.</p>
                <button class="btn btn-primary" onclick="loadPlaces()">Спробувати ще раз</button>
            </div>`;
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>'"]/g,
        tag => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[tag] || tag)
    );
}

// Кольорові поради (F3)
async function loadTips() {
    const tipsList = document.getElementById('tips-list');
    if (!tipsList) return;
    try {
        const response = await fetch('/api/tips');
        if (!response.ok) return;
        const tips = await response.json();

        if (!Array.isArray(tips) || tips.length === 0) {
            tipsList.innerHTML = '<p style="color: #666;">Порад поки немає.</p>';
            return;
        }

        tipsList.innerHTML = tips.map(t => `
            <div class="card tip-card">
                <h3>🌲 ${escapeHtml(t.title)}</h3>
                <p>${escapeHtml(t.content || t.text)}</p>
            </div>
        `).join('');
    } catch (error) {
        tipsList.innerHTML = '<p style="color: #666;">Поради тимчасово недоступні.</p>';
    }
}

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

    window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'block') {
            modal.style.display = 'none';
        }
    });
}

window.openModalById = function (placeId) {
    const place = allPlacesData.find(p => (p.id || p._id) === placeId);
    if (!place) return;

    const modal = document.getElementById('placeModal');
    const modalDetails = document.getElementById('modalDetails');
    if (!modal || !modalDetails) return;

    window.currentPlaceName = place.name;

    const imageUrl = getPlaceImage(place);
    const formattedRegion = place.region ? place.region.replace(/_/g, ' ') : '';
    const safeCategory = categoryTranslations[place.category] || place.category;
    const safeDifficulty = difficultyTranslations[place.difficulty] || place.difficulty;

    const lat = place.latitude;
    const lng = place.longitude;
    const googleMapsUrl = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;

    modalDetails.innerHTML = `
        <h2 style="margin-top:0; color: #1b4332; font-size: 1.6rem; margin-bottom: 15px;">${escapeHtml(place.name)}</h2>
        <img src="${imageUrl}" alt="${escapeHtml(place.name)}" class="modal-img" onerror="this.src='https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=600&q=80'" />
        <div class="modal-info-grid">
            <div class="info-badge">📍 <strong>Регіон:</strong> ${escapeHtml(formattedRegion)}</div>
            <div class="info-badge">🏷️ <strong>Категорія:</strong> ${escapeHtml(safeCategory)}</div>
            <div class="info-badge">⚡ <strong>Складність:</strong> ${escapeHtml(safeDifficulty)}</div>
            <div class="info-badge">🌐 <strong>Координати:</strong> ${lat}, ${lng}</div>
        </div>
        <p class="modal-description">${escapeHtml(place.description)}</p>
        <p><a href="${googleMapsUrl}" target="_blank" rel="noopener noreferrer" class="map-link">Відкрити місце на Google Картах 🗺️</a></p>
    `;

    modal.style.display = 'block';
};

// Відгуки (F4)
async function loadAllReviews() {
    const container = document.getElementById('reviews-container');
    if (!container) return;

    container.innerHTML = '<p style="color: #666;">Завантаження відгуків...</p>';
    try {
        const response = await fetch('/api/feedback');
        if (!response.ok) throw new Error('Помилка завантаження відгуків');

        const reviews = await response.json();

        if (!Array.isArray(reviews) || reviews.length === 0) {
            container.innerHTML = '<p style="color: #666; font-style: italic;">Ще немає відгуків. Залиште свій перший вражаючий коментар нижче! ✨</p>';
            return;
        }

        container.innerHTML = reviews.map(r => `
            <div class="review-card-item">
                <div class="review-header">
                    <strong>👤 ${escapeHtml(r.name)}</strong>
                    <span class="stars">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</span>
                </div>
                <p class="review-msg">${escapeHtml(r.message)}</p>
                <small class="review-date">🕒 ${new Date(r.createdAt).toLocaleDateString()}</small>
            </div>
        `).join('');
    } catch (error) {
        console.error(error);
        container.innerHTML = '<p style="color: red;">Не вдалося завантажити відгуки.</p>';
    }
}

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
                alert('Дякуємо! Ваш відгук успішно додано. 🎉');
            } else if (response.status === 429) {
                alert('Забагато запитів! Зачекайте хвилину.');
            } else {
                const errData = await response.json().catch(() => ({}));
                alert(errData.error || 'Помилка при відправці відгуку.');
            }
        } catch (error) {
            console.error(error);
            alert('Помилка мережі.');
        }
    });
}

function setupAdminPanel() {
    const form = document.getElementById('add-place-form');
    const adminKeyInput = document.getElementById('admin-key-input');

    if (adminKeyInput) {
        adminKeyInput.addEventListener('input', () => {
            loadPlaces();
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
            name: document.getElementById('place-name').value.trim(),
            category: document.getElementById('place-category').value,
            region: document.getElementById('place-region').value,
            description: document.getElementById('place-desc').value.trim(),
            difficulty: document.getElementById('place-difficulty').value,
            latitude: parseFloat(document.getElementById('place-lat').value),
            longitude: parseFloat(document.getElementById('place-lng').value),
            imageUrl: document.getElementById('place-image').value.trim() || null
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
                alert('Неправильний адмін-ключ!');
            } else {
                const errData = await response.json().catch(() => ({}));
                alert(errData.error || 'Помилка при додаванні місця.');
            }
        } catch (error) {
            console.error(error);
            alert('Помилка мережі.');
        }
    });
}

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