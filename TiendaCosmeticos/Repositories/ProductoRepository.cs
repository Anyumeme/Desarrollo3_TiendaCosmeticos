using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Repositories
{
    public class ProductoRepository
    {
        private readonly List<Producto> _productos = new();

        public void Add(Producto producto)
        {
            if (producto == null) return;
            _productos.Add(producto);
        }

        public List<Producto> GetAll()
        {
            return _productos.ToList();
        }

        public Producto? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _productos.FirstOrDefault(p => BusquedaHelper.CoincideId(p?.Id, id));
        }

        public List<Producto> GetByMarca(string marca)
        {
            if (string.IsNullOrWhiteSpace(marca)) return new List<Producto>();
            return _productos
                .Where(p => string.Equals(p?.Marca, marca.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Producto> GetActivos()
        {
            return _productos.Where(p => p.Estado).ToList();
        }

        public List<Producto> GetInactivos()
        {
            return _productos.Where(p => !p.Estado).ToList();
        }
    }
}

