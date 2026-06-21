using System.Net;
using System.Text.Json;
using Athary.API.Middleware;
using Athary.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_ShouldCallNext_WhenNoException()
    {
        var called = false;
        var middleware = new ExceptionMiddleware(_ => { called = true; return Task.CompletedTask; }, _loggerMock.Object);

        var context = new DefaultHttpContext();
        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnNotFound_WhenKeyNotFoundException()
    {
        var middleware = new ExceptionMiddleware(_ => throw new KeyNotFoundException("Not found"), _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Not found", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnForbidden_WhenUnauthorizedAccessException()
    {
        var middleware = new ExceptionMiddleware(_ => throw new UnauthorizedAccessException("Forbidden"), _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Forbidden", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnBadRequest_WhenInvalidOperationException()
    {
        var middleware = new ExceptionMiddleware(_ => throw new InvalidOperationException("Invalid"), _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Invalid", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnInternalServerError_WhenGenericException()
    {
        var middleware = new ExceptionMiddleware(_ => throw new Exception("Something went wrong"), _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("An internal server error occurred.", response.Message);
    }
}
