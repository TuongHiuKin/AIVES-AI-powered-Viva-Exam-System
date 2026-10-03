using System.Text;
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
        var gradeExportRows = new List<ClassGradeExportRowDto>();

        // Nhóm tất cả sinh viên có lượt thi trong lớp (kể cả chưa graded)
        var allStudentsInClass = allAttempts
            .GroupBy(a => a.StudentId)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in allStudentsInClass)
        {
            var studentId = group.Key;
            var studentName = group.First().StudentName;
            var studentGradedAttempts = group
                .Where(a => a.ExamState == ExamState.Completed && a.GradingState == GradingState.Graded)
                .OrderBy(a => a.AttemptOrdinal)
                .ToList();

            decimal? avgScore = null;
            if (studentGradedAttempts.Count > 0)
            {
                avgScore = Math.Round(studentGradedAttempts.Sum(a => a.NormalizedScore) / studentGradedAttempts.Count, 1);
                studentAverages.Add(avgScore.Value);
            }

            var att1 = group.FirstOrDefault(a => a.AttemptOrdinal == 1);
            var att2 = group.FirstOrDefault(a => a.AttemptOrdinal == 2);

            string att1Str = att1 != null
                ? (att1.GradingState == GradingState.Graded ? $"{att1.NormalizedScore:0.0}" : "Đang chấm")
                : "Chưa thi";

            string att2Str = att2 != null
                ? (att2.ExamState == ExamState.Abandoned ? "Bỏ dở" : (att2.GradingState == GradingState.Graded ? $"{att2.NormalizedScore:0.0}" : "Đang chấm"))
                : "Chưa thi";

            bool isPassed = avgScore.HasValue && avgScore.Value >= settings.PassThresholdScore;

            gradeExportRows.Add(new ClassGradeExportRowDto
            {
                StudentId = studentId,
                StudentName = studentName,
                ClassId = classId,
                Attempt1Score = att1Str,
                Attempt2Score = att2Str,
                AverageScore = avgScore.HasValue ? $"{avgScore.Value:0.0}" : "Chưa có",
                ResultStatus = isPassed ? "ĐẠT" : "CHƯA ĐẠT",
                Note = studentGradedAttempts.Count < group.Count() ? "Có lượt thi chờ chấm / bỏ dở" : "Đã hoàn thành"
            });
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
                CognitiveLevel = first.CognitiveLevel,
                TotalAssessed = totalAssessed,
                PassedCount = passedCount,
                FailedCount = failedCount,
                PassRate = passRate,
                FailRate = failRate,
                IsHardest = false
            });
        }

        // DEC-09, DEC-10: Xác định câu khó nhất (tỷ lệ không đạt cao nhất, hiển thị tất cả các câu đồng hạng)
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
            TotalStudentsCount = allStudentsInClass.Count,
            AverageClassScore = classAverageScore,
            FilteredAttemptOrdinal = specificAttemptOrdinal,
            DataScopeLabel = dataScopeLabel,
            QuestionStats = rankedStats,
            HardestQuestions = hardestQuestions,
            ScoreDistribution = scoreDistribution,
            GradeExportRows = gradeExportRows
        };
    }

    public async Task<byte[]> ExportClassGradeSheetCsvAsync(
        string teacherId,
        string classId,
        string examId,
        CancellationToken ct = default)
    {
        var stats = await GetClassStatisticsAsync(teacherId, classId, examId, null, ct);
        if (stats == null)
        {
            throw new InvalidOperationException("Không tìm thấy dữ liệu lớp để xuất bảng điểm.");
        }

        var sb = new StringBuilder();
        // Header
        sb.AppendLine("STT,Mã sinh viên,Họ và tên,Lớp,Lượt thi 1,Lượt thi 2,Điểm trung bình (Thang 10),Kết quả,Ghi chú");

        int stt = 1;
        foreach (var row in stats.GradeExportRows)
        {
            sb.AppendLine($"{stt++},{EscapeCsv(row.StudentId)},{EscapeCsv(row.StudentName)},{EscapeCsv(row.ClassId)},{EscapeCsv(row.Attempt1Score)},{EscapeCsv(row.Attempt2Score)},{EscapeCsv(row.AverageScore)},{EscapeCsv(row.ResultStatus)},{EscapeCsv(row.Note)}");
        }

        // Ghi kèm BOM UTF-8 để Microsoft Excel trên Windows tự nhận font tiếng Việt không bị lỗi font
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, result, preamble.Length, bytes.Length);

        return result;
    }

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";
        if (text.Contains(',') || text.Contains('"') || text.Contains('\n'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return $"\"{text}\"";
    }

    public async Task<StudentReportDto?> GetStudentExamDetailForTeacherAsync(
        string teacherId,
        string classId,
        string examId,
        string studentId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new ArgumentException("Teacher ID cannot be empty.", nameof(teacherId));
        if (string.IsNullOrWhiteSpace(classId))
            throw new ArgumentException("Class ID cannot be empty.", nameof(classId));
        if (string.IsNullOrWhiteSpace(examId))
            throw new ArgumentException("Exam ID cannot be empty.", nameof(examId));
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));

        // DEC-11: Giảng viên chỉ được xem dữ liệu của lớp được phân công
        var isAssigned = await _repository.IsTeacherAssignedToClassAsync(teacherId, classId, ct);
        if (!isAssigned)
        {
            throw new UnauthorizedAccessException($"Giảng viên {teacherId} không có quyền truy cập lớp {classId}.");
        }

        var settings = await _repository.GetExamSettingsAsync(examId, ct);
        if (settings == null)
            return null;

        // Lấy tất cả lượt thi của sinh viên cho kỳ thi này
        var studentAttempts = await _repository.GetStudentAttemptsAsync(studentId, examId, ct);

        // Lọc các lượt thi thuộc đúng lớp học phần được phân công
        var classAttempts = studentAttempts
            .Where(a => string.Equals(a.ClassId, classId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!classAttempts.Any())
        {
            // Kiểm tra xem sinh viên có trong danh sách lớp này không
            var classAllAttempts = await _repository.GetClassAttemptsAsync(classId, examId, ct);
            var studentInClass = classAllAttempts.Any(a => string.Equals(a.StudentId, studentId, StringComparison.OrdinalIgnoreCase));
            if (!studentInClass)
            {
                throw new InvalidOperationException($"Sinh viên {studentId} không thuộc lớp {classId}.");
            }
        }

        // Lọc các lượt thi completed & graded để tính trung bình
        var completedAttempts = classAttempts.Where(a => a.ExamState == ExamState.Completed).ToList();
        var gradedAttempts = completedAttempts.Where(a => a.GradingState == GradingState.Graded).ToList();
        var pendingAttempts = completedAttempts.Where(a => a.GradingState == GradingState.Pending).ToList();

        decimal? averageScore = null;
        if (gradedAttempts.Any())
        {
            var sumNormalized = gradedAttempts.Sum(a => a.NormalizedScore);
            averageScore = Math.Round(sumNormalized / gradedAttempts.Count, 2);
        }

        decimal? passRate = null;
        if (gradedAttempts.Any())
        {
            var passedCount = gradedAttempts.Count(a => a.IsPassed);
            passRate = Math.Round(((decimal)passedCount / gradedAttempts.Count) * 100.0m, 1);
        }

        var studentName = classAttempts.FirstOrDefault()?.StudentName ?? studentId;

        return new StudentReportDto
        {
            StudentId = studentId,
            StudentName = studentName,
            ExamId = examId,
            ExamTitle = settings.ExamTitle,
            CompletedAttemptsCount = completedAttempts.Count,
            MaxAllowedAttemptsCount = settings.MaxAllowedAttempts,
            GradedAttemptsCount = gradedAttempts.Count,
            PendingAttemptsCount = pendingAttempts.Count,
            AverageScore = averageScore,
            PassRatePercentage = passRate,
            Attempts = classAttempts.OrderBy(a => a.AttemptOrdinal).ToList()
        };
    }

    public Task<List<ExamSettingsSnapshot>> GetAvailableExamsAsync(CancellationToken ct = default)
    {
        return _repository.GetAvailableExamsAsync(ct);
    }

    public Task<List<string>> GetAvailableClassesForTeacherAsync(string teacherId, CancellationToken ct = default)
    {
        return _repository.GetAvailableClassesForTeacherAsync(teacherId, ct);
    }
}
