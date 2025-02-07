// Testing client-side JavaScript validation code with Jest

module.exports = {
    testEnvironment: 'node',
    roots: ['<rootDir>/Uxcheckmate/Jest_Tests'],
    testMatch: ['**/*.test.js'],
    transform: {
        '^.+\\.js$': 'babel-jest',
    },
};

const { validateURL } = require('~Uxcheckmate/Uxcheckmate_Main/wwwroot/js/handleUserUrl.js');

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