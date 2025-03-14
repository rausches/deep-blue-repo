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
                                        Sorting Behavior
===========================================================================================================
*/
    // Attach an event listener to the 'sortSelect' dropdown element
    document.getElementById('sortSelect').addEventListener('change', function() {
        // Retrieve the currently selected sort order value from the dropdown
        const sortOrder = this.value;
        
        // Make a fetch request to the server to retrieve sorted issues
        // The sortOrder is passed as a query parameter along with the model ID
        fetch(`/Home/GetSortedIssues?id=@Model.Id&sortOrder=${sortOrder}`)
            .then(response => response.json()) // Parse the JSON response
            .then(data => {
                // Update the inner HTML of the design issues container with the returned HTML content
                document.getElementById('designIssuesContainer').innerHTML = data.designHtml;

                // Update the inner HTML of the accessibility issues container with the returned HTML content
                document.getElementById('accessibilityIssuesContainer').innerHTML = data.accessibilityHtml;

                // Re-initialize Bootstrap collapsible elements to ensure new content is interactive
                initBootstrapCollapses();
            });
    });

    // Function to initialize all Bootstrap collapsible elements within accordions
    function initBootstrapCollapses() {
        // Select all accordion elements on the page
        document.querySelectorAll('.accordion').forEach(accordion => {
            // Select all collapsible elements within the current accordion
            const collapses = accordion.querySelectorAll('.accordion-collapse');
            
            // Initialize each collapsible element
            collapses.forEach(collapse => {
                new bootstrap.Collapse(collapse, {
                    toggle: false, // Do not automatically toggle upon initialization
                    parent: accordion // Set the accordion as the parent element for collapse behavior
                });
            });

            // Add click event listeners to all accordion buttons within the current accordion
            accordion.querySelectorAll('.accordion-button').forEach(button => {
                button.addEventListener('click', function() {
                    // Determine the target collapse element associated with the clicked button
                    const target = document.querySelector(this.dataset.bsTarget);
                    
                    // Retrieve an existing Bootstrap collapse instance or create a new one
                    const collapse = bootstrap.Collapse.getInstance(target) || new bootstrap.Collapse(target);
                    
                    if (target.classList.contains('show')) {
                        // If the target collapse is currently open, hide it
                        collapse.hide();
                    } else {
                        // Hide all other collapsible elements in the same accordion, except the target
                        collapses.forEach(c => {
                            if (c !== target) bootstrap.Collapse.getInstance(c)?.hide();
                        });
                        // Show the target collapse element
                        collapse.show();
                    }
                });
            });
        });
    }
    
    // Perform initial initialization of collapsible elements on page load
    initBootstrapCollapses();