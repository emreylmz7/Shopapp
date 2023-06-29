using shopapp.entity;

namespace shopapp.business.Abstract
{
    public interface ICategoryService:IValidator<Category>
    {
        Task<Category> GetById(int id);
        Task<List<Category>> GetAll();
        Category GetByIdWithProducts(int categoryId);
        void Create(Category entity);
        void Update(Category entity);
        void Delete(Category entity);
        void DeleteFromCategory(int categoryId,int productId);
        
    }
}