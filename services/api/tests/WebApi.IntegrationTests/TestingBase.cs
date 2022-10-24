using System;
using System.Net.Http;
using System.Text;
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

    protected static string AlterBase64Key(string key)
    {
        StringBuilder keyBuilder = new(key);
        int lastIdx = Array.IndexOf(_base64Chars, keyBuilder[^1]);

        if (lastIdx == _base64Chars.Length - 1)
        {
            keyBuilder[^1] = _base64Chars[0];
        }
        else
        {
            keyBuilder[^1] = _base64Chars[lastIdx + 1];
        }

        return keyBuilder.ToString();
    }

    private static readonly char[] _base64Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();
}
