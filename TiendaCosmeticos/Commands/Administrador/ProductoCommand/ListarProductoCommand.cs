using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ProductoCommand
{
    public class ListarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _bodegaServices;

        public ListarProductoCommand(IConsoleWriter writer, BodegaServices bodegaServices)
        {
            _writer = writer;
            _bodegaServices = bodegaServices;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CONSULTA Y CATALOGO DE PRODUCTOS");

            // 1. Menu de opciones de listado y filtrado
            _writer.DisplayMenu(
                "Seleccione una opcion de consulta:",
                "Listar todos los productos",
                "Listar solo productos activos",
                "Listar solo productos inactivos",
                "Listar productos vencidos",
                "Listar productos con stock bajo o agotado (<= 5)",
                "Filtrar productos por categoria",
                "Buscar producto por ID",
                "Volver al menu anterior"
            );

            string opcion = new Validator("Ingrese su opcion: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 8)
                .ReadAndValidate();

            // Opcion 8: Salir al menu anterior
            if (opcion == "8")
            {
                return;
            }

            // 2. Opcion 7: Busqueda individual por ID a traves del servicio
            if (opcion == "7")
            {
                string idBuscar = new Validator("\nIngrese el ID del producto (ej. PROD-0001): ")
                    .NotEmpty()
                    .ReadAndValidate();

                try
                {
                    Producto producto = _bodegaServices.ObtenerPorId(idBuscar);

                    string estadoTexto = producto.Estado ? "ACTIVO" : "INACTIVO";
                    string vencidoTexto = producto.EstaVencido() ? "SI (CADUCADO)" : "NO (VIGENTE)";

                    _writer.ShowMessage("\n--- Ficha del Producto ---");
                    _writer.ShowMessage($"ID:                  {producto.Id}");
                    _writer.ShowMessage($"Nombre:              {producto.Nombre}");
                    _writer.ShowMessage($"Marca:               {producto.Marca}");
                    _writer.ShowMessage($"Categoria:           {producto.Categoria}");
                    _writer.ShowMessage($"Precio Unitario:     C$ {producto.Precio:N2}");
                    _writer.ShowMessage($"Stock en Bodega:     {producto.Stock} unidades");
                    _writer.ShowMessage($"Fecha Vencimiento:   {producto.FechaVencimiento:dd/MM/yyyy}");
                    _writer.ShowMessage($"¿Esta Vencido?:      {vencidoTexto}");
                    _writer.ShowMessage($"Estado en Catalogo:  {estadoTexto}");
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                }
                catch (Exception ex)
                {
                    _writer.ShowError($"Ocurrio un error inesperado al consultar el producto: {ex.Message}");
                }

                _writer.WaitForKey();
                return;
            }

            List<Producto> lista = new();

            try
            {
                switch (opcion)
                {
                    case "1":
                        lista = _bodegaServices.ObtenerTodos();
                        break;
                    case "2":
                        lista = _bodegaServices.ObtenerSoloActivos();
                        break;
                    case "3":
                        lista = _bodegaServices.ObtenerSoloInactivos();
                        break;
                    case "4":
                        lista = _bodegaServices.ObtenerVencidos();
                        break;
                    case "5":
                        lista = _bodegaServices.ObtenerStockBajo(5);
                        break;
                    case "6":
                        _writer.DisplayMenu(
                            "Seleccione la categoria:",
                            "Maquillaje",
                            "Cuidado de la Piel",
                            "Cuidado del Cabello",
                            "Fragancias",
                            "Accesorios",
                            "Otros",
                            "Volver al menu anterior"
                        );
                        string catOpcion = new Validator("Ingrese la categoria: ")
                            .NotEmpty()
                            .OnlyDigits()
                            .InputBetweenXandY(1, 7)
                            .ReadAndValidate();

                        if (catOpcion == "7")
                        {
                            return;
                        }

                        CategoriaProductos categoria = (CategoriaProductos)(int.Parse(catOpcion) - 1);
                        lista = _bodegaServices.ObtenerTodos().Where(p => p.Categoria == categoria).ToList();
                        break;
                }

                if (lista.Count == 0)
                {
                    _writer.ShowMessage("\nNo se encontraron productos con el criterio seleccionado.");
                    _writer.WaitForKey();
                    return;
                }

                // 3. Formato de tabla para presentacion ordenada
                _writer.ShowMessage("\n" + new string('-', 112));
                _writer.ShowMessage(string.Format("{0,-10} | {1,-24} | {2,-15} | {3,-16} | {4,10} | {5,6} | {6,-11} | {7,-8}",
                    "ID", "NOMBRE", "MARCA", "CATEGORIA", "PRECIO", "STOCK", "VENCE", "ESTADO"));
                _writer.ShowMessage(new string('-', 112));

                foreach (var p in lista)
                {
                    string estadoTexto = p.Estado ? "Activo" : "Inactivo";
                    string nombreCorto = p.Nombre.Length > 24 ? p.Nombre.Substring(0, 21) + "..." : p.Nombre;
                    string venceTexto = p.FechaVencimiento.ToString("dd/MM/yyyy");

                    if (p.EstaVencido())
                    {
                        venceTexto += "*";
                    }

                    _writer.ShowMessage(string.Format("{0,-10} | {1,-24} | {2,-15} | {3,-16} | {4,10:N2} | {5,6} | {6,-11} | {7,-8}",
                        p.Id,
                        nombreCorto,
                        p.Marca,
                        p.Categoria,
                        p.Precio,
                        p.Stock,
                        venceTexto,
                        estadoTexto));
                }

                _writer.ShowMessage(new string('-', 112));
                _writer.ShowMessage($"Total de productos encontrados: {lista.Count}");
                if (lista.Any(p => p.EstaVencido()))
                {
                    _writer.ShowMessage("(*) Productos marcados con asterisco se encuentran actualmente vencidos.");
                }
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al listar productos: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
