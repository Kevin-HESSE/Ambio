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

    // Enhanced navigation patches the DOM without closing an open dialog.
    window.Blazor?.addEventListener("enhancedload", closeSheets);
})();
