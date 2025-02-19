export default {
    testEnvironment: 'jsdom',
    roots: ['<rootDir>'],
    testMatch: ['**/*.test.js'],
    transform: {
      '^.+\\.js$': 'babel-jest',
    },
  };
  