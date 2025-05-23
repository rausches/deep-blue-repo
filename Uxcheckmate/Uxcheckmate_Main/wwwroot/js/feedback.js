function updateCharCount() {
    const textarea = document.getElementById("feedbackInput");
    const count = textarea.value.length;
    document.getElementById("charCount").textContent = `${count} / 255 characters used`;
}
document.addEventListener("DOMContentLoaded", function () {
    updateCharCount();
});