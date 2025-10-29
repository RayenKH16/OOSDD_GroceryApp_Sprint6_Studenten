using Grocery.App.ViewModels;

namespace Grocery.App.Views
{
    public partial class ProductView : ContentPage
    {
        // Laat MAUI/DI de VM injecteren
        public ProductView(ProductViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ProductViewModel vm)
                vm.Refresh(); // lijst vullen (en opnieuw bij terugnavigeren)
        }
    }
}
