document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const sidebarToggle = document.getElementById("sidebarToggle");
    const mainContent = document.querySelector(".main-content");
    const header = document.querySelector(".header");

    sidebarToggle.addEventListener("click", function () {
        sidebar.classList.toggle("closed");
        if (sidebar.classList.contains("closed")) {
            sidebar.style.transform = "translateX(-100%)";
            mainContent.style.marginLeft = "0";
            header.style.marginLeft = "0";
        } else {
            sidebar.style.transform = "translateX(0)";
            mainContent.style.marginLeft = "250px";
            header.style.marginLeft = "250px";
        }
    });
});
