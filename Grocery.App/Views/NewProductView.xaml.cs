using Grocery.App.ViewModels;

namespace Grocery.App.Views
{
    public partial class NewProductView : ContentPage
    {
        // DI levert NewProductViewModel
        public NewProductView(NewProductViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
