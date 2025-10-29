using Grocery.Core.Services;
using Grocery.App.ViewModels;
using Grocery.App.Views;
using Microsoft.Extensions.Logging;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Data.Repositories;
using CommunityToolkit.Maui;
using System.Security.Principal;

namespace Grocery.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Services
            builder.Services.AddSingleton<IGroceryListService, GroceryListService>();
            builder.Services.AddSingleton<IGroceryListItemsService, GroceryListItemsService>();
            builder.Services.AddSingleton<IProductService, ProductService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IClientService, ClientService>();
            builder.Services.AddSingleton<IFileSaverService, FileSaverService>();
            builder.Services.AddSingleton<IBoughtProductsService, BoughtProductsService>();
            builder.Services.AddSingleton<ICategoryService, CategoryService>();
            builder.Services.AddSingleton<IProductCategoryService, ProductCategoryService>();

            // Repositories
            builder.Services.AddSingleton<IGroceryListRepository, GroceryListRepository>();
            builder.Services.AddSingleton<IGroceryListItemsRepository, GroceryListItemsRepository>();
            builder.Services.AddSingleton<IProductRepository, ProductRepository>();
            builder.Services.AddSingleton<IClientRepository, ClientRepository>();
            builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
            builder.Services.AddSingleton<IProductCategoryRepository, ProductCategoryRepository>();

            // Global VM
            builder.Services.AddSingleton<GlobalViewModel>();

            // UC19: ADMIN PRINCIPAL (nodig voor IsInRole("admin"))
            // TODO: later vervangen door daadwerkelijke ingelogde gebruiker
            builder.Services.AddSingleton<IPrincipal>(_ =>
                new GenericPrincipal(new GenericIdentity("AdminUser"), new[] { "admin" })
            );

            // Views + ViewModels (DI)
            builder.Services.AddTransient<GroceryListsView>().AddTransient<GroceryListViewModel>();
            builder.Services.AddTransient<GroceryListItemsView>().AddTransient<GroceryListItemsViewModel>();
            builder.Services.AddTransient<ProductView>().AddTransient<ProductViewModel>();
            builder.Services.AddTransient<ChangeColorView>().AddTransient<ChangeColorViewModel>();
            builder.Services.AddTransient<LoginView>().AddTransient<LoginViewModel>();
            builder.Services.AddTransient<BestSellingProductsView>().AddTransient<BestSellingProductsViewModel>();
            builder.Services.AddTransient<BoughtProductsView>().AddTransient<BoughtProductsViewModel>();
            builder.Services.AddTransient<CategoriesView>().AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<ProductCategoriesView>().AddTransient<ProductCategoriesViewModel>();

            // ✅ UC19: New Product view + VM registreren
            builder.Services.AddTransient<NewProductView>().AddTransient<NewProductViewModel>();

            return builder.Build();
        }
    }
}
