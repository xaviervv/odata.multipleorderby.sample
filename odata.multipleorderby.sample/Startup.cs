using System;
using System.Linq;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using MediatR;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using OData.MultipleOrderBy.Sample.Controllers;

namespace OData.MultipleOrderBy.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAutoMapper(cfg =>
                {
                    cfg.AddExpressionMapping();
                }, typeof(Startup))

                .AddMediatR(typeof(Startup));

            services.AddRouting(options => options.LowercaseUrls = true);

            services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            //.AddFeatureFolders()
            //.AddFluentValidation(fv =>
            //{
            //    fv.RegisterValidatorsFromAssemblyContaining<Startup>();
            //    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false; // only use fluentvalidation
            //});

            services.AddOData();

            services.AddHttpContextAccessor();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), serverOptions =>
                {
                    serverOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ApplicationDbContext.SchemaName);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc(builder =>
            {
                var emBuilder = new ODataConventionModelBuilder();
                // builder.EnableLowerCamelCase();
                emBuilder.EntitySet<RolesDto>("Roles");

                builder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                builder.MapODataServiceRoute("odata", "odata", emBuilder.GetEdmModel());
                builder.EnableDependencyInjection();

                builder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });











            // add some dummy data
            var serviceProvider = app.ApplicationServices;

            // since this only happens once, on startup, create a scoped context for this. Once issue 10000 on github has been fixed, we can start using HasData on the context instead of this.
            using (var scope = serviceProvider.CreateScope())
            {
                // migrate context should be the first operation
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                if (context.Roles == null || !context.Roles.Any())
                {
                    var role1 = new DbRole { Description = "This is an admin group.", Name = "Admins" };
                    var role2 = new DbRole { Description = "This is a user group.", Name = "Users" };
                    context.Roles.Add(role1);
                    context.Roles.Add(role2);
                    
                    var user1 = new DbUser { Description = "First user", Name = new Name("xavier", "van varenberg"), UserName = "hairydruidy" };
                    context.Users.Add(user1);

                    context.SaveChanges();

                    context.UserRoles.Add(new DbUserRole { RoleId = role1.Id, UserId = user1.Id});
                    context.UserRoles.Add(new DbUserRole { RoleId = role2.Id, UserId = user1.Id});

                    context.SaveChanges();
                }

                //context.Dispose();
            }
        }
    }
}
