function addOption(select, str)
{
   let opt = document.createElement("option");
   opt.value = str;
   opt.innerHTML = str;
   select.appendChild(opt);
}
function addUniqueValue(select, uniqueSet)
{
   uniqueSet.values().forEach((n) => {
      addOption(select, n);
   });
}
function createFilter()
{
   const tables = document.getElementsByTagName("table");
   const heads = tables[0].getElementsByTagName("th");

   //add select for Name and Summary
   for (const head of heads) 
   {
      if (head.innerHTML === "Name")
      {
         let selector = document.createElement("select");
         selector.setAttribute("id", "selName");
         head.appendChild(selector);
         addOption(selector, "all");
      }
      if (head.innerHTML === "Summary")
      {
         let selector = document.createElement("select");
         selector.setAttribute("id", "selSummary");
         head.appendChild(selector);
         addOption(selector, "all");
      }
   }

   //get unique value of Name and Summary
   let uniqueName = new Set();
   let uniqueSummary = new Set();
   
   const rows = tables[0].querySelectorAll("tr");
   for (let i=1; i<rows.length; i++)
   {
      const columns = rows[i].querySelectorAll("td");
      //Name
      uniqueName.add(columns[1].innerHTML.split('.').pop()); 
      //Summary
      uniqueSummary.add(columns[2].innerHTML); 
   }
   
   //add options to Name and Summary selectors
   let selectName = document.getElementById("selName");
   let selectSummary = document.getElementById("selSummary");
   addUniqueValue(selectName, uniqueName);
   addUniqueValue(selectSummary, uniqueSummary);

   selectName.addEventListener("change", function(evt) { 
      filterRows(rows, 1, evt.target.value, selectName); 
   });
   selectSummary.addEventListener("change", function(evt) {
      filterRows(rows, 2, evt.target.value, selectSummary);
   });
}
function filterRows(rows, column, value, select)
{
   const selects = document.getElementsByTagName("select");
   for (let i = 0; i < selects.length; i++)
   {
      if (selects[i].id != select.id) 
      {
         selects[i].value = "all";
      }
   }
   for (let i = 0; i < rows.length; i++) 
   {
      let td = rows[i].getElementsByTagName("td")[column];
      if (td) 
      {
         let txtValue = td.textContent || td.innerText;
         if (select.id === "selName")
         {
            txtValue = txtValue.split('.').pop();
         }
         if (txtValue.toUpperCase() === value.toUpperCase() || value === "all")
         {
            rows[i].style.display = "";
         } 
         else
         {
            rows[i].style.display = "none";
         }
      }       
   }
}

createFilter();
