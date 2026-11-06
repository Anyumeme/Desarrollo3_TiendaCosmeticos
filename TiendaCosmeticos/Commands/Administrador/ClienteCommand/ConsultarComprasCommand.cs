using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ClienteCommand
{
    public class ConsultarComprasCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ClienteService _clienteService;
        private readonly VentaServices _ventaServices;

        public ConsultarComprasCommand(
            IConsoleWriter writer,
            ClienteService clienteService,
            VentaServices ventaServices)
        {
            _writer = writer;
            _clienteService = clienteService;
            _ventaServices = ventaServices;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("HISTORIAL DE COMPRAS DE CLIENTE");

            // 1. Solicitar el cliente a consultar
            string criterio = new Validator("Ingrese el ID o Cedula del cliente: ")
                .NotEmpty()
                .ReadAndValidate();

            Cliente cliente;
            try
            {
                cliente = _clienteService.BuscarCliente(criterio);
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return;
            }

            // 2. Mostrar ficha del cliente
            _writer.ShowMessage("\n--- Informacion del Cliente ---");
            _writer.ShowMessage($"ID:      {cliente.Id}");
            _writer.ShowMessage($"Nombre:  {cliente.Nombre}");
            _writer.ShowMessage($"Cedula:  {cliente.Cedula}");
            _writer.ShowMessage($"Celular: {cliente.NumeroCelular}");

            // 3. Consultar las compras asociadas a su cedula
            try
            {
                List<Venta> compras = _ventaServices.ObtenerVentasPorCliente(cliente.Cedula);

                if (compras.Count == 0)
                {
                    _writer.ShowMessage("\nEl cliente no registra compras realizadas en el sistema.");
                    _writer.WaitForKey();
                    return;
                }

                _writer.ShowMessage("\n--- Historial de Ventas Registradas ---");

                int index = 1;
                foreach (var venta in compras)
                {
                    string estadoTexto = venta.Estado ? "Activa" : "ANULADA";
                    string vendedorTexto = venta.Vendedor?.Nombre ?? "N/A";

                    _writer.ShowMessage("\n" + new string('-', 85));
                    _writer.ShowMessage($"Compra #{index} | Factura: {venta.Id}");
                    _writer.ShowMessage($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm} | Atendido por: {vendedorTexto} | Estado: {estadoTexto}");
                    _writer.ShowMessage("Articulos:");

                    foreach (var d in venta.Detalles)
                    {
                        string nombreProd = d.Producto?.Nombre ?? "Cosmetico";
                        _writer.ShowMessage($"  * {d.Cantidad,2}x {nombreProd,-30} @ C$ {d.PrecioUnitario,8:N2}  =  C$ {d.Subtotal,8:N2}");
                    }

                    _writer.ShowMessage($"Total de esta compra: C$ {venta.Total:N2}");
                    index++;
                }

                _writer.ShowMessage(new string('-', 85));
                decimal totalActivo = compras.Where(v => v.Estado).Sum(v => v.Total);
                _writer.ShowMessage($"Total de compras registradas: {compras.Count}");
                _writer.ShowMessage($"Monto total acumulado (solo compras activas): C$ {totalActivo:N2}");
                _writer.ShowMessage(new string('-', 85));
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al consultar las compras: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
