console.log("reportResults.js loaded!");

/*
===========================================================================================================
JS for view more on flagged item results. If making changes to this file please verify it still works!
===========================================================================================================
*/

function toggleSelector(id) {
    // Existing toggle code remains the same
    const shortEl = document.getElementById(`selectorShort-${id}`);
    const fullEl = document.getElementById(`selectorFull-${id}`);
    const linkEl = document.getElementById(`toggleLink-${id}`);

    if (shortEl.style.display === 'none') {
        shortEl.style.display = 'inline';
        fullEl.style.display = 'none';
        linkEl.textContent = 'View More';
    } else {
        shortEl.style.display = 'none';
        fullEl.style.display = 'inline';
        linkEl.textContent = 'View Less';
    }
}

/*
===========================================================================================================
                                        Sorting Behavior 
===========================================================================================================
*/

document.addEventListener('DOMContentLoaded', function() {
    // Get reference to the sort selection dropdown element
    const sortSelect = document.getElementById('sortSelect');
    
    // Safety check: Ensure the element exists before proceeding
    if (!sortSelect) {
        console.error('Sort select element not found!');
        return; // Exit early to prevent further errors
    }

    // Get the report ID from data attribute (data-report-id)
    // This replaces the Razor @Model.Id syntax for proper separation of concerns
    // The value is stored in HTML using: <select data-report-id="@Model.Id">
    const reportId = sortSelect.dataset.reportId;

    // Add event listener for when the dropdown selection changes
    sortSelect.addEventListener('change', function() {
        // Get the currently selected value from the dropdown
        const sortOrder = this.value;
        
        // Make API call to get sorted issues
        fetch(`/Home/GetSortedIssues?id=${reportId}&sortOrder=${sortOrder}`)
            // Convert response to JSON format
            .then(response => response.json())
            // Handle successful response
            .then(data => {
                // Update design issues section with new HTML
                // innerHTML is used because server returns pre-rendered HTML
                document.getElementById('designIssuesContainer').innerHTML = data.designHtml;
                
                // Update accessibility issues section similarly
                document.getElementById('accessibilityIssuesContainer').innerHTML = data.accessibilityHtml;
                
                // Reinitialize collapsible elements after DOM update
                // Required because new elements won't have Bootstrap handlers attached
                initBootstrapCollapses();
            })
            // Handle any errors in the fetch chain
            .catch(error => console.error('Error fetching sorted issues:', error));
    });

    // Initialize collapsible elements on initial page load
    initBootstrapCollapses();
});


function initBootstrapCollapses() {
    // Find all accordion containers on the page
    // Uses querySelectorAll to handle multiple accordions
    document.querySelectorAll('.accordion').forEach(accordion => {
        // Find all collapsible elements within this accordion
        const collapses = accordion.querySelectorAll('.accordion-collapse');
        
        // Initialize each collapsible element with Bootstrap
        collapses.forEach(collapse => {
            // Create new Bootstrap Collapse instance
            new bootstrap.Collapse(collapse, {
                toggle: false, // Prevent auto-toggle on init
                parent: accordion // Set accordion as parent for exclusive behavior
            });
        });

        // Add custom click handlers to all accordion buttons
        accordion.querySelectorAll('.accordion-button').forEach(button => {
            button.addEventListener('click', function() {
                // Get target collapse element from data attribute
                const target = document.querySelector(this.dataset.bsTarget);
                
                // Get existing Bootstrap instance or create new one
                const collapse = bootstrap.Collapse.getInstance(target) || 
                               new bootstrap.Collapse(target);
                
                // Toggle visibility based on current state
                if (target.classList.contains('show')) {
                    // If open, hide the collapse
                    collapse.hide();
                } else {
                    // If closed, first hide all siblings
                    collapses.forEach(c => {
                        if (c !== target) {
                            // Get instance and hide if exists
                            const instance = bootstrap.Collapse.getInstance(c);
                            instance?.hide();
                        }
                    });
                    // Then show the target collapse
                    collapse.show();
                }
            });
        });
    });
}

document.addEventListener("DOMContentLoaded", function () {

    // Grab the sorting dropdown element
    const sortSelect = document.getElementById('sortSelect');
    if (!sortSelect) return; // Exit if sort dropdown isn't found (fail-safe)

    // Extract the report ID from a data attribute set on the sort dropdown
    const reportId = sortSelect.dataset.reportId;

    // Store the current sort value
    let currentSortOrder = sortSelect.value;

    // Will hold the ID for the interval-based polling timer
    let pollingIntervalId;

    // Used to prevent the summary modal from being shown more than once
    let hasShownModal = false;

    // Polling function: fetch updated issue data from the server
    async function fetchUpdatedIssues() {
        try {
            // Send request to the backend API with current sort order and cache-busting timestamp
            const response = await fetch(`${window.location.origin}/Home/GetSortedIssues?id=${reportId}&sortOrder=${currentSortOrder}&t=${Date.now()}`);
            if (!response.ok) throw new Error("Fetch failed");

            // Parse the JSON response
            const data = await response.json();

            // Update the Design Issues section if new HTML is provided
            if (data.designHtml) {
                // Replace existing design issue content with the new sorted HTML
                document.getElementById('designIssuesContainer').innerHTML = data.designHtml;

                // Re-attach click event listeners to any ✨ buttons
                delegateOptimizeButtons();

                // Update the displayed issue count
                const totalIssues = document.querySelectorAll('.issue-card').length;
                document.getElementById('totalIssues').innerHTML = `Total Issues Found: ${totalIssues}`;
            }

            // Check if the backend marked the report as "Completed"
            if (data.status === "Completed") {

                // Update the summary text content dynamically
                const summaryElement = document.getElementById('summary');
                if (summaryElement) {
                    summaryElement.innerText = data.summary || "No summary available.";
                    summaryElement.classList.add('visible');
                }

                // Hide the loading overlay on the design issues section
                const overlay = document.getElementById('designIssuesOverlay');
                overlay?.classList.add('hidden');

                // If no design issues are found, display a success alert instead
                const designIssuesCount = document.querySelectorAll('#designIssuesContainer .issue-card').length;
                if (designIssuesCount === 0) {
                    document.getElementById('designIssuesContainer').innerHTML = `
                        <div class="alert alert-success">
                            <strong>No UX issues found! Your site looks great.</strong>
                        </div>
                    `;
                }

                // Show the AI-generated summary modal once
                if (!hasShownModal) {
                    const modalElement = document.getElementById('onLoadModal');
                    if (modalElement) {
                        const modal = new bootstrap.Modal(modalElement);
                        modal.show();
                        hasShownModal = true;
                    }
                }

                // Enable the sort dropdown again now that analysis is done
                sortSelect.disabled = false;

                // Reveal the "View Summary" button
                document.getElementById("viewSummaryBtn").hidden = false;

                // Stop polling because analysis is complete
                clearInterval(pollingIntervalId);
                console.log("Polling stopped — report analysis completed!");
            }
        } catch (error) {
            console.error("Failed to fetch updated issues:", error);
        }
    }

    // When the user changes the sort option, re-fetch updated sorted issues
    sortSelect.addEventListener('change', () => {
        currentSortOrder = sortSelect.value;
        fetchUpdatedIssues();
    });

    // Allow users to re-open the summary modal manually
    document.getElementById("viewSummaryBtn").addEventListener("click", function () {
        const modalElement = document.getElementById('onLoadModal');
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
        }
    });

    // Perform the first fetch immediately so the user sees results quickly
    fetchUpdatedIssues();

    // Start polling every second to check for updated report status and content
    pollingIntervalId = setInterval(fetchUpdatedIssues, 1000);
});


// Attach click handlers to all ✨ buttons that exist
function delegateOptimizeButtons() {
    document.querySelectorAll(".optimize-btn").forEach(btn => {
        btn.addEventListener("click", async function () {
            // Get the issue's message and category from data attributes
            const message = btn.dataset.message;
            const category = btn.dataset.category;

            // Look for the parent container where we want to show the AI response
            const container = btn.closest('.issueItem') || btn.closest('.col-md-11');
            const responseContainer = container.querySelector('.ai-response');
            if (!responseContainer) return;

            // Show a spinner and loading message while we fetch the AI insight
            responseContainer.classList.remove('d-none');
            responseContainer.innerHTML = `
                <div class="d-flex align-items-center">
                    <div class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></div>
                    <span>Fetching AI insights…</span>
                </div>
            `;

            try {
                // Send the raw message and category to the backend API
                const response = await fetch('/api/chat/improve', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ message: message, category: category })
                });

                // If the request failed, throw an error
                if (!response.ok) throw new Error("Fetch failed");

                // Read and display the AI-generated suggestion
                const data = await response.text();
                responseContainer.textContent = data;
            } catch (error) {
                // Show a fallback error message if the fetch fails
                responseContainer.textContent = "Server is busy. Try again later.";
                console.error(error);
            }
        });
    });
}

