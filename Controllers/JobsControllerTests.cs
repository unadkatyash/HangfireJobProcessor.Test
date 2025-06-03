using HangfireJobProcessor.Controllers;
using HangfireJobProcessor.IService;
using HangfireJobProcessor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireJobProcessor.Test.Controllers
{
    public class JobsControllerTests
    {
        private readonly Mock<IJobService> _mockJobService;
        private readonly Mock<ILogger<JobsController>> _mockLogger;
        private readonly JobsController _controller;

        public JobsControllerTests()
        {
            _mockJobService = new Mock<IJobService>();
            _mockLogger = new Mock<ILogger<JobsController>>();
            _controller = new JobsController(_mockJobService.Object, _mockLogger.Object);
        }

        [Fact]
        public void EnqueueEmail_WithValidRequest_ShouldReturnOkResult()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };
            var jobId = "test-job-id";

            _mockJobService
                .Setup(x => x.EnqueueEmailJob(request))
                .Returns(jobId);

            var result = _controller.EnqueueEmail(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobResponse>(okResult.Value);
            Assert.Equal(jobId, response.JobId);
            Assert.Equal("Enqueued", response.Status);
        }

        [Fact]
        public void EnqueueEmail_WhenServiceThrows_ShouldReturnBadRequest()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockJobService
                .Setup(x => x.EnqueueEmailJob(request))
                .Throws(new InvalidOperationException("Service error"));

            var result = _controller.EnqueueEmail(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<JobResponse>(badRequestResult.Value);
            Assert.Equal("Error", response.Status);
            Assert.Equal("Service error", response.Message);
        }

        [Fact]
        public void EnqueueReport_WithValidRequest_ShouldReturnOkResult()
        {
            var request = new ReportJobRequest
            {
                ReportType = "SalesReport",
                Parameters = new Dictionary<string, object> { { "Year", 2023 } }
            };
            var jobId = "report-job-id";

            _mockJobService
                .Setup(x => x.EnqueueReportJob(request))
                .Returns(jobId);

            var result = _controller.EnqueueReport(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobResponse>(okResult.Value);
            Assert.Equal(jobId, response.JobId);
            Assert.Equal("Enqueued", response.Status);
        }

        [Fact]
        public void ScheduleEmail_WithValidRequest_ShouldReturnOkResult()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Scheduled Email",
                Body = "This will be sent later"
            };
            var scheduledAt = DateTime.UtcNow.AddHours(1);
            var jobId = "scheduled-job-id";

            _mockJobService
                .Setup(x => x.ScheduleEmailJob(request, scheduledAt))
                .Returns(jobId);

            var result = _controller.ScheduleEmail(request, scheduledAt);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobResponse>(okResult.Value);
            Assert.Equal(jobId, response.JobId);
            Assert.Equal("Scheduled", response.Status);
            Assert.Contains(scheduledAt.ToString(), response.Message);
        }

        [Fact]
        public void ScheduleReport_WithValidRequest_ShouldReturnOkResult()
        {
            var request = new ReportJobRequest
            {
                ReportType = "MonthlyReport",
                Parameters = new Dictionary<string, object>()
            };
            var scheduledAt = DateTime.UtcNow.AddDays(1);
            var jobId = "scheduled-report-id";

            _mockJobService
                .Setup(x => x.ScheduleReportJob(request, scheduledAt))
                .Returns(jobId);

            var result = _controller.ScheduleReport(request, scheduledAt);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<JobResponse>(okResult.Value);
            Assert.Equal(jobId, response.JobId);
            Assert.Equal("Scheduled", response.Status);
        }
    }
}
