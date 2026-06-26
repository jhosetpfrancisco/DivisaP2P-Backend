# DivisaP2P — Backend (API)

Plataforma P2P de intercambio de divisas. Backend del Proyecto del Curso de **Desarrollo de Ambiente Web (ESAN)**.

## Stack
- **ASP.NET Core Web API — .NET 10**
- **PostgreSQL** (probado en Docker, pg17) — enfoque Database-First
- **Entity Framework Core 10** (Npgsql)
- **JWT** (autenticación) + **BCrypt** (hash de contraseñas)

## Estructura de la solución (`DivisaP2P.slnx`)
```
DivisaP2P.Library/              Capa de dominio + infraestructura (class library)
  Core/
    Common/      Constantes, reglas de negocio, JwtSettings, ResultadoServicio, paginación
    Entities/    Entidades (Usuario, Oferta, Transaccion, Voucher, Disputa, ...)
    DTOs/        Objetos de transferencia (entrada/salida de la API)
    Interfaces/  Contratos de repositorios y servicios
  Infrastructure/
    DivisaP2PDbContext.cs   Mapeo EF Core (fluent API)
    Repositories/           Acceso a datos (patrón Repository)
    Services/               Lógica de negocio (validaciones + orquestación)

DivisaP2P.WebApi/               Capa de presentación (Web API)
  Controllers/   Auth, Perfil, Ofertas, Transacciones, Calificaciones, Disputas,
                 Notificaciones, Admin
  Program.cs     DI, JWT, CORS, pipeline

Database/
  01_CreateDatabase_PostgreSQL.sql  Script de creación de esquema + datos de ejemplo (PostgreSQL)
  01_CreateDatabase.sql             Script equivalente para SQL Server (legado / referencia)
```

Arquitectura en capas: **Controller → Service (interfaz) → Repository (interfaz) → DbContext**.
Los controladores no conocen el `DbContext`; dependen de interfaces inyectadas.

## Puesta en marcha

### 1. Crear la base de datos (PostgreSQL)
Levanta un PostgreSQL local. Ejemplo con Docker:
```powershell
docker run -d --name divisap2p-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=DivisaP2PDB -p 5432:5432 postgres:17
```
Carga el esquema + datos de ejemplo:
```powershell
docker exec -i divisap2p-postgres psql -U postgres -d DivisaP2PDB < Database\01_CreateDatabase_PostgreSQL.sql
```
> Si usas otro host/puerto/usuario/clave, ajusta la cadena de conexión
> `DivisaP2PDB` en `DivisaP2P.WebApi/appsettings.json`.

### 2. Levantar la API
```powershell
dotnet run --project DivisaP2P.WebApi
```
La API queda en `http://localhost:5180` (y `https://localhost:7180`).
Documento OpenAPI en `http://localhost:5180/openapi/v1.json`.

### 3. Probar
Usa el archivo `DivisaP2P.WebApi/DivisaP2P.WebApi.http` (VS / VS Code REST Client)
o Postman. Primero haz **login** para obtener el token JWT y envíalo en el header
`Authorization: Bearer <token>` al resto de endpoints.

## Usuarios de prueba (contraseña: `Password1`)
| Correo | Rol |
|---|---|
| admin@divisap2p.com | ADM |
| juan.perez@correo.com | USU |
| maria.salas@correo.com | USU |
| pedro.vargas@correo.com | USU |
| ventas@turismoandino.com | ETU |

## Cobertura de historias de usuario
| US | Endpoint principal |
|---|---|
| US-001 Registro usuario | `POST /api/auth/registro` |
| US-002 Login (JWT, bloqueo por intentos) | `POST /api/auth/login` |
| US-003 Perfil + cuentas bancarias | `GET/PUT /api/perfil`, `/api/perfil/cuentas` |
| US-004 Publicar oferta | `POST /api/ofertas` |
| US-005 Búsqueda y filtrado | `GET /api/ofertas` |
| US-006 Editar/cancelar oferta | `PUT/DELETE /api/ofertas/{id}` |
| US-007 Matching automático | `GET /api/ofertas/{id}/matches` |
| US-008 Iniciar transacción | `POST /api/transacciones` |
| US-009 Reporte de pago/entrega + voucher | `POST /api/transacciones/{id}/reportar` |
| US-010 Validación por contraparte | `POST /api/transacciones/{id}/validar` |
| US-011 Estados + línea de tiempo | `GET /api/transacciones/{id}` |
| US-012 Calificación | `POST /api/calificaciones` |
| US-013 Historial | `GET /api/transacciones` |
| US-014 Apertura de disputa | `POST /api/disputas` |
| US-015 Resolución de disputa (ADM) | `POST /api/disputas/{id}/resolver` |
| US-016 Dashboard (ADM) | `GET /api/admin/dashboard` |
| US-017 Gestión de usuarios (ADM) | `GET /api/admin/usuarios`, `.../bloquear` |
| US-018 Reportes exportables (ADM) | `GET /api/admin/reportes/transacciones[/export]` |
| US-019 Registro ETU + aprobación | `POST /api/auth/registro-empresa`, `/api/admin/empresas/{id}/aprobar` |
| US-020 Ofertas en volumen ETU | `POST /api/ofertas` (monto hasta 500 000 para ETU) |
| US-021 Notificaciones | `GET /api/notificaciones` |

## Supuesto de roles en la transacción
El usuario que **toma** una oferta es el **Comprador** (paga primero en la divisa
origen); el que **publicó** la oferta es el **Vendedor** (entrega después en la
divisa destino). La máquina de estados se documenta en `TransaccionService`.
