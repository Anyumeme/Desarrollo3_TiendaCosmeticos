using System;
using System.Collections.Generic;
using System.Text;

namespace TiendaCosmeticos.Models
{
    public class DetalleVenta
    {
        public Producto Producto { get; private set; }
        public int Cantidad { get; private set; }
        public decimal PrecioUnitario { get; private set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;

        public DetalleVenta() { }
        public DetalleVenta(Producto producto, int cantidad, decimal precioUnitario)
        {
            Producto = producto;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }
    }
}
