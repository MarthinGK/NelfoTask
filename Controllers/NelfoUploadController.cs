using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace cwproj.Controllers;

public class HealthCheckRequest
{
    public bool? CheckMoreStuff { get; set; }
}

public class HealthCheckResponse
{
    public string? Message { get; set; }
}

[ApiController]
[Route("[controller]/[action]")]
public class NelfoUploadController : ControllerBase
{
    private readonly ILogger<NelfoUploadController> _logger;

    public NelfoUploadController(ILogger<NelfoUploadController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public HealthCheckResponse HealthCheck(HealthCheckRequest? req)
    {
        _logger.LogInformation($"HealthCheck: {req?.CheckMoreStuff ?? false}");

        return new HealthCheckResponse()
        {
            Message = "OK"
        };
    }

    [HttpPost]
    public IActionResult NelfoFileUpload(IFormFile file)
    {

        var reader = new StreamReader(file.OpenReadStream());

        var json = reader.ReadToEnd();

        var lines = json.Split('\n');

        Dictionary<string, object> vh = new Dictionary<string, object>();
        Dictionary<string, object> products = new Dictionary<string, object>(); ;

        foreach (var line in lines)
        {
            List<string> currentLine = new List<string>();
            var elements = line.Split(';');

            foreach (var element in elements)
            {
                currentLine.Add(element.Trim());
            }

            if (currentLine[0] == "VH" && currentLine[1] == "EFONELFO")
            {
                try
                {
                    vh["orgNo"] = currentLine[3];
                    vh["orgName"] = currentLine[10];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }

            else if (currentLine[0] == "VL" && currentLine[1] == "1")
            {
                try
                {
                    products["productNo"] = currentLine[2];
                    products["description"] = currentLine[3];
                    products["priceUnit"] = currentLine[6];
                    products["price"] = currentLine[8];
                    products["quantity"] = currentLine[9];
                    products["weight"] = currentLine[16];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }

            else if (currentLine[0] == "VX" && currentLine[1] == "VEKT")
            {
                try
                {
                    products["weight"] = currentLine[2];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        Dictionary<string, object> jsonOutput = new Dictionary<string, object>
        {
            { "seller", vh },
            { "products", products },
        };
        string jsonString = JsonConvert.SerializeObject(jsonOutput);

        return Ok(jsonString);
    }
}