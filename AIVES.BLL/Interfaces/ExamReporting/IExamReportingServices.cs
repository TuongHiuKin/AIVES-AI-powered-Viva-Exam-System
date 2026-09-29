using AIVES.BLL.Models.ExamReporting;

namespace AIVES.BLL.Interfaces.ExamReporting;

public interface IExamReportRepository
{
    Task<List<AttemptSnapshotDto>> GetStudentAttemptsAsync(string studentId, string examId, CancellationToken ct = default);
    Task<List<AttemptSnapshotDto>> GetClassAttemptsAsync(string classId, string examId, CancellationToken ct = default);
    Task<bool> IsTeacherAssignedToClassAsync(string teacherId, string classId, CancellationToken ct = default);
    Task<ExamSettingsSnapshot?> GetExamSettingsAsync(string examId, CancellationToken ct = default);
    Task<List<string>> GetAvailableClassesForTeacherAsync(string teacherId, CancellationToken ct = default);
    Task<List<ExamSettingsSnapshot>> GetAvailableExamsAsync(CancellationToken ct = default);
}

public interface IStudentReportService
{
    Task<StudentReportDto?> GetStudentReportAsync(string studentId, string examId, CancellationToken ct = default);
}

public interface IClassReportService
{
    Task<ClassStatisticsDto?> GetClassStatisticsAsync(
        string teacherId,
        string classId,
        string examId,
        int? specificAttemptOrdinal = null,
        CancellationToken ct = default);
}
