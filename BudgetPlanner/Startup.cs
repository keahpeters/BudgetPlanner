using BudgetPlanner.Identity;
using BudgetPlanner.Models;
using BudgetPlanner.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetPlanner
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(this.Configuration);
            services.AddSingleton<IDbConnectionFactory>(s => new SqlConnectionFactory(this.Configuration.GetConnectionString("BudgetPlanner")));
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<ICategoryGroupRepository, CategoryGroupRepository>();
            services.AddSingleton<ICategoryRepository, CategoryRepository>();
            services.AddIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();
            services.AddTransient<IUserStore<ApplicationUser>, UserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, RoleStore>();
            services.AddTransient<IUserManagerWrapper<ApplicationUser>, UserManagerWrapper<ApplicationUser>>();
            services.AddTransient<ISignInManagerWrapper<ApplicationUser>, SignInManagerWrapper<ApplicationUser>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseFileServer();
            app.UseNodeModules(env.ContentRootPath);
            app.UseAuthentication();
            app.UseMvc(this.ConfigureRoutes);
            app.Run(ctx => ctx.Response.WriteAsync("Not found"));
        }

        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Default",
                "{controller=Home}/{action=Index}/{id?}");
        }
    }
}