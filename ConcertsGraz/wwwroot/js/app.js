// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    // 1. DOM-Elemente greifen
    const grid = document.getElementById("concert-grid");
        const filterLocation = document.getElementById("filter-venue");
    const filterGenre = document.getElementById("filter-genre");
    const filterDate = document.getElementById("filter-date");
    const filterPrice = document.getElementById("filter-price");
    const filterBar = document.querySelector(".filter-bar");

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
    const navConcerts = document.getElementById("nav-concerts");

    const tabLogin = document.getElementById("tab-login");
    const tabRegister = document.getElementById("tab-register");
    const formLogin = document.getElementById("form-login");
    const formRegister = document.getElementById("form-register");
    const authMessage = document.getElementById("auth-message");
    const profileEditArea = document.getElementById("profile-edit-area");
    const formChangeUsername = document.getElementById("form-change-username");
    const formChangeEmail = document.getElementById("form-change-email");
    const formChangePassword = document.getElementById("form-change-password");
    const formDeleteAccount = document.getElementById("form-delete-account");
    const btnOpenDeleteAccount = document.getElementById("btn-open-delete-account");
    const newUsername = document.getElementById("new-username");
    const newEmail = document.getElementById("new-email");
    const confirmEmailPass = document.getElementById("confirm-email-pass");
    const currentPass = document.getElementById("current-pass");
    const newPass = document.getElementById("new-pass");
    const newPassConfirm = document.getElementById("new-pass-confirm");
    const deleteConfirmPass = document.getElementById("delete-confirm-pass");
    const profileMessage = document.getElementById("profile-message");

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
    function logout() {
        localStorage.removeItem("token");
        localStorage.removeItem("user");

        const concertSection = document.getElementById("concert-section");
        const profileSection = document.getElementById("profile-section");

        concertSection?.classList.remove("hidden");
        profileSection?.classList.add("hidden");
        filterBar?.classList.remove("hidden");

        // Filter zurücksetzen
        filterLocation.value = "all";
        filterGenre.value = "all";
        filterDate.value = "all";
        filterPrice.value = "all";

        renderConcerts(allConcerts);

        updateAuthUI();
    }

    btnLogout?.addEventListener("click", logout);

    btnProfile?.addEventListener("click", () => {
        const concertSection = document.getElementById("concert-section");
        const profileSection = document.getElementById("profile-section");

        concertSection?.classList.add("hidden");
        profileSection?.classList.remove("hidden");
        filterBar?.classList.add("hidden");
        loadProfileData();
    });

    navConcerts?.addEventListener("click", () => {
        const concertSection = document.getElementById("concert-section");
        const profileSection = document.getElementById("profile-section");

        concertSection?.classList.remove("hidden");
        profileSection?.classList.add("hidden");
        filterBar?.classList.remove("hidden");
    });

    async function loadProfileData() {
        const profileCurrentUser = document.getElementById("profile-current-user");
        const displayUsername = document.getElementById("display-username");
        const displayEmail = document.getElementById("display-email");
        const profileMessage = document.getElementById("profile-message");

        if (profileMessage) {
            profileMessage.textContent = "";
            profileMessage.className = "auth-message hidden";
        }

        try {
            const response = await fetch("/api/users/profile");

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Profil konnte nicht geladen werden.");
            }

            const profile = await response.json();

            if (profileCurrentUser) profileCurrentUser.textContent = profile.username;
            if (displayUsername) displayUsername.textContent = profile.username;
            if (displayEmail) displayEmail.textContent = profile.email;
        } catch (error) {
            console.error("Fehler beim Laden des Profils:", error);

            if (profileMessage) {
                profileMessage.textContent = error.message || "Profil konnte nicht geladen werden.";
                profileMessage.className = "auth-message error";
            }
        }
    }

    function showProfileMessage(text, type = "error") {
        if (!profileMessage) return;
        profileMessage.textContent = text;
        profileMessage.className = `auth-message ${type}`;
    }

    function clearProfileMessage() {
        if (!profileMessage) return;
        profileMessage.textContent = "";
        profileMessage.className = "auth-message hidden";
    }

    function hideProfileEditForms() {
        formChangeUsername?.reset();
        formChangeEmail?.reset();
        formChangePassword?.reset();
        formDeleteAccount?.reset();
        formChangeUsername?.classList.add("hidden");
        formChangeEmail?.classList.add("hidden");
        formChangePassword?.classList.add("hidden");
        formDeleteAccount?.classList.add("hidden");
        profileEditArea?.classList.add("hidden");
    }

    document.querySelectorAll('.btn-edit[data-target="username"], .btn-edit[data-target="email"]').forEach(button => {
        button.addEventListener("click", () => {
            const target = button.dataset.target;
            const form = target === "username" ? formChangeUsername : formChangeEmail;
            const input = target === "username" ? newUsername : newEmail;
            const display = document.getElementById(target === "username" ? "display-username" : "display-email");

            hideProfileEditForms();
            clearProfileMessage();
            form?.classList.remove("hidden");
            profileEditArea?.classList.remove("hidden");

            if (input) {
                input.value = display?.textContent.trim() || "";
                input.focus();
            }
        });
    });

    document.querySelector('.btn-edit[data-target="password"]')?.addEventListener("click", () => {
        hideProfileEditForms();
        clearProfileMessage();
        formChangePassword?.classList.remove("hidden");
        profileEditArea?.classList.remove("hidden");
        currentPass?.focus();
    });

    btnOpenDeleteAccount?.addEventListener("click", () => {
        hideProfileEditForms();
        clearProfileMessage();
        formDeleteAccount?.classList.remove("hidden");
        profileEditArea?.classList.remove("hidden");
        deleteConfirmPass?.focus();
    });

    [formChangeUsername, formChangeEmail, formChangePassword, formDeleteAccount].forEach(form => {
        form?.querySelector(".btn-cancel")?.addEventListener("click", () => {
            hideProfileEditForms();
            clearProfileMessage();
        });
    });

    formChangeUsername?.addEventListener("submit", async (e) => {
        e.preventDefault();
        await updateProfile({ username: newUsername.value.trim() }, "Benutzername erfolgreich aktualisiert.");
    });

    formChangeEmail?.addEventListener("submit", async (e) => {
        e.preventDefault();
        await updateProfile({
            email: newEmail.value.trim(),
            currentPassword: confirmEmailPass.value
        }, "E-Mail-Adresse erfolgreich aktualisiert.");
    });

    formChangePassword?.addEventListener("submit", async (e) => {
        e.preventDefault();

        if (!currentPass.value || !newPass.value || !newPassConfirm.value) {
            showProfileMessage("Bitte fülle alle Passwortfelder aus.", "error");
            return;
        }

        if (newPass.value !== newPassConfirm.value) {
            showProfileMessage("Die neuen Passwörter stimmen nicht überein.", "error");
            return;
        }

        try {
            const response = await fetch("/api/users/updatePW", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    currentPassword: currentPass.value,
                    newPassword: newPass.value
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Passwort konnte nicht aktualisiert werden.");
            }

            hideProfileEditForms();
            showProfileMessage("Passwort erfolgreich aktualisiert.", "success");
        } catch (error) {
            console.error("Fehler beim Aktualisieren des Passworts:", error);
            showProfileMessage(error.message || "Passwort konnte nicht aktualisiert werden.", "error");
        }
    });

    formDeleteAccount?.addEventListener("submit", async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/users/delete", {
                method: "DELETE"
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Konto konnte nicht gelöscht werden.");
            }

            logout();
        } catch (error) {
            console.error("Fehler beim Löschen des Kontos:", error);
            showProfileMessage(error.message || "Konto konnte nicht gelöscht werden.", "error");
        }
    });

    async function updateProfile(body, successMessage) {
        try {
            const response = await fetch("/api/users/update", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Profil konnte nicht aktualisiert werden.");
            }

            hideProfileEditForms();
            await loadProfileData();
            showProfileMessage(successMessage, "success");
        } catch (error) {
            console.error("Fehler beim Aktualisieren des Profils:", error);
            showProfileMessage(error.message || "Profil konnte nicht aktualisiert werden.", "error");
        }
    }
    
    

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
});
