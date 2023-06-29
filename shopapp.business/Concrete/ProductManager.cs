using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.data.Abstract;

namespace shopapp.business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IUnitOfWork _unitofwork;
        public ProductManager(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;    
        }

        public bool Create(Product entity)
        {
            if(Validation(entity))
            {
                _unitofwork.Products.Create(entity);
                _unitofwork.Save();
                return true;
            }
            return false;
        }

        public void Delete(Product entity)
        {
            //İş kurallarını uygula
            _unitofwork.Products.Delete(entity);
            _unitofwork.Save();
        }

        public async Task<List<Product>> GetAll()
        {
            return await _unitofwork.Products.GetAll();
        }

        public async Task<Product> GetById(int id)
        {
            return await _unitofwork.Products.GetById(id);
        }

        public Product GetByIdWithCategories(int id)
        {
            return _unitofwork.Products.GetByIdWithCategories(id);
        }

        public int GetCountByCategory(string category)
        {
            return _unitofwork.Products.GetCountByCategory(category);
        }

        public List<Product> GetHomePageProducts()
        {
            return _unitofwork.Products.GetHomePageProducts();
        }

        public List<Product> GetProductByCategory(string name,int page,int pageSize)
        {
            return _unitofwork.Products.GetProductByCategory(name,page,pageSize); 
        }

        public Product GetProductDetails(string url)
        {
            return _unitofwork.Products.GetProductDetails(url);
        }

        public List<Product> GetSearchResult(string searchString)
        {
            return _unitofwork.Products.GetSearchResult(searchString);
        }

        public void Update(Product entity)
        {
            _unitofwork.Products.Update(entity);
            _unitofwork.Save();
        }

        public bool Update(Product entity, int[] categoryIds)
        {
            if(Validation(entity))
            {
                if (categoryIds.Length==0)
                {
                    ErrorMessage += "Ürün için en az bir Kategori seçmelisiniz.";
                    return false;   
                }
                _unitofwork.Products.Update(entity,categoryIds);
                _unitofwork.Save();
                return true;
            }
            return false;
        }





        public string ErrorMessage { get; set;}
        public bool Validation(Product entity)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(entity.Name))
            {
                ErrorMessage += "Ürün İsmi Girmelisiniz. \n";
                isValid = false;
                
            }
            if (entity.Price<0)
            {
                ErrorMessage += "Fiyat alanı 0 dan küçük olamaz. \n";
                isValid = false;
            }

            return isValid;
        }

        public async Task UpdateAsync(Product entityToUpdate, Product entity)
        {
            entityToUpdate.Name = entity.Name;
            entityToUpdate.Price = entity.Price;
            entityToUpdate.Description = entity.Description;

            await _unitofwork.SaveAsync();
        }

        public async Task DeleteAsync(Product entity)
        {
            _unitofwork.Products.Delete(entity);
            await _unitofwork.SaveAsync();
        }

        public async Task<Product> CreateAsync(Product entity)
        {
            await _unitofwork.Products.CreateAsync(entity);
            await _unitofwork.SaveAsync();
            return entity;
        }
    }
}