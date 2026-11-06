# Estado del Proyecto: Modulos Pendientes

> AVISO:
> Toda la logica del negocio, navegacion de menus, validaciones de seguridad, cero caidas y controladores de los 3 roles (Administrador, Vendedor y Bodeguero) estan 100% terminados y funcionales.

---

### Modulos pendientes por implementar:

1. Persistencia en Archivo Binario de Datos (.bin):
   - Guardar y cargar automaticamente en disco mediante BinaryWriter y BinaryReader todas las listas del negocio:
     - Administradores (con la cuenta inicial de Victor).
     - Empleados (Vendedores y Bodegueros).
     - Contratos laborales.
     - Clientes.
     - Productos cosmeticos.
     - Facturas de ventas y sus detalles.
   - Si el archivo existe al arrancar, recupera la informacion; si no existe, carga los datos semilla y lo genera.

2. Archivo Binario de Logs y Auditoria (tienda_logs.bin):
   - Registro cronologico inmutable de todas las acciones del sistema:
     - Inicios y cierres de sesion.
     - Altas, ediciones, bajas (desactivaciones) y reactivaciones.
     - Cobros y anulaciones de facturas.

3. Opcion 5 del Administrador ("Auditoria y Consulta de Logs"):
   - Conectar el visualizador en consola para que el Administrador pueda consultar la lista de eventos guardados en el archivo binario de logs.

---
Fin del aviso.
