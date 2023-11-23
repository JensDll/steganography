using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Steganography.Test;

internal sealed class GenerateGraphTests
{
    [Test]
    public void Generate_Graph()
    {
        DfaGraphWriter graphWriter = TestSetup.Factory.Services.GetRequiredService<DfaGraphWriter>();
        EndpointDataSource endpointDataSource = TestSetup.Factory.Services.GetRequiredService<EndpointDataSource>();

        using StringWriter writer = new();

        graphWriter.Write(endpointDataSource, writer);

        TestContext.WriteLine(writer.ToString());
    }
}
