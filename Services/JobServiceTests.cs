using Hangfire;
using HangfireJobProcessor.Models;
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
    public class JobServiceTests
    {
        private readonly Mock<ILogger<JobService>> _mockLogger;
        private readonly JobService _jobService;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;

        public JobServiceTests()
        {
            _mockLogger = new Mock<ILogger<JobService>>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _jobService = new JobService(_mockLogger.Object);
        }

        [Fact]
        public void EnqueueEmailJob_WithValidRequest_ShouldReturnJobId()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            var result = _jobService.EnqueueEmailJob(request);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void EnqueueReportJob_WithValidRequest_ShouldReturnJobId()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object> { { "Year", 2023 } },
                OutputFormat = "PDF"
            };

            var result = _jobService.EnqueueReportJob(request);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ScheduleEmailJob_WithValidRequest_ShouldReturnJobId()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Scheduled Email",
                Body = "This is a scheduled email"
            };
            var scheduledAt = DateTime.UtcNow.AddHours(1);

            var result = _jobService.ScheduleEmailJob(request, scheduledAt);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ScheduleReportJob_WithValidRequest_ShouldReturnJobId()
        {
            var request = new ReportJobRequest
            {
                ReportType = "MonthlyReport",
                Parameters = new Dictionary<string, object> { { "Month", "January" } }
            };
            var scheduledAt = DateTime.UtcNow.AddDays(1);

            var result = _jobService.ScheduleReportJob(request, scheduledAt);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
