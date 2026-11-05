# Requisitos Funcionales (RF) - AdministradorService

## 1. Descripcion General
El componente `AdministradorService` concentra toda la logica de negocio correspondiente a la gestion global de la tienda de cosmeticos. Es el unico servicio con permisos completos para administrar personal (vendedores y bodegueros), Clientes, contratos laborales, desactivacion de productos y auditoria.

### Dependencias requeridas en el constructor
Para operar, este servicio debe recibir e inyectar los siguientes repositorios:
- `EmpleadoRepository`
- `ClienteRepository`
- `ProductoRepository`
- `ContratoRepository`

---

## 2. Requisitos Funcionales por Modulo

### Modulo A: Gestion de Vendedores (Rol.Vendedor)

* **RF-01: Registrar Nuevo Vendedor**
  - Entrada: Cedula, Nombre, Celular, Fecha de Nacimiento, Genero, Correo, Direccion, Estado Civil, Usuario, Contrasenia, Salario Mensual.
  - Reglas de negocio:
    - Generar el identificador unico con prefijo 'VEN-' y correlativo de dos digitos (ejemplo: VEN-01). El contador debe considerar registros activos e inactivos para no repetir codigos.
    - Validar que la persona tenga al menos 18 anios de edad cumplidos a la fecha actual (`EdadMinimaException`).
    - Validar que la cedula no exista previamente en ningun empleado ni cliente (`CedulaDuplicadaException`).
    - Validar que el nombre de usuario para el login no este ocupado (`UsuarioDuplicadoException`).
    - Crear simultaneamente el `Contrato` laboral activo con el salario mensual indicado y asignado al vendedor.
  - Salida: Objeto `Empleado` registrado.

* **RF-02: Editar Vendedor Existente**
  - Entrada: ID del vendedor, nuevos datos (Nombre, Celular, Fecha Nacimiento, Genero, Correo, Direccion, Estado Civil, nueva Cedula opcional).
  - Reglas de negocio:
    - Buscar el vendedor por ID; si no existe, lanzar `RegistroNoEncontradoException`.
    - Si el vendedor esta inactivo, lanzar `RegistroInactivoException` (prohibido editar registros inactivos sin reactivacion previa).
    - El campo `Id` es inmutable (no se puede modificar).
    - Si se modifica la cedula, verificar que el nuevo valor no pertenezca a otro registro diferente.
    - Aplicar actualizacion de datos base y laborales.

* **RF-03: Cambiar Estado de Vendedor (Eliminacion Logica / Reactivacion)**
  - Entrada: ID del vendedor, nuevo estado deseado (`true` para activar, `false` para desactivar).
  - Reglas de negocio:
    - Verificar existencia del vendedor (`RegistroNoEncontradoException`).
    - Si se intenta desactivar a alguien ya inactivo o activar a alguien ya activo, lanzar `EstadoYaAsignadoException`.
    - Validacion de dependencia activa: No se puede desactivar a un vendedor si tiene un contrato activo vigente sin rescindir o cancelar previamente (`DependenciaActivaException`).
    - Al desactivar, el registro pasa a `Estado = false` (nunca se borra de la lista).
    - Al reactivar, el registro pasa a `Estado = true`.

* **RF-04: Buscar Vendedor**
  - Entrada: Criterio de busqueda (por `Id` o por `Cedula`).
  - Reglas de negocio:
    - Retorna el registro coincidente, sin importar si esta activo o inactivo, para permitir ver su ficha y estado.
    - Si no existe coincidencia, lanzar `RegistroNoEncontradoException`.

* **RF-05: Listar Vendedores con Filtros**
  - Entrada: Tipo de filtro (`Todos`, `SoloActivos`, `SoloInactivos`).
  - Reglas de negocio:
    - Filtrar sobre la lista de empleados donde `Rol == Rol.Vendedor`.
    - Retornar la lista correspondiente indicando siempre el estado de cada uno.

---

### Modulo B: Gestion de Bodegueros (Rol.Bodeguero)

* **RF-06: Registrar Nuevo Bodeguero**
  - Entrada: Cedula, Nombre, Celular, Fecha de Nacimiento, Genero, Correo, Direccion, Estado Civil, Usuario, Contrasenia, Salario Mensual.
  - Reglas de negocio:
    - Generar el identificador con prefijo 'BOD-' correlativo (ejemplo: BOD-01).
    - Validar mayoria de edad: minimo 18 anios (`EdadMinimaException`).
    - Validar que la cedula no este duplicada en el sistema (`CedulaDuplicadaException`).
    - Validar que el usuario no exista (`UsuarioDuplicadoException`).
    - Crear el `Contrato` correspondiente con su salario mensual y cargo "Encargado de Bodega".

* **RF-07: Editar Bodeguero Existente**
  - Mismas reglas que RF-02: buscar por ID, prohibir edicion en estado inactivo, mantener ID inmutable y validar cedula unica si cambia.

* **RF-08: Cambiar Estado de Bodeguero (Activar / Desactivar)**
  - Mismas reglas que RF-03: validacion de estado actual, confirmacion y verificacion de contrato laboral vigente antes de desactivar.

* **RF-09: Buscar Bodeguero (por ID o Cedula)**
  - Retorna la ficha del bodeguero indicando claramente si esta activo o inactivo.

* **RF-10: Listar Bodegueros (Todos, Solo Activos, Solo Inactivos)**
  - Filtrar sobre la coleccion de empleados con `Rol == Rol.Bodeguero`.

---

### Modulo C: Gestion de Clientes

* **RF-11: Registrar Nueva Cliente**
  - Entrada: Cedula, Nombre, Celular, Fecha de Nacimiento, Genero.
  - Reglas de negocio:
    - A la Cliente NO se le solicita correo ni direccion domiciliar.
    - Generar identificador correlativo con prefijo 'CLI-' (ejemplo: CLI-01).
    - Validar edad minima: la Cliente debe tener al menos 16 anios cumplidos (`EdadMinimaException`).
    - Validar que la cedula no exista en el sistema (`CedulaDuplicadaException`).
    - Asignar fecha de registro automatica (`DateTime.Now`).

* **RF-12: Editar Cliente Existente**
  - Entrada: ID de la Cliente, nuevos valores de Nombre, Celular, Genero, Fecha Nacimiento, nueva Cedula opcional.
  - Reglas de negocio:
    - Buscar por ID (`RegistroNoEncontradoException`).
    - Bloquear edicion si esta inactiva (`RegistroInactivoException`).
    - ID inmutable.
    - Validar que la nueva cedula no coincida con otro registro existente.

* **RF-13: Cambiar Estado de Cliente (Eliminacion Logica / Reactivacion)**
  - Entrada: ID de la Cliente, nuevo estado.
  - Reglas de negocio:
    - Verificar que no tenga ya el estado solicitado (`EstadoYaAsignadoException`).
    - Conmutar estado `Activo` / `Inactivo`.

* **RF-14: Buscar Cliente (por ID o Cedula)**
  - Localiza el registro y retorna su ficha con el estado actual visible.

* **RF-15: Listar Clientes con Filtros**
  - Retornar sublistas segun parametro: `Todas`, `SoloActivas`, `SoloInactivas`.

---

### Modulo D: Administracion y Desactivacion de Productos

* **RF-16: Desactivar Producto (Baja Logica Exclusiva de Administracion)**
  - Entrada: ID del producto (ejemplo: PROD-01).
  - Reglas de negocio:
    - El Administrador es el unico autorizado para dar de baja productos cosmeticos.
    - Buscar el producto por ID (`RegistroNoEncontradoException`).
    - Si el producto ya esta inactivo, lanzar `EstadoYaAsignadoException`.
    - Cambiar estado a `false` mediante `producto.Desactivar()`.

* **RF-17: Reactivar Producto**
  - Entrada: ID del producto.
  - Reglas de negocio:
    - Si ya esta activo, lanzar `EstadoYaAsignadoException`.
    - Verificar que el producto no se encuentre vencido a la fecha actual (`ProductoVencidoException`). Un producto caducado no debe reactivarse para la venta.
    - Cambiar estado a `true` mediante `producto.Activar()`.

* **RF-18: Listar Productos con Filtro de Estado**
  - Permite al administrador auditar el catalogo: ver todos los productos con su estado, ver solo productos activos o ver productos descatalogados/inactivos.

---

### Modulo E: Planilla de Pagos y Contratos

* **RF-19: Consultar Planilla de Pagos Mensual**
  - Entrada: Ninguna.
  - Reglas de negocio:
    - Obtener todos los empleados activos (`Rol == Rol.Vendedor` y `Rol == Rol.Bodeguero`).
    - Cruzar cada empleado con su contrato vigente en `ContratoRepository`.
    - Totalizar los salarios mensuales de los trabajadores activos.
    - Retornar una estructura con el desglose por trabajador (ID, Nombre, Cargo, Salario Mensual) y el total general de planilla de la tienda.

* **RF-20: Modificar Salario o Cargo en Contrato**
  - Entrada: ID del contrato o Cedula del empleado, nuevo salario mensual, nuevo cargo.
  - Reglas de negocio:
    - Validar que el nuevo salario sea estrictamente mayor a cero.
    - Actualizar los terminos del contrato activo.

---

## 3. Matriz de Excepciones a Lanzar por el Servicio

| Situacion Anomala | Excepcion a Lanzar | Mensaje Esperado |
| :--- | :--- | :--- |
| Registro buscado no existe en repositorio | `RegistroNoEncontradoException` | Identificador no encontrado |
| Registro esta inactivo y se intenta editar | `RegistroInactivoException` | Debe reactivarse antes de modificar |
| Se intenta asignar el mismo estado actual | `EstadoYaAsignadoException` | El registro ya se encuentra activo/inactivo |
| Cedula repetida en empleado o cliente | `CedulaDuplicadaException` | La cedula ya pertenece a otro registro |
| Nombre de usuario ya en uso para login | `UsuarioDuplicadoException` | El usuario ya existe en el sistema |
| Vendedor o bodeguero menor de 18 anios | `EdadMinimaException` | Minimo 18 anios para empleados |
| Cliente menor de 16 anios | `EdadMinimaException` | Minimo 16 anios para Clientes |
| Empleado con contrato activo al desactivar | `DependenciaActivaException` | Posee contrato laboral vigente |
| Intentar reactivar cosmetico caducado | `ProductoVencidoException` | Producto vencido, riesgo sanitario |

