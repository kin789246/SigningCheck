﻿function addOption(select, str) {
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
        let displayName = false;
        let displaySummary = false;
        if (name.toUpperCase() === selectName.value.toUpperCase() ||
            selectName.value === "all") {
            displayName = true;
        }
        if (summary.toUpperCase() === selectSummary.value.toUpperCase() ||
            selectSummary.value === "all") {
            displaySummary = true;
        }
        if (selectSummary.value === "non-WHQL" &&
            summary.toUpperCase() != "WHQL SIGNED") {
            displaySummary = true;
        }
        if (displayName && displaySummary) {
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
function toggleColumn(columnName, show) {
    const elements = document.querySelectorAll('.' + columnName);
    elements.forEach(element => {
        element.style.display = show ? '' : 'none';
    });
}
function addCheckbox(columnName, columnClass, isCheck) {
    const displayOptions = document.getElementById('displayOptions');
    const label = document.createElement('label');
    const checkbox = document.createElement('input');
    checkbox.type = 'checkbox';
    checkbox.id = columnClass;
    checkbox.className = 'toggle-column';
    checkbox.dataset.column = columnName + '-column';
    checkbox.checked = isCheck;
    checkbox.addEventListener('change', function () {
        toggleColumn(columnName + '-column', this.checked);
    });
    label.appendChild(checkbox);
    label.appendChild(document.createTextNode(` ${columnName}`));
    displayOptions.appendChild(label);
}
function updateStickyPosition() {
    const rows = document.getElementsByTagName("tr");
    const divFilter = document.getElementById("displayOptions");
    const divFilterHeight = divFilter.getBoundingClientRect().height;
    rows[0].style.top = divFilterHeight + 'px';
//    let leftOffset = 0;
//    divFilter.childNodes.forEach(label => {
//        let labelW = label.getBoundingClientRect().width;
//        label.style.left = leftOffset + 'px';
//        leftOffset += labelW;
//    });
}
function addFilter() {
    let head = document.getElementById("Name-column");
    let select = document.createElement("select");
    select.setAttribute("id", "selName");
    select.setAttribute("title", "selName");
    select.addEventListener("change", filterRows);
    head.appendChild(select);
    addOption(select, "all");

    head = document.getElementById("Summary-column");
    select = document.createElement("select");
    select.setAttribute("id", "selSummary");
    select.setAttribute("title", "selSummary");
    select.addEventListener("change", filterRows);
    head.appendChild(select);
    addOption(select, "all");
    addOption(select, "non-WHQL");

    setUniqueOptions();
}
// Add resizers to headers
function addResizers() {
    const headers = document.querySelectorAll('th');
    headers.forEach(header => {
        const resizer = document.createElement('div');
        resizer.className = 'resizer';
        header.appendChild(resizer);
        initColumnResize(header, resizer);
    });
}
function initColumnResize(col, resizer) {
    let startX = 0;
    let width = 0;
    const mouseDownHandler = function (e) {
        startX = e.clientX;
        const styles = window.getComputedStyle(col);
        width = parseInt(styles.width, 10);
        document.addEventListener('mousemove', resizeColumn);
        document.addEventListener('mouseup', stopResize);
    };
    const resizeColumn = function (e) {
        const dx = e.clientX - startX;
        col.style.width = `${width + dx}px`;
    };
    const stopResize = function () {
        document.removeEventListener('mousemove', resizeColumn);
        document.removeEventListener('mouseup', stopResize);
    };
    resizer.addEventListener('mousedown', mouseDownHandler);
}
function createTips() {
    const tooltip = document.createElement('div');
    tooltip.className = 'tooltip';
    document.body.appendChild(tooltip);
    document.querySelectorAll('td').forEach(cell => {
        if (cell.classList.length != 0) {
            cell.addEventListener('mouseenter', function (event) {
                const content = cell.textContent.trim();
                if (content.length > 0) {
                    tooltip.textContent = content;
                    tooltip.style.width = "";
                    tooltip.style.display = 'block';
                    const rect = cell.getBoundingClientRect();
                        tooltip.style.width = `${rect.width - 14}px`;
                    let rectTip = tooltip.getBoundingClientRect();
                    let offsetX = 0;
                    if (rect.width > rectTip.width) {
                        offsetX = (rect.width - rectTip.width) / 2;
                    }
                    let leftVal = rect.left + window.scrollX + offsetX;
                    if (rect.right > window.innerWidth) {
                        leftVal -= rect.right - window.innerWidth + 36;
                    }
                    if (rect.left < 0) {
                        leftVal = window.scrollX;
                    }
                    tooltip.style.left = `${leftVal}px`;
                    tooltip.style.top = `${rect.top + window.scrollY}px`;
                }
            });
            cell.addEventListener('mouseleave', function () {
                tooltip.style.display = 'none';
            });
        }
    });
}
function setColumnWidth() {
    const headers = document.querySelectorAll('th');
    const columnsToToggle = ['Name', 'Summary', 'Path', 'Other', 'Signers'];
    headers.forEach(header => {
        fitWidth = true;
        for (col of columnsToToggle) {
            if (header.textContent.includes(col)) {
                fitWidth = false;
                break;
            }
        }
        if (fitWidth) {
            const textLength = header.textContent.trim().length - 1;
            header.style.width = `${textLength}em`;
        }
        if (header.textContent.includes("Expiry")) {
            const textLength = header.textContent.trim().length;
            header.style.width = `${textLength}em`;
        }
    });
}
document.addEventListener('DOMContentLoaded', function () {
    addFilter();
    addResizers();
    createTips();
    setColumnWidth();
    // Define the columns to be toggled
    const columnsToToggle = ['Path', 'Other', 'Signers'];
    columnsToToggle.forEach(columnName => {
        const columnClass = 'checkbox-' + columnName;
        if (columnName === "Path") {
            addCheckbox(columnName, columnClass, true);
        }
        else {
            addCheckbox(columnName, columnClass, false);
        }
    });

    // Initialize the columns visibility
    document.querySelectorAll('.toggle-column').forEach(checkbox => {
        toggleColumn(checkbox.dataset.column, checkbox.checked);
    });

    updateStickyPosition();
});
