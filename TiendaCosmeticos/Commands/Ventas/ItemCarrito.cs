using System;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Commands.Ventas
{
    public class ItemCarrito
    {
        public Producto Producto { get; }
        public int Cantidad { get; set; }

        public ItemCarrito(Producto producto, int cantidad)
        {
            Producto = producto;
            Cantidad = cantidad;
        }
    }
}

