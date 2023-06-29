using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.business.Abstract
{
    public interface IProductService:IValidator<Product>
    {
        Product GetByIdWithCategories(int id);
        Product GetProductDetails(string url);
        List<Product> GetProductByCategory(string name,int page,int pageSize);
        List<Product> GetSearchResult(string searchString);
        Task<Product> GetById(int id);
        List<Product> GetHomePageProducts();
        Task<List<Product>> GetAll();
        Task UpdateAsync(Product entityToUpdate,Product entity);
        Task<Product> CreateAsync(Product entity);
        bool Create(Product entity);
        void Update(Product entity);
        void Delete(Product entity);
        Task DeleteAsync(Product entity);
        int GetCountByCategory(string category);
        bool Update(Product entity, int[] categoryIds);
    }
}