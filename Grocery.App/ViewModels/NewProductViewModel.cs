using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Security.Principal;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IPrincipal _principal;

        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private int stock;
        [ObservableProperty] private decimal price;
        [ObservableProperty] private DateTime? shelfLifeDate;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? error;

        public NewProductViewModel(IProductService productService, IPrincipal? principal = null)
        {
            _productService = productService;
            _principal = principal ?? System.Threading.Thread.CurrentPrincipal
                ?? new GenericPrincipal(new GenericIdentity(string.Empty), Array.Empty<string>());
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Alleen admin
            if (!_principal.IsInRole("admin"))
            {
                Error = "Alleen gebruikers met de admin-rol mogen nieuwe producten aanmaken.";
                await Shell.Current.DisplayAlert("Geen toegang", Error, "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Error = null;

                if (string.IsNullOrWhiteSpace(Name))
                    throw new ArgumentException("Naam is verplicht.");

                if (Stock <= 0)
                    throw new ArgumentException("Voorraad moet groter zijn dan 0.");

                if (Price <= 0)
                    throw new ArgumentException("Prijs moet groter zijn dan 0.");

                var tht = ShelfLifeDate.HasValue
                    ? DateOnly.FromDateTime(ShelfLifeDate.Value)
                    : default;

                _productService.Add(new Product(0, Name.Trim(), Stock, tht, Price));

                await Shell.Current.DisplayAlert("Succes", "Product aangemaakt.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                await Shell.Current.DisplayAlert("Fout", Error, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task CancelAsync()
        {
            return Shell.Current.GoToAsync("..");
        }
    }
}
