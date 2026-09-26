# Đặc tả chức năng — FU News Management System

**Mục đích:** nêu rõ từng function phải nhận gì, xử lý gì, trả gì và kiểm tra thế nào. Đây là yêu cầu dự kiến, **không phải trạng thái code đã hoàn thành**. Business rules có mã BR ở [business-rules.md](business-rules.md); issue thực hiện nằm ở [bảng DB-first](assignment01-db-first-function-task-map.md).

Có **31 function ID**: Architecture 5, Authentication 4, News 7, Category 5, Account 5, Report 1, Profile 1, History 1, Public 1, Lecturer 1. News Article Management là luồng demo chính. Mọi function chạm DB phải đi qua Controller → Service → Repository → DAO/DbContext; tên method cụ thể được chốt ở issue sau khi có schema.

## A. Nền tảng và kiến trúc

### ARCH-01 — Cấu trúc MVC + ba lớp

- Đầu vào: yêu cầu Assignment và file map đã duyệt.
- Việc làm: có Web MVC, Services, Repositories, DataAccess, BusinessObjects và Tests; project reference chỉ đi theo chiều đã định; framework thuộc .NET 5/6/7/8; giải quyết khác biệt với CI.
- Đầu ra/kiểm tra: ứng dụng MVC có startup, Controller nhận request và trả View/Redirect; không có project reference vòng; solution build được. Việc có thư mục hoặc file rỗng chưa đạt function này.

### ARCH-02 — Entity, DbContext và SQL Server

- Đầu vào: schema SQL Server chính thức và connection string cấu hình.
- Việc làm: định nghĩa Entity, DbSet, khóa, quan hệ và mapping theo đúng schema; cấu hình EF Core SQL Server, không đoán News–Tag hoặc kiểu cột.
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

Đường đi phải được kiểm chứng cho từng thao tác: View News → NewsArticleController → INewsArticleService/NewsArticleService → INewsArticleRepository/NewsArticleRepository → NewsArticleDAO → FUNewsManagementDbContext → SQL Server. NewsTag/Tag được xử lý theo schema chính thức; không xem tên file là bằng chứng quan hệ.

### NEWS-01 — List News

- Đầu vào: request Staff đến trang News.
- Việc làm: Service lấy danh sách qua Repository/DAO; nạp thông tin Category, Tags, Status và creator theo dữ liệu có thật; View hiển thị danh sách kể cả trường hợp rỗng.
- Đầu ra/kiểm tra: Staff thấy News tương ứng, link/button đi tới action có thật; role khác không vào trang quản lý.

### NEWS-02 — Search News

- Đầu vào: keyword và bộ lọc được UI cung cấp; giá trị null/rỗng được chấp nhận.
- Việc làm: input đi từ View qua Controller/Service đến LINQ ở tầng Data; lọc trên trường tìm kiếm được chốt theo schema, không lọc giả bằng JavaScript trên một trang dữ liệu thiếu.
- Đầu ra/kiểm tra: có keyword trả đúng tập con; keyword trống trả danh sách hợp lý; không lỗi null; không vượt quyền Staff.

### NEWS-03 — Create News bằng popup

- Đầu vào: form modal gồm trường News bắt buộc theo schema, Category, Tags và Status; account ID lấy từ Staff đăng nhập.
- Việc làm: mở modal thật, nạp Category/Tag từ dữ liệu thật, validate server; Service quyết định creator và ngày tạo, gọi Repository/DAO để lưu News và liên kết liên quan.
- Đầu ra/kiểm tra: dữ liệu hợp lệ được tạo và xuất hiện trong list; dữ liệu sai không tạo bản ghi và lỗi hiển thị trong modal; CSRF token hợp lệ.

### NEWS-04 — Update News bằng popup

- Đầu vào: News ID và form Edit; modal nạp dữ liệu News, Category, Tags, Status hiện tại.
- Việc làm: kiểm tra ID tồn tại, quyền và ModelState; Service cập nhật đúng bản ghi, giữ nguyên creator/ngày tạo, cập nhật thông tin sửa nếu schema có; đồng bộ Tag theo NEWS-07.
- Đầu ra/kiểm tra: modal có dữ liệu cũ, Update lưu đúng ID, lỗi nhập liệu không làm mất dữ liệu, ID không tồn tại được xử lý an toàn.

### NEWS-05 — Delete News có xác nhận

- Đầu vào: News ID; lần đầu bấm Delete chỉ mở dialog.
- Việc làm: Cancel không gửi lệnh xóa; Confirm gửi request thay đổi dữ liệu được bảo vệ; backend kiểm tra ID/quyền, xóa News và xử lý liên kết theo schema.
- Đầu ra/kiểm tra: chỉ Confirm mới xóa; ID sai/đã xóa không làm ứng dụng crash; không còn liên kết hỏng.

### NEWS-06 — Validation News

- Đầu vào: dữ liệu Create/Update.
- Việc làm: ViewModel có Required/Length/DataType phù hợp schema; Controller kiểm tra ModelState; Service kiểm tra business rule như Category/Tag hợp lệ và trạng thái cho phép; không tin creator/client audit fields.
- Đầu ra/kiểm tra: request sai bị từ chối ở server, không ghi DB; lỗi rõ và modal vẫn dùng được sau khi sai.

### NEWS-07 — Gắn, sửa và gỡ Tag

- Đầu vào: danh sách Tag được chọn khi Create/Update.
- Việc làm: kiểm tra Tag tồn tại; lưu/sync quan hệ theo schema thực; thêm không trùng, gỡ Tag bỏ chọn, xử lý khi Delete. **CHỜ SCHEMA:** khóa và cardinality chưa được xác nhận.
- Đầu ra/kiểm tra: reload News vẫn thấy Tag đúng; Update không nhân đôi; bỏ Tag hoặc xóa News không để quan hệ sai.

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

### ACC-05 — Delete Account có xác nhận

- Đầu vào: Account ID, Cancel hoặc Confirm.
- Việc làm: chỉ Confirm gọi backend; kiểm tra Admin và ID; xử lý ràng buộc tham chiếu theo schema/quyết định đã chốt, không tự chọn cascade.
- Đầu ra/kiểm tra: Cancel giữ tài khoản; Confirm xử lý an toàn; sai ID và sai role bị từ chối phù hợp.

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

- Đầu vào: StartDate, EndDate từ form Admin.
- Việc làm: validate giá trị và thứ tự ngày; query News theo CreatedDate bằng LINQ; sắp xếp CreatedDate giảm dần; hiển thị kết quả/empty state.
- Đầu ra/kiểm tra: khoảng ngày sai hiển thị lỗi, không chạy query sai; chỉ Admin truy cập; kết quả đúng khoảng và đúng thứ tự. Quy ước EndDate/múi giờ được chốt trước khi triển khai.

## Tiêu chí chung cho mọi function

Backend role/ownership, server ModelState và business rule được kiểm tra dù người dùng bỏ qua UI. Các form thay đổi dữ liệu có antiforgery. Service không chứa View/HTTP logic; View không truy vấn DB; Controller không truy cập DbContext/DbSet/DAO/Repository trực tiếp. Khi đánh dấu Done, issue phải có đường đi thật qua các lớp, build/test pass và bằng chứng kiểm tra thủ công cho UI liên quan.
