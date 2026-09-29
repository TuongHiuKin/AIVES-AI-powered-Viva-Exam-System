using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.BLL.Interfaces;

public interface ICategoryService
{
    // TV4 - Tìm các Category bằng tên. Có thể trống
    Task<List<Category>> SearchCategoriesAsync(string? keyword = null, CancellationToken ct = default);
    Task<Category?> GetCategoriesAsync(int id, CancellationToken ct = default);

    // TV4 - Tạo Category mới. Các Category sử dụng Name và Description
    Task<int> CreateCategoriesAsync(string name, string? description = null, CancellationToken ct = default);

    // TV4 - Cập nhật Category. ID không thay đổi
    Task<bool> UpdateCategoriesAsync(int id, string name, string? description = null, CancellationToken ct = default);

    // TV4 - Xóa Category
    Task<DeleteResult> DeleteCategoriesAsync(int id, CancellationToken ct = default);
}