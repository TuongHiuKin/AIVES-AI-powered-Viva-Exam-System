---
name: Task Assignment
about: Giao việc Assignment 1 theo kiến trúc MVC và ba lớp
title: "[TASK] <Module>: <Kết quả cần đạt>"
labels: ["task"]
assignees: ""
---

> Đọc `AGENTS.md` và `docs/assignment01-team-rule-and-issue-plan.md` trước khi làm. Một issue chỉ có một người chịu trách nhiệm chính; ghi rõ người review.

## Mục tiêu và phạm vi

- **Giai đoạn:** <!-- STRUCTURE-REVIEW hoặc IMPLEMENTATION -->
- **Kết quả cần bàn giao:** <!-- Mô tả một kết quả có thể kiểm tra -->
- **Module / Function IDs:** <!-- Ví dụ ARCH-01, NEWS-01..NEWS-07; dùng ID trong function plan -->
- **Ưu tiên:** <!-- P0/P1/P2/P3 -->
- **Người thực hiện / người review:** <!-- GitHub username hoặc tên thành viên -->
- **Issue phụ thuộc:** <!-- Link issue; nếu cần schema thật, ghi BLOCKED BY SCHEMA -->
- **Ngoài phạm vi:** <!-- Những chức năng/file thuộc issue khác -->

## File được phép thay đổi và hợp đồng giữa các lớp

| File / folder | Chủ sở hữu | Thay đổi dự kiến |
| --- | --- | --- |
| <!-- path --> | <!-- member --> | <!-- tên/contract hoặc code --> |

- **Đường đi cần chứng minh khi implement:** View → Controller → Service → Repository → DAO/DbContext → SQL Server.
- **Tên interface/method hoặc ViewModel dùng chung:** <!-- Chốt trước khi các issue khác phụ thuộc -->
- **Shared file cần phối hợp:** <!-- Program.cs, csproj, DbContext/entity, _Layout, site.js... -->

## Ràng buộc bắt buộc khi implement

- [ ] Controller chỉ phụ thuộc Service interface; không dùng DbContext/DbSet/DAO/Repository trực tiếp.
- [ ] Service chứa business rule và gọi Repository; Repository gọi DAO; DAO truy vấn scoped DbContext bằng EF Core/LINQ.
- [ ] DAO có Singleton thread-safe qua `Instance` nếu thuộc bốn DAO bắt buộc; DbContext không Singleton.
- [ ] Không đoán schema News–Tag hoặc kiểu cột; đối chiếu schema thật trước khi map.
- [ ] Connection string và DefaultAdmin lấy từ `appsettings.json`; role Staff = 1, Lecturer = 2.
- [ ] Backend kiểm tra role/ownership; form thay đổi dữ liệu có server validation và antiforgery.
- [ ] Nếu liên quan News/Category/Account: Create/Update dùng modal, Delete có Cancel/Confirm; CategoryService chặn xóa Category đang được News dùng.

## Tiêu chí nghiệm thu — chọn đúng giai đoạn

### STRUCTURE-REVIEW: chưa viết chức năng

- [ ] Sơ đồ project/reference và danh sách file hiện có/file đề xuất rõ ràng; không gọi file rỗng là chức năng đã xong.
- [ ] Phạm vi từng issue, người phụ trách, thứ tự phụ thuộc và file dùng chung đã được người review duyệt.
- [ ] Các giả định về schema/Tag và tên method chưa chốt được ghi là câu hỏi mở.
- [ ] Không sửa mã nguồn, không thêm package, không chạy migration/database write trong issue này.

### IMPLEMENTATION: chỉ áp dụng sau khi cấu trúc được duyệt

- [ ] Có đường đi thực tế qua các lớp, không bỏ Service/Repository.
- [ ] Có kiểm thử có ý nghĩa cho business rule và đường đi quan trọng; `dotnet build` và `dotnet test` pass.
- [ ] Đã kiểm tra thủ công chức năng UI/role liên quan; ghi kết quả và trường hợp lỗi.
- [ ] PR chỉ chứa file trong phạm vi issue; CI pass trước khi merge vào `main`.

## Bằng chứng bàn giao / ghi chú cho người review

<!-- Link PR, kết quả build/test, ảnh hoặc bước manual test. Nếu dùng AI agent, ghi agent đã làm gì và những phần cần kiểm tra lại. -->
