const API_URL = "/api";
const API_BASE_URL = API_URL; // Щоб уникнути помилок у старих функціях

document.addEventListener("DOMContentLoaded", () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilterButtons();
    setupModalEvents();
});


// 1. Сперевірка Health API (F0)
async function checkHealth() {
    const healthStatusEl = document.getElementById("health-status");
    if (!healthStatusEl) return;

    try {
        const response = await fetch(`${API_BASE_URL}/health`);
        if (!response.ok) throw new Error("Сервер недоступний");

        const data = await response.json();
        healthStatusEl.className = "status-box success";
        healthStatusEl.innerHTML = `🟢 Статус: ${data.status || 'OK'} | БД: ${data.database || 'Connected'}`;
    } catch (error) {
        healthStatusEl.className = "status-box error";
        healthStatusEl.innerHTML = `🔴 Помилка підключення до API: ${error.message}`;
    }
}


// 2. Завантаження місць та фільтрація (F1 + F2)
async function loadPlaces(category = "") {
    const placesListEl = document.getElementById("places-list");
    if (!placesListEl) return;

    // Стан завантаження (UI-state 1)
    placesListEl.innerHTML = `<p class="placeholder"><i class="fa-solid fa-spinner fa-spin"></i> Завантаження місць...</p>`;

    try {
        const url = category
            ? `${API_BASE_URL}/places?category=${encodeURIComponent(category)}`
            : `${API_BASE_URL}/places`;

        const response = await fetch(url);
        if (!response.ok) throw new Error("Не вдалося завантажити список місць");

        const places = await response.json();

        // Порожній масив (UI-state 2)
        if (!places || places.length === 0) {
            placesListEl.innerHTML = `<p class="placeholder">Місць у цій категорії поки немає.</p>`;
            return;
        }

        // Відображення карток
        placesListEl.innerHTML = places.map(place => {
            const id = place.id || place._id;
            return `
                <div class="card" onclick="openPlaceDetails('${id}')">
                    <img src="${place.imageUrl || 'https://via.placeholder.com/300x200?text=Karpaty'}" alt="${place.name}">
                    <div class="card-content">
                        <h3>${place.name}</h3>
                        <p class="category-badge"><i class="fa-solid fa-tag"></i> ${place.category || 'Загальне'}</p>
                        <p>${place.description ? place.description.substring(0, 90) + '...' : 'Опис відсутній'}</p>
                        <button class="btn-details">Детальніше</button>
                    </div>
                </div>
            `;
        }).join("");

    } catch (error) {
        // Стан помилки з кнопкою повтору (UI-state 3)
        placesListEl.innerHTML = `
            <div class="error-box">
                <p>⚠️ Помилка: ${error.message}</p>
                <button onclick="loadPlaces('${category}')" class="retry-btn">
                    <i class="fa-solid fa-rotate-right"></i> Спробувати ще раз
                </button>
            </div>
        `;
    }
}

// 3. Завантаження порад для походу (F3) 🟢
async function loadTips() {
    const tipsListEl = document.getElementById("tips-list");
    if (!tipsListEl) return;

    tipsListEl.innerHTML = `<p class="placeholder"><i class="fa-solid fa-spinner fa-spin"></i> Завантаження порад...</p>`;

    try {
        const response = await fetch(`${API_BASE_URL}/tips`);
        if (!response.ok) throw new Error("Не вдалося завантажити поради");

        const tips = await response.json();

        if (!tips || tips.length === 0) {
            tipsListEl.innerHTML = `<p class="placeholder">Пораду поки не додано.</p>`;
            return;
        }

        tipsListEl.innerHTML = tips.map(tip => `
            <div class="tip-card">
                <span class="tip-category"><i class="fa-solid fa-lightbulb"></i> ${tip.category || 'Загальне'}</span>
                <h3>${tip.title}</h3>
                <p>${tip.text}</p>
            </div>
        `).join("");



    } catch (error) {
        tipsListEl.innerHTML = `
            <div class="error-box">
                <p>⚠️ Помилка: ${error.message}</p>
                <button onclick="loadTips()" class="retry-btn">
                    <i class="fa-solid fa-rotate-right"></i> Спробувати ще раз
                </button>
            </div>
        `;
    }
}

// 4. Обробка кнопок фільтрів (F2)
function setupFilterButtons() {
    const buttons = document.querySelectorAll(".filter-btn");
    buttons.forEach(btn => {
        btn.addEventListener("click", (e) => {
            buttons.forEach(b => b.classList.remove("active"));
            e.target.classList.add("active");

            const selectedCategory = e.target.getAttribute("data-category");
            loadPlaces(selectedCategory);
        });
    });
}

// 5. Модальне вікно деталей місця (F2)
async function openPlaceDetails(id) {
    const modal = document.getElementById("placeModal");
    const modalDetails = document.getElementById("modalDetails");

    if (!modal || !modalDetails) return;

    modalDetails.innerHTML = `<p><i class="fa-solid fa-spinner fa-spin"></i> Завантаження деталей...</p>`;
    modal.style.display = "flex";

    try {
        const response = await fetch(`${API_BASE_URL}/places/${id}`);
        if (!response.ok) throw new Error("Не вдалося отримати деталі про місце");

        const place = await response.json();

        modalDetails.innerHTML = `
            <h2>${place.name}</h2>
            <p><strong>Категорія:</strong> ${place.category || 'Не вказано'}</p>
            <p><strong>Регіон / Локація:</strong> ${place.region || 'Карпати'}</p>
            <p><strong>Опис:</strong> ${place.description || 'Повний опис відсутній.'}</p>
            ${place.latitude && place.longitude ? `
                <a class="map-link" href="https://www.google.com/maps/search/?api=1&query=${place.latitude},${place.longitude}" target="_blank">
                    <i class="fa-solid fa-map-location-dot"></i> Відкрити на Google Карті
                </a>
            ` : ''}
        `;
    } catch (error) {
        modalDetails.innerHTML = `<p class="error-text">⚠️ Помилка завантаження: ${error.message}</p>`;
    }
}

// Події закриття модального вікна
function setupModalEvents() {
    const modal = document.getElementById("placeModal");
    const closeBtn = document.querySelector(".close-btn");

    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            modal.style.display = "none";
        });
    }

    window.addEventListener("click", (event) => {
        if (event.target === modal) {
            modal.style.display = "none";
        }
    });
}
