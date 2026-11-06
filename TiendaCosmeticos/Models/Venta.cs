using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaCosmeticos.Models
{
    public class Venta
    {
        public Guid Id { get; private set; }
        public Cliente Cliente { get; private set; } = null!;
        public Empleado Vendedor { get; private set; } = null!;
        public DateTime Fecha { get; private set; }
        public bool Estado { get; private set; } = true;
        public List<DetalleVenta> Detalles { get; private set; } = new List<DetalleVenta>();
        public decimal Subtotal => Detalles.Sum(d => d.Subtotal);
        public decimal Descuento { get; private set; }
        public decimal Total => Subtotal - Descuento;

        public Venta() { }
        public Venta(Cliente cliente, Empleado vendedor, DateTime fecha)
        {
            Id = Guid.NewGuid();
            Cliente = cliente;
            Vendedor = vendedor;
            Fecha = fecha;
            Estado = true;
            Descuento = 0;
        }

        public void AplicarDescuento(decimal descuento)
        {
            if (descuento > 0)
            {
                Descuento = descuento;
            }
        }

        public void AgregarDetalle(Producto producto, int cantidad, decimal precioUnitario)
        {
            DetalleVenta detalle = new DetalleVenta(producto, cantidad, precioUnitario);
            Detalles.Add(detalle);
        }

        public void AnularVenta()
        {
            Estado = false;
        }
    }
}
