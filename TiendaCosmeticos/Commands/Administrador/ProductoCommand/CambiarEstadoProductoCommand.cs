using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ProductoCommand
{
    public class CambiarEstadoProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ProductoRepository _productoRepository;

        public CambiarEstadoProductoCommand(IConsoleWriter writer, ProductoRepository productoRepository)
        {
            _writer = writer;
            _productoRepository = productoRepository;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CAMBIO DE ESTADO DE PRODUCTO (ACTIVAR / DESACTIVAR)");

            // 1. Solicitar el ID del producto
            _writer.ShowMessage("Ingrese el ID del producto a modificar (o '0' para volver al menu):");
            string id = new Validator("ID del producto (ej. PROD-0001): ")
                .NotEmpty()
                .ReadAndValidate();

            if (id == "0")
            {
                return;
            }

            // 2. Busqueda en el catalogo
            Producto? producto = _productoRepository.GetById(id);

            if (producto == null)
            {
                _writer.ShowError($"No se encontro ningun producto con el ID '{id}'.");
                _writer.WaitForKey();
                return;
            }

            // 3. Ficha actual del producto
            string estadoActualTexto = producto.Estado ? "ACTIVO" : "INACTIVO";
            string vencidoTexto = producto.EstaVencido() ? "SI (CADUCADO)" : "NO (VIGENTE)";

            _writer.ShowMessage("\n--- Informacion del Producto ---");
            _writer.ShowMessage($"ID:                 {producto.Id}");
            _writer.ShowMessage($"Nombre:             {producto.Nombre}");
            _writer.ShowMessage($"Marca:              {producto.Marca}");
            _writer.ShowMessage($"Categoria:          {producto.Categoria}");
            _writer.ShowMessage($"Precio:             C$ {producto.Precio:N2}");
            _writer.ShowMessage($"Stock:              {producto.Stock} unidades");
            _writer.ShowMessage($"Fecha Vencimiento:  {producto.FechaVencimiento:dd/MM/yyyy}");
            _writer.ShowMessage($"¿Esta Vencido?:     {vencidoTexto}");
            _writer.ShowMessage($"Estado Actual:      {estadoActualTexto}");

            // 4. Conmutacion de estado
            try
            {
                if (producto.Estado)
                {
                    // Desactivar producto activo
                    string confirmacion = new Validator("\nEl producto esta ACTIVO. ¿Esta seguro de que desea DESACTIVARLO? (S/N): ")
                        .NotEmpty()
                        .ConfirmacionSN()
                        .ReadAndValidate();

                    if (confirmacion.Trim().ToUpper() != "S")
                    {
                        _writer.ShowMessage("\nOperacion cancelada por el usuario.");
                        _writer.WaitForKey();
                        return;
                    }

                    producto.Desactivar();
                    _writer.ShowMessage($"\nEl producto '{producto.Nombre}' ({producto.Id}) ha sido DESACTIVADO correctamente.");
                    _writer.ShowMessage("No aparecera disponible para la realizacion de nuevas ventas.");
                }
                else
                {
                    // Reactivar producto inactivo
                    if (producto.EstaVencido())
                    {
                        _writer.ShowError($"\nEl producto '{producto.Nombre}' ({producto.Id}) vencio el {producto.FechaVencimiento:dd/MM/yyyy}.");
                        _writer.ShowError("Por normativas sanitarias y de calidad, no se puede reactivar un cosmetico caducado.");
                        _writer.WaitForKey();
                        return;
                    }

                    string confirmacion = new Validator("\nEl producto esta INACTIVO. ¿Esta seguro de que desea REACTIVARLO? (S/N): ")
                        .NotEmpty()
                        .ConfirmacionSN()
                        .ReadAndValidate();

                    if (confirmacion.Trim().ToUpper() != "S")
                    {
                        _writer.ShowMessage("\nOperacion cancelada por el usuario.");
                        _writer.WaitForKey();
                        return;
                    }

                    producto.Activar();
                    _writer.ShowMessage($"\nEl producto '{producto.Nombre}' ({producto.Id}) ha sido REACTIVADO exitosamente.");
                    _writer.ShowMessage("Ya se encuentra disponible nuevamente en el catalogo activo de ventas.");
                }
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al cambiar el estado: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
