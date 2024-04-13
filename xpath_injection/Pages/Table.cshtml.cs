using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace xpath_injection.Pages
{
    public class TableModel : PageModel
    {
        public IEnumerable<XElement> TableData { get; set; }

        public void OnGet()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);
            TableData = doc.Root.Elements("Row");
        }
    }
}