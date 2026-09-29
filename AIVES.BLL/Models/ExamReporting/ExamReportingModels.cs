namespace AIVES.BLL.Models.ExamReporting;

public enum ExamEvaluationMode
{
    Score = 1,
    PassFail = 2
}

public enum ExamState
{
    InProgress = 1,
    Completed = 2,
    Abandoned = 3,
    Cancelled = 4
}

public enum GradingState
{
    Pending = 1,
    Graded = 2,
    Failed = 3
}

public class ExamSettingsSnapshot
{
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string SettingsVersion { get; set; } = "1.0";
    public ExamEvaluationMode EvaluationMode { get; set; } = ExamEvaluationMode.Score;
    public int MaxAllowedAttempts { get; set; } = 3;
    public decimal PassThresholdScore { get; set; } = 5.0m;
    public decimal MinPassPercentage { get; set; } = 60.0m;
    public List<string> RequiredCriteriaKeys { get; set; } = new();
}

public class QuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionTitle { get; set; } = string.Empty;
    public decimal EarnedScore { get; set; }
    public decimal MaxScore { get; set; } = 10.0m;
    public bool IsPassed { get; set; }
    public GradingState GradingState { get; set; } = GradingState.Graded;
    public string StudentAnswer { get; set; } = string.Empty;
    public string AiFeedback { get; set; } = string.Empty;
    public string RubricCriteria { get; set; } = string.Empty;
    public Dictionary<string, bool> CriteriaOutcomes { get; set; } = new();
}

public class AttemptSnapshotDto
{
    public string AttemptId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public int AttemptOrdinal { get; set; } = 1;
    public ExamState ExamState { get; set; } = ExamState.Completed;
    public GradingState GradingState { get; set; } = GradingState.Graded;
    public DateTime? CompletedAt { get; set; }
    public string SettingsVersion { get; set; } = "1.0";
    public List<QuestionResultDto> QuestionResults { get; set; } = new();

    public decimal TotalEarnedScore => QuestionResults.Sum(q => q.EarnedScore);
    public decimal TotalMaxScore => QuestionResults.Sum(q => q.MaxScore);

    /// <summary>
    /// Đưa điểm về thang điểm chuẩn 10 (DEC-12)
    /// </summary>
    public decimal NormalizedScore => TotalMaxScore > 0 ? Math.Round((TotalEarnedScore / TotalMaxScore) * 10.0m, 2) : 0m;

    public bool IsPassed { get; set; }
}

public class StudentReportDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public int CompletedAttemptsCount { get; set; }
    public int MaxAllowedAttemptsCount { get; set; }
    public int GradedAttemptsCount { get; set; }
    public int PendingAttemptsCount { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? PassRatePercentage { get; set; }
    public List<AttemptSnapshotDto> Attempts { get; set; } = new();
}

public class QuestionStatDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuestionTitle { get; set; } = string.Empty;
    public int TotalAssessed { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal PassRate { get; set; }
    public decimal FailRate { get; set; }
    public bool IsHardest { get; set; }
    public int Rank { get; set; }
}

public class ClassScoreBinDto
{
    public decimal Score { get; set; }
    public int StudentCount { get; set; }
}

public class ClassStatisticsDto
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public int TotalStudentsCount { get; set; }
    public decimal AverageClassScore { get; set; }
    public int? FilteredAttemptOrdinal { get; set; }
    public string DataScopeLabel { get; set; } = "Lượt thi số 1";
    public List<QuestionStatDto> QuestionStats { get; set; } = new();
    public List<QuestionStatDto> HardestQuestions { get; set; } = new();
    public List<ClassScoreBinDto> ScoreDistribution { get; set; } = new();
}
