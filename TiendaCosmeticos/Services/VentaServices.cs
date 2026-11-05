using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class VentaServices
    {
        private readonly VentaRepository _ventaRepository;
        private readonly ProductoRepository _productoRepository;
        private readonly ClienteRepository _clienteRepository;

        public VentaServices(
            VentaRepository ventaRepository,
            ProductoRepository productoRepository,
            ClienteRepository clienteRepository)
        {
            _ventaRepository = ventaRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;
        }

        public void RegistrarVenta(Venta venta)
        {
            if (venta == null)
            {
                throw new TiendaException("Los datos de la venta no son validos.");
            }

            ValidarCliente(venta.Cliente);
            ValidarVendedor(venta.Vendedor);
            ValidarDetallesYStock(venta.Detalles);
            DescontarInventario(venta.Detalles);

            _ventaRepository.Add(venta);
        }

        public void AnularVenta(Guid idVenta)
        {
            Venta venta = ObtenerPorId(idVenta);

            if (!venta.Estado)
            {
                throw new EstadoYaAsignadoException("Venta", false);
            }

            // Devolvemos el stock a cada producto al cancelar la venta
            foreach (var detalle in venta.Detalles)
            {
                if (detalle.Producto != null)
                {
                    detalle.Producto.AumentarStock(detalle.Cantidad);
                }
            }

            venta.AnularVenta();
        }

        public Venta ObtenerPorId(Guid id)
        {
            Venta? venta = _ventaRepository.GetById(id);
            if (venta == null)
            {
                throw new RegistroNoEncontradoException("Venta", id.ToString());
            }
            return venta;
        }

        public List<Venta> ObtenerTodas()
        {
            return _ventaRepository.GetAll();
        }

        public List<Venta> ObtenerActivas()
        {
            return _ventaRepository.GetActivas();
        }

        public List<Venta> ObtenerAnuladas()
        {
            return _ventaRepository.GetAnuladas();
        }

        public List<Venta> ObtenerVentasPorCliente(string cedulaCliente)
        {
            return _ventaRepository.GetByCliente(cedulaCliente);
        }

        public List<Venta> ObtenerVentasPorVendedor(string cedulaVendedor)
        {
            return _ventaRepository.GetByVendedor(cedulaVendedor);
        }

        // --- Metodos privados de validacion y reglas de negocio ---

        private void ValidarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new TiendaException("La venta debe tener un cliente asignado.");
            }

            if (!cliente.Estado)
            {
                throw new RegistroInactivoException("Cliente", cliente.Id);
            }

            int edad = cliente.CalcularEdad();
            if (edad < 16)
            {
                throw new EdadMinimaException("el cliente", 16, edad);
            }
        }

        private void ValidarVendedor(Empleado vendedor)
        {
            if (vendedor == null)
            {
                throw new TiendaException("La venta debe ser procesada por un vendedor autenticado.");
            }

            if (!vendedor.Estado)
            {
                throw new RegistroInactivoException("Vendedor", vendedor.Id);
            }
        }

        private void ValidarDetallesYStock(List<DetalleVenta> detalles)
        {
            if (detalles == null || detalles.Count == 0)
            {
                throw new TiendaException("La venta debe contener al menos un producto.");
            }

            foreach (var detalle in detalles)
            {
                Producto prod = detalle.Producto;

                if (prod == null)
                {
                    throw new TiendaException("Uno de los productos en la venta no es valido.");
                }

                if (!prod.Estado)
                {
                    throw new RegistroInactivoException("Producto", prod.Id);
                }

                if (prod.EstaVencido())
                {
                    throw new ProductoVencidoException(prod.Nombre, prod.FechaVencimiento);
                }

                if (detalle.Cantidad <= 0)
                {
                    throw new TiendaException($"La cantidad para '{prod.Nombre}' debe ser mayor a cero.");
                }

                if (detalle.Cantidad > 200)
                {
                    throw new LimiteVentaSuperadoException(detalle.Cantidad);
                }

                if (prod.Stock < detalle.Cantidad)
                {
                    throw new StockInsuficienteException(prod.Nombre, prod.Stock, detalle.Cantidad);
                }
            }
        }

        private void DescontarInventario(List<DetalleVenta> detalles)
        {
            foreach (var detalle in detalles)
            {
                detalle.Producto.DisminuirStock(detalle.Cantidad);
            }
        }
    }
}
