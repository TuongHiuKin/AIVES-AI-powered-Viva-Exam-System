using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Models.ExamReporting;

namespace AIVES.BLL.Services.ExamReporting;

public class StudentReportService : IStudentReportService
{
    private readonly IExamReportRepository _repository;

    public StudentReportService(IExamReportRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<StudentReportDto?> GetStudentReportAsync(string studentId, string examId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(examId))
            throw new ArgumentException("Exam ID cannot be empty.", nameof(examId));

        var settings = await _repository.GetExamSettingsAsync(examId, ct);
        if (settings == null)
            return null;

        var attempts = await _repository.GetStudentAttemptsAsync(studentId, examId, ct);
        var studentName = attempts.FirstOrDefault()?.StudentName ?? "Sinh viên";

        // DEC-01: Chỉ tính các lượt thi ở trạng thái Completed vào số lượt hoàn thành
        var completedAttempts = attempts
            .Where(a => a.ExamState == ExamState.Completed)
            .OrderBy(a => a.AttemptOrdinal)
            .ToList();

        var pendingAttempts = attempts
            .Where(a => a.ExamState == ExamState.InProgress || a.GradingState == GradingState.Pending)
            .ToList();

        // Đánh giá từng lượt thi theo cấu hình của Giảng viên (DEC-02, DEC-04, DEC-12)
        foreach (var attempt in attempts)
        {
            attempt.EvaluationMode = settings.EvaluationMode;
            foreach (var q in attempt.QuestionResults)
            {
                q.EvaluationMode = settings.EvaluationMode;
            }

            if (settings.EvaluationMode == ExamEvaluationMode.PassFail)
            {
                var totalQuestions = attempt.QuestionResults.Count;
                var passedQuestions = attempt.QuestionResults.Count(q => q.IsPassed);
                var passRate = totalQuestions > 0 ? ((decimal)passedQuestions / totalQuestions) * 100m : 0m;

                // Kiểm tra tiêu chí bắt buộc (DEC-04)
                var hasRequiredCriteria = settings.RequiredCriteriaKeys.All(key =>
                    attempt.QuestionResults.Any(q => q.CriteriaOutcomes.TryGetValue(key, out var met) && met));

                attempt.IsPassed = passRate >= settings.MinPassPercentage && hasRequiredCriteria;
            }
            else
            {
                // DEC-03, DEC-05: Điểm số
                attempt.IsPassed = attempt.NormalizedScore >= settings.PassThresholdScore;
            }
        }

        // DEC-05, DEC-10: Các lượt đã hoàn thành và đã có kết quả chấm đầy đủ
        var fullyGradedAttempts = completedAttempts
            .Where(a => a.GradingState == GradingState.Graded)
            .ToList();

        decimal? averageScore = null;
        if (settings.EvaluationMode == ExamEvaluationMode.Score && fullyGradedAttempts.Count > 0)
        {
            // Điểm trung bình = Tổng điểm chuẩn hóa các lượt có kết quả đầy đủ / Số lượt đó (DEC-05, DEC-12)
            averageScore = Math.Round(fullyGradedAttempts.Sum(a => a.NormalizedScore) / fullyGradedAttempts.Count, 2);
        }

        decimal? passRatePercentage = null;
        if (completedAttempts.Count > 0)
        {
            passRatePercentage = Math.Round(((decimal)completedAttempts.Count(a => a.IsPassed) / completedAttempts.Count) * 100m, 2);
        }

        return new StudentReportDto
        {
            StudentId = studentId,
            StudentName = studentName,
            ExamId = examId,
            ExamTitle = settings.ExamTitle,
            EvaluationMode = settings.EvaluationMode,
            CompletedAttemptsCount = completedAttempts.Count,
            MaxAllowedAttemptsCount = settings.MaxAllowedAttempts,
            GradedAttemptsCount = fullyGradedAttempts.Count,
            PendingAttemptsCount = pendingAttempts.Count,
            AverageScore = averageScore,
            PassRatePercentage = passRatePercentage,
            Attempts = attempts.OrderBy(a => a.AttemptOrdinal).ToList()
        };
    }

    public Task<List<ExamSettingsSnapshot>> GetAvailableExamsAsync(CancellationToken ct = default)
    {
        return _repository.GetAvailableExamsAsync(ct);
    }
}
