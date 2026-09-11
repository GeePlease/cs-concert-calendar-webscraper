// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    // 1. DOM-Elemente greifen
    const grid = document.getElementById("concert-grid");
    const filterLocation = document.getElementById("filter-venue");
    const filterGenre = document.getElementById("filter-genre");
    const filterDate = document.getElementById("filter-date");
    const filterPrice = document.getElementById("filter-price");

    let allConcerts = []; // Hält die Daten im Speicher

    // 2. Daten vom C# Backend holen
    fetchConcerts();

    async function fetchConcerts() {
        try {
            const response = await fetch("/api/concerts");
            if (!response.ok) throw new Error("API antwortet nicht");

            allConcerts = await response.json();
            renderConcerts(allConcerts);
        } catch (error) {
            console.error("Fehler:", error);
            grid.innerHTML = `<p style="color: var(--text-muted); grid-column: 1/-1;">Konzerte konnten nicht geladen werden.</p>`;
        }
    }

    // 3. JSON in HTML-Karten umwandeln & einfügen
    function renderConcerts(concerts) {
        if (!concerts || concerts.length === 0) {
            grid.innerHTML = `<p style="color: var(--text-muted); grid-column: 1/-1;">Keine Konzerte gefunden.</p>`;
            return;
        }

        grid.innerHTML = concerts.map(c => `
            <article class="concert-card">
                <div class="card-badge">${formatDate(c.date)}</div>
                <div class="card-content">
                    <span class="card-genre">${escapeHtml(c.genre || 'Sonstiges')}</span>
                    <h3 class="card-title">${escapeHtml(c.title || c.artist)}</h3>
                    <p class="card-location">📍 ${escapeHtml(c.venue)}</p>
                    <p class="card-price">🎟️ ${formatPrice(c.price)}</p>
                </div>
                <div class="card-footer">
                    <button class="btn-bookmark" type="button" data-id="${c.id}">♡ Merken</button>
                </div>
            </article>
        `).join("");
    }

    // 4. Filter-Events
    [filterLocation, filterGenre, filterDate, filterPrice].forEach(select => {
        select?.addEventListener("change", applyFilters);
    });

    function applyFilters() {
        let filtered = [...allConcerts];

        if (filterLocation.value !== "all") {
            filtered = filtered.filter(c => c.location?.toLowerCase().includes(filterLocation.value.toLowerCase()));
        }
        if (filterGenre.value !== "all") {
            filtered = filtered.filter(c => c.genre?.toLowerCase().includes(filterGenre.value.toLowerCase()));
        }

        renderConcerts(filtered);
    }

    // 5. Modal & Tab Steuerung & Auth-UI Elemente
    const modal = document.getElementById("auth-modal");
    const btnOpenLogin = document.getElementById("btn-open-login");
    const btnCloseModal = document.getElementById("btn-close-modal");
    const userMenu = document.getElementById("user-menu");
    const btnLogout = document.getElementById("btn-logout");
    const btnProfile = document.getElementById("btn-open-profile");

    const tabLogin = document.getElementById("tab-login");
    const tabRegister = document.getElementById("tab-register");
    const formLogin = document.getElementById("form-login");
    const formRegister = document.getElementById("form-register");
    const authMessage = document.getElementById("auth-message");

    // Steuerung der Header-Buttons (Login vs. User-Menü)
    function updateAuthUI() {
        const token = localStorage.getItem("token");
        const isLoggedIn = !!token;

        if (isLoggedIn) {
            btnOpenLogin?.classList.add("hidden");
            userMenu?.classList.remove("hidden");
        } else {
            btnOpenLogin?.classList.remove("hidden");
            userMenu?.classList.add("hidden");
        }
    }

    // Beim Laden der Seite direkt ausführen
    updateAuthUI();

    // Hilfsfunktionen für Feedback-Meldungen im Modal
    function showAuthMessage(text, type = "error") {
        if (!authMessage) return;
        authMessage.textContent = text;
        authMessage.className = `auth-message ${type}`;
    }

    function clearAuthMessage() {
        if (!authMessage) return;
        authMessage.textContent = "";
        authMessage.className = "auth-message hidden";
    }

    // Modal öffnen & schließen
    btnOpenLogin?.addEventListener("click", () => {
        clearAuthMessage();
        modal?.classList.remove("hidden");
    });

    btnCloseModal?.addEventListener("click", () => {
        clearAuthMessage();
        modal?.classList.add("hidden");
    });

    modal?.addEventListener("click", (e) => {
        if (e.target === modal) {
            clearAuthMessage();
            modal.classList.add("hidden");
        }
    });

    // Tab-Umschaltung
    tabLogin?.addEventListener("click", () => {
        clearAuthMessage();
        tabLogin.classList.add("active");
        tabRegister.classList.remove("active");
        formLogin.classList.remove("hidden");
        formRegister.classList.add("hidden");
    });

    tabRegister?.addEventListener("click", () => {
        clearAuthMessage();
        tabRegister.classList.add("active");
        tabLogin.classList.remove("active");
        formRegister.classList.remove("hidden");
        formLogin.classList.add("hidden");
    });

    // Logout durchführen
    btnLogout?.addEventListener("click", () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        updateAuthUI();
    });

    // 6. Authentifizierung: API Absenden

    // Registrierung absenden
    formRegister?.addEventListener("submit", async (e) => {
        e.preventDefault();
        clearAuthMessage();

        const password = document.getElementById("reg-pass").value;
        const passwordConfirm = document.getElementById("reg-pass-confirm").value;

        // Frontend-Check: Passwörter vergleichen
        if (password !== passwordConfirm) {
            showAuthMessage("Die eingegebenen Passwörter stimmen nicht überein!", "error");
            return;
        }

        const body = {
            username: document.getElementById("reg-user").value,
            email: document.getElementById("reg-email").value,
            password: password
        };

        try {
            const res = await fetch("/api/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });
            const data = await res.json();
            if (!res.ok) throw new Error(data.message || "Fehler bei Registrierung");

            showAuthMessage("Registrierung erfolgreich! Bitte melde dich an.", "success");
            setTimeout(() => {
                tabLogin.click();
            }, 1200);
        } catch (err) {
            showAuthMessage(err.message, "error");
        }
    });

    // Login absenden
    formLogin?.addEventListener("submit", async (e) => {
        e.preventDefault();
        clearAuthMessage();

        const body = {
            username: document.getElementById("login-user").value,
            password: document.getElementById("login-pass").value
        };

        try {
            const res = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });
            const data = await res.json();
            if (!res.ok) throw new Error(data.message || "Login fehlgeschlagen");

            // Token & User im Speichern ablegen
            if (data.token) {
                localStorage.setItem("token", data.token);
            }
            localStorage.setItem("user", JSON.stringify(data));

            // Header umschalten, Meldung löschen, Modal schließen
            updateAuthUI();
            clearAuthMessage();
            modal.classList.add("hidden");
        } catch (err) {
            showAuthMessage(err.message, "error");
        }
    });

    // Hilfsfunktionen
    function formatDate(dateStr) {
        if (!dateStr) return "TBA";
        const d = new Date(dateStr);
        return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString("de-AT", { day: "2-digit", month: "short" }).toUpperCase();
    }

    function formatPrice(price) {
        if (!price || price === 0) return "Gratis / Pay As You Wish";
        return typeof price === "number" ? `${price.toFixed(2).replace('.', ',')} €` : price;
    }

    function escapeHtml(str) {
        return (str || "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }
});