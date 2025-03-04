const { validateURL } = require("../Uxcheckmate_Main/wwwroot/js/handleUserUrl.js");


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