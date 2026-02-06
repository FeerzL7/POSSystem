using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Application.UseCases.Ventas;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure.Persistence.UnitOfWork;
using POSSystem.Presentation.WPF.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POSSystem.Presentation.WPF.ViewModels
{
    public class VentaViewModel : ViewModelBase
    {
        private readonly ILogger<VentaViewModel> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CrearVentaUseCase _crearVentaUseCase;
        private readonly EscanearProductoUseCase _escanearProductoUseCase;
        private readonly RegistrarPagoUseCase _registrarPagoUseCase;
        private readonly FinalizarVentaUseCase _finalizarVentaUseCase;
        private readonly CancelarVentaUseCase _cancelarVentaUseCase;

        // Venta actual (en memoria)
        private Venta? _ventaActual;

        // Propiedades observables
        private string _codigoBarras = string.Empty;
        private string _folioVenta = string.Empty;
        private decimal _subtotal;
        private decimal _impuestos;
        private decimal _total;
        private decimal _montoPagado;
        private decimal _cambio;
        private string _mensajeEstado = string.Empty;
        private bool _ventaEnProceso;
        private bool _ventaPagada;

        public VentaViewModel(
            ILogger<VentaViewModel> logger,
            IUnitOfWork unitOfWork,
            CrearVentaUseCase crearVentaUseCase,
            EscanearProductoUseCase escanearProductoUseCase,
            RegistrarPagoUseCase registrarPagoUseCase,
            FinalizarVentaUseCase finalizarVentaUseCase,
            CancelarVentaUseCase cancelarVentaUseCase)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _crearVentaUseCase = crearVentaUseCase ?? throw new ArgumentNullException(nameof(crearVentaUseCase));
            _escanearProductoUseCase = escanearProductoUseCase ?? throw new ArgumentNullException(nameof(escanearProductoUseCase));
            _registrarPagoUseCase = registrarPagoUseCase ?? throw new ArgumentNullException(nameof(registrarPagoUseCase));
            _finalizarVentaUseCase = finalizarVentaUseCase ?? throw new ArgumentNullException(nameof(finalizarVentaUseCase));
            _cancelarVentaUseCase = cancelarVentaUseCase ?? throw new ArgumentNullException(nameof(cancelarVentaUseCase));

            ProductosVenta = new ObservableCollection<DetalleVentaDto>();

            NuevaVentaCommand = new RelayCommand(async () => await NuevaVenta(), () => !VentaEnProceso);
            EscanearProductoCommand = new RelayCommand(async () => await EscanearProducto(), () => VentaEnProceso && !VentaPagada);
            QuitarProductoCommand = new RelayCommand<DetalleVentaDto>(async (detalle) => await QuitarProducto(detalle), (detalle) => VentaEnProceso && !VentaPagada && detalle != null);
            PagarEfectivoCommand = new RelayCommand(async () => await PagarEfectivo(), () => VentaEnProceso && !VentaPagada && Total > 0);
            PagarTarjetaCommand = new RelayCommand(async () => await PagarTarjeta(), () => VentaEnProceso && !VentaPagada && Total > 0);
            FinalizarVentaCommand = new RelayCommand(async () => await FinalizarVenta(), () => VentaPagada);
            CancelarVentaCommand = new RelayCommand(async () => await CancelarVenta(), () => VentaEnProceso);

            MensajeEstado = "Presione 'Nueva Venta' para comenzar";
            VentaEnProceso = false;
            VentaPagada = false;

            _logger.LogInformation("VentaViewModel inicializado");
        }
        

        #region Propiedades

        public ObservableCollection<DetalleVentaDto> ProductosVenta { get; }

        public string CodigoBarras
        {
            get => _codigoBarras;
            set => SetProperty(ref _codigoBarras, value);
        }

        public string FolioVenta
        {
            get => _folioVenta;
            set => SetProperty(ref _folioVenta, value);
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set => SetProperty(ref _subtotal, value);
        }

        public decimal Impuestos
        {
            get => _impuestos;
            set => SetProperty(ref _impuestos, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public decimal MontoPagado
        {
            get => _montoPagado;
            set
            {
                if (SetProperty(ref _montoPagado, value))
                {
                    CalcularCambio();
                }
            }
        }

        public decimal Cambio
        {
            get => _cambio;
            set => SetProperty(ref _cambio, value);
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        public bool VentaEnProceso
        {
            get => _ventaEnProceso;
            set
            {
                if (SetProperty(ref _ventaEnProceso, value))
                {
                    RefrescarComandos();
                }
            }
        }

        public bool VentaPagada
        {
            get => _ventaPagada;
            set
            {
                if (SetProperty(ref _ventaPagada, value))
                {
                    RefrescarComandos();
                }
            }
        }

        #endregion

        #region Comandos

        public ICommand NuevaVentaCommand { get; }
        public ICommand EscanearProductoCommand { get; }
        public ICommand QuitarProductoCommand { get; }
        public ICommand PagarEfectivoCommand { get; }
        public ICommand PagarTarjetaCommand { get; }
        public ICommand FinalizarVentaCommand { get; }
        public ICommand CancelarVentaCommand { get; }

        #endregion

        #region Métodos

        private async Task NuevaVenta()
        {
            try
            {
                _logger.LogInformation("Iniciando nueva venta");

                var usuarioId = Guid.NewGuid();
                var resultado = await _crearVentaUseCase.ExecuteAsync(usuarioId);

                if (!resultado.Exitoso)
                {
                    MensajeEstado = $"Error: {resultado.Mensaje}";
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var ventaDto = resultado.Datos;
                _ventaActual = Venta.Crear(
                    Domain.ValueObjects.Folio.Desde(ventaDto.Folio).Value,
                    usuarioId).Value;

                ProductosVenta.Clear();
                FolioVenta = ventaDto.Folio;
                CodigoBarras = string.Empty;
                ActualizarTotales();

                VentaEnProceso = true;
                VentaPagada = false;
                MensajeEstado = $"Nueva venta iniciada: {ventaDto.Folio}";

                _logger.LogInformation("Venta {Folio} creada exitosamente", ventaDto.Folio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nueva venta");
                MensajeEstado = "Error al crear venta";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EscanearProducto()
        {
            try
            {
                if (_ventaActual == null)
                {
                    MensajeEstado = "Debe crear una venta primero";
                    return;
                }

                if (string.IsNullOrWhiteSpace(CodigoBarras))
                {
                    MensajeEstado = "Ingrese un código de barras";
                    return;
                }

                _logger.LogInformation("Escaneando producto: {CodigoBarras}", CodigoBarras);

                var resultado = await _escanearProductoUseCase.ExecuteAsync(
                    CodigoBarras,
                    _ventaActual.Id,
                    cantidad: 1);

                if (!resultado.Exitoso)
                {
                    MensajeEstado = resultado.Mensaje;
                    MessageBox.Show(resultado.Mensaje, "Producto no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CodigoBarras = string.Empty;
                    return;
                }

                var producto = resultado.Datos;
                var productoEntity = await ObtenerProductoPorCodigo(CodigoBarras);

                if (productoEntity != null)
                {
                    var resultadoAgregar = _ventaActual.AgregarProducto(productoEntity, 1);

                    if (resultadoAgregar.IsSuccess)
                    {
                        ActualizarListaProductos();
                        ActualizarTotales();
                        MensajeEstado = $"Producto agregado: {producto.Nombre}";
                        CodigoBarras = string.Empty;
                    }
                    else
                    {
                        MensajeEstado = resultadoAgregar.Error;
                        MessageBox.Show(resultadoAgregar.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al escanear producto");
                MensajeEstado = "Error al agregar producto";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task QuitarProducto(DetalleVentaDto? detalle)
        {
            if (detalle == null || _ventaActual == null) return;

            try
            {
                var resultado = _ventaActual.QuitarProducto(detalle.ProductoId);

                if (resultado.IsSuccess)
                {
                    ActualizarListaProductos();
                    ActualizarTotales();
                    MensajeEstado = $"Producto eliminado: {detalle.NombreProducto}";
                }
                else
                {
                    MessageBox.Show(resultado.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar producto");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            await Task.CompletedTask;
        }

        private async Task PagarEfectivo()
        {
            if (_ventaActual == null) return;

            try
            {
                var inputDialog = new InputMontoPagoDialog("Efectivo", Total);
                if (inputDialog.ShowDialog() != true)
                    return;

                var monto = inputDialog.MontoPagado;

                if (monto < Total)
                {
                    MessageBox.Show(
                        $"Monto insuficiente. Falta: {(Total - monto):C}",
                        "Pago insuficiente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var resultado = _ventaActual.RegistrarPago(monto, TipoPago.Efectivo);

                if (resultado.IsSuccess)
                {
                    MontoPagado = monto;
                    VentaPagada = true;
                    MensajeEstado = $"Pago registrado. Cambio: {Cambio:C}";
                    await FinalizarVenta();
                }
                else
                {
                    MessageBox.Show(resultado.Error, "Error en pago", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago en efectivo");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PagarTarjeta()
        {
            if (_ventaActual == null) return;

            try
            {
                var monto = Total;
                var inputDialog = new InputReferenciaDialog();
                if (inputDialog.ShowDialog() != true)
                    return;

                var referencia = inputDialog.Referencia;
                var resultado = _ventaActual.RegistrarPago(monto, TipoPago.TarjetaDebito, referencia);

                if (resultado.IsSuccess)
                {
                    MontoPagado = monto;
                    Cambio = 0;
                    VentaPagada = true;
                    MensajeEstado = "Pago con tarjeta registrado";
                    await FinalizarVenta();
                }
                else
                {
                    MessageBox.Show(resultado.Error, "Error en pago", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago con tarjeta");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FinalizarVenta()
        {
            if (_ventaActual == null) return;

            try
            {
                _logger.LogInformation("Finalizando venta {Folio}", _ventaActual.Folio.Valor);

                var usuarioId = Guid.NewGuid();
                var resultado = await _finalizarVentaUseCase.ExecuteAsync(_ventaActual, usuarioId);

                if (resultado.Exitoso)
                {
                    MensajeEstado = $"Venta finalizada: {_ventaActual.Folio.Valor}";

                    MessageBox.Show(
                        $"Venta finalizada exitosamente\n\nFolio: {_ventaActual.Folio.Valor}\nTotal: {Total:C}\nCambio: {Cambio:C}",
                        "Venta Completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LimpiarVenta();
                }
                else
                {
                    MessageBox.Show(
                        $"Error al finalizar venta:\n{resultado.Mensaje}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al finalizar venta");
                MessageBox.Show($"Error crítico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CancelarVenta()
        {
            if (_ventaActual == null) return;

            try
            {
                var result = MessageBox.Show(
                    "¿Está seguro de cancelar esta venta?",
                    "Cancelar Venta",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var inputDialog = new InputMotivoDialog();
                if (inputDialog.ShowDialog() != true)
                    return;

                var motivo = inputDialog.Motivo;
                var usuarioId = Guid.NewGuid();

                var resultado = await _cancelarVentaUseCase.ExecuteAsync(_ventaActual, motivo, usuarioId);

                if (resultado.Exitoso)
                {
                    MensajeEstado = "Venta cancelada";
                    LimpiarVenta();
                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar venta");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarListaProductos()
        {
            ProductosVenta.Clear();
            if (_ventaActual != null)
            {
                foreach (var detalle in _ventaActual.Detalles)
                {
                    ProductosVenta.Add(detalle.ToDto());
                }
            }
        }

        private void ActualizarTotales()
        {
            Subtotal = _ventaActual?.Subtotal ?? 0;
            Impuestos = _ventaActual?.Impuestos ?? 0;
            Total = _ventaActual?.Total ?? 0;
            CalcularCambio();
        }

        private void CalcularCambio()
        {
            Cambio = MontoPagado > Total ? MontoPagado - Total : 0;
        }

        private void LimpiarVenta()
        {
            _ventaActual = null;
            ProductosVenta.Clear();
            FolioVenta = string.Empty;
            CodigoBarras = string.Empty;
            MontoPagado = 0;
            Cambio = 0;
            ActualizarTotales();
            VentaEnProceso = false;
            VentaPagada = false;
            MensajeEstado = "Presione 'Nueva Venta' para comenzar";
        }

        private void RefrescarComandos()
        {
            (NuevaVentaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EscanearProductoCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PagarEfectivoCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PagarTarjetaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (FinalizarVentaCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (CancelarVentaCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task<Producto> ObtenerProductoPorCodigo(string codigoBarras)
        {
            try
            {
                // Buscar producto en la base de datos
                var producto = await _unitOfWork.Productos.GetByCodigoBarrasAsync(codigoBarras);
                return producto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por código {CodigoBarras}", codigoBarras);
                return null;
            }
        }

        #endregion
    }

    #region Diálogos auxiliares (placeholder)

    public class InputMontoPagoDialog
    {
        public decimal MontoPagado { get; set; }

        public InputMontoPagoDialog(string tipoPago, decimal total)
        {
            MontoPagado = total + 100;
        }

        public bool? ShowDialog()
        {
            return true;
        }
    }

    public class InputReferenciaDialog
    {
        public string Referencia { get; set; } = "REF-12345";

        public bool? ShowDialog()
        {
            return true;
        }
    }

    public class InputMotivoDialog
    {
        public string Motivo { get; set; } = "Cancelado por cliente";

        public bool? ShowDialog()
        {
            return true;
        }
    }

    #endregion
}