using System;
using System.Text;
using System.Text.Json;

// Usage: dotnet run --project tools/TokenGenerator -- '{"name":"user1","sub":"user1","role":"Inputter"}'
if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project tools/TokenGenerator -- '{\"name\":\"user1\",\"sub\":\"user1\",\"role\":\"Inputter\"}'");
    return;
}

var payloadJson = args[0];
var headerJson = JsonSerializer.Serialize(new { alg = "none", typ = "JWT" });

static string Base64UrlEncode(string input)
{
    var bytes = Encoding.UTF8.GetBytes(input);
    var base64 = Convert.ToBase64String(bytes);
    base64 = base64.Split('=')[0]; // Remove any trailing '='
    base64 = base64.Replace('+', '-');
    base64 = base64.Replace('/', '_');
    return base64;
}

var token = $"{Base64UrlEncode(headerJson)}.{Base64UrlEncode(payloadJson)}.";
Console.WriteLine(token);
