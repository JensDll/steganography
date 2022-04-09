using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApi.IntegrationTests;

public class TestingBase
{
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void BaseSetUp()
    {
        WebApplicationFactory<Program> app = new();
        Client = app.CreateClient();
    }
}
