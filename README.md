# 📑 Billing API – Backend (.NET + SQL Server)

## 📌 Descripción
Este proyecto implementa una API REST para la gestión de **clientes**, **productos** y **facturas**.  
Es parte de una prueba técnica orientada a **.NET + Angular**, siguiendo principios de **Clean Code** y buenas prácticas.

---

## ⚙️ Tecnologías utilizadas
- **.NET 8** con ASP.NET Core Web API (standalone, sin módulos legacy)  
- **SQL Server 2022** como base de datos  
- **Dapper** como micro ORM para consultas directas a procedimientos almacenados  
- **Docker Desktop + WSL2** para ejecutar SQL Server en contenedor  
- **Swagger/OpenAPI** para documentación y prueba de endpoints  

---

## 🛠️ Decisiones de diseño
1. **Procedimientos almacenados (SPs)**  
   - La prueba especificaba no usar ORM completos (como Entity Framework).  
   - Toda la lógica de negocio (CRUD de clientes, productos, facturas) está implementada con SPs.  
   - Se creó un SP genérico `sp_Response` para unificar códigos y mensajes de retorno.

2. **Estructura por capas**  
   - **DTOs** → representación de entrada/salida  
   - **Repositories** → llamados a SPs (aislamiento de acceso a datos)  
   - **Services** → lógica de negocio (transformaciones, validaciones mínimas)  
   - **Controllers** → capa expuesta vía API  

3. **Conexión a BD**  
   - Uso de `IConfiguration` y `appsettings.json` para manejar la `ConnectionString`.  
   - En local se utilizó **Secret Manager** para no exponer credenciales.  
   - El README aclara cómo debe configurarse un usuario `developer` en SQL Server para replicar.  

4. **Errores y respuestas**  
   - Estandarización de respuestas con estructura:  
     ```json
     {
       "code": 0,
       "message": "Success",
       "data": { "id": 5, "razonSocial": "Geek Store" }
     }
     ```
5. **Idioma**
   - Para mantener la coherencia con los estándares definidos en la prueba y las guías entregadas, la estructura de la base de datos se diseñó en español (nombres de tablas y columnas).
   Sin embargo, por comodidad y para asegurar un flujo de desarrollo más orgánico y consistente con las prácticas habituales, los procedimientos almacenados y la capa de backend se implementaron en inglés.

---

## 📂 Estructura del proyecto
```plain
Billing.Api/            -> Web API (.NET)
Billing.Application/    -> Services
Billing.Domain/         -> DTOs
Billing.Infrastructure/ -> Repositories (Dapper) y acceso a datos
Billing.sln             -> Solution
```
---
## ▶️ Cómo ejecutar (local)
1. **Base de datos**
   - Edita el archivo init.sql colocando el nombre de la base de datos y los datos de usuario indicados en la prueba.
   - Levanta SQL Server (local o Docker) y corre tu init.sql (DB, tablas, SPs, seeds, usuario).
  
2. **Connection string (segura con User-Secrets)**
  ```bash
    dotnet user-secrets init --project Billing.Api
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=[DATABASE_NAME];User Id=[USER_ID;Password=[PASSWORD];TrustServerCertificate=True;" --project Billing.Api
    dotnet user-secrets list --project Billing.Api
  ```
3. **Cors (Angular en 4200)**

En Program.cs, registra y usa la política:
```csharp
  var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

  builder.Services.AddCors(options =>
  {
      options.AddPolicy(name: MyAllowSpecificOrigins,
          policy =>
          {
              policy.WithOrigins("http://localhost:4200") // frontend Angular
                    .AllowAnyHeader()
                    .AllowAnyMethod();
          });
  });
  
  var app = builder.Build();
  
  if (app.Environment.IsDevelopment())
  {
      app.UseCors(MyAllowSpecificOrigins);
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1");
          c.RoutePrefix = string.Empty;
      });
  }
```

4. **Ejecutar la API**

```bash
  dotnet run --project Billing.Api
```
- Swagger: http://localhost:5000/swagger (o el puerto que tengas configurado).

---
## 🧪 Scripts SQL (resumen de piezas clave)
- TVP para detalles de factura
  ```sql
    CREATE TYPE dbo.InvoiceDetailType AS TABLE
    (
      IdProducto INT NOT NULL,
      Cantidad   INT NOT NULL,
      Notas      VARCHAR(200) NULL
    );
  ```

- Creación de factura (extracto de flujo)
  - Validar cliente y número de factura.
  - Insertar cabecera (TblFacturas), leer SCOPE_IDENTITY().
  - Insertar detalles (TblDetallesFactura) consultando precio actual desde CatProductos.
  - Actualizar totales (subtotal, IVA 19%, total).
  - Responder con sp_Response.

---

## 🧭 Convenciones de respuesta
- Envelope uniforme:
```json
  {
    "code": 0,
    "message": "Success",
    "data": { /* objeto, lista o id creado */ }
  }
```
- Para listados con conteo:
```json
  {
    "code": 0,
    "message": "Success",
    "data": {
      "count": 42,
      "results": [ /* ... */ ]
    }
  }
```

---

## 💡 Cosas que propondría / siguientes pasos

- Auth con JWT y autorización por roles.

- Validaciones Fluent (ej. FluentValidation) en capa Application.

- Métricas y tracing (Serilog + OpenTelemetry) para performance.

- Cache de catálogos (ej. MemoryCache/Redis) para productos y tipos.

- CI/CD con Docker Compose (API + SQL Server) y GitHub Actions.

- Internacionalización de sp_Response (en-US / es-CO).

- Pruebas automatizadas de servicios y smoke tests de SPs.

---

## 👤 Autor y fecha

Autor: Kevin Sanchez Gomez

Fecha: Septiembre 2025
