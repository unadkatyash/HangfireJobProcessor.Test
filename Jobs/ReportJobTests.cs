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
    public class ReportJobTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<ReportJob>> _mockLogger;
        private readonly ReportJob _reportJob;

        public ReportJobTests()
        {
            _mockReportService = new Mock<IReportService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<ReportJob>>();
            _reportJob = new ReportJob(_mockReportService.Object, _mockEmailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessReportJob_WithValidRequest_ShouldCallReportService()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object> { { "Year", 2023 } },
                OutputFormat = "PDF"
            };

            var reportData = new byte[] { 1, 2, 3, 4, 5 };
            _mockReportService
                .Setup(x => x.GenerateReportAsync(request.ReportType, request.Parameters, request.OutputFormat))
                .ReturnsAsync(reportData);

            await _reportJob.ProcessReportJob(request);

            _mockReportService.Verify(
                x => x.GenerateReportAsync(request.ReportType, request.Parameters, request.OutputFormat),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReportJob_WithEmailTo_ShouldSendEmail()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object>(),
                OutputFormat = "PDF",
                EmailTo = "recipient@example.com"
            };

            var reportData = new byte[] { 1, 2, 3, 4, 5 };
            _mockReportService
                .Setup(x => x.GenerateReportAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(reportData);

            await _reportJob.ProcessReportJob(request);

            _mockEmailService.Verify(
                x => x.SendEmailAsync(request.EmailTo, It.Is<string>(s => s.Contains("SalesReport Report")),
                    It.IsAny<string>(), null, null, true),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReportJob_WithoutEmailTo_ShouldNotSendEmail()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object>(),
                OutputFormat = "PDF"
            };

            var reportData = new byte[] { 1, 2, 3, 4, 5 };
            _mockReportService
                .Setup(x => x.GenerateReportAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(reportData);

            await _reportJob.ProcessReportJob(request);

            _mockEmailService.Verify(
                x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessReportJob_WhenReportServiceThrows_ShouldPropagateException()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object>(),
                OutputFormat = "PDF"
            };

            var expectedException = new InvalidOperationException("Report generation failed");
            _mockReportService
                .Setup(x => x.GenerateReportAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ThrowsAsync(expectedException);

            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _reportJob.ProcessReportJob(request));

            Assert.Equal(expectedException.Message, actualException.Message);
        }

        [Fact]
        public async Task ProcessReportJob_ShouldLogCorrectMessages()
        {
            var request = new ReportJobRequest
            {
                ReportType = "TestReport",
                Parameters = new Dictionary<string, object>(),
                OutputFormat = "PDF"
            };

            var reportData = new byte[] { 1, 2, 3, 4, 5 };
            _mockReportService
                .Setup(x => x.GenerateReportAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(reportData);

            await _reportJob.ProcessReportJob(request);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processing report job: {request.ReportType}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Report job completed successfully: {request.ReportType}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
