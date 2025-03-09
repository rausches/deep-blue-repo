console.log("reportResults.js loaded!");

/*
===================================================================================================
JS for view more on flagged item results. If making changes to this please verify it still works!
===================================================================================================
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
/*document.addEventListener("DOMContentLoaded", function () {
    // Get the URL from the hidden input
    const url = document.getElementById("analyzed-url").value;

    // Ensure URL is valid before making the API call
    if (url) {
        fetch(`/api/analysis/all-reports/${encodeURIComponent(url)}`)
            .then(response => response.json())
            .then(data => {
                const resultsContainer = document.getElementById("results-container");

                // Clear previous results if any
                resultsContainer.innerHTML = `
                    <h3>Analysis Results</h3>
                    <p>Total Reports: ${data.Item1.length}</p>
                    <table class="table table-striped table-bordered">
                        <thead class="thead-dark">
                            <tr>
                                <th>Category</th>
                                <th>Recommendations</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${data.Item1.map(report => `
                                <tr>
                                    <td>${report.Category ? report.Category.Name : "No Category Assigned"}</td>
                                    <td>${report.Recommendations}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>

                    <h3>Accessibility Issues</h3>
                    <p>Total Issues: ${data.Item2.length}</p>
                    <table class="table table-striped table-bordered">
                        <thead class="thead-dark">
                            <tr>
                                <th>Code</th>
                                <th>Message</th>
                                <th>Selector</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${data.Item2.map(issue => `
                                <tr>
                                    <td>${issue.Code}</td>
                                    <td>${issue.Message}</td>
                                    <td>${issue.Selector}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                `;
            })
            .catch(error => console.error("Error fetching analysis data:", error));
    }
});
*/