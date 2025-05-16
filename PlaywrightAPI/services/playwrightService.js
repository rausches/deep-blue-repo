import { chromium } from 'playwright';
import runAxeAudit from './axeService.js';

export async function runAnalysis(url, fullPage = false) {
    const browser = await chromium.launch({
        headless: true,
        timeout: 30000
    });

    const page = await browser.newPage();
    await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });

    const screenshotBuffer = await page.screenshot({ fullPage });
    const screenshotBase64 = screenshotBuffer.toString('base64');
    const html = await page.content();
    const axeResults = await runAxeAudit(page);

    // ✅ Inject DOM scraper function into the page
    await page.addScriptTag({ content: `${scrapeDomDataFunction}` });

    // ✅ Execute scraping logic in browser context
    const domData = await page.evaluate(() => window.scrapeDomData());

    await browser.close();

    // ✅ Merge all data into one object
    return {
        url,
        screenshotBase64,
        html,
        axeResults,
        ...domData // ← spread everything scraped from window.scrapeDomData
    };
}

// ✅ Define your scraping function here as a string (note: template literal)
const scrapeDomDataFunction = `
    window.scrapeDomData = () => {
        const allElements = [...document.querySelectorAll("*")];

        return {
            html: document.documentElement.outerHTML,
            textContent: document.body.innerText,
            headings: document.querySelectorAll("h1,h2,h3,h4,h5,h6").length,
            paragraphs: document.querySelectorAll("p").length,
            links: [...document.querySelectorAll("a")].map(a => a.href),
            fonts: [...new Set(
                allElements.map(e => getComputedStyle(e).fontFamily).filter(Boolean)
            )],
            hasFavicon: !!document.querySelector('link[rel~="icon"]'),
            faviconUrl: (() => {
                const link = document.querySelector('link[rel~="icon"]');
                return link ? link.href : null;
            })(),
            externalCssLinks: [...document.querySelectorAll('link[rel="stylesheet"]')].map(link => link.href),
            externalJsLinks: [...document.querySelectorAll('script[src]')].map(script => script.src),
            inlineCssList: [...document.querySelectorAll("style")].map(s => s.textContent.trim()),
            inlineJsList: [...document.querySelectorAll("script:not([src])")].map(s => s.textContent.trim()),
            scrollHeight: document.documentElement.scrollHeight,
            scrollWidth: document.documentElement.scrollWidth,
            viewportHeight: window.innerHeight,
            viewportWidth: window.innerWidth,
            viewportLabel: \`\${window.innerWidth}x\${window.innerHeight}\`
        };
    };
`;
