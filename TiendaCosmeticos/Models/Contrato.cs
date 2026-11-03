using System;

namespace TiendaCosmeticos.Models
{
    public class Contrato
    {
        public Guid IdContrato { get; private set; } = Guid.NewGuid();
        public Empleado Empleado { get; private set; } = null!;
        public DateTime FechaInicio { get; private set; }
        public decimal SalarioMensual { get; private set; }
        public string Cargo { get; private set; } = string.Empty;
        public bool Activo { get; private set; } = true;

        public Contrato() { }

        public Contrato(
            Empleado empleado,
            decimal salarioMensual,
            string cargo,
            DateTime? fechaInicio = null,
            bool activo = true,
            Guid? idContrato = null)
        {
            IdContrato = (idContrato.HasValue && idContrato.Value != Guid.Empty)
                ? idContrato.Value
                : Guid.NewGuid();

            Empleado = empleado;
            SalarioMensual = salarioMensual;
            Cargo = string.IsNullOrWhiteSpace(cargo) ? (empleado?.Rol.ToString() ?? "Empleado") : cargo.Trim();
            FechaInicio = fechaInicio ?? DateTime.Now;
            Activo = activo;
        }

        public void CancelarContrato()
        {
            Activo = false;
        }

        public void ReactivarContrato()
        {
            Activo = true;
        }

        public void ActualizarSalario(decimal nuevoSalario)
        {
            if (nuevoSalario > 0)
            {
                SalarioMensual = nuevoSalario;
            }
        }

        public void ActualizarCargo(string nuevoCargo)
        {
            if (!string.IsNullOrWhiteSpace(nuevoCargo))
            {
                Cargo = nuevoCargo.Trim();
            }
        }
    }
}
