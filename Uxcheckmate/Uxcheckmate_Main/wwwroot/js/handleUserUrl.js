// JavaScript file to handle user input URL

function validateURL() {
    event.preventDefault();
    var urlInput = document.getElementById('urlInput').value;
    var urlRegex = /^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w-]*)*$/;

    var confirmationMessage = document.getElementById('confirmationMessage');

    if (urlRegex.test(urlInput)) {
        confirmationMessage.innerHTML = "<div class='alert alert-success'>Your URL has been successfully submitted!</div>";
        return true;
    } else {
        confirmationMessage.innerHTML = `
        <div class='alert alert-danger'>
            <h5><strong>Invalid URL! A non-valid URL can occur if:</strong></h5>
            <p><strong>Invalid characters:</strong> http://example[dot]com (no brackets or spaces)</p>
            <p><strong>Incorrect format:</strong> http:/example.com (missing one slash)</p>
            <p><strong>Missing Domain Extension:</strong> http://example (should have .com, .org, .edu, etc.)</p>
            <p><strong>Spaces:</strong> http://example .com (spaces not allowed)</p>
        </div>`;
        return false;
    }
}