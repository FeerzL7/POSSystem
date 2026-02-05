using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Interfaces;
using POSSystem.Domain.ValueObjects;

namespace POSSystem.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IUnitOfWork unitOfWork, ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando carga de datos de prueba...");

                // Verificar si ya hay productos
                var productosExistentes = await unitOfWork.Productos.GetAllAsync();
                if (productosExistentes.Any())
                {
                    logger.LogInformation("La base de datos ya contiene productos. Omitiendo seed.");
                    return;
                }

                // Crear productos
                var productosData = new[]
                {
                    ("7501234567890", "Coca-Cola 600ml", 15.00m, 10.00m, "Bebidas", true, 100),
                    ("7501234567891", "Sabritas Original 45g", 12.00m, 8.00m, "Botanas", true, 150),
                    ("7501234567892", "Gansito Marinela", 10.00m, 7.00m, "Dulces", true, 200),
                    ("7501234567893", "Agua Ciel 1L", 8.00m, 5.00m, "Bebidas", true, 180),
                    ("7501234567894", "Bimbo Blanco Grande", 35.00m, 25.00m, "Panaderia", true, 50),
                    ("7501234567895", "Cerveza Corona 355ml", 22.00m, 15.00m, "Bebidas", true, 120),
                    ("7501234567896", "Huevo San Juan 12 pzas", 45.00m, 35.00m, "Lacteos", true, 80),
                    ("7501234567897", "Leche Lala 1L", 22.00m, 18.00m, "Lacteos", true, 100),
                    ("7501234567898", "Papel Higienico Suave 4 rollos", 28.00m, 20.00m, "Hogar", true, 90),
                    ("7501234567899", "Pringles Original", 35.00m, 25.00m, "Botanas", true, 60),
                    ("7501234567800", "Galletas Oreo", 18.00m, 12.00m, "Dulces", true, 140),
                    ("7501234567801", "Red Bull 250ml", 32.00m, 22.00m, "Bebidas", true, 75),
                    ("7501234567802", "Tostadas Charras", 15.00m, 10.00m, "Botanas", true, 110),
                    ("7501234567803", "Frijoles La Costeña 580g", 25.00m, 18.00m, "Enlatados", true, 95),
                    ("7501234567804", "Atun Tuny 140g", 18.00m, 13.00m, "Enlatados", true, 130)
                };

                // Primero guardar todos los productos
                foreach (var (codigoBarras, nombre, precioVenta, precioCosto, categoria, gravadoIVA, stockInicial) in productosData)
                {
                    var cb = CodigoBarras.Crear(codigoBarras);
                    if (cb.IsFailure)
                    {
                        logger.LogWarning("Código de barras inválido: {CodigoBarras}", codigoBarras);
                        continue;
                    }

                    var productoResult = Producto.Crear(
                        cb.Value,
                        nombre,
                        precioVenta,
                        precioCosto,
                        $"Descripción de {nombre}",
                        categoria,
                        gravadoIVA);

                    if (productoResult.IsFailure)
                    {
                        logger.LogWarning("Error al crear producto {Nombre}: {Error}", nombre, productoResult.Error);
                        continue;
                    }

                    await unitOfWork.Productos.AddAsync(productoResult.Value);
                }

                // Guardar productos primero
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Productos guardados exitosamente");

                // Ahora crear inventarios para cada producto
                var productos = await unitOfWork.Productos.GetAllAsync();
                var stockPorCodigo = productosData.ToDictionary(p => p.Item1, p => p.Item7);

                foreach (var producto in productos)
                {
                    var stockInicial = stockPorCodigo.ContainsKey(producto.CodigoBarras.Valor)
                        ? stockPorCodigo[producto.CodigoBarras.Valor]
                        : 100;

                    var inventarioResult = Inventario.Crear(
                        producto.Id,
                        stockInicial,
                        stockMinimo: 10,
                        stockMaximo: 500);

                    if (inventarioResult.IsSuccess)
                    {
                        await unitOfWork.Inventarios.AddAsync(inventarioResult.Value);
                    }
                }

                // Guardar inventarios
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Inventarios guardados exitosamente");

                logger.LogInformation("Datos de prueba cargados: {Count} productos", productos.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cargar datos de prueba");
                throw;
            }
        }
    }
}