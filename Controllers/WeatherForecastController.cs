using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;

namespace MiddlewareDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Random _random = new();


    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        // Get the "X-Correlation-Id" header from the request
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
        logger.LogInformation("Handling the request. CorrelationId:{CorrelationId}", correlationId);
        // Call another service with the same "X-Correlation-Id" headerwhen you set up the HttpClient
        //var httpContent = new StringContent("Hello world!");
        //httpContent.Headers.Add("X-Correlation-Id", correlationId);
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("rate-limiting")]
    [EnableRateLimiting(policyName: "fixed")]
    public ActionResult RateLimitingDemo()
    {
        return Ok($"Hello {DateTime.Now.Ticks.ToString()}");
    }
    [HttpGet("request-timeout")]
    [RequestTimeout(5000)]
    public async Task<ActionResult> RequestTimeoutDemo()
    {
        var delay = _random.Next(1, 10);
        logger.LogInformation($"Delaying for {delay} seconds");
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), Request.
            HttpContext.RequestAborted);
        }
        catch
        {
            logger.LogWarning("The request timed out");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
            "The request timed out");
        }
        return Ok($"Hello! The task is complete in {delay} seconds");
    }
}
