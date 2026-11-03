using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class AuthService
    {
        private readonly AdministradorRepository _administradorRepository;
        private readonly EmpleadoRepository _empleadoRepository;

        public AuthService(
            AdministradorRepository administradorRepository,
            EmpleadoRepository empleadoRepository)
        {
            _administradorRepository = administradorRepository;
            _empleadoRepository = empleadoRepository;
        }

        public IAutentication? Autenticar(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            string userTrim = username.Trim();

            // 1. Revisar administradores
            Administrador? admin = _administradorRepository.GetByUser(userTrim);
            if (admin != null && admin.Password == password)
            {
                return admin;
            }

            // 2. Revisar empleados (Vendedores o Bodegueros)
            Empleado? empleado = _empleadoRepository.GetByUser(userTrim);
            if (empleado != null && empleado.Password == password)
            {
                return empleado;
            }

            return null;
        }
    }
}
