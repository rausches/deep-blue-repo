const { validateURL, handleUserUrl } = require('../Uxcheckmate_Main/wwwroot/js/handleUserUrl.js');


test('Should return true for valid URL', () => {
    expect(validateURL('https://uxcheckmate.com')).toBeTruthy();
});

test('Should return false for empty URL', () => {   
    expect(validateURL('')).toBeFalsy();
});

test('Should return false for invalid URL', () => {
    expect(validateURL('http://example[dot]com')).toBeFalsy();
    expect(validateURL('http:/example.com')).toBeFalsy();
    expect(validateURL('http://example')).toBeFalsy();
    expect(validateURL('http://example .com')).toBeFalsy();
});

test('Should return false for missing protocol (http or https)', () => {
    expect(validateURL('example.com')).toBeFalsy(); 
});


// This test checks if the loader is displayed and then the scanning screen is shown after 3 seconds
// The test uses jest.advanceTimersByTime to fast-forward the 3-second timeout
test('Should display loader and then scanning screen', () => {
    jest.useFakeTimers(); // Mock time-based functions like setTimeout 

    // Set up the HTML structure including the urlInput, loaderWrapper, scanningWrapper, and customPopupMessage
    document.body.innerHTML = `
        <input id="urlInput" type="text" value="https://example.com">
        <div id="loaderWrapper" style="display: none;"></div>
        <div id="scanningWrapper" style="display: none;"></div>
        <div id="responseMessage"></div>
        <div id="customPopupMessage" style="display: none;"></div> <!-- Add this element for the test -->
    `;

    handleUserUrl(new Event('submit'));

    // Check if loader is shown
    expect(document.getElementById('loaderWrapper').style.display).toBe('flex');

    // Simulate the timeout for the scanning screen
    jest.advanceTimersByTime(3000); // Fast-forward the 3-second timeout

    // Check if scanning screen is shown
    expect(document.getElementById('scanningWrapper').style.display).toBe('block');
});


