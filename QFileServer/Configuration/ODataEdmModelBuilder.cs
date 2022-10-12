using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using QFileServer.Definitions.DTOs;

namespace QFileServer.Configuration
{
    public class ODataEdmModelBuilder
    {
        public static IEdmModel Build()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<QFileServerDTO>("QFileServerOData");
            return builder.GetEdmModel();
        }
    }
}
