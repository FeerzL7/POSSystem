using System;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using POSSystem.Presentation.WPF.Commands;

namespace POSSystem.Presentation.WPF.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación.
    /// Controla la navegación entre vistas.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly VentaViewModel _ventaViewModel;
        private readonly CajaViewModel _cajaViewModel;
        private readonly InventarioViewModel _inventarioViewModel;

        private ViewModelBase _currentViewModel;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            VentaViewModel ventaViewModel,
            CajaViewModel cajaViewModel,
            InventarioViewModel inventarioViewModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ventaViewModel = ventaViewModel ?? throw new ArgumentNullException(nameof(ventaViewModel));
            _cajaViewModel = cajaViewModel ?? throw new ArgumentNullException(nameof(cajaViewModel));
            _inventarioViewModel = inventarioViewModel ?? throw new ArgumentNullException(nameof(inventarioViewModel));

            // Comandos de navegación
            MostrarVentaCommand = new RelayCommand(MostrarVenta);
            MostrarCajaCommand = new RelayCommand(MostrarCaja);
            MostrarInventarioCommand = new RelayCommand(MostrarInventario);

            // Vista inicial: Ventas
            CurrentViewModel = _ventaViewModel;

            _logger.LogInformation("MainViewModel inicializado");
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand MostrarVentaCommand { get; }
        public ICommand MostrarCajaCommand { get; }
        public ICommand MostrarInventarioCommand { get; }

        private void MostrarVenta()
        {
            CurrentViewModel = _ventaViewModel;
            _logger.LogInformation("Navegando a Vista de Ventas");
        }

        private void MostrarCaja()
        {
            CurrentViewModel = _cajaViewModel;
            _logger.LogInformation("Navegando a Vista de Caja");
        }

        private void MostrarInventario()
        {
            CurrentViewModel = _inventarioViewModel;
            _logger.LogInformation("Navegando a Vista de Inventario");
        }
    }
}