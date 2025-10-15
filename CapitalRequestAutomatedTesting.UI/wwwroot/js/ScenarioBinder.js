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

        // 🆕 Bind file upload events for this partial
        this.bindFileUploadEvents(partial, scenarioId);

        DevLogger.groupEnd();
    },

    // 🆕 Add this new method to handle file upload events
    bindFileUploadEvents(partial, scenarioId) {
        const filePicker = partial.querySelector("#filePicker");
        const addFileButton = partial.querySelector("#addFileButton");
        const fileList = partial.querySelector("#fileList");
        const fileUploadPathsInput = this.getField(partial, "FileUploadPaths");

        if (!filePicker || !addFileButton || !fileList) {
            DevLogger.info("File upload elements not found in partial", scenarioId);
            return;
        }

        // Prevent multiple bindings
        if (addFileButton.dataset.bound === "true") {
            DevLogger.info("File upload events already bound for", scenarioId);
            return;
        }

        addFileButton.dataset.bound = "true";
        DevLogger.info("Binding file upload events for scenario", scenarioId);

        // Store files array for this partial
        if (!partial._selectedFiles) {
            partial._selectedFiles = [];
        }

        // Add File button click handler
        addFileButton.addEventListener("click", () => {
            const file = filePicker.files[0];
            if (!file) {
                DevLogger.warn("No file selected");
                return;
            }

            // Check if file already added
            const existingFile = partial._selectedFiles.find(f => 
                f.name === file.name && f.size === file.size
            );
            
            if (existingFile) {
                DevLogger.warn("File already added", file.name);
                return;
            }

            // Add file to array
            partial._selectedFiles.push(file);
            
            // Update the UI
            this.updateFileList(partial, fileList);
            
            // Update hidden input for form submission
            this.updateFileUploadPaths(partial, fileUploadPathsInput);
            
            // Clear the file picker
            filePicker.value = "";
            
            DevLogger.info("File added", file.name);
        });

        // Optional: Handle file picker change to show selected file name
        filePicker.addEventListener("change", () => {
            const file = filePicker.files[0];
            if (file) {
                DevLogger.info("File selected", file.name);
            }
        });
    },

    // 🆕 Update the file list UI
    updateFileList(partial, fileList) {
        if (!partial._selectedFiles || !fileList) return;

        fileList.innerHTML = "";
        
        partial._selectedFiles.forEach((file, index) => {
            const listItem = document.createElement("li");
            listItem.className = "list-group-item d-flex justify-content-between align-items-center";
            
            listItem.innerHTML = `
                <span>
                    <i class="fas fa-file me-2"></i>
                    ${file.name} <small class="text-muted">(${this.formatFileSize(file.size)})</small>
                </span>
                <button type="button" class="btn btn-sm btn-outline-danger" data-file-index="${index}">
                    <i class="fas fa-times"></i> Remove
                </button>
            `;
            
            // Add remove button handler
            const removeButton = listItem.querySelector("button");
            removeButton.addEventListener("click", () => {
                this.removeFile(partial, index, fileList);
            });
            
            fileList.appendChild(listItem);
        });
    },

    // 🆕 Remove file from list
    removeFile(partial, index, fileList) {
        if (!partial._selectedFiles) return;
        
        const removedFile = partial._selectedFiles.splice(index, 1)[0];
        DevLogger.info("File removed", removedFile.name);
        
        // Update UI and hidden input
        this.updateFileList(partial, fileList);
        const fileUploadPathsInput = this.getField(partial, "FileUploadPaths");
        this.updateFileUploadPaths(partial, fileUploadPathsInput);
    },

    // 🆕 Update hidden input for form submission
    updateFileUploadPaths(partial, fileUploadPathsInput) {
        if (!partial._selectedFiles || !fileUploadPathsInput) return;
        
        // For now, just store file names - you may need to upload files to server first
        const filePaths = partial._selectedFiles.map(file => file.name);
        fileUploadPathsInput.value = JSON.stringify(filePaths);
        
        DevLogger.info("FileUploadPaths updated", filePaths);
    },

    // 🆕 Helper method to format file size
    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
        DevLogger.info("🧠 bindSelectedGroupEvents loaded");

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
                    DevLogger.error("Error loading groups/reviewers:", error);
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
            DevLogger.warn("Reviewer select not found");
            return;
        }

        DevLogger.info("✅ Bound reviewer change event");

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
                DevLogger.warn("Missing required values for reviewer fetch");
                return;
            }

            showLoadingDelayed("Loading Reviewer Details...", 200);

            DevLogger.info("🧪 Fetching with targetGroupId:", targetGroupId);

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
                    DevLogger.error("❌ Error fetching reviewer details:", err);
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
    findSiblingFieldBySwap(partial, baseKey, fromKey, toKey) {
        const baseField = this.getField(partial, fromKey);
        if (!baseField || !baseField.id) return null;

        const siblingId = baseField.id.replace(fromKey, toKey);
        return partial.querySelector(`#${siblingId}`);
    }

};


