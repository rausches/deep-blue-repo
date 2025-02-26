document.addEventListener("DOMContentLoaded", function () {
    const exportButton = document.getElementById("exportPdfButton");
    const url = document.getElementById("analyzed-url").value;

    if (exportButton) {
        exportButton.addEventListener("click", function () {
            if (!url) {
                alert("No valid URL provided.");
                return;
            }

            fetch(`/scraper/export-pdf?url=${encodeURIComponent(url)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Failed to generate PDF.");
                    }
                    return response.blob();
                })
                .then(blob => {
                    const blobUrl = window.URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = blobUrl;
                    a.download = "UX_Report.pdf";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                })
                .catch(error => {
                    console.error("Error generating PDF:", error);
                    alert("Error generating PDF. Please try again.");
                });
        });
    }
});
