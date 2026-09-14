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
    const btnOpenLogin = document.querySelector(".btn-login") || document.getElementById("btn-open-login");
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

            // 1. Text lesen und sicher prüfen, ob Inhalt da ist
            const text = await res.text();
            const data = text ? JSON.parse(text) : {};

            // 2. HTTP-Status prüfen
            if (!res.ok) {
                throw new Error(data.message || `Server-Fehler ${res.status}: ${res.statusText}`);
            }

            // 3. Token & User im Speicher ablegen
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

    // ============================================================
    // Profil-Logik
    // ============================================================

    const profileModal       = document.getElementById('profile-modal');
    const btnOpenProfile     = document.getElementById('btn-open-profile');
    const btnCloseProfile    = document.getElementById('btn-close-profile');
    const profileMessage     = document.getElementById('profile-message');
    const profileCurrentUser = document.getElementById('profile-current-user');

    // User-Daten aus localStorage lesen (so wie dein Login es speichert)
    function getLoggedInUser() {
        const stored = localStorage.getItem("user");
        return stored ? JSON.parse(stored) : null;
    }

    // Auth-Token für API-Aufrufe
    function getAuthToken() {
        return localStorage.getItem("token") || "";
    }

    // Tabs
    const tabProfileUser   = document.getElementById('tab-profile-user');
    const tabProfileEmail  = document.getElementById('tab-profile-email');
    const tabProfilePass   = document.getElementById('tab-profile-pass');
    const tabProfileDelete = document.getElementById('tab-profile-delete');

    // Formulare
    const formChangeUsername = document.getElementById('form-change-username');
    const formChangeEmail    = document.getElementById('form-change-email');
    const formChangePassword = document.getElementById('form-change-password');
    const formDeleteAccount  = document.getElementById('form-delete-account');

    const profileTabs  = [tabProfileUser, tabProfileEmail, tabProfilePass, tabProfileDelete];
    const profileForms = [formChangeUsername, formChangeEmail, formChangePassword, formDeleteAccount];

    // --- Tab wechseln ---
    function switchProfileTab(index) {
        profileTabs.forEach((tab, i) => tab.classList.toggle('active', i === index));
        profileForms.forEach((form, i) => form.classList.toggle('hidden', i !== index));
        profileMessage.classList.add('hidden');
        profileMessage.textContent = '';
    }

    tabProfileUser.addEventListener('click',   () => switchProfileTab(0));
    tabProfileEmail.addEventListener('click',  () => switchProfileTab(1));
    tabProfilePass.addEventListener('click',   () => switchProfileTab(2));
    tabProfileDelete.addEventListener('click', () => switchProfileTab(3));

    // --- Modal öffnen / schließen ---
    btnOpenProfile?.addEventListener('click', () => {
        const user = getLoggedInUser();
        profileCurrentUser.textContent = user?.username || user?.email || 'Unbekannt';
        profileModal.classList.remove('hidden');
    });

    btnCloseProfile?.addEventListener('click', () => {
        profileModal.classList.add('hidden');
        profileForms.forEach(f => f.reset());
        switchProfileTab(0);
    });

    // Klick außerhalb der Card schließt das Modal
    profileModal?.addEventListener('click', (e) => {
        if (e.target === profileModal) btnCloseProfile.click();
    });

    // --- Feedback anzeigen ---
    function showProfileMessage(text, isSuccess) {
        profileMessage.textContent = text;
        profileMessage.className = 'auth-message ' + (isSuccess ? 'success' : 'error');
        profileMessage.classList.remove('hidden');
    }

    // --- 1: Benutzername ändern ---
    formChangeUsername?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            newUsername: document.getElementById('new-username').value,
            currentPassword: document.getElementById('confirm-user-pass').value
        };
        try {
            const res = await fetch('/api/user/username', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getAuthToken()
                },
                body: JSON.stringify(payload)
            });
            if (res.ok) {
                // localStorage aktualisieren
                const user = getLoggedInUser();
                if (user) {
                    user.username = payload.newUsername;
                    localStorage.setItem("user", JSON.stringify(user));
                }
                showProfileMessage('Benutzername erfolgreich geändert.', true);
                formChangeUsername.reset();
                profileCurrentUser.textContent = payload.newUsername;
            } else {
                const err = await res.text();
                showProfileMessage('Fehler: ' + err, false);
            }
        } catch (err) {
            showProfileMessage('Netzwerkfehler: ' + err.message, false);
        }
    });

    // --- 2: E-Mail ändern ---
    formChangeEmail?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            newEmail: document.getElementById('new-email').value,
            currentPassword: document.getElementById('confirm-email-pass').value
        };
        try {
            const res = await fetch('/api/user/email', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getAuthToken()
                },
                body: JSON.stringify(payload)
            });
            if (res.ok) {
                const user = getLoggedInUser();
                if (user) {
                    user.email = payload.newEmail;
                    localStorage.setItem("user", JSON.stringify(user));
                }
                showProfileMessage('E-Mail erfolgreich geändert.', true);
                formChangeEmail.reset();
            } else {
                const err = await res.text();
                showProfileMessage('Fehler: ' + err, false);
            }
        } catch (err) {
            showProfileMessage('Netzwerkfehler: ' + err.message, false);
        }
    });

    // --- 3: Passwort ändern ---
    formChangePassword?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const newPass = document.getElementById('new-pass').value;
        const newPassConfirm = document.getElementById('new-pass-confirm').value;

        if (newPass !== newPassConfirm) {
            showProfileMessage('Die neuen Passwörter stimmen nicht überein.', false);
            return;
        }

        const payload = {
            currentPassword: document.getElementById('current-pass').value,
            newPassword: newPass
        };
        try {
            const res = await fetch('/api/user/password', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getAuthToken()
                },
                body: JSON.stringify(payload)
            });
            if (res.ok) {
                showProfileMessage('Passwort erfolgreich geändert.', true);
                formChangePassword.reset();
            } else {
                const err = await res.text();
                showProfileMessage('Fehler: ' + err, false);
            }
        } catch (err) {
            showProfileMessage('Netzwerkfehler: ' + err.message, false);
        }
    });

    // --- 4: Konto löschen ---
    formDeleteAccount?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const reallyDelete = confirm('Möchtest du dein Konto wirklich endgültig löschen? Diese Aktion kann NICHT rückgängig gemacht werden.');
        if (!reallyDelete) return;

        const payload = {
            currentPassword: document.getElementById('delete-confirm-pass').value
        };
        try {
            const res = await fetch('/api/user', {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getAuthToken()
                },
                body: JSON.stringify(payload)
            });
            if (res.ok) {
                showProfileMessage('Konto erfolgreich gelöscht. Du wirst abgemeldet.', true);
                setTimeout(() => {
                    // Gleiche Keys wie dein Logout-Button
                    localStorage.removeItem("token");
                    localStorage.removeItem("user");
                    profileModal.classList.add('hidden');
                    updateAuthUI();
                }, 2000);
            } else {
                const err = await res.text();
                showProfileMessage('Fehler: ' + err, false);
            }
        } catch (err) {
            showProfileMessage('Netzwerkfehler: ' + err.message, false);
        }
    });
    
});