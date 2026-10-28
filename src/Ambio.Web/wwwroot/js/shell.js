// App shell behaviors, and those of the static filter forms. They work on every page without an interactive circuit,
// the static Identity pages included.
(() => {
    const closeSheets = () => {
        for (const sheet of document.querySelectorAll("dialog.sheet[open]")) {
            sheet.close();
        }
    };

    const hidePopovers = () => {
        for (const popover of document.querySelectorAll("[popover]:popover-open")) {
            popover.hidePopover();
        }
    };

    // Shows a toast from the browser, such as after a copy, built from the template of ToastHost so it looks like the
    // toasts of the server. It replaces the toast shown, and the next page removes it.
    const showToast = (message, icon) => {
        const toast = document.getElementById("toast-template")?.content.firstElementChild?.cloneNode(true);
        const host = document.querySelector(".toast-host");
        if (!toast || !host) {
            return;
        }

        toast.querySelector(".bi").classList.add(`bi-${icon}`);
        toast.querySelector(".toast-message").textContent = message;
        host.replaceChildren(toast);
    };

    // Copies the text of a field during the click itself: Safari refuses a copy that comes later, such as after a round
    // trip to the server. The Clipboard API needs a secure context (HTTPS or localhost); elsewhere, such as plain HTTP on
    // a local network, the text is selected and copied with execCommand.
    const copy = async (field) => {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(field.value);
            return;
        }

        field.select();
        if (!document.execCommand("copy")) {
            throw new Error("The browser refused the copy.");
        }
    };

    // Called by the Sheet component of an interactive page, such as after its form was saved.
    window.ambio.sheet = {
        close: (id) => document.getElementById(id)?.close(),
    };

    document.addEventListener("click", (event) => {
        const opener = event.target.closest("[data-sheet-open]");
        if (opener) {
            document.getElementById(opener.dataset.sheetOpen)?.showModal();
            return;
        }

        // A Copy button: data-copy names the field, data-copied and data-copy-failed the messages of its toast.
        const copier = event.target.closest("[data-copy]");
        if (copier) {
            copy(document.getElementById(copier.dataset.copy)).then(
                () => showToast(copier.dataset.copied, "clipboard-check"),
                () => showToast(copier.dataset.copyFailed, "exclamation-circle"));
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

        // A filter applies as soon as its select or its checkbox changes. requestSubmit fires the submit event, which
        // enhanced navigation intercepts; submit() would reload the whole page.
        if (event.target.matches("form[data-auto-submit] :is(select, input[type=checkbox])")) {
            event.target.form.requestSubmit();
        }
    });

    // A static form sent from its sheet closes it first: the Filters sheet that holds its form, or the sheet or popover
    // of the ⋯ menu that holds the button sending a hidden form, such as Archive. Enhanced navigation would otherwise
    // drop the open attribute of the dialog without closing it, and leave the page inert behind it.
    document.addEventListener("submit", (event) => {
        if (event.target.matches("form[data-enhance]")) {
            for (const sheet of event.target.querySelectorAll("dialog.sheet[open]")) {
                sheet.close();
            }
            event.submitter?.closest("dialog.sheet[open]")?.close();
            event.submitter?.closest("[popover]:popover-open")?.hidePopover();
        }
    });

    // A filter form leaves its empty fields out of the URL: "?status=applied" rather than "?q=&status=applied&…".
    document.addEventListener("formdata", (event) => {
        if (event.target.matches("form[data-filter-form]")) {
            for (const [name, value] of [...event.formData]) {
                if (value === "") {
                    event.formData.delete(name);
                }
            }
        }
    }, true);

    syncThemeSelectors();
    window.addEventListener("storage", syncThemeSelectors);

    // Enhanced navigation patches the DOM: it leaves an open dialog or popover open and unchecks the theme selectors.
    window.Blazor?.addEventListener("enhancedload", () => {
        closeSheets();
        hidePopovers();
        syncThemeSelectors();
    });
})();
