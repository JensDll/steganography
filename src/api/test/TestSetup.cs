using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace steganography.api.tests;

[SetUpFixture]
internal class TestSetup
{
    private static TestWebApplicationFactory<Program> s_factory = null!;

    internal static HttpClient Client = null!;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        s_factory = new TestWebApplicationFactory<Program>();
        Client = s_factory.CreateClient();
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        s_factory.Dispose();
        Client.Dispose();
    }
}

internal class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{ }
