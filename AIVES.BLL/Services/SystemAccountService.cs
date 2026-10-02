using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.BLL.Services;

public class SystemAccountServices : ISystemAccountServices
{
    private readonly INewsArticleRepository _articlesRepository;
    private readonly ISystemAccountRepository _systemAccountRepository;

    public SystemAccountServices(INewsArticleRepository articlesRepository, ISystemAccountRepository systemAccountRepository)
    {
        _articlesRepository = articlesRepository ?? throw new ArgumentNullException(nameof(articlesRepository));
        _systemAccountRepository = systemAccountRepository ?? throw new ArgumentNullException(nameof(systemAccountRepository));
    }

    public async Task<List<NewsArticle>> GetNewsArticlesFromUserAsync(int accountId, CancellationToken ct = default)
    {
        var exist = _systemAccountRepository.GetByIdAsync(accountId);
        if (exist == null)
            throw new ArgumentException("Cannot find this user", nameof(exist));
        return await _articlesRepository.GetByCreatorAsync(accountId, null, ct);
    }

    public async Task<List<NewsArticle>> GetNewsArticlesLecturerAsync(int accountId, CancellationToken ct = default)
    {
        var exist = _systemAccountRepository.GetByIdAsync(accountId);
        if (exist == null)
            throw new ArgumentException("Cannot find this user", nameof(accountId));
        if (exist.Result.AccountRole != 2)
        {
            throw new ArgumentException("Only Lecturers are allowed to view this", nameof(exist));
        }
        return await _articlesRepository.GetByCreatorAsync(accountId, null, ct);
    }

    public async Task<List<NewsArticle>> GetNewsArticlesPublicAsync(CancellationToken ct = default)
    {
        return await _articlesRepository.GetActiveAsync(null, ct);
    }

    public async Task<bool> UpdateProfileAsync(int id, string accountName, string accountEmail, string accountPassword, CancellationToken ct = default)
    {
        if (id <= 0) return false;

        // 1. Lấy User trong DB
        var exist = await _systemAccountRepository.GetByIdAsync(id, false, ct);
        if (exist == null) return false;

        // 2. Xác định các input
        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("Account name cannot be empty.", nameof(accountName));
        if (string.IsNullOrWhiteSpace(accountEmail))
            throw new ArgumentException("Email cannot be empty.", nameof(accountEmail));
        if (string.IsNullOrWhiteSpace(accountPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(accountPassword));

        // 3. Chuyển qua cho Repo thực thi
        var command = new ProfileUpdate(
            Id: id,
            Name: accountName,
            Email: accountEmail
            );
        return await _systemAccountRepository.UpdateProfileAsync(command, ct);
    }
}