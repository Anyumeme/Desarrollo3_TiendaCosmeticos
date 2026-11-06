using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Ventas
{
    public class MostrarComprobanteCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly Venta _venta;

        public MostrarComprobanteCommand(IConsoleWriter writer, Venta venta)
        {
            _writer = writer;
            _venta = venta;
        }

        public void Execute()
        {
            if (_venta == null)
            {
                _writer.ShowError("No se proporcionaron datos validos de la venta.");
                return;
            }

            string estadoFactura = _venta.Estado ? "ACTIVA" : "ANULADA";

            _writer.Clear();
            _writer.ShowHeader("COMPROBANTE DE VENTA");
            _writer.ShowMessage($"Factura ID:  {_venta.Id}");
            _writer.ShowMessage($"Estado:      {estadoFactura}");
            _writer.ShowMessage($"Fecha:       {_venta.Fecha:dd/MM/yyyy HH:mm}");
            _writer.ShowMessage($"Atendido por: {_venta.Vendedor.Nombre} ({_venta.Vendedor.Rol})");
            _writer.ShowMessage($"Cliente:     {_venta.Cliente.Nombre} | Cedula: {_venta.Cliente.Cedula} | Cel: {_venta.Cliente.NumeroCelular}");
            _writer.ShowMessage(new string('-', 78));
            _writer.ShowMessage(string.Format("{0,-6} | {1,-32} | {2,14} | {3,16}", "CANT", "PRODUCTO", "PRECIO U.", "SUBTOTAL"));
            _writer.ShowMessage(new string('-', 78));

            foreach (var detalle in _venta.Detalles)
            {
                string nombreProd = detalle.Producto.Nombre;
                if (nombreProd.Length > 32)
                {
                    nombreProd = nombreProd.Substring(0, 29) + "...";
                }

                _writer.ShowMessage(string.Format("{0,-6} | {1,-32} | {2,14:N2} | {3,16:N2}",
                    detalle.Cantidad,
                    nombreProd,
                    detalle.PrecioUnitario,
                    detalle.Subtotal));
            }

            _writer.ShowMessage(new string('-', 78));
            _writer.ShowMessage($"Subtotal bruto:                                            C$ {_venta.Subtotal,12:N2}");

            if (_venta.Descuento > 0)
            {
                _writer.ShowMessage($"Descuento aplicado:                                      - C$ {_venta.Descuento,12:N2}");
            }
            else
            {
                _writer.ShowMessage("Descuento aplicado:                                         C$         0.00");
            }

            _writer.ShowMessage(new string('=', 78));
            _writer.ShowMessage($"TOTAL NETO:                                                C$ {_venta.Total,12:N2}");
            _writer.ShowMessage(new string('=', 78));
        }

        public void Undo()
        {
        }
    }
}

