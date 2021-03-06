using AuthService;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System.IO;


namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
"/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.ConfigureRepositoryManager();
            services.ConfigureSqlContext(Configuration);
            services.AddControllers(config =>
            {
              config.RespectBrowserAcceptHeader = true;
              config.ReturnHttpNotAcceptable = true;
              config.CacheProfiles.Add("120SecondDuration", new CacheProfile { Duration = 120 });
            }).AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()
            .AddCustomCSVFormatter();
            services.ConfigureCors();
            services.ConfigureLoggerService();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.ConfigureDataShaper();
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            services.ConfigureVersioning();
            services.ConfigureResponseCaching();
            services.ConfigureHttpCachHeaders();
            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);
            services.ConfigureSwagger();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerManager loggerManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            app.ConfigureExceptionHandler(loggerManager);
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "Company Employee API v1");
                s.SwaggerEndpoint("/swagger/v2/swagger.json", "Company Employee API v2");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
