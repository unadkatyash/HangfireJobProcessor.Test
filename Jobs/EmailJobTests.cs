using HangfireJobProcessor.IService;
using HangfireJobProcessor.Jobs;
using HangfireJobProcessor.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireJobProcessor.Test.Jobs
{
    public class EmailJobTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<EmailJob>> _mockLogger;
        private readonly EmailJob _emailJob;

        public EmailJobTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<EmailJob>>();
            _emailJob = new EmailJob(_mockEmailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessEmailJob_WithValidRequest_ShouldCallEmailService()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = true
            };

            _mockEmailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            await _emailJob.ProcessEmailJob(request);

            _mockEmailService.Verify(
                x => x.SendEmailAsync(request.To, request.Subject, request.Body, request.Cc, request.Bcc, request.IsHtml),
                Times.Once);
        }

        [Fact]
        public async Task ProcessEmailJob_WithCcAndBcc_ShouldPassCorrectParameters()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                Cc = new List<string> { "cc1@example.com", "cc2@example.com" },
                Bcc = new List<string> { "bcc@example.com" },
                IsHtml = false
            };

            await _emailJob.ProcessEmailJob(request);

            _mockEmailService.Verify(
                x => x.SendEmailAsync(request.To, request.Subject, request.Body, request.Cc, request.Bcc, false),
                Times.Once);
        }

        [Fact]
        public async Task ProcessEmailJob_WhenEmailServiceThrows_ShouldPropagateException()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            var expectedException = new InvalidOperationException("Email service failed");
            _mockEmailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .ThrowsAsync(expectedException);

            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _emailJob.ProcessEmailJob(request));

            Assert.Equal(expectedException.Message, actualException.Message);
        }

        [Fact]
        public async Task ProcessEmailJob_ShouldLogCorrectMessages()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            await _emailJob.ProcessEmailJob(request);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processing email job for {request.To}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Email job completed successfully for {request.To}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
