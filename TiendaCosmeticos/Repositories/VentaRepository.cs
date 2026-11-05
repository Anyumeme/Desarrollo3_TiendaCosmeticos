using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Repositories
{
    public class VentaRepository
    {
        private readonly List<Venta> _ventas = new();

        public void Add(Venta venta)
        {
            if (venta == null) return;
            _ventas.Add(venta);
        }

        public List<Venta> GetAll()
        {
            return _ventas.ToList();
        }

        public Venta? GetById(Guid id)
        {
            if (id == Guid.Empty) return null;
            return _ventas.FirstOrDefault(v => v.Id == id);
        }

        public List<Venta> GetByCliente(string cedulaCliente)
        {
            if (string.IsNullOrWhiteSpace(cedulaCliente)) return new List<Venta>();
            return _ventas
                .Where(v => string.Equals(v?.Cliente?.Cedula, cedulaCliente.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Venta> GetByVendedor(string cedulaVendedor)
        {
            if (string.IsNullOrWhiteSpace(cedulaVendedor)) return new List<Venta>();
            return _ventas
                .Where(v => string.Equals(v?.Vendedor?.Cedula, cedulaVendedor.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Venta> GetActivas()
        {
            return _ventas.Where(v => v.Estado).ToList();
        }

        public List<Venta> GetAnuladas()
        {
            return _ventas.Where(v => !v.Estado).ToList();
        }
    }
}
