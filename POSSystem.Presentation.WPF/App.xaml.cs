using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POSSystem.Application.UseCases.Caja;
using POSSystem.Application.UseCases.Inventario;
using POSSystem.Application.UseCases.Ventas;
using POSSystem.Domain.DomainServices;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure;
using POSSystem.Infrastructure.Persistence;
using POSSystem.Infrastructure.Persistence.Context;
using POSSystem.Presentation.WPF.ViewModels;

namespace POSSystem.Presentation.WPF
{
    public partial class App : System.Windows.Application
    {
        private IHost? _host;

        public App()
        {
            _host = CreateHostBuilder().Build();
        }

        private IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddInfrastructure(context.Configuration);

                    services.AddScoped<CrearVentaUseCase>();
                    services.AddScoped<EscanearProductoUseCase>();
                    services.AddScoped<RegistrarPagoUseCase>();
                    services.AddScoped<FinalizarVentaUseCase>();
                    services.AddScoped<CancelarVentaUseCase>();
                    services.AddScoped<ReversarVentaUseCase>();
                    services.AddScoped<AbrirCajaUseCase>();
                    services.AddScoped<CerrarCajaUseCase>();
                    services.AddScoped<RegistrarRetiroEfectivoUseCase>();
                    services.AddScoped<ConsultarStockUseCase>();
                    services.AddScoped<AjustarInventarioUseCase>();

                    services.AddScoped<GeneradorFolio>();

                    services.AddTransient<MainViewModel>();
                    services.AddTransient<VentaViewModel>();
                    services.AddTransient<CajaViewModel>();
                    services.AddTransient<InventarioViewModel>();

                    services.AddTransient<MainWindow>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (_host != null)
            {
                await _host.StartAsync();

                // Inicializar base de datos
                await InitializeDatabaseAsync();

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                using (_host)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                }
            }

            base.OnExit(e);
        }

        private async System.Threading.Tasks.Task InitializeDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<App>>();

            try
            {
                // Obtener DbContext y aplicar migraciones
                var context = services.GetRequiredService<POSDbContext>();
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas exitosamente");

                // Cargar datos de prueba
                var unitOfWork = services.GetRequiredService<IUnitOfWork>();
                await SeedData.InitializeAsync(unitOfWork, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al inicializar la base de datos");
                MessageBox.Show(
                    $"Error al inicializar la base de datos:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}