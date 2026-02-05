using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using POSSystem.Application.UseCases.Caja;
using POSSystem.Presentation.WPF.Commands;

namespace POSSystem.Presentation.WPF.ViewModels
{
    /// <summary>
    /// ViewModel para gestión de caja.
    /// </summary>
    public class CajaViewModel : ViewModelBase
    {
        private readonly ILogger<CajaViewModel> _logger;
        private readonly AbrirCajaUseCase _abrirCajaUseCase;
        private readonly CerrarCajaUseCase _cerrarCajaUseCase;
        private readonly RegistrarRetiroEfectivoUseCase _registrarRetiroUseCase;

        private bool _cajaAbierta;
        private string _numeroCaja = string.Empty;
        private decimal _fondoInicial;
        private decimal _saldoActual;
        private string _mensajeEstado = string.Empty;

        public CajaViewModel(
            ILogger<CajaViewModel> logger,
            AbrirCajaUseCase abrirCajaUseCase,
            CerrarCajaUseCase cerrarCajaUseCase,
            RegistrarRetiroEfectivoUseCase registrarRetiroUseCase)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _abrirCajaUseCase = abrirCajaUseCase ?? throw new ArgumentNullException(nameof(abrirCajaUseCase));
            _cerrarCajaUseCase = cerrarCajaUseCase ?? throw new ArgumentNullException(nameof(cerrarCajaUseCase));
            _registrarRetiroUseCase = registrarRetiroUseCase ?? throw new ArgumentNullException(nameof(registrarRetiroUseCase));

            AbrirCajaCommand = new RelayCommand(async () => await AbrirCaja(), () => !CajaAbierta);
            CerrarCajaCommand = new RelayCommand(async () => await CerrarCaja(), () => CajaAbierta);
            RegistrarRetiroCommand = new RelayCommand(async () => await RegistrarRetiro(), () => CajaAbierta);

            NumeroCaja = "1";
            FondoInicial = 500;
            MensajeEstado = "Caja cerrada";
            CajaAbierta = false;
        }

        #region Propiedades

        public bool CajaAbierta
        {
            get => _cajaAbierta;
            set
            {
                if (SetProperty(ref _cajaAbierta, value))
                {
                    RefrescarComandos();
                }
            }
        }

        public string NumeroCaja
        {
            get => _numeroCaja;
            set => SetProperty(ref _numeroCaja, value);
        }

        public decimal FondoInicial
        {
            get => _fondoInicial;
            set => SetProperty(ref _fondoInicial, value);
        }

        public decimal SaldoActual
        {
            get => _saldoActual;
            set => SetProperty(ref _saldoActual, value);
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        #endregion

        #region Comandos

        public ICommand AbrirCajaCommand { get; }
        public ICommand CerrarCajaCommand { get; }
        public ICommand RegistrarRetiroCommand { get; }

        #endregion

        #region Métodos

        private async System.Threading.Tasks.Task AbrirCaja()
        {
            try
            {
                if (!int.TryParse(NumeroCaja, out int numero))
                {
                    MessageBox.Show("Número de caja inválido", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var usuarioId = Guid.NewGuid(); // Hardcoded por ahora

                var resultado = await _abrirCajaUseCase.ExecuteAsync(numero, FondoInicial, usuarioId);

                if (resultado.Exitoso)
                {
                    CajaAbierta = true;
                    SaldoActual = FondoInicial;
                    MensajeEstado = $"Caja {numero} abierta. Fondo inicial: {FondoInicial:C}";
                    MessageBox.Show(resultado.Mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir caja");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task CerrarCaja()
        {
            try
            {
                // Solicitar saldo declarado
                var saldoDeclarado = SaldoActual; // Simplificación

                var usuarioId = Guid.NewGuid();

                var resultado = await _cerrarCajaUseCase.ExecuteAsync(saldoDeclarado, usuarioId);

                if (resultado.Exitoso)
                {
                    CajaAbierta = false;
                    MensajeEstado = "Caja cerrada";
                    MessageBox.Show(resultado.Mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar caja");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task RegistrarRetiro()
        {
            try
            {
                decimal monto = 100; // Simplificación
                string motivo = "Retiro de efectivo";
                var usuarioId = Guid.NewGuid();

                var resultado = await _registrarRetiroUseCase.ExecuteAsync(monto, motivo, usuarioId);

                if (resultado.Exitoso)
                {
                    SaldoActual -= monto;
                    MensajeEstado = resultado.Mensaje;
                    MessageBox.Show(resultado.Mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar retiro");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefrescarComandos()
        {
            (AbrirCajaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CerrarCajaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RegistrarRetiroCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}