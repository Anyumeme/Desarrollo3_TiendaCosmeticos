using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ProductoCommand
{
    public class EditarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ProductoRepository _productoRepository;

        public EditarProductoCommand(IConsoleWriter writer, ProductoRepository productoRepository)
        {
            _writer = writer;
            _productoRepository = productoRepository;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("EDICION DE PRODUCTO COSMETICO");

            // 1. Solicitar el ID del producto
            _writer.ShowMessage("Ingrese el ID del producto que desea editar (o '0' para volver al menu):");
            string id = new Validator("ID del producto (ej. PROD-0001): ")
                .NotEmpty()
                .ReadAndValidate();

            if (id == "0")
            {
                return;
            }

            // 2. Busqueda en el repositorio
            Producto? producto = _productoRepository.GetById(id);

            if (producto == null)
            {
                _writer.ShowError($"No se encontro ningun producto con el ID '{id}'.");
                _writer.WaitForKey();
                return;
            }

            if (!producto.Estado)
            {
                _writer.ShowError($"El producto '{producto.Id}' se encuentra inactivo. Debe reactivarlo antes de modificar sus datos.");
                _writer.WaitForKey();
                return;
            }

            // 3. Ficha con los datos actuales
            _writer.ShowMessage("\n--- Datos Actuales del Producto ---");
            _writer.ShowMessage($"ID (Inmutable):      {producto.Id}");
            _writer.ShowMessage($"Nombre:              {producto.Nombre}");
            _writer.ShowMessage($"Marca:               {producto.Marca}");
            _writer.ShowMessage($"Categoria:           {producto.Categoria}");
            _writer.ShowMessage($"Precio Unitario:     C$ {producto.Precio:N2}");
            _writer.ShowMessage($"Stock en Bodega:     {producto.Stock} unidades");
            _writer.ShowMessage($"Fecha Vencimiento:   {producto.FechaVencimiento:dd/MM/yyyy}");

            _writer.ShowMessage("\n--- Ingrese los Nuevos Datos ---");

            // 4. Captura de nuevos valores
            string nuevoNombre = new Validator("Nuevo nombre del producto: ")
                .NotEmpty()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            string nuevaMarca = new Validator("Nueva marca o fabricante: ")
                .NotEmpty()
                .MinLength(2)
                .MaxLength(30)
                .ReadAndValidate();

            _writer.DisplayMenu(
                "Seleccione la nueva categoria:",
                "Maquillaje",
                "Cuidado de la Piel",
                "Cuidado del Cabello",
                "Fragancias",
                "Accesorios",
                "Otros",
                "Mantener categoria actual"
            );

            string catOpcion = new Validator("Ingrese su opcion (1-7): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 7)
                .ReadAndValidate();

            CategoriaProductos nuevaCategoria = catOpcion == "7"
                ? producto.Categoria
                : (CategoriaProductos)(int.Parse(catOpcion) - 1);

            string precioTexto = new Validator("Nuevo precio unitario en C$: ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();
            decimal nuevoPrecio = decimal.Parse(precioTexto.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

            string stockTexto = new Validator("Nuevo stock de unidades (0 o mas): ")
                .NotEmpty()
                .EnteroNoNegativo()
                .ReadAndValidate();
            int nuevoStock = int.Parse(stockTexto.Trim());

            string fechaTexto = new Validator("Nueva fecha de vencimiento (dd/MM/yyyy): ")
                .NotEmpty()
                .FormatoFecha("dd/MM/yyyy")
                .FechaNoVencida("dd/MM/yyyy")
                .ReadAndValidate();
            DateTime nuevaFechaVencimiento = DateTime.ParseExact(fechaTexto.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // 5. Confirmacion del usuario
            string confirmacion = new Validator("\n¿Esta seguro de guardar los cambios para este producto? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nEdicion cancelada por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 6. Aplicar cambios
            try
            {
                producto.Actualizar(
                    nuevoNombre.Trim(),
                    nuevaMarca.Trim(),
                    nuevaCategoria,
                    nuevoPrecio,
                    nuevoStock,
                    nuevaFechaVencimiento
                );

                _writer.ShowMessage($"\nProducto '{producto.Id}' actualizado exitosamente.");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al actualizar el producto: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
