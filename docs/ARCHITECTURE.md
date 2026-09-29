# Kiến trúc AIVES — nền hiện có và hướng nhóm 6

## 1. Hiện trạng và mục tiêu khác nhau

Mục tiêu mới: nhóm 6 — báo cáo từng lượt, tổng hợp cá nhân, thống kê lớp/biểu đồ. Nguồn nghiệp vụ là [business-rules.md](business-rules.md); đầu việc ở [TASKS.md](TASKS.md); contract/phạm vi triển khai ở [G6-IMPLEMENTATION-HANDOFF.md](G6-IMPLEMENTATION-HANDOFF.md).

Source đang có nền News; cập nhật docs không có nghĩa báo cáo thi đã được xây. Khi review phải ghi nhánh/commit, không dùng tiến độ của nhánh local để suy ra code teammate trên nhánh khác. Chưa có xác nhận mapping/schema thi dùng được trong lượt đồng bộ tài liệu này.

## 2. Project thực hiện có

| Project/folder | Vai trò | Reference |
| --- | --- | --- |
| StudentNameMVC | Controllers, Views, ViewModels, startup/config/static assets | BLL, DAL tại startup |
| AIVES.BLL | Interfaces/Services, nghiệp vụ và tính toán | DAL |
| AIVES.DAL | Entities, Context, Repositories, Singleton DAOs, EF/LINQ | Không BLL/MVC |
| AIVES.Tests | Unit tests và SQL tests opt-in | Hiện checkout tham chiếu BLL/DAL; nhánh Auth có thể thêm MVC |

Solution `AIVESSystem.sln`, các project .NET 8. Giữ tên folder/project thực hiện có; không tự rename để “giống domain”. Các entity hiện tại SystemAccount/NewsArticle/Category/Tag/NewsTag và SQL scripts 001/002 thuộc News, không phải dữ liệu thi.

## 3. Ranh giới bắt buộc

Luồng production: View → Controller → Service → Repository → DAO → DbContext → SQL Server.

- MVC: HTTP input, ModelState, authorization, map ViewModel, View/Redirect. Controller không gọi DAL trực tiếp.
- BLL: phối hợp use case, kiểm tra quyền phạm vi theo trusted actor, áp dụng settings, tính tổng/trung bình/tỷ lệ. Không dùng DbContext hoặc MVC View/HTTP types.
- DAL: contract đọc/ghi, entity mapping, query scope/LINQ/persistence qua DAO; không định nghĩa công thức UI khác BLL.
- Entity phản ánh schema thực; DTO đọc là snapshot cho use case, không bắt buộc là entity EF; ViewModel chỉ phục vụ UI.
- Controller/Service không trả nguyên Account navigation/hash ra UI.
- Không vòng reference. MVC DAL reference chỉ cho composition root, không là lý do bỏ BLL.

## 4. Lifetime và persistence

Giữ Repository + DAO, DAO Lazy Singleton không chứa DbContext trong field. Repository scoped nhận context scoped rồi truyền vào DAO theo operation. Không singleton context, không query song song trên cùng instance context.

DAL hiện có `AddAivesDataAccess` dùng SQL Server từ configuration. Không tự gọi Migrate/EnsureCreated. DB-first, scripts mới cần review/quyền riêng; không chạy lại 001/002 để “tạo dữ liệu nhóm 6”. Credentials/AI keys không nằm trong source/log.

Các snapshot settings/result cho nhóm 6 là contract dự kiến, chưa chứng minh có bảng. Bên Data xác nhận mapping trước adapter SQL; không tự nhét exam score vào NewsContent.

## 5. Cách nối nhóm 6 theo giai đoạn

| Giai đoạn | Phần thật cần có | Boundary chưa hoàn thiện |
| --- | --- | --- |
| A — BLL/test riêng | Tính báo cáo cá nhân, tổng/trung bình, tỷ lệ và bins | Fake repository và trusted actor trong test |
| B — HTTP/UI | Controller → Service, authorization thật, chart render | Data adapter có thể là fixture demo được chấp nhận riêng |
| C — Tích hợp | Auth, lớp được phân công, result provider/DAO/DB thật | Không còn fake tại production registrations |

Tên/method đề xuất nằm ở handoff, không phải code đã triển khai. Giai đoạn A không chứng minh login/DB/UI hoặc AI thật; giai đoạn B không được hard-code user để né phần Auth thiếu.

Identity mapper: principal → trusted actor; không tin studentId/role từ client. Numeric role News 1/2 không tự map thành Student/Teacher. Quyền lớp đọc từ contract nguồn đáng tin, không từ query string. Chưa có mapper/role contract thì block gói HTTP, vẫn test BLL được.

## 6. Sửa file và xác minh

- Xác minh owner/current branch/diff trước khi implement; gói A chỉ thêm file dưới namespace/folder nhóm 6 theo handoff. Program, Auth, shared layout, project files, interfaces hiện có, schema/DbContext là shared files.
- Giữ endpoint News hiện có. Không ghi đè ReportController Admin để biến thành báo cáo thi nếu chưa cho phép.
- Build `dotnet build AIVESSystem.sln --configuration Release`; test phù hợp với thay đổi sau khi đọc setup/SQL opt-in. SDK local .local/dotnet có thể dùng nếu đã tồn tại.
- Mock test không kiểm chứng SQL, UI, antiforgery/middleware. Tách pass/fail/skip và ghi rõ snapshot.
- Không restore/cài thêm dependency mới, seed, migration, SQL-write test hoặc publish khi chưa được phép. Nếu build thiếu package/SDK thì báo blocker môi trường, không sửa nghiệp vụ để né.

## 7. Tài liệu lịch sử

[Bản kiến trúc News trước đây](archive/NEWS-ARCHITECTURE.md) giữ hướng dẫn và kết quả bàn giao cũ để không mất thông tin. Chỉ đọc khi bảo trì News hoặc đối chiếu test cũ; không áp tiến độ/quyền/module đó như tiêu chí nhóm 6.
