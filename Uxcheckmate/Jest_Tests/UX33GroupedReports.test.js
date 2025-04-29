const { deleteReport, viewReportDetails, findReportById } = require('../Uxcheckmate_Main/wwwroot/js/userDashboardReports');


describe('userDashboardReports.js', () => {
    let groupedReports;

    beforeEach(() => {
        // Mock groupedReports
        groupedReports = {
            "example.com": [
                { id: 1, url: "https://example.com", date: "2025-04-26T00:00:00Z", designIssues: [], accessibilityIssues: [] },
                { id: 2, url: "https://example.com/page", date: "2025-04-27T00:00:00Z", designIssues: [], accessibilityIssues: [] }
            ]
        };

        // Mock DOM elements
        document.body.innerHTML = `
            <div id="report-1"></div>
            <div id="report-2"></div>
            <div id="websiteAccordion-examplecom" class="badge">2</div>
        `;
    });

    afterEach(() => {
        jest.clearAllMocks();
    });
    test('deleteReport removes a report from DOM and updates groupedReports', async () => {
        global.fetch = jest.fn(() =>
            Promise.resolve({ ok: true })
        );

        await deleteReport(1);

        expect(document.getElementById('report-1')).toBeNull(); 
        expect(groupedReports["example.com"].length).toBe(1); 
        expect(groupedReports["example.com"][0].id).toBe(2); 
    });
    
    test('findReportById finds a report by its ID', () => {
        const report = findReportById(1);
        expect(report).toEqual(groupedReports["example.com"][0]);

        const nonExistentReport = findReportById(999);
        expect(nonExistentReport).toBeNull(); 
    });
});
