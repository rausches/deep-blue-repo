// Initiates the export of a report to Jira.
function sendToJira(reportId) {
    fetch('/JiraAPI/IsConnected')
        .then(response => response.json())
        .then(data => {
            if (!data.connected) {
                // If not connected, start OAuth authorization flow
                window.location.href = `/JiraAuth/Authorize?reportId=${reportId}`;
                return;
            }

            // If connected, fetch available Jira projects
            fetch('/JiraAPI/GetProjects')
                .then(response => response.json())
                .then(projects => {
                    // Show project selection modal
                    showProjectDropdownModal(projects, reportId);
                });
        });
}

//Creates and displays a Bootstrap modal allowing the user to select a Jira project. 
function showProjectDropdownModal(projects, reportId) {
    // Check if modal already exists; if not, create it
    let modal = document.getElementById('projectSelectModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'projectSelectModal';
        modal.classList.add('modal', 'fade');
        modal.tabIndex = -1;
        modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Select Jira Project</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <select class="form-select" id="jiraProjectDropdown"></select>
                    <div id="jiraExportStatus" class="mt-3 text-center text-muted small"></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" id="confirmProjectButton">Export</button>
                </div>
            </div>
        </div>`;
        document.body.appendChild(modal);
    }

    // Populate dropdown with available projects
    const select = modal.querySelector('#jiraProjectDropdown');
    select.innerHTML = '';
    projects.forEach(p => {
        const option = document.createElement('option');
        option.value = p.id;
        option.textContent = `${p.key} - ${p.name}`;
        select.appendChild(option);
    });

    // Set up button to trigger export with selected project
    modal.querySelector('#confirmProjectButton').onclick = function() {
        const selectedId = select.value;
        exportReportToJira(reportId, selectedId);
    };

    // Show the modal
    new bootstrap.Modal(modal).show();
}

// Sends a request to export a report to Jira under the selected project.
function exportReportToJira(reportId, projectId) {
    // Get references to modal elements
    const modal = document.getElementById('projectSelectModal');
    const exportButton = modal.querySelector('#confirmProjectButton');
    const statusDiv = modal.querySelector('#jiraExportStatus');

    // Show spinner and disable export button to prevent double submission
    if (exportButton) {
        exportButton.disabled = true;
        exportButton.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Sending...
        `;
    }

    // Send POST request to the server to export the report
    fetch(`/JiraAPI/ExportReportToJira?reportId=${reportId}&projectKey=${encodeURIComponent(projectId)}`, {
        method: 'POST'
    })
    .then(response => {
        if (response.ok) {
            // Success: show success message in modal
            statusDiv.textContent = "Report successfully exported to Jira.";

            // Automatically close modal after a short delay (1 second)
            setTimeout(() => {
                bootstrap.Modal.getInstance(modal).hide(); // Close modal
                statusDiv.textContent = ""; // Clear message for next time
            }, 1000);
        } else {
            // Server responded with error: show error message in modal
            response.text().then(text => {
                statusDiv.textContent = `Failed to send report. (${response.status})`;
            });
        }
    })
    .catch(() => {
        // Network or unexpected error: show error message in modal
        statusDiv.textContent = "An error occurred while sending the report.";
    })
    .finally(() => {
        // Re-enable button and reset text no matter what happened
        if (exportButton) {
            exportButton.disabled = false;
            exportButton.textContent = "Export";
        }
    });
}

// Function to delete a report
function deleteReport(reportId) {

    fetch(`/Home/DeleteReport?reportId=${reportId}`, {
        method: 'DELETE'
    })
    .then(response => {
        if (response.ok) {
            const reportElement = document.getElementById(`report-${reportId}`);
            if (reportElement) {
                reportElement.remove(); // Remove the report from the DOM

                for (const domain in groupedReports) {
                    const reportIndex = groupedReports[domain].findIndex(r => r.id === reportId);
                    if (reportIndex !== -1) {
                        // Remove the report from the groupedReports object
                        groupedReports[domain].splice(reportIndex, 1);

                        // Update the badge count
                        const badge = document.querySelector(`#websiteAccordion-${domain.replace(/\W/g, '')} .badge`);
                        if (badge) {
                            badge.textContent = groupedReports[domain].length;
                        }

                        // If no reports remain for the domain, remove the domain card
                        if (groupedReports[domain].length === 0) {
                            const domainCard = document.getElementById(`websiteAccordion-${domain.replace(/\W/g, '')}`);
                            if (domainCard) {
                                domainCard.parentElement.remove();
                            }
                        }
                        break;
                    }
                }
            }
        } else {
            console.error('Failed to delete report:', response.statusText);
        }
    })
    .catch(error => console.error('Error deleting report:', error));
}


// Function to format date
function formattedDate(dateString) {
    const [year, month, day] = dateString.split('T')[0].split('-');
    return new Date(year, month - 1, day).toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric' 
    });
}

// Function to download a report as a PDF
function downloadReport(reportId) {
    fetch(`/Home/DownloadReport?id=${reportId}`)  
        .then(response => response.blob())
        .then(blob => {
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = `UxCheckmate_Report_ID_${reportId}.pdf`;  
            link.click();  
        })
        .catch(error => console.error('Error downloading the report:', error));
}

// Function to view a report's details in a modal
function viewReportDetails(reportId) {
    const report = findReportById(reportId);
    if (report) {
        const modalBody = document.getElementById('reportModalBody');
        const safeReportId = `report-${report.id}`; 

        modalBody.innerHTML = `

            <header class="myUserDashHeader">
                <h1 id="reportHeader">${report.url}</h1>
                <p>${formattedDate(report.date)}</p>
                <a href="#" class="btn btn-secondary" onclick="downloadReport(${report.id})">Download Report</a>
            </header>

            <h3 class="mt-3 title-issues">Design Issues</h3>
            <p class="text-subtle">${report.designIssues.length} Issues found</p>
            ${report.designIssues.length > 0 ? `
                <div class="accordion" id="designIssuesAccordion-${safeReportId}">
                    ${report.designIssues.map((issue, index) => `
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="designIssueHeader-${safeReportId}-${index}">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#designIssueContent-${safeReportId}-${index}" aria-expanded="false" aria-controls="designIssueContent-${safeReportId}-${index}">
                                    Severity: ${issue.severity}
                                </button>
                            </h2>
                            <div id="designIssueContent-${safeReportId}-${index}" class="accordion-collapse collapse" aria-labelledby="designIssueHeader-${safeReportId}-${index}" data-bs-parent="#designIssuesAccordion-${safeReportId}">
                                <div class="accordion-body">
                                    <p><strong>Message:</strong> ${issue.message}</p>
                                    <p><small class="text-subtle">Category: ${issue.category}</small></p>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            ` : `
                <p class="text-success">Congrats! No Design Issues</p>
            `}

            <h3 class="mt-3 title-issues">Accessibility Issues</h3>
            <p class="text-subtle">${report.accessibilityIssues.length} Issues found</p>
            ${report.accessibilityIssues.length > 0 ? `
                <div class="accordion" id="accessibilityIssuesAccordion-${safeReportId}">
                    ${report.accessibilityIssues.map((issue, index) => `
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="accessibilityIssueHeader-${safeReportId}-${index}">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#accessibilityIssueContent-${safeReportId}-${index}" aria-expanded="false" aria-controls="accessibilityIssueContent-${safeReportId}-${index}">
                                    Severity: ${issue.severity}
                                </button>
                            </h2>
                            <div id="accessibilityIssueContent-${safeReportId}-${index}" class="accordion-collapse collapse" aria-labelledby="accessibilityIssueHeader-${safeReportId}-${index}" data-bs-parent="#accessibilityIssuesAccordion-${safeReportId}">
                                <div class="accordion-body">
                                    <p><strong>Message:</strong> ${issue.message}</p>
                                    <p><small class="text-subtle">Category: ${issue.category} | WCAG: ${issue.wcag}</small></p>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            ` : `
                <p class="text-success">No accessibility issues</p>
            `}
        `;

        const modal = new bootstrap.Modal(document.getElementById('reportModal'));
        modal.show();
    } else {
        console.error('Report not found:', reportId);
    }
}

// Function to find a report by its ID
function findReportById(id) {
    for (const domain in groupedReports) {
        const report = groupedReports[domain].find(r => r.id === id);
        if (report) return report;
    }
    return null;
}

// Group reports by domain and dynamically load them into the dashboard
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
        folderDiv.classList.add('folder-card', 'glass-card', 'mb-3');
    
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
                            <div class="col-md-3 d-flex justify-content-center align-items-center">
                                <span class="badge bg-primary">${groupedReports[domain].length} reports</span>
                            </div>
                        </button>
                    </h2>
    
                    <div id="${collapseId}" class="accordion-collapse collapse" aria-labelledby="${headingId}" data-bs-parent="#${accordionId}">
                        <div class="accordion-body">
                            <div class="row fw-bold mb-2">
                                <div class="col-sm-5 groupedReportHeader">Webpage</div>
                                <div class="col-sm-3 groupedReportHeader">Date</div>
                                <div class="col-sm-3 groupedReportHeader">Actions</div>
                            </div>
    
                            ${groupedReports[domain].map(r => `
                                <div class="row mb-2" id="report-${r.id}">
                                    <div class="col-sm-5 text-break groupedReportHeader">${r.url}</div> 
                                    <div class="col-sm-3 groupedReportHeader">${formattedDate(r.date)}</div>
                                    <div class="col-sm-1">
                                        <button class="reportBtns" onclick="viewReportDetails(${r.id})">
                                            <img src="/images/eye.png" alt="eye icon">
                                        </button>
                                    </div>
                                    <div class="col-sm-1">
                                        <button class="reportBtns exportJiraBtn" onclick="sendToJira(${r.id})">
                                            <img src="/images/file-export.png" alt="export icon">
                                        </button>    
                                    </div>
                                    <div class="col-sm-1">
                                        <button class="reportBtns deleteReportbtn" onclick="deleteReport(${r.id})">
                                            <img src="/images/trash.png" alt="trash icon">
                                        </button>
                                    </div>
                                </div>
                                <hr>
                            `).join('')}
                        </div>
                    </div>
    
                </div>
            </div>
        `;
    
        container.appendChild(folderDiv);
    }

   // Automatically resume export if user was redirected back after Jira login 
    const urlParams = new URLSearchParams(window.location.search);
    const resumeReportId = urlParams.get("resumeReportId");
    if (resumeReportId) {
        sendToJira(parseInt(resumeReportId));
        window.history.replaceState({}, document.title, window.location.pathname); // Clean URL
    }
});

// Toggle function to show/hide reports inside a folder
function toggleReports(domain) {
    const list = document.getElementById(`reports-${domain}`);
    list.style.display = (list.style.display === 'none') ? 'block' : 'none';
}

// Export the functions for testing purposes
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { deleteReport, formattedDate, downloadReport, viewReportDetails, findReportById };
    module.exports.groupedReports = groupedReports; // Export the groupedReports object for testing
}