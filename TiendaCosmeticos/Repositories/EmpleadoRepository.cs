using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Repositories
{
    public class EmpleadoRepository
    {
        private readonly List<Empleado> _empleados = new();

        public void Add(Empleado empleado)
        {
            if (empleado == null) return;
            _empleados.Add(empleado);
        }

        public List<Empleado> GetAll()
        {
            return _empleados.ToList();
        }

        public Empleado? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _empleados.FirstOrDefault(e => string.Equals(e?.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Empleado? GetByCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _empleados.FirstOrDefault(e => string.Equals(e?.Cedula, cedula.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Empleado? GetByUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user)) return null;
            return _empleados.FirstOrDefault(e => string.Equals(e?.User, user.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public List<Empleado> GetVendedores()
        {
            return _empleados.Where(e => e.Rol == Rol.Vendedor).ToList();
        }

        public List<Empleado> GetBodegueros()
        {
            return _empleados.Where(e => e.Rol == Rol.Bodeguero).ToList();
        }

        public Empleado? GetByCelular(int celular)
        {
            if (celular <= 0) return null;
            return _empleados.FirstOrDefault(e => e != null && e.NumeroCelular == celular);
        }
    }
}

