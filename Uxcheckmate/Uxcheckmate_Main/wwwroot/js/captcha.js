function onCaptchaSuccess(token) {
    document.getElementById('g-recaptcha-response').value = token;
    setTimeout(() => {
        const container = document.getElementById('captchaContainer');
        if (container) container.classList.add('d-none');
    }, 1500);
}

function onCaptchaLoadCallback() {
    const container = document.getElementById('captchaContainer');
    if (container) {
        const siteKey = container.getAttribute('data-sitekey');
        grecaptcha.render('captchaContainer', {
            sitekey: siteKey,
            callback: onCaptchaSuccess
        });
    }
}
