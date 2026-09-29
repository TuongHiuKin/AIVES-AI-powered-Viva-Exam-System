using AIVES.DAL.Entities;

namespace AIVES.BLL.Interfaces;

public interface ISystemAccountServices
{
    // TV4 - Cập nhật tài khoản. Chỉ có thể thay đổi tên, email và mật khẩu của tài khoản đó
    Task<bool> UpdateProfileAsync(int id, string accountName, string accountEmail, string accountPassword, CancellationToken ct = default);

    // TV4 - Xem các News Articles của người dùng đang đăng nhập
    Task<List<NewsArticle>> GetNewsArticlesFromUserAsync(int accountId, CancellationToken ct = default);

    // TV4 - Người dùng xem các News Articles
    Task<List<NewsArticle>> GetNewsArticlesPublicAsync(CancellationToken ct = default);

    // TV4 - Lecturer xem các News Articles. AccountRole = 2
    Task<List<NewsArticle>> GetNewsArticlesLecturerAsync(int accountId, CancellationToken ct = default);
}