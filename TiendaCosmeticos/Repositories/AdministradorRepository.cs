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
            return _administradores.FirstOrDefault(a => a.User.Equals(user, StringComparison.OrdinalIgnoreCase));
        }

        public Administrador? GetByCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _administradores.FirstOrDefault(a => a.Cedula.Equals(cedula, StringComparison.OrdinalIgnoreCase));
        }
    }
}
