document.addEventListener("DOMContentLoaded", function () {
    const columns = document.querySelectorAll("#disclosureRowOne .col-md-4");

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                columns.forEach((col, index) => {
                    const box = col.querySelector('.disclosureBoxes');
                    setTimeout(() => {
                        box.classList.add("slide-in");
                    }, index * 500); // Stagger: 0ms → 500ms → 1000ms
                });

                observer.disconnect(); // Only animate once
            }
        });
    }, {
        threshold: 0.3
    });

    observer.observe(document.querySelector("#disclosureRowOne"));
});
