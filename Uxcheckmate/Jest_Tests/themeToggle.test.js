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
        <select id="theme-selector">
            <option value="light">Light Mode</option>
            <option value="dark">Dark Mode</option>
        </select>
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
