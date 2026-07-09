using WebApp.Model.Product;

namespace WebApp.Service.Product
{
    public interface IProductService
    {
        Task<Data.Entity.Product> Add(CreateProductRequestModel model);
        Task<bool> Delete(int id);
        Task<IEnumerable<ProductListItemDto>> ProductList();
        Task<IEnumerable<CategoryLookupDto>> CategoryList();
        Task<IEnumerable<SubCategoryLookupDto>> SubCategoryList();
        Task<Data.Entity.Product> Update(UpdateProductModel model);
    }
}


