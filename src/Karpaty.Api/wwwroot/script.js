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

document.addEventListener("DOMContentLoaded", () => {
    checkHealth();
});