using System;

namespace TiendaCosmeticos.Models
{
    public class Producto
    {
        public string Id { get; private set; } = string.Empty;
        public string Nombre { get; private set; } = string.Empty;
        public string Marca { get; private set; } = string.Empty;
        public CategoriaProductos Categoria { get; private set; }
        public decimal Precio { get; private set; }
        public int Stock { get; private set; }
        public DateTime FechaVencimiento { get; private set; }
        public bool Estado { get; private set; } = true;

        public Producto() { }

        public Producto(
            string id,
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fechaVencimiento,
            bool estado = true)
        {
            Id = id ?? string.Empty;
            Nombre = nombre ?? string.Empty;
            Marca = marca ?? string.Empty;
            Categoria = categoria;
            Precio = precio;
            Stock = stock;
            FechaVencimiento = fechaVencimiento;
            Estado = estado;
        }

        public void Desactivar()
        {
            Estado = false;
        }

        public void Activar()
        {
            Estado = true;
        }

        public bool EstaVencido()
        {
            return FechaVencimiento.Date < DateTime.Today;
        }

        public bool AumentarStock(int cantidad)
        {
            if (cantidad <= 0) return false;
            Stock += cantidad;
            return true;
        }

        public bool DisminuirStock(int cantidad)
        {
            if (cantidad <= 0 || cantidad > Stock) return false;
            Stock -= cantidad;
            return true;
        }

        public void Actualizar(
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fechaVencimiento)
        {
            Nombre = nombre ?? string.Empty;
            Marca = marca ?? string.Empty;
            Categoria = categoria;
            Precio = precio;
            Stock = stock;
            FechaVencimiento = fechaVencimiento;
        }
    }
}
