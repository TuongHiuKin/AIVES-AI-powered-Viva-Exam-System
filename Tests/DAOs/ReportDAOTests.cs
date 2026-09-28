using AIVES.DAL.DAOs;
using AIVES.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIVES.Tests.DAOs
{
    public class ReportDAOTests
    {
        [Fact]
        public async Task GetReportDataAsync_ValidDates_ReturnsData()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AIVESDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var context = new AIVESDbContext(options);
            context.NewsArticles.Add(new NewsArticle { NewsArticleId = 1, CreatedDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            // Act
            var result = await ReportDAO.Instance.GetReportDataAsync(context, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}