const themeSelectors = document.querySelectorAll('[data-theme]');

themeSelectors.forEach(btn => {
    btn.addEventListener('click', () => {
        const theme = btn.getAttribute('data-theme'); // Get the theme from the button's data attribute
        // document.documentElement.className = ""; // Clear existing classes
        // document.documentElement.classList.add(`theme-${theme}`); // Add the new theme class
        setTheme(theme); // Call the setTheme function to apply the new theme
        // localStorage.setItem("theme", theme); // Save the selected theme to localStorage
    });
});

function saveTheme(theme) {
    localStorage.setItem("theme", theme);
}

function setTheme(theme) {
    const body = document.documentElement; // Use documentElement for immediate styling

    // Remove existing theme classes
    body.classList.remove(
        "light-theme",
        "dark-theme",
        "royal-theme",
        "neon-theme",
        "pastel-theme",
        "contrast-theme");

    // Apply new theme
    body.classList.add(`${theme}-theme`);

    // Save theme selection
    saveTheme(theme);

    // Sync desktop and mobile menus
    document.querySelectorAll("#theme-selector, #theme-selector-lg").forEach(selector => {
        if (selector) selector.value = theme;
    });
}

function loadTheme() {
    const savedTheme = localStorage.getItem("theme") || "light";
    setTheme(savedTheme); // Calls `setTheme()` to apply styles properly
}

if (typeof module !== 'undefined') {
    module.exports = { setTheme, saveTheme, loadTheme };
}
