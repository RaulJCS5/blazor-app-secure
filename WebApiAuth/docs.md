To **authenticate users across multiple APIs**, your second API (WebApiBlog) needs to **validate JWT tokens** issued by the first API (WebApiAuth). Here’s how you can do it:

---

## **Steps to Authenticate Users in WebApiBlog**

- **Extract and Configure JWT Authentication** in `WebApiBlog`  
- **Use Authentication Middleware** (`UseAuthentication`)  
- **Protect Endpoints with `[Authorize]`**  

---

### **Step 1: Add Authentication to `WebApiBlog`**
Modify `Program.cs` in **WebApiBlog** to validate tokens from WebApiAuth.

#### **Install required package**
Run this command in WebApiBlog’s project:
```sh
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

#### **Update `Program.cs` to Configure JWT Authentication**
Modify `Program.cs` in **WebApiBlog**:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApiBlog.Data;
using WebApiBlog.Repository;
using WebApiBlog.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection") ?? throw new InvalidOperationException("Connection string 'DatabaseConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 🔹 JWT Authentication Configuration
var jwtSecret = builder.Configuration["Jwt:Secret"]; // Same secret from WebApiAuth
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is missing in configuration.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "joncena", // Same issuer from WebApiAuth
            ValidAudience = "joncena", // Same audience from WebApiAuth
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔹 Enable Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```
---

### **Step 2: Protect Endpoints in WebApiBlog**
Now, in your **controller** (e.g., `ProductController`), use `[Authorize]` to protect API routes:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiBlog.Service;

namespace WebApiBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<BaseResponseModel>> GetProducts()
        {
            var products = await productService.GetProducts();
            foreach (var product in products)
            {
                product.Description = product.Description != null ? product.Description : null;
            }
            return Ok(new BaseResponseModel { Success = true, Data = products });
        }
    }
}
```

---

## **How It Works Now**
- **WebApiAuth** issues a JWT when a user logs in.  
- **WebApiBlog** verifies that JWT using the same secret key.  
- Requests to **protected endpoints** (`[Authorize]`) in **WebApiBlog** require a valid JWT in the `Authorization` header.  

---

## **Testing the Integration**
### **Step 1: Get a Token from `WebApiAuth`**
Make a `POST` request to **WebApiAuth’s** login endpoint:
```http
POST https://localhost:7178/api/auth/login
Content-Type: application/json

{
    "Username" : "user123",
    "Password" : "Pass123!"
}
```
Response:
```json
{
    "token": "your_generated_jwt_here"
}
```

### **Step 2: Use Token in WebApiBlog**
Make a `GET` request to **WebApiBlog’s** protected endpoint:
```http
GET https://localhost:7290/api/product
Authorization: Bearer your_generated_jwt_here
```
- **If the token is valid**, the response will contain product data.  
- **If the token is missing or invalid**, the API returns `401 Unauthorized`.

---

## **🔹 Summary**
- **WebApiAuth generates JWT tokens**  
- **WebApiBlog verifies JWT tokens**  
- **Users must send tokens in the `Authorization` header**  
- **Protected endpoints require authentication (`[Authorize]`)**  
