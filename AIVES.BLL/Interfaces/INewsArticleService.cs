using AIVES.DAL.Entities;

namespace AIVES.BLL.Interfaces;

public interface INewsArticleService
{
    // --- 1. Dành cho luồng quản lý của Staff (TV3 - N01) ---
    Task<List<NewsArticle>> SearchNewsAsync(string? keyword = null, CancellationToken ct = default);
    Task<NewsArticle?> GetNewsByIdAsync(int id, CancellationToken ct = default);

    // --- 2. Thao tác ghi dữ liệu của Staff (TV3 - N02, N03, N04) ---
    // Nhận dữ liệu sạch đã qua xác thực từ controller, CreatedById lấy từ Staff đăng nhập
    Task<int> CreateNewsAsync(string title, string content, int categoryId, byte status, int createdById, IReadOnlyCollection<int> tagIds, CancellationToken ct = default);
    
    // UpdatedById lấy từ Staff đăng nhập, giữ nguyên CreatedById ban đầu
    Task<bool> UpdateNewsAsync(int id, string title, string content, int categoryId, byte status, int updatedById, IReadOnlyCollection<int> tagIds, CancellationToken ct = default);
    
    // Xóa bài viết và xóa các liên kết NewsTag của bài, giữ lại Tag và Category
    Task<bool> DeleteNewsAsync(int id, CancellationToken ct = default);

    // --- 3. Dữ liệu nạp vào form Popup Modal (TV3 - N02, N03) ---
    Task<List<Category>> GetCategoriesForDropdownAsync(CancellationToken ct = default);
    Task<List<Tag>> GetAllTagsAsync(CancellationToken ct = default);

    // --- 4. Các hàm cung cấp cho TV4 & TV5 (Hợp đồng dùng chung) ---
    // TV4 (PUBLIC-01, LECT-01): Xem tin Active (Status = 1)
    Task<List<NewsArticle>> GetActiveNewsAsync(string? keyword = null, CancellationToken ct = default);
    
    // TV4 (HISTORY-01): Staff xem lại lịch sử các bài do chính mình tạo
    Task<List<NewsArticle>> GetNewsByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default);
    
    // TV5 (REPORT-01): Báo cáo Admin theo khoảng thời gian
    Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default);
}
