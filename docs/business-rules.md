# Business rules — FU News Management System

**Trạng thái:** đặc tả cần triển khai; không khẳng định code hiện đã thực hiện. Nguồn quyết định là yêu cầu Assignment và [AGENTS.md](../AGENTS.md). Những chi tiết phụ thuộc schema được đánh dấu **CHỜ SCHEMA**, không suy đoán từ file rỗng hoặc kế hoạch cũ.

## 1. Vai trò và quyền truy cập

| Vai trò | Danh tính | Được phép | Không được phép |
| --- | --- | --- | --- |
| Public | Không đăng nhập | Xem News Active | Vào trang quản lý, xem News Inactive |
| Lecturer | AccountRole = 2 | Đăng nhập, xem News Active | Tạo/sửa/xóa News, Category, Account; xem báo cáo Admin |
| Staff | AccountRole = 1 | Quản lý News và Category, xem/sửa Profile của mình, xem News do mình tạo | Quản lý Account, xem báo cáo Admin, sửa Profile người khác |
| Admin | Tài khoản mặc định lấy từ appsettings.json | Quản lý Account Staff/Lecturer, xem Report | Dùng quyền Staff chỉ vì menu hiển thị; quyền bổ sung phải được quyết định rõ |

- **BR-01 — Đăng nhập:** nhập Email và Password. Tài khoản Admin được đối chiếu với DefaultAdmin:Email/Password trong appsettings.json; Staff/Lecturer được đối chiếu với tài khoản trong DB. Đăng nhập sai không tạo trạng thái xác thực. Đăng xuất xóa trạng thái xác thực.
- **BR-02 — Phân quyền phía server:** Controller/action phải kiểm tra vai trò và danh tính thật. Ẩn menu không phải bảo vệ endpoint. Request không đăng nhập hoặc sai quyền phải bị từ chối/chuyển hướng phù hợp.
- **BR-03 — Phạm vi bản thân:** Profile và History lấy account ID từ trạng thái đăng nhập, không tin ID tùy ý do URL/form gửi. Staff không được sửa Profile hoặc xem History của người khác.
- **BR-04 — Public/Lecturer News:** chỉ trả về NewsStatus = 1 từ tầng truy vấn; NewsStatus = 0 không xuất hiện qua các endpoint này. Public truy cập được mà không cần đăng nhập.

## 2. News Article — luồng demo chính

- **BR-05 — Quản lý News:** Staff có thể list, search, create, update và delete News. Mỗi thao tác dữ liệu phải đi qua Controller → Service → Repository → DAO/DbContext.
- **BR-06 — Category của News:** Create/Update phải chọn Category tồn tại và hợp lệ theo schema/rule được chốt. Không lưu tham chiếu Category không tồn tại; dropdown được nạp từ dữ liệu thật.
- **BR-07 — Creator và thời gian:** khi tạo News, danh tính người tạo lấy từ Staff đang đăng nhập, không lấy từ trường client có thể sửa. CreatedDate/ModifiedDate được quản lý nhất quán nếu các cột này có trong schema thật; Update không được vô tình đổi creator hoặc thời điểm tạo.
- **BR-08 — Tags:** Create/Update gắn Tags theo quan hệ DB thật; không tạo liên kết lặp, bỏ Tag phải cập nhật liên kết đúng, Delete không để dữ liệu liên kết hỏng. **CHỜ SCHEMA:** tên bảng/cột, khóa và cardinality chưa được xác nhận.
- **BR-09 — Status:** NewsStatus = 1 là Active, 0 là Inactive theo yêu cầu. Staff có thể quản lý status; Public/Lecturer chỉ đọc Active. Kiểu dữ liệu chính xác của cột là **CHỜ SCHEMA**.
- **BR-10 — Search:** keyword trống/null không gây lỗi và trả danh sách hợp lý; keyword có nội dung được lọc bằng LINQ trên các trường tìm kiếm được chốt theo schema. Không gọi tên một cột chưa được xác nhận là yêu cầu bắt buộc.

## 3. Category, Account, Profile và Report

- **BR-11 — Xóa Category đang dùng:** CategoryService.DeleteCategory() kiểm tra có News tham chiếu Category hay không. Có tham chiếu thì từ chối xóa và trả lỗi nghiệp vụ; không có thì cho phép xóa. Chặn ở UI không thay thế kiểm tra ở Service. Quy tắc áp dụng dù News đang Active hay Inactive, vì yêu cầu là “đang được News sử dụng”.
- **BR-12 — Account Management:** chỉ Admin được list/search/create/update/delete tài khoản Staff và Lecturer. Role hợp lệ của tài khoản DB là 1 hoặc 2. Không hiển thị password hiện có trong UI; validation và kiểm tra email trùng phải ở backend. Ràng buộc xóa Account đang được News tham chiếu là **CHỜ SCHEMA/QUYẾT ĐỊNH**, không tự chọn hành vi.
- **BR-13 — Staff Profile:** chỉ đọc/sửa tài khoản của chính mình; form không được tự nâng role hoặc đổi ID. Dữ liệu cập nhật phải được validate ở server.
- **BR-14 — Own News History:** lọc theo creator/account ID thật của Staff hiện tại. Không trả News do Staff khác tạo.
- **BR-15 — Admin Report:** nhận StartDate và EndDate; từ chối khoảng ngày không hợp lệ; lọc News theo CreatedDate bằng LINQ và sắp xếp giảm dần. Chỉ Admin được truy cập. Quy ước múi giờ và cách tính toàn bộ ngày EndDate cần được chốt khi có schema/kiểu dữ liệu.

## 4. Quy tắc giao diện, validation và kiến trúc

- **BR-16 — Popup:** Create và Update cho News, Category, Account phải mở modal/dialog thực sự, nạp dữ liệu cần thiết và hiển thị lỗi khi input sai; một trang Create/Edit độc lập không thay thế yêu cầu này.
- **BR-17 — Xác nhận xóa:** lần nhấn Delete đầu chỉ mở xác nhận với Cancel và Confirm. Cancel không gửi thao tác xóa. Confirm mới gọi backend; backend xử lý ID thiếu/sai an toàn.
- **BR-18 — Server validation:** Login, News, Category, Account, Profile và Report phải kiểm tra dữ liệu ở server qua ViewModel/ModelState và rule nghiệp vụ. JavaScript validation chỉ hỗ trợ trải nghiệm, không quyết định dữ liệu có hợp lệ hay không.
- **BR-19 — Nguồn cấu hình:** ConnectionStrings:DefaultConnection và DefaultAdmin:Email/Password lấy từ appsettings.json/ASP.NET configuration, không hard-code C# credential hoặc connection string trong Controller/Service/Repository/DAO.
- **BR-20 — Ranh giới lớp:** Controller chỉ dùng Service interface. Service chứa/điều phối business rule và gọi Repository; Repository gọi DAO; DAO dùng EF Core/LINQ với DbContext. Không bỏ Service hoặc Repository chỉ vì EF Core đã có DbContext.
- **BR-21 — Lifetime:** bốn DAO bắt buộc có Instance Singleton thread-safe; DAO Singleton không giữ DbContext scoped trong field. DbContext không Singleton. DI đăng ký lifetime phù hợp.
- **BR-22 — Chất lượng bàn giao:** build solution và test có ý nghĩa phải pass; CI phải pass trước merge vào main. Không coi file rỗng, method placeholder hay test không có assertion là chức năng hoàn thành.

## 5. Quyết định cần chốt trước khi code Data

1. Schema SQL Server chính thức: bảng/cột, kiểu dữ liệu, PK/FK, nullability, giới hạn độ dài và quan hệ News–Tag.
2. Ràng buộc khi xóa News, Tag, Account có quan hệ dữ liệu; không tự thêm cascade hoặc hành vi từ chối khi chưa đối chiếu DB và yêu cầu.
3. Các trường News dùng cho Search và cách xử lý ngày EndDate trong Report.
4. Phiên bản .NET hợp lệ và EF Core/CI tương ứng.

Các mục chờ quyết định phải được ghi trong issue Data; không biến giả định thành Entity hoặc migration.
