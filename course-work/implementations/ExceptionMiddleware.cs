using DentalClinicApp.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace DentalClinicApp.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next) => _next = next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            var code = ex switch { DbUpdateException => HttpStatusCode.Conflict, KeyNotFoundException => HttpStatusCode.NotFound, ArgumentException => HttpStatusCode.BadRequest, UnauthorizedAccessException => HttpStatusCode.Forbidden, _ => HttpStatusCode.InternalServerError };
            context.Response.StatusCode = (int)code;
            var res = new ErrorResponse((int)code, ex.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(res));
        }
    }
}
