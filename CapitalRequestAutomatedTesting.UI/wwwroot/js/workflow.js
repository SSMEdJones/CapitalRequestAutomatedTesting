document.addEventListener("DOMContentLoaded", function () {
    document.getElementById('requestIdDropdown').addEventListener('change', function () {
        const id = getSelectText('requestIdDropdown');
        const runButton = document.getElementById('runSelectedButton');
        const scenarioList = document.getElementById('scenarioList');
        const status = document.getElementById('status');
        scenarioList.innerHTML = "";
        status.innerHTML = "";



        if (isValidRequestId(id)) {
            loadWorkflowActions(id);
            runButton.style.display = "inline-block";
            scenarioList.style.display = "block";
            status.style.display = "block";
        } else {
            runButton.style.display = "none";
            scenarioList.style.display = "none";
            status.style.display = "none";
        }
    });
});

//document.getElementById('requestIdDropdown').addEventListener('change', function () {
//    const id = getSelectText('requestIdDropdown');

//    if (isValidRequestId(id)) {
//        loadWorkflowActions(id); // Only load actions if ID is valid
//        runButton.style.display = "inline-block";
//        scenarioList.style.display = "block";
//    } else {
//        runButton.style.display = "none";
//        scenarioList.style.display = "none";
//    }

//});

function isValidRequestId(id) {
    return /^\d+$/.test(id); // Checks if id is composed only of digits
}

async function runSelectedScenarios() {
    const id = getSelectText('requestIdDropdown');

    if (!isValidRequestId(id)) {
        alert('⚠️ Please select a valid Request ID before running scenarios.');
        return;
    }

    const checkboxes = document.querySelectorAll('input[name="scenario"]:checked');
    const statusDiv = document.getElementById('status');

    statusDiv.innerHTML = '';
    statusDiv.style.display = 'block';
    showLoading('Running selected scenarios...');

    const totalStart = performance.now();
    let totalElapsed = 0;

    for (let i = 0; i < checkboxes.length; i++) {
        const checkbox = checkboxes[i];
        const scenario = checkbox.value;

        // Get the label text (assumes checkbox is inside a <label>)
        const labelText = checkbox.parentElement.textContent.trim();

        const selectedRadio = document.querySelector(`input[name="action-${scenario}"]:checked`);
        const selectedAction = selectedRadio ? selectedRadio.value : null;

        if (scenario.toLowerCase().includes("verify") && !selectedAction) {
            alert(`⚠️ Please select an action for: "${labelText}"`);
            hideLoading();
            return;
        }

        // Use labelText in your status messages
        const startTime = performance.now();
        try {
            const debug = `/Workflow/RunScenario?scenario=${scenario}&id=${encodeURIComponent(id)}&selectedAction=${encodeURIComponent(selectedAction || '')}`;

            const response = await fetch(`/Workflow/RunScenario?scenario=${scenario}&id=${encodeURIComponent(id)}&selectedAction=${encodeURIComponent(selectedAction || '')}`);
            const result = await response.json();
            const endTime = performance.now();
            const duration = ((endTime - startTime) / 1000).toFixed(2);
            totalElapsed += parseFloat(duration);

            if (!result.success) {
                statusDiv.innerHTML += `<p style="color:red;">❌ ${labelText}: ${result.message} <em>(${duration}s)</em></p>`;
                break;
            } else {
                statusDiv.innerHTML += `<p style="color:green;">✅ ${labelText}: ${result.message} <em>(${duration}s)</em></p>`;
            }
        } catch (error) {
            const endTime = performance.now();
            const duration = ((endTime - startTime) / 1000).toFixed(2);
            totalElapsed += parseFloat(duration);
            statusDiv.innerHTML += `<p style="color:red;">❌ Error running "${labelText}": ${error.message} <em>(${duration}s)</em></p>`;
            break;
        }

        // ... rest of your loop
    }



    const totalEnd = performance.now();
    const totalDuration = ((totalEnd - totalStart) / 1000).toFixed(2);
    statusDiv.innerHTML += `<p><strong>🧮 Total time: ${totalDuration}s</strong></p>`;

    hideLoading();
}

function runScenario(scenario) {
    return fetch(`/Workflow/RunScenario?scenario=${scenario}`)
        .then(response => response.json())
        .then(data => {
            const statusDiv = document.getElementById('status');
            if (data.success) {
                statusDiv.innerHTML += `<p style="color:green;">${data.message}</p>`;
                return true;
            } else {
                statusDiv.innerHTML += `<p style="color:red;">${data.message}</p>`;
                return false;
            }
        })
        .catch(error => {
            document.getElementById('status').innerHTML += `<p style="color:red;">Error: ${error.message}</p>`;
            return false;
        });
}

window.onload = async function () {
    // Load scenarios
    //const scenarioResponse = await fetch('/Test/GetScenarios');
    //const scenarios = await scenarioResponse.json();
    //const scenarioListDiv = document.getElementById('scenarioList');
    //scenarios.forEach(s => {
    //    const label = document.createElement('label');
    //    label.innerHTML = `<input type="checkbox" name="scenario" value="${s.id}"> ${s.name}`;
    //    scenarioListDiv.appendChild(label);
    //});


    const modal = document.getElementById('loadingModal');
    modal.style.display = 'flex'; // Show modal

    const requestResponse = await fetch('/Workflow/GetRequestIds');
    const requests = await requestResponse.json();
    const dropdown = document.getElementById('requestIdDropdown');
    requests.forEach(r => {
        const option = document.createElement('option');
        option.value = r.id;
        option.text = r.name;
        dropdown.appendChild(option);
    });

    modal.style.display = 'none'; // Hide modal
      

}

function getSelectText(id) {
    const select = document.getElementById(id);
    const selectedOption = select.options[select.selectedIndex];

    return selectedOption.text;;
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

async function loadWorkflowActions(id) {
    try {
        showLoading('Loading available scenarios...');

        const response = await fetch(`Workflow/GetWorkflowDashboardActions?id=${encodeURIComponent(id)}`);
        const data = await response.json();

        const actionsSet = new Set();
        data.forEach(action => {
            actionsSet.add(JSON.stringify(action));
        });

        const actions = Array.from(actionsSet).map(item => JSON.parse(item));
        const container = document.getElementById('scenarioList');
        container.innerHTML = '';

        actions.forEach(item => {
            const wrapper = document.createElement('div');
            wrapper.classList.add('scenario-item');

            const label = document.createElement('label');
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.name = 'scenario';
            checkbox.value = item.scenarioId; // e.g., "1_it_verify"

            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(` ${item.identifier} - ${item.actionName}`));
            wrapper.appendChild(label);

            // Check if the value includes "verify"
            const shouldShowRadio = checkbox.value.toLowerCase().includes("verify");

            const radioContainer = document.createElement('div');
            radioContainer.classList.add('radio-options');
            radioContainer.style.display = 'none';

            if (shouldShowRadio) {
                ['Verify', 'Request More Info'].forEach(actionType => {
                    const radioLabel = document.createElement('label');
                    const radio = document.createElement('input');
                    radio.type = 'radio';
                    radio.name = `action-${item.scenarioId}`;
                    radio.value = actionType;
                    radioLabel.appendChild(radio);
                    radioLabel.appendChild(document.createTextNode(` ${actionType}`));
                    radioContainer.appendChild(radioLabel);
                    radioContainer.appendChild(document.createElement('br'));
                });
            }

            wrapper.appendChild(radioContainer);
            container.appendChild(wrapper);

            checkbox.addEventListener('change', function () {
                radioContainer.style.display = (this.checked && shouldShowRadio) ? 'block' : 'none';
            });
        });


    } catch (error) {
        console.error('Error loading workflow actions:', error);
        alert('Failed to load workflow actions. Please try again.');
    } finally {
        hideLoading();
    }
}


async function loadScenarios() {
    const response = await fetch('/Workflow/GetAvailableScenarios');
    const scenarios = await response.json();

    const dropdown = document.getElementById('scenarioDropdown');
    dropdown.innerHTML = '';

    scenarios.forEach(s => {
        const option = document.createElement('option');
        option.value = s.Id;
        option.textContent = s.Name;
        dropdown.appendChild(option);
    });
}



