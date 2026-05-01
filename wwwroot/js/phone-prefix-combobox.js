(function (window) {
    const defaultOptionClass = 'flex w-full items-center gap-2 px-3 py-2 text-left text-sm text-slate-700 hover:bg-indigo-50 focus:bg-indigo-50 focus:outline-none';

    const fetchJson = async (url) => {
        const response = await fetch(url, { headers: { Accept: 'application/json' } });
        return response.ok ? await response.json() : [];
    };

    const clearChildren = (element) => {
        while (element.firstChild) {
            element.removeChild(element.firstChild);
        }
    };

    const createFlag = (prefix) => {
        const flag = document.createElement('img');
        flag.alt = '';
        flag.className = 'h-4 w-6 shrink-0 rounded-sm object-cover ring-1 ring-slate-200';
        flag.src = prefix.flagPath || '';
        return flag;
    };

    const createText = (className, value) => {
        const span = document.createElement('span');
        span.className = className;
        span.textContent = value || '';
        return span;
    };

    const init = (options) => {
        const root = options.root;
        const endpoint = options.endpoint;
        const onChange = typeof options.onChange === 'function' ? options.onChange : () => {};
        const emptyText = options.emptyText || 'Nessun prefisso trovato';
        const defaultDialCode = options.defaultDialCode || '+39';
        const defaultIso2 = options.defaultIso2 || 'IT';
        const optionClass = options.optionClass || defaultOptionClass;

        const combobox = root?.querySelector('[data-phone-prefix-combobox]');
        const value = root?.querySelector('[data-phone-prefix-value]');
        const countryIso2 = root?.querySelector('[data-phone-country-iso2]');
        const toggle = root?.querySelector('[data-phone-prefix-toggle]');
        const panel = root?.querySelector('[data-phone-prefix-panel]');
        const search = root?.querySelector('[data-phone-prefix-search]');
        const optionList = root?.querySelector('[data-phone-prefix-options]');
        const flag = root?.querySelector('[data-phone-prefix-flag]');
        const label = root?.querySelector('[data-phone-prefix-label]');
        let prefixes = [];

        const getSelectedPrefix = () => {
            const iso2 = countryIso2?.value || '';
            const dialCode = value?.value || '';
            return prefixes.find((prefix) => prefix.iso2 === iso2)
                || prefixes.find((prefix) => prefix.dialCode === dialCode)
                || null;
        };

        const closePanel = () => {
            panel?.classList.add('hidden');
            toggle?.setAttribute('aria-expanded', 'false');
        };

        const openPanel = () => {
            panel?.classList.remove('hidden');
            toggle?.setAttribute('aria-expanded', 'true');
            search?.focus();
            search?.select();
        };

        const renderOptions = (searchTerm = '') => {
            if (!optionList) {
                return;
            }

            const selectedIso2 = countryIso2?.value || '';
            const normalizedSearch = searchTerm.trim().toLowerCase();
            const filtered = prefixes
                .filter((prefix) => {
                    const haystack = [
                        prefix.iso2,
                        prefix.iso3,
                        prefix.countryName,
                        prefix.localizedCountryName,
                        prefix.dialCode
                    ].filter(Boolean).join(' ').toLowerCase();
                    return !normalizedSearch || haystack.includes(normalizedSearch);
                })
                .slice(0, 80);

            clearChildren(optionList);
            if (filtered.length === 0) {
                const empty = document.createElement('div');
                empty.className = 'px-3 py-2 text-sm text-slate-500';
                empty.textContent = emptyText;
                optionList.append(empty);
                return;
            }

            filtered.forEach((prefix) => {
                const option = document.createElement('button');
                option.type = 'button';
                option.role = 'option';
                option.className = optionClass;
                option.dataset.iso2 = prefix.iso2 || '';
                option.setAttribute('aria-selected', String(prefix.iso2 === selectedIso2));
                option.append(createFlag(prefix));
                option.append(createText('min-w-0 flex-1 truncate', prefix.localizedCountryName || prefix.countryName));
                option.append(createText('shrink-0 font-semibold text-slate-500', prefix.dialCode));
                option.addEventListener('click', () => {
                    setSelectedPrefix(prefix);
                    closePanel();
                    toggle?.focus();
                });
                optionList.append(option);
            });
        };

        const setSelectedPrefix = (prefix) => {
            if (!prefix || !value || !countryIso2) {
                return;
            }

            value.value = prefix.dialCode || '';
            countryIso2.value = prefix.iso2 || '';
            if (label) {
                label.textContent = `${prefix.iso2 || ''} ${prefix.dialCode || ''}`.trim();
            }
            if (toggle) {
                toggle.title = `${prefix.localizedCountryName || prefix.countryName || ''} ${prefix.dialCode || ''}`.trim();
            }
            if (flag) {
                flag.src = prefix.flagPath || '';
                flag.hidden = !prefix.flagPath;
            }
            renderOptions(search?.value || '');
            onChange(prefix);
        };

        const load = async () => {
            prefixes = endpoint ? await fetchJson(endpoint) : [];
            if (prefixes.length === 0) {
                onChange(null);
                return prefixes;
            }

            const currentIso2 = countryIso2?.value || '';
            const currentDialCode = value?.value || defaultDialCode;
            const selected = prefixes.find((prefix) => prefix.iso2 === currentIso2)
                || prefixes.find((prefix) => prefix.dialCode === currentDialCode && prefix.iso2 === defaultIso2)
                || prefixes.find((prefix) => prefix.dialCode === currentDialCode)
                || prefixes.find((prefix) => prefix.iso2 === defaultIso2)
                || prefixes[0];
            setSelectedPrefix(selected);
            return prefixes;
        };

        toggle?.addEventListener('click', () => {
            const isOpen = toggle.getAttribute('aria-expanded') === 'true';
            if (isOpen) {
                closePanel();
            } else {
                openPanel();
            }
        });

        search?.addEventListener('input', () => {
            renderOptions(search.value);
        });

        search?.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                closePanel();
                toggle?.focus();
                return;
            }

            if (event.key === 'Enter') {
                event.preventDefault();
                optionList?.querySelector('button')?.click();
            }
        });

        document.addEventListener('click', (event) => {
            if (combobox && !combobox.contains(event.target)) {
                closePanel();
            }
        });

        return {
            load,
            getSelectedPrefix,
            setSelectedPrefix
        };
    };

    window.DashboardPhonePrefixCombobox = { init };
})(window);
