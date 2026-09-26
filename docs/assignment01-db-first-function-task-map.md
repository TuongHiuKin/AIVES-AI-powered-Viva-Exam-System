# Bản đồ function → issue theo DB-first (nhóm 5 người)

**Trạng thái:** 24 issue **nháp**, chưa tạo/gán trên GitHub, chưa triển khai code hoặc DB. Chủ dự án là Data owner. TV2–TV5 là chỗ giữ cho bốn thành viên chưa có GitHub username. Chi tiết hành vi của 31 function nằm ở [đặc tả chức năng](assignment01-function-plan.md); rule ở [business-rules.md](business-rules.md).

## 1. Vì sao DB đi trước

Hiện chưa có schema SQL Server chính thức trong workspace. Trước khi sửa Entity hoặc DbContext, chủ dự án cần chốt bảng, cột, kiểu, khóa, quan hệ News–Tag và môi trường DB được phép dùng. Tên file hiện có không phải bằng chứng schema. TV2 có thể xử lý framework/CI/MVC nền song song với việc này; các module phụ thuộc DB chỉ bắt đầu sau cổng D04.

Thứ tự Data: D00 schema → D01 Entity/DbContext/EF → D02 DAO Singleton → D03 Repository contract/implementation → D04 DI DB và truy vấn chỉ đọc. D04 là mốc bàn giao cho các thành viên làm Service/MVC. Data owner quản lý BusinessObjects, DataAccess, Repositories và cấu hình DB; thay đổi contract sau bàn giao phải qua issue và review của chủ dự án.

## 2. Số lượng và phân công dự kiến

Có 31 function ID trong đặc tả. Chúng được giao trách nhiệm chính trong 20 issue chức năng/kiến trúc, cộng bốn issue hỗ trợ P00, D00, D03, Q01, thành **24 issue nháp**. Issue hỗ trợ có thay đổi code (D03, Q01) cũng ghi function ID liên quan theo AGENTS.md; ID lặp ở đây là để kiểm tra/tích hợp, không tạo thêm function mới.

| Người | Phạm vi | Số issue riêng |
| --- | --- | ---: |
| Bạn | Schema, Entity/DbContext, DAO, Repository, DB DI/kết nối | 5 |
| TV2 | Framework/CI/MVC nền, UI chung, Login, phân quyền | 4 |
| TV3 | News main flow | 4 |
| TV4 | Category, Profile/History, Public/Lecturer | 5 |
| TV5 | Account Management, Report | 4 |
| Cả nhóm | Duyệt cấu trúc P00 và tích hợp Q01 | 2 |

Số issue không thể hiện chính xác thời gian: D01/D02, N02/N03 và Q01 thường phức tạp hơn. Chủ dự án thay TV2–TV5 bằng username và điều chỉnh tải sau khi nhóm xem chi tiết function.

## 3. Danh sách issue và phụ thuộc

| Issue | Owner | Function ID | Cần xong trước | Bàn giao/tiêu chí chính |
| --- | --- | --- | --- | --- |
| P00 — Duyệt scope, file map, owner | Cả nhóm; bạn chốt | — | Không | Duyệt business rules, 31 function, tên file, phần chờ schema; không code |
| P01 — Framework, CI, MVC nền | TV2 | ARCH-01, AUTH-04 | P00 | .NET hợp lệ và CI khớp; startup MVC, route Account/Login, build nền |
| D00 — Chốt schema và DB được phép dùng | Bạn | — | P00 | Bảng/cột/PK/FK/NewsTag/status có nguồn xác thực; môi trường kiểm tra rõ |
| D01 — Entity, DbContext, EF SQL Server | Bạn | ARCH-02 | D00, P01 | Mapping đúng schema, DbContext scoped, connection string từ appsettings |
| D02 — Bốn DAO Singleton | Bạn | ARCH-03 | D01 | Instance thread-safe, EF/LINQ thật, không giữ scoped DbContext |
| D03 — Repository contract và implementation | Bạn; TV2–TV5 review | ARCH-04 (hỗ trợ) | D02 | Interface đủ cho News/Category/Account/Tag, History, Active News, Report và rule xóa Category |
| D04 — DI Data và kết nối chỉ đọc | Bạn; TV2 review Program.cs | ARCH-04 | D03, P01 | Lifetime đúng, truy vấn đọc được trên DB cho phép; contract Data chốt |
| U01 — Layout, navigation, modal chung | TV2 | ARCH-05 | D04 | UI chung cho bốn vai trò, modal shell/validation; link có action thật |
| A01 — Login và Logout | TV2 | AUTH-01, AUTH-02 | D04, U01 | Admin từ config, Staff/Lecturer từ DB; sai credential và logout đúng |
| A02 — Backend authorization | TV2 | AUTH-03 | A01 | Role/endpoint bảo vệ ở server, thử URL trực tiếp |
| C01 — Category list/search | TV4 | CAT-01, CAT-02 | D04, A02 | Query qua đủ lớp, keyword trống an toàn |
| C02 — Category create/update | TV4 | CAT-03, CAT-04 | C01, U01 | Hai popup, validation server, dữ liệu lưu đúng |
| C03 — Category delete | TV4 | CAT-05 | C01, D03, U01 | Confirm/Cancel; Service chặn Category đang được News dùng |
| N01 — News list/search | TV3 | NEWS-01, NEWS-02 | D04, A02 | List và LINQ search qua đủ lớp |
| N02 — News create/update/validation | TV3 | NEWS-03, NEWS-04, NEWS-06 | N01, C01, U01 | Hai popup, Category/Status/Creator, ModelState, lưu đúng |
| N03 — News–Tag | TV3; bạn review phần Data | NEWS-07 | D00, D03, N02 | Gắn/sửa/gỡ Tag theo schema, không trùng/hỏng liên kết |
| N04 — News delete | TV3 | NEWS-05 | N01, U01 | Cancel không xóa, Confirm mới xóa, ID sai an toàn |
| M01 — Account list/search | TV5 | ACC-01, ACC-02 | D04, A02 | Chỉ Admin, không hiển thị password |
| M02 — Account create/update | TV5 | ACC-03, ACC-04 | M01, U01 | Hai popup, role 1/2, validation server |
| M03 — Account delete | TV5 | ACC-05 | M01, U01 | Confirm/Cancel; sai role/ID bị xử lý an toàn |
| S01 — Staff Profile/History | TV4; TV3 cung cấp News query | PROFILE-01, HISTORY-01 | A02, N01 | Chỉ tài khoản và News của chính Staff |
| V01 — Public/Lecturer Active News | TV4; TV3 cung cấp News query | PUBLIC-01, LECT-01 | D04, A02, N01 | Public không login; chỉ NewsStatus = 1 |
| R01 — Admin Report | TV5; TV3 cung cấp News query | REPORT-01 | A02, N01 | Date range hợp lệ, CreatedDate filter, giảm dần, Admin-only |
| Q01 — Tích hợp, test, demo | Cả nhóm; bạn kiểm Data | ARCH-01 (kiểm tra) | Các issue trên | Build/test/CI pass và demo News end-to-end; kiểm tra quyền/rule/UI |

P01 và D00 có thể chạy song song sau P00. U01/A01/A02 và các issue chức năng chỉ bắt đầu khi D04 đã bàn giao. Việc đăng ký Service DI hoàn chỉnh được bổ sung khi Service implementation có thật; Q01 kiểm toàn bộ DI. Không tạo Service placeholder để tuyên bố D04 đạt.

## 4. Checklist bàn giao của Data owner

- D00: ghi nguồn schema/version, tên DB, bảng/cột, kiểu dữ liệu, khóa, nullability và quan hệ; đánh dấu rõ phần chưa biết. Không tự tạo NewsTag mapping từ giả định.
- D01: Entity/DbContext/DbSet/relationship đúng schema; SQL Server provider lấy ConnectionStrings:DefaultConnection; DbContext scoped, không Singleton.
- D02: bốn DAO có Instance thread-safe; method nhận DbContext theo thao tác/request hoặc cơ chế an toàn tương đương; không cất scoped context trong Singleton.
- D03: TV2–TV5 xem trước repository contract. Phải có năng lực query News theo Category/creator/status/CreatedDate và thao tác Tag theo schema để họ không phải tự sửa Data sau này.
- D04: kiểm tra đăng ký DI và truy vấn **chỉ đọc** trên DB được phép; lưu kết quả kiểm tra. Không dùng migration hoặc ghi/xóa dữ liệu như phép thử kết nối.

Nếu schema chưa có, D01–D04 và N03 ghi **BLOCKED BY SCHEMA**. Khi Data contract đã chốt, thay đổi cần issue riêng và review của bạn. Có GitHub username rồi mới thêm CODEOWNERS/branch protection nếu nhóm muốn enforce quyền review trên GitHub.
