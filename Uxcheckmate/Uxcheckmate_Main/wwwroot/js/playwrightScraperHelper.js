// Async function to scrape the content of all external CSS stylesheets
window.scrapeExternalCss = async () => {
    // Collect all <link rel="stylesheet"> elements and extract their href URLs
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"]')).map(l => l.href);

    // Fetch the content of each CSS file in parallel using Promise.allSettled
    const results = await Promise.allSettled(
        links.map(async href => {
            try {
                // Attempt to fetch the CSS file
                const res = await fetch(href);
                // Return the raw text content
                return await res.text();
            } catch {
                return '';
            }
        })
    );

    // Return an array of CSS contents (empty string if fetch failed)
    return results.map(r => r.status === 'fulfilled' ? r.value : '');
};

// Define a global async function to scrape the content of all external JavaScript files
window.scrapeExternalJs = async () => {
    // Collect all <script> tags with a src attribute and extract their URLs
    const scripts = Array.from(document.querySelectorAll('script[src]')).map(s => s.src);

    // Fetch the content of each JS file in parallel using Promise.allSettled
    const results = await Promise.allSettled(
        scripts.map(async src => {
            try {
                // Attempt to fetch the JS file
                const res = await fetch(src);
                // Return the raw JavaScript content
                return await res.text();
            } catch {
                return '';                        
            }
        })
    );

    // Return an array of JS contents or empty string if fetch failed
    return results.map(r => r.status === 'fulfilled' ? r.value : '');
};
