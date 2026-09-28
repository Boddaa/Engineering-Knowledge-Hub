namespace EngineeringKnowledgeHub.Tests.Api;

using System.IO;
using System.Text.Json;
using EngineeringKnowledgeHub.Api.Common;
using EngineeringKnowledgeHub.Application.Common.Exceptions;
using EngineeringKnowledgeHub.Domain.Exceptions;
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
    public async Task TryHandleAsync_InDevelopment_KnownException_Should_Include_Exception_Message()
    {
        // Arrange
        var env = new TestHostEnvironment { EnvironmentName = Environments.Development };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new DomainRuleViolationException("Domain rule violated in development mode.");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("Domain rule violated in development mode.", problem.Detail);
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

    [Fact]
    public async Task TryHandleAsync_InProduction_UnexpectedException_Should_Not_Leak_StackTrace_Or_ConnectionStrings()
    {
        // Arrange
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new Exception("Server=dbprod01;Database=EKH_Prod;Password=Secret123;\n   at System.Data.SqlClient.Open()");

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
        Assert.DoesNotContain("dbprod01", problem.Detail);
        Assert.DoesNotContain("Password", problem.Detail);
        Assert.DoesNotContain("System.Data", problem.Detail);
        Assert.Equal("An unexpected server error occurred. Please try again later.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_KnownException_WithSensitiveDetails_Should_Sanitize_Message()
    {
        // Arrange
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new ValidationException("Failed query SELECT * FROM Users WHERE Password='xyz'");

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.DoesNotContain("SELECT", problem.Detail);
        Assert.DoesNotContain("Password", problem.Detail);
        Assert.Equal("The request failed validation.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Should_Map_To_404_NotFound()
    {
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new NotFoundException("KnowledgeArticle", Guid.NewGuid());

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Should_Map_To_400_BadRequest()
    {
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new ValidationException("Field 'Title' is invalid.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("Field 'Title' is invalid.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_Should_Map_To_409_Conflict()
    {
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new ConflictException("Slug already exists.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("Slug already exists.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_DomainRuleViolationException_Should_Map_To_409_Conflict()
    {
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new DomainRuleViolationException("Author cannot approve own article.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("Author cannot approve own article.", problem.Detail);
    }

    private class TestDomainException(string message) : DomainException(message);

    [Fact]
    public async Task TryHandleAsync_DomainException_Should_Map_To_400_BadRequest()
    {
        var env = new TestHostEnvironment { EnvironmentName = Environments.Production };
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new TestDomainException("Generic domain rule violation");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal("Generic domain rule violation", problem.Detail);
    }
}
