// Function to validate the URL format
function validateURL(urlInput) {
    var urlRegex = /^(https?:\/\/)?([\w-]+\.[\w.-]+)+(\/[\w-]*)*$/;
    return urlRegex.test(urlInput);
}

// Function to handle user input and fetch scraping results
async function handleUserUrl(event) {
    event.preventDefault();

    var urlInput = document.getElementById("urlInput").value.trim();
    var confirmationMessage = document.getElementById("confirmationMessage");
    var resultsContainer = document.getElementById("resultsContainer");

    confirmationMessage.innerHTML = ""; // Clear previous messages
    resultsContainer.innerHTML = ""; // Clear previous results

    if (!urlInput) {
        confirmationMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Empty URL!</strong></h5>
            <p>Please enter a URL before submitting.</p>
        </div>`;
        return;
    }

    if (!validateURL(urlInput)) {
        confirmationMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Invalid URL!</strong></h5>
            <p>Ensure your URL follows the correct format (e.g., https://example.com).</p>
        </div>`;
        return;
    }

    confirmationMessage.innerHTML = "<div class='alert alert-info'>Fetching data...</div>";

    try {
        let response = await fetch(`/scraper/extract?url=${encodeURIComponent(urlInput)}`);
        let data = await response.json();

        if (response.ok) {
            confirmationMessage.innerHTML = "<div class='alert alert-success'>Data extracted successfully!</div>";

            resultsContainer.innerHTML = `
                <h3>Extracted Data</h3>
                <p><strong>Headings (h2):</strong> ${data.headings}</p>
                <p><strong>Images:</strong> ${data.images}</p>
            `;
        } else {
            confirmationMessage.innerHTML = `<div class='alert alert-danger'>Error: ${data.error || "Something went wrong!"}</div>`;
        }
    } catch (error) {
        confirmationMessage.innerHTML = `<div class='alert alert-danger'>Network error: ${error.message}</div>`;
    }
}

// Event listener for form submission
if (typeof window !== "undefined") {
    window.onload = function () {
        document.getElementById("urlForm").addEventListener("submit", handleUserUrl);
    };
}

// Export functions for Jest testing
module.exports = { validateURL, handleUserUrl };
