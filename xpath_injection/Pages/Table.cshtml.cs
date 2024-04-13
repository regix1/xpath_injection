using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace xpath_injection.Pages
{
    public class TableModel : PageModel
    {
        public IEnumerable<XElement> TableData { get; set; }

        [BindProperty]
        public string NewUsername { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public void OnGet()
        {
            LoadTableData();
        }

        public IActionResult OnPostAdd()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            var newRow = new XElement("Row",
                new XElement("Username", NewUsername),
                new XElement("Password", NewPassword)
            );

            doc.Root.Add(newRow);
            doc.Save(xmlFilePath);

            return RedirectToPage();
        }

        public IActionResult OnPostRemove(string username)
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            var rowToRemove = doc.Root.Elements("Row")
                .FirstOrDefault(r => r.Element("Username").Value == username);

            if (rowToRemove != null)
            {
                rowToRemove.Remove();
                doc.Save(xmlFilePath);
            }

            return RedirectToPage();
        }

        private void LoadTableData()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);
            TableData = doc.Root.Elements("Row");
        }
    }
}