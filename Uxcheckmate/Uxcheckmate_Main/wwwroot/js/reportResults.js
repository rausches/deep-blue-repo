console.log("reportResults.js loaded!");

/*
===========================================================================================================
JS for view more on flagged item results. If making changes to this file please verify it still works!
===========================================================================================================
*/

function toggleSelector(id) {
    // Grab references to the short text, full text, and link
    const shortEl = document.getElementById(`selectorShort-${id}`);
    const fullEl = document.getElementById(`selectorFull-${id}`);
    const linkEl = document.getElementById(`toggleLink-${id}`);

    // If short is hidden, switch back to short
    if (shortEl.style.display === 'none') {
        shortEl.style.display = 'inline';
        fullEl.style.display = 'none';
        linkEl.textContent = 'View More';
    } 
    // Otherwise show the full text
    else {
        shortEl.style.display = 'none';
        fullEl.style.display = 'inline';
        linkEl.textContent = 'View Less';
    }
}

/*
===========================================================================================================
                                            REPORT SORTING
===========================================================================================================
*/

// Store the original HTML of both accordions when the page loads
let originalDesignAccordion = '';
let originalAccessibilityAccordion = '';

// Create deep copies of the original design and accessibility issues
// These will be used as the source of truth for all sorting operations
let originalDesignIssues = [];
let originalAccessibilityIssues = [];

document.addEventListener('DOMContentLoaded', function() {
    // Save the original HTML structure of both accordions
    originalDesignAccordion = document.getElementById('designIssuesAccordion').innerHTML;
    originalAccessibilityAccordion = document.getElementById('accessibilityIssuesAccordion').innerHTML;
    
    // Extract and store the original issue data
    extractOriginalIssueData();
    
    // Add event listener to the sort dropdown
    document.getElementById('sortOptions').addEventListener('change', function() {
        const sortBy = this.value;
        console.log("Sort option changed to:", sortBy);
        
        if (sortBy === 'category') {
            // Restore original category-based grouping
            console.log("Restoring original category view");
            document.getElementById('designIssuesAccordion').innerHTML = originalDesignAccordion;
            document.getElementById('accessibilityIssuesAccordion').innerHTML = originalAccessibilityAccordion;
        } else {
            // Sort by severity (high-to-low or low-to-high)
            console.log("Sorting by severity:", sortBy);
            const highToLow = sortBy === 'severity-high-low';
            sortBySeverity(highToLow);
        }
    });
    
    // Ensure all report elements have the uxcheckmate-report-element class
    // This helps prevent style contamination from analyzed sites
    addReportClassesToElements();
});

// Add namespace class to all report elements to prevent style conflicts
function addReportClassesToElements() {
    const reportContainer = document.querySelector('.container');
    if (reportContainer) {
        reportContainer.classList.add('uxcheckmate-report');
        
        // Add class to all selects in the report
        const selects = reportContainer.querySelectorAll('select');
        selects.forEach(select => {
            select.classList.add('uxcheckmate-select');
        });
        
        // Add class to all paragraphs in the report
        const paragraphs = reportContainer.querySelectorAll('p');
        paragraphs.forEach(p => {
            p.classList.add('uxcheckmate-text');
        });
    }
}

// Extract original issue data from the DOM
function extractOriginalIssueData() {
    console.log("Extracting original issue data");
    
    // Extract design issues
    originalDesignIssues = [];
    const designAccordion = document.getElementById('designIssuesAccordion');
    if (!designAccordion) {
        console.error("Design accordion not found");
        return;
    }
    
    const designItems = designAccordion.querySelectorAll('.accordion-item');
    designItems.forEach(item => {
        const categoryNameEl = item.querySelector('.accordion-button strong');
        if (!categoryNameEl) return;
        
        const categoryName = categoryNameEl.textContent.trim();
        const accordionBody = item.querySelector('.accordion-body');
        if (!accordionBody) return;
        
        const messageEl = accordionBody.querySelector('.col-md-11');
        const message = messageEl ? messageEl.textContent.trim() : '';
        
        const severityEl = accordionBody.querySelector('.badge');
        const severity = severityEl ? severityEl.textContent.trim() : 'Low';
        
        const severityValue = severity === 'High' ? 3 : (severity === 'Medium' ? 2 : 1);
        
        originalDesignIssues.push({
            categoryName,
            severity,
            severityValue,
            message: messageEl ? messageEl.textContent.trim() : ''
        });
    });
    
    console.log(`Extracted ${originalDesignIssues.length} design issues`);
    
    // Extract accessibility issues
    originalAccessibilityIssues = [];
    const accessibilityAccordion = document.getElementById('accessibilityIssuesAccordion');
    if (!accessibilityAccordion) {
        console.error("Accessibility accordion not found");
        return;
    }
    
    const categoryGroups = accessibilityAccordion.querySelectorAll('.accordion-item');
    categoryGroups.forEach(group => {
        const categoryNameEl = group.querySelector('.accordion-button strong');
        if (!categoryNameEl) return;
        
        const categoryName = categoryNameEl.textContent.trim();
        const issueContainers = group.querySelectorAll('.issueContainer');
        
        issueContainers.forEach(container => {
            const detailsEl = container.querySelector('.col-md-11');
            const details = detailsEl ? detailsEl.textContent.trim() : '';
            
            const severityEl = container.querySelector('.badge');
            const severity = severityEl ? severityEl.textContent.trim() : 'Low';
            
            // Map severity text to numeric value
            let severityValue = 1;
            if (severity === 'Critical') severityValue = 4;
            else if (severity === 'Severe') severityValue = 3;
            else if (severity === 'Moderate') severityValue = 2;
            
            // Extract selector information if it exists
            const selectorShortEl = container.querySelector('[id^="selectorShort-"]');
            const selectorFullEl = container.querySelector('[id^="selectorFull-"]');
            const toggleLinkEl = container.querySelector('[id^="toggleLink-"]');
            
            const selectorData = {
                text: selectorShortEl ? selectorShortEl.textContent : null,
                fullText: selectorFullEl ? selectorFullEl.textContent : null,
                id: selectorShortEl ? selectorShortEl.id.split('-')[1] : null,
                hasToggle: !!toggleLinkEl
            };
            
            originalAccessibilityIssues.push({
                categoryName,
                severity,
                severityValue,
                details,
                selector: selectorData,
                originalHtml: container.outerHTML
            });
        });
    });
    
    console.log(`Extracted ${originalAccessibilityIssues.length} accessibility issues`);
}

function sortBySeverity(highToLow) {
    // Sort design issues
    sortDesignIssuesBySeverity(highToLow);
    
    // Sort accessibility issues
    sortAccessibilityIssuesBySeverity(highToLow);
    
    // Re-apply classes to ensure consistent styling
    addReportClassesToElements();
}

/*
===========================================================================================================
                                            DESIGN SORTING
===========================================================================================================
*/

function sortDesignIssuesBySeverity(highToLow) {
    console.log(`Sorting design issues: ${highToLow ? 'high to low' : 'low to high'}`);
    
    // Get design accordion element
    const designAccordion = document.getElementById('designIssuesAccordion');
    if (!designAccordion || originalDesignIssues.length === 0) {
        console.error("Design accordion not found or no issues to sort");
        return;
    }
    
    // Clear the accordion
    designAccordion.innerHTML = '';
    
    // Group issues by severity
    const severityGroups = {
        'High': [],
        'Medium': [],
        'Low': []
    };
    
    // Add each issue to its severity group
    originalDesignIssues.forEach(issue => {
        severityGroups[issue.severity].push(issue);
    });
    
    // Determine display order based on sort direction
    const severityOrder = highToLow ? 
        ['High', 'Medium', 'Low'] : 
        ['Low', 'Medium', 'High'];
    
    // Create and append severity group headers
    severityOrder.forEach(severity => {
        const issues = severityGroups[severity];
        if (!issues || issues.length === 0) {
            console.log(`No ${severity} severity design issues found`);
            return;
        }
        
        console.log(`Creating group for ${severity} severity design issues (${issues.length} issues)`);
        
        // Create a stable group ID
        const groupId = `design-severity-${severity.toLowerCase()}`;
        
        // Create the accordion item for this severity group
        const groupItem = document.createElement('div');
        groupItem.className = 'accordion-item reportAccordion uxcheckmate-report-element';
        groupItem.innerHTML = `
            <h2 class="accordion-header reportAccordion uxcheckmate-report-element" id="heading-${groupId}">
                <button class="accordion-button reportAccordionButton collapsed uxcheckmate-report-element" type="button" 
                        data-bs-toggle="collapse" 
                        data-bs-target="#collapse-${groupId}" 
                        aria-expanded="false" 
                        aria-controls="collapse-${groupId}">
                    <strong class="uxcheckmate-text">${severity} Severity Issues</strong> &nbsp;
                    <span class="text-muted uxcheckmate-text">
                        (${issues.length} issues)
                    </span>
                </button>
            </h2>
            <div id="collapse-${groupId}" class="accordion-collapse collapse uxcheckmate-report-element" 
                aria-labelledby="heading-${groupId}" 
                data-bs-parent="#designIssuesAccordion">
                <div class="accordion-body uxcheckmate-report-element" id="body-${groupId}">
                </div>
            </div>
        `;
        
        // Add the group to the accordion
        designAccordion.appendChild(groupItem);
        
        // Get reference to the group body
        const groupBody = document.getElementById(`body-${groupId}`);
        if (!groupBody) {
            console.error(`Could not find body element for ${severity} design issues`);
            return;
        }
        
        // Add each issue to the group body
        issues.forEach((issue, index) => {
            const issueDiv = document.createElement('div');
            issueDiv.className = 'issueContainer p-3 mb-3 border rounded uxcheckmate-report-element';
            issueDiv.innerHTML = `
            <div class="row mb-2 uxcheckmate-report-element">
                <div class="col-md-12 uxcheckmate-report-element">
                    <span class="badge bg-secondary reportAccordion uxcheckmate-report-element">${issue.categoryName}</span>
                </div>
            </div>
            <div class="row uxcheckmate-report-element">
                <div class="col-md-12 reportAccordionText uxcheckmate-report-element">
                    <p class="reportAccordionText uxcheckmate-text">${issue.message}</p>
                </div>
            </div>
            `;
            groupBody.appendChild(issueDiv);
        });
    });
}

/*
===========================================================================================================
                                        ACCESSIBILITY SORTING
===========================================================================================================
*/

function sortAccessibilityIssuesBySeverity(highToLow) {
    console.log(`Sorting accessibility issues: ${highToLow ? 'high to low' : 'low to high'}`);
    
    // Get accessibility accordion element
    const accessibilityAccordion = document.getElementById('accessibilityIssuesAccordion');
    if (!accessibilityAccordion || originalAccessibilityIssues.length === 0) {
        console.error("Accessibility accordion not found or no issues to sort");
        return;
    }
    
    // Clear the accordion
    accessibilityAccordion.innerHTML = '';
    
    // Group issues by severity
    const severityGroups = {
        'Critical': [],
        'Severe': [],
        'Moderate': [],
        'Low': []
    };
    
    // Add each issue to its severity group
    originalAccessibilityIssues.forEach(issue => {
        if (severityGroups[issue.severity]) {
            severityGroups[issue.severity].push(issue);
        } else {
            // Default to Low if the severity is unknown
            severityGroups['Low'].push(issue);
        }
    });
    
    // Determine display order based on sort direction
    const severityOrder = highToLow ? 
        ['Critical', 'Severe', 'Moderate', 'Low'] : 
        ['Low', 'Moderate', 'Severe', 'Critical'];
    
    // Create and append severity group headers
    severityOrder.forEach(severity => {
        const issues = severityGroups[severity];
        if (!issues || issues.length === 0) {
            console.log(`No ${severity} severity accessibility issues found`);
            return;
        }
        
        console.log(`Creating group for ${severity} severity accessibility issues (${issues.length} issues)`);
        
        // Create a stable group ID
        const groupId = `accessibility-severity-${severity.toLowerCase()}`;
        
        // Create the accordion item for this severity group
        const groupItem = document.createElement('div');
        groupItem.className = 'accordion-item reportAccordion uxcheckmate-report-element';
        groupItem.innerHTML = `
            <h2 class="accordion-header reportAccordion uxcheckmate-report-element" id="heading-${groupId}">
                <button class="accordion-button reportAccordionButton collapsed uxcheckmate-report-element" type="button" 
                        data-bs-toggle="collapse" 
                        data-bs-target="#collapse-${groupId}" 
                        aria-expanded="false" 
                        aria-controls="collapse-${groupId}">
                    <strong class="uxcheckmate-text">${severity} Severity Issues</strong> &nbsp;
                    <span class="text-muted uxcheckmate-text">
                        (${issues.length} issues)
                    </span>
                </button>
            </h2>
            <div id="collapse-${groupId}" class="accordion-collapse collapse uxcheckmate-report-element" 
                aria-labelledby="heading-${groupId}" 
                data-bs-parent="#accessibilityIssuesAccordion">
                <div class="accordion-body reportAccordion uxcheckmate-report-element" id="body-${groupId}">
                </div>
            </div>
        `;
        
        // Add the group to the accordion
        accessibilityAccordion.appendChild(groupItem);
        
        // Get reference to the group body
        const groupBody = document.getElementById(`body-${groupId}`);
        if (!groupBody) {
            console.error(`Could not find body element for ${severity} accessibility issues`);
            return;
        }
        
        // Add each issue to the group body
        issues.forEach((issue, index) => {
            const issueDiv = document.createElement('div');
            issueDiv.className = 'issueContainer p-3 mb-3 border rounded uxcheckmate-report-element';
            
            // Add category badge
            let issueHtml = `
                <div class="row mb-2 uxcheckmate-report-element">
                    <div class="col-md-12 uxcheckmate-report-element">
                        <span class="badge bg-secondary reportAccordion uxcheckmate-report-element">${issue.categoryName}</span>
                    </div>
                </div>
                <div class="row uxcheckmate-report-element">
                    <div class="col-md-12 reportAccordionText uxcheckmate-report-element">
                        <p class="reportAccordionText uxcheckmate-text">${issue.details}</p>
                    </div>
                </div>
            `;
            
            // Add selector information if available
            if (issue.selector && issue.selector.text) {
                const selectorId = issue.selector.id || `issue-${index}`;
                
                if (issue.selector.hasToggle && issue.selector.fullText) {
                    // Include the toggle functionality
                    issueHtml += `
                        <div class="row uxcheckmate-report-element">
                            <div class="col-md-12 reportAccordionText uxcheckmate-report-element">
                                <p class="reportAccordionText uxcheckmate-text">
                                    <strong class="uxcheckmate-text">Flagged Item:</strong>
                                    <span id="selectorShort-${selectorId}" class="reportAccordionText uxcheckmate-text">${issue.selector.text}</span>
                                    <span id="selectorFull-${selectorId}" class="reportAccordionText uxcheckmate-text" style="display:none;">
                                        ${issue.selector.fullText}
                                    </span>
                                    <a href="javascript:void(0);"
                                       id="toggleLink-${selectorId}"
                                       class="reportAccordionText uxcheckmate-report-element"
                                       onclick="toggleSelector('${selectorId}')">
                                        View More
                                    </a>
                                </p>
                            </div>
                        </div>
                    `;
                } else {
                    // Simple display without toggle
                    issueHtml += `
                        <div class="row uxcheckmate-report-element">
                            <div class="col-md-12 reportAccordionText uxcheckmate-report-element">
                                <p class="reportAccordionText uxcheckmate-text">
                                    <strong class="uxcheckmate-text">Flagged Item:</strong>
                                    <span id="selectorShort-${selectorId}" class="reportAccordionText uxcheckmate-text">${issue.selector.text}</span>
                                </p>
                            </div>
                        </div>
                    `;
                }
            }
            
            issueDiv.innerHTML = issueHtml;
            groupBody.appendChild(issueDiv);
        });
    });
}