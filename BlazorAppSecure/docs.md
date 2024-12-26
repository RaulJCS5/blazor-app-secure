# Documentation

- Authentication with Blazor Server App ASP.NET Core Identity in .NET 9

## Nugget package installed

To install the specified NuGet packages via the command line, you can use the `dotnet add package` command for each package. Here are the commands you need to run:


```sh
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Swashbuckle.AspNetCore
```

You can also check for the latest versions of these packages on the [NuGet Gallery](https://www.nuget.org/) and update the version numbers accordingly.

Run these commands in the terminal from the root directory of your project. This will add the specified packages to your project file (`BlazorAppSecure.csproj`).

## How to generate database migrations

To generate database migrations for your project, follow these steps:

1. **Ensure the Microsoft.EntityFrameworkCore.Tools Package is Installed**:
   Make sure you have the Microsoft.EntityFrameworkCore.Tools

2. **Add a `DbContext` Class**:
   Ensure you have a `DbContext` class defined in your project. It looks like you already have one named 


3. **Add a Connection String**:
   Ensure your connection string is correctly configured in appsettings.json

4. **Generate Migrations**:
   Open a terminal in the root of your project and run the following command to generate a new migration:

```sh
dotnet ef migrations add InitialCreate
```

```sh
PS C:\Users\<your_user>\OneDrive - PGA\Desktop\repo\BlazorAppSecure> dotnet ef migrations add InitialCreate
Build started...
Build succeeded.
The Entity Framework tools version '8.0.10' is older than that of the runtime '9.0.0'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
Done. To undo this action, use 'ef migrations remove'
```
    
   This command will create a new migration file in the `Migrations` folder (or a folder you specify).

5. **Apply Migrations**:
   To apply the migrations to your database, run the following command:

   ```sh
   dotnet ef database update
   ```

Here is a summary of the commands you need to run:

```sh
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Make sure your Program.cs file includes the necessary setup for Entity Framework Core, which it looks like it already does:

```csharp


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
```