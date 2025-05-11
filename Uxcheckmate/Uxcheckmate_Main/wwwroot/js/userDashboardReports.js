let groupedReports = {}; 

document.addEventListener('DOMContentLoaded', () => {
    groupedReports = {};

    reportsByDomain.forEach(report => {
        const url = report.url;
        const domain = url.replace("https://", "").replace("http://", "").split('/')[0];

        if (!groupedReports[domain]) {
            groupedReports[domain] = [];
        }
        groupedReports[domain].push(report);
    });

    const container = document.getElementById('reportListSameDomain');
    
    for (const domain in groupedReports) {
        const folderDiv = document.createElement('div');
        folderDiv.classList.add('folder-card', 'mb-3', 'p-3', 'glass-card');
    
        const safeDomain = domain.replace(/\W/g, '');
        const accordionId = `websiteAccordion-${safeDomain}`;
        const headingId = `heading-${safeDomain}`;
        const collapseId = `collapse-${safeDomain}`;
    
        folderDiv.innerHTML = `
            <div class="accordion" id="${accordionId}">
                <div class="accordion-item">
                    <h2 class="accordion-header" id="${headingId}">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#${collapseId}" aria-expanded="false" aria-controls="${collapseId}">
                            <div class="col-md-8 d-flex justify-content-between align-items-left">
                                ${domain}
                            </div>
                            <div class="col-md-4 d-flex justify-content-center align-items-center">
                                <span class="badge bg-primary">${groupedReports[domain].length} reports</span>
                            </div>
                        </button>
                    </h2>
    
                    <div id="${collapseId}" class="accordion-collapse collapse" aria-labelledby="${headingId}" data-bs-parent="#${accordionId}">
                        <div class="accordion-body">
                            <div class="row fw-bold mb-2">
                                <div class="col-sm">Webpage</div>
                                <div class="col-sm">Report ID</div>
                                <div class="col-sm">Date</div>
                                <div class="col-sm">Actions</div>
                                <div class="col-sm">Export</div>
                                <div class="col-sm">Delete</div>
                            </div>
    
                            ${groupedReports[domain].map(r => `
                                <div class="row mb-2" id="report-${r.id}">
                                    <div class="col-sm text-break">${r.url}</div> 
                                    <div class="col-sm">${r.id}</div>
                                    <div class="col-sm">${formattedDate(r.date)}</div>
                                    <div class="col-sm">
                                        <button class="btn btn-secondary btn-sm" onclick="viewReportDetails(${r.id})">View Report</button>
                                    </div>
                                    <div class="col-sm">
                                        <button class="btn btn-primary btn-sm" onclick="sendToJira(${r.id})">Export to Jira</button>    
                                    </div>
                                    <div class="col-sm">
                                        <button class="btn btn-danger btn-sm deleteReportbtn" onclick="deleteReport(${r.id})">Delete</button>
                                    </div>
                                </div>
                            `).join('')}
                        </div>
                    </div>
    
                </div>
            </div>
        `;
    
        container.appendChild(folderDiv);
    }

    // ✅ FINAL FIX: Resume export after login
    const urlParams = new URLSearchParams(window.location.search);
    const resumeReportId = urlParams.get("resumeReportId");
    if (resumeReportId) {
        sendToJira(parseInt(resumeReportId));
        window.history.replaceState({}, document.title, window.location.pathname);
    }
});

function sendToJira(reportId) {
    fetch('/JiraAPI/IsConnected')
        .then(response => response.json())
        .then(data => {
            if (!data.connected) {
                window.location.href = `/JiraAuth/Authorize?reportId=${reportId}`;
                return;
            }

            const report = findReportById(reportId);
            if (!report) {
                alert("Unable to find report.");
                return;
            }

            const projectKey = prompt("Enter Jira project key to export to (e.g. MYPROJECT):");
            if (!projectKey) {
                alert("Export canceled. No project key provided.");
                return;
            }

            const sendButton = document.querySelector(`#report-${reportId} button.sendToJiraButton`);
            if (sendButton) {
                sendButton.disabled = true;
                sendButton.textContent = "Sending...";
            }

            fetch(`/JiraAPI/ExportReportToJira?reportId=${reportId}&projectKey=${encodeURIComponent(projectKey)}`, {
                method: 'POST'
            })
            .then(response => {
                if (sendButton) {
                    sendButton.disabled = false;
                    sendButton.textContent = "Send to Jira";
                }

                if (response.ok) {
                    alert(`Report ${reportId} successfully exported to Jira project ${projectKey}!`);
                } else {
                    response.text().then(text => {
                        console.error('Failed to send report to Jira:', text);
                        alert(`Failed to send report to Jira. (${response.status})`);
                    });
                }
            })
            .catch(error => {
                if (sendButton) {
                    sendButton.disabled = false;
                    sendButton.textContent = "Send to Jira";
                }
                console.error('Error sending report to Jira:', error);
                alert('An error occurred while sending the report to Jira.');
            });
        });
}

function findReportById(reportId) {
    for (const domain in groupedReports) {
        const report = groupedReports[domain].find(r => r.id === parseInt(reportId));
        if (report) return report;
    }
    return null;
}

// Your existing functions:
function formattedDate(dateStr) {
    const date = new Date(dateStr);
    return date.toLocaleDateString();
}

function deleteReport(reportId) {
    if (!confirm("Are you sure you want to delete this report?")) return;

    fetch(`/Reports/Delete/${reportId}`, {
        method: 'DELETE'
    })
    .then(response => {
        if (response.ok) {
            const reportElement = document.getElementById(`report-${reportId}`);
            if (reportElement) reportElement.remove();
        } else {
            alert("Failed to delete report.");
        }
    })
    .catch(error => console.error("Error deleting report:", error));
}

function viewReportDetails(reportId) {
    window.location.href = `/Reports/Details/${reportId}`;
}
