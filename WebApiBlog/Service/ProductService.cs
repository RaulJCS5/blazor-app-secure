using WebApiBlog.Model;
using WebApiBlog.Repository;
using WebApiBlog.Service;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository) // Use standard constructor
    {
        _productRepository = productRepository;
    }

    public Task<ProductModel> CreateProduct(ProductModel productModel)
    {
        return _productRepository.CreateProduct(productModel);
    }

    public Task<ProductModel> GetProduct(int id)
    {
        return _productRepository.GetProduct(id);
    }

    public Task<List<ProductModel>> GetProducts()
    {
        return _productRepository.GetProducts();
    }

    public Task<bool> ProductModelExists(int id)
    {
        return _productRepository.ProductModelExists(id);
    }

    public Task UpdateProduct(ProductModel productModel)
    {
        return _productRepository.UpdateProduct(productModel);
    }

    public Task DeleteProduct(int id)
    {
        return _productRepository.DeleteProduct(id);
    }
}
