import { ScenarioBinder } from './ScenarioBinder.js';
import { DevLogger } from './logger.js';

// Enable logging if not already set
window.DEBUG = window.DEBUG || true;

const ScenarioInitializer = {
    init() {
        DevLogger.info("Initializing ScenarioInitializer", "🚀");
        this.loadRequestIds();         // AJAX load
    },

    loadRequestIds() {
        DevLogger.info("Loading request IDs", "📋");
        showLoadingDelayed("Loading Requests...", 200);  // Show spinner

        fetch("/Scenario/GetRequestIds")
            .then(response => response.json())
            .then(data => {
                const $requestSelect = $('#RequestId');

                if (!$requestSelect.length) {
                    DevLogger.warn("RequestId dropdown not found", "⛔");
                    cancelDelayedLoading();  // Hide spinner on error
                    return;
                }

                // Destroy existing Select2 instance if present
                if ($.fn.select2 && $requestSelect.hasClass('select2-hidden-accessible')) {
                    $requestSelect.select2('destroy');
                }

                // Populate the dropdown
                $requestSelect.empty().append('<option value="">-- Select One --</option>');
                data.forEach(item => {
                    $requestSelect.append(new Option(item.text, item.value));
                });

                DevLogger.info("Request IDs loaded", data.length);

                // Let ScenarioBinder handle UI + behavior
                ScenarioBinder.bindRequestIdEvents();

                cancelDelayedLoading();  // Hide spinner on success
            })
            .catch(error => {
                DevLogger.error("Error loading request IDs", error);
                cancelDelayedLoading();  // Hide spinner on error
            });
    },

    observeDynamicPartials(proposalId) {
        if (!proposalId) {
            DevLogger.warn("No proposal ID found for observer", "🛑");
            return;
        }

        DevLogger.info("Setting up mutation observer for proposal", proposalId);

        const observer = new MutationObserver((mutations) => {
            mutations.forEach(mutation => {
                mutation.addedNodes.forEach(node => {
                    if (node.nodeType === 1 && node.classList.contains("scenario-partial")) {
                        DevLogger.info("Dynamic partial detected", node.id);
                        ScenarioBinder.bindScenario(node, proposalId);
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
};

document.addEventListener("DOMContentLoaded", () => {
    DevLogger.info("DOM loaded, initializing modules", "🌐");
    ScenarioBinder.init();
    ScenarioInitializer.init();

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    const form = document.getElementById("scenarioForm");
    if (!form) {
        DevLogger.warn("scenarioForm not found");
        return;
    }

    form.addEventListener("submit", (e) => {
        e.preventDefault(); // Prevent default form submission

        const actionType = document.getElementById("actionType")?.value;
        if (actionType === "RunSelected") {
            showSpinner("Running selected scenario...");

            // 📦 Use FormData to serialize the entire form
            const formData = new FormData(form);
            formData.append("actionType", actionType); // Include actionType if not part of the form

            fetch("/Scenario/Index", {
                method: "POST",
                body: formData // No need for headers — browser sets content type automatically
            })
                .then(res => {
                    if (!res.ok) throw new Error("Request failed");
                    return res.text(); // Or .json() depending on response type
                })
                .then(data => {
                    console.log("Success:", data);
                    // Handle response or redirect if needed
                })
                .catch(err => {
                    console.error("Error:", err);
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });

});


//document.addEventListener("DOMContentLoaded", () => {
//    DevLogger.info("DOM loaded, initializing modules", "🌐");
//    ScenarioBinder.init();
//    ScenarioInitializer.init();

//    observer.observe(document.body, {
//        childList: true,
//        subtree: true
//    });
//});

const observer = new MutationObserver((mutations, obs) => {
    const requestSelect = document.getElementById("RequestId");
    const requestingGroupSelect = document.getElementById("RequestingGroupId");
    const replyingGroupSelect = document.getElementById("ReplyingGroupId");
    const targetGroupSelect = document.getElementById("TargetGroupId");
    const reviewerSelect = document.getElementById("ReviewerId");

    // Find the container where the elements were added
    let container = null;
    if (mutations.length > 0 && mutations[0].target) {
        container = mutations[0].target.closest('.scenario-partial') || document.body;
    }

    if ((requestingGroupSelect && targetGroupSelect) || replyingGroupSelect) {
        DevLogger.info("DOM changed - binding selected group events", "🔄");
        ScenarioBinder.bindSelectedGroupEvents(container); // Pass container
    }

    if (reviewerSelect) {
        DevLogger.info("DOM changed - binding reviewer events", "👤");
        ScenarioBinder.bindReviewerEvents(container); // Pass container
    }
});

document.addEventListener('change', async function (e) {
    if (e.target.name !== 'SelectedScenarioIds') return;

    const scenarioId = e.target.value;
    const targetId = e.target.getAttribute('data-target');
    const requestSelect = document.getElementById("RequestId");
    const proposalId = requestSelect?.value;

    // Add debug logging
    DevLogger.group("Scenario Selection Changed");
    DevLogger.info("Scenario toggled", scenarioId);
    DevLogger.info("Target ID", targetId);
    DevLogger.info("Request/Proposal ID", proposalId);
    DevLogger.info("Request element found", !!requestSelect);
    DevLogger.groupEnd();

    // Enhanced validation
    if (!proposalId || proposalId === "0" || proposalId === 0) {
        DevLogger.warn("Valid Proposal ID not found or is zero", proposalId);
        return;
    }

    const partial = document.getElementById(targetId);
    const show = e.target.checked;

    // If showing and partial doesn't have content, fetch it
    if (show && partial && partial.children.length === 0) {
        try {
            DevLogger.info("Fetching partial view for scenario", scenarioId);
            showLoadingDelayed("Loading Scenario...", 200);

            // Use requestId parameter name to match controller expectation
            const params = new URLSearchParams({
                scenarioId,
                requestId: proposalId  // Changed from proposalId to requestId
            });

            const html = await fetch(`/Scenario/GetPartialViewForScenario?${params}`)
                .then(r => r.text());

            partial.innerHTML = html;
            partial.style.display = 'block';
            
            DevLogger.info("Partial view injected successfully", targetId);
            cancelDelayedLoading();

            // Bind events to the newly loaded partial
            ScenarioBinder.bindScenarioPartial(partial, proposalId);
            
        } catch (err) {
            DevLogger.error("Error loading scenario partial", err);
            cancelDelayedLoading();
            return;
        }
    } else if (partial) {
        // Just toggle visibility for already loaded partials
        partial.style.display = show ? "block" : "none";
        DevLogger.info(`Scenario partial ${show ? 'shown' : 'hidden'}`, targetId);
    }
});

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

function showLoading(message = 'Loading...') {
    const modal = document.getElementById('loadingModal');
    const messageElem = document.getElementById('loadingMessage');
    if (modal && messageElem) {
        messageElem.textContent = message;
        modal.style.display = 'flex';
        DevLogger.info("Loading indicator displayed", message);
    } else {
        DevLogger.warn("Cannot show loading - modal elements not found", "⚠️");
    }
}

function hideLoading() {
    const modal = document.getElementById('loadingModal');
    if (modal) {
        modal.style.display = 'none';
        DevLogger.info("Loading indicator hidden", "✓");
    } else {
        DevLogger.warn("Cannot hide loading - modal element not found", "⚠️");
    }
}

function showSpinner(message = "Loading...") {
    const modal = document.getElementById("loadingModal");
    const messageEl = document.getElementById("loadingMessage");
    if (modal && messageEl) {
        messageEl.textContent = message;
        modal.style.display = "flex";
        DevLogger.info("Spinner displayed", message);
    }
}

function hideSpinner() {
    const modal = document.getElementById("loadingModal");
    if (modal) {
        modal.style.display = "none";
        DevLogger.info("Spinner hidden", "✓");
    }
}
