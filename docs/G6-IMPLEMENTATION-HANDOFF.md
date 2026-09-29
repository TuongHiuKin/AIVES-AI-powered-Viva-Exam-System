# Bàn giao định hướng nhóm 6 — phạm vi, contract và cấu hình

Ngày 29/09/2026. Người dùng yêu cầu **định hình đúng hướng trước khi chỉnh sửa dự án để demo main flow**, chưa yêu cầu implement code ngay. Tài liệu này là phương án kỹ thuật để review và lập diff, không tự cấp quyền triển khai, chạy DB, commit/push hoặc merge.

## 1. Trạng thái quyết định

| Nội dung | Trạng thái |
| --- | --- |
| Nghiệp vụ DEC-01..DEC-12 | Đã duyệt; nguồn duy nhất: business-rules.md |
| MVC/BLL/DAL, EF Core, SQL Server | Giữ nền kỹ thuật hiện hành; không tự chuyển stack |
| Package A/B/C và tên contract dưới đây | Phương án triển khai đề xuất, chưa tạo code |
| Giá trị fixture DEMO-01, normalization/rounding/bins | Cấu hình test đề xuất, chưa là quy tắc production do GV duyệt |
| Thành viên/nhánh đang làm báo cáo thi | Chưa xác định; không suy từ phân công News |
| Role mapper thật và schema exam/result | Chưa xác nhận, không thay role/code/schema News bằng suy đoán |
| Chấp nhận mock cho buổi demo | Cần chủ dự án/giảng viên xác nhận mức cho phép |
| AGY tự implement | Chưa được giao ở bước chuẩn bị này |

Đọc [TASKS.md](TASKS.md), [business-rules.md](business-rules.md), [ARCHITECTURE.md](ARCHITECTURE.md), [checklist](CODE-REVIEW-CHECKLIST.md). Nếu contract tương thích đã có trên nhánh teammate, reuse/map nó, không tạo bản cạnh tranh vì tên trong tài liệu.

## 2. Chia phạm vi để không đụng code đồng đội

### Package A — logic đọc báo cáo và test, độc lập SQL/HTTP

Mục tiêu: phần G6-00, G6-06/07/08/09 tính toán, G6-10; có báo cáo cá nhân và thống kê thực thi từ input fixture, chưa tuyên bố web demo được.

File mới dự kiến (chỉ thêm khi user giao implement, inventory xác nhận chưa trùng):
- `AIVES.BLL/Models/ExamReporting/`: DTO và output thuần .NET.
- `AIVES.BLL/Interfaces/ExamReporting/`: Service/actor contracts.
- `AIVES.BLL/Services/ExamReporting/`: business calculations.
- `AIVES.DAL/Repositories/Models/ExamReporting/`: read snapshots, không entity EF.
- `AIVES.DAL/Repositories/Interfaces/ExamReporting/`: repository đọc, nếu cần.
- `AIVES.Tests/ExamReporting/`: fixtures, fake repository, tests.

Không sửa Program, Auth, Account/News/ReportController cũ, shared layout, appsettings, csproj, interfaces cũ, DB/Entities/DbContext hoặc DAO News. Không đăng ký fake vào ứng dụng production. Nếu cần sửa file ngoài phạm vi để build, báo file/lý do và xin phạm vi bổ sung, không âm thầm mở rộng.

### Package B — chạy luồng HTTP/UI

Mục tiêu: login thật → báo cáo cá nhân → giảng viên thống kê/biểu đồ. Đây mới là phần quan trọng cho demo web.

Chỉ bắt đầu sau khi xác định role/identity mapper và owner file chung:
- Đề xuất controller/view mới tách `StudentReports` và `ClassReports`; dùng tên thực khác nếu nhóm đã có. Không ghi đè ReportController Admin của News.
- Program/DI/routes/Auth/shared navigation được sửa có chủ đích trong phạm vi riêng đã duyệt.
- Student identity lấy từ principal; teacher class access từ nguồn phân công đáng tin. Không map Staff=Student, không route/query để đổi user giả.
- Nếu dùng nguồn fixture cho UI, cần opt-in cấu hình demo rõ, giới hạn môi trường test/demo và không tự thay production SQL registration.
- Không làm “demo login giả” thành tính năng production. Harness test identity không chứng minh login thật.

Điểm dừng: chưa biết mapping role/phân công lớp → không mở endpoint bằng AllowAnonymous để né, không tuyên bố sẵn sàng demo HTTP; vẫn có thể review Package A.

### Package C — nguồn kết quả và dữ liệu thật

- Adapter repository → DAO → DbContext/SQL hoặc nguồn backend teammate được duyệt.
- Mọi đề xuất schema phải nêu mapping, keys, constraints, ảnh hưởng module cũ và cách nâng cấp; không chạy SQL chỉ vì docs được duyệt.
- Quyền thay database, migration/seed, AI API/network/chi phí hoặc Auth phải nằm trong yêu cầu riêng.
- Package C chưa xong không đồng nghĩa Package A sai; nhưng mock input không chứng minh persistence/AI thật.

## 3. Contract đọc tối thiểu (thiết kế đề xuất, không phải schema đã có)

Tên dưới đây là tên gợi ý. IDs trong contract đề xuất là chuỗi opaque không rỗng để adapter có thể map int/GUID của nguồn; đây **không đổi kiểu khóa DB hiện tại**. Nếu interface teammate đã chốt dùng int/GUID, giữ nó và thống nhất mapping, không tạo ID giả.

Các giá trị score/percent/scale dùng decimal, không dùng float cho quyết định sát ngưỡng. Collection rỗng hợp lệ; thiếu dữ liệu khác với số 0.

| Snapshot | Thông tin cần có | Ý nghĩa/bảo vệ |
| --- | --- | --- |
| ReportActor | SubjectId, role ngữ nghĩa Student/Teacher | BLL nhận từ trusted adapter, không từ form. Tên role trong principal thật còn phải map |
| ClassExamScope | ClassId, ExamId | Đầu vào chọn phạm vi, không tự cấp quyền lớp |
| Assignment | TeacherId/ClassId/ExamId hoặc CourseId và mapping | Nguồn quyền đọc; query phải kiểm tra trước khi trả dữ liệu cá nhân |
| ExamSettingsSnapshot | SettingsVersion, EvaluationMode, MaxCompletedAttempts, câu hỏi/rubric, điều kiện đạt toàn lượt | Gắn phiên bản với mỗi attempt; không dùng settings mới để diễn giải lại điểm cũ |
| QuestionRule | QuestionId, QuestionVersion, rubric/criteria, MaxScore (score), PassThreshold (score), required criteria (pass/fail) | AI áp dụng, BLL/report không tự sinh chuẩn. Threshold xác định đơn vị score hay percent rõ ràng |
| AttemptSnapshot | AttemptId, StudentId, ClassId, ExamId, AttemptOrdinal, ExamState, SettingsVersion, CompletedAt, expected question set | Một lượt giữ riêng; completedCount khác gradedCount. Ordinal từ nguồn, không tự suy bằng số lượt đã hoàn thành |
| QuestionResult | AttemptId + QuestionId/version, AnswerText hoặc reference, GradingState, EarnedScore hoặc evaluation evidence, Feedback, criteria outcomes | Mỗi câu trong lượt một kết quả hiện hành; chưa chấm null, không thành 0/sai |
| ReportingDataset | Scope, settings versions, students/roster cần thiết, attempts/results | Có metadata nguồn Live/Fixture và các bản ghi excluded/pending để giải thích mẫu số |

Hai trạng thái độc lập, tên đề xuất:
- ExamState: InProgress / Completed / Abandoned / Cancelled.
- GradingState: Pending / Graded / Failed.
- Một lượt Completed + Pending vẫn tính vào số lượt đã thi, chưa vào average.
- Một lượt chỉ đủ điểm tổng khi mọi câu trong tập câu phải chấm có kết quả hợp lệ. Câu bị bỏ trống được chấm 0/không đạt nếu rubric/quy trình nguồn kết luận như vậy; không tự coi missing record là 0.
- Chế độ Score: 0 <= EarnedScore <= MaxScore, MaxScore > 0. Threshold có đơn vị rõ, so bằng giá trị chưa làm tròn.
- Chế độ PassFail: có outcomes tiêu chí đủ để giải thích kết luận; điều kiện bắt buộc phải thỏa ngoài tỷ lệ tối thiểu.
- Từ chối snapshot mâu thuẫn (hai điểm khác nhau cho cùng attempt/question, unknown settings, score out of range), không bỏ qua rồi báo đủ dữ liệu. JOIN lặp cùng dữ liệu được dedupe theo key, không đếm đôi.

### Repository và Service boundary đề xuất

Chỉ cần read contracts cho gói báo cáo, không thêm CRUD rỗng:

| Contract gợi ý | Input | Output / trách nhiệm |
| --- | --- | --- |
| IExamReportRepository.ReadStudentAttemptsAsync | StudentId, ExamId, CancellationToken | Snapshot kết quả và settings của chính SV |
| IExamReportRepository.IsTeacherAssignedAsync | TeacherId, ClassId, ExamId, CancellationToken | Xác nhận quyền lớp từ nguồn tin cậy |
| IExamReportRepository.ReadClassResultsAsync | ClassId, ExamId, CancellationToken | Dữ liệu lớp sau khi Service xác nhận quyền |
| IStudentReportService.GetAsync | Trusted actor, ExamId, CancellationToken | Lấy studentId từ actor; danh sách lượt và tổng hợp |
| IClassReportService.GetAsync | Trusted actor, scope, QuestionStatsSelection, ReportDisplayOptions, CancellationToken | Kiểm tra Teacher + assignment rồi lấy dữ liệu/tính kết quả |

Service không nhận studentId từ client để xác định chủ báo cáo. Các filters/options client gửi phải validate và đối chiếu cấu hình được cho phép. Direct service test với actor giả là test riêng; boundary production tạo actor vẫn phải được kiểm chứng.

Lỗi có ngữ nghĩa: Forbidden, NotFound, InvalidInput, InvalidDataset, SourceUnavailable. Tên kiểu/exception tùy conventions thực có. Không thay lỗi nguồn bằng empty success; không trả raw SQL/stack/secret. MVC adapter map response phù hợp, BLL không phụ thuộc HttpContext.

### Output cho hai màn hình

- StudentReport: completed/max, từng lượt + từng câu/feedback/rubric, raw total/max, normalized total khi cần, gradedAttemptCount, pending count, average nullable, pass/fail từng lượt và tỷ lệ có nhãn.
- ClassReport: scope, student averages/eligible counts, question rates/ranking với numerator/denominator, distribution buckets, excluded/pending counts, mode/thang/settings metadata.
- UI không tính lại công thức riêng; chart nhận dữ liệu aggregate của cùng Service.
- Null average không vào bucket 0. Student có ít nhất một lượt đủ điểm có thể có average tạm thời, ghi rõ số lượt được dùng.

## 4. Quy ước tính toán để triển khai

Những công thức là DEC đã duyệt; chi tiết kỹ thuật thêm dưới đây là đề xuất có thể thay khi thống nhất với nhóm.

### Điểm cá nhân và biểu đồ

1. Lấy các lượt Completed trong đúng exam/student.
2. Ghi số hoàn thành để hiển thị quota, độc lập chấm.
3. Chỉ lượt đủ kết quả hợp lệ mới có tổng/average.
4. Tổng thô mỗi lượt = tổng điểm câu. Nếu chung thang, có thể trình bày raw average với thang đó.
5. Nếu khác thang, chuẩn hóa mỗi lượt theo `EarnedTotal / MaxTotal * DisplayScale`, rồi trung bình với trọng số bằng nhau giữa các lượt theo DEC-05/12.
6. Thang DisplayScale phải ghi rõ; không cộng 8/10 với 12/20 rồi lấy trung bình thô. Không trộn hai evaluation modes hoặc các exam không tương đương.
7. Histogram: boundaries tăng dần, các khoảng trái đóng/phải mở; khoảng cuối gồm điểm tối đa. Mỗi SV đủ dữ liệu đúng một bucket. Không round trước khi phân loại/bin; làm tròn chỉ nhãn.

### Tỷ lệ theo câu

- SelectedAttempt: chọn ordinal được yêu cầu; mỗi SV/câu một kết quả hợp lệ của lượt đó. Không có lượt đó thì không vào mẫu số.
- AllAttempts: mẫu số là các lượt trả lời đã chấm, không phải số SV duy nhất. Luôn ghi nhãn khác.
- Nhóm theo QuestionId + version/rubric tương thích; không gộp câu khác nhau chỉ vì cùng vị trí “câu 1”.
- Score mode: phân loại đạt theo threshold GV; PassFail mode theo outcomes tiêu chí GV.
- PassRate = passed / graded responses * 100; FailRate = failed / graded responses * 100. Chưa chấm/failed grading không phải failed answer.
- Dùng phân số/decimal chưa round để xếp hạng và xét đồng hạng; chỉ format tỷ lệ lúc hiển thị. Mẫu số 0 trả null/unavailable và không tham gia ranking.
- Không lấy trung bình phần trăm của từng lớp/câu để giả thành tỷ lệ tổng; tổng hợp numerator/denominator trên đúng tập dữ liệu.

### Khác biệt cần giữ rõ

Histogram trung bình gộp lượt của mỗi SV; thống kê câu theo lượt được chọn. Hai biểu đồ/bảng không bắt phải có cùng mẫu số nếu khác đơn vị; cần cùng scope lớp/exam và nhãn rõ. UI không được ngụ ý cả hai đều là “lượt 1” khi histogram đang là trung bình.

## 5. DEMO-01 — cấu hình fixture đề xuất, không phải production defaults

| Trường | Giá trị test đề xuất | Quy tắc |
| --- | --- | --- |
| MaxCompletedAttempts | 3 | Không chia average cho 3 nếu mới đủ 2 lượt |
| Score question max | 10 | Chỉ fixture, mỗi câu GV có thể cấu hình khác |
| Score pass threshold | 6 điểm trên 10 | Đặt rõ trong QuestionRule, không hard-code Service |
| PassFail minimum passed questions | 75% | Fixture 4 câu; required criteria phải đạt |
| DisplayScale | 10 khi cần chuẩn hóa | Trên UI phải ghi /10, không lẫn raw total /20 |
| DisplayDecimals | 2, MidpointRounding.AwayFromZero | Đề xuất nhãn demo; không ảnh hưởng threshold/ranking |
| Histogram boundaries | 0,2,4,6,8,10 | Nhãn [0,2),...,[8,10]; test mốc biên |
| QuestionStatsSelection | SelectedAttempt ordinal=1 | Có ca test AllAttempts riêng |
| Dataset | Hai lớp, hai GV, A/B/C và người ngoài scope | Không dùng dữ liệu/secret sinh viên thật |

Cần người phụ trách xác nhận các giá trị hiển thị trước demo. Agent có thể dùng DEMO-01 làm test fixture khi được giao Package A, nhưng không nói GV đã duyệt 6 điểm/75%/bins này cho mọi kỳ thi.

Kết quả tính tay:
- Lượt 1 tổng A/B/C = 12/6/12 trên 20; lượt 2 = 16/10/12 trên 20.
- Raw averages = 14/8/12 trên 20; nếu DEMO-01 chuẩn hóa /10 thì averages = 7/4/6.
- Histogram DEMO-01: [4,6) có B=1; [6,8) có A,C=2. Tổng 3 SV, không 6 lượt.
- Q1 lượt 1 = 8/4/6 trên 10: đạt 2/3; Q2 = 4/2/6: không đạt 2/3, khó hơn Q1.
- 3/4 câu đạt nhưng required criterion thất bại → lượt không đạt; pending không tự là không đạt.
- Thêm Completed+Pending: completedCount tăng, gradedCount/average chưa tăng; bỏ dở không tăng completedCount.

## 6. File chung và điều kiện cho phép sửa

| Thành phần | Trước khi AGY sửa cần gì? |
| --- | --- |
| Program/DI/routes/layout | Xác nhận owner, snapshot đang sửa, diff dự kiến và package B được giao |
| Auth/claim/role mapper | Mapping semantic role và scope được xác nhận, test quyền, không dùng mã Staff cũ làm Student |
| Entities/DbContext/database | Schema diff được duyệt riêng và môi trường thực thi được phép |
| Repo/Service interface teammate | Owner đồng ý contract, adapter/backward compatibility nếu cần |
| Existing ReportController | Không overwrite; reuse chỉ khi chức năng/owner thật đã thống nhất |
| Packages/csproj/CI | Nêu nhu cầu trước, tận dụng .NET/xUnit đã có; không thêm framework chỉ để demo |
| Git/issue/PR | Yêu cầu riêng; chuẩn bị docs không cấp quyền commit/push/merge |

Nếu chưa biết owner, chỉ đọc inventory và lập kế hoạch; không sửa file chung. Ghi dependency thực sự đang làm khi có bằng chứng, không tự nhận hộ tiến độ teammate.

## 7. Prompt theo giai đoạn

### Prompt dùng ngay: review + đề xuất diff, không implement

```text
Đọc AGENTS.md, skill funews-architecture và docs/business-rules.md,
ARCHITECTURE.md, TASKS.md, CODE-REVIEW-CHECKLIST.md, G6-IMPLEMENTATION-HANDOFF.md.
Review nhóm 6 theo DEC đã duyệt. Xác định code nào đang có thật, mock, hoặc
đồng đội chưa push. Đề xuất package nhỏ nhất để chạy hai luồng SV/GV.
Lập danh sách file sẽ thêm/sửa, contract nào khớp/chưa khớp, owner và dependency.
Đối chiếu DEMO-01: chỉ là fixture, không phải production rule.
Không sửa source/Auth/DB, không cài package, không publish. Báo riêng phần cần
chốt/permission; không viết lại toàn bộ hệ thống hoặc tiếp tục lấy News làm demo.
```

### Prompt triển khai Package A — chỉ gửi sau khi chủ dự án muốn bắt đầu code

```text
Tôi giao triển khai Package A trong G6-IMPLEMENTATION-HANDOFF.md, theo snapshot
[branch/commit] và phạm vi file đã được review [danh sách].
Áp dụng DEC ở business-rules.md; dùng DEMO-01 chỉ trong tests.
Giữ nguyên code teammate và không sửa Auth/Program/DB/schema/shared interfaces.
Nếu file dự định thêm đã có người làm, báo conflict trước khi sửa.
Implement logic báo cáo/thống kê thật với fake repository chỉ trong tests,
build/test an toàn và báo kết quả. Không tự commit/push hoặc mở package B/C.
```

Không gửi prompt triển khai khi danh sách file/snapshot còn chưa xác định. Muốn chạy demo UI phải giao riêng Package B và giải quyết auth/identity/data source; Package A pass không phải website đã chạy main flow.
