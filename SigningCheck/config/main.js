function addOption(select, str) {
    let opt = document.createElement("option");
    opt.value = str;
    opt.innerHTML = str;
    select.appendChild(opt);
}
function addUniqueValue(select, uniqueSet) {
    uniqueSet.forEach((n) => {
        addOption(select, n);
    });
}
function removeAllChildren(element) {
    while (element.hasChildNodes()) {
        element.removeChild(element.firstChild);
    }
}
function filterRows() {
    const tables = document.getElementsByTagName("table");
    const rows = tables[0].querySelectorAll("tr");
    const selectName = document.getElementById("selName");
    const selectSummary = document.getElementById("selSummary");
    for (let i = 1; i < rows.length; i++) {
        const tdName = rows[i].getElementsByTagName("td")[1];
        const tdSummary = rows[i].getElementsByTagName("td")[2];
        let name = tdName.innerText.split('.').pop();
        let summary = tdSummary.innerText;
        if (name.toUpperCase() === selectName.value.toUpperCase() &&
            summary.toUpperCase() === selectSummary.value.toUpperCase()
        ) {
            rows[i].style.display = "";
        }
        else if (selectName.value === "all" &&
            summary.toUpperCase() === selectSummary.value.toUpperCase()) {
            rows[i].style.display = "";
        }
        else if (name.toUpperCase() === selectName.value.toUpperCase() &&
            selectSummary.value === "all"
        ) {
            rows[i].style.display = "";
        }
        else if (selectName.value === "all" && selectSummary.value === "all") {
            rows[i].style.display = "";
        }
        else {
            rows[i].style.display = "none";
        }
    }
}
function setUniqueOptions() {
    //get unique value of Name and Summary
    let uniqueName = new Set();
    let uniqueSummary = new Set();
    const tables = document.getElementsByTagName("table");
    const rows = tables[0].querySelectorAll("tr");
    for (let i = 1; i < rows.length; i++) {
        if (rows[i].style.display == "none") {
            continue;
        }
        const columns = rows[i].querySelectorAll("td");
        //Name
        uniqueName.add(columns[1].innerHTML.split('.').pop());
        //Summary
        uniqueSummary.add(columns[2].innerHTML);
    }

    //add options to Name and Summary selectors
    const selectName = document.getElementById("selName");
    const selectSummary = document.getElementById("selSummary");
    addUniqueValue(selectName, uniqueName);
    addUniqueValue(selectSummary, uniqueSummary);
}
function DoFilter() {
    const tables = document.getElementsByTagName("table");
    const heads = tables[0].getElementsByTagName("th");

    //add select for Name and Summary
    for (const head of heads) {
        if (head.innerHTML === "Name") {
            let select = document.createElement("select");
            select.setAttribute("id", "selName");
            select.setAttribute("title", "selName");
            select.addEventListener("change", filterRows);
            head.appendChild(select);
            addOption(select, "all");
        }
        if (head.innerHTML === "Summary") {
            let select = document.createElement("select");
            select.setAttribute("id", "selSummary");
            select.setAttribute("title", "selSummary");
            select.addEventListener("change", filterRows);
            head.appendChild(select);
            addOption(select, "all");
        }
    }
    setUniqueOptions();
    addClassForCols("sticky", 3);
    updateStickyColumns();
}
function updateStickyColumns() {
    const rows = document.querySelectorAll('tr');
    rows.forEach(row => {
        let leftOffset = 0;
        const cells = row.querySelectorAll('.sticky');
        cells.forEach(cell => {
            cell.style.left = leftOffset + 'px';
            leftOffset += Math.floor(cell.getBoundingClientRect().width) - 0.5;
        });
    });
}
function addClassForCols(c, nCol) {
    const rows = document.querySelectorAll('tr');
    rows.forEach(tr => {
        for (let i = 0; i < nCol; i++) {
            tr.cells[i].className = c;
        }
    });
}
function observeColumnChanges() {
    const table = document.querySelector('table');
    const columns = table.querySelectorAll('th');

    const resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            updateStickyColumns();
        }
    });

    columns.forEach(column => {
        resizeObserver.observe(column);
    });
}
document.addEventListener('DOMContentLoaded', function () {
    observeColumnChanges();
    DoFilter();
});
