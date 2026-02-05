using System;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace POSSystem.Presentation.WPF.ViewModels
{
    /// <summary>
    /// ViewModel para gestión de inventario.
    /// </summary>
    public class InventarioViewModel : ViewModelBase
    {
        private readonly ILogger<InventarioViewModel> _logger;
        private string _mensajeEstado = string.Empty;

        public InventarioViewModel(ILogger<InventarioViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MensajeEstado = "Módulo de inventario (en construcción)";
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }
    }
}