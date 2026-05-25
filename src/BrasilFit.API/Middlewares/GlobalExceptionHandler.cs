using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Middlewares;

// Implementa IExceptionHandler (mecanismo oficial desde .NET 8) em conjunto com
// AddProblemDetails() registrado no Program.cs.
// Responsabilidade unica: traduzir excecoes para respostas RFC 7807 (ProblemDetails),
// deixando o front com um shape de erro previsivel.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, titulo) = MapearExcecao(exception);

        _logger.Log(
            statusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
            exception,
            "Excecao {Tipo} interceptada: {Mensagem}",
            exception.GetType().Name,
            exception.Message);

        var problema = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = exception.Message,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path
        };

        // TraceId facilita correlacionar com logs do servidor durante o debug.
        problema.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Em desenvolvimento devolvemos o stack para acelerar o debug do front.
        // Em producao essa informacao NUNCA deve vazar.
        if (_env.IsDevelopment() && statusCode >= 500)
        {
            problema.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problema, ct);
        return true; // sinaliza que a excecao ja foi tratada
    }

    private static (int Status, string Titulo) MapearExcecao(Exception ex) => ex switch
    {
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden,    "Acesso negado."),
        KeyNotFoundException        => (StatusCodes.Status404NotFound,     "Recurso nao encontrado."),
        InvalidOperationException   => (StatusCodes.Status400BadRequest,   "Regra de negocio violada."),
        ArgumentException           => (StatusCodes.Status400BadRequest,   "Requisicao invalida."),
        NotImplementedException     => (StatusCodes.Status501NotImplemented, "Funcionalidade nao implementada."),
        TimeoutException            => (StatusCodes.Status504GatewayTimeout, "Tempo limite excedido."),
        _                           => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
    };
}
