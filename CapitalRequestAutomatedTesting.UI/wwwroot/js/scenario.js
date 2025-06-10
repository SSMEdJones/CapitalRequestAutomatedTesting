window.onload = async function () {
    const modal = document.getElementById('loadingModal');
    modal.style.display = 'flex';

    // Load request IDs
    const requestResponse = await fetch('/Scenario/GetRequestIds');
    const requests = await requestResponse.json();
    const dropdown = document.getElementById('requestIdDropdown');
    requests.forEach(r => {
        const option = document.createElement('option');
        option.value = r.id;
        option.text = r.name;
        dropdown.appendChild(option);
    });

    modal.style.display = 'none';
};

// Show scenario list after request is selected
document.getElementById('requestIdDropdown').addEventListener('change', function () {
    const requestId = this.value;
    const scenarioListDiv = document.getElementById('scenarioList');
    const runButton = document.getElementById('runSelectedButton');

    if (requestId) {
        scenarioListDiv.style.display = 'block';
        runButton.style.display = 'block';

        // Load partials for each scenario
        document.querySelectorAll('.scenario-wrapper').forEach(wrapper => {
            const scenarioId = wrapper.querySelector('input[type="checkbox"]').value;
            const targetDiv = wrapper.querySelector('.scenario-partial');

            fetch(`/Scenario/LoadScenarioPartial?scenarioId=${scenarioId}&requestId=${requestId}`)
                .then(response => response.text())
                .then(html => {
                    targetDiv.innerHTML = html;
                })
                .catch(error => {
                    console.error('Error loading partial view:', error);
                });
        });

    } else {
        scenarioListDiv.style.display = 'none';
        runButton.style.display = 'none';
    }
});

//document.getElementById('requestIdDropdown').addEventListener('change', function () {
//    const scenarioListDiv = document.getElementById('scenarioList');
//    const runButton = document.getElementById('runSelectedButton');

//    if (this.value) {
//        scenarioListDiv.style.display = 'block';
//        runButton.style.display = 'block';
//    } else {
//        scenarioListDiv.style.display = 'none';
//        runButton.style.display = 'none';
//    }
//});

// Toggle partial views when checkboxes are clicked
document.addEventListener('change', function (e) {
    if (e.target.name === 'selectedScenarioIds') {
        const targetId = e.target.getAttribute('data-target');
        const partial = document.getElementById(targetId);
        if (partial) {
            partial.style.display = e.target.checked ? 'block' : 'none';
        }
    }
});

async function onRequestingGroupChange(selectElement) {
    const requestingGroupId = selectElement.value;

    // Find the closest scenario-partial container
    const container = selectElement.closest('.scenario-partial');

    // Get the selected proposalId (RequestId) from the main dropdown
    const proposalId = document.getElementById('requestIdDropdown').value;

    // Find the target and reviewer dropdowns within the same partial
    const targetGroupDropdown = container.querySelector('[name$=".TargetGroupId"]');
    const reviewerDropdown = container.querySelector('[name$=".ReviewerId"]');
    try {
    const response = await fetch(`/Scenario/LoadScenarioPartial?scenarioId=${scenarioId}&requestId=${requestId}`)
        .then(response => response.text())
        .then(html => {
            targetDiv.innerHTML = html;

            // Now that the partial is loaded, query the elements
            const container = targetDiv.closest('.scenario-wrapper');
            const targetGroupDropdown = container.querySelector('[name$=".TargetGroupId"]');
            const reviewerDropdown = container.querySelector('[name$=".ReviewerId"]');

            console.log('Target Group:', targetGroupDropdown);
            console.log('Reviewer:', reviewerDropdown);
        });
        if (!response.ok) throw new Error('Network response was not ok');
        const data = await response.json();

        // Clear and populate Target Groups
        targetGroupDropdown.innerHTML = '';
        data.targetGroups.forEach(item => {
            const option = document.createElement('option');
            option.value = item.value;
            option.text = item.text;
            targetGroupDropdown.appendChild(option);
        });

        // Clear and populate Reviewers
        reviewerDropdown.innerHTML = '';
        data.reviewers.forEach(item => {
            const option = document.createElement('option');
            option.value = item.value;
            option.text = item.text;
            reviewerDropdown.appendChild(option);
        });
    } catch (error) {
        console.error('Error fetching filtered data:', error);
    }
}

    //try {
    //    const response = await fetch(`/Scenario/GetTargetGroupsAndReviewers?proposalId=${proposalId}&requestingGroupId=${requestingGroupId}`);
    //    if (!response.ok) throw new Error('Network response was not ok');

    //    const data = await response.json();

    //    // Clear and populate Target Groups
    //    targetGroupDropdown.innerHTML = '';
    //    data.targetGroups.forEach(item => {
    //        const option = document.createElement('option');
    //        option.value = item.value;
    //        option.text = item.text;
    //        targetGroupDropdown.appendChild(option);
    //    });

    //    // Clear and populate Reviewers
    //    reviewerDropdown.innerHTML = '';
    //    data.reviewers.forEach(item => {
    //        const option = document.createElement('option');
    //        option.value = item.value;
    //        option.text = item.text;
    //        reviewerDropdown.appendChild(option);
    //    });

    //} catch (error) {
    //    console.error('Error fetching filtered data:', error);
    //}



