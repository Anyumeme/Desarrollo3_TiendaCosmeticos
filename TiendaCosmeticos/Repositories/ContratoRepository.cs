using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Models;

namespace TiendaCosmeticos.Repositories
{
    public class ContratoRepository
    {
        private readonly List<Contrato> _contratos = new();

        public void Add(Contrato contrato)
        {
            if (contrato == null) return;
            _contratos.Add(contrato);
        }

        public List<Contrato> GetAll()
        {
            return _contratos.ToList();
        }

        public Contrato? GetById(Guid idContrato)
        {
            if (idContrato == Guid.Empty) return null;
            return _contratos.FirstOrDefault(c => c.IdContrato == idContrato);
        }

        public Contrato? GetByEmpleadoCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _contratos.FirstOrDefault(c => string.Equals(c?.Empleado?.Cedula, cedula.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Contrato? GetContratoActivo(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            return _contratos.FirstOrDefault(c => c != null && c.Activo && string.Equals(c.Empleado?.Cedula, cedula.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
