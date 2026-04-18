// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {
    initPageSearch();
    initToasts();
    initRegisterForm();
});

function initPageSearch() {
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
}

function initToasts() {
    const toasts = document.querySelectorAll("[data-toast]");

    toasts.forEach(function (toast) {
        const closeToast = function () {
            toast.classList.add("opacity-0");
            window.setTimeout(function () {
                toast.remove();
            }, 200);
        };

        toast.addEventListener("click", closeToast);
        window.setTimeout(closeToast, 5000);
    });
}

function initRegisterForm() {
    const registerForm = document.querySelector("[data-register-form]");

    if (!registerForm) {
        return;
    }

    const requiredFields = registerForm.querySelectorAll("[data-register-required]");
    const submitButton = registerForm.querySelector("[data-register-submit]");

    if (!submitButton) {
        return;
    }

    const refreshSubmitState = function () {
        const allRequiredFieldsFilled = Array.from(requiredFields)
            .every(function (field) {
                return field.value.trim().length > 0;
            });

        submitButton.disabled = !allRequiredFieldsFilled;
    };

    requiredFields.forEach(function (field) {
        field.addEventListener("input", refreshSubmitState);
        field.addEventListener("change", refreshSubmitState);
    });

    refreshSubmitState();
}
