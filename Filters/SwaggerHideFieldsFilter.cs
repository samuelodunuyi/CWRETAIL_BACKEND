using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CWSERVER.Filters
{
    public class SwaggerHideFieldsFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context.Type == null)
                return;

            // Get properties that should be hidden in Swagger
            var hiddenProperties = context.Type.GetProperties()
                .Where(p => 
                    p.Name == "Id" || 
                    p.Name.EndsWith("Id") || 
                    p.Name == "CreatedAt" || 
                    p.Name == "CreatedBy" || 
                    p.Name == "UpdatedAt" || 
                    p.Name == "UpdatedBy")
                .Select(p => p.Name.ToCamelCase()); // Convert to camelCase for JSON

            // Remove hidden properties from required list
            if (schema.Required != null)
            {
                foreach (var propertyName in hiddenProperties)
                {
                    if (schema.Required.Contains(propertyName))
                    {
                        schema.Required.Remove(propertyName);
                    }
                }
            }

            // Remove hidden properties from schema
            foreach (var propertyName in hiddenProperties)
            {
                if (schema.Properties.ContainsKey(propertyName))
                {
                    schema.Properties.Remove(propertyName);
                }
            }
        }
    }

    // Extension method to convert property names to camelCase
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
                return str;

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}