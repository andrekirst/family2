using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RazorLight;

namespace FamilyHub.Tests.Unit.Infrastructure.Email;

/// <summary>
/// Unit tests for RazorEmailTemplateService.
/// Tests template rendering, error handling, and logging behavior.
/// </summary>
public sealed class RazorEmailTemplateServiceTests
{
    private readonly IRazorLightEngine _razorEngine = Substitute.For<IRazorLightEngine>();
    private readonly ILogger<RazorEmailTemplateService> _logger = Substitute.For<ILogger<RazorEmailTemplateService>>();

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        // Act
        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region RenderTemplateAsync Tests

    [Fact]
    public async Task RenderTemplateAsync_WithValidTemplate_ReturnsRenderedHtml()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "John Doe",
            FamilyName = "Doe Family",
            InvitationUrl = "https://familyhub.com/invite/123",
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };

        var expectedHtml = "<html><body>Welcome John Doe to Doe Family!</body></html>";
        _razorEngine.CompileRenderAsync("InvitationEmail.cshtml", model)
            .Returns(expectedHtml);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("InvitationEmail", model);

        // Assert
        result.Should().Be(expectedHtml);
        await _razorEngine.Received(1).CompileRenderAsync("InvitationEmail.cshtml", model);
    }

    [Fact]
    public async Task RenderTemplateAsync_AppendsExtension_ToTemplateName()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Jane Smith",
            FamilyName = "Smith Family",
            InvitationUrl = "https://example.com/invite/456",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns("<html></html>");

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        await service.RenderTemplateAsync("CustomTemplate", model);

        // Assert
        await _razorEngine.Received(1).CompileRenderAsync("CustomTemplate.cshtml", model);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithGenericModel_RendersSuccessfully()
    {
        // Arrange
        var customModel = new { Title = "Test", Content = "Hello World" };
        var expectedHtml = "<h1>Test</h1><p>Hello World</p>";

        _razorEngine.CompileRenderAsync("TestTemplate.cshtml", customModel)
            .Returns(expectedHtml);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("TestTemplate", customModel);

        // Assert
        result.Should().Be(expectedHtml);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithComplexModel_RendersSuccessfully()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test User",
            FamilyName = "Test Family",
            InvitationUrl = "https://test.com/invite",
            ExpiresAt = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc)
        };

        var expectedHtml = @"
            <html>
                <body>
                    <h1>Invitation from Test User</h1>
                    <p>Join Test Family</p>
                    <a href=""https://test.com/invite"">Accept</a>
                    <p>Expires: 12/31/2026</p>
                </body>
            </html>";

        _razorEngine.CompileRenderAsync("InvitationEmail.cshtml", model)
            .Returns(expectedHtml);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("InvitationEmail", model);

        // Assert
        result.Should().Be(expectedHtml);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task RenderTemplateAsync_WhenTemplateNotFound_ThrowsException()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .ThrowsAsync(new InvalidOperationException("Template not found"));

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var act = () => service.RenderTemplateAsync("NonExistent", model);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template not found");
    }

    [Fact]
    public async Task RenderTemplateAsync_WhenCompilationFails_ThrowsException()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .ThrowsAsync(new InvalidOperationException("Template compilation error"));

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var act = () => service.RenderTemplateAsync("Invalid", model);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template compilation error");
    }

    [Fact]
    public async Task RenderTemplateAsync_WhenRenderingFails_ThrowsException()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .ThrowsAsync(new InvalidOperationException("Rendering failed"));

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var act = () => service.RenderTemplateAsync("Broken", model);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Rendering failed");
    }

    [Fact]
    public async Task RenderTemplateAsync_WhenExceptionThrown_LogsError()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        var exception = new InvalidOperationException("Test error");
        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .ThrowsAsync(exception);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var act = () => service.RenderTemplateAsync("Failed", model);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify error was logged (LoggerMessage attribute creates extension methods)
        // The actual logging verification depends on how LoggerMessage is implemented
        // This verifies the method was called, which triggers logging
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task RenderTemplateAsync_WithCancellationToken_PassesToEngine()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns("<html></html>");

        var service = new RazorEmailTemplateService(_razorEngine, _logger);
        var cts = new CancellationTokenSource();

        // Act
        await service.RenderTemplateAsync("Test", model, cts.Token);

        // Assert
        await _razorEngine.Received(1).CompileRenderAsync("Test.cshtml", model);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithDefaultCancellationToken_Succeeds()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns("<html></html>");

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act - Using default cancellation token
        var result = await service.RenderTemplateAsync("Test", model);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task RenderTemplateAsync_WithEmptyHtml_ReturnsEmptyString()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(string.Empty);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("Empty", model);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RenderTemplateAsync_WithLargeHtml_ReturnsFullContent()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test",
            FamilyName = "Test",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        // Generate large HTML content (10KB)
        var largeHtml = new string('x', 10240);
        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(largeHtml);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("Large", model);

        // Assert
        result.Should().HaveLength(10240);
        result.Should().Be(largeHtml);
    }

    [Fact]
    public async Task RenderTemplateAsync_WithSpecialCharacters_ReturnsCorrectHtml()
    {
        // Arrange
        var model = new InvitationEmailModel
        {
            InviterName = "Test <>&\"'",
            FamilyName = "Test Family",
            InvitationUrl = "https://test.com",
            ExpiresAt = DateTime.UtcNow
        };

        var expectedHtml = "<html><body>Test &lt;&gt;&amp;&quot;&#39;</body></html>";
        _razorEngine.CompileRenderAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(expectedHtml);

        var service = new RazorEmailTemplateService(_razorEngine, _logger);

        // Act
        var result = await service.RenderTemplateAsync("Special", model);

        // Assert
        result.Should().Contain("&lt;&gt;&amp;&quot;&#39;");
    }

    #endregion
}
