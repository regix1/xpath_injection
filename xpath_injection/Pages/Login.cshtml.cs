using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace xpath_injection.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            var xmlFilePath = "Users.xml";
            var doc = XDocument.Load(xmlFilePath);

            var user = doc.Root.Elements("User")
                .FirstOrDefault(u => u.Element("Username").Value == Username &&
                                     u.Element("Password").Value == Password);

            if (user != null)
            {
                // Login successful, redirect to the table page
                return RedirectToPage("Table");
            }
            else
            {
                // Login failed, display an error message
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }
        }
    }
}
