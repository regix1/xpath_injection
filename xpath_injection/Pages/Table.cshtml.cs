using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;
using System.Xml.XPath;

namespace xpath_injection.Pages
{
    public class TableModel : PageModel
    {
        public IEnumerable<XElement> Users { get; set; }

        public IEnumerable<XElement> Login { get; set; }

        public IEnumerable<XElement> Personal { get; set; }

        public bool IsAdmin { get; set; }

        [BindProperty]
        public string NewUsername { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string NewRole { get; set; }

        [BindProperty]
        public string NewPhone { get; set; }

        [BindProperty]
        public string NewEmail { get; set; }

        [BindProperty]
        public string NewSSN { get; set; }

        public IActionResult OnGet()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (string.IsNullOrEmpty(userType))
            {
                // User is not logged in, redirect to the login page
                return RedirectToPage("Login");
            }

            IsAdmin = userType == "Admin";
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
                    new XElement("Password", NewPassword),
                new XElement("Personal",
                    new XElement("Role"),
                    new XElement("Phone"),
                    new XElement("Email"),
                    new XElement("SSN")))
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

            var userToRemove = doc.XPathSelectElement($"//User//Login[Username='{username}']");

            if (userToRemove != null)
            {
                userToRemove.Remove();
                doc.Save(xmlFilePath);
            }

            return RedirectToPage();
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