document.addEventListener("DOMContentLoaded", function () {
    const buttons = document.querySelectorAll("[data-bs-toggle='collapse']");

    buttons.forEach(button => {
        const targetId = button.getAttribute("data-bs-target");
        const target = document.querySelector(targetId);

        target.addEventListener("show.bs.collapse", () => {
            button.textContent = "−";
        });

        target.addEventListener("hide.bs.collapse", () => {
            button.textContent = "+";
        });
    });
});