# Lưu trữ News — business rules

> BẢN LỊCH SỬ: nội dung bên dưới thuộc News Management trước khi đổi main flow. Không dùng các quyền, yêu cầu, tiến độ hoặc lệnh setup bên dưới làm chỉ dẫn cho nhóm 6 thi vấn đáp. Không tự chạy SQL. Nguồn hiện hành: [business rules](../business-rules.md), [architecture](../ARCHITECTURE.md), [tasks](../TASKS.md).

# Business rules — FU News Management System

**Trạng thái:** quy tắc nghiệp vụ đã được chủ dự án xác nhận để làm cơ sở thiết kế DB. Chủ dự án đã tạo DB `AIVES`; Entities/EF mapping đã được đối chiếu với schema thực bằng truy vấn chỉ đọc. Chức năng nghiệp vụ chưa được triển khai. Giảng viên **không cung cấp script DB**; nhóm tự thiết kế schema Microsoft SQL Server theo các quyết định dưới đây.

Đây là nguồn quy tắc nghiệp vụ dùng chung. Đọc tiếp [kiến trúc](NEWS-ARCHITECTURE.md) và [task, phân công, đặc tả function](../TASKS.md). Quy tắc làm việc của agent nằm ở [AGENTS.md](../../AGENTS.md).

## 0. Quyết định thiết kế đã được chủ dự án duyệt

| Mã | Nội dung đã chốt | Hệ quả cần thể hiện trong DB và nghiệp vụ |
| --- | --- | --- |
| SC-01 | Mỗi News bắt buộc thuộc đúng một Category; một Category có thể có nhiều News | CategoryId bắt buộc và tham chiếu Category tồn tại; Category chưa có News vẫn hợp lệ |
| SC-02 | News và Tag có quan hệ nhiều–nhiều, qua NewsTag | Một bài có nhiều Tag; một Tag dùng cho nhiều bài; không lặp cùng cặp News–Tag |
| SC-03 | News không bắt buộc có Tag | Danh sách Tag rỗng được chấp nhận; được gỡ hết Tag khỏi bài |
| SC-04 | Category chỉ có một cấp | Không thêm quan hệ cha–con hoặc ParentCategoryId trong phạm vi đã duyệt |
| SC-05 | Từ chối xóa vĩnh viễn Account còn được News tham chiếu qua CreatedById hoặc UpdatedById | Giữ creator/người sửa gần nhất; soft delete Account theo SC-10; không cascade xóa News theo Account |
| SC-06 | Từ chối xóa Tag đang được News sử dụng | Có thể gỡ liên kết Tag khỏi từng News; việc gỡ liên kết không xóa Tag dùng chung |
| SC-07 | Xóa News thì xóa bài và các liên kết NewsTag của bài | Giữ lại Category, Account và Tag; thực hiện nhất quán, không để liên kết mồ côi |
| SC-08 | Có lưu người sửa và ngày sửa News | Có UpdatedById và ModifiedDate; Update giữ nguyên CreatedById và CreatedDate |
| SC-09 | Khi tạo News, bắt buộc có tiêu đề, nội dung, Category và trạng thái | Creator và ngày tạo do hệ thống ghi từ Staff đang đăng nhập; không tin trường audit do client gửi |
| SC-10 | Admin soft delete Account để ngừng sử dụng; hard delete chỉ khi không còn News tham chiếu | Bổ sung IsDeleted, chặn đăng nhập/phiên hiện tại, giữ Account cho thông tin tác giả; không tự cấp quyền Admin xóa News |
| SC-11 | Lưu thay đổi News, audit và các liên kết NewsTag một cách nguyên tử | Ưu tiên một SaveChangesAsync; nếu cần nhiều lần lưu, cùng transaction; lỗi thì rollback toàn bộ lần lưu |

SC-01 đến SC-09 là quyết định nền; SC-05 được làm rõ và SC-10/11 được chủ dự án duyệt bổ sung. Các Entity nghiệp vụ là SystemAccount, Category, NewsArticle, Tag; NewsTag biểu diễn liên kết. Schema nền và IsDeleted đã có trong DB/Entity. DAO/Repository đã triển khai; Service/Auth/UI còn cần làm, không nhầm data contract với chức năng hoàn chỉnh.

## 1. Vai trò và quyền truy cập

| Vai trò | Danh tính | Được phép | Không được phép |
| --- | --- | --- | --- |
| Public | Không đăng nhập | Xem News Active | Vào trang quản lý, xem News Inactive |
| Lecturer | AccountRole = 2 | Đăng nhập, xem News Active | Tạo/sửa/xóa News, Category, Account; xem báo cáo Admin |
| Staff | AccountRole = 1 | Quản lý News và Category, xem/sửa Profile của mình, xem News do mình tạo | Quản lý Account, xem báo cáo Admin, sửa Profile người khác |
| Admin | Tài khoản mặc định lấy từ appsettings.json | Quản lý Account Staff/Lecturer, xem Report | Dùng quyền Staff chỉ vì menu hiển thị; quyền bổ sung phải được quyết định rõ |

- **BR-01 — Đăng nhập:** nhập Email và Password. Tài khoản Admin được đối chiếu với DefaultAdmin:Email/Password trong appsettings.json; Staff/Lecturer được đối chiếu với tài khoản trong DB. Mật khẩu tài khoản DB được tạo và xác minh bằng `PasswordHasher<SystemAccount>` của ASP.NET Core Identity; chỉ lưu giá trị hash trong `AccountPasswordHash`, không so sánh plaintext hoặc tự tạo thuật toán hash. Admin mặc định vẫn lấy mật khẩu cấu hình theo yêu cầu Assignment, không có dòng Admin trong DB. Đăng nhập sai không tạo trạng thái xác thực. Đăng xuất xóa trạng thái xác thực.
- **BR-02 — Phân quyền phía server:** Controller/action phải kiểm tra vai trò và danh tính thật. Ẩn menu không phải bảo vệ endpoint. Request không đăng nhập hoặc sai quyền phải bị từ chối/chuyển hướng phù hợp.
- **BR-03 — Phạm vi bản thân:** Profile và History lấy account ID từ trạng thái đăng nhập, không tin ID tùy ý do URL/form gửi. Staff không được sửa Profile hoặc xem History của người khác.
- **BR-04 — Public/Lecturer News:** chỉ trả về NewsStatus = 1 từ tầng truy vấn; NewsStatus = 0 không xuất hiện qua các endpoint này. Public truy cập được mà không cần đăng nhập.

## 2. News Article — luồng demo chính

- **BR-05 — Quản lý News:** Staff có thể list, search, create, update và delete News. Mỗi thao tác dữ liệu phải đi qua Controller → Service → Repository → DAO/DbContext.
- **BR-06 — Category của News:** Create/Update phải chọn đúng một Category tồn tại (SC-01). Category chỉ có một cấp (SC-04). Không lưu tham chiếu Category không tồn tại; dropdown được nạp từ dữ liệu thật.
- **BR-07 — Creator và thời gian:** khi tạo News, CreatedById lấy từ Staff đang đăng nhập và CreatedDate do hệ thống ghi. Khi sửa, hệ thống ghi UpdatedById và ModifiedDate, giữ nguyên creator và ngày tạo (SC-08). Không tin giá trị audit do client gửi. Bản script D00 ở mục 6 dùng audit UTC, UpdatedById/ModifiedDate cùng null trước lần sửa đầu và cùng có giá trị sau khi sửa.
- **BR-08 — Tags:** quan hệ nhiều–nhiều qua NewsTag đã được duyệt (SC-02). Cho phép không chọn Tag (SC-03); thêm không trùng cặp, gỡ Tag chỉ xóa liên kết. Khi Delete News, xóa các liên kết của bài nhưng giữ Tag (SC-07).
- **BR-09 — Status:** NewsStatus = 1 là Active, 0 là Inactive. Staff quản lý status; Public/Lecturer chỉ đọc Active. Status bắt buộc khi tạo; bản script D00 dùng TINYINT với CHECK 0/1, không đặt giá trị mặc định để bên tạo phải truyền status.
- **BR-10 — Search:** keyword null/rỗng/chỉ có khoảng trắng trả danh sách trong phạm vi quyền truy cập; keyword có nội dung được `Trim()` và lọc bằng LINQ ở DAL. News tìm theo `NewsTitle` hoặc `NewsContent`; Category theo `CategoryName` hoặc `CategoryDescription` (NULL được xem là không khớp); Account theo `AccountName` hoặc `AccountEmail`. Không tìm trên password/hash hoặc dữ liệu ngoài schema. Việc trả dữ liệu đúng quyền vẫn do backend kiểm soát, không dựa vào UI.

## 3. Category, Account, Profile và Report

- **BR-11 — Xóa Category đang dùng:** CategoryService.DeleteCategory() kiểm tra có News tham chiếu Category hay không. Có tham chiếu thì từ chối xóa và trả lỗi nghiệp vụ; không có thì cho phép xóa. Chặn ở UI không thay thế kiểm tra ở Service. Quy tắc áp dụng dù News đang Active hay Inactive, vì yêu cầu là “đang được News sử dụng”.
- **BR-12 — Account Management:** chỉ Admin được list/search/create/update/delete tài khoản Staff và Lecturer. Role hợp lệ là 1 hoặc 2. Không hiển thị password/hash trong UI. Chuẩn hóa email bằng `Trim()` và `ToLowerInvariant()` khi tạo/sửa/đăng nhập; Service kiểm tra trùng, unique constraint DB bảo vệ cả ghi đồng thời. Dùng `PasswordHasher<SystemAccount>` thống nhất giữa Auth và Account. Delete thông thường là soft delete theo BR-26; thao tác xóa vĩnh viễn riêng phải từ chối nếu còn News tham chiếu CreatedById hoặc UpdatedById. Giữ FK NO ACTION, không cascade xóa News hoặc xóa trắng audit để xóa Account.
- **BR-13 — Staff Profile:** chỉ đọc/sửa tài khoản của chính mình; form không được tự nâng role hoặc đổi ID. Dữ liệu cập nhật phải được validate ở server.
- **BR-14 — Own News History:** lọc theo creator/account ID thật của Staff hiện tại. Không trả News do Staff khác tạo.
- **BR-15 — Admin Report:** chỉ Admin truy cập; nhận StartDate/EndDate dạng ngày theo giờ Việt Nam UTC+7 và từ chối ngày thiếu/sai hoặc StartDate > EndDate. Service chuyển 00:00 StartDate và 00:00 ngày sau EndDate sang UTC, kiểm tra mốc cuối không vượt phạm vi ngày hỗ trợ. DAL lọc `CreatedDate >= startUtc && CreatedDate < endExclusiveUtc`, lấy đủ ngày kết thúc; sắp xếp `CreatedDate` giảm dần rồi `NewsArticleId` giảm dần khi trùng thời gian. Đây là quyết định đã được chủ dự án duyệt (mục đề xuất D07).

## 4. Quy tắc giao diện, validation và kiến trúc

- **BR-16 — Popup:** Create và Update cho News, Category, Account phải mở modal/dialog thực sự, nạp dữ liệu cần thiết và hiển thị lỗi khi input sai; một trang Create/Edit độc lập không thay thế yêu cầu này.
- **BR-17 — Xác nhận xóa:** lần nhấn Delete đầu chỉ mở xác nhận với Cancel và Confirm. Cancel không gửi thao tác xóa. Confirm mới gọi backend; backend xử lý ID thiếu/sai an toàn.
- **BR-18 — Server validation:** Login, News, Category, Account, Profile và Report phải kiểm tra dữ liệu ở server qua ViewModel/ModelState và rule nghiệp vụ. JavaScript validation chỉ hỗ trợ trải nghiệm, không quyết định dữ liệu có hợp lệ hay không.
- **BR-19 — Nguồn cấu hình:** ConnectionStrings:DefaultConnection và DefaultAdmin:Email/Password lấy từ appsettings.json/ASP.NET configuration, không hard-code C# credential hoặc connection string trong Controller/Service/Repository/DAO.
- **BR-20 — Ranh giới lớp:** Controller chỉ dùng Service interface. Service chứa/điều phối business rule và gọi Repository; Repository gọi DAO; DAO dùng EF Core/LINQ với DbContext. Không bỏ Service hoặc Repository chỉ vì EF Core đã có DbContext.
- **BR-21 — Lifetime:** bốn DAO bắt buộc có Instance Singleton thread-safe; DAO Singleton không giữ DbContext scoped trong field. DbContext không Singleton. DI đăng ký lifetime phù hợp.
- **BR-22 — Chất lượng bàn giao:** build solution và test có ý nghĩa phải pass; CI phải pass trước merge vào main. Không coi file rỗng, method placeholder hay test không có assertion là chức năng hoàn thành.

- **BR-23 — Xóa Tag:** từ chối xóa Tag khi còn bất kỳ liên kết NewsTag nào; gỡ liên kết khỏi từng bài không xóa Tag (SC-06). Đây là quy tắc Data/Service, không tự mở rộng thành yêu cầu tạo màn hình CRUD Tag riêng.
- **BR-24 — Xóa News:** sau khi xác nhận và kiểm tra quyền, xóa News cùng các NewsTag của bài; giữ Category, Account và Tag. Việc xóa phải nhất quán trong một thao tác lưu/transaction phù hợp; D00 chốt cách thực hiện FK/cascade (SC-07).
- **BR-25 — Nội dung News bắt buộc:** tiêu đề và nội dung không rỗng sau khi loại khoảng trắng, Category tồn tại, status hợp lệ. Tags có thể rỗng. Kiểm tra server-side khi Create/Update; độ dài theo schema chi tiết (SC-09).

- **BR-26 — Soft delete Account (đã duyệt):** bổ sung `SystemAccount.IsDeleted BIT NOT NULL DEFAULT 0`, map thành bool. Admin xác nhận Delete thì đặt true; không xóa dòng Account hoặc News. List/search Account thông thường chỉ lấy IsDeleted = false. Từ chối đăng nhập Account đã xóa; kiểm tra trạng thái ở request xác thực tiếp theo để phiên đã cấp không tiếp tục dùng quyền. Query News phải giữ được creator/editor đã soft delete, tránh global filter Account làm mất News khi JOIN navigation bắt buộc. Email của Account soft delete vẫn bị giữ bởi UNIQUE; chưa mở chính sách tái sử dụng email. Hard delete có xác nhận riêng, chỉ khi không còn bất kỳ News nào tham chiếu cả hai FK. Không mở rộng quyền Admin xóa News từ quyết định này.
- **BR-27 — Audit và lưu nguyên tử (đã duyệt):** khi Create, Service lấy creator từ Staff đăng nhập, DB cấp CreatedDate UTC; UpdatedById/ModifiedDate ban đầu NULL. Khi Update, Service giữ thông tin tạo, đặt editor và thời gian UTC. Đây chỉ là người sửa gần nhất, không phải bảng lịch sử mọi lần sửa. DAL gom thay đổi nội dung, audit, thêm/gỡ NewsTag vào một SaveChangesAsync; nếu use case cần nhiều lần Save thì bao cùng transaction. Thành công tất cả hoặc rollback toàn bộ; không giữ transaction trong lúc người dùng đang mở modal. Service quyết định nghiệp vụ, Repository/DAO thực hiện lưu/transaction; BLL không truy cập DbContext.

## 5. Trạng thái D00 và phần còn cần review

1. Chủ dự án đã chạy script 001/002 thành công; metadata và EF read đã được kiểm tra. Entities/DbContext, DAO Singleton, Repository và DI Data đã triển khai, có test SQL Server trên DB tạm. Chưa có CRUD qua Service/MVC.
2. IsDeleted đã map bool/default false. DAL ẩn Account soft-deleted trong query thông thường, giữ tác giả News, chặn hard delete theo cả hai FK. TV2/TV5 còn triển khai chặn login/phiên hiện tại và thao tác Admin. Quyền Admin vẫn là Account/Report.
3. Đã chốt cơ chế `PasswordHasher<SystemAccount>` cho Staff/Lecturer, chuẩn hóa email và trường Search ở BR-01/10/12. TV1/TV2/TV5 dùng cùng định dạng hash và cùng cách chuẩn hóa; chưa triển khai Auth/Account thực tế.
4. Contract Repository cho Auth/Account, Category, News/NewsTag và Tag đã có chữ ký/implementation; xem mục 8 ARCHITECTURE.md. DAL Report lọc khoảng UTC; TV3/TV5 còn xử lý input ngày Việt Nam ở BLL. Lưu News–NewsTag nguyên tử đã test rollback. Chưa seed dữ liệu mẫu vào AIVES.

Không ghi News–Tag hay các chính sách đã duyệt là “chờ giảng viên cung cấp schema”. Schema vật lý D00 đã được tạo và đối chiếu với Entity/mapping; các quyết định còn mở ở trên là chính sách ứng dụng/contract triển khai, không phải yêu cầu tạo lại DB. Cập nhật tài liệu này không thay đổi database.

## 6. Script DB và từ điển dữ liệu D00

Bảng dưới mô tả schema hiện có gồm `SystemAccount.IsDeleted BIT NOT NULL DEFAULT 0` (SC-10). Chủ dự án đã chạy [002-add-system-account-soft-delete.sql](../../database/002-add-system-account-soft-delete.sql); Entity/mapping đã cập nhật và query EF đã pass. DB mới chạy 001 rồi 002; DB có schema nền chỉ chạy 002. Không chạy lại initializer 001 để nâng cấp. Script 002 gán 0 khi thêm cột; chạy lại giữ nguyên cờ xóa, đã có test trên DB tạm.

File: [database/001-create-schema.sql](../../database/001-create-schema.sql). Đây là bản thiết kế kỹ thuật cụ thể để chủ dự án review và chạy trên SQL Server phát triển; SC-01..SC-09 vẫn là những quyết định nghiệp vụ đã duyệt. Script không tự được chạy bởi việc sửa tài liệu hoặc build solution.

| Bảng | Cột và kiểu SQL | Null/default |
| --- | --- | --- |
| SystemAccount | AccountId INT IDENTITY(1,1), PK | NOT NULL; DB tự sinh |
| SystemAccount | AccountName NVARCHAR(100) | NOT NULL; không rỗng |
| SystemAccount | AccountEmail NVARCHAR(254) | NOT NULL; không rỗng; UNIQUE, collation Latin1_General_100_CI_AS không phân biệt hoa/thường |
| SystemAccount | AccountPasswordHash NVARCHAR(512) | NOT NULL; không rỗng; không seed mật khẩu |
| SystemAccount | AccountRole TINYINT | NOT NULL; CHECK IN (1,2); không default |
| SystemAccount | IsDeleted BIT | NOT NULL; DEFAULT 0; soft delete đặt 1 |
| Category | CategoryId INT IDENTITY(1,1), PK | NOT NULL |
| Category | CategoryName NVARCHAR(100) | NOT NULL; không rỗng |
| Category | CategoryDescription NVARCHAR(500) | NULL được phép; không có cột cha–con |
| Tag | TagId INT IDENTITY(1,1), PK | NOT NULL |
| Tag | TagName NVARCHAR(100) | NOT NULL; không rỗng |
| NewsArticle | NewsArticleId INT IDENTITY(1,1), PK | NOT NULL |
| NewsArticle | NewsTitle NVARCHAR(200), NewsContent NVARCHAR(MAX) | NOT NULL; không rỗng |
| NewsArticle | CategoryId INT, FK Category | NOT NULL |
| NewsArticle | NewsStatus TINYINT | NOT NULL; CHECK IN (0,1); không default |
| NewsArticle | CreatedById INT, FK SystemAccount | NOT NULL |
| NewsArticle | CreatedDate DATETIME2(3) | NOT NULL; mặc định SYSUTCDATETIME(), lưu UTC |
| NewsArticle | UpdatedById INT, FK SystemAccount; ModifiedDate DATETIME2(3) | Cùng NULL trước lần sửa đầu; cùng có giá trị sau sửa; ModifiedDate >= CreatedDate |
| NewsTag | NewsArticleId INT, TagId INT | Cả hai NOT NULL; PK ghép (NewsArticleId, TagId), đồng thời là hai FK |

| FK | ON DELETE | Hành vi |
| --- | --- | --- |
| NewsArticle.CategoryId → Category | NO ACTION | Chặn xóa Category có News |
| NewsArticle.CreatedById → SystemAccount | NO ACTION | Chặn xóa người tạo đang được News tham chiếu |
| NewsArticle.UpdatedById → SystemAccount | NO ACTION | Đã duyệt chặn hard delete người sửa gần nhất đang được tham chiếu |
| NewsTag.NewsArticleId → NewsArticle | CASCADE | Xóa News tự xóa các liên kết của chính bài |
| NewsTag.TagId → Tag | NO ACTION | Chặn xóa Tag đang được dùng; cascade phía News không xóa Tag |

Tất cả FK dùng ON UPDATE NO ACTION. Index hỗ trợ Category lookup, History theo creator/ngày, Report theo ngày, Active News theo status/ngày và kiểm tra Tag/người sửa đang được tham chiếu. Không tự thêm unique tên Category/Tag vì chưa có rule yêu cầu. Cho phép News có 0 liên kết NewsTag.

Lưu ý khi implement: TINYINT tương ứng byte trong C#; nếu dùng enum/bool phải map rõ. SQL CHECK không rỗng chỉ xử lý khoảng trắng thông thường; server vẫn phải kiểm tra whitespace/email/role/identity đầy đủ. DbContext/Service phải quản lý audit khi sửa và bảo vệ creator/ngày tạo; script không có trigger tự sửa audit. Hash format cần khớp giữa Auth và Account.

Cách dùng: mở toàn bộ file trong SSMS, kết nối instance phát triển đã chọn rồi Execute. Script dùng GO nên không gửi nguyên file như một câu lệnh EF. Nếu chưa có DB thì tạo AIVES; nếu DB đã có bất kỳ bảng người dùng nào thì dừng mà không thay đổi bảng/dữ liệu. Đây là initializer chạy một lần; nâng cấp schema dùng script riêng được review. DDL bảng nằm trong transaction; CREATE DATABASE ở ngoài transaction nên nếu khởi tạo bảng lỗi, DB mới có thể còn rỗng. Không DROP, không ALTER DB cũ, không seed Account/Admin hoặc dữ liệu mẫu. Chỉ hiện thông báo thành công khi commit schema xong.
