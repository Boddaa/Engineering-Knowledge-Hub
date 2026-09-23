namespace EngineeringKnowledgeHub.Tests.Api;

using System.IO;
using System.Text.Json;
using EngineeringKnowledgeHub.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class GlobalExceptionHandlerTests
{
    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "EngineeringKnowledgeHub.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_Should_Include_Exception_Message()
    {
        // Arrange
        var env = new TestHostEnvironment { EnvironmentName = Environments.Development };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new InvalidOperationException("Sensitive internal crash reason");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.Equal("Sensitive internal crash reason", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_Should_Not_Leak_Exception_Message()
    {
        // Arrange
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new InvalidOperationException("Sensitive database credentials or SQL query");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.DoesNotContain("Sensitive database credentials", problem.Detail);
        Assert.Equal("An unexpected server error occurred. Please try again later.", problem.Detail);
    }
}
