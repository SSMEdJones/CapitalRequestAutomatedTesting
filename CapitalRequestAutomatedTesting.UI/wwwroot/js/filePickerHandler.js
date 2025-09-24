function setupFilePicker(filePickerId, fileListId, addButtonId) {
    const fileList = [];
    const filePicker = document.getElementById(filePickerId);
    const fileListElement = document.getElementById(fileListId);

    document.getElementById(addButtonId)?.addEventListener('click', () => {
        const file = filePicker.files[0];
        if (!file) return;

        fileList.push(file);

        const li = document.createElement('li');
        li.className = 'list-group-item';
        li.textContent = file.name;
        fileListElement.appendChild(li);

        filePicker.value = '';
    });
}
