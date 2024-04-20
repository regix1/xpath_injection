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
        public string ErrorMessage { get; private set; }
        public bool ShowPhone { get; private set; }
        public bool ShowEmail { get; private set; }
        public bool ShowSSN { get; private set; }

        [BindProperty]
        public string NewUsername { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        private readonly string xmlFilePath = "TableData.xml";

        public IActionResult OnGet(string searchTerm)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("Login");
            }

            IsAdmin = IsAdminUser(username);
            SearchTerm = searchTerm;
            LoadTableData();
            return Page();
        }

        private bool IsAdminUser(string username)
        {
            var doc = XDocument.Load(xmlFilePath);
            // Vulnerable XPath Query: No single quote escaping
            var query = $"//User[Login/Username='{username}' and Personal/Privilege='admin']";
            return doc.XPathSelectElement(query) != null;
        }

        public IActionResult OnPostAdd()
        {
            var doc = XDocument.Load(xmlFilePath);

            var newUser = new XElement("User",
                new XElement("Login",
                    new XElement("Username", NewUsername),
                    new XElement("Password", NewPassword)),
                new XElement("Personal",
                    new XElement("Role", "User"),
                    new XElement("Phone", ""),
                    new XElement("Email", ""),
                    new XElement("SSN", ""),
                    new XElement("Privilege", "user")));

            doc.Root.Add(newUser);
            doc.Save(xmlFilePath);
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(string username)
        {
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

        private void LoadTableData()
        {
            var doc = XDocument.Load(xmlFilePath);
            Users = IsAdmin ? doc.XPathSelectElements("//User")
                            : doc.XPathSelectElements("//User[not(Personal/Privilege='admin')]");

            ShowPhone = IsAdmin;
            ShowEmail = IsAdmin;
            ShowSSN = IsAdmin;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                // Intentionally vulnerable XPath Query: No single quote escaping
                var unsafeXPathQuery = $"//User[Login/Username='{SearchTerm}' or " +
                                        $"Personal/Role='{SearchTerm}' or " +
                                        $"Personal/Phone='{SearchTerm}' or " +
                                        $"Personal/Email='{SearchTerm}' or " +
                                        $"Personal/SSN='{SearchTerm}' or " +
                                        $"Personal/Privilege='{SearchTerm}']";

                try
                {
                    Users = doc.XPathSelectElements(unsafeXPathQuery);
                }
                catch (System.Xml.XPath.XPathException ex)
                {
                    ErrorMessage = $"Error in XPath Query: {ex.Message}";
                }
            }
        }
    }
}