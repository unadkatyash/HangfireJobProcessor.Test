using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Text;
using HangfireJobProcessor.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HangfireJobProcessor.Test.Integration
{
    public class HangfireIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HangfireIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task EnqueueEmail_ShouldReturnSuccessResponse()
        {
            var request = new EmailJobRequest
            {
                To = "test@example.com",
                Subject = "Integration Test Email",
                Body = "This is a test email from integration test"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/jobs/email", content);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var jobResponse = JsonSerializer.Deserialize<JobResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(jobResponse);
            Assert.NotEmpty(jobResponse.JobId);
            Assert.Equal("Enqueued", jobResponse.Status);
        }

        [Fact]
        public async Task EnqueueReport_ShouldReturnSuccessResponse()
        {
            var request = new ReportJobRequest
            {
                ReportType = "IntegrationTestReport",
                Parameters = new Dictionary<string, object>
                {
                    { "TestParam", "TestValue" }
                },
                OutputFormat = "PDF"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/jobs/report", content);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var jobResponse = JsonSerializer.Deserialize<JobResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(jobResponse);
            Assert.NotEmpty(jobResponse.JobId);
            Assert.Equal("Enqueued", jobResponse.Status);
        }

        [Fact]
        public async Task HangfireDashboard_ShouldBeAccessible()
        {
            var response = await _client.GetAsync("/hangfire");

            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
