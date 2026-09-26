# Tài liệu PRN222 Assignment 1 — FU News Management System

Thư mục này chỉ mô tả **yêu cầu và kế hoạch** cho FUNewsManagementSystem. Tên file, class hoặc function trong tài liệu không có nghĩa chức năng đã được lập trình. Quy tắc bắt buộc cho người và AI agent nằm ở [AGENTS.md](../AGENTS.md).

## Đọc theo thứ tự

1. [Business rules](business-rules.md): quy tắc nghiệp vụ, quyền của từng vai trò và các điểm chưa thể kết luận khi chưa có schema DB.
2. [Đặc tả chức năng](assignment01-function-plan.md): 31 function ID, đầu vào, xử lý, đầu ra và tiêu chí kiểm tra của từng function; News Article Management là luồng demo chính.
3. [Kiến trúc và quy tắc nhóm](assignment01-team-rule-and-issue-plan.md): MVC + ba lớp, ranh giới file, năm người và quy trình issue/PR.
4. [Bảng function → task DB-first](assignment01-db-first-function-task-map.md): 24 issue nháp, phụ thuộc và mốc bàn giao Data.

## Phạm vi đã chốt

- ASP.NET Core MVC, EF Core, LINQ, Microsoft SQL Server; framework phải thuộc phạm vi Assignment cho phép (.NET 5/6/7/8), CI dùng SDK tương ứng.
- Luồng bắt buộc: View → Controller → Service → Repository → DAO/DbContext → SQL Server. Controller chỉ phụ thuộc Service interface.
- Main flow: Staff quản lý News Article gồm list, search, create, update, delete, Category, Tags, status và validation.
- Admin quản lý Account và xem Report; Staff quản lý News, Category, Profile và News History; Lecturer và Public chỉ xem News Active.
- Create/Update của News, Category và Account dùng popup/modal; Delete có Cancel và Confirm.

## Điều chưa được tài liệu này tự quyết

Workspace chưa có schema SQL Server chính thức để xác nhận bảng, cột, kiểu khóa, quan hệ News–Tag và ràng buộc xóa. Data owner phải đối chiếu nguồn schema thật trước khi Entity/DbContext/DAO được triển khai. Tài liệu không đưa ra dữ liệu mẫu hoặc migration giả định.

Hiện là **giai đoạn duyệt cấu trúc và phân việc**. Chưa đánh dấu function nào là Done chỉ nhờ có file hoặc tên class. Issue implementation chỉ bắt đầu sau khi chủ dự án duyệt file map và Data contract; xem [issue template](../.github/ISSUE_TEMPLATE/task-assignment.md).
