using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using xpath_injection.Models;
using xpath_injection.Services;

namespace xpath_injection.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly XmlDataService _xmlDataService;

        public ProductsModel(XmlDataService xmlDataService)
        {
            _xmlDataService = xmlDataService;
        }

        public List<Product> Products { get; set; }

        public void OnGet()
        {
            Products = _xmlDataService.GetProducts();
        }
    }
}