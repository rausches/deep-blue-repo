// JavaScript file to handle user input URL
// Function to validate the URL format
function validateURL(urlInput) {
    var urlRegex = /^(https?:\/\/)([\w-]+\.)+[\w-]+(\/[\w-]*)*$/;
    return urlRegex.test(urlInput);
}

// Function to handle user input URL
function handleUserUrl(event) {

    event.preventDefault();
    var urlInput = document.getElementById('urlInput').value.trim();
    var responseMessage = document.getElementById('responseMessage');
    var loaderWrapper = document.getElementById('loaderWrapper');

    // Check if the URL is empty
    if (urlInput === "") {
        responseMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Empty URL!</strong></h5>
            <p>Please enter a URL before</p>
        </div>`;
        return false;
    }

    // Check if the URL has spaces
    if (urlInput.includes(' ')) {
        responseMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Spaces are not allowed in URLs.</strong></h5>
            <p>Please check your URL for any spaces</strong></p>
        </div>`;
        return false;
    }

    //Check if domain extension is missing
    if (!/\.[a-z]{2,}($|\/)/.test(urlInput)) {
        responseMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Missing or incorrect domain extension.</strong></h5>
            <p>Please ensure your URL has a valid domain extension (e.g., .com, .org, .edu, etc.)</p>
        </div>`;
        return false;
    }

    // Check if the URL has domain extension http/https
    if (!urlInput.includes('http://') && !urlInput.includes('https://') ) {
        responseMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Invalid URL! A non-valid URL can occur if:</strong></h5>
            <p><strong>Missing Protocol:</strong> example.com (should have http:// or https://)</p>
            <p><strong>Incorrect format:</strong> http:/example.com (missing one slash)</p>
        </div>`;
        return false; 
    }
    
    // Hide the confirmation message and show loader
    responseMessage.innerHTML = "";
    loaderWrapper.style.display = 'flex';

    // URL is valid, send to the server
    document.getElementById('urlForm').submit();    

    closePopup();

}

// Function to close the custom popup message
function closePopup() {
    var customPopup = document.getElementById('customPopupMessage');
    customPopup.style.display = 'none';  // Hide the popup message
}

// Check if the window object is available
if (typeof window !== 'undefined') {
    window.onload = function () {
        document.getElementById('urlForm').addEventListener('submit', handleUserUrl);
    };
}

// Export the functions to be used in other files (Jest_Tests/handleUserUrl.test.js)
module.exports = { validateURL, handleUserUrl };