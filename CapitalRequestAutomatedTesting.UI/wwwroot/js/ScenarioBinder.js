import { showLoadingDelayed, cancelDelayedLoading, populateDropdown} from './scenario.js';
import { DevLogger } from './logger.js';
// Enable logging
window.DEBUG = true; // Set to true to enable logging, false to disable

export const ScenarioBinder = {
    init() {
        DevLogger.info("Initializing ScenarioBinder", "🚀");
        this.bindRequestIdEvents(); // Bind the dropdown listener
    },    
    bindRequestIdEvents() {
        const requestSelect = document.getElementById("RequestId");
        const scenarioContainer = document.getElementById("scenarioContainer");

        if (!requestSelect || !scenarioContainer) {
            DevLogger.warn("RequestId or scenarioContainer not found", "⛔");
            return;
        }

        if (requestSelect.dataset.bound === "true") {
            DevLogger.info("RequestId already bound", "🔁");
            return;
        }

        // Initialize Select2
        $('#RequestId').select2({
            tags: true,
            placeholder: "Select or enter a Request ID",
            allowClear: true
        });

        requestSelect.dataset.bound = "true";

        // Listen for dropdown change
        requestSelect.addEventListener("change", () => {
            const requestId = requestSelect.value;
            if (!requestId) {
                scenarioContainer.innerHTML = "";
                DevLogger.warn("No Request ID selected — skipping fetch", "🚫");
                return;
            }

            DevLogger.info("Fetching scenarios for", requestId);

            fetch(`/Scenario/GetScenariosForRequest?requestId=${requestId}`)
                .then(response => response.text())
                .then(html => {
                    scenarioContainer.innerHTML = html;
                    DevLogger.info("Scenario HTML injected", "✅");

                    ScenarioBinder.rebindScenarioPartials(requestId);
                    this.observeDynamicPartials(requestId);  // Use this instead of ScenarioInitializer
                })
                .catch(error => {
                    DevLogger.error("Error loading scenarios", error);
                });
        });
    },
    bindScenarioPartial(partial, proposalId) {
        if (!partial || !proposalId) {
            DevLogger.warn("Cannot bind scenario partial", {
                partialExists: !!partial,
                proposalId
            });
            return;
        }

        // Check if partial is empty - if so, wait for content
        if (!partial.children.length) {
            DevLogger.info("Empty partial - waiting for content", partial.id);
            return;
        }

        const scenarioId = partial.id?.replace("partial-", "") || "Unknown";
        DevLogger.group(`Binding Partial for Scenario ${scenarioId}`);

        // Continue with existing binding logic...
        const requestingGroupSelect = this.getField(partial, "requestingGroupId");
        const replyingGroupSelect = this.getField(partial, "replyingGroupId");
        
        // Bind appropriate events based on scenario type
        if (requestingGroupSelect) {
            // Check if this is a replying scenario by looking for the replyingGroupId field
            const replyingGroupField = this.getField(partial, "replyingGroupId");
            const isReplyingScenario = !!replyingGroupField;

            // Skip binding requesting group for replying scenarios
            if (isReplyingScenario) {
                DevLogger.info("Skipping binding for requesting group in replying scenario", partial.id);
            } else {
                this.bindGroupChange(partial, proposalId, "requesting");
            }
        }

        if (replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "replying");
        }

        DevLogger.groupEnd();
    },

    bindGroupChange(partial, proposalId, groupType) {
        const groupSelect = this.getField(
            partial,
            groupType === "replying" ? "replyingGroupId" : "requestingGroupId"
        );

        DevLogger.group("Group Change Binding");
        DevLogger.info("Binding change event to", groupSelect?.id);
        DevLogger.info("Current group value", groupSelect?.value);
        DevLogger.info("bindGroupChange called for groupType", groupType);
        DevLogger.groupEnd();

        if (!groupSelect || groupSelect.__bound) return;

        groupSelect.__bound = true;

        groupSelect.addEventListener("change", () => {
            const groupId = groupSelect.value;
            if (!groupId || !proposalId) return;
            
            DevLogger.info(`${groupType} group changed to`, groupId);

            fetch(`/Scenario/GetTargetGroupsAndReviewers?proposalId=${proposalId}&groupId=${groupId}&groupType=${groupType}`)
                .then(res => res.json())
                .then(data => {
                    const targetGroupSelect = this.getField(partial, "targetGroupId");
                    const reviewerSelect = this.getField(partial, "reviewerId");

                    if (groupType === "replying") {
                        const select = this.getField(partial, "RequestingGroupId");

                        const requestingGroup = this.findSiblingFieldBySwap(partial, "GroupId", "ReplyingGroupId", "RequestingGroupId");

                        if (data.targetGroups?.length > 1) {

                            //this.toggleFormGroupByFieldId(requestingGroup, true, partial);
                            
                            //DevLogger.info("Multiple target groups found", data.targetGroups.length);
                            populateDropdown(select, data.targetGroups, "-- Select One --");
                            select.value = "";
                        } else {
                            //this.toggleFormGroupByFieldId(requestingGroup, false, partial);
                            populateDropdown(select, data.targetGroups, "-- Select One --");
                            // ✅ Auto-select the only available group
                            if (data.targetGroups.length === 1) {
                                select.selectedIndex = 1;
                            }
                        }
                    }

                    // Only populate targetGroupSelect if it's not a reply scenario
                    if (targetGroupSelect && groupType !== "replying") {
                        populateDropdown(targetGroupSelect, data.targetGroups, "--Select One--");
                    }

                    if (reviewerSelect) {
                        populateDropdown(reviewerSelect, data.reviewers, "--Select Reviewer--");

                        this.bindReviewerChange(partial, proposalId, groupType);
                        DevLogger.info("Reviewer change event bound for groupType", groupType);
                    }
                });
        });
    },

    bindSelectedGroupEvents(partial) {
        if (!partial) {
            DevLogger.warn("⛔ bindSelectedGroupEvents: partial not provided");
            return;
        }

        const $ = ScenarioBinder.getField.bind(ScenarioBinder);
        const proposalId = document.getElementById("RequestId")?.value;
        const scenarioId = partial.id?.replace("partial-", "") || "Unknown";

        const replyingGroupSelect = $(partial, "replyingGroupId");
        const targetGroupSelect = $(partial, "targetGroupId");
        const reviewerSelect = $(partial, "reviewerId");

        if (!replyingGroupSelect || !targetGroupSelect || !reviewerSelect) {
            DevLogger.warn("⚠️ Missing fields for reply-triggered binding", {
                scenarioId,
                replyingGroup: !!replyingGroupSelect,
                targetGroup: !!targetGroupSelect,
                reviewer: !!reviewerSelect
            });
            return;
        }

        if (replyingGroupSelect.dataset.bound === "true") {
            DevLogger.info("🔁 Replying group already bound for", scenarioId);
            return;
        }

        replyingGroupSelect.dataset.bound = "true";

        replyingGroupSelect.addEventListener("change", function () {
            const replyingGroupId = this.value;
            if (!replyingGroupId || !proposalId) return;

            DevLogger.group(`🔗 Replying Group Changed: ${replyingGroupId}`);
            DevLogger.info("📤 Fetching target groups + reviewers for reply");

            fetch(`/Scenario/GetTargetGroupsAndReviewers?proposalId=${proposalId}&replyingGroupId=${replyingGroupId}`)
                .then(response => response.json())
                .then(data => {
                    targetGroupSelect.innerHTML = '<option value="">--Select One--</option>';
                    data.targetGroups?.forEach(group => {
                        const option = document.createElement("option");
                        option.value = group.value;
                        option.text = group.text;
                        targetGroupSelect.appendChild(option);
                    });

                    reviewerSelect.innerHTML = '<option value="">--Select Reviewer--</option>';
                    data.reviewers?.forEach(reviewer => {
                        const option = document.createElement("option");
                        option.value = reviewer.value;
                        option.text = reviewer.text;
                        reviewerSelect.appendChild(option);
                    });

                    DevLogger.table("🎯 Reply-triggered dropdowns", {
                        TargetGroups: data.targetGroups?.length || 0,
                        Reviewers: data.reviewers?.length || 0
                    });

                    DevLogger.groupEnd();
                })
                .catch(error => {
                    DevLogger.error("🔥 Error fetching reply-driven data", error);
                });
        });
    },

    rebindScenarioPartials(requestId) {
        DevLogger.info("Rebinding scenario partials for request", requestId);
        
        // First bind any existing partials
        document.querySelectorAll(".scenario-partial").forEach(partial => {
            if (partial.children.length > 0) {
                DevLogger.info("Binding existing partial", partial.id);
                this.bindScenarioPartial(partial, requestId);
            }
        });

        // Then set up an observer for partials that get content later
        const observer = new MutationObserver((mutations) => {
            mutations.forEach(mutation => {
                if (mutation.type === 'childList' && 
                    mutation.target.classList.contains('scenario-partial')) {
                    DevLogger.info("Partial content changed - binding", mutation.target.id);
                    this.bindScenarioPartial(mutation.target, requestId);
                }
            });
        });

        // Observe all scenario partials
        document.querySelectorAll(".scenario-partial").forEach(partial => {
            observer.observe(partial, { childList: true, subtree: true });
        });

        // Store observer for cleanup later if needed
        this._partialObserver = observer;
    },
    
    getField(container, key) {
        const normalizedKey = key.toLowerCase();
        console.log(`🔍 Looking for field with key: "${key}" (normalized: "${normalizedKey}")`);

        const allElements = Array.from(container.querySelectorAll("select, input, textarea"));
        console.log(`📊 Found ${allElements.length} potential fields:`);

        allElements.forEach(el => {
            console.log(`➡️ ID: ${el.id} | NAME: ${el.name}`);
        });

        const directMatch = allElements.find(el =>
            el.id && el.id.toLowerCase().endsWith(normalizedKey)
        );
        if (directMatch) {
            console.log("✅ Direct match found by ID suffix:", directMatch);
            return directMatch;
        }

        const aspMatch = allElements.find(el =>
            el.id && el.id.toLowerCase().includes(`__${normalizedKey}`)
        );
        if (aspMatch) {
            console.log("✅ Match found by ASP.NET-style ID pattern:", aspMatch);
            return aspMatch;
        }

        const nameMatch = allElements.find(el =>
            el.name && el.name.toLowerCase().endsWith(`.${normalizedKey}`)
        );
        if (nameMatch) {
            console.log("✅ Match found by name attribute:", nameMatch);
            return nameMatch;
        }

        console.warn("❌ No match found for key:", key);
        return null;
    },
    
    bindScenario(partial, proposalId) {
        const requestingGroupSelect = this.getField(partial, "requestingGroupId");
        const replyingGroupSelect = this.getField(partial, "replyingGroupId");

        if (requestingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "requesting");
        }

        if (replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "replying");
        }
    },
    
    bindReviewerChange(partial, proposalId) {
        const reviewerSelect = this.getField(partial, "reviewerId");
        if (!reviewerSelect) return;

        reviewerSelect.addEventListener("change", () => {
            const reviewerId = reviewerSelect.value;
            const requestingGroupField = this.getField(partial, "requestingGroupId");
            const requestingGroupId = requestingGroupField?.value;
            const targetGroupId = this.getField(partial, "targetGroupId")?.value || null;
            const replyingGroupId = this.getField(partial, "replyingGroupId")?.value || null;

            const scenarioId = partial.id.replace("partial-", "");
            const checkbox = document.querySelector(`input[type="checkbox"][value="${scenarioId}"]`);
            const displayText = checkbox?.parentElement?.textContent?.trim();

            const params = new URLSearchParams({
                reviewerId,
                proposalId,
                requestingGroupId,
                targetGroupId,
                replyingGroupId,
                displayText
            });

            fetch(`/Scenario/GetReviewerDetails?${params}`)
                .then(res => res.json())
                .then(data => {
                    const fields = [
                        this.getField(partial, "reviewerEmail"),
                        this.getField(partial, "reviewerUserId"),
                        this.getField(partial, "requestedInformation"),
                        this.getField(partial, "returnedInformation"),
                        this.getField(partial, "requestedInfoId"),
                        requestingGroupField
                    ];

                    const values = [
                        data.reviewerEmail,
                        data.reviewerUserId,
                        data.requestedInformation,
                        data.returnedInformation,
                        data.requestedInfoId,
                        data.requestingGroupId
                    ];

                    fields.forEach((field, i) => {
                        if (!field) return;

                        // Only assign requestingGroupId if it's non-empty
                        if (field === requestingGroupField) {
                            if (data.requestingGroupId) {
                                field.value = data.requestingGroupId;
                            }
                        } else {
                            field.value = values[i];
                        }
                    });

                    // 🔹 Auto-select requesting group only if not already selected
                    if (
                        requestingGroupField &&
                        requestingGroupField.options.length === 2 &&
                        requestingGroupField.value === ""
                    ) {
                        requestingGroupField.selectedIndex = 1;
                    }

                    DevLogger.info("Fields populated from reviewer data", {
                        email: data.reviewerEmail,
                        requestedInfoId: data.requestedInfoId,
                        requestedInformation: data.requestedInformation,
                        returnedInformation: data.returnedInformation,
                        requestingGroupId: data.requestingGroupId
                    });
                });
        });
    }
,
    
    bindSelectedGroupEvents(partial) {
        console.log("🧠 bindSelectedGroupEvents loaded");

        const requestingGroupSelect = this.getField(partial, "requestingGroupId");
        const targetGroupSelect = this.getField(partial, "targetGroupId");
        const reviewerSelect = this.getField(partial, "reviewerId");
        const proposalId = document.getElementById("RequestId")?.value;

        if (!requestingGroupSelect || !targetGroupSelect || !reviewerSelect) return;

        if (requestingGroupSelect.dataset.bound === "true") return;
        requestingGroupSelect.dataset.bound = "true";

        requestingGroupSelect.addEventListener("change", function () {
            const requestingGroupId = this.value;
            if (!requestingGroupId || !proposalId) return;

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
                    });

                    reviewerSelect.innerHTML = '<option value="">--Select Reviewer--</option>';
                    data.reviewers.forEach(reviewer => {
                        const option = document.createElement("option");
                        option.value = reviewer.value;
                        option.text = reviewer.text;
                        reviewerSelect.appendChild(option);
                    });

                    cancelDelayedLoading();
                })
                .catch(error => {
                    console.error("Error loading groups/reviewers:", error);
                    cancelDelayedLoading();
                });
        });
    },
    
    bindReviewerEvents(partial) {

        const reviewerSelect = this.getField(partial, "reviewerId");
        const emailInput = this.getField(partial, "reviewerEmail");
        const userIdInput = this.getField(partial, "reviewerUserId");
        const requestedInfoTextarea = this.getField(partial, "requestedInformation");
        const returnedInfoTextarea = this.getField(partial, "returnedInfoTextarea");
        const proposalId = this.getField(partial, "proposalId")?.value;

        if (!reviewerSelect) {
            console.warn("Reviewer select not found");
            return;
        }

        console.log("✅ Bound reviewer change event");

        reviewerSelect.addEventListener("change", () => {
            const reviewerId = reviewerSelect.value;
            const requestingGroupId = this.getField(partial, "requestingGroupId")?.value ?? "";
            const replyingGroupId = this.getField(partial, "replyingGroupId")?.value ?? "";
            const targetGroupId = this.getField(partial, "targetGroupId")?.value ?? "";

            const checkbox = partial.querySelector('input[type="checkbox"]');
            const displayText = checkbox?.parentElement?.textContent?.trim();

            const groupId = requestingGroupId || replyingGroupId;
            const groupType = requestingGroupId ? "requesting" : "replying";

            if (!reviewerId || !proposalId || !groupId) {
                console.warn("Missing required values for reviewer fetch");
                return;
            }

            showLoadingDelayed("Loading Reviewer Details...", 200);

            console.log("🧪 Fetching with targetGroupId:", targetGroupId);

            const params = new URLSearchParams({
                reviewerId,
                proposalId,
                requestingGroupId,
                targetGroupId,
                replyingGroupId,
                displayText
            });

            fetch(`/Scenario/GetReviewerDetails?${params}`)
                .then(res => res.json())
                .then(data => {
                    if (emailInput) emailInput.value = data.reviewerEmail;
                    if (userIdInput) userIdInput.value = data.reviewerUserId;
                    if (requestedInfoTextarea) requestedInfoTextarea.value = data.requestedInformation;
                    if (returnedInfoTextarea) returnedInfoTextarea.value = data.returnedInformation;

                    cancelDelayedLoading();
                })
                .catch(err => {
                    console.error("❌ Error fetching reviewer details:", err);
                    cancelDelayedLoading();
                });
        });
    },  
    toggleFormGroupByFieldId(fieldId, show, container) {
        if (!fieldId || !container) {
            DevLogger.warn("Missing field ID or container for toggle");
            return;
        }

        //const field = container.querySelector(`#${fieldId}`);
        //if (!field) {
        //    DevLogger.warn("Field not found in toggle", fieldId);
        //    return;
        //}

        const formGroup = fieldId.closest(".form-group");
        if (!formGroup) {
            DevLogger.warn("Form group wrapper not found for field", fieldId);
            return;
        }

        formGroup.style.display = show ? "block" : "none";
        DevLogger.info(`Form group for "${fieldId}" set to`, show ? "visible" : "hidden");
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
                        this.bindScenario(node, proposalId);
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    },
    findSiblingFieldBySwap(partial, baseKey, fromKey, toKey) {
        const baseField = this.getField(partial, fromKey);
        if (!baseField || !baseField.id) return null;

        const siblingId = baseField.id.replace(fromKey, toKey);
        return partial.querySelector(`#${siblingId}`);
    }

};


