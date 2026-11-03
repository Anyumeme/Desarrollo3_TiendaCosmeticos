using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Repositories
{
    public class ClienteRepository
    {
        private readonly List<Cliente> _clientes = new();

        public void Add(Cliente cliente)
        {
            if (cliente == null) return;
            _clientes.Add(cliente);
        }

        public List<Cliente> GetAll()
        {
            return _clientes.ToList();
        }

        public Cliente? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _clientes.FirstOrDefault(c => c.Id.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Cliente? GetByCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _clientes.FirstOrDefault(c => c.Cedula.Equals(cedula.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}

