using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Domain.Exceptions;

namespace Revenda.Vehicles.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var problem = Translate(exception);

            if (problem.Status >= (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Falha não tratada em {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static ProblemDetails Translate(Exception exception) => exception switch
    {
        InvalidStateTransitionException conflict =>
            Build(HttpStatusCode.Conflict, "Operação incompatível com o estado atual", conflict.Message),
        DuplicateLicensePlateException duplicate =>
            Build(HttpStatusCode.Conflict, "Placa já cadastrada", duplicate.Message),

        // Duas compras simultâneas do mesmo veículo: a segunda perde a corrida.
        DbUpdateConcurrencyException =>
            Build(
                HttpStatusCode.Conflict,
                "Veículo indisponível",
                "O veículo foi reservado por outro comprador durante a operação."),

        DomainException domain => Build(HttpStatusCode.BadRequest, "Requisição inválida", domain.Message),
        VehicleNotFoundException notFound =>
            Build(HttpStatusCode.NotFound, "Recurso não encontrado", notFound.Message),
        SaleNotFoundException notFound =>
            Build(HttpStatusCode.NotFound, "Recurso não encontrado", notFound.Message),
        UnauthorizedAccessException unauthorized =>
            Build(HttpStatusCode.Unauthorized, "Não autorizado", unauthorized.Message),
        _ => Build(HttpStatusCode.InternalServerError, "Erro interno", "Não foi possível concluir a operação.")
    };

    private static ProblemDetails Build(HttpStatusCode status, string title, string detail) => new()
    {
        Status = (int)status,
        Title = title,
        Detail = detail
    };
}
