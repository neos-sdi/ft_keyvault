namespace FormuleTech.AppConfig.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly string connectionString = "Data Source=127.0.0.1, 1433;Persist Security Info=True;User ID=sa;Connect Timeout=60;Database=ft;TrustServerCertificate=True;pwd=P@ssword";
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<string> Get()
    {
        var result = new List<string>();
        using (var connection = new SqlConnection(connectionString))
        {
            using (var command = new SqlCommand("Select Name from Cities", connection))
            {
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
                connection.Close();
            }
        }
        return result;
    }
}
