document.getElementById('requestIdDropdown').addEventListener('change', function () {
    const id = getSelectText('requestIdDropdown');
    if (isValidRequestId(id)) {
        loadWorkflowActions(id); // Only load actions if ID is valid
    }
});

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
        const scenario = checkboxes[i].value;
        const startTime = performance.now();

        try {
            const response = await fetch(`/Workflow/RunScenario?scenario=${scenario}&id=${encodeURIComponent(id)}`);
            const result = await response.json();
            const endTime = performance.now();
            const duration = ((endTime - startTime) / 1000).toFixed(2);
            totalElapsed += parseFloat(duration);

            if (!result.success) {
                statusDiv.innerHTML += `<p style="color:red;">❌ ${result.message} <em>(${duration}s)</em></p>`;
                break;
            } else {
                statusDiv.innerHTML += `<p style="color:green;">✅ ${result.message} <em>(${duration}s)</em></p>`;
            }
        } catch (error) {
            const endTime = performance.now();
            const duration = ((endTime - startTime) / 1000).toFixed(2);
            totalElapsed += parseFloat(duration);
            statusDiv.innerHTML += `<p style="color:red;">❌ Error running "${scenario}": ${error.message} <em>(${duration}s)</em></p>`;
            break;
        }

        const avgTime = totalElapsed / (i + 1);
        const remaining = checkboxes.length - (i + 1);
        const estimatedRemaining = (avgTime * remaining).toFixed(2);
        statusDiv.innerHTML += `<p style="color:gray;"><em>⏳ Estimated time remaining: ${estimatedRemaining}s</em></p>`;
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
        const data = await response.json(); // Parse as JSON

        // Extract unique actions from the JSON structure
        const actionsSet = new Set();
        data.forEach(action => {
            actionsSet.add(JSON.stringify(action)); // Use JSON.stringify to ensure uniqueness
        });

        const actions = Array.from(actionsSet).map(item => JSON.parse(item));

        const container = document.getElementById('scenarioList');
        container.innerHTML = ''; // Clear existing

        actions.forEach(item => {
            const label = document.createElement('label');
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.name = 'scenario';
            checkbox.value = item.scenarioId;
            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(` ${item.identifier} - ${item.actionName}`));
            container.appendChild(label);
            container.appendChild(document.createElement('br'));
        });


    } catch (error) {
        console.error('Error loading workflow actions:', error);
        alert('Failed to load workflow actions. Please try again.');
    } finally {
        hideLoading()
    }
    
}


