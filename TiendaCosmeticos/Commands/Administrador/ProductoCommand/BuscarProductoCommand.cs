using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ProductoCommand
{
    public class BuscarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ProductoRepository _productoRepository;

        public BuscarProductoCommand(IConsoleWriter writer, ProductoRepository productoRepository)
        {
            _writer = writer;
            _productoRepository = productoRepository;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("BUSQUEDA DE PRODUCTO EN CATALOGO");

            // 1. Solicitar el ID del producto a consultar
            _writer.ShowMessage("Ingrese el ID del producto que desea consultar (o '0' para volver al menu):");
            string id = new Validator("ID del producto (ej. PROD-0001): ")
                .NotEmpty()
                .ReadAndValidate();

            if (id == "0")
            {
                return;
            }

            // 2. Busqueda en repositorio
            Producto? producto = _productoRepository.GetById(id);

            if (producto == null)
            {
                _writer.ShowError($"No se encontro ningun producto registrado con el ID '{id}'.");
                _writer.WaitForKey();
                return;
            }

            // 3. Ficha con los datos completos del cosmetico
            string estadoTexto = producto.Estado ? "ACTIVO" : "INACTIVO";
            string vencidoTexto = producto.EstaVencido() ? "SI (CADUCADO)" : "NO (VIGENTE)";

            _writer.ShowMessage("\n--- Informacion Detallada del Producto ---");
            _writer.ShowMessage($"ID:                 {producto.Id}");
            _writer.ShowMessage($"Nombre:             {producto.Nombre}");
            _writer.ShowMessage($"Marca:              {producto.Marca}");
            _writer.ShowMessage($"Categoria:          {producto.Categoria}");
            _writer.ShowMessage($"Precio Unitario:    C$ {producto.Precio:N2}");
            _writer.ShowMessage($"Stock en Bodega:    {producto.Stock} unidades");
            _writer.ShowMessage($"Fecha Vencimiento:  {producto.FechaVencimiento:dd/MM/yyyy}");
            _writer.ShowMessage($"¿Esta Vencido?:     {vencidoTexto}");
            _writer.ShowMessage($"Estado en Catalogo: {estadoTexto}");

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
