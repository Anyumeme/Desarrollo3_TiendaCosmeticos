using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Repositories
{
    public class AdministradorRepository
    {
        private readonly List<Administrador> _administradores = new();

        public void Add(Administrador administrador)
        {
            if (administrador == null) return;
            _administradores.Add(administrador);
        }

        public List<Administrador> GetAll()
        {
            return _administradores.ToList();
        }

        public Administrador? GetByUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user)) return null;
            return _administradores.FirstOrDefault(a => string.Equals(a?.User, user.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Administrador? GetByCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _administradores.FirstOrDefault(a => string.Equals(a?.Cedula, cedula.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Administrador? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _administradores.FirstOrDefault(a => string.Equals(a?.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Administrador? GetByCelular(int celular)
        {
            if (celular <= 0) return null;
            return _administradores.FirstOrDefault(a => a != null && a.NumeroCelular == celular);
        }
    }
}
