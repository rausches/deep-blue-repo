function onCaptchaCompleted(token) {
    document.getElementById('CaptchaToken').value = token;
    setTimeout(function() {
        var captchaDiv = document.querySelector('.g-recaptcha');
        if (captchaDiv) {
            captchaDiv.style.display = 'none';
        }
    }, 1500);
}