using System.Net.Sockets;
using System.Security.Authentication;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Api.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Database conflict", "The resource changed during the request. Refresh and try again."),
            DbUpdateException { InnerException: SqlException } => (StatusCodes.Status503ServiceUnavailable, "Database unavailable", "The database is temporarily unavailable. Try again later."),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database conflict", "The operation conflicts with existing or related data."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request", "The request could not be processed."),
            FormatException or ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", "One or more request values are invalid."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required."),
            SqlException => (StatusCodes.Status503ServiceUnavailable, "Database unavailable", "The database is temporarily unavailable. Try again later."),
            SmtpCommandException or SmtpProtocolException or ServiceNotConnectedException or SocketException or IOException or AuthenticationException => (StatusCodes.Status503ServiceUnavailable, "External service unavailable", "A required service is temporarily unavailable. Try again later."),
            _ => default
        };

        if (response == default)
        {
            return false;
        }

        logger.LogWarning(exception, "Handled API exception as HTTP {StatusCode}", response.Item1);
        context.Response.StatusCode = response.Item1;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = response.Item1,
            Title = response.Item2,
            Detail = response.Item3,
            Instance = context.Request.Path
        }, cancellationToken);

        return true;
    }
}
