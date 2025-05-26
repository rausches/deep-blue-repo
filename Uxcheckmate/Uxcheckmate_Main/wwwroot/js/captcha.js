function onCaptchaSuccess(token) {
    fetch('/Home/ValidateCaptcha', {
        method: 'POST',
        headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        body: 'captchaToken=' + encodeURIComponent(token)
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            document.getElementById('urlForm').submit();
        } else {
            showError(data.error || 'CAPTCHA failed.');
        }
    });
}

function onCaptchaLoadCallback() {
    console.log("onCaptchaLoadCallback called");
    const container = document.getElementById('captchaContainer');
    if (container) {
        const siteKey = container.getAttribute('data-sitekey');
        console.log("Site key:", siteKey);
        grecaptcha.render('captchaContainer', {
            sitekey: siteKey,
            callback: onCaptchaSuccess
        });
    } else {
        console.log("captchaContainer not found!");
    }
}