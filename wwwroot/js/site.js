// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {
    const customerSearchForm = document.getElementById("customer-search-form");
    const customerSearchInput = document.getElementById("customer-search-input");

    if (!customerSearchForm || !customerSearchInput) {
        return;
    }

    if (customerSearchForm.dataset.searchEnabled !== "true") {
        customerSearchInput.disabled = true;
        return;
    }

    let debounceTimer;
    let lastSubmittedValue = (customerSearchForm.dataset.currentSearch || "").trim();

    customerSearchForm.addEventListener("submit", function (event) {
        const currentValue = customerSearchInput.value.trim();

        if (currentValue.length > 0 && currentValue.length < 3) {
            event.preventDefault();
        }
    });

    customerSearchInput.addEventListener("input", function () {
        const currentValue = customerSearchInput.value.trim();

        window.clearTimeout(debounceTimer);

        debounceTimer = window.setTimeout(function () {
            if (currentValue.length >= 3) {
                if (currentValue !== lastSubmittedValue) {
                    lastSubmittedValue = currentValue;
                    customerSearchForm.submit();
                }

                return;
            }

            if (currentValue.length === 0 && lastSubmittedValue.length > 0) {
                lastSubmittedValue = "";
                customerSearchForm.submit();
            }
        }, 2000);
    });
});
