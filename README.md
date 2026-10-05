# Personal Car Garage

Sistema de administración de automóviles para personas. Permite a un administrador de plataforma gestionar usuarios y a cada gestor administrar uno o varios vehículos, incluyendo mantenimientos y trámites únicos o periódicos.

## Stack

- Backend: .NET 10, Clean Architecture, Entity Framework Core, JWT
- Base de datos: SQL Server local (`localhost`)
- Frontend: Angular 22, Tailwind CSS 4, daisyUI 5, Font Awesome Free

## Estructura

```
api/PersonalCarGarage.slnx
api/PersonalCarGarage.Domain
api/PersonalCarGarage.Application
api/PersonalCarGarage.Infrastructure
api/PersonalCarGarage.Api
web
```

## Roles

- **Administrador**: gestiona usuarios, asigna gestores a vehículos y ve toda la plataforma.
- **Gestor de vehículos**: administra uno o varios automóviles asignados, registra mantenimientos y trámites.

## Cuentas de demostración

| Rol | Correo | Contraseña |
| --- | --- | --- |
| Administrador | `admin@personalcargarage.com` | `Admin123!` |
| Gestor | `manager@personalcargarage.com` | `Manager123!` |

## Cómo ejecutar

1. Asegúrate de que SQL Server esté en ejecución. La cadena por defecto usa `Server=localhost`. Si tu instancia es Express con nombre, cambia `appsettings.json` a `Server=.\\SQLEXPRESS`.
2. API:

```bash
dotnet run --project api/PersonalCarGarage.Api --launch-profile http
```

La API queda en `http://localhost:5021` y crea la base `PersonalCarGarage` al arrancar.

3. Frontend:

```bash
cd web
npm start
```

Abre `http://localhost:4200`. Si el puerto 4200 está ocupado, usa `npx ng serve --port 4201`.
