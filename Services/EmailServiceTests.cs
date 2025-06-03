using HangfireJobProcessor.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace HangfireJobProcessor.Test.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(x => x["Email:SmtpHost"]).Returns("smtp.gmail.com");
            _mockConfiguration.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(x => x["Email:Username"]).Returns("test@test.com");
            _mockConfiguration.Setup(x => x["Email:Password"]).Returns("password");
            _mockConfiguration.Setup(x => x["Email:FromAddress"]).Returns("test@test.com");

            _emailService = new EmailService(_mockLogger.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task SendEmailAsync_WithValidParameters_ShouldComplete()
        {
            var to = "recipient@test.com";
            var subject = "Test Subject";
            var body = "Test Body";

            var exception = await Record.ExceptionAsync(() =>
                _emailService.SendEmailAsync(to, subject, body));

            Assert.Null(exception);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Starting email send to {to}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Email sent successfully to {to}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_WithCcAndBcc_ShouldComplete()
        {
            var to = "recipient@test.com";
            var subject = "Test Subject";
            var body = "Test Body";
            var cc = new List<string> { "cc@test.com" };
            var bcc = new List<string> { "bcc@test.com" };

            var exception = await Record.ExceptionAsync(() =>
                _emailService.SendEmailAsync(to, subject, body, cc, bcc, true));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData(null)]
        public async Task SendEmailAsync_WithInvalidEmail_ShouldNotThrow(string? invalidEmail)
        {
            var subject = "Test Subject";
            var body = "Test Body";

            var exception = await Record.ExceptionAsync(() =>
                _emailService.SendEmailAsync(invalidEmail!, subject, body));

            Assert.Null(exception);
        }
    }
}
