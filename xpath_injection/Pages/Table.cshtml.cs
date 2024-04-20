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
        public IEnumerable<UserModel> Users { get; private set; }
        public bool IsAdmin { get; private set; }
        public string SearchTerm { get; private set; }
        public string ErrorMessage { get; private set; }

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

            // Assume that the user is not an admin initially.
            IsAdmin = false;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var unsafeXPathQuery = $"//User[Login/Username='{SearchTerm}' or " +
                                       $"Personal/Role='{SearchTerm}' or " +
                                       $"Personal/Phone='{SearchTerm}' or " +
                                       $"Personal/Email='{SearchTerm}' or " +
                                       $"Personal/SSN='{SearchTerm}']";

                try
                {
                    // Fetch users based on the search term.
                    var foundUsers = doc.XPathSelectElements(unsafeXPathQuery);

                    // Determine if any of the found users are admins.
                    // WARNING: This is for demonstration only. It is extremely unsafe and should never be done in production.
                    foreach (var user in foundUsers)
                    {
                        var privilegeElement = user.XPathSelectElement("Personal/Privilege");
                        if (privilegeElement != null && privilegeElement.Value == "admin")
                        {
                            IsAdmin = true;
                            break; // If any admin is found in the search results, break the loop.
                        }
                    }

                    // Now we select the user information based on the IsAdmin flag.
                    Users = foundUsers.Select(u => new UserModel
                    {
                        Username = u.Element("Login")?.Element("Username")?.Value,
                        Role = u.Element("Personal")?.Element("Role")?.Value,
                        Phone = IsAdmin ? u.Element("Personal")?.Element("Phone")?.Value : "Unavailable",
                        Email = IsAdmin ? u.Element("Personal")?.Element("Email")?.Value : "Unavailable",
                        SSN = IsAdmin ? u.Element("Personal")?.Element("SSN")?.Value : "Unavailable"
                    });
                }
                catch (System.Xml.XPath.XPathException ex)
                {
                    ErrorMessage = $"Error in XPath Query: {ex.Message}";
                }
            }
            else
            {
                // If there is no search term, we do not display any users.
                Users = Enumerable.Empty<UserModel>();
            }
        }
    }

        public class UserModel
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string SSN { get; set; }
    }
}