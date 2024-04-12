using System.Xml.Serialization;
using xpath_injection.Models;

namespace xpath_injection.Services
{
    public class XmlDataService
    {
        private readonly string _filepath = Path.Combine(Directory.GetCurrentDirectory(), "data.xml");

        public List<Product> GetProducts()
        {
            var serializer = new XmlSerializer(typeof(List<Product>), new XmlRootAttribute("Products") { Namespace = "" });
            using (var reader = new StreamReader(_filepath))
            {
                var products = (List<Product>)serializer.Deserialize(reader);
                return products;
            }
        }

        public void SaveProducts(List<Product> products)
        {
            var serializer = new XmlSerializer(typeof(List<Product>));
            using (var writer = new StreamWriter(_filepath))
            {
                serializer.Serialize(writer, products);
            }
        }
    }
}