using Core.Domain;
using Core.DomainServices;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Web_App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPatientFileRepository, PatientFileRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IRemarkRepository, RemarkRepository>();
            services.AddScoped<ITherapistRepository, TherapistRepository>();
            services.AddScoped<ITreatmentRepository, TreatmentRepository>();
            services.AddScoped<ITreatmentPlanRepository, TreatmentPlanRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<SwfIdentityContext>()
                .AddDefaultTokenProviders();

            AuthorizationOptions options = new AuthorizationOptions();
            options.AddPolicy(Role.ADMINISTRATOR_ROLE, policy => policy.RequireClaim(Role.ADMINISTRATOR_ROLE));
            options.AddPolicy(Role.THERAPIST_ROLE, policy => policy.RequireClaim(Role.THERAPIST_ROLE));
            options.AddPolicy(Role.STUDENTTHERAPIST_ROLE, policy => policy.RequireClaim(Role.STUDENTTHERAPIST_ROLE));
            options.AddPolicy(Role.PATIENT_ROLE, policy => policy.RequireClaim(Role.PATIENT_ROLE));

            services.AddAuthorization(x => x = options);

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.LoginPath = "/Login/Index";
            });

            services.AddControllers()
                .AddNewtonsoftJson();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddHealthChecks();
            services.AddRazorPages();

            bool development = false;
            services.AddDbContext<SwfContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString(development ? "DataLocal" : "DataAzure"),
                    x => x.MigrationsAssembly("Infrastructure"));
            });

            services.AddDbContext<SwfIdentityContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString(development ? "IdentityLocal" : "IdentityAzure"),
                    x => x.MigrationsAssembly("Infrastructure"));
            });

            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");

                endpoints.MapControllerRoute(
                    name: "slug",
                    pattern: "patient",
                    defaults: new { controller = "Home", action = "Index"});
            });
        }
    }
}