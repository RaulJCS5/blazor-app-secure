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

## Docker

### Summary of Changes for Docker Orchestration

#### 1. Moving the Solution File
- **Moved the Solution File**: The solution file (`BlazorAppSecure.sln`) was moved to a parent directory to separate it from the project files.
- **Updated the Project Path in the Solution File**: The path to the project file in the solution file was updated to reflect the new location.

#### 2. Docker Configuration

-  __Container Orchestrator Support...__
   - **Added Docker Orchestrator Support**: Created a docker-compose.yml file to define the services required for your application.

#### 3. Defined Services in 

docker-compose.yml


- **blazorappsecure**:
  - **Image**: `${DOCKER_REGISTRY-}blazorappsecure`
  - **Container Name**: blazorappsecure
  - **Build Context**: `.`
  - **Dockerfile**: Dockerfile
  - **Ports**: `5000:5000`, `5001:5001`
  
- **blazorappsecure.database**:
   - **Image**: `postgres:latest`
   - **Container Name**: `blazorappsecure.database`
   - **Environment Variables**: 
      - `POSTGRES_USER: postgres`
      - `POSTGRES_PASSWORD: postgres`
      - `POSTGRES_DB: blazorappsecure`
   - **Volumes**: `./.containers/blazorappsecure.database:/var/lib/postgresql/data`
   - **Ports**: `5432:5432`
  
- **pgadmin**:
   - **Image**: `dpage/pgadmin4`
   - **Container Name**: `pgadmin`
   - **Environment Variables**:
      - `PGADMIN_DEFAULT_EMAIL: admin@example.com`
      - `PGADMIN_DEFAULT_PASSWORD: admin`
   - **Ports**: `5050:80`
   - **Depends On**: `blazorappsecure.database`

#### 4. Connection String Update
- **Updated Connection String**: Modified the connection string in your configuration to connect to the PostgreSQL database running in the Docker container.
  ```json
  "ConnectionStrings": {
    "Database": "Host=blazorappsecure.database;Port=5432;Database=blazorappsecure;Username=postgres;Password=postgres;Include Error Detail=true"
  }
  ```

#### 5. pgAdmin for Browser Access

- **Added pgAdmin Service**: Included pgAdmin in the docker-compose.yml file to manage the PostgreSQL database through a web interface.

- **Access pgAdmin**: After running `docker-compose up`, you can access pgAdmin in your browser at `http://localhost:5050` using the credentials specified in the environment variables.

#### 6. Docker Ignore Configuration

- **Created .dockerignore File**: Added a .dockerignore file to exclude unnecessary files and directories from the Docker build context.

#### 7. Git Ignore Configuration
- **Updated .gitignore File**: Added the .containers directory to the .gitignore file to exclude it from version control.

  ```ignore
  **/.containers
  ```
