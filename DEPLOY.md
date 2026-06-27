# Deploy — DivisaP2P Backend (Railway)

API ASP.NET Core .NET 10 + EF Core (Npgsql). Se construye con Docker (`Dockerfile`).

## Servicio en Railway

1. Crea el plugin **PostgreSQL** en el proyecto (si no existe).
2. **New Service → GitHub repo** → `DivisaP2P-Backend`, **branch `main`**. Railway detecta el `Dockerfile`.
3. **Variables del servicio backend:**

   | Variable | Valor |
   |---|---|
   | `ConnectionStrings__DivisaP2PDB` | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};SSL Mode=Disable` |
   | `Jwt__ClaveSecreta` | una clave fuerte propia (mín. 32 caracteres) |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |

   > Las `${{Postgres.*}}` son **referencias** a tu plugin de Postgres (red privada interna,
   > por eso `SSL Mode=Disable`). **Nunca** se commitea la contraseña al repo.
   > Si conectas por el proxy público en vez de la red interna, usa
   > `SSL Mode=Require;Trust Server Certificate=true`.

4. **Networking → Generate Domain** para la URL pública del API (la usa el frontend en `VITE_API_URL`).

## Base de datos (una sola vez)

El esquema es Database-First (sin migraciones EF). Carga el esquema + datos de ejemplo
una vez contra el Postgres de Railway:

```bash
psql "<connection-string-publica-de-railway>" -f Database/01_CreateDatabase_PostgreSQL.sql
```

## Entornos

| Entorno | Config | Base de datos |
|---|---|---|
| dev (local) | `appsettings.Development.json` + `appsettings.json` | Postgres local (`localhost:5432`) |
| prod (Railway) | `appsettings.Production.json` + **env vars** | Postgres de Railway (env var) |

## Notas

- El `Dockerfile` enlaza a `http://0.0.0.0:$PORT` (Railway inyecta `PORT`).
- CORS está en `AllowAnyOrigin` (sirve para el demo). Para endurecer, restringe al dominio del frontend en `Program.cs`.
- Probar el contenedor localmente:
  ```bash
  docker build -t divisap2p-backend .
  docker run --rm -p 8080:8080 -e PORT=8080 \
    -e "ConnectionStrings__DivisaP2PDB=Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true" \
    divisap2p-backend
  ```
