# 🏪 POSSystem - Sistema de Punto de Venta

Sistema de Punto de Venta profesional desarrollado en C# con arquitectura limpia.

## 🏗️ Arquitectura

El sistema sigue los principios de **Clean Architecture** con separación estricta de responsabilidades:

- **Domain**: Lógica de negocio pura, sin dependencias externas
- **Application**: Casos de uso y orquestación
- **Infrastructure**: Persistencia, servicios externos
- **Presentation**: UI en WPF con patrón MVVM

## 🚀 Tecnologías

- .NET 8.0
- WPF (Windows Presentation Foundation)
- Entity Framework Core
- SQLite
- xUnit para pruebas

## 📁 Estructura del Proyecto
```
POSSystem/
├── src/
│   ├── POSSystem.Domain/
│   ├── POSSystem.Application/
│   ├── POSSystem.Infrastructure/
│   └── POSSystem.Presentation.WPF/
├── tests/
│   ├── POSSystem.Domain.Tests/
│   ├── POSSystem.Application.Tests/
│   └── POSSystem.Infrastructure.Tests/
└── docs/
```

## 🎯 Funcionalidades

- ✅ Escaneo de productos
- ✅ Gestión de inventario con reservas
- ✅ Procesamiento de ventas
- ✅ Múltiples métodos de pago
- ✅ Generación de tickets
- ✅ Corte de caja
- ✅ Auditoría completa

## 🔧 Configuración

1. Clonar el repositorio
2. Abrir `POSSystem.sln` en Visual Studio
3. Restaurar paquetes NuGet
4. Compilar la solución
5. Ejecutar migraciones de base de datos
6. Ejecutar el proyecto WPF

## 📝 Estado del Proyecto

🚧 En desarrollo activo