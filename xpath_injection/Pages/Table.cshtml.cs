using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace xpath_injection.Pages
{
    public class TableModel : PageModel
    {
        public IEnumerable<XElement> Users { get; private set; }
        public bool IsAdmin { get; private set; }
        public string SearchTerm { get; private set; }
        public bool ViewSensitiveData { get; private set; }

        [BindProperty]
        public string NewUsername { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public IActionResult OnGet(string searchTerm)
        {
            var userType = HttpContext.Session.GetString("UserType");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(username))
            {
                return RedirectToPage("Login");
            }

            IsAdmin = userType == "Admin";
            SearchTerm = searchTerm;

            LoadTableData();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                PerformSearch();
            }

            return Page();
        }

        private void PerformSearch()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            // Vulnerable XPath Query
            var unsafeXPathQuery = $"//User[Login/Username[contains(.,'{SearchTerm}')] or " +
                                   $"Personal/Role[contains(.,'{SearchTerm}')] or " +
                                   $"Personal/Phone[contains(.,'{SearchTerm}')] or " +
                                   $"Personal/Email[contains(.,'{SearchTerm}')] or " +
                                   $"Personal/SSN[contains(.,'{SearchTerm}')]]";
            Users = doc.XPathSelectElements(unsafeXPathQuery);
            // Check if any admin data was included
            ViewSensitiveData = Users.Any(user => user.Element("Personal")?.Element("Role")?.Value == "Admin");
        }

        private void LoadTableData()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);
            Users = doc.XPathSelectElements("//User");
        }

        public IActionResult OnPostAdd()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);
            var newUser = new XElement("User",
                new XElement("Login",
                    new XElement("Username", NewUsername),
                    new XElement("Password", NewPassword)),
                new XElement("Personal",
                    new XElement("Role", "User"),
                    new XElement("Phone", ""),
                    new XElement("Email", ""),
                    new XElement("SSN", "")));

            var userType = HttpContext.Session.GetString("UserType");
            if (userType == "Admin")
            {
                doc.XPathSelectElement("/Users/AdminUsers").Add(newUser);
            }
            else
            {
                doc.XPathSelectElement("/Users/NormalUsers").Add(newUser);
            }
            doc.Save(xmlFilePath);

            return RedirectToPage();
        }

        public IActionResult OnPostRemove(string username)
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);
            var userToRemove = doc.XPathSelectElement($"//User[Login/Username='{username}']");

            if (userToRemove != null)
            {
                userToRemove.Remove();
                doc.Save(xmlFilePath);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("Login");
        }
    }
}
