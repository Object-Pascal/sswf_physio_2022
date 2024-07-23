using Core.DomainServices;
using HotChocolate;
using HotChocolate.AspNetCore;
using Infrastructure;
using Infrastructure.API;
using Infrastructure.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Portal.API.GraphQL;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Portal.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [System.Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portal.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            services.AddDbContext<SwfApiContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("StamDataAzure"),
                    x => x.MigrationsAssembly("Infrastructure.API"));
            });
            services.AddDbContext<SwfIdentityContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityAzure"),
                    x => x.MigrationsAssembly("Infrastructure"));
            });

            services.AddScoped<IDiagnoseRepository, DiagnoseRepository>();
            services.AddScoped<ITreatmentTypeRepository, TreatmentTypeRepository>();
            services.AddScoped<ApiQuery>();
            services.AddScoped<ApiMutation>();

            services.AddGraphQL(g => SchemaBuilder.New()
                .AddServices(g)
                .AddType<DiagnoseType>()
                .AddType<TreatmentTypeType>()
                .AddQueryType<ApiQuery>()
                .AddMutationType<ApiMutation>()
                .Create());

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<SwfIdentityContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("701f6335-e6c0-4bb4-866c-e08ef2328b9c"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                SwfApiContext.IsDevelopment = true;
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Physio Portal v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapControllers();
            });
        }
    }
}
