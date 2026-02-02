using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroCaja = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EstaAbierta = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    FondoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    FechaApertura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioAperturaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UsuarioCierreId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SaldoFinalDeclarado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ObservacionesCierre = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioCosto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GravadoIVA = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Folio = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MotivoCancelacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TasaIVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.16m),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CajaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StockFisico = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CantidadReservada = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    StockMinimo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 10),
                    StockMaximo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1000),
                    UltimaActualizacionStock = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventarios_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    StockAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    StockPosterior = table.Column<int>(type: "INTEGER", nullable: false),
                    Concepto = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NombreProducto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GravadoIVA = table.Column<bool>(type: "INTEGER", nullable: false),
                    TasaIVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TipoPago = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservasInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservasInventario_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_EstaAbierta",
                table: "Cajas",
                column: "EstaAbierta");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_NumeroCaja",
                table: "Cajas",
                column: "NumeroCaja",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaId",
                table: "MovimientosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_FechaMovimiento",
                table: "MovimientosCaja",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_Referencia",
                table: "MovimientosCaja",
                column: "Referencia");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_TipoMovimiento",
                table: "MovimientosCaja",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                table: "MovimientosInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Referencia",
                table: "MovimientosInventario",
                column: "Referencia");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                table: "MovimientosInventario",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Activo",
                table: "Productos",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Categoria",
                table: "Productos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoBarras",
                table: "Productos",
                column: "CodigoBarras",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_Estado",
                table: "ReservasInventario",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_FechaExpiracion",
                table: "ReservasInventario",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_ProductoId",
                table: "ReservasInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_VentaId",
                table: "ReservasInventario",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Estado",
                table: "Ventas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FechaCreacion",
                table: "Ventas",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Folio",
                table: "Ventas",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesVenta");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "ReservasInventario");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}
