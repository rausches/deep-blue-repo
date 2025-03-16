const reportResults = require('../Uxcheckmate_Main/wwwroot/js/reportResults');

describe('Sorting Behavior in reportResults.js', () => {
  beforeAll(() => {
    // Provide a dummy bootstrap object to prevent errors in initBootstrapCollapses.
    global.bootstrap = {
      Collapse: class {
        constructor(element, options) {
          this.element = element;
          this.options = options;
        }
        hide() {}
        show() {}
        static getInstance() {
          return null;
        }
      }
    };
  });

  beforeEach(() => {
    // Set up a minimal DOM for the test.
    document.body.innerHTML = `
      <select id="sortSelect" data-report-id="123">
        <option value="category">Category</option>
        <option value="severity-high-low">Severity (High to Low)</option>
        <option value="severity-low-high">Severity (Low to High)</option>
      </select>
      <div id="designIssuesContainer"></div>
      <div id="accessibilityIssuesContainer"></div>
    `;

    // Reset fetch mock
    global.fetch = jest.fn();
    // Clear the require cache to ensure the script is re-executed for each test.
    jest.resetModules();
  });

  it('should fetch sorted issues and update the DOM on sort selection change', async () => {
    // Create a fake response for the fetch call.
    const mockResponse = {
      designHtml: '<div>Updated Design Issues</div>',
      accessibilityHtml: '<div>Updated Accessibility Issues</div>',
    };

    // Set up the fetch mock to resolve with the fake response.
    global.fetch.mockResolvedValue({
      json: () => Promise.resolve(mockResponse)
    });

    // Dispatch the DOMContentLoaded event to trigger the event listener registration.
    document.dispatchEvent(new Event('DOMContentLoaded'));

    // Get the sort select element and simulate a change event.
    const sortSelect = document.getElementById('sortSelect');
    sortSelect.value = 'severity-high-low';
    sortSelect.dispatchEvent(new Event('change'));

    // Wait for the promise chain (fetch and then DOM updates) to resolve.
    await new Promise(process.nextTick);

    // Verify that fetch was called with the expected URL.
    expect(global.fetch).toHaveBeenCalledWith('/Home/GetSortedIssues?id=123&sortOrder=severity-high-low');

    // Check that the containers are updated with the returned HTML.
    expect(document.getElementById('designIssuesContainer').innerHTML).toBe('<div>Updated Design Issues</div>');
    expect(document.getElementById('accessibilityIssuesContainer').innerHTML).toBe('<div>Updated Accessibility Issues</div>');
  });
});
