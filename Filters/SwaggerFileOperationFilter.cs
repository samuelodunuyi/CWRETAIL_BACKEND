using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace CWSERVER.Filters
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFileParameter = context.ApiDescription.ParameterDescriptions.Any(p =>
                p.Type == typeof(IFormFile) ||
                Nullable.GetUnderlyingType(p.Type) == typeof(IFormFile) ||
                p.Type == typeof(List<IFormFile>) ||
                (p.Type.IsGenericType && p.Type.GetGenericTypeDefinition() == typeof(List<>) && 
                 Nullable.GetUnderlyingType(p.Type.GetGenericArguments()[0]) == typeof(IFormFile)) ||
                p.Type == typeof(IEnumerable<IFormFile>) ||
                p.Type == typeof(ICollection<IFormFile>));

            if (!hasFileParameter) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    }
                }
            };

            var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                if (parameter.Type == typeof(IFormFile) || Nullable.GetUnderlyingType(parameter.Type) == typeof(IFormFile))
                {
                    schema.Properties[parameter.Name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else if (parameter.Type == typeof(List<IFormFile>) || 
                         (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(List<>) && 
                          Nullable.GetUnderlyingType(parameter.Type.GetGenericArguments()[0]) == typeof(IFormFile)) ||
                         parameter.Type == typeof(IEnumerable<IFormFile>) ||
                         parameter.Type == typeof(ICollection<IFormFile>))
                {
                    schema.Properties[parameter.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    };
                }
                else
                {
                    // Handle other form parameters (like DTOs)
                    try
                    {
                        schema.Properties[parameter.Name] = context.SchemaGenerator.GenerateSchema(parameter.Type, context.SchemaRepository);
                    }
                    catch
                    {
                        // Fallback for complex types
                        schema.Properties[parameter.Name] = new OpenApiSchema { Type = "string" };
                    }
                }

                if (parameter.IsRequired)
                {
                    schema.Required.Add(parameter.Name);
                }
            }

            // Remove parameters that are now part of the request body
            var parametersToRemove = operation.Parameters
                .Where(p => context.ApiDescription.ParameterDescriptions.Any(pd => pd.Name == p.Name && IsFileParameter(pd)))
                .ToList();

            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }
        }

        private bool IsFileParameter(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription parameter)
         {
             return parameter.Type == typeof(IFormFile) ||
                    Nullable.GetUnderlyingType(parameter.Type) == typeof(IFormFile) ||
                    parameter.Type == typeof(List<IFormFile>) ||
                    (parameter.Type.IsGenericType && parameter.Type.GetGenericTypeDefinition() == typeof(List<>) && 
                     Nullable.GetUnderlyingType(parameter.Type.GetGenericArguments()[0]) == typeof(IFormFile)) ||
                    parameter.Type == typeof(IEnumerable<IFormFile>) ||
                    parameter.Type == typeof(ICollection<IFormFile>);
         }
     }
 }