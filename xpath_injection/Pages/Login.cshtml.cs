using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;
using System.Xml.XPath;

namespace xpath_injection.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            var xmlFilePath = "TableData.xml";
            var doc = XDocument.Load(xmlFilePath);

            try
            {
                var user = doc.XPathSelectElement($"//User[Login/Username='{Username}' and Login/Password='{Password}']");

                if (user != null)
                {
                    // Login successful, store the user type and username in the session
                    var userType = user.Parent.Name.LocalName == "AdminUsers" ? "Admin" : "Normal";
                    HttpContext.Session.SetString("UserType", userType);
                    HttpContext.Session.SetString("Username", Username);
                    return RedirectToPage("Table");
                }
                else
                {
                    // Login failed, display an error message
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }
            }
            catch (System.Xml.XPath.XPathException ex)
            {
                // Display the backend error message
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}