const API_URL = "/api";

async function apiGet(endpoint) {
    const response = await fetch(`${API_URL}${endpoint}`);
    if (!response.ok) {
        throw new Error(`Помилка HTTP: ${response.status}`);
    }
    return await response.json();
}

async function checkHealth() {
    const statusEl = document.getElementById("health-status");
    try {
        const data = await apiGet("/health");
        statusEl.textContent = `API: ${data.status} | База даних: ${data.database}`;
        statusEl.className = "status-box status-ok";
    } catch (error) {
        statusEl.textContent = "Не вдалося підключитися до API (очікуємо бекенд)";
        statusEl.className = "status-box status-error";
    }
}

// Функція для F1: завантаження та рендеринг карток місць
async function fetchPlaces() {
    const container = document.getElementById("places-list");

    // 1. ⏳ UI-стан: «Завантаження…» (спінер)
    container.innerHTML = `
        <div class="ui-state loading-state" style="grid-column: 1 / -1; text-align: center; padding: 30px;">
            <div class="spinner" style="width: 35px; height: 35px; border: 4px solid #e3e3e3; border-top: 4px solid var(--primary-color); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 10px auto;"></div>
            <p>Завантаження цікавих місць…</p>
        </div>
    `;

    try {
        const places = await apiGet("/places");

        // 2. 📭 UI-стан: «Немає місць» (якщо прийшов порожній масив)
        if (!places || places.length === 0) {
            container.innerHTML = `
                <div class="ui-state empty-state" style="grid-column: 1 / -1; text-align: center; padding: 30px; color: #666;">
                    <i class="fa-solid fa-map-location-dot" style="font-size: 30px; color: #bdc3c7; margin-bottom: 10px;"></i>
                    <p>На жаль, наразі немає доступних місць у базі.</p>
                </div>
            `;
            return;
        }

        // Рендеринг реальних карток із MongoDB Atlas
        container.innerHTML = "";
        places.forEach(place => {
            const card = document.createElement("div");
            card.className = "place-card";

            // Безпечне зчитування полів (підтримує і маленькі, і великі літери з бекенду)
            const imageUrl = place.imageUrl || place.ImageUrl || "https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=600&q=80";
            const title = place.title || place.Title || "Карпатська локація";
            const category = place.category || place.Category || "Маршрут";
            const region = place.region || place.Region || "Карпати";
            const difficulty = place.difficulty || place.Difficulty || "Середня";
            const description = place.description || place.Description || "Опис відсутній.";

            card.innerHTML = `
                <div class="place-image-wrapper" style="position: relative; height: 160px; overflow: hidden; border-radius: 6px 6px 0 0;">
                    <img src="${imageUrl}" alt="${title}" style="width: 100%; height: 100%; object-fit: cover;">
                    <span class="place-badge" style="position: absolute; top: 10px; left: 10px; background: rgba(0,0,0,0.7); color: white; padding: 4px 10px; border-radius: 12px; font-size: 0.8rem;">
                        <i class="fa-solid fa-tag"></i> ${category}
                    </span>
                </div>
                <div class="place-info" style="padding: 15px; display: flex; flex-direction: column; gap: 8px; flex-grow: 1;">
                    <h3 style="color: var(--primary-color); font-size: 1.1rem;">${title}</h3>
                    <div style="display: flex; justify-content: space-between; font-size: 0.85rem; color: #666; background: #f8f9fa; padding: 6px 8px; border-radius: 4px;">
                        <span><i class="fa-solid fa-location-dot"></i> ${region}</span>
                        <span><i class="fa-solid fa-gauge"></i> ${difficulty}</span>
                    </div>
                    <p style="font-size: 0.9rem; color: #555;">${description}</p>
                </div>
            `;
            container.appendChild(card);
        });

    } catch (error) {
        console.error("Помилка завантаження місць:", error);

        // 3. ⚠️ UI-стан: «Помилка завантаження» + кнопка «Спробувати ще раз»
        container.innerHTML = `
            <div class="ui-state error-state" style="grid-column: 1 / -1; text-align: center; padding: 30px;">
                <i class="fa-solid fa-triangle-exclamation" style="font-size: 35px; color: #e74c3c; margin-bottom: 10px;"></i>
                <p style="color: #721c24; margin-bottom: 12px;">Не вдалося завантажити дані з сервера.</p>
                <button class="retry-btn" onclick="fetchPlaces()" style="background: var(--primary-color); color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;">
                    <i class="fa-solid fa-rotate-right"></i> Спробувати ще раз
                </button>
            </div>
        `;
    }
}

// Додаємо анімацію спінера в динаміку
const styleSheet = document.createElement("style");
styleSheet.innerHTML = `@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }`;
document.head.appendChild(styleSheet);

document.addEventListener("DOMContentLoaded", () => {
    checkHealth();
    fetchPlaces(); // Викликаємо завантаження карток для F1
});