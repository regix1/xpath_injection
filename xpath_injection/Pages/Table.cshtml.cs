using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            System.Diagnostics.Debug.WriteLine(IsAdmin);
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

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var unsafeXPathQuery = $"//User[Login/Username='{SearchTerm}' or " +
                                        $"Personal/Role='{SearchTerm}' or " +
                                        $"Personal/Phone='{SearchTerm}' or " +
                                        $"Personal/Email='{SearchTerm}' or " +
                                        $"Personal/SSN='{SearchTerm}']";



                if (CheckCondition($"//User[Login/Username='{SearchTerm}']") ||
                    CheckCondition($"//User[Personal/Role='{SearchTerm}']") ||
                    CheckCondition($"//User[Personal/Phone='{SearchTerm}']") ||
                    CheckCondition($"//User[Personal/Email='{SearchTerm}']") ||
                    CheckCondition($"//User[Personal/SSN='{SearchTerm}']"))
                {
                    IsAdmin = !IsAdmin;
                }


                try
                {
                    Users = doc.XPathSelectElements(unsafeXPathQuery)
                        .Select(u => new UserModel
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
                Users = IsAdmin ? doc.XPathSelectElements("//User")
                                    .Select(u => new UserModel
                                    {
                                        Username = u.Element("Login")?.Element("Username")?.Value,
                                        Role = u.Element("Personal")?.Element("Role")?.Value,
                                        Phone = u.Element("Personal")?.Element("Phone")?.Value,
                                        Email = u.Element("Personal")?.Element("Email")?.Value,
                                        SSN = u.Element("Personal")?.Element("SSN")?.Value
                                    })
                                  : doc.XPathSelectElements($"//User[not(Personal/Privilege='admin')]")
                                    .Select(u => new UserModel
                                    {
                                        Username = u.Element("Login")?.Element("Username")?.Value,
                                        Role = u.Element("Personal")?.Element("Role")?.Value,
                                        Phone = "Unavailable",
                                        Email = "Unavailable",
                                        SSN = "Unavailable"
                                    });
            }
        }
        bool CheckCondition(string xpath)
        {
            var nodeCount = CountMatchingNodes(xpath);

            return nodeCount > 1;
        }

        int CountMatchingNodes(string xpath)
        {
            try
            {
                var doc = XDocument.Load(xmlFilePath);
                var matchingElements = doc.XPathSelectElements(xpath);
                var count = matchingElements.Count(u =>
                    (u.Element("Login")?.Element("Username") != null) &&
                    (u.Element("Login")?.Element("Password") != null) &&
                    (u.Element("Personal")?.Element("Phone") != null) &&
                    (u.Element("Personal")?.Element("Email") != null) &&
                    (u.Element("Personal")?.Element("SSN") != null)
                );

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                return 0;
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