using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Demosuelos.Api.Services;

public class GeminiInterpretacionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiInterpretacionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> ExplicarResultadosAsync(
        string codigoMuestra,
        decimal? humedad,
        decimal? limiteLiquido,
        decimal? limitePlastico,
        decimal? indicePlasticidad)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "No encontré la API key de Gemini. Configúrala en appsettings.Development.json o en secretos del entorno.";
        }

        var prompt = $"""
Eres un asistente técnico para laboratorio de suelos.

Explica estos resultados en español, de forma concreta, clara y bien explicada, en un solo párrafo, sin listas, sin títulos y sin saltos de línea. Máximo 110 palabras.

Debes:
- explicar brevemente qué representan la humedad natural, el límite líquido, el límite plástico y el índice de plasticidad;
- dar una conclusión general del comportamiento esperado del suelo;
- indicar de forma preliminar qué tipo de construcción ligera o nivel de exigencia podría ser más compatible, solo como orientación general.

No debes:
- afirmar que la obra planeada es viable o inviable de forma definitiva;
- recomendar cimentaciones específicas;
- reemplazar el estudio geotécnico ni el criterio profesional;
- inventar datos faltantes.

Aclara al final, en una sola frase corta dentro del mismo párrafo, que la conclusión es orientativa y que para definir viabilidad real se requieren más datos geotécnicos.

Datos:
Muestra: {codigoMuestra}
Humedad natural: {Formatear(humedad)} %
Límite líquido: {Formatear(limiteLiquido)} %
Límite plástico: {Formatear(limitePlastico)} %
Índice de plasticidad: {Formatear(indicePlasticidad)} %
""";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent");

        request.Headers.Add("x-goog-api-key", apiKey);

        request.Content = JsonContent.Create(new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        });

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return $"Gemini devolvió error {(int)response.StatusCode}: {responseText}";
        }

        using var doc = JsonDocument.Parse(responseText);

        var texto = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return string.IsNullOrWhiteSpace(texto)
            ? "Gemini no devolvió texto."
            : texto;
    }

    private static string Formatear(decimal? valor)
    {
        return valor.HasValue
            ? valor.Value.ToString("0.00", CultureInfo.InvariantCulture)
            : "N/D";
    }
}