
const API_URL = "/api";

document.addEventListener("DOMContentLoaded", () => {
    checkHealth();
    loadPlaces();
    loadTips();
    setupFilters();
    setupModal();
    setupReviewForm();
    setupAdminPanel();
    loadAllReviews();
});

// Перевірка стану API
async function checkHealth() {
    const statusBox = document.getElementById("health-status");
    if (!statusBox) return;

    try {
        const response = await fetch(`${API_URL}/health`);

        if (response.ok) {
            statusBox.textContent = "API працює стабільно ✅";
            statusBox.style.backgroundColor = "#d4edda";
            statusBox.style.color = "#155724";
        } else {
            statusBox.textContent = "Помилка сервера ❌";
        }
    } catch (error) {
        console.error(error);
        statusBox.textContent = "Сервер не відповідає ❌";
        statusBox.style.backgroundColor = "#f8d7da";
        statusBox.style.color = "#721c24";
    }
}

// Безпечне відображення тексту
function escapeHtml(value) {
    return String(value ?? "").replace(/[&<>"']/g, char => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[char]);
}

// Завантаження туристичних місць
async function loadPlaces(category = "") {
    const placesList = document.getElementById("places-container");
    if (!placesList) return;

    placesList.innerHTML = "<p>Завантаження місць...</p>";

    try {
        let url = `${API_URL}/places`;

        if (category) {
            url += `?category=${encodeURIComponent(category)}`;
        }

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error("Не вдалося завантажити місця");
        }

        const places = await response.json();

        if (!Array.isArray(places) || places.length === 0) {
            placesList.innerHTML = "<p>Місць поки немає.</p>";
            return;
        }

        const adminKey = document
            .getElementById("admin-key-input")?.value.trim() || "";

        placesList.innerHTML = places.map(place => {
            const id = escapeHtml(place.id);
            const name = escapeHtml(place.name);
            const description = escapeHtml(place.description);
            const categoryName = escapeHtml(place.category);
            const region = escapeHtml(place.region);
            const difficulty = escapeHtml(place.difficulty);

            const image = place.imageUrl
                ? `<img src="${escapeHtml(place.imageUrl)}"
                        alt="${name}"
                        class="place-image">`
                : "";

            const deleteButton = adminKey
                ? `<button type="button"
                           onclick="deletePlace('${id}')">
                       Видалити
                   </button>`
                : "";

            return `
                <article class="place-card">
                    ${image}
                    <h3>${name}</h3>
                    <p>${description}</p>
                    <p>Категорія: ${categoryName}</p>
                    <p>Регіон: ${region}</p>
                    <p>Складність: ${difficulty}</p>

                    <button type="button"
                            onclick="openPlaceDetails('${id}')">
                        Детальніше
                    </button>

                    ${deleteButton}
                </article>
            `;
        }).join("");

    } catch (error) {
        console.error(error);
        placesList.innerHTML =
            "<p>Помилка завантаження туристичних місць.</p>";
    }
}

// Фільтрація місць
function setupFilters() {
    const buttons = document.querySelectorAll(".filter-btn");

    buttons.forEach(button => {
        button.addEventListener("click", () => {
            buttons.forEach(item => item.classList.remove("active"));
            button.classList.add("active");

            const category = button.dataset.category || "";
            loadPlaces(category);
        });
    });
}

// Завантаження порад
async function loadTips() {
    const tipsContainer = document.getElementById("tips-container");
    if (!tipsContainer) return;

    try {
        const response = await fetch(`${API_URL}/tips`);

        if (!response.ok) {
            throw new Error("Не вдалося завантажити поради");
        }

        const tips = await response.json();

        if (!Array.isArray(tips) || tips.length === 0) {
            tipsContainer.innerHTML = "<p>Порад поки немає.</p>";
            return;
        }

        tipsContainer.innerHTML = tips.map(tip => `
            <article class="tip-card">
                <h3>${escapeHtml(tip.title)}</h3>
                <p>${escapeHtml(tip.content ?? tip.description ?? "")}</p>
            </article>
        `).join("");

    } catch (error) {
        console.error(error);
        tipsContainer.innerHTML =
            "<p>Не вдалося завантажити поради.</p>";
    }
}

// Налаштування модального вікна
function setupModal() {
    const modal = document.getElementById("place-modal");
    const closeButton = document.getElementById("close-modal");

    if (!modal || !closeButton) return;

    closeButton.addEventListener("click", () => {
        modal.style.display = "none";
    });

    modal.addEventListener("click", event => {
        if (event.target === modal) {
            modal.style.display = "none";
        }
    });

    window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'block') {
            modal.style.display = 'none';
        }
    });
}

// Детальна інформація про місце
async function openPlaceDetails(id) {
    const modal = document.getElementById("place-modal");
    const details = document.getElementById("modalDetails");

    if (!modal || !details) return;

    details.innerHTML = "<p>Завантаження...</p>";
    modal.style.display = "block";

    try {
        const response = await fetch(
            `${API_URL}/places/${encodeURIComponent(id)}`
        );

        if (!response.ok) {
            throw new Error("Місце не знайдено");
        }

        const place = await response.json();

        window.currentPlaceName = place.name;

        const mapLink = place.latitude != null && place.longitude != null
            ? `https://www.google.com/maps?q=${encodeURIComponent(place.latitude)},${encodeURIComponent(place.longitude)}`
            : "";

        details.innerHTML = `
            <h2>${escapeHtml(place.name)}</h2>
            <p>${escapeHtml(place.description)}</p>
            <p>Категорія: ${escapeHtml(place.category)}</p>
            <p>Регіон: ${escapeHtml(place.region)}</p>
            <p>Складність: ${escapeHtml(place.difficulty)}</p>
            ${mapLink
                ? `<a href="${mapLink}" target="_blank" rel="noopener noreferrer">
                       Відкрити на карті
                   </a>`
                : ""}
        `;

    } catch (error) {
        console.error(error);
        details.innerHTML = "<p>Не вдалося завантажити інформацію.</p>";
    }
}

// Завантаження всіх відгуків
async function loadAllReviews() {
    const reviewsContainer =
        document.getElementById("reviews-container");

    if (!reviewsContainer) return;

    try {
        const response = await fetch(`${API_URL}/feedback`);

        if (!response.ok) {
            throw new Error("Не вдалося завантажити відгуки");
        }

        const reviews = await response.json();

        if (!Array.isArray(reviews) || reviews.length === 0) {
            reviewsContainer.innerHTML = "<p>Відгуків поки немає.</p>";
            return;
        }

        reviewsContainer.innerHTML = reviews.map(review => {
            const rating = Number(review.rating) || 0;
            const stars = "★".repeat(Math.max(0, Math.min(5, rating)));

            return `
                <article class="review-card">
                    <h3>${escapeHtml(review.name ?? "Анонім")}</h3>
                    <p>${stars}</p>
                    <p>${escapeHtml(review.message ?? "")}</p>
                    <small>
                        ${escapeHtml(review.createdAt ?? "")}
                    </small>
                </article>
            `;
        }).join("");

    } catch (error) {
        console.error(error);
        reviewsContainer.innerHTML =
            "<p>Не вдалося завантажити відгуки.</p>";
    }
}

// Надсилання відгуку
function setupReviewForm() {
    const form = document.getElementById("review-form");
    if (!form) return;

    form.addEventListener("submit", async event => {
        event.preventDefault();

        const name = document.getElementById("reviewer-name").value.trim();
        const email = document.getElementById("reviewer-email").value.trim();
        const rating = Number(
            document.getElementById("review-rating").value
        );
        const userMessage =
            document.getElementById("review-text").value.trim();

        const placeName = window.currentPlaceName || "Не вказано";

        const message = `📍 [Місце: ${placeName}] — ${userMessage}`;

        try {
            const response = await fetch(`${API_URL}/feedback`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name,
                    email,
                    rating,
                    message
                })
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));

                if (response.status === 429) {
                    alert("Забагато запитів. Спробуйте пізніше.");
                    return;
                }

                throw new Error(error.error || "Не вдалося надіслати відгук");
            }

            alert("Відгук успішно надіслано!");

            form.reset();

            const modal = document.getElementById("place-modal");
            if (modal) modal.style.display = "none";

            loadAllReviews();

        } catch (error) {
            console.error(error);
            alert(error.message || "Помилка під час надсилання відгуку.");
        }
    });
}

// Адмін-панель: додавання та видалення місць
function setupAdminPanel() {
    const form = document.getElementById("add-place-form");
    const adminKeyInput = document.getElementById("admin-key-input");
            loadPlaces(
                document.querySelector(".filter-btn.active")?.dataset.category || ""
    if (!form) return;

        event.preventDefault();
        const adminKey = adminKeyInput?.value.trim() || "";

        if (!adminKey) {
            alert("Введіть адміністративний ключ!");
        }

        const newPlace = {
            name: document.getElementById("place-name").value.trim(),
            category: document.getElementById("place-category").value,
            region: document.getElementById("place-region").value,
            description: document.getElementById("place-desc").value.trim(),
            difficulty: document.getElementById("place-difficulty").value,
            latitude: Number(
                document.getElementById("place-latitude").value
            ),
            longitude: Number(
                document.getElementById("place-longitude").value
            ),
            imageUrl: document.getElementById("place-image").value.trim() || null
        };

        try {
            const response = await fetch(`${API_URL}/places`, {
                headers: {
                    "Content-Type": "application/json",
                    "X-Admin-Key": adminKey
                },
                body: JSON.stringify(newPlace)
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.error || "Не вдалося додати місце");
            }
                method: "POST",
            return;

            alert("Місце успішно додано!");

    form.addEventListener("submit", async event => {


    }
            form.reset();

        });
            const activeCategory =
            );

                document.querySelector(".filter-btn.active")?.dataset.category || "";
        adminKeyInput.addEventListener("input", () => {


            loadPlaces(activeCategory);


        } catch (error) {
            console.error(error);

            alert(error.message || "Помилка під час додавання місця.");

        }

    });

}


// Видалення місця
window.deletePlace = async function(id) {
    const adminKey =

        document.getElementById("admin-key-input")?.value.trim() || "";


    if (!adminKey) {
        alert("Введіть адміністративний ключ!");
        return;
    }

    if (!confirm("Ви впевнені, що хочете видалити це місце?")) {
        return;

    }

    try {
        const response = await fetch(
            `${API_URL}/places/${encodeURIComponent(id)}`,

            {
                method: "DELETE",
                headers: {
                    "X-Admin-Key": adminKey
                }
            }
        );

        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.error || "Не вдалося видалити місце");
        }

        alert("Місце успішно видалено!");

        const activeCategory =
            document.querySelector(".filter-btn.active")?.dataset.category || "";

        loadPlaces(activeCategory);

    } catch (error) {
        console.error(error);
        alert(error.message || "Помилка під час видалення місця.");
    }
};

