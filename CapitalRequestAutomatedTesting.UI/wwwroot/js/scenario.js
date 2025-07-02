$(document).ready(function () {
    $('#RequestId').select2({
        tags: true,
        placeholder: "Select or enter a Request ID",
        allowClear: true
    });
});

document.addEventListener("DOMContentLoaded", () => {
    bindRequestIdEvents(); // bind immediately if already present

    loadRequestIds(); // 🔁 Load request list via AJAX
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});

function bindRequestIdEvents() {
    console.log("Binding RequestId event...");

    const $requestSelect = $('#RequestId');
    const scenarioContainer = document.getElementById("scenarioContainer");

    if (!$requestSelect.length || !scenarioContainer) {
        console.warn("RequestId or scenarioContainer not found");
        return;
    }

    if ($requestSelect.data('bound') === true) {
        console.log("RequestId already bound");
        return;
    }

    $requestSelect.data('bound', true);

    $requestSelect.on("change", function () {
        const requestId = $(this).val();
        console.log("RequestId changed:", requestId);

        if (!requestId) {
            scenarioContainer.innerHTML = "";
            return;
        }

        fetch(`/Scenario/GetScenariosForRequest?requestId=${requestId}`)
            .then(response => {
                showLoadingDelayed("Loading Requests...", 200);
                return response.text();
            })
            .then(html => {
                scenarioContainer.innerHTML = html;
                cancelDelayedLoading();
            })
            .catch(error => {
                console.error("Error loading scenarios:", error);
                cancelDelayedLoading();
            });
    });
}

//function bindRequestIdEvents() {
//    console.log("Binding RequestId event...");

//    const requestSelect = document.getElementById("RequestId");
//    const scenarioContainer = document.getElementById("scenarioContainer");

//    if (!requestSelect || !scenarioContainer) {
//        console.warn("RequestId or scenarioContainer not found");
//        return;
//    }

//    if (requestSelect.dataset.bound === "true") {
//        console.log("RequestId already bound");
//        return;
//    }

//    requestSelect.dataset.bound = "true";

//    requestSelect.addEventListener("change", function () {
//        console.log("RequestId changed:", this.value);
//        const requestId = this.value;
//        if (!requestId) {
//            scenarioContainer.innerHTML = "";
//            return;
//        }

//        fetch(`/Scenario/GetScenariosForRequest?requestId=${requestId}`)
//            .then(response => {
//                showLoadingDelayed("Loading Requests...", 200);
//                return response.text();
//            })
//            .then(html => {
//                scenarioContainer.innerHTML = html;
//                cancelDelayedLoading()
//            })
//            .catch(error => {
//                console.error("Error loading scenarios:", error);
//                cancelDelayedLoading();
//            });

//    });
//}

function bindRequestingGroupEvents() {
    const requestingGroupSelect = document.getElementById("RequestingGroupId");
    const targetGroupSelect = document.getElementById("TargetGroupId");
    const reviewerSelect = document.getElementById("ReviewerId");
    const proposalId = document.getElementById("RequestId")?.value;

    if (!requestingGroupSelect || !targetGroupSelect || !reviewerSelect) return;

    if (requestingGroupSelect.dataset.bound === "true") return;
    requestingGroupSelect.dataset.bound = "true";

    requestingGroupSelect.addEventListener("change", function () {
        const requestingGroupId = this.value;

        showLoadingDelayed("Loading Groups and Reviewers...", 200);

        fetch(`/Scenario/GetTargetGroupsAndReviewers?proposalId=${proposalId}&requestingGroupId=${requestingGroupId}`)
            .then(response => response.json())
            .then(data => {
                targetGroupSelect.innerHTML = '<option value="">--Select One--</option>';
                data.targetGroups.forEach(group => {
                    const option = document.createElement("option");
                    option.value = group.value;
                    option.text = group.text;
                    targetGroupSelect.appendChild(option);
                    cancelDelayedLoading();
                });

                reviewerSelect.innerHTML = '<option value="">--Select Reviewer--</option>';
                data.reviewers.forEach(reviewer => {
                    const option = document.createElement("option");
                    option.value = reviewer.value;
                    option.text = reviewer.text;
                    reviewerSelect.appendChild(option);
                });
            })
            .catch(error => {
                console.error("Error loading groups/reviewers:", error);
                cancelDelayedLoading();
            });
    });
}

function bindReviewerEvents() {
    const reviewerSelect = partial.querySelector('[id$="ReviewerId"]');
    const emailInput = partial.querySelector('[id$="ReviewerEmail"]');
    const userIdInput = partial.querySelector('[id$="ReviewerUserId"]');
    const requestedInfoTextarea = partial.querySelector('[id$="RequestedInformation"]');

    if (!reviewerSelect || !emailDisplay || !userIdDisplay || !requestedInfoTextarea) return;

    reviewerSelect.addEventListener("change", function () {
        console.log("✅ Reviewer changed:", this.value);

        const reviewerId = this.value;
        const proposalId = document.getElementById("RequestId")?.value;
        const requestingGroupId = document.getElementById("RequestingGroupId")?.value;
        const targetGroupId = document.getElementById("TargetGroupId")?.value;
        const displayText = document.getElementById("DisplayText")?.value || "Request More Information";

        if (!reviewerId || !proposalId || !requestingGroupId || !targetGroupId) {
            console.warn("Missing required values for fetch");
            return;
        }

        showLoadingDelayed("Loading Reviewer Details...", 200);
        fetch(`/Scenario/GetReviewerDetails?reviewerId=${reviewerId}&proposalId=${proposalId}&requestingGroupId=${requestingGroupId}&targetGroupId=${targetGroupId}&displayText=${encodeURIComponent(displayText)}`)
            .then(response => response.json())
            .then(data => {
                emailDisplay.textContent = data.reviewerEmail;
                userIdDisplay.textContent = data.reviewerUserId;
                requestedInfoTextarea.value = data.requestedInformation;
                cancelDelayedLoading();
            })
            .catch(error => {
                console.error("Error loading reviewer details:", error);
                cancelDelayedLoading();
            });
    });

}



// Observe the DOM for when the dropdowns are added
const observer = new MutationObserver((mutations, obs) => {
    const requestSelect = document.getElementById("RequestId");
    const requestingGroupSelect = document.getElementById("RequestingGroupId");
    const targetGroupSelect = document.getElementById("TargetGroupId");
    const reviewerSelect = document.getElementById("ReviewerId");

    if (requestSelect) {
        bindRequestIdEvents();
    }

    if (requestingGroupSelect && targetGroupSelect) {
        bindRequestingGroupEvents();
    }

    if (reviewerSelect) {
        bindReviewerEvents();
    }
});

document.addEventListener("DOMContentLoaded", () => {
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});

document.addEventListener('change', function (e) {
    if (e.target.name === 'SelectedScenarioIds') {
        const targetId = e.target.getAttribute('data-target');
        const partial = document.getElementById(targetId);

        if (partial) {
            partial.style.display = e.target.checked ? 'block' : 'none';

            if (e.target.checked) {
                // Wait for DOM to update
                setTimeout(() => {

                    const requestingGroupSelect = partial.querySelector('[id$="RequestingGroupId"]');
                    const targetGroupSelect = partial.querySelector('[id$="TargetGroupId"]');
                    const reviewerSelect = partial.querySelector('[id$="ReviewerId"]');

                    if (!requestingGroupSelect || !targetGroupSelect || !reviewerSelect) {
                        console.warn("Missing one or more group dropdowns in partial");
                        return;
                    }

                    // Prevent double-binding
                    if (requestingGroupSelect.dataset.bound === "true") return;
                    requestingGroupSelect.dataset.bound = "true";

                    requestingGroupSelect.addEventListener("change", function () {
                        const proposalId = document.getElementById("RequestId")?.value;
                        const requestingGroupId = this.value;

                        if (!requestingGroupId || !proposalId) return;

                        fetch(`/Scenario/GetTargetGroupsAndReviewers?proposalId=${proposalId}&requestingGroupId=${requestingGroupId}`)
                            .then(response => response.json())
                            .then(data => {
                                targetGroupSelect.innerHTML = '<option value="">--Select One--</option>';
                                data.targetGroups.forEach(group => {
                                    const option = document.createElement("option");
                                    option.value = group.value;
                                    option.text = group.text;
                                    targetGroupSelect.appendChild(option);
                                });

                                reviewerSelect.innerHTML = '<option value="">--Select Reviewer--</option>';
                                data.reviewers.forEach(reviewer => {
                                    const option = document.createElement("option");
                                    option.value = reviewer.value;
                                    option.text = reviewer.text;
                                    reviewerSelect.appendChild(option);
                                });

                                // ✅ Re-bind the change event after populating
                                reviewerSelect.addEventListener("change", function () {
                                    console.log("✅ Reviewer changed:", this.value);

                                    const reviewerId = this.value;
                                    const proposalId = document.getElementById("RequestId")?.value;
                                    const requestingGroupId = partial.querySelector('[id$="RequestingGroupId"]')?.value;
                                    const targetGroupId = partial.querySelector('[id$="TargetGroupId"]')?.value;
                                    const displayText = partial.querySelector('[id$="DisplayText"]')?.value || "Request More Information";

                                    if (!reviewerId || !proposalId || !requestingGroupId || !targetGroupId) {
                                        console.warn("Missing required values for reviewer fetch");
                                        return;
                                    }

                                    fetch(`/Scenario/GetReviewerDetails?reviewerId=${reviewerId}&proposalId=${proposalId}&requestingGroupId=${requestingGroupId}&targetGroupId=${targetGroupId}&displayText=${encodeURIComponent(displayText)}`)
                                        .then(response => response.json())
                                        .then(data => {
                                            const emailInput = partial.querySelector('[id$="ReviewerEmail"]');
                                            const userIdInput = partial.querySelector('[id$="ReviewerUserId"]');
                                            const requestedInfoTextarea = partial.querySelector('[id$="RequestedInformation"]');

                                            if (emailInput) emailInput.value = data.reviewerEmail;
                                            if (userIdInput) userIdInput.value = data.reviewerUserId;
                                            if (requestedInfoTextarea) requestedInfoTextarea.value = data.requestedInformation;
                                        });
                                });

                            });
                    });
                }, 0);

            }
        }
    }
});

function showSpinner(message = "Loading...") {
    const modal = document.getElementById("loadingModal");
    const messageEl = document.getElementById("loadingMessage");
    if (modal && messageEl) {
        messageEl.textContent = message;
        modal.style.display = "flex";
    }
}

function hideSpinner() {
    const modal = document.getElementById("loadingModal");
    if (modal) {
        modal.style.display = "none";
    }
}

function loadRequestIds() {
    showLoadingDelayed("Loading Requests...", 200);

    fetch("/Scenario/GetRequestIds")
        .then(response => response.json())
        .then(data => {
            const $requestSelect = $('#RequestId');

            if (!$requestSelect.length) {
                console.warn("RequestId dropdown not found");
                cancelDelayedLoading();
                return;
            }

            // Destroy existing Select2 instance if present
            if ($.fn.select2 && $requestSelect.hasClass('select2-hidden-accessible')) {
                $requestSelect.select2('destroy');
            }

            // Clear and repopulate options
            $requestSelect.empty().append('<option value="">-- Select One --</option>');
            data.forEach(item => {
                $requestSelect.append(new Option(item.text, item.value));
            });

            // Re-initialize Select2
            $requestSelect.select2({
                tags: true,
                placeholder: "Select or enter a Request ID",
                allowClear: true
            });

            bindRequestIdEvents(); // rebind after populating
            cancelDelayedLoading();
        })
        .catch(error => {
            console.error("Error loading request IDs:", error);
            cancelDelayedLoading();
        });
}


let spinnerTimeout;

function showLoadingDelayed(message = "Loading...", delay = 200) {
    spinnerTimeout = setTimeout(() => showLoading(message), delay);
}

function cancelDelayedLoading() {
    clearTimeout(spinnerTimeout);
    hideLoading();
}

function showLoading(message = 'Loading...') {
    const modal = document.getElementById('loadingModal');
    const messageElem = document.getElementById('loadingMessage');
    messageElem.textContent = message;
    modal.style.display = 'flex';
}

function hideLoading() {
    const modal = document.getElementById('loadingModal');
    modal.style.display = 'none';
}





