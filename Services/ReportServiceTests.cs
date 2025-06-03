using HangfireJobProcessor.Service;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireJobProcessor.Test.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<ILogger<ReportService>> _mockLogger;
        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            _mockLogger = new Mock<ILogger<ReportService>>();
            _reportService = new ReportService(_mockLogger.Object);
        }

        [Fact]
        public async Task GenerateReportAsync_WithValidParameters_ShouldReturnReportData()
        {
            var reportType = "SalesReport";
            var parameters = new Dictionary<string, object>
            {
                { "StartDate", "2023-01-01" },
                { "EndDate", "2023-12-31" }
            };
            var outputFormat = "PDF";

            var result = await _reportService.GenerateReportAsync(reportType, parameters, outputFormat);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            var reportContent = System.Text.Encoding.UTF8.GetString(result);
            Assert.Contains(reportType, reportContent);
            Assert.Contains(outputFormat, reportContent);
            Assert.Contains("StartDate=2023-01-01", reportContent);
            Assert.Contains("EndDate=2023-12-31", reportContent);
        }

        [Theory]
        [InlineData("SalesReport", "PDF")]
        [InlineData("InventoryReport", "Excel")]
        [InlineData("UserReport", "CSV")]
        public async Task GenerateReportAsync_WithDifferentFormats_ShouldReturnCorrectFormat(string reportType, string format)
        {
            var parameters = new Dictionary<string, object> { { "TestParam", "TestValue" } };

            var result = await _reportService.GenerateReportAsync(reportType, parameters, format);

            Assert.NotNull(result);
            var reportContent = System.Text.Encoding.UTF8.GetString(result);
            Assert.Contains(reportType, reportContent);
            Assert.Contains(format, reportContent);
        }

        [Fact]
        public async Task GenerateReportAsync_WithEmptyParameters_ShouldComplete()
        {
            var reportType = "BasicReport";
            var parameters = new Dictionary<string, object>();

            var result = await _reportService.GenerateReportAsync(reportType, parameters);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateReportAsync_ShouldLogCorrectMessages()
        {
            var reportType = "TestReport";
            var parameters = new Dictionary<string, object>();

            await _reportService.GenerateReportAsync(reportType, parameters);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Generating {reportType} report")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Report generated successfully: {reportType}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
