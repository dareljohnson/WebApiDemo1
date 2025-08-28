# Swagger UI Setup for TodoApp.Api

This guide explains how to add Swagger/OpenAPI documentation and UI to your .NET Framework 4.8.1 Web API project using Swashbuckle.

## 1. Install Swashbuckle via NuGet

- In Visual Studio, open the **Package Manager Console** and run:

```
Install-Package Swashbuckle -Version 5.6.0
```

> **Note:** Swashbuckle 5.x is the latest version supporting .NET Framework Web API (not .NET Core).

## 2. Register Swagger in WebApiConfig

Add the following to `TodoApp.Api/App_Start/WebApiConfig.cs`:

```csharp
using Swashbuckle.Application;
// ...existing code...
public static void Register(HttpConfiguration config)
{
    // ...existing code...
    config.EnableSwagger(c =>
    {
        c.SingleApiVersion("v1", "TodoApp API");
        c.IncludeXmlComments($"{System.AppDomain.CurrentDomain.BaseDirectory}bin/TodoApp.Api.XML");
    })
    .EnableSwaggerUi();
    // ...existing code...
}
```

- Ensure XML documentation is enabled in project properties (Build > Output > XML documentation file).

## 3. Build and Browse

- Rebuild the solution.
- Run the API project and navigate to `/swagger` (e.g., `http://localhost:12345/swagger`).
- The interactive Swagger UI will display all endpoints, models, and allow live testing.

## 4. Customize

- See [Swashbuckle docs](https://github.com/domaindrivendev/Swashbuckle) for advanced config (auth, descriptions, etc).

---

## Troubleshooting
- If you see a 404 at `/swagger`, ensure the NuGet package is installed and `EnableSwagger` is called in `WebApiConfig`.
- For XML comments, ensure the XML file is generated and the path is correct.
