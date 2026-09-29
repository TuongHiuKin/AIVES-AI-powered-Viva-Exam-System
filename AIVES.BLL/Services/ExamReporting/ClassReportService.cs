using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Models.ExamReporting;

namespace AIVES.BLL.Services.ExamReporting;

public class ClassReportService : IClassReportService
{
    private readonly IExamReportRepository _repository;

    public ClassReportService(IExamReportRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ClassStatisticsDto?> GetClassStatisticsAsync(
        string teacherId,
        string classId,
        string examId,
        int? specificAttemptOrdinal = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new ArgumentException("Teacher ID cannot be empty.", nameof(teacherId));
        if (string.IsNullOrWhiteSpace(classId))
            throw new ArgumentException("Class ID cannot be empty.", nameof(classId));
        if (string.IsNullOrWhiteSpace(examId))
            throw new ArgumentException("Exam ID cannot be empty.", nameof(examId));

        // DEC-11: Giảng viên chỉ được xem lớp được phân công
        var isAssigned = await _repository.IsTeacherAssignedToClassAsync(teacherId, classId, ct);
        if (!isAssigned)
        {
            throw new UnauthorizedAccessException($"Giảng viên không có quyền truy cập dữ liệu của lớp {classId}.");
        }

        var settings = await _repository.GetExamSettingsAsync(examId, ct);
        if (settings == null)
            return null;

        var allAttempts = await _repository.GetClassAttemptsAsync(classId, examId, ct);

        // Lọc các lượt thi Completed và Graded
        var completedAttempts = allAttempts
            .Where(a => a.ExamState == ExamState.Completed && a.GradingState == GradingState.Graded)
            .ToList();

        // 1. Tính điểm trung bình của từng sinh viên (DEC-05, DEC-06)
        var studentAttemptsGroup = completedAttempts
            .GroupBy(a => a.StudentId)
            .ToList();

        var studentAverages = new List<decimal>();
        foreach (var group in studentAttemptsGroup)
        {
            var attemptsList = group.ToList();
            if (attemptsList.Count > 0)
            {
                var avg = Math.Round(attemptsList.Sum(a => a.NormalizedScore) / attemptsList.Count, 1);
                studentAverages.Add(avg);
            }
        }

        // DEC-06: Biểu đồ phân bố điểm X = Điểm số trung bình, Y = Số lượng sinh viên (mỗi SV tính đúng 1 lần)
        var scoreDistribution = studentAverages
            .GroupBy(score => score)
            .OrderBy(g => g.Key)
            .Select(g => new ClassScoreBinDto
            {
                Score = g.Key,
                StudentCount = g.Count()
            })
            .ToList();

        decimal classAverageScore = studentAverages.Count > 0
            ? Math.Round(studentAverages.Average(), 2)
            : 0m;

        // 2. Thống kê câu hỏi (DEC-07, DEC-08, DEC-09, DEC-10)
        List<AttemptSnapshotDto> targetAttemptsForQuestions;
        string dataScopeLabel;

        if (specificAttemptOrdinal.HasValue)
        {
            targetAttemptsForQuestions = completedAttempts
                .Where(a => a.AttemptOrdinal == specificAttemptOrdinal.Value)
                .ToList();
            dataScopeLabel = $"Lượt thi số {specificAttemptOrdinal.Value}";
        }
        else
        {
            targetAttemptsForQuestions = completedAttempts;
            dataScopeLabel = "Gộp tất cả các lượt thi";
        }

        // Gom tất cả các câu trả lời hợp lệ đã được chấm
        var questionResults = targetAttemptsForQuestions
            .SelectMany(a => a.QuestionResults)
            .Where(q => q.GradingState == GradingState.Graded)
            .GroupBy(q => q.QuestionId)
            .ToList();

        var questionStatsList = new List<QuestionStatDto>();

        foreach (var group in questionResults)
        {
            var totalAssessed = group.Count();
            int passedCount;

            if (settings.EvaluationMode == ExamEvaluationMode.PassFail)
            {
                passedCount = group.Count(q => q.IsPassed);
            }
            else
            {
                // DEC-03: Chế độ điểm dùng PassThresholdScore cấu hình bởi GV
                passedCount = group.Count(q => q.EarnedScore >= settings.PassThresholdScore);
            }

            var failedCount = totalAssessed - passedCount;
            var passRate = totalAssessed > 0 ? Math.Round(((decimal)passedCount / totalAssessed) * 100m, 2) : 0m;
            var failRate = totalAssessed > 0 ? Math.Round(((decimal)failedCount / totalAssessed) * 100m, 2) : 0m;

            var first = group.First();
            questionStatsList.Add(new QuestionStatDto
            {
                QuestionId = group.Key,
                QuestionTitle = first.QuestionTitle,
                TotalAssessed = totalAssessed,
                PassedCount = passedCount,
                FailedCount = failedCount,
                PassRate = passRate,
                FailRate = failRate,
                IsHardest = false
            });
        }

        // DEC-09, DEC-10: Xác định câu khó nhất (tỷ lệ không đạt cao nhất, hiển thị tất cả các câu đồng hạng)
        // Loại trừ các câu không có dữ liệu đánh giá (totalAssessed == 0)
        var assessedQuestions = questionStatsList.Where(q => q.TotalAssessed > 0).ToList();
        var maxFailRate = assessedQuestions.Count > 0 ? assessedQuestions.Max(q => q.FailRate) : 0m;

        var hardestQuestions = new List<QuestionStatDto>();
        if (assessedQuestions.Count > 0 && maxFailRate > 0m)
        {
            foreach (var q in assessedQuestions.Where(q => q.FailRate == maxFailRate))
            {
                q.IsHardest = true;
                hardestQuestions.Add(q);
            }
        }

        // Xếp hạng: Tỷ lệ đạt giảm dần (DEC-09)
        var rankedStats = questionStatsList
            .OrderByDescending(q => q.PassRate)
            .ThenBy(q => q.QuestionId)
            .ToList();

        for (int i = 0; i < rankedStats.Count; i++)
        {
            rankedStats[i].Rank = i + 1;
        }

        return new ClassStatisticsDto
        {
            ClassId = classId,
            ClassName = $"Lớp {classId}",
            ExamId = examId,
            ExamTitle = settings.ExamTitle,
            TotalStudentsCount = studentAttemptsGroup.Count,
            AverageClassScore = classAverageScore,
            FilteredAttemptOrdinal = specificAttemptOrdinal,
            DataScopeLabel = dataScopeLabel,
            QuestionStats = rankedStats,
            HardestQuestions = hardestQuestions,
            ScoreDistribution = scoreDistribution
        };
    }
}
