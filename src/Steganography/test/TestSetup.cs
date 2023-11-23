using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Steganography.Test;

[SetUpFixture]
internal sealed class TestSetup
{
    public static TestWebApplicationFactory<Program> Factory { get; private set; } = null!;

    public static HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Factory = new TestWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}

internal sealed class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{ }
