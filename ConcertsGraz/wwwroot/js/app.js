// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    // 1. DOM-Elemente greifen
    const grid = document.getElementById("concert-grid");
    const filterLocation = document.getElementById("filter-location");
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
                    <p class="card-location">📍 ${escapeHtml(c.location)}</p>
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
