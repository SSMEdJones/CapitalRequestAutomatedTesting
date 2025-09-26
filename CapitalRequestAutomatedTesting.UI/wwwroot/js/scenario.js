import { ScenarioBinder } from './ScenarioBinder.js';
import { DevLogger } from './logger.js';
import { fetchJsonOrRenderError } from './fetchHelpers.js';
// Enable logging if not already set
window.DEBUG = window.DEBUG || true;

const ScenarioInitializer = {
    init() {
        DevLogger.info("Initializing ScenarioInitializer", "🚀");
        this.loadRequestIds();         // AJAX load
    },

    loadRequestIds() {
        DevLogger.info("Loading request IDs", "📋");
        showLoadingDelayed("Loading Requests...", 200);

        fetchJsonOrRenderError("/Scenario/GetRequestIds")
            .then(data => {
                const $requestSelect = $('#RequestId');
                if (!$requestSelect.length) {
                    DevLogger.warn("RequestId dropdown not found", "⛔");
                    return;
                }

                if ($.fn.select2 && $requestSelect.hasClass('select2-hidden-accessible')) {
                    $requestSelect.select2('destroy');
                }

                $requestSelect.empty().append('<option value="">-- Select One --</option>');
                data.forEach(item => $requestSelect.append(new Option(item.text, item.value)));

                DevLogger.info("Request IDs loaded", data.length);
                ScenarioBinder.bindRequestIdEvents();
            })
            .catch(err => {
                DevLogger.error("Error loading request IDs", err);
            })
            .finally(cancelDelayedLoading);
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

    form.addEventListener("submit", async (e) => {
        e.preventDefault(); // Prevent default form submission

        const actionType = document.getElementById("actionType")?.value;
        if (actionType === "RunSelected") {
            await runSelectedScenarios(form);
        }
    });
});

// SignalR-related variables and functions
let connection;
let connectionId;

// Initialize SignalR connection
async function initializeSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/scenarioProgressHub")
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
                // Exponential backoff or fixed delay
                return Math.min(10000, retryContext.previousRetryCount * 2000);
            }
        })
        .build();

    // Handle progress updates
    connection.on("UpdateProgress", function (data) {
        showScenarioProgress(data.current, data.total, data.scenarioName, data.currentStep);
    });

    // Handle scenario completion
    connection.on("ScenarioComplete", function (data) {
        DevLogger.info(`Scenario completed: ${data.scenarioName}`, "✅");
        // You could show a brief success indicator here
    });

    // Handle errors
    connection.on("ScenarioError", function (data) {
        DevLogger.error(`Scenario failed: ${data.scenarioName}`, data.error);
        const statusDiv = document.getElementById('status');
        if (statusDiv) {
            statusDiv.innerHTML = `
                <div class="alert alert-danger">
                    <strong>❌ Error in ${data.scenarioName}:</strong><br>
                    ${data.error}
                </div>
            `;
        }
    });

    connection.onclose(error => {
        DevLogger.warn("SignalR connection closed", error);
    });

    connection.onreconnecting(error => {
        DevLogger.info("SignalR reconnecting...", error);
    });

    connection.onreconnected(connectionId => {
        DevLogger.info("SignalR reconnected", connectionId);
    });


    try {
        await connection.start();
        connectionId = connection.connectionId;
        await connection.invoke("JoinScenarioGroup", connectionId);
        DevLogger.info("SignalR connected", connectionId);
    } catch (err) {
        DevLogger.error("SignalR connection error", err);
    }
}

// New function to handle running selected scenarios with progress tracking
async function runSelectedScenarios(form) {
    const requestSelect = document.getElementById("RequestId");
    const requestId = requestSelect?.value;

    if (!isValidRequestId(requestId)) {
        alert('⚠️ Please select a valid Request ID before running scenarios.');
        return;
    }

    const checkboxes = document.querySelectorAll('input[name="SelectedScenarioIds"]:checked');
    const statusDiv = document.getElementById('status') || createStatusDiv();

    if (checkboxes.length === 0) {
        alert('⚠️ Please select at least one scenario to run.');
        return;
    }

    // Ensure SignalR is connected
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        await initializeSignalR();
    }

    statusDiv.innerHTML = '';
    statusDiv.style.display = 'block';

    // Get scenario details for better messaging
    const selectedScenarios = getSelectedScenarioDetails(checkboxes);
    const scenarioNames = selectedScenarios.map(s => s.name);

    showScenarioProgress(0, selectedScenarios.length, "System", "Initializing scenarios");

    DevLogger.info(`Starting ${selectedScenarios.length} scenario(s)`, scenarioNames);

    const totalStart = performance.now();

    try {
        const formData = new FormData(form);
        formData.append("actionType", "RunSelected");
        formData.append("connectionId", connectionId); // Pass connection ID

        // Show initial progress
        updateScenarioProgress(0, selectedScenarios.length, "System", "Starting scenario execution");

        // Submit the form and let the server handle sequential processing
        const response = await fetch("/Scenario/Index", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        // Check if response is HTML (redirect) or JSON
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
            const result = await response.json();
            handleJsonResponse(result, statusDiv);
        } else {
            // Server returned HTML (likely a redirect), replace current page
            const html = await response.text();
            document.open();
            document.write(html);
            document.close();
        }

    } catch (error) {
        DevLogger.error('Error running scenarios:', error);
        statusDiv.innerHTML = `
            <div class="alert alert-danger">
                <strong>❌ Error running scenarios:</strong><br>
                ${error.message || 'Unknown error occurred'}
            </div>
        `;
    }
}

function getSelectedScenarioDetails(checkboxes) {
    return Array.from(checkboxes).map(checkbox => {
        const scenarioCard = checkbox.closest('.card, .scenario-item, .form-check');
        const nameElement = scenarioCard?.querySelector('.scenario-name, .card-title, label');
        const scenarioName = nameElement?.textContent?.trim() || checkbox.value;

        return {
            id: checkbox.value,
            name: scenarioName,
            element: checkbox
        };
    });
}

function createStatusDiv() {
    const statusDiv = document.createElement('div');
    statusDiv.id = 'status';
    statusDiv.className = 'mt-3';

    const form = document.getElementById('scenarioForm');
    if (form && form.parentNode) {
        form.parentNode.insertBefore(statusDiv, form.nextSibling);
    } else {
        document.body.appendChild(statusDiv);
    }

    return statusDiv;
}

function showScenarioProgress(current, total, scenarioName, currentStep) {
    const statusDiv = document.getElementById('status') || createStatusDiv();
    const progressPercent = total > 0 ? Math.round((current / total) * 100) : 0;

    statusDiv.innerHTML = `
        <div class="progress-container">
            <div class="alert alert-info">
                <div class="d-flex align-items-center mb-2">
                    <div class="spinner-border spinner-border-sm me-2" role="status">
                        <span class="visually-hidden">Processing...</span>
                    </div>
                    <strong>🔄 Processing Scenario ${current + 1} of ${total}</strong>
                </div>
                <div class="scenario-details">
                    <strong>Scenario:</strong> ${scenarioName}<br>
                    <strong>Current Step:</strong> ${currentStep}
                </div>
                <div class="progress mt-2" style="height: 25px;">
                    <div class="progress-bar progress-bar-striped progress-bar-animated bg-primary" 
                         role="progressbar"
                         style="width: ${progressPercent}%"
                         aria-valuenow="${current}"
                         aria-valuemin="0"
                         aria-valuemax="${total}">
                        ${progressPercent}%
                    </div>
                </div>
                <small class="text-muted">
                    <i class="fas fa-info-circle"></i> 
                    Scenarios are executed sequentially to maintain dependencies
                </small>
            </div>
        </div>
    `;
}

function updateScenarioProgress(current, total, scenarioName, currentStep) {
    showScenarioProgress(current, total, scenarioName, currentStep);
    DevLogger.info(`Scenario Progress: ${current + 1}/${total}`, `${scenarioName} - ${currentStep}`);
}

function handleJsonResponse(result, statusDiv) {
    if (result.success) {
        const totalElapsed = result.executionTime || 0;
        statusDiv.innerHTML = `
            <div class="alert alert-success">
                <div class="d-flex align-items-center">
                    <i class="fas fa-check-circle me-2"></i>
                    <strong>✅ All scenarios completed successfully!</strong>
                </div>
                <div class="mt-2">
                    <strong>Results:</strong>
                    <ul class="mb-0 mt-1">
                        ${result.scenarios?.map(s => `
                            <li>${s.name}: ${s.success ? '✅ Success' : '❌ Failed'}</li>
                        `).join('') || ''}
                    </ul>
                </div>
                ${totalElapsed > 0 ? `
                    <div class="mt-2">
                        <small class="text-muted">
                            <i class="fas fa-clock"></i> 
                            Total execution time: ${(totalElapsed / 1000).toFixed(2)} seconds
                        </small>
                    </div>
                ` : ''}
            </div>
        `;
    } else {
        statusDiv.innerHTML = `
            <div class="alert alert-warning">
                <strong>⚠️ Some scenarios completed with issues:</strong>
                <ul class="mb-0 mt-1">
                    ${result.scenarios?.map(s => `
                        <li>${s.name}: ${s.success ? '✅ Success' : `❌ ${s.error || 'Failed'}`}</li>
                    `).join('') || ''}
                </ul>
            </div>
        `;
    }
}

function isValidRequestId(id) {
    return id && id !== "" && id !== "0" && id !== 0;
}

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


// Initialize SignalR when DOM loads
document.addEventListener("DOMContentLoaded", async () => {
    DevLogger.info("DOM loaded, initializing modules", "🌐");

    try {
        ScenarioBinder.init();
        ScenarioInitializer.init();
    } catch (e) {
        DevLogger.error("Initialization error", e);
    }

    await initializeSignalR();
    // File upload handling is now exclusively in ScenarioBinder.bindFileUploadEvents()
});

