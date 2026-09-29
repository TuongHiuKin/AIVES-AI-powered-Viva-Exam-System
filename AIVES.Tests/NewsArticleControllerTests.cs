using System.Security.Claims;
using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StudentNameMVC.Controllers;
using StudentNameMVC.ViewModels;
using Xunit;

namespace AIVES.Tests;

public class NewsArticleControllerTests
{
    private class DummyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private class DummyUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; set; } = new();
        public string? Action(UrlActionContext actionContext) => $"/{actionContext.Action}";
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => true;
        public string? Link(string? routeName, object? values) => "/";
        public string? RouteUrl(UrlRouteContext routeContext) => "/";
    }

    private class FakeNewsArticleServiceForController : INewsArticleService
    {
        public int LastCreatedById { get; private set; }
        public int LastUpdatedById { get; private set; }
        public bool CreateCalled { get; private set; }
        public bool UpdateCalled { get; private set; }

        public Task<int> CreateNewsAsync(string title, string content, int categoryId, byte status, int createdById, IReadOnlyCollection<int> tagIds, CancellationToken ct = default)
        {
            CreateCalled = true;
            LastCreatedById = createdById;
            return Task.FromResult(100);
        }

        public Task<bool> UpdateNewsAsync(int id, string title, string content, int categoryId, byte status, int updatedById, IReadOnlyCollection<int> tagIds, CancellationToken ct = default)
        {
            UpdateCalled = true;
            LastUpdatedById = updatedById;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteNewsAsync(int id, CancellationToken ct = default) => Task.FromResult(true);
        public Task<NewsArticle?> GetNewsByIdAsync(int id, CancellationToken ct = default) => Task.FromResult<NewsArticle?>(new NewsArticle { NewsArticleId = id, CategoryId = 1 });
        public Task<List<NewsArticle>> SearchNewsAsync(string? keyword = null, CancellationToken ct = default) => Task.FromResult(new List<NewsArticle>());
        public Task<List<Category>> GetCategoriesForDropdownAsync(CancellationToken ct = default) => Task.FromResult(new List<Category>());
        public Task<List<Tag>> GetAllTagsAsync(CancellationToken ct = default) => Task.FromResult(new List<Tag>());
        public Task<List<NewsArticle>> GetActiveNewsAsync(string? keyword = null, CancellationToken ct = default) => Task.FromResult(new List<NewsArticle>());
        public Task<List<NewsArticle>> GetNewsByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default) => Task.FromResult(new List<NewsArticle>());
        public Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default) => Task.FromResult(new List<NewsArticle>());
    }

    private static NewsArticleController CreateControllerWithUser(FakeNewsArticleServiceForController service, ClaimsPrincipal? user)
    {
        var controller = new NewsArticleController(service);
        var httpContext = new DefaultHttpContext();
        if (user != null)
        {
            httpContext.User = user;
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new DummyTempDataProvider());
        controller.Url = new DummyUrlHelper();

        return controller;
    }

    [Fact]
    public async Task Create_WithUnauthenticatedUser_ReturnsChallengeAndDoesNotCallService()
    {
        // Arrange
        var service = new FakeNewsArticleServiceForController();
        var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity()); // IsAuthenticated = false
        var controller = CreateControllerWithUser(service, unauthenticatedUser);

        var model = new NewsArticleFormViewModel
        {
            NewsTitle = "Test Title",
            NewsContent = "Test Content",
            CategoryId = 1,
            NewsStatus = 1
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        Assert.IsType<ChallengeResult>(result);
        Assert.False(service.CreateCalled);
    }

    [Fact]
    public async Task Create_WithInvalidClaimFormat_ReturnsChallengeAndDoesNotCallService()
    {
        // Arrange
        var service = new FakeNewsArticleServiceForController();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid_id_not_int")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var controller = CreateControllerWithUser(service, user);

        var model = new NewsArticleFormViewModel
        {
            NewsTitle = "Test Title",
            NewsContent = "Test Content",
            CategoryId = 1,
            NewsStatus = 1
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        Assert.IsType<ChallengeResult>(result);
        Assert.False(service.CreateCalled);
    }

    [Fact]
    public async Task Create_WithValidAuthenticatedStaff_CallsServiceWithCorrectCreatorId()
    {
        // Arrange
        var service = new FakeNewsArticleServiceForController();
        const int expectedStaffId = 42;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedStaffId.ToString()),
            new Claim(ClaimTypes.Role, "Staff")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var controller = CreateControllerWithUser(service, user);

        var model = new NewsArticleFormViewModel
        {
            NewsTitle = "Test Title",
            NewsContent = "Test Content",
            CategoryId = 1,
            NewsStatus = 1
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        Assert.IsType<JsonResult>(result);
        Assert.True(service.CreateCalled);
        Assert.Equal(expectedStaffId, service.LastCreatedById);
    }

    [Fact]
    public async Task Edit_WithUnauthenticatedUser_ReturnsChallengeAndDoesNotCallService()
    {
        // Arrange
        var service = new FakeNewsArticleServiceForController();
        var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        var controller = CreateControllerWithUser(service, unauthenticatedUser);

        var model = new NewsArticleFormViewModel
        {
            NewsArticleId = 5,
            NewsTitle = "Edit Title",
            NewsContent = "Edit Content",
            CategoryId = 1,
            NewsStatus = 1
        };

        // Act
        var result = await controller.Edit(5, model);

        // Assert
        Assert.IsType<ChallengeResult>(result);
        Assert.False(service.UpdateCalled);
    }

    [Fact]
    public async Task Edit_WithValidAuthenticatedStaff_CallsServiceWithCorrectEditorId()
    {
        // Arrange
        var service = new FakeNewsArticleServiceForController();
        const int expectedStaffId = 77;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedStaffId.ToString()),
            new Claim(ClaimTypes.Role, "Staff")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var controller = CreateControllerWithUser(service, user);

        var model = new NewsArticleFormViewModel
        {
            NewsArticleId = 10,
            NewsTitle = "Edit Title",
            NewsContent = "Edit Content",
            CategoryId = 1,
            NewsStatus = 1
        };

        // Act
        var result = await controller.Edit(10, model);

        // Assert
        Assert.IsType<JsonResult>(result);
        Assert.True(service.UpdateCalled);
        Assert.Equal(expectedStaffId, service.LastUpdatedById);
    }
}
