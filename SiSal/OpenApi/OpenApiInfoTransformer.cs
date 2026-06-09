using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SiSal.API.OpenApi
{
    public sealed class OpenApiInfoTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            document.Info = new OpenApiInfo
            {
                Title = "SISAL API",
                Version = "v1",
                Description = "Sistema de control de salidas (SISAL)."
            };
            return Task.CompletedTask;
        }
    }

}
