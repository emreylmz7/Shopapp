using shopapp.entity;

namespace shopapp.data.Abstract
{
    public interface ICategoryRepository:IRepository<Category>
    {
        Category GetByIdWithProducts(int categoryId);

        void DeleteFromCategory(int categoryId,int productId);
        
    }
}