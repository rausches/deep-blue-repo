console.log("reportResults.js loaded!");

/*
===========================================================================================================
JS for view more on flagged item results. If making changes to this file please verify it still works!
===========================================================================================================
*/

function toggleSelector(id) {
    // Grab references to the short text, full text, and link
    const shortEl = document.getElementById(`selectorShort-${id}`);
    const fullEl = document.getElementById(`selectorFull-${id}`);
    const linkEl = document.getElementById(`toggleLink-${id}`);

    // If short is hidden, switch back to short
    if (shortEl.style.display === 'none') {
        shortEl.style.display = 'inline';
        fullEl.style.display = 'none';
        linkEl.textContent = 'View More';
    } 
    // Otherwise show the full text
    else {
        shortEl.style.display = 'none';
        fullEl.style.display = 'inline';
        linkEl.textContent = 'View Less';
    }
}


