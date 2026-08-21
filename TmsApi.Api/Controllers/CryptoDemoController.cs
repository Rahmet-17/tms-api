using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/crypto-demo")]
public class CryptoDemoController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        var service = new CryptoDemoService();

        string hash1 = service.HashUserPassword("Password123!");
        string hash2 = service.HashUserPassword("Password123!");

        // hash1 and hash2 are completely different strings
        // because of unique random salts!
        Console.WriteLine($"Hash 1: {hash1}");
        Console.WriteLine($"Hash 2: {hash2}");

        // Both verify to true against the same plain text:
        bool match1 = service.VerifyUserPassword(
            "Password123!",
            hash1);

        bool match2 = service.VerifyUserPassword(
            "Password123!",
            hash2);

        return Ok(new
        {
            hash1,
            hash2,
            match1,
            match2
        });
    }
}