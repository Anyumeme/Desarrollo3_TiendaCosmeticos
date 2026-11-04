using System;
using System.Collections.Generic;
using System.Text;

namespace TiendaCosmeticos.Models
{
    internal class Venta
    {
        public Guid Id { get; private set; }
        public Cliente Cliente { get; private set; }
        public Empleado Vendedor { get; private set; }
        public DateTime Fecha { get; private set; }
        public decimal Total { get; private set; }

        public Venta() { }
        public Venta(Cliente cliente, Empleado vendedor, DateTime fecha, decimal total)
        {
            Id = Guid.NewGuid();
            Cliente = cliente;
            Vendedor = vendedor;
            Fecha = fecha;
            Total = total;
        }

    }
}
