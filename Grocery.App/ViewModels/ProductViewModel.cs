using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Grocery.App.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        public ObservableCollection<Product> Products { get; }
        public ICommand GoToNewProductCommand { get; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;
            Products = new ObservableCollection<Product>();

            Refresh();

            GoToNewProductCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Grocery.App.Views.NewProductView));
            });
        }

        public void Refresh()
        {
            Products.Clear();
            foreach (Product p in _productService.GetAll())
                Products.Add(p);
        }
    }
}
