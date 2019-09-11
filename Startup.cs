using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using fileuploads3.Models;
using fileuploads3.AwsUtils;
using Amazon.S3;
using Amazon.Runtime;
using Amazon;

namespace fileuploads3
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSwaggerGen(c =>
           {
               c.SwaggerDoc("v1", new Info { Title = "File Upload to S3 API", Version = "v1" });
               c.OperationFilter<FormFileSwaggerFilter>();
           });
            services.AddMvcCore().AddApiExplorer();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            services.Configure<AppSettings>(appSettingsSection);
            services.AddSingleton<IS3Service, S3Service>();
            services.AddAWSService<IAmazonS3>();

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)

        {
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            loggerFactory.AddFile("Logs/Upload2S3API-{Date}.txt");
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger(c =>
            {
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "S3 Bucket API");
            });
        }
    }
    /// <summary>
    /// Filter to enable handling file upload in swagger
    /// </summary>
    public class FormFileSwaggerFilter : IOperationFilter
    {
        private const string formDataMimeType = "multipart/form-data";
        private static readonly string[] formFilePropertyNames =
            typeof(IFormFile).GetTypeInfo().DeclaredProperties.Select(p => p.Name).ToArray();
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var parameters = operation.Parameters;
            if (parameters == null || parameters.Count == 0) return;
            var formFileParameterNames = new List<string>();
            var formFileSubParameterNames = new List<string>();
            foreach (var actionParameter in context.ApiDescription.ActionDescriptor.Parameters)
            {
                var properties =
                    actionParameter.ParameterType.GetProperties()
                        .Where(p => p.PropertyType == typeof(IFormFile))
                        .Select(p => p.Name)
                        .ToArray();
                if (properties.Length != 0)
                {
                    formFileParameterNames.AddRange(properties);
                    formFileSubParameterNames.AddRange(properties);
                    continue;
                }
                if (actionParameter.ParameterType != typeof(IFormFile)) continue;
                formFileParameterNames.Add(actionParameter.Name);
            }
            if (!formFileParameterNames.Any()) return;
            var consumes = operation.Consumes;
            consumes.Clear();
            consumes.Add(formDataMimeType);
            foreach (var parameter in parameters.ToArray())
            {
                if (!(parameter is NonBodyParameter) || parameter.In != "formData") continue;
                if (formFileSubParameterNames.Any(p => parameter.Name.StartsWith(p + "."))
                    || formFilePropertyNames.Contains(parameter.Name))
                    parameters.Remove(parameter);
            }
            foreach (var formFileParameter in formFileParameterNames)
            {
                parameters.Add(new NonBodyParameter()
                {
                    Name = formFileParameter,
                    Type = "file",
                    In = "formData"
                });
            }
        }
    }
}
