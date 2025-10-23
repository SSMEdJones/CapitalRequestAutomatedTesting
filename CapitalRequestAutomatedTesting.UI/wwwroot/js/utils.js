import { DevLogger } from './logger.js';

export function populateDropdown(select, items, defaultText) {
    if (!select) {
        DevLogger.warn("Cannot populate dropdown - select element is null", defaultText);
        return;
    }

    select.innerHTML = `<option value="">${defaultText}</option>`;
    items?.forEach(item => {
        const option = document.createElement("option");
        option.value = item.value;
        option.text = item.text;
        select.appendChild(option);
    });

    DevLogger.info("Dropdown populated", {
        selectId: select.id,
        itemCount: items?.length || 0,
        defaultText
    });
}

let spinnerTimeout;

export function showLoadingDelayed(message = "Loading...", delay = 200) {
    DevLogger.info("Showing delayed loading indicator", message);
    spinnerTimeout = setTimeout(() => showLoading(message), delay);
}

export function cancelDelayedLoading() {
    DevLogger.info("Canceling delayed loading indicator", "✓");
    clearTimeout(spinnerTimeout);
    hideLoading();
}

export function showLoading(message = 'Loading...') {
    const modal = document.getElementById('loadingModal');
    const messageElem = document.getElementById('loadingMessage');
    if (modal && messageElem) {
        messageElem.textContent = message;
        modal.style.display = 'flex';
        DevLogger.info("Loading indicator displayed", message);
    }
}

export function hideLoading() {
    const modal = document.getElementById('loadingModal');
    if (modal) {
        modal.style.display = 'none';
        DevLogger.info("Loading indicator hidden", "✓");
    }
}

export function showSpinner(message = "Loading...") {
    const modal = document.getElementById("loadingModal");
    const messageEl = document.getElementById("loadingMessage");
    if (modal && messageEl) {
        messageEl.textContent = message;
        modal.style.display = "flex";
        DevLogger.info("Spinner displayed", message);
    }
}

export function hideSpinner() {
    const modal = document.getElementById("loadingModal");
    if (modal) {
        modal.style.display = "none";
        DevLogger.info("Spinner hidden", "✓");
    }
}