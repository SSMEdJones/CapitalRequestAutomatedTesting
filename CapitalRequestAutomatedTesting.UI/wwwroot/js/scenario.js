
window.onload = async function () {
    const modal = document.getElementById('loadingModal');
    modal.style.display = 'flex'; // Show modal

    // Load request IDs
    const requestResponse = await fetch('/Workflow/GetRequestIds');
    const requests = await requestResponse.json();
    const dropdown = document.getElementById('requestIdDropdown');
    requests.forEach(r => {
        const option = document.createElement('option');
        option.value = r.id;
        option.text = r.name;
        dropdown.appendChild(option);
    });

    // Load scenarios
    const scenarioResponse = await fetch('/Scenario/Scenarios');
    const scenarios = await scenarioResponse.json();
    const scenarioListDiv = document.getElementById('scenarioList');
    scenarios.forEach(s => {
        const label = document.createElement('label');
        label.innerHTML = `<input type="checkbox" name="selectedScenarioIds" value="${s.id}"> ${s.name}`;
        scenarioListDiv.appendChild(label);
    });

    modal.style.display = 'none'; // Hide modal
    scenarioListDiv.style.display = 'block';
    document.getElementById('runSelectedButton').style.display = 'block';
}
