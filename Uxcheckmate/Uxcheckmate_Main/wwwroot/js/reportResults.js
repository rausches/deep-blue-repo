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

