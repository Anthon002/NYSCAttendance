using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.JWTHandler;
using NYSCAttendance.Infrastructure.Repos.Integrations.Contracts;
using NYSCAttendance.Infrastructure.Repos.Integrations.Implementations;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Repos.Services.Implementations;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettingsOptions>(configuration.GetSection("ApplicationSettings"));
        services.AddScoped<IUtilityService, UtilityService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHttpClient();
        services.AddScoped<IBrevo, BrevoService>();
        services.AddScoped<IJWTHandler, JWTRequestHandler>();
        return services;
    }

    public static IServiceCollection RegisterIdentity(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = DateTimeOffset.UtcNow.AddYears(50) - DateTimeOffset.UtcNow;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidIssuer = configuration["ApplicationSettings:JwtSettings:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["ApplicationSettings:JwtSettings:Secret"]!)),
            };
        });

        return services;
    }
    public static IServiceCollection RegisterAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppConstants.AdminPolicyCode, policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim("PolicyCode", AppConstants.AdminPolicyCode);
            });
            options.AddPolicy(AppConstants.CorperPolicyCode, policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim("PolicyCode", AppConstants.CorperPolicyCode);
            });
        });

        return services;
    }

    public static IServiceCollection RegisterPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(option =>
        {
            option.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        return services;
    }

    public static IServiceCollection RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NYSC Attendance",
                Version = "1.0"
            });

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = "Authorization"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });

            option.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }

    public static IServiceCollection RegisterCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("corPolicy", opt =>
            {
                var origins = new string[]
                  {
                    "http://localhost:5268",
                    "http://127.0.0.1:5500",
                    "https://nyscattendance.netlify.app"
                  };
                opt.AllowAnyHeader();
                opt.AllowAnyMethod();
                opt.WithOrigins(origins);
            });
        });
        return services;
    }
    public static IServiceCollection RegisterRateLimiter(this IServiceCollection service)
    {
        service.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status400BadRequest;
            options.AddPolicy("policy1", httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 1
                }
            ));
        });

        return service;
    }
}


public static class ModelBuilderExtension
{
    public static void Seed(this ModelBuilder builder)
    {
        builder.Entity<AppUser>().HasData(
            new AppUser
            {
                ConcurrencyStamp = "b52f3b7a-2675-45c4-8f16-25344a3b2b8a",
                Email = "chineduanulugwo@mailinator.com",
                UserName = "chineduanulugwo@mailinator.com",
                FirstName = "Chinedu",
                LastName = "Anulugwo",
                EmailConfirmed = true,
                Id = 1,
                NormalizedEmail = "CHINEDUANULUGWO@MAILINATOR.COM",
                NormalizedUserName = "chineduanulugwo@mailinator.com",
                PasswordHash = "AQAAAAIAAYagAAAAEMJtZS1gNHQEEqgpeQB3izs+BayIhbpPoXLKaJyBsi1tEhyAb0zYUdXV84xiaqYHsw==",
                PhoneNumber = "+2348000000000",
                PhoneNumberConfirmed = true,
                SecurityStamp = "cf0a489e-4c77-4235-9471-8a8ff9db18a2",
                CreatedAt = new DateTimeOffset(2025, 10, 12, 15, 56, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2025, 10, 12, 15, 56, 0, TimeSpan.Zero),
                UserType = UserTypeEnum.Admin
            }
        );

        builder.Entity<AppUserClaim>().HasData(
            new AppUserClaim
            {
                ClaimType = AppConstants.Permission,
                ClaimValue = AppConstants.TeamManagement,
                UserId = 1,
                Id = 1
            }
        );

        builder.Entity<AppUserClaim>().HasData(
            new AppUserClaim
            {
                ClaimType = AppConstants.Permission,
                ClaimValue = AppConstants.LGAManagement,
                UserId = 1,
                Id = 2
            }
        );
    }
}
