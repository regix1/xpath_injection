using System.Xml.Serialization;

namespace xpath_injection.Models
{
    [XmlRoot("Product", Namespace = "")]
    public class Product
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }
    }
}