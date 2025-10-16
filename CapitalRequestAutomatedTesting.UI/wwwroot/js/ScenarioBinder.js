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
        if (!partial || !proposalId || partial._scenarioBound) {
            if (partial._scenarioBound) {
                DevLogger.info("Scenario partial already bound - skipping", partial.id);
            }
            return;
        }

        partial._scenarioBound = true;

        if (!partial.children.length) {
            DevLogger.info("Empty partial - waiting for content", partial.id);
            partial._scenarioBound = false;
            return;
        }

        const scenarioId = partial.id?.replace("partial-", "") || "Unknown";
        DevLogger.info(`Binding Partial for Scenario ${scenarioId}`);

        const replyingGroupSelect = this.getField(partial, "replyingGroupId");

        if (replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "replying");

            // 🔥 SIMPLE: One auto-selection check
            if (replyingGroupSelect.options.length === 2 && replyingGroupSelect.value === "") {
                DevLogger.info("Auto-selecting single replying group", replyingGroupSelect.options[1].text);
                replyingGroupSelect.selectedIndex = 1;
                replyingGroupSelect.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }

        const requestingGroupSelect = this.getField(partial, "requestingGroupId");
        if (requestingGroupSelect && !replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "requesting");
        }

        this.bindFileUploadEvents(partial, scenarioId);
    },
    //bindScenarioPartial(partial, proposalId) {
    //    if (!partial || !proposalId) {
    //        DevLogger.warn("Cannot bind scenario partial", {
    //            partialExists: !!partial,
    //            proposalId
    //        });
    //        return;
    //    }

    //    // 🔥 FIX: Set bound flag IMMEDIATELY to prevent any race conditions
    //    if (partial._scenarioBound) {
    //        DevLogger.info("Scenario partial already bound - skipping", partial.id);
    //        return;
    //    }
    //    partial._scenarioBound = true; // Set this FIRST thing

    //    // Check if partial is empty - if so, wait for content
    //    if (!partial.children.length) {
    //        DevLogger.info("Empty partial - waiting for content", partial.id);
    //        partial._scenarioBound = false; // Reset if empty
    //        return;
    //    }

    //    const scenarioId = partial.id?.replace("partial-", "") || "Unknown";
    //    DevLogger.group(`Binding Partial for Scenario ${scenarioId}`);

    //    // Continue with existing binding logic...
    //    const requestingGroupSelect = this.getField(partial, "requestingGroupId");
    //    const replyingGroupSelect = this.getField(partial, "replyingGroupId");

    //    // Bind appropriate events based on scenario type
    //    if (requestingGroupSelect) {
    //        const replyingGroupField = this.getField(partial, "replyingGroupId");
    //        const isReplyingScenario = !!replyingGroupField;

    //        if (isReplyingScenario) {
    //            DevLogger.info("Skipping binding for requesting group in replying scenario", partial.id);
    //        } else {
    //            this.bindGroupChange(partial, proposalId, "requesting");
    //        }
    //    }

    //    if (replyingGroupSelect) {
    //        this.bindGroupChange(partial, proposalId, "replying");
            
    //        // 🔥 SINGLE auto-selection timeout
    //        setTimeout(() => {
    //            DevLogger.info("Checking replying group for auto-selection", {
    //                optionsLength: replyingGroupSelect.options.length,
    //                currentValue: replyingGroupSelect.value
    //            });

    //            // Only auto-select if there are exactly 2 options and nothing selected
    //            if (replyingGroupSelect.options.length === 2 && replyingGroupSelect.value === "") {
    //                partial.style.visibility = 'hidden';
    //                DevLogger.info("Auto-selecting single replying group", replyingGroupSelect.options[1].text);
    //                replyingGroupSelect.selectedIndex = 1;
    //                replyingGroupSelect.dispatchEvent(new Event('change', { bubbles: true }));
                    
    //                setTimeout(() => {
    //                    partial.style.visibility = 'visible';
    //                }, 800);
    //            } else {
    //                DevLogger.info("Skipping auto-selection - already selected or multiple options");
    //            }
    //        }, 50);
    //    }

    //    this.bindFileUploadEvents(partial, scenarioId);
    //    DevLogger.groupEnd();
    //},

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
                    // 🔥 FIX: Only look for targetGroupId in Request scenarios
                    const targetGroupSelect = groupType !== "replying" ? this.getField(partial, "targetGroupId") : null;
                    const reviewerSelect = this.getField(partial, "reviewerId");

                    if (groupType === "replying") {
                        const select = this.getField(partial, "RequestingGroupId");

                        if (data.targetGroups?.length > 1) {
                            populateDropdown(select, data.targetGroups, "-- Select One --");
                            select.value = "";
                        } else {
                            populateDropdown(select, data.targetGroups, "-- Select One --");
                            // ✅ Auto-select the only available group
                            if (data.targetGroups.length === 1) {
                                select.selectedIndex = 1;
                                DevLogger.info("Single requesting group - auto-selected", data.targetGroups[0].text);
                            }
                        }
                    }

                    // 🔥 FIX: Only populate targetGroupSelect for Request scenarios
                    if (targetGroupSelect && groupType === "requesting") {
                        populateDropdown(targetGroupSelect, data.targetGroups, "--Select One--");
                    }

                    if (reviewerSelect) {
                        populateDropdown(reviewerSelect, data.reviewers, "--Select Reviewer--");
                        this.bindReviewerChange(partial, proposalId, groupType);
                        DevLogger.info("Reviewer change event bound for groupType", groupType);
                    }
                });
        });

        // 🔥 MOVE AUTO-SELECTION TO AFTER BINDING BUT USE A DIFFERENT APPROACH
        // Check for auto-selection after the DOM is stable and the field has options
        if (groupType === "replying") {
            // Check for auto-selection after a minimal delay to ensure DOM is ready
            setTimeout(() => {
                DevLogger.info("Checking replying group for auto-selection", {
                    optionsLength: groupSelect.options.length,
                    currentValue: groupSelect.value
                });

                // Only auto-select if there are exactly 2 options (default + one option) and nothing selected
                if (groupSelect.options.length === 2 && groupSelect.value === "") {
                    // Hide the partial temporarily to prevent flashing
                    partial.style.visibility = 'hidden';

                    DevLogger.info("Auto-selecting single replying group", groupSelect.options[1].text);
                    groupSelect.selectedIndex = 1;
                    groupSelect.dispatchEvent(new Event('change', { bubbles: true }));

                    // Show it again after processing
                    setTimeout(() => {
                        partial.style.visibility = 'visible';
                    }, 800);
                }
            }, 50); // Minimal delay
        }
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
            if (!requestingGroupId ||!proposalId) return;
            DevLogger.info("requestingGroupId change");
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
    },

    getField(container, key) {
        const normalizedKey = key.toLowerCase();
        DevLogger.info(`🔍 Looking for field with key: "${key}" (normalized: "${normalizedKey}")`);

        const allElements = Array.from(container.querySelectorAll("select, input, textarea"));
        DevLogger.info(`📊 Found ${allElements.length} potential fields:`);

        allElements.forEach(el => {
            DevLogger.info(`➡️ ID: ${el.id} | NAME: ${el.name}`);
        });

        const directMatch = allElements.find(el =>
            el.id && el.id.toLowerCase().endsWith(normalizedKey)
        );
        if (directMatch) {
            DevLogger.info("✅ Direct match found by ID suffix:", directMatch.id);
            return directMatch;
        }

        const aspMatch = allElements.find(el =>
            el.id && el.id.toLowerCase().includes(`__${normalizedKey}`)
        );
        if (aspMatch) {
            DevLogger.info("✅ Match found by ASP.NET-style ID pattern:", aspMatch.id);
            return aspMatch;
        }

        const nameMatch = allElements.find(el =>
            el.name && el.name.toLowerCase().endsWith(`.${normalizedKey}`)
        );
        if (nameMatch) {
            DevLogger.info("✅ Match found by name attribute:", nameMatch.name);
            return nameMatch;
        }

        // 🔥 ADD MORE DEBUG - Let's see what's actually being compared
        DevLogger.warn("❌ No match found for key:", key);
        DevLogger.info("🐛 DEBUG: Detailed matching attempts:");
        allElements.forEach(el => {
            if (el.id) {
                const idLower = el.id.toLowerCase();
                DevLogger.info(`🔍 Testing ID: "${el.id}" (lowercase: "${idLower}")`);
                DevLogger.info(`   - endsWith("${normalizedKey}"): ${idLower.endsWith(normalizedKey)}`);
                DevLogger.info(`   - includes("__${normalizedKey}"): ${idLower.includes(`__${normalizedKey}`)}`);
            }
        });
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
    bindScenarioPartial(partial, proposalId) {
        if (!partial || !proposalId || partial._scenarioBound) {
            if (partial._scenarioBound) {
                DevLogger.info("Scenario partial already bound - skipping", partial.id);
            }
            return;
        }

        partial._scenarioBound = true;

        if (!partial.children.length) {
            DevLogger.info("Empty partial - waiting for content", partial.id);
            partial._scenarioBound = false;
            return;
        }

        const scenarioId = partial.id?.replace("partial-", "") || "Unknown";
        DevLogger.info(`Binding Partial for Scenario ${scenarioId}`);

        const replyingGroupSelect = this.getField(partial, "replyingGroupId");

        if (replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "replying");

            // 🔥 SIMPLE: One auto-selection check
            if (replyingGroupSelect.options.length === 2 && replyingGroupSelect.value === "") {
                DevLogger.info("Auto-selecting single replying group", replyingGroupSelect.options[1].text);
                replyingGroupSelect.selectedIndex = 1;
                replyingGroupSelect.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }

        const requestingGroupSelect = this.getField(partial, "requestingGroupId");
        if (requestingGroupSelect && !replyingGroupSelect) {
            this.bindGroupChange(partial, proposalId, "requesting");
        }

        this.bindFileUploadEvents(partial, scenarioId);
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

                    // Auto-select requesting group only if not already selected
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
};

