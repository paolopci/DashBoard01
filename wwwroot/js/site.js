// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {
    const pageSearchForm = document.getElementById("page-search-form");
    const pageSearchInput = document.getElementById("page-search-input");

    if (!pageSearchForm || !pageSearchInput) {
        return;
    }

    if (pageSearchForm.dataset.searchEnabled !== "true") {
        pageSearchInput.disabled = true;
        return;
    }

    let debounceTimer;
    let lastSubmittedValue = (pageSearchForm.dataset.currentSearch || "").trim();

    pageSearchForm.addEventListener("submit", function (event) {
        const currentValue = pageSearchInput.value.trim();

        if (currentValue.length > 0 && currentValue.length < 3) {
            event.preventDefault();
        }
    });

    pageSearchInput.addEventListener("input", function () {
        const currentValue = pageSearchInput.value.trim();

        window.clearTimeout(debounceTimer);

        debounceTimer = window.setTimeout(function () {
            if (currentValue.length >= 3) {
                if (currentValue !== lastSubmittedValue) {
                    lastSubmittedValue = currentValue;
                    pageSearchForm.submit();
                }

                return;
            }

            if (currentValue.length === 0 && lastSubmittedValue.length > 0) {
                lastSubmittedValue = "";
                pageSearchForm.submit();
            }
        }, 2000);
    });
});
