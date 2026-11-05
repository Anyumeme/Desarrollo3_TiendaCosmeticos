using System;
using System.Collections.Generic;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Data
{
    public static class Seeds
    {
        public static void Inicializar(
            AdministradorRepository? adminRepo = null,
            EmpleadoRepository? empleadoRepo = null,
            ClienteRepository? clienteRepo = null,
            ProductoRepository? productoRepo = null,
            ContratoRepository? contratoRepo = null)
        {
            // 1. Usuario inicial obligatorio: Victor (Administrador)
            if (adminRepo != null && adminRepo.GetAll().Count == 0)
            {
                Administrador victor = new Administrador(
                    id: "ADM-01",
                    cedula: "001-010180-0000A",
                    nombre: "Victor",
                    numeroCelular: 88889999,
                    fechaNacimiento: new DateTime(1980, 1, 1),
                    genero: Genero.Masculino,
                    correoElectronico: "victor@tiendacosmeticos.com",
                    direccion: "Oficina Central",
                    estadoCivil: "Soltero",
                    user: "Victor",
                    password: "Vl122050*/"
                );
                adminRepo.Add(victor);
            }

            // 2. Empleados de prueba (Vendedor y Bodeguero)
            if (empleadoRepo != null && empleadoRepo.GetAll().Count == 0)
            {
                Empleado vendedor = new Empleado(
                    id: "VEN-01",
                    cedula: "001-101095-0001A",
                    nombre: "Ana Lopez",
                    numeroCelular: 88881111,
                    fechaNacimiento: new DateTime(1995, 10, 10),
                    genero: Genero.Femenino,
                    correoElectronico: "ana.lopez@tiendacosmeticos.com",
                    direccion: "Managua, Colonia Centroamerica",
                    estadoCivil: "Soltera",
                    user: "ana",
                    password: "Password123*",
                    rol: Rol.Vendedor
                );

                Empleado bodeguero = new Empleado(
                    id: "BOD-01",
                    cedula: "001-200592-0002B",
                    nombre: "Carlos Perez",
                    numeroCelular: 87772222,
                    fechaNacimiento: new DateTime(1992, 5, 20),
                    genero: Genero.Masculino,
                    correoElectronico: "carlos.perez@tiendacosmeticos.com",
                    direccion: "Masaya, Barrio San Jeronimo",
                    estadoCivil: "Casado",
                    user: "carlos",
                    password: "Password123*",
                    rol: Rol.Bodeguero
                );

                empleadoRepo.Add(vendedor);
                empleadoRepo.Add(bodeguero);

                // 3. Contratos con salario mensual y Guid autogenerado
                if (contratoRepo != null && contratoRepo.GetAll().Count == 0)
                {
                    contratoRepo.Add(new Contrato(vendedor, 12000m, "Vendedor de Mostrador"));
                    contratoRepo.Add(new Contrato(bodeguero, 14000m, "Encargado de Bodega"));
                }
            }

            // 4. Clientes de prueba
            if (clienteRepo != null && clienteRepo.GetAll().Count == 0)
            {
                Cliente cliente1 = new Cliente(
                    id: "CLI-01",
                    cedula: "001-150500-0003C",
                    nombre: "Maria Gomez",
                    numeroCelular: 88884444,
                    fechaNacimiento: new DateTime(2000, 5, 15),
                    genero: Genero.Femenino
                );

                Cliente cliente2 = new Cliente(
                    id: "CLI-02",
                    cedula: "001-080398-0004D",
                    nombre: "Sofia Ramirez",
                    numeroCelular: 87775555,
                    fechaNacimiento: new DateTime(1998, 3, 8),
                    genero: Genero.Femenino
                );

                clienteRepo.Add(cliente1);
                clienteRepo.Add(cliente2);
            }

            // 5. Productos cosmeticos iniciales
            if (productoRepo != null && productoRepo.GetAll().Count == 0)
            {
                productoRepo.Add(new Producto(
                    "PROD-01", "Labial Mate Superstay", "Maybelline", CategoriaProductos.Maquillaje,
                    350.00m, 50, DateTime.Today.AddMonths(18)
                ));

                productoRepo.Add(new Producto(
                    "PROD-02", "Base Fit Me Matte", "Maybelline", CategoriaProductos.Maquillaje,
                    480.00m, 35, DateTime.Today.AddMonths(24)
                ));

                productoRepo.Add(new Producto(
                    "PROD-03", "Mascara Lash Sensational", "Maybelline", CategoriaProductos.Maquillaje,
                    390.00m, 40, DateTime.Today.AddMonths(12)
                ));

                productoRepo.Add(new Producto(
                    "PROD-04", "Serum Acido Hialuronico", "L'Oreal", CategoriaProductos.CuidadoPiel,
                    650.00m, 20, DateTime.Today.AddMonths(15)
                ));

                productoRepo.Add(new Producto(
                    "PROD-05", "Polvo Compacto Colorstay", "Revlon", CategoriaProductos.Maquillaje,
                    420.00m, 25, DateTime.Today.AddMonths(20)
                ));
            }
        }
    }
}