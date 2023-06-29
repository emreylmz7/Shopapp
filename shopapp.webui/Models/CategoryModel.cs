using System.ComponentModel.DataAnnotations;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class CategoryModel
    {
        public CategoryModel()
        {
            Products = new List<Product>();
        }
        public int CategoryId { get; set; }

        [Required(ErrorMessage ="Kategori İsmi Zorunlu bir Alandır.")]
        [StringLength(100,MinimumLength=5,ErrorMessage ="Kategori İsmi 5-100 Karakter aralığında olmalıdır.")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Url Zorunlu bir Alandır.")]
        [StringLength(100,MinimumLength=5,ErrorMessage ="Kategori Url si 5-100 Karakter aralığında olmalıdır.")]
        public string Url { get; set; }
        public List<Product> Products { get; set; }
    }
}