document.addEventListener("DOMContentLoaded", function () {
    const themeSelectors = document.querySelectorAll("#theme-selector, #theme-selector-lg"); // Select both dropdowns
    const body = document.documentElement; // Use documentElement for immediate styling

    // Load the saved theme
    loadTheme();

    themeSelectors.forEach(selector => {
        selector.value = localStorage.getItem("theme") || "light";

        selector.addEventListener("change", function () {
            setTheme(this.value);
        });
    });
});

function saveTheme(theme) {
    localStorage.setItem("theme", theme);
}

function setTheme(theme) {
    const body = document.documentElement; // Use documentElement for immediate styling

    // Remove existing theme classes
    body.classList.remove("light-theme", "dark-theme");

    // Apply new theme
    body.classList.add(theme + "-theme");

    // Save theme selection
    saveTheme(theme);

    // Sync desktop and mobile menus
    document.querySelectorAll("#theme-selector, #theme-selector-lg").forEach(selector => {
        selector.value = theme;
    });
}

function loadTheme() {
    const savedTheme = localStorage.getItem("theme") || "light";
    setTheme(savedTheme); // Calls `setTheme()` to apply styles properly
}

if (typeof module !== 'undefined') {
    module.exports = { setTheme, saveTheme, loadTheme };
}
