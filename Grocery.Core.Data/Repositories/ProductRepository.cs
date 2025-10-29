using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly List<Product> products;

        public ProductRepository()
        {
            products =
            [
                new Product(1, "Melk", 300, new DateOnly(2025, 9, 25), 0.95m),
                new Product(2, "Kaas", 100, new DateOnly(2025, 9, 30), 7.98m),
                new Product(3, "Brood", 400, new DateOnly(2025, 9, 12), 2.19m),
                new Product(4, "Cornflakes", 0, new DateOnly(2025, 12, 31), 1.48m)
            ];
        }

        public List<Product> GetAll() => products;

        public Product? Get(int id) => products.FirstOrDefault(p => p.Id == id);

        public Product Add(Product item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Naam is verplicht.", nameof(item));
            if (item.Price < 0)
                throw new ArgumentException("Prijs moet ≥ 0 zijn.", nameof(item));

            int newId = (products.Count == 0) ? 1 : products.Max(p => p.Id) + 1;
            var created = new Product(newId, item.Name, item.Stock, item.ShelfLife, item.Price);
            products.Add(created);
            return created;
        }

        public Product? Delete(Product item)
        {
            if (item is null) return null;
            var existing = products.FirstOrDefault(p => p.Id == item.Id);
            if (existing is null) return null;

            products.Remove(existing);
            return existing;
        }

        public Product? Update(Product item)
        {
            var product = products.FirstOrDefault(p => p.Id == item.Id);
            if (product is null) return null;

            product.Name = item.Name;
            product.Stock = item.Stock;
            product.ShelfLife = item.ShelfLife;
            product.Price = item.Price;
            return product;
        }
    }
}
