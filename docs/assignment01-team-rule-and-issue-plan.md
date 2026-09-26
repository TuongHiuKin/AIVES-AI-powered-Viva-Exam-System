# Kiến trúc MVC + ba lớp và quy tắc giao việc cho nhóm 5 người

**Giai đoạn hiện tại:** duyệt cấu trúc, business rules, function và issue. Chưa viết implementation chỉ để làm đầy file. Tài liệu này là file map được [AGENTS.md](../AGENTS.md) nhắc tới; [bảng task DB-first](assignment01-db-first-function-task-map.md) là kế hoạch issue hiện hành.

## 1. Ranh giới phụ thuộc

| Lớp | Project | Chứa | Được gọi |
| --- | --- | --- | --- |
| Presentation | StudentNameMVC | Controller, View, ViewModel, Program.cs | Chỉ Service interface ở Controller |
| Business | FUNewsManagement.Services | Service interface/implementation, business rule | Repository interface |
| Data Access | FUNewsManagement.Repositories | Repository interface/implementation | DAO |
| Data Access | FUNewsManagement.DataAccess | DAO, FUNewsManagementDbContext, EF Core/LINQ | SQL Server |
| Entity | FUNewsManagement.BusinessObjects | Entity theo schema thật | Được các lớp cần kiểu dữ liệu tham chiếu |

Luồng demo News phải truy được: View → NewsArticleController → INewsArticleService/NewsArticleService → INewsArticleRepository/NewsArticleRepository → NewsArticleDAO → FUNewsManagementDbContext → SQL Server. Cùng nguyên tắc áp dụng cho Search/Create/Update/Delete. Controller không inject DbContext/Repository/DAO. Program.cs là composition root để đăng ký DI, không phải nơi viết nghiệp vụ.

## 2. File map để duyệt

Các file sau đã có tên trong workspace nhưng nhiều file còn rỗng; “có file” không đồng nghĩa “có chức năng”.

| Module | File/chỗ đặt dự kiến | Owner đề xuất |
| --- | --- | --- |
| Schema và Entity | DB schema được chủ dự án xác nhận; BusinessObjects: SystemAccount, Category, NewsArticle, Tag, NewsTag | Bạn |
| EF và DAO | DataAccess: FUNewsManagementDbContext, NewsArticleDAO, CategoryDAO, SystemAccountDAO, TagDAO | Bạn |
| Repository | Repositories: bốn interface và bốn implementation cho NewsArticle, Category, SystemAccount, Tag | Bạn |
| Platform/Auth/UI chung | StudentNameMVC/Program.cs phần MVC/auth/route; AccountController/Login; AuthService; _Layout, navigation/modal JS | TV2; phối hợp bạn ở DI DB |
| News | NewsArticleController, INewsArticleService/NewsArticleService, NewsArticleViewModel, Views/NewsArticle và các modal partial | TV3 |
| Category, Staff, Public | CategoryService/Controller/Views; Profile/History/Home Controller, ViewModel và Views tương ứng | TV4 |
| Account và Report | SystemAccountService, AccountController action Admin, ReportController, ViewModels, Views | TV5 |
| Account popup còn thiếu | Views/Account/Index.cshtml và ba partial Create/Edit/Delete | TV5, chỉ tạo khi issue Account bắt đầu |

Tên method, thuộc tính Entity, kiểu khóa và Tag mapping phải chốt sau schema. TV3 cần cung cấp Service query Active News, News theo creator và Report cho TV4/TV5 qua contract được review; TV4/TV5 không tự sửa file Service của TV3.

## 3. Quy tắc issue và bảo vệ phần Data

1. Mỗi issue ghi function ID, người làm, người review, file được sửa, phụ thuộc, đầu ra và tiêu chí kiểm tra. Dùng [issue template](../.github/ISSUE_TEMPLATE/task-assignment.md). P00/Q01 là việc chung; các issue khác chỉ có một owner chính.
2. Bạn làm Data owner cho schema, BusinessObjects, DataAccess, Repositories và phần DB trong Program.cs. TV2–TV5 review năng lực repository họ cần trước khi D04 được chốt. Sau D04, muốn đổi Data contract phải có issue/PR thay đổi được bạn review; không sửa Data để vá nhanh lỗi UI.
3. Program.cs là file chung: TV2 phụ trách MVC/auth/route, bạn phụ trách DB registration. Hai người thống nhất thứ tự PR hoặc chia rõ đoạn thay đổi để tránh ghi đè; một người sửa file tại một thời điểm.
4. Các thành viên khác chỉ bắt đầu code chức năng phụ thuộc DB sau mốc D04. Trước đó họ có thể duyệt contract, chuẩn bị UI/Service design trên issue nhưng không đoán schema.
5. Controller chỉ phụ thuộc Service interface; Service gọi Repository; Repository gọi DAO; DAO dùng DbContext scoped. DAO Singleton thread-safe không giữ scoped context.
6. PR implementation cần mô tả call path, role/ownership, server validation, kết quả build/test và bước test thủ công. Không merge vào main khi CI fail. Cấu trúc được duyệt không tự động được coi là tính năng hoàn thành.

## 4. Cổng duyệt trước khi tạo issue thật

- Chủ dự án duyệt [business rules](business-rules.md), [31 function](assignment01-function-plan.md), file map ở trên và [24 task nháp](assignment01-db-first-function-task-map.md).
- Có GitHub username của năm người để gán owner/reviewer; có thể thêm CODEOWNERS/branch protection sau khi đã biết tài khoản.
- Có schema SQL Server chính thức, môi trường DB được phép kiểm tra chỉ đọc, và phiên bản .NET hợp lệ.
- Issue Data ghi rõ những phần còn CHỜ SCHEMA. Không chạy migration hoặc thay đổi DB chung như một cách “test kết nối”.

Cho tới khi các cổng này được duyệt, tài liệu là kế hoạch để nhóm nhận xét, không phải lệnh cho agent tự viết ứng dụng.
