# Tasks, phân công và đặc tả chức năng — FU News Management System

**Mục đích:** một nơi duy nhất chứa nội dung function, 24 task nháp, phân công nhóm 5 người, phụ thuộc, phần làm chung và tiêu chí bàn giao. Đây là kế hoạch, chưa phải GitHub issue đã tạo/gán và không phải báo cáo tính năng đã hoàn thành. Đọc [business rules và quyết định đã duyệt](business-rules.md), [kiến trúc](ARCHITECTURE.md) và [AGENTS.md](../AGENTS.md) trước khi implement.

Có **31 function ID**: Architecture 5, Authentication 4, News 7, Category 5, Account 5, Report 1, Profile 1, History 1, Public 1, Lecturer 1. News Article Management là luồng demo chính. Mọi function chạm DB đi qua Controller → Service → Repository → DAO → DbContext. Các mục A–F mô tả function; mục G–K là bảng task, phân công và cách bàn giao.

Chủ dự án đã xác nhận giảng viên không cung cấp script DB và duyệt SC-01..SC-11. D00 đã chốt schema; DAL và test dữ liệu đã triển khai. Không cần duyệt lại quan hệ News–Tag, soft delete hoặc kiểu ID đã chốt. Service/MVC/Auth/UI vẫn chưa hoàn thành.

D00/D01 đã có script 001/002, DB AIVES gồm IsDeleted, Entities/EF mapping khớp và đọc được 5 bảng. D02/D03 đã có 4 DAO Singleton, 4 Repository/interfaces và write contracts. D04 có AddAivesDataAccess, kiểm tra DI/lifetime, Release build 0 warning/error và 17/17 test pass trên .NET 8 (27/09/2026). Data contract sẵn sàng để các owner review và dùng; chưa seed AIVES, chưa xác nhận CI remote/peer review hoặc chức năng end-to-end. Chi tiết contract/cách chạy tại mục 8 ARCHITECTURE.md.

## A. Nền tảng và kiến trúc

### ARCH-01 — Cấu trúc MVC + ba lớp

- Đầu vào: yêu cầu Assignment và file map đã duyệt.
- Việc làm: dùng các thư mục StudentNameMVC, AIVES.BLL, AIVES.DAL và AIVES.Tests (tên file project tương ứng với thư mục, xem ARCHITECTURE.md); project reference một chiều MVC → BLL → DAL; target .NET 8 và CI tương ứng. File placement theo ARCHITECTURE.md.
- Đầu ra/kiểm tra: ứng dụng MVC có startup, Controller nhận request và trả View/Redirect; không có project reference vòng; solution build được. Việc có thư mục hoặc file rỗng chưa đạt function này.

### ARCH-02 — Entity, DbContext và SQL Server

- Đầu vào: schema SQL Server do Data owner thiết kế và duyệt tại D00 theo SC-01..SC-09, cùng connection string cấu hình.
- Việc làm: định nghĩa Entity, DbSet, khóa và mapping theo schema đã duyệt; cấu hình EF Core SQL Server; Category–News 1–n, News–Tag n–n qua NewsTag và các quy tắc xóa đã chốt.
- Đầu ra/kiểm tra: DbContext scoped truy vấn đọc được dữ liệu trong DB được phép; Entity và mapping khớp bảng/cột/khóa; không cần migration ghi DB để chứng minh kết nối.

### ARCH-03 — DAO Singleton

- Đầu vào: DbContext đã map và các nhu cầu query.
- Việc làm: NewsArticleDAO, CategoryDAO, SystemAccountDAO, TagDAO có Instance thread-safe; DAO chứa truy vấn EF Core/LINQ và thao tác dữ liệu.
- Đầu ra/kiểm tra: cùng một Instance DAO được dùng an toàn, không giữ DbContext scoped lâu dài; DbContext không Singleton; repository thực sự gọi DAO.

### ARCH-04 — Dependency Injection

- Đầu vào: interface/implementation đã tồn tại.
- Việc làm: đăng ký DbContext, Repository, Service và các thành phần auth theo lifetime thích hợp; Program.cs cấu hình MVC, SQL Server, authentication/authorization và route.
- Đầu ra/kiểm tra: ứng dụng khởi động và resolve dependency thành công; Controller chỉ nhận Service interface; không có lỗi lifetime Singleton → scoped. Phần DB có thể được kiểm tra tại D04, còn toàn bộ Service DI được xác nhận ở Q01.

### ARCH-05 — Layout và thành phần UI chung

- Đầu vào: ma trận vai trò và các màn hình cần modal.
- Việc làm: layout/navigation phù hợp Public, Lecturer, Staff, Admin; cơ chế mở modal, hiển thị lỗi và thông báo chung.
- Đầu ra/kiểm tra: link dẫn đến action có thật; menu đúng vai trò; có modal hoạt động trong các module; ẩn menu không thay thế backend authorization.

## B. Authentication và phân quyền

### AUTH-01 — Login bằng Email + Password

Áp dụng bổ sung SC-10/BR-26: Auth từ chối Account IsDeleted; xác thực các request tiếp theo phải vô hiệu hóa principal/cookie của Account đã soft delete hoặc hard delete. Kiểm tra có test cho người đã đăng nhập trước khi Admin xóa Account.

- Đầu vào: Email, Password từ form Login; DefaultAdmin từ appsettings; tài khoản Staff/Lecturer từ DB.
- Việc làm: validate input ở server; so khớp thông tin đúng nguồn; tạo trạng thái xác thực với role và account ID của người dùng DB; đăng nhập sai trả lỗi rõ ràng mà không tạo session/cookie hợp lệ.
- Đầu ra/kiểm tra: Admin, Staff, Lecturer hợp lệ đăng nhập được; mật khẩu sai và email không tồn tại bị từ chối; không hard-code credential C#.

### AUTH-02 — Logout

- Đầu vào: yêu cầu logout từ người đã đăng nhập.
- Việc làm: xóa trạng thái xác thực/session/cookie và điều hướng về trang phù hợp.
- Đầu ra/kiểm tra: sau logout không truy cập lại endpoint được bảo vệ bằng trạng thái cũ.

### AUTH-03 — Backend role authorization

- Đầu vào: principal/role đã xác thực và request đến Controller/action.
- Việc làm: bảo vệ Admin Account/Report, Staff News/Category/Profile/History; Lecturer chỉ xem Active News; Public chỉ được vào endpoint công khai.
- Đầu ra/kiểm tra: thử truy cập URL trực tiếp bằng sai role vẫn bị chặn; account ID cho Profile/History lấy từ principal, không lấy từ URL tùy ý.

### AUTH-04 — Default route

- Đầu vào: request đến đường dẫn gốc.
- Việc làm: cấu hình route mặc định đến Account/Login.
- Đầu ra/kiểm tra: mở URL gốc đến đúng trang Login; route/action tồn tại.

## C. News Article Management — main flow của Staff

Đường đi phải được kiểm chứng cho từng thao tác: View News → NewsArticleController → INewsArticleService/NewsArticleService → INewsArticleRepository/NewsArticleRepository → NewsArticleDAO → AIVESDbContext → SQL Server. News–Tag nhiều–nhiều qua NewsTag đã được chủ dự án duyệt; mapping cột/khóa cụ thể theo D00.

### NEWS-01 — List News

- Đầu vào: request Staff đến trang News.
- Việc làm: Service lấy danh sách qua Repository/DAO; nạp thông tin Category, Tags, Status và creator theo dữ liệu có thật; View hiển thị danh sách kể cả trường hợp rỗng.
- Đầu ra/kiểm tra: Staff thấy News tương ứng, link/button đi tới action có thật; role khác không vào trang quản lý.

### NEWS-02 — Search News

- Đầu vào: keyword và bộ lọc được UI cung cấp; giá trị null/rỗng được chấp nhận.
- Việc làm: input đi từ View qua Controller/Service đến LINQ ở tầng Data; lọc trên trường tìm kiếm được chốt theo schema, không lọc giả bằng JavaScript trên một trang dữ liệu thiếu.
- Đầu ra/kiểm tra: có keyword trả đúng tập con; keyword trống trả danh sách hợp lý; không lỗi null; không vượt quyền Staff.

### NEWS-03 — Create News bằng popup

- Đầu vào: modal với tiêu đề, nội dung, đúng một Category và Status bắt buộc; Tags có thể rỗng; account ID lấy từ Staff đăng nhập.
- Việc làm: mở modal thật, nạp Category/Tag từ dữ liệu thật, validate server; Service quyết định creator và ngày tạo, gọi Repository/DAO để lưu News và liên kết liên quan.
- Đầu ra/kiểm tra: dữ liệu hợp lệ được tạo và xuất hiện trong list; dữ liệu sai không tạo bản ghi và lỗi hiển thị trong modal; CSRF token hợp lệ.

### NEWS-04 — Update News bằng popup

- Đầu vào: News ID và form Edit; modal nạp dữ liệu News, Category, Tags, Status hiện tại.
- Việc làm: kiểm tra ID tồn tại, quyền và ModelState; Service cập nhật đúng bản ghi, giữ nguyên CreatedById/CreatedDate, ghi UpdatedById/ModifiedDate từ hệ thống theo SC-08; đồng bộ Tag theo NEWS-07.
- Đầu ra/kiểm tra: modal có dữ liệu cũ, Update lưu đúng ID, lỗi nhập liệu không làm mất dữ liệu, ID không tồn tại được xử lý an toàn.

### NEWS-05 — Delete News có xác nhận

- Đầu vào: News ID; lần đầu bấm Delete chỉ mở dialog.
- Việc làm: Cancel không gửi lệnh xóa; Confirm gửi request được bảo vệ; backend kiểm tra ID/quyền, xóa News và NewsTag của bài một cách nhất quán, giữ Category, Account và Tag theo SC-07.
- Đầu ra/kiểm tra: chỉ Confirm mới xóa; ID sai/đã xóa không làm ứng dụng crash; không còn liên kết hỏng.

### NEWS-06 — Validation News

- Đầu vào: dữ liệu Create/Update.
- Việc làm: ViewModel có Required/Length/DataType phù hợp schema; Controller kiểm tra ModelState; Service kiểm tra business rule như Category/Tag hợp lệ và trạng thái cho phép; không tin creator/client audit fields.
- Đầu ra/kiểm tra: request sai bị từ chối ở server, không ghi DB; lỗi rõ và modal vẫn dùng được sau khi sai.

### NEWS-07 — Gắn, sửa và gỡ Tag

Áp dụng BR-27: lưu nội dung, audit và các thay đổi NewsTag nguyên tử; test lỗi lưu phải giữ nguyên toàn bộ trạng thái trước đó.

- Đầu vào: danh sách Tag được chọn khi Create/Update.
- Việc làm: kiểm tra Tag tồn tại; sync quan hệ nhiều–nhiều qua NewsTag; không trùng cặp, gỡ Tag bỏ chọn, chấp nhận danh sách rỗng. Khóa vật lý theo D00; cardinality đã chốt tại SC-02.
- Đầu ra/kiểm tra: reload thấy Tag đúng; Update không nhân đôi; gỡ hết Tag hợp lệ; xóa News chỉ xóa các liên kết của bài; DAL từ chối xóa Tag còn được dùng, không cần thêm màn hình quản lý Tag riêng.

## D. Category Management — Staff

### CAT-01 — List Category

- Đầu vào: request Staff.
- Việc làm: lấy Category qua Service → Repository → DAO và hiển thị danh sách/empty state.
- Đầu ra/kiểm tra: Staff thấy Category thật; role không phù hợp không truy cập endpoint quản lý.

### CAT-02 — Search Category

- Đầu vào: keyword có thể rỗng.
- Việc làm: lọc bằng LINQ trên thuộc tính Category được schema xác nhận; keyword rỗng trả danh sách hợp lý.
- Đầu ra/kiểm tra: kết quả phù hợp keyword, không crash với null/rỗng.

### CAT-03 — Create Category bằng popup

- Đầu vào: form modal gồm trường Category hợp lệ theo schema.
- Việc làm: validate server, kiểm tra rule nghiệp vụ cần thiết, lưu qua Service/Repository/DAO.
- Đầu ra/kiểm tra: Category mới xuất hiện; input sai giữ modal và hiển thị lỗi, không ghi DB.

### CAT-04 — Update Category bằng popup

- Đầu vào: Category ID và form Edit được nạp dữ liệu cũ.
- Việc làm: kiểm tra ID, quyền, validation; cập nhật đúng Category qua đủ các lớp.
- Đầu ra/kiểm tra: giá trị mới persisted; ID sai xử lý an toàn; input sai không ghi DB.

### CAT-05 — Delete Category có quy tắc phụ thuộc

- Đầu vào: Category ID và Confirm từ dialog.
- Việc làm: CategoryService.DeleteCategory() kiểm tra bất kỳ News nào đang tham chiếu Category. Có tham chiếu thì từ chối và thông báo; không có thì cho phép Repository/DAO xóa.
- Đầu ra/kiểm tra: test cả Category đang dùng và chưa dùng; gọi backend trực tiếp cũng không vượt qua rule; Cancel không xóa.

## E. Account Management — Admin

### ACC-01 — List Account

- Đầu vào: request Admin.
- Việc làm: lấy danh sách Staff/Lecturer qua đủ các lớp và hiển thị thông tin cần thiết, không hiển thị mật khẩu.
- Đầu ra/kiểm tra: Admin thấy list/empty state; role khác truy cập URL bị chặn.

### ACC-02 — Search Account

- Đầu vào: keyword có thể rỗng.
- Việc làm: tìm theo thông tin Account được chốt (ví dụ tên/email nếu schema hỗ trợ) bằng LINQ; giữ giới hạn Admin.
- Đầu ra/kiểm tra: trả tập con đúng, keyword rỗng an toàn.

### ACC-03 — Create Account bằng popup

- Đầu vào: form modal với trường Account theo schema và role 1 (Staff) hoặc 2 (Lecturer).
- Việc làm: validate server, kiểm tra email trùng/role hợp lệ, tạo tài khoản qua Service/Repository/DAO; cách lưu password theo quyết định bảo mật/Assignment đã chốt.
- Đầu ra/kiểm tra: Account hợp lệ tạo được; input sai/role khác 1–2 không tạo; lỗi hiển thị trong modal.

### ACC-04 — Update Account bằng popup

- Đầu vào: Account ID và dữ liệu Edit đã nạp từ DB.
- Việc làm: kiểm tra Admin, ID, role, validation; cập nhật đúng tài khoản; không đổ mật khẩu hiện tại ra View; Admin cấu hình không bị nhầm với account DB.
- Đầu ra/kiểm tra: reload thấy thay đổi đúng ID; request giả mạo role/ID không vượt backend validation.

### ACC-05 — Soft delete và xóa vĩnh viễn Account có xác nhận

- Đầu vào: Account ID, Cancel hoặc Confirm.
- Việc làm: chỉ Confirm gọi backend; kiểm tra Admin và ID. Delete thường đặt IsDeleted = true, giữ các liên kết News. Xóa vĩnh viễn là thao tác riêng, Service chặn nếu còn CreatedById hoặc UpdatedById tham chiếu. Giữ email unique, không cascade xóa News; query News vẫn đọc được creator/editor đã soft delete.
- Đầu ra/kiểm tra: Cancel giữ nguyên; soft delete ngăn login và request xác thực tiếp theo; list/search thường ẩn Account đã xóa; News không biến mất khi nạp tác giả. Hard delete Account bị tham chiếu phải bị chặn; không tham chiếu được xóa sau xác nhận. Kiểm tra sai ID/role ở backend.

## F. Chức năng bổ trợ

### PROFILE-01 — Staff xem và sửa Profile của mình

- Đầu vào: danh tính Staff đã đăng nhập và form cập nhật.
- Việc làm: lấy account ID từ principal, nạp profile, validate server và cập nhật trường được phép; không cho đổi ID/role qua form.
- Đầu ra/kiểm tra: Staff sửa được mình, không sửa được người khác dù thay URL hoặc payload.

### HISTORY-01 — Staff xem News do mình tạo

- Đầu vào: account ID từ principal Staff.
- Việc làm: query News theo creator ID qua Service/Repository/DAO; hiển thị list/empty state.
- Đầu ra/kiểm tra: không trả News của Staff khác; không tin creator ID do client cung cấp.

### PUBLIC-01 — Public xem Active News

- Đầu vào: request không đăng nhập.
- Việc làm: query NewsStatus = 1 ở backend, hiển thị danh sách chỉ đọc.
- Đầu ra/kiểm tra: người chưa đăng nhập xem được; NewsStatus = 0 không xuất hiện.

### LECT-01 — Lecturer xem Active News

- Đầu vào: request từ account role 2.
- Việc làm: xác thực Lecturer, query NewsStatus = 1 ở backend và hiển thị chỉ đọc.
- Đầu ra/kiểm tra: Lecturer xem được Active; Inactive và mọi action quản lý đều bị chặn.

### REPORT-01 — Admin Report theo ngày tạo

- Đầu vào: StartDate, EndDate dạng ngày theo giờ Việt Nam UTC+7 từ form Admin.
- Việc làm: Service validate ngày, thứ tự và giới hạn ngày; chuyển 00:00 StartDate và 00:00 ngày sau EndDate sang UTC. DAL query `CreatedDate >= startUtc && CreatedDate < endExclusiveUtc`; sắp xếp CreatedDate giảm dần rồi NewsArticleId giảm dần; hiển thị kết quả/empty state.
- Đầu ra/kiểm tra: khoảng ngày sai hiển thị lỗi, không chạy query sai; chỉ Admin truy cập; test cùng ngày, bản ghi trong ngày kết thúc và bản ghi đúng mốc đầu ngày kế tiếp. Quy ước đã được chủ dự án duyệt tại BR-15.

## Tiêu chí chung cho mọi function

Backend role/ownership, server ModelState và business rule được kiểm tra dù người dùng bỏ qua UI. Các form thay đổi dữ liệu có antiforgery. Service không chứa View/HTTP logic; View không truy vấn DB; Controller không truy cập DbContext/DbSet/DAO/Repository trực tiếp. Khi đánh dấu Done, issue phải có đường đi thật qua các lớp, build/test pass và bằng chứng kiểm tra thủ công cho UI liên quan.

## G. Phân công nhóm 5 người

| Người | Phạm vi chính | Số task riêng |
| --- | --- | ---: |
| TV1 — chủ dự án | Schema, DB, toàn bộ DAL, cấu hình/DI DB | 5 |
| TV2 | Nền MVC/CI, UI chung, Auth và phân quyền | 4 |
| TV3 | News Article Management, Service query News dùng chung | 4 |
| TV4 | Category, Profile/History, Public/Lecturer | 5 |
| TV5 | Account Management, Report | 4 |
| Cả nhóm; TV1 điều phối | P00 duyệt phân công và Q01 tích hợp/demo | 2 |

Tổng **24 task**, gồm 22 task có owner riêng và 2 task chung. TV2–TV5 là ký hiệu tạm; phải gán tên/GitHub username và reviewer trước khi bắt đầu issue. Số task không phản ánh đầy đủ tải: Data, News Create/Update/Tag và tích hợp cần nhiều phối hợp. Mỗi người viết test cho phần mình trong task; không dồn mọi test sang Q01.

## H. Danh sách task và phụ thuộc

| ID | Owner chính | Function ID | Phụ thuộc để hoàn thành | Nội dung và đầu ra bắt buộc |
| --- | --- | --- | --- | --- |
| P00 | TV1 điều phối cả nhóm | — | Không | Ghi nhận business rules đã duyệt; chốt file map, người làm/review, scope, 31 function và 24 task; không coi duyệt rule là code đã xong |
| P01 | TV2 | ARCH-01, AUTH-04 | P00 | Kiểm tra phần nền đã có; .NET 8/CI khớp, startup/build; route Account/Login thực chạy khi A01 tích hợp |
| D00 | TV1 | — | P00 | Tự thiết kế schema theo SC-01..SC-09; chốt từ điển cột/khóa/độ dài, chính sách FK, môi trường và script tạo DB để review |
| D01 | TV1 | ARCH-02 | D00, P01 phần build nền | Tạo DB bằng script đã duyệt trên môi trường cho phép; Entities, DbContext/DbSet, mapping, SQL Server provider, cấu hình kết nối; dữ liệu mẫu nếu được issue cho phép |
| D02 | TV1 | ARCH-03 | D01 | Bốn DAO Instance Singleton thread-safe, EF/LINQ thật, không giữ scoped DbContext; thực hiện quan hệ và quy tắc xóa đã duyệt |
| D03 | TV1 | ARCH-04 hỗ trợ | D02 cho implementation | Chốt interface với TV2–TV5, triển khai Repository; đủ Account/Category/News/Tag và các query dùng chung; review contract có thể bắt đầu ngay sau D00 |
| D04 | TV1 | ARCH-04 | D03, P01 phần build nền | Đăng ký DI Data cùng TV2; kiểm tra lifetime, truy vấn chỉ đọc; bàn giao connection instructions và contract DAL đã xác nhận |
| U01 | TV2 | ARCH-05 | D04 cho tích hợp dữ liệu | Layout, navigation bốn vai trò, modal shell và hiển thị validation; thiết kế UI có thể chuẩn bị trong lúc chờ Data |
| A01 | TV2 | AUTH-01, AUTH-02 | D04, U01 | Login/Logout, Admin config, Staff/Lecturer DB, cookie/session và lỗi credential; hoàn thiện action cho AUTH-04 |
| A02 | TV2 | AUTH-03 | A01 | Backend role protection, identity/account ID dùng chung; kiểm thử URL trực tiếp với đúng/sai role |
| C01 | TV4 | CAT-01, CAT-02 | D04, A02 | Category List/Search và Service lookup cho News; keyword rỗng an toàn |
| C02 | TV4 | CAT-03, CAT-04 | C01, U01 | Category Create/Update modal, server validation, Category một cấp |
| C03 | TV4 | CAT-05 | C01, D03, U01 | Confirm/Cancel; CategoryService.DeleteCategory chặn Category có News Active hoặc Inactive |
| N01 | TV3 | NEWS-01, NEWS-02 | D04, A02 | News List/Search qua đủ lớp; chuẩn bị Service query Active/creator/CreatedDate cho TV4–TV5 theo contract |
| N02 | TV3 | NEWS-03, NEWS-04, NEWS-06 | N01, C01, U01 | Create/Update modal; tiêu đề/nội dung/Category/status bắt buộc, audit server, validation; phần Tag kết hợp N03 trước khi nghiệm thu hoàn chỉnh |
| N03 | TV3 | NEWS-07 | D00, D03, N02 phần form/lưu bài | News–Tag nhiều–nhiều; cho phép không Tag, thêm/gỡ/sync không trùng; TV1 review persistence; hoàn thiện các ca Create/Update có Tag |
| N04 | TV3 | NEWS-05 | N01, U01, N03 cho ca xóa bài có Tag | Confirm mới xóa News và liên kết của bài, giữ Category/Account/Tag; ID sai an toàn |
| M01 | TV5 | ACC-01, ACC-02 | D04, A02 | Account List/Search Admin-only; không lộ password; chốt Service contract cho Profile với TV4 |
| M02 | TV5 | ACC-03, ACC-04 | M01, U01 | Account Create/Update modal, role 1/2, email/validation; phối hợp TV2 về credential format |
| M03 | TV5 | ACC-05 | M01, U01, D01 bổ sung IsDeleted, A01/A02 | Confirm/Cancel; soft delete giữ News; hard delete chỉ khi không còn cả hai FK tham chiếu; TV2 phối hợp chặn login/phiên hiện tại |
| S01 | TV4 | PROFILE-01, HISTORY-01 | A02, N01, AccountService Profile contract từ TV5 | Profile của chính Staff, không cho đổi role/ID; History lọc creator thực; TV3/TV5 cung cấp Service method đã thống nhất |
| V01 | TV4 | PUBLIC-01, LECT-01 | D04, A02, N01 | Public truy cập không login, Lecturer chỉ đọc, backend lọc Active; TV3 cung cấp query |
| R01 | TV5 | REPORT-01 | A02, N01 | StartDate/EndDate, validation, CreatedDate filter/sort giảm dần; TV3 cung cấp query qua Service |
| Q01 | TV1 điều phối cả nhóm | ARCH-01, ARCH-04 kiểm tra | Các task implementation trên | Tích hợp DI và role/UI, build/test/CI, demo News end-to-end; kiểm thử regression, xóa có quan hệ, Profile/History/Report |

Các phụ thuộc trong bảng là điều kiện để nghiệm thu đầy đủ. P01 có phần build nền bàn giao sớm cho D01; phần route Login chỉ hoàn thành sau A01, không tạo vòng chờ giữa D01 và A01. N02/N03 được phối hợp: phần lưu bài trước, gắn Tag sau, nghiệm thu tổng thể sau cả hai.

## I. File ownership và task phải làm chung

| Thành phần | Owner/phối hợp | Quy tắc làm việc |
| --- | --- | --- |
| Schema, script DB, toàn bộ AIVES.DAL | TV1; TV2–TV5 review contract | Thành viên khác yêu cầu query/mapping qua issue; không tự sửa Data để vá UI |
| Program.cs | TV2 quản lý file; TV1 phụ trách nội dung DB | Một người sửa tại một thời điểm; DB DI và Auth/MVC theo PR có thứ tự; chỉ đăng ký Service khi implementation tồn tại |
| AuthService, LoginViewModel, Views/Account/Login | TV2 | TV5 phối hợp credential/role; không tự viết lại Auth |
| AccountController dùng chung Login và Admin CRUD | TV2 ở A01, bàn giao TV5 ở M01–M03 | Không sửa song song cùng file; TV5 giữ nguyên Login/Logout; nếu muốn tách Controller phải chốt lại file map trong issue trước |
| Layout, navigation, modal/shared JS | TV2 | TV3–TV5 dùng contract UI thống nhất, yêu cầu thay đổi qua issue/PR |
| NewsArticleService/interface và News MVC | TV3 | TV4 cần Active/History, TV5 cần Report; chốt chữ ký query trước khi làm module tiêu thụ |
| CategoryService/interface và Category MVC | TV4 | TV3 dùng lookup khi tạo/sửa News; TV1 cung cấp kiểm tra tham chiếu cho rule xóa |
| SystemAccountService/interface và Admin Account MVC | TV5 | TV4 cần Profile method; chốt contract sớm, TV4 không tự sửa file Service của TV5 |
| Profile/History/Home MVC | TV4 | Gọi Service đã thống nhất với TV3/TV5; account ID lấy từ principal qua cơ chế TV2 |
| Report MVC | TV5 | Dùng News Service query do TV3 sở hữu; TV1 triển khai query DAL |
| News–Tag | TV3 + TV1 | TV3 phụ trách form/use case; TV1 phụ trách mapping/transaction, kiểm thử không trùng và không mất Tag dùng chung |
| Project files và interface dùng chung | Owner liên quan; TV1 review Data | Không thêm package/reference hoặc đổi contract ngoài issue; phân một người chỉnh file tại một thời điểm |

Account Index/Create/Edit/Delete partial hiện chưa có: TV5 chỉ tạo các View cần thiết trong issue M01–M03. Không tạo sẵn class/file rỗng để đánh dấu task hoàn thành. Chỉ thêm TagService hoặc Service khác khi use case cần và đã thống nhất owner, không nhân đôi query/implementation.

## J. Thứ tự và mốc bàn giao Data

1. P00 chốt phạm vi và phân công. Quyết định nghiệp vụ SC-01..SC-09 đã duyệt không cần chờ duyệt lại; những chi tiết D00 còn mở vẫn phải được giải quyết.
2. P01 phần nền và D00 chạy song song. TV2–TV5 chuẩn bị UI/Service design, nhu cầu query, ca kiểm thử trên issue; không tự đoán kiểu dữ liệu.
3. D00 → D01 → D02 → D03 implementation → D04. Review Repository/Service contract có thể làm sớm trong giai đoạn Data; DAL handoff chỉ đạt khi implementation và truy vấn đã kiểm tra.
4. Sau D04, các owner triển khai Service/MVC theo contract. TV2 hoàn thiện UI/Auth; nghiệm thu module bảo vệ cần A02. C01, N01, M01 có thể triển khai song song khi các đầu vào của chúng đã sẵn sàng.
5. Hoàn thiện CRUD, Tag, Profile/History, Public/Lecturer, Report rồi Q01. Service DI được bổ sung trong issue của từng feature; Q01 kiểm tra toàn bộ.

Các mục bàn giao của TV1:

Bổ sung SC-10/11 đã triển khai ở DAL: script 002 đã được chủ dự án chạy, Entity/mapping IsDeleted và contract soft/hard delete có thật; News–NewsTag có test atomic rollback. TV2 còn kiểm tra Account trong Auth/phiên; TV5 còn thao tác Admin; TV3 còn Service/News UI.

- D00: schema có phiên bản, data dictionary, PK/FK, uniqueness, nullability, kiểu ID, audit ban đầu, policy cho Account chỉ là người sửa, credential format, môi trường DB và script được review; tuân thủ SC-01..SC-09.
- D01: schema và Entity/DbContext khớp; SQL Server cấu hình từ appsettings; script trong repository dùng lại được trên môi trường phát triển. Việc chạy script tạo DB cần nằm trong phạm vi issue/môi trường đã được cho phép, không chạy tự động từ yêu cầu sửa tài liệu.
- D02: DAO Instance thread-safe, không giữ scoped context; kiểm tra persistence/quan hệ trên DB test được phép khi issue yêu cầu ghi dữ liệu.
- D03: chủ dự án đã duyệt phạm vi contract cho Auth/Account, Category lookup/usage, News CRUD/Search/Active/creator/CreatedDate và NewsTag. TV1 triển khai chữ ký/implementation cụ thể, ghi đầu vào, đầu ra và lỗi để TV2–TV5 sử dụng; trạng thái duyệt phạm vi không đồng nghĩa code đã hoàn thành.
- D04: DI đúng và truy vấn chỉ đọc thực chạy; hướng dẫn cấu hình local, phiên bản schema và kết quả kiểm tra. Không dùng migration/write để thử kết nối.

Nếu chưa có thiết kế cột/khóa chi tiết, ghi **CHỜ THIẾT KẾ CHI TIẾT D00**; không ghi “chờ script giảng viên”. Sau D04, đổi Data contract cần issue riêng và TV1 review.

## K. Quy tắc tạo issue, review và Done

Dùng [issue template](../.github/ISSUE_TEMPLATE/task-assignment.md). Mỗi issue có ID task/function, một owner chính, reviewer được nêu tên, danh sách file được sửa, phụ thuộc, phạm vi DB nếu có, acceptance criteria và ngoài phạm vi. P00/Q01 vẫn có TV1 điều phối. Tên TV2–TV5 phải được thay bằng thành viên thật trước khi assign; chưa tự tạo CODEOWNERS/branch protection khi chưa có tài khoản và quyết định của nhóm.

Structure review xác nhận ranh giới, tên file, contract và ownership; không thay thế nghiệm thu implementation. PR implementation cần đường đi thật View → Controller → Service → Repository → DAO → DbContext, backend role/ownership, server validation, test có assertion, kết quả build/test và manual UI checks. Test gồm dữ liệu hợp lệ/sai, ID mất, query rỗng, role sai, Category/Account/Tag bị chặn xóa, News không Tag, nhiều Tag và xóa News giữ dữ liệu dùng chung. Không merge khi CI fail; không xem run không có test là đã kiểm thử nghiệp vụ. Git theo skill team-git-workflow; không tự push hoặc merge khi chỉ được giao sửa tài liệu/code local.

## L. Bàn giao hiện tại cho 5 người

| Người | Bắt đầu tiếp theo | Làm chung / đầu vào |
| --- | --- | --- |
| TV1 — Kiên | Review và bàn giao D00–D04 đã có code/test; hỗ trợ thay đổi contract qua issue, không tự sửa module của người khác | TV2 review DI/Auth queries; TV3 review NewsTag; TV4/TV5 review query dùng chung |
| TV2 | P01, U01, A01, A02: layout/modal, Login/Logout, cookie/role, từ chối Account đã xóa ở request tiếp theo | TV5 thống nhất PasswordHasher; lần lượt sửa Program.cs và AccountController, không cùng lúc |
| TV3 | N01–N04: News Service + MVC, chốt Service query Active/History/Report trước khi người khác dùng | TV4 Category lookup; TV1 persistence; TV5 Report; dùng contract DAL đã có |
| TV4 | C01–C03 rồi S01/V01: Category CRUD, Profile/History, Public/Lecturer | CategoryService vẫn phải kiểm tra HasNewsAsync; nhận Service contract News từ TV3 và Profile từ TV5 |
| TV5 | M01–M03 rồi R01: Account Service/CRUD soft-hard delete, Report ngày Việt Nam | TV2 hash/role/khóa phiên; TV4 Profile method; TV3 News query |

Phần có thể làm song song: sau khi chốt BLL interfaces, TV2 Auth, TV3 News Service, TV4 Category Service, TV5 Account Service. UI bảo vệ chỉ nghiệm thu khi A02 xong. Không tự sửa schema/DAL để né một contract thiếu; yêu cầu TV1 trên issue.

Việc tổ chức còn thiếu: tên/GitHub username của TV2–TV5 và reviewer thật, issue numbers, BLL interface/DTO dùng chung, peer review/CI trước merge. Chưa tạo/gán GitHub issue hoặc PR; không coi bảng task là issue đã tồn tại. Nhánh bàn giao là `kien/data-access-handoff`, bao gồm rename AIVES/schema/docs đã được chủ dự án duyệt và DAL lần này. `docs/skill-review.md` là thay đổi ngoài gói bàn giao, giữ local và không commit.
