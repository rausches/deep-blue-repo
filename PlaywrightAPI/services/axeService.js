export default async function runAxeAudit(page) {
    await page.addScriptTag({
        url: 'https://cdnjs.cloudflare.com/ajax/libs/axe-core/4.8.2/axe.min.js'
    });

    return await page.evaluate(async () => {
        return await window.axe.run();
    });
}
