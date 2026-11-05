using System;
using System.Collections.Generic;
using System.Text;

namespace TiendaCosmeticos.Models
{
    public class Venta
    {
        public Guid Id { get; private set; }
        public Cliente Cliente { get; private set; }
        public Empleado Vendedor { get; private set; }
        public DateTime Fecha { get; private set; }
        public bool Estado { get; private set; } = true;
        public List<DetalleVenta> Detalles { get; private set; } = new List<DetalleVenta>();
        public decimal Total => Detalles.Sum(d => d.Subtotal);

        public Venta() { }
        public Venta(Cliente cliente, Empleado vendedor, DateTime fecha)
        {
            Id = Guid.NewGuid();
            Cliente = cliente;
            Vendedor = vendedor;
            Fecha = fecha;
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
