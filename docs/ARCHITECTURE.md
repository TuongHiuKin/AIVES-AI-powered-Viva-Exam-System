# Kiến trúc — FU News Management System

Tài liệu này hướng dẫn cấu trúc project và ranh giới code. Quyết định nghiệp vụ/schema nằm trong [business-rules.md](business-rules.md); function, task, owner và quy trình bàn giao nằm trong [TASKS.md](TASKS.md). Đọc [AGENTS.md](../AGENTS.md) và skill kiến trúc trước khi sửa code.

## 1. Cấu trúc hiện tại và mức độ triển khai

```text
AIVESSystem.sln
AIVESSystem.slnx
StudentNameMVC/
  Controllers/
  Views/
  ViewModels/
  Models/ErrorViewModel.cs
  wwwroot/
  Program.cs
  appsettings.json
AIVES.BLL/
  Interfaces/
  Services/
AIVES.DAL/
  DependencyInjection.cs
  Entities/
  Context/AIVESDbContext.cs
  DAOs/
  Repositories/
    Interfaces/
    Implementations/
    Models/WriteCommands.cs
AIVES.Tests/
database/
  001-create-schema.sql
  002-add-system-account-soft-delete.sql
.agents/skills/
  funews-architecture/SKILL.md
  team-git-workflow/SKILL.md
docs/
  business-rules.md
  ARCHITECTURE.md
  TASKS.md
AGENTS.md
```

Cả bốn project target .NET 8, CI dùng SDK .NET 8. StudentNameMVC là tên giữ từ scaffold, chưa được xác nhận là tên sinh viên theo quy ước nộp bài. Hai solution file liệt kê cùng bốn project; dùng đường dẫn .sln rõ ràng khi chạy build/test.

Theo yêu cầu đổi tên đồng bộ, BLL/DAL/Tests và các file project tương ứng dùng tiền tố AIVES; assembly/namespace mặc định theo tên project mới. Solution là AIVESSystem.sln/.slnx, file context là AIVESDbContext.cs, database trong cấu hình và script là AIVES. Đường dẫn solution và ProjectReference đã cập nhật. StudentNameMVC giữ nguyên vì không chứa chuỗi cần đổi. Thao tác đổi tên trong workspace không đổi tên hoặc chuyển dữ liệu của database đã tồn tại trên SQL Server; cần kiểm tra DB đích trước khi chạy ứng dụng/script.

Đã triển khai Entities, mapping EF Core SQL Server (gồm IsDeleted), bốn DAO Singleton, bốn Repository/interface, write contracts và DI Data scoped. DAL có CRUD và query thực. Controller, BLL, ViewModel nghiệp vụ và hầu hết feature Razor vẫn rỗng; Auth/Service/UI chưa hoạt động. Không coi DAL hoàn thành là chức năng end-to-end đã xong.

Đã soạn [script khởi tạo SQL Server](../database/001-create-schema.sql) cho D00. Chủ dự án đã tạo DB `AIVES` trên SQL Server phát triển. Metadata 5 bảng, cột, FK, CHECK và index đã được đối chiếu bằng truy vấn chỉ đọc; mapping EF bám DB thực. Script SQL là nguồn quản lý schema theo hướng DB-first; thay đổi schema sau này dùng script riêng, không tự trộn với EF migrations.

Kiểm chứng bàn giao ngày 27/09/2026: Release build bằng SDK 8.0.425 thành công, 0 warning/error; 17/17 test pass trên runtime .NET 8.0.31 (8 model/DI/Singleton/contract, 1 đọc DB AIVES, 8 integration ghi trên DB tạm). SDK cài riêng ở `.local/dotnet`, không commit và không thay target framework. Các test Auth/Category/News scaffold cũ vẫn rỗng; chưa kiểm thử UI/Auth. Program.cs map Account/Login nhưng action chưa implement; Home/Error cũng chưa có action thực.

## 2. Ba lớp và chiều phụ thuộc

| Lớp | Nơi đặt | Trách nhiệm | Gọi trực tiếp |
| --- | --- | --- | --- |
| Presentation | StudentNameMVC | HTTP input, ModelState, bảo vệ endpoint, ViewModel, View/Redirect | Service interface |
| Business Logic | AIVES.BLL/Interfaces và Services | Use case, business validation/authorization, điều phối Repository | Repository interface |
| Data Access | AIVES.DAL | Entities, mapping, Repository contract/implementation, DAO, EF Core/LINQ | Repository → DAO → DbContext → SQL Server |

Project references hiện tại (biểu diễn theo tên thư mục):

- StudentNameMVC → AIVES.BLL và AIVES.DAL.
- AIVES.BLL → AIVES.DAL.
- AIVES.DAL không tham chiếu BLL hoặc MVC.
- AIVES.Tests → BLL và DAL.

MVC tham chiếu DAL để Program.cs cấu hình EF/DI. Controller tuyệt đối không inject DbContext, DbSet, Repository hoặc DAO, không gọi SQL. BLL không dùng DbContext và không phụ thuộc MVC. Không tạo vòng reference.

## 3. Entity, ViewModel, Repository và DAO

`SystemAccount.IsDeleted` đã có trong DB sau khi chủ dự án chạy 002; Entity/mapping và DAL soft delete đã triển khai. Không có global query filter Account: query News vẫn nạp được creator/editor đã soft delete. TV2 còn phải chặn login/phiên hiện tại, TV5 còn phải triển khai Service và UI Admin. Lưu News–NewsTag nguyên tử đã kiểm thử ở DAL, chưa có UI gọi luồng này.

**Entity** ở DAL/Entities biểu diễn dữ liệu theo thiết kế D00: SystemAccount, Category, NewsArticle, Tag, NewsTag. Quan hệ/hành vi xóa đã duyệt có mã SC-01..SC-09 trong business rules; kiểu cột/khóa/độ dài được thiết kế tại D00. Giảng viên không cung cấp script DB; nhóm tự thiết kế từ các quyết định đã duyệt, không đợi nguồn schema bên ngoài.

**ViewModel** ở StudentNameMVC/ViewModels biểu diễn form/dữ liệu hiển thị, validation và danh sách lựa chọn. Không đưa Entity vào MVC để bắt chước template. Models/ErrorViewModel.cs thuộc template và có vai trò riêng. Không dùng form binding để cho client thay role, creator hoặc audit.

**Repository interface** ở DAL/Repositories/Interfaces là contract cho BLL. Implementation ở DAL/Repositories/Implementations chuyển thao tác persistence cho DAO, không chứa UI/business rule thuộc BLL. **DAO** ở DAL/DAOs thực hiện EF Core/LINQ và lưu dữ liệu qua context; tránh nhân đôi cùng một truy vấn/CRUD trong cả hai lớp. Theo quy ước hiện tại, Repository phải gọi DAO; không tự bỏ DAO khi implement.

Bốn DAO NewsArticleDAO, CategoryDAO, SystemAccountDAO, TagDAO phải có Instance Singleton thread-safe. Singleton không giữ DbContext scoped trong field. DAO có thể nhận context theo từng thao tác/request hoặc dùng cơ chế lifetime an toàn được chốt trong issue. DbContext không Singleton; không dùng cùng context đồng thời cho nhiều tác vụ.

## 4. DI, configuration và quy tắc kỹ thuật

Program.cs gọi `AddAivesDataAccess(connectionString)` trong `AIVES.DAL/DependencyInjection.cs`. Extension đăng ký `AIVESDbContext` với `UseSqlServer` và bốn Repository interface/implementation, tất cả scoped. DAO truy cập qua `Instance` (Lazy thread-safe), nhận context qua tham số, không có context field. Không gọi `EnsureCreated` hoặc `Migrate`. Service DI bổ sung sau khi từng owner triển khai BLL. Khi triển khai:

- Giữ DbContext SQL Server từ ConnectionStrings:DefaultConnection với lifetime scoped; Service/Repository có lifetime tương thích.
- DefaultAdmin:Email/Password đọc từ appsettings/configuration; không hard-code credential trong C#.
- TV1 phụ trách DB registrations, TV2 phụ trách MVC/Auth/route; Program.cs chỉ có một người sửa tại một thời điểm theo thứ tự PR.
- DI Service bổ sung khi Service thật tồn tại; không đăng ký class giả để tuyên bố Data gate đạt.
- Dùng LINQ cho query/search/filter/sort; DbContext/mapping không rời DAL. FK/unique bảo vệ tính toàn vẹn; Service vẫn kiểm tra rule để trả lỗi nghiệp vụ rõ ràng.
- Tạo DB, chạy script, seed hay migration cần phạm vi issue/môi trường cho phép; không dùng thao tác ghi để thử kết nối chỉ đọc.

Hướng dẫn build/test và cấu hình local ở mục 8. Không cần roll-forward khi dùng SDK/runtime .NET 8.

## 5. Luồng News cần triển khai và kiểm chứng

Luồng bắt buộc: View News → NewsArticleController → INewsArticleService/NewsArticleService → INewsArticleRepository/NewsArticleRepository → NewsArticleDAO → AIVESDbContext → SQL Server. Đã có đoạn Repository → DAO → DbContext; phần View/Controller/Service còn cần TV3 triển khai. Các method thực ở cả Repository và DAO: `SearchAsync`, `GetByIdAsync`, `GetActiveAsync`, `GetByCreatorAsync`, `GetByCreatedDateRangeAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.

| Thao tác | MVC/BLL phải làm | DAL phải làm |
| --- | --- | --- |
| List/Search | Nhận keyword/bộ lọc, xác nhận Staff, gọi Service, chuẩn bị ViewModel | Query LINQ, nạp Category/Tag/creator phù hợp; keyword rỗng an toàn |
| Create | Modal/ModelState; kiểm tra tiêu đề/nội dung/Category/status, lấy creator từ identity | Lưu News và 0..n liên kết Tag nhất quán |
| Update | Nạp dữ liệu cũ; kiểm tra ID/quyền; giữ creator/ngày tạo, ghi người sửa/ngày sửa | Cập nhật đúng bài, sync NewsTag không trùng, cho phép bỏ hết Tag |
| Delete | Confirm/Cancel, kiểm tra quyền/ID ở backend | Xóa News cùng liên kết của bài; giữ Category, Account, Tag |

Category lookup do CategoryService cung cấp. Query News Active, theo creator và theo CreatedDate phải có contract dùng chung trước khi TV4/TV5 triển khai History/Public/Report. CategoryService.DeleteCategory kiểm tra mọi News tham chiếu trước khi xóa. Account/Tag delete tuân thủ business rules ở backend, không chỉ khóa nút UI.

## 6. Cách thêm chức năng

1. Đọc business-rules.md, function/task trong TASKS.md và issue được giao; xác định owner, reviewer, file và contract liên quan.
2. Chốt/reuse ViewModel, Service interface, Repository operations; schema chi tiết theo D00. Chỉ tạo thành phần cần cho chức năng.
3. Implement business rule ở BLL, persistence ở DAL; Controller chỉ điều phối trình bày và gọi Service.
4. Tạo View đúng folder Controller; Create/Update Account/Category/News có modal, Delete có confirmation; có server validation, backend authorization và antiforgery phù hợp.
5. Cập nhật DI; build, chạy test có ý nghĩa, kiểm tra thủ công UI/role/dữ liệu trên môi trường cho phép; ghi rõ phần chưa xác minh.

## 7. Kiểm tra trước khi nghiệm thu

- File đúng lớp, reference một chiều; Controller và View không truy cập DB.
- Service có business rule thật và gọi Repository; Repository gọi DAO; DAO dùng EF Core/LINQ.
- DI đúng lifetime; Singleton DAO không giữ scoped context; configuration không hard-code trong C#.
- Entity/mapping bám thiết kế do chủ dự án duyệt; quan hệ, Tags optional và chính sách xóa có test.
- News main flow xuyên suốt các lớp, auth/ownership đúng, modal/validation/confirmation hoạt động.
- Build/test pass với test có assertion; ghi manual checks và trạng thái CI trước merge. Không coi số file hoặc zero-test run là bằng chứng hoàn thành.

## 8. DAL contract bàn giao D03/D04

Nguồn chữ ký chính thức: `AIVES.DAL/Repositories/Interfaces/*.cs`. Input ghi nằm trong `Repositories/Models/WriteCommands.cs`, không phải ViewModel; Service tạo input sau khi validate/kiểm tra quyền, không bind trực tiếp từ HTTP.

| Contract | Method và cách dùng | Owner sử dụng |
| --- | --- | --- |
| ISystemAccountRepository | SearchAsync chỉ lấy chưa xóa; GetByIdAsync mặc định loại soft-deleted, includeDeleted dành cho thao tác Admin có quyền; GetByEmailAsync cho Auth; EmailExistsAsync bao gồm soft-deleted, exceptId dùng khi edit | TV2, TV5 |
| ISystemAccountRepository | CreateAsync(AccountCreate) nhận hash; UpdateAsync(AccountUpdate) cho Admin; UpdateProfileAsync(ProfileUpdate) không cho đổi role/ID; NewPasswordHash=null giữ hash cũ | TV5 cung cấp Service cho TV4 |
| ISystemAccountRepository | SoftDeleteAsync giữ liên kết; IsReferencedAsync kiểm tra creator **hoặc** editor; HardDeleteAsync có kết quả InUse/NotFound/Deleted | TV5, phối hợp TV2 khóa phiên |
| ICategoryRepository | SearchAsync dùng cho list/search/lookup; GetByIdAsync; HasNewsAsync; CreateAsync; UpdateAsync; DeleteAsync | TV4; CategoryService cung cấp lookup cho TV3 |
| INewsArticleRepository | SearchAsync title/content, GetActiveAsync lọc status=1, GetByIdAsync(activeOnly:true) cho public detail; GetByCreatorAsync cho History | TV3 cung cấp Service query cho TV4 |
| INewsArticleRepository | GetByCreatedDateRangeAsync(startUtc,endExclusiveUtc): UTC, đầu bao gồm/cuối loại trừ; sort CreatedDate DESC rồi NewsArticleId DESC | TV3 cung cấp Service query cho TV5 |
| INewsArticleRepository | CreateAsync(NewsCreate), UpdateAsync(NewsUpdate), DeleteAsync: audit+NewsTag cùng một SaveChangesAsync; TagIds bắt buộc khác null nhưng có thể rỗng, ID trùng tự dedupe | TV3 |
| ITagRepository | GetAllAsync, GetByIdAsync, GetByIdsAsync (trả các ID tồn tại, Service so sánh tập ID để phát hiện thiếu), IsUsedAsync, DeleteAsync | TV3 qua Service; chưa có màn hình Tag CRUD |

Quy ước kết quả/lỗi:

- Query read dùng AsNoTracking; News nạp Category, CreatedBy, UpdatedBy, NewsTags.Tag. Entity Account có hash nhạy cảm: Service luôn map sang DTO/ViewModel chỉ gồm trường cần hiển thị, không trả nguyên Entity ra JSON/View form/log.
- Create trả ID do DB sinh. Update và News Delete trả false nếu không có ID; Account Update/Profile cũng false khi đã soft delete. SoftDelete trả true nếu bản ghi tồn tại (kể cả đã xóa), false nếu không tồn tại.
- Category/Tag/Account hard delete trả enum `DeleteResult`. Service vẫn phải kiểm tra business rule trước khi gọi; DAL/FK bảo vệ bổ sung và xử lý race FK bằng InUse. Không có quyền Admin/Staff ở DAL; Service và endpoint phải bổ sung.
- News Create không nhận ngày tạo/editor từ client: DB cấp UTC CreatedDate. Update chỉ nhận editor và ModifiedUtc từ Service; creator/ngày tạo được giữ nguyên. SQL datetime2 đọc về DateTime.Kind=Unspecified nhưng giá trị lưu là UTC; Service đánh dấu UTC trước khi chuyển sang giờ Việt Nam, không coi đó là giờ local máy.
- Report từ chối Kind khác UTC hoặc start>=end bằng ArgumentException trước query. BLL chịu trách nhiệm parse ngày Việt Nam, required/range/overflow. Các query null/whitespace keyword không lọc keyword; role/status/creator filters vẫn giữ nguyên.
- DB sai FK/CHECK/unique báo DbUpdateException, không giả success. BLL kiểm tra trước và chuyển lỗi cạnh tranh thành thông báo nghiệp vụ; không đưa raw SQL message ra UI. Cancellation truyền nguyên xuống EF.
- Mỗi write là một thao tác lưu hoàn chỉnh; không stage thay đổi ngoài Repository, không gọi song song trên cùng scoped context, không trộn nhiều use case vào một context rồi mong rollback chung. Sau lỗi SaveChanges, ChangeTracker được clear và lỗi được ném lại. Nếu tương lai cần nhiều lần lưu chung transaction, yêu cầu TV1 bổ sung contract riêng.
- Hiện chưa có optimistic concurrency token; edit đồng thời cùng bài dùng trạng thái lưu sau. Không tự thêm cột/schema để thay đổi chính sách này.

### Chạy trên máy thành viên

1. Cài SDK .NET 8 và SQL Server. DB mới chạy 001 rồi 002 trong SSMS; DB đã có schema 001 chỉ chạy 002. Không chạy migration hoặc initializer lại trên DB có dữ liệu.
2. Cấu hình `ConnectionStrings:DefaultConnection` cho instance local qua ASP.NET configuration (có thể dùng environment `ConnectionStrings__DefaultConnection`). Không commit SQL password/cấu hình máy cá nhân. Cấu hình hiện dùng Windows Authentication, DB AIVES; không có mật khẩu SQL.
3. `dotnet restore AIVESSystem.sln`, `dotnet build AIVESSystem.sln --configuration Release`, `dotnet test AIVESSystem.sln --configuration Release --no-build`. Mặc định chạy 8 test không cần DB; 9 test SQL opt-in bị skip rõ ràng.
4. Trên máy TV1 đang có SDK riêng: thay `dotnet` bằng `& ./.local/dotnet/dotnet.exe`. `.local` bị gitignore, không được đóng gói/bàn giao SDK như source.

Đọc DB thật (không ghi):

```powershell
$env:AIVES_TEST_CONNECTION_STRING = (Get-Content -Raw StudentNameMVC/appsettings.json | ConvertFrom-Json).ConnectionStrings.DefaultConnection
dotnet test AIVESSystem.sln --configuration Release --filter 'Category=SqlServerReadOnly'
```

Integration ghi dữ liệu, chỉ chạy khi được phép tạo/xóa **DB tạm** trên instance test/development:

```powershell
$env:AIVES_TEST_SERVER_CONNECTION_STRING = (Get-Content -Raw StudentNameMVC/appsettings.json | ConvertFrom-Json).ConnectionStrings.DefaultConnection
$env:AIVES_RUN_DATABASE_WRITE_TESTS = '1'
dotnet test AIVESSystem.sln --configuration Release --filter 'Category=SqlServerWrite'
Remove-Item Env:AIVES_RUN_DATABASE_WRITE_TESTS, Env:AIVES_TEST_SERVER_CONNECTION_STRING
```

Fixture luôn thay tên DB bằng `AIVES_DAL_Test_<GUID>`, tự tạo, chạy bản sao script 001/002 với đích đã thay, kiểm thử rồi xóa đúng DB do nó tạo. Không ghi DB AIVES dù connection đầu vào mang tên đó. Login SQL cần quyền CREATE/DROP DATABASE trên instance test. Test kiểm tra rollback, soft/hard delete, FK, NewsTag, search/filter/report, missing ID và chạy lại 002 giữ cờ IsDeleted. Không dùng thông số opt-in trên production. CI mặc định chỉ chạy model/DI tests; kết quả CI remote và manual UI chưa được xác nhận bởi lần chạy local này.
