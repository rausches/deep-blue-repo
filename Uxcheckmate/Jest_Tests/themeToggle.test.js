/**
 * @jest-environment jsdom
 */
const { setTheme, saveTheme, loadTheme } = require("../Uxcheckmate_Main/wwwroot/js/themeSwitcher.js");

beforeEach(() => {
    jest.restoreAllMocks(); // Reset mocks between tests

    // Mock localStorage
    jest.spyOn(Storage.prototype, "getItem").mockImplementation((key) => {
        return key === "theme" ? "dark" : null;
    });
    jest.spyOn(Storage.prototype, "setItem").mockImplementation(() => {});

    // Mock classList behavior using a Set
    let appliedClasses = new Set();

    Object.defineProperty(document.documentElement, "classList", {
        value: {
            add: jest.fn((cls) => appliedClasses.add(cls)),
            remove: jest.fn((cls) => appliedClasses.delete(cls)),
            contains: jest.fn((cls) => appliedClasses.has(cls)),
        },
        configurable: true,
    });

    document.body.innerHTML = `
        <select id="theme-selector"></select>
        <select id="theme-selector-lg"></select>

    `;
});

test("Sarah selects dark mode, and the interface changes to dark", () => {
    setTheme("dark");

    // Expect "dark-theme" to be added
    expect(document.documentElement.classList.contains("dark-theme")).toBe(true);
    // Expect "light-theme" to be removed
    expect(document.documentElement.classList.contains("light-theme")).toBe(false);

    // Ensure theme is saved to localStorage
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "dark");
});

test("David selects light mode, and the interface changes to light", () => {
    setTheme("light");

    // Expect "dark-theme" to be removed
    expect(document.documentElement.classList.contains("dark-theme")).toBe(false);
    // Expect "light-theme" to be added
    expect(document.documentElement.classList.contains("light-theme")).toBe(true);

    // Ensure theme is saved to localStorage
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "light");
});

test("Priya navigates across pages, and the theme persists", () => {
    jest.spyOn(Storage.prototype, "getItem").mockReturnValue("dark");

    // Call loadTheme to simulate page reload
    loadTheme();

    // Verify the theme is restored
    expect(document.documentElement.classList.contains("dark-theme")).toBe(true);
});

test("David selects royal mode", () => {
    setTheme("royal");
    expect(document.documentElement.classList.contains("light-theme")).toBe(false);
    expect(document.documentElement.classList.contains("royal-theme")).toBe(true);
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "royal");
});

test("Priya selects neon theme", () => {
    setTheme("neon");
    // removes royal-theme
    expect(document.documentElement.classList.contains("royal-theme")).toBe(false);
    expect(document.documentElement.classList.contains("neon-theme")).toBe(true);
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "neon");
});

test("Sarah selects pastel theme", () => {
    setTheme("pastel");
    expect(document.documentElement.classList.contains("neon-theme")).toBe(false);
    expect(document.documentElement.classList.contains("pastel-theme")).toBe(true);
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "pastel");
});

test("Priya selects high contrast theme", () => {
    setTheme("contrast");
    expect(document.documentElement.classList.contains("pastel-theme")).toBe(false);
    expect(document.documentElement.classList.contains("contrast-theme")).toBe(true);
    expect(localStorage.setItem).toHaveBeenCalledWith("theme", "contrast");
});

test("Priya navigates across pages, and the theme contrast persists", () => {
    jest.spyOn(Storage.prototype, "getItem").mockReturnValue("contrast");

    // Call loadTheme to simulate page reload
    loadTheme();

    // Verify the theme is restored
    expect(document.documentElement.classList.contains("contrast-theme")).toBe(true);
});