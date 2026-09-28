using AIVES.BLL.Services;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.MVC.ViewModels;
using Moq;

namespace AIVES.Tests.Services
{
    public class ReportServiceTests
    {
        [Fact]
        public async Task GenerateReportAsync_ValidDates_ReturnsReportData()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetReportDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ReportDataItem> { new ReportDataItem { NewsArticleId = 1 } });

            var service = new ReportService(mockRepo.Object);

            // Act
            var result = await service.GenerateReportAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}