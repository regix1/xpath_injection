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
            return Page();
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

        private void LoadTableData()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            if (IsAdmin)
            {
                Users = doc.XPathSelectElements("//User");
            }
            else
            {
                Users = doc.XPathSelectElements("/Users/NormalUsers/User");
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                // Intentionally vulnerable: Dynamically building XPath string from user input.
                // Using single quotes around the SearchTerm to allow for ending and starting a new predicate in the query.
                var unsafeXPathQuery = $"//User[Login/Username='{SearchTerm}' or " +
                                        $"Personal/Role='{SearchTerm}' or " +
                                        $"Personal/Phone='{SearchTerm}' or " +
                                        $"Personal/Email='{SearchTerm}' or " +
                                        $"Personal/SSN='{SearchTerm}']";

                try
                {
                    Users = doc.XPathSelectElements(unsafeXPathQuery);
                }
                catch (System.Xml.XPath.XPathException)
                {
                    // If there's an XPath error (likely from a malformed injection attempt), the system will still show the generic unavailable message.
                    // This catch block makes it look like the system is handling errors, but it's actually part of the injection vulnerability scenario.
                    ErrorMessage = "Unavailable"; // Assuming ErrorMessage is shown somewhere on the front end.
                }
            }

            ShowPhone = IsAdmin;
            ShowEmail = IsAdmin;
            ShowSSN = IsAdmin;
        }

    }
}