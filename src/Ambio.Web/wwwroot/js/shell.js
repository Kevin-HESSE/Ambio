// App shell behaviors. They work on every page without an interactive circuit, the static Identity
// pages included.
(() => {
    const closeSheets = () => {
        for (const sheet of document.querySelectorAll("dialog.sheet[open]")) {
            sheet.close();
        }
    };

    document.addEventListener("click", (event) => {
        const opener = event.target.closest("[data-sheet-open]");
        if (opener) {
            document.getElementById(opener.dataset.sheetOpen)?.showModal();
            return;
        }

        // A click on the dialog itself lands on its backdrop. A link leaves the page, so the sheet closes too.
        const sheet = event.target.closest("dialog.sheet");
        if (sheet && (event.target === sheet || event.target.closest("a[href], [data-sheet-close]"))) {
            sheet.close();
        }
    });

    // The theme selectors are rendered unchecked: the choice is only known here (window.ambio.theme, set in App.razor).
    const syncThemeSelectors = () => {
        const choice = window.ambio.theme.choice();
        for (const input of document.querySelectorAll("[data-theme-selector] input")) {
            input.checked = input.value === choice;
        }
    };

    document.addEventListener("change", (event) => {
        if (event.target.matches("[data-theme-selector] input")) {
            window.ambio.theme.set(event.target.value);
            syncThemeSelectors();
        }
    });

    syncThemeSelectors();
    window.addEventListener("storage", syncThemeSelectors);

    // Enhanced navigation patches the DOM: it leaves an open dialog open and unchecks the theme selectors.
    window.Blazor?.addEventListener("enhancedload", () => {
        closeSheets();
        syncThemeSelectors();
    });
})();
