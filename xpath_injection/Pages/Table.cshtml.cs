using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace xpath_injection.Pages
{
    public class TableModel : PageModel
    {
        public IEnumerable<XElement> Users { get; set; }
        public bool IsAdmin { get; set; }
        public XElement CurrentUser { get; set; }

        [BindProperty]
        public string NewUsername { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public IActionResult OnGet()
        {
            // Get the user type and username from the session
            var userType = HttpContext.Session.GetString("UserType");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(username))
            {
                // User is not logged in, redirect to the login page
                return RedirectToPage("Login");
            }

            IsAdmin = userType == "Admin";
            LoadTableData();

            if (!IsAdmin)
            {
                CurrentUser = Users.FirstOrDefault(u => u.Element("Login")?.Element("Username")?.Value == username);
            }

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            var newUser = new XElement("User",
                new XElement("Login",
                    new XElement("Username", NewUsername),
                    new XElement("Password", NewPassword)
                ),
                new XElement("Personal",
                    new XElement("Role", "User"),
                    new XElement("Phone", ""),
                    new XElement("Email", ""),
                    new XElement("SSN", "")
                )
            );

            // Get the user type from the session
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
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to the login page
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
        }
    }
}