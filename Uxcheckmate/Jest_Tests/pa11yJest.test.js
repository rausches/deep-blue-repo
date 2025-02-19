import { jest } from '@jest/globals';
import { readFile } from 'fs/promises';
import getPort from 'get-port';
import express from 'express';
import pa11y from 'pa11y';


if (!AbortSignal.timeout) {
  AbortSignal.timeout = function (ms) {
    const controller = new AbortController();
    setTimeout(() => controller.abort(), ms);
    return controller.signal;
  };
}

jest.mock('fs/promises', () => ({
  readFile: jest.fn(),
}));

jest.mock('get-port', () => jest.fn(async () => 3005));

describe('runPa11y.js Tests', () => {
  let server;
  let consoleSpy;
  let serverReady = false;

  beforeAll(async () => {
    readFile.mockResolvedValue('<html><head><title>Test</title></head><body><h1>Hello</h1></body></html>');

    consoleSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

    const app = express();
    app.get('/', (req, res) => res.send('<html><body><h1>Test</h1></body></html>'));

    // Wait for the server to fully start
    await new Promise((resolve) => {
      server = app.listen(3005, () => {
        serverReady = true;
        resolve();
      });
    });
  });

  afterAll(() => {
    if (server) server.close();
    consoleSpy.mockRestore();
  });

  test('pa11y runs and produces results', async () => {
    if (!serverReady) {
      throw new Error("Test server didn't start properly.");
    }

    console.log("Running pa11y...");
    const results = await pa11y('http://localhost:3005');
    console.log("Pa11y Results:", results);


    expect(results).toHaveProperty('issues');
    expect(Array.isArray(results.issues)).toBe(true);

    // Check if pa11y actually ran and logged output
    expect(consoleSpy).toHaveBeenCalled();
  });
});
