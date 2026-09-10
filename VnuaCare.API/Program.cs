using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VnuaCare.Business.Business.Authorizations.AuthorizationCommands;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Services.CacheService;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Infrastructure.Services;
using VnuaCare.Shared.ContextAccessor;

// Cấu hình Dapper tự động map tên cột dạng snake_case (user_id, password_hash) sang PascalCase (UserId, PasswordHash)
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Cấu hình Swagger UI kèm hỗ trợ JWT Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "V-Care Health API", Version = "v1" });

    // Đọc comment XML từ code để hiển thị mô tả API trên Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Cấu hình nút Authorize trên Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT Bearer Token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    c.DocInclusionPredicate((docName, apiDesc) => true);
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }
        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }
        return new[] { "Uncategorized" };
    });
});

// 3. Kết nối CSDL SQL Server qua Entity Framework Core
builder.Services.AddDbContext<VnuaCareDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<VnuaCareReadDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Đăng ký MediatR xử lý Command & Query
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserLoginCommand).Assembly));

// 5. Đăng ký các dịch vụ cốt lõi (PasswordHasher, JwtService)
builder.Services.AddScoped<IBcryptPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddLocalization();

// 6. Cấu hình xác thực người dùng bằng JWT Bearer Token
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? "VnuaCareHealthSecretKey2026SuperSecureKeyDefault123456!"))
    };

    // Cấu hình nhận Token từ Query String khi kết nối SignalR realtime
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 7. Đăng ký HttpContextAccessor và ContextAccessor Wrapper
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextAccessor, HttpContextAccessorWrapper>();
builder.Services.AddScoped<Func<IContextAccessor>>(sp => () => sp.GetRequiredService<IContextAccessor>());

// 8. Đăng ký Cache Service (Hỗ trợ cả Redis và In-Memory)
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "VnuaCare_";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Kích hoạt Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
