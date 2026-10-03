# Đối chiếu code và chia issue cho 5 thành viên

Ngày: **01/10/2026**. Mục đích: dùng làm đầu vào giao việc cho AGY, dựa trên các yêu cầu sửa Admin, đăng nhập, báo cáo sinh viên và trang giảng viên.

## 1. Snapshot và cách sử dụng

- Workspace: `D:\School\AIVES_AI_powered_Viva_Exam_System`.
- Nhánh đã đọc: `kien/local-integration`.
- Commit: `4ab6e06286b9dd39b6a4d97596e7a95d0a694619`.
- Working tree sạch trước khi tạo tài liệu này. Chỉ đối chiếu source local; chưa fetch hay xác minh nhánh của các thành viên.
- Đây là review tĩnh và kế hoạch giao việc. Chưa sửa implementation, chạy ứng dụng, build/test, kiểm tra DB hay gửi email. Các kết luận UI cần kiểm tra trình duyệt khi triển khai.
- Các mã `FIX-01`…`FIX-05` là mã nội bộ trong tài liệu, **chưa phải issue đã tạo trên GitHub**. TV1…TV5 là vị trí đề xuất; điền tên thật và đối chiếu ownership hiện có trước khi giao.
- Giữ [business-rules.md](business-rules.md), [ARCHITECTURE.md](ARCHITECTURE.md), [G6-IMPLEMENTATION-HANDOFF.md](G6-IMPLEMENTATION-HANDOFF.md), [CODE-REVIEW-CHECKLIST.md](CODE-REVIEW-CHECKLIST.md) làm nguồn nghiệp vụ và giới hạn kỹ thuật. Tài liệu này bổ sung đợt sửa hiện tại, không thay toàn bộ [TASKS.md](TASKS.md).

### Cách hiểu dùng để lập task

1. “Sắp xếp ID theo thứ tự”: mặc định **AccountId tăng dần theo số**, giữ nguyên ID trong DB. Không đánh lại khóa chính để lấp khoảng trống sau xóa; nếu cần STT liên tục thì đó là cột riêng.
2. “Xóa phần đăng nhập trong trang đăng nhập”: tạm hiểu là ẩn **liên kết Đăng nhập trên navbar** khi đang ở `Account/Login`. Giữ form và nút submit Đăng nhập.
3. “Khóa tài khoản, xóa tài khoản”: tách rõ hai hành động và hậu quả. Code hiện dùng `IsDeleted` cho khóa và `HardDelete` cho xóa vĩnh viễn; giữ cách này làm cơ sở đề xuất, cần chốt semantics trước khi thay model. Mở khóa là mở rộng tùy chọn, không tự thêm vào tiêu chí bắt buộc.
4. “Lớp học”: chưa rõ lớp hành chính hay lớp học phần; phải chốt nguồn/mapping, cách hiển thị nếu nhiều lớp. Không lấy lớp từ một lượt thi bất kỳ.
5. “Tiêu chú rubric” được hiểu là **tiêu chí rubric**. Đánh giá chi tiết gồm nhận xét AI, điểm mạnh/điểm yếu và điểm/kết quả có nguồn rõ.

## 2. Kết quả đối chiếu yêu cầu với code

Các đường dẫn và số dòng dưới đây thuộc snapshot trên; khi code đổi, tìm lại theo method/tên trường.

| Yêu cầu | Hiện trạng có bằng chứng | Phần cần làm | Issue |
| --- | --- | --- | --- |
| Sắp xếp ID | `AIVES.DAL/DAOs/SystemAccountDAO.cs:20-26`, `SearchAsync`: `OrderBy(AccountName).ThenBy(AccountId)` | Đổi thứ tự chính thành ID tăng dần; bảo toàn ID | FIX-01 |
| Lọc theo vai trò | `AccountController.Index` dòng 137 chỉ nhận `keyword`; `AccountIndexViewModel` dòng 18 chỉ có `Keyword`; view Account/Index dòng 72 chỉ có ô tìm tên/email | Thêm role filter xuyên UI → BLL → DAL, kết hợp keyword | FIX-01 |
| Cột MSSV/lớp | `AIVES.DAL/Entities/SystemAccount.cs` và `AccountItemViewModel` chưa có MSSV/lớp; DbContext chỉ có các entity tài khoản/News. `AttemptSnapshotDto` có StudentId/ClassId nhưng thuộc báo cáo mock | Chốt liên kết Account ↔ Student ↔ lớp và nguồn thật, rồi bổ sung read DTO/cột | FIX-01 |
| Khóa tài khoản | `AccountController.Delete` dòng 302 gọi `SoftDeleteAccountAsync`; DAO dòng 82 đặt `IsDeleted=true`. Nhưng `SearchAsync` dòng 22 loại hết `IsDeleted`, nên tài khoản đã khóa biến mất khỏi danh sách, dù view có nhãn “Đã khóa” | Sửa query quản trị và trải nghiệm trạng thái; kiểm chứng chặn login/cookie | FIX-02, phối hợp FIX-01 |
| Xóa tài khoản | Đã có `HardDelete`, modal và nút “Xóa cứng”. DAO `IsReferencedAsync` dòng 44 chỉ kiểm tra NewsArticles, trong khi modal nói cả dữ liệu kỳ thi | Sửa nhãn/thông báo, xác minh ràng buộc đúng nguồn dữ liệu, chặn xóa có tham chiếu ở backend | FIX-02 |
| Quên mật khẩu | Không có link tại `Views/Account/Login.cshtml`; tìm source BLL/DAL/MVC/tests không thấy Forgot/ResetPassword, reset token hay dịch vụ gửi mail | Tạo luồng khôi phục hoàn chỉnh sau khi chốt token/email/persistence | FIX-03 |
| Email hoặc mật khẩu sai | `AccountController.cs:57` **đã có đúng** “Email hoặc mật khẩu không chính xác.”; Login view dòng 17 có validation summary | Test và sửa cách render nếu tái hiện lỗi; không tạo thông báo thứ hai | FIX-03 |
| Đăng nhập bị lặp | `_Layout.cshtml:65-73` render link Đăng nhập cho mọi trang anonymous; Login có nút submit riêng | Ẩn riêng link navbar khi ở trang Login | FIX-03 |
| Thứ tự báo cáo SV | `Views/StudentReports/Index.cshtml:189-266`: câu hỏi → ghi chú GV → transcript → điểm mạnh/yếu → nhận xét AI → rubric. Badge điểm còn nằm trước nội dung câu hỏi | Đổi thành câu hỏi → câu trả lời → ghi chú GV → đánh giá chi tiết → rubric | FIX-04 |
| Màu sắc báo cáo | View dùng nhiều màu badge, `text-warning` trên nền trắng, phần nhận xét/rubric là `small text-muted` | Giảm màu trang trí, tăng khả năng đọc; chưa đo contrast hay xem UI thực tế | FIX-04 |
| GV bấm xem chi tiết điểm SV | `Views/ClassReports/Index.cshtml:255-285` chỉ render hàng điểm; `ClassReportsController` chỉ có Index và ExportGradeSheet | Thêm nút/route/service chi tiết theo SV/lớp/đợt/lượt, kiểm tra quyền | FIX-05 |

### Phụ thuộc quan trọng phát hiện thêm

- **Dữ liệu báo cáo là mock:** `StudentNameMVC/Program.cs:38` đăng ký `MockExamReportRepository` trực tiếp; repository tự tạo EXAM/SV/GV/lớp. Không dùng các mã mẫu đó làm MSSV hoặc quan hệ tài khoản thật. Chưa có adapter SQL báo cáo trong snapshot đã đọc.
- **Danh tính đang chưa nối đúng:** `StudentReportsController.Index` nhận `studentId`, mặc định SV01 và view cho chuyển SV01/SV02/SV03. `StudentReportService` không nhận trusted actor. `ClassReportsController` nhận `teacherId`, mặc định GV01; service kiểm tra assignment bằng chính giá trị được truyền. Có kiểm tra role nhưng chưa thấy ràng buộc các mã này với principal.
- **Role có mâu thuẫn:** UI gọi role 1 là Sinh viên; `ApplicationRoles.FromDatabaseRole` vẫn map 1 → Staff, entity ghi 1=Staff, 2=Lecturer. Đổi nhãn không chứng minh đã có contract Student. Không tự đổi mã role hoặc coi Staff là Student được duyệt.
- **Boundary:** hai controller báo cáo đang inject `IExamReportRepository` để lấy lựa chọn. Khi sửa luồng liên quan, chuyển việc lấy dữ liệu qua BLL service theo kiến trúc đã quy định.
- **Tính điểm cần đối chiếu khi dùng lại:** báo cáo cá nhân làm tròn trung bình 2 chữ số, bảng lớp làm tròn 1; bảng lớp chỉ có cột lượt 1/2. View SV chưa tách rõ mode Đạt/Không đạt và chỉ xử lý một số trạng thái. Đây là điểm kiểm tra cho FIX-04/05, không phải quyền viết lại toàn bộ engine chấm điểm.

Các điểm trên là bằng chứng source của snapshot, chưa phải kết quả khai thác/thử HTTP. Quyền xem dữ liệu là điều kiện nghiệm thu tích hợp, không chỉ là việc ẩn menu.

## 3. Phân công 5 người

| Người đề xuất | Issue chính / tiêu đề dùng tạo issue | Reviewer đề xuất | Khối lượng tương đối | Đầu ra |
| --- | --- | --- | --- | --- |
| **Cao Thắng** (`@caothang1504`) | [#16: FIX-01 Admin — sắp ID, lọc vai trò, hiển thị MSSV và lớp học](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/16) | Mai Trung (`@mtrungggg`) | Lớn nếu thiếu nguồn hồ sơ | Danh sách đúng thứ tự/bộ lọc và contract dữ liệu hồ sơ |
| **Mai Trung** (`@mtrungggg`) | [#17: FIX-02 Admin — hoàn thiện thao tác khóa và xóa tài khoản](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/17) | Lê Quang Hào (`@QHao2508`) | Vừa–lớn | Trạng thái khóa nhìn thấy, thao tác rõ ràng, bảo vệ ràng buộc |
| **Lê Quang Hào** (`@QHao2508`) | [#18: FIX-03 Đăng nhập — quên mật khẩu, thông báo lỗi và bỏ link trùng](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/18) | Mai Trung (`@mtrungggg`) | Lớn | Luồng khôi phục, login UX; đầu mối Auth/identity contract |
| **Lê Văn Khang** (`@Khangla12`) | [#19: FIX-04 Sinh viên — sắp lại báo cáo và cải thiện khả năng đọc](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/19) | Tưởng Hiếu Kiên (`@TuongHiuKin`) | Vừa, tăng khi nối quyền | Giao diện báo cáo đúng thứ tự, component chi tiết dùng chung |
| **Tưởng Hiếu Kiên** (`@TuongHiuKin`) | [#20: FIX-05 Giảng viên — xem chi tiết điểm từng sinh viên](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/20) | Lê Văn Khang (`@Khangla12`) | Lớn | Luồng drill-down có scope, dùng lại component và dữ liệu kết quả |

Không dồn toàn bộ test cho một người: mỗi owner viết/chạy test của issue, reviewer kiểm tra chéo. FIX-03 chia mốc UI và backend khôi phục để không giữ các thay đổi nhỏ chờ hạ tầng email.

## 4. Nội dung từng issue để giao AGY

### FIX-01 — Admin: sắp ID, lọc vai trò, thêm MSSV/lớp

**Owner:** Cao Thắng (@caothang1504). **Reviewer:** Mai Trung (@mtrungggg). **GitHub Issue:** [#16](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/16). **Ưu tiên:** P1. **Liên quan kế hoạch cũ:** G6-00/01/02 (nguồn danh tính, chỉ phần phục vụ issue này).

**Phạm vi file:**

- `StudentNameMVC/Views/Account/Index.cshtml`: phần filter, header/cell dữ liệu, empty state.
- `StudentNameMVC/ViewModels/SystemAccountViewModel.cs`: chỉ index/item và các trường đọc mới.
- `StudentNameMVC/Controllers/AccountController.cs`: chỉ action Index.
- `AIVES.BLL/Interfaces/ISystemAccountService.cs`, `Services/SystemAccountService.cs`: phần tìm kiếm/danh sách.
- `AIVES.DAL/Repositories/Interfaces/ISystemAccountRepository.cs`, `Repositories/Implementations/SystemAccountRepository.cs`, `DAOs/SystemAccountDAO.cs`: phần query danh sách.
- Test danh sách/tìm kiếm; entity/DbContext/schema chỉ có kế hoạch mapping trước, sửa khi có phạm vi được duyệt.

**Checklist triển khai:**

- [ ] Chốt read contract tài khoản gồm ID, tên, email, role, trạng thái, MSSV nullable, lớp theo nguồn được xác nhận; một tài khoản vẫn một hàng khi có nhiều lớp.
- [ ] Đặt thứ tự mặc định `AccountId ASC` tại query; không sort dạng chuỗi hay cập nhật ID.
- [ ] Thêm lựa chọn “Tất cả vai trò” và các vai trò hợp lệ theo contract; kết hợp bằng AND với keyword; giữ lựa chọn sau submit, có xóa bộ lọc. Không thêm Admin cấu hình vào danh sách DB bằng dữ liệu giả.
- [ ] Validate role ở server. Làm rõ thẻ thống kê là số trong kết quả lọc hay tổng hệ thống; đề xuất dùng kết quả lọc và ghi nhãn tương ứng với logic hiện có.
- [ ] Thêm MSSV và lớp; thiếu dữ liệu hiện “Chưa cập nhật”, role không áp dụng hiện “—”. Không suy MSSV từ AccountId/email, không nối hồ sơ bằng tên.
- [ ] Nhận yêu cầu TV2 để query **quản trị** trả cả tài khoản đã khóa, trong khi lookup đăng nhập vẫn chỉ lấy tài khoản hoạt động. Không mở rộng mọi query hiện có một cách mù quáng.

**Tiêu chí nghiệm thu:**

- [ ] Dữ liệu ID 2, 10, 21 hiển thị đúng 2 → 10 → 21, dù tên có thứ tự khác; không cần ID liên tục.
- [ ] Lọc từng role, role + keyword, xóa lọc, không có kết quả và role không hợp lệ đều có kết quả xác định.
- [ ] MSSV/lớp đúng mapping đã chốt; ca thiếu hồ sơ, nhiều lớp, giảng viên không có MSSV không gây lỗi/trùng hàng.
- [ ] Test BLL và query phù hợp; không coi fake trả sẵn danh sách đã sort là bằng chứng LINQ/SQL đã đúng.

**Phụ thuộc:** role/identity contract với TV3; semantics khóa với TV2; chủ nguồn dữ liệu xác nhận Account ↔ Student ↔ lớp. Có thể hoàn thành sort/filter và layout trước; chưa có nguồn thật thì phần MSSV/lớp ghi PARTIAL, không đóng issue như đã tích hợp.

### FIX-02 — Admin: khóa và xóa tài khoản

**Owner:** Mai Trung (@mtrungggg). **Reviewer:** Lê Quang Hào (@QHao2508). **GitHub Issue:** [#17](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/17). **Ưu tiên:** P1, nâng P0 nếu kiểm thử phát hiện vượt quyền/xóa mất dữ liệu.

**Phạm vi file:** AccountController vùng Delete/HardDelete và modal; `Views/Account/_DeleteConfirmPartial.cshtml`, `_HardDeleteConfirmPartial.cshtml`; phần action/JS trong Account/Index; service/repository/DAO các method khóa/xóa/tham chiếu. Phần query Search do TV1 sở hữu. Auth/cookie do TV3 sở hữu; TV2 bổ sung test hành vi phối hợp.

**Checklist triển khai:**

- [ ] Phân biệt nhãn “Khóa tài khoản” và “Xóa tài khoản”; modal xóa nêu rõ xóa vĩnh viễn. Tránh nhãn người dùng “Xóa cứng”, “Xóa mềm”, “Hard Delete”. Giữ thao tác Sửa hiện có.
- [ ] Khóa chỉ vô hiệu hóa truy cập, giữ bản ghi và liên kết. Sau khóa, quản trị vẫn thấy hàng “Đã khóa”, nút khóa không còn bấm lặp; thống nhất trạng thái nút Sửa cho tài khoản khóa với khả năng backend hiện có.
- [ ] Giữ POST + antiforgery + Admin authorization. Kiểm tra quyền/ID ở backend cho cả gọi trực tiếp, không dựa vào disabled button.
- [ ] Giữ kiểm tra tham chiếu News đang có. Bổ sung tham chiếu hồ sơ/lớp/kết quả chỉ khi nguồn/mapping thật đã xác nhận; xử lý cả lỗi ràng buộc phát sinh giữa kiểm tra và xóa.
- [ ] Thông báo đúng điều đã kiểm tra. Khi dữ liệu thi chưa nối, không khẳng định “chưa có dữ liệu nào tham chiếu” hoặc “đã bảo toàn toàn bộ lịch sử thi” như đã xác minh.
- [ ] Giữ filter khi reload sau thao tác; xử lý không tìm thấy, tài khoản đã khóa, có tham chiếu và lỗi tải modal mà không để nút loading mắc kẹt.

**Tiêu chí nghiệm thu:**

- [ ] Khóa tài khoản đang hoạt động: bản ghi còn, trạng thái đúng, đăng nhập mới bị chặn; cookie đã đăng nhập bị từ chối ở request được bảo vệ tiếp theo. Code đã có `ActiveAccountCookieEvents`, kiểm chứng và dùng lại.
- [ ] Xóa tài khoản không tham chiếu thành công trong môi trường test được phép; có tham chiếu trả lỗi rõ và không mất dữ liệu. Không chạy thử xóa trên DB thật mặc định.
- [ ] Người không phải Admin và POST thiếu token không thực hiện được thao tác; GET không thay trạng thái.
- [ ] Không tác động Admin cấu hình như một tài khoản DB, không bổ sung mở khóa hay đổi schema ngoài scope được giao.

**Phụ thuộc:** TV1 cập nhật query danh sách; TV3 xác minh login/cookie; nguồn dữ liệu thi quyết định mức kiểm chứng ràng buộc ngoài News. Hoàn thiện UI/unit test được trước, bằng chứng SQL/HTTP ghi riêng.

### FIX-03 — Đăng nhập: quên mật khẩu, lỗi đăng nhập, link navbar

**Owner:** Lê Quang Hào (@QHao2508). **Reviewer:** Mai Trung (@mtrungggg). **GitHub Issue:** [#18](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/18). **Ưu tiên:** P1. **Liên quan:** G6-02 cho contract danh tính, không ôm việc sửa cả hai màn hình báo cáo.

**Phạm vi file:** `Views/Account/Login.cshtml`, `Views/Shared/_Layout.cshtml`, ViewModel Login và ViewModel/View khôi phục mới; vùng Authentication của AccountController hoặc controller khôi phục riêng; `IAuthService`, `AuthService`, security/Auth tests. TV3 là đầu mối tích hợp `Program.cs` và claims/policy khi đã được giao. Token store/email adapter/persistence mới cần thiết kế và owner/phạm vi xác nhận trước.

**Mốc A — UI và lỗi đăng nhập:**

- [ ] Giữ đúng câu đã có “Email hoặc mật khẩu không chính xác.” cho sai email/sai mật khẩu; kiểm tra ModelState và validation summary để lỗi hiện rõ, không nhân đôi. Lỗi định dạng/bỏ trống vẫn theo field.
- [ ] Chỉ ẩn link Đăng nhập trên navbar ở trang Login; giữ submit, Enter để gửi form, RememberMe và ReturnUrl nội bộ.
- [ ] Thêm link “Quên mật khẩu?” đến trang khôi phục có hướng dẫn/trạng thái thật; không đặt link chết hoặc chỉ báo gửi mail thành công giả.

**Mốc B — khôi phục mật khẩu:**

- [ ] Chốt cách khôi phục và nhà cung cấp email. Đề xuất: email chứa link token một lần, thời hạn được cấu hình, ràng buộc đúng account/mục đích; nhóm xác nhận trước khi dùng production.
- [ ] Dùng cơ chế token chuẩn được duyệt; không lưu token dạng thô, không cho đặt lại chỉ bằng email/AccountId, không gửi mật khẩu cũ/mới qua mail. Không tự cài package.
- [ ] Request khôi phục phản hồi chung cho email có/không tồn tại, có giới hạn tần suất; reset kiểm tra token hợp lệ, hết hạn, đã dùng, bị sửa và tài khoản khóa/xóa. Chốt cách vô hiệu phiên cũ sau reset và thực thi theo contract Auth.
- [ ] Hash mật khẩu mới bằng cơ chế hiện có, cập nhật và tiêu thụ token an toàn để tránh dùng lại đồng thời; token gửi lại có chính sách hủy rõ ràng. Không log token/password.
- [ ] Credentials cấu hình ngoài source. Admin hiện nằm trong `DefaultAdminOptions`, không có bản ghi DB: xác định rõ luồng này áp dụng tài khoản DB; khôi phục Admin là quyết định riêng, không ghi đè appsettings bằng form.

**Đầu nối sớm cho TV1/TV4/TV5:** cùng chủ nguồn dữ liệu chốt principal AccountId ↔ StudentId/TeacherId, role ngữ nghĩa và quyền lớp/học phần. TV3 sở hữu adapter identity; TV4/TV5 sở hữu kiểm tra scope tại use case báo cáo. Thiếu mapping thì từ chối truy cập tương ứng, không fallback SV01/GV01 hoặc tự đổi Staff thành Student.

**Tiêu chí nghiệm thu:**

- [ ] Login sai email, sai password, account khóa đều không đăng nhập được và không tiết lộ email có tồn tại; login đúng/RememberMe/ReturnUrl/logout không bị hồi quy.
- [ ] Navbar không có link Đăng nhập trùng ở Login; form vẫn hoạt động qua bàn phím/mobile.
- [ ] Token hợp lệ đặt lại thành công, mật khẩu cũ thất bại, mật khẩu mới thành công; token hết hạn/sai/đã dùng và account bị khóa/xóa không reset được.
- [ ] Có test rate limit, response không lộ tài khoản, replay/cạnh tranh dùng token, email adapter lỗi và chính sách phiên cũ.
- [ ] Mock email chỉ trong test/harness; gửi mail thực tế phải kiểm chứng ở môi trường được phép. Chưa có email/token store thì mốc B còn blocked, không báo toàn issue Done.

**Phụ thuộc:** lựa chọn email, contract reset và phạm vi persistence/Auth được duyệt. Mốc A và adapter identity có thể bàn giao trước backend khôi phục.

### FIX-04 — Sinh viên: thứ tự và màu sắc báo cáo

**Owner:** Lê Văn Khang (@Khangla12). **Reviewer:** Tưởng Hiếu Kiên (@TuongHiuKin). **GitHub Issue:** [#19](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/19). **Ưu tiên:** P1 cho UI; quyền sở hữu dữ liệu là điều kiện trước nghiệm thu tích hợp. **Liên quan:** G6-06/07.

**Phạm vi file:** `Views/StudentReports/Index.cshtml`, `StudentReportsController.cs`, `StudentReportService.cs`; ViewModel/partial báo cáo và CSS riêng dự kiến tạo mới. TV4 sở hữu component chi tiết dùng chung, phối hợp TV5 khi mở rộng contract trong `ExamReportingModels.cs`/`IExamReportingServices.cs`. Không thay CSS toàn site nếu CSS riêng đủ đáp ứng.

**Checklist triển khai:**

- [ ] Mỗi câu có thứ tự đọc/DOM: **(1) Câu hỏi → (2) Câu trả lời thí sinh → (3) Ghi chú giảng viên → (4) Đánh giá chi tiết → (5) Tiêu chí rubric**. Điểm/badge đánh giá đặt trong khối đánh giá, không đứng trước nội dung câu hỏi.
- [ ] Giữ câu hỏi AI làm rõ và câu trả lời tiếp theo bên trong phần hội thoại/câu trả lời, theo đúng trình tự; không bỏ transcript hiện có.
- [ ] Phần đánh giá gom nhận xét AI, điểm mạnh/yếu, điểm AI đề xuất và điểm GV chốt với nhãn nguồn rõ. Không lấy điểm AI giả thành điểm GV hay suy chưa chốt thành 0.
- [ ] Thiếu ghi chú/rubric/nhận xét thể hiện trạng thái thiếu dữ liệu rõ; không tạo nội dung mẫu trên production. Dữ liệu theo đúng câu/lượt.
- [ ] Dùng nền sáng, chữ tối dễ đọc, màu nhấn nhất quán; trạng thái có cả chữ/icon, không chỉ màu. Kiểm tra contrast và focus bằng công cụ trình duyệt; ảnh trước/sau desktop/mobile là bằng chứng UI.
- [ ] Tách partial/component hiển thị chi tiết để TV5 dùng lại, không để component gọi service/DB hoặc tự tính điểm. Truyền dữ liệu đã được backend kiểm tra quyền.
- [ ] Nhận actor từ adapter của TV3, chặn SV đọc người khác ở service; bỏ chuyển hồ sơ/mã mặc định khỏi luồng thật. Lookup danh sách kỳ thi đi qua service và đúng scope.
- [ ] Hiển thị mode Score/PassFail và Pending/Failed/Abandoned/Cancelled theo contract, không tự biến thiếu kết quả thành 0/không đạt. Đối chiếu chỗ phải sửa liên quan với DEC-05/10/12, báo riêng nếu cần mở rộng engine ngoài issue.

**Tiêu chí nghiệm thu:**

- [ ] Hai SV, nhiều lượt và nhiều câu đều đúng thứ tự năm khối; nội dung dài/thiếu ghi chú không vỡ layout ở màn hình nhỏ và zoom 200%.
- [ ] Giữ đúng câu hỏi/câu trả lời/ghi chú/đánh giá/rubric gốc; không mất follow-up và không lẫn dữ liệu giữa các lượt.
- [ ] Score hiện điểm kèm thang; PassFail hiện kết quả đúng mode; pending/lỗi chấm hiện trạng thái, không báo 0 như điểm đã chốt.
- [ ] SV A đổi studentId/attemptId không xem được B; anonymous bị chặn; role chưa có mapping không được cấp quyền ngầm. Direct controller test không thay bằng chứng middleware HTTP.
- [ ] Có component contract bàn giao cho TV5, ảnh UI và test phần logic/quyền đã sửa. Không viết test chỉ kiểm tra màu CSS giống implementation.

**Phụ thuộc:** TV3 cung cấp identity mapper; TV1/chủ nguồn cung cấp mapping hồ sơ; nguồn kết quả hiện mock phải công khai. UI có thể kiểm tra bằng fixture riêng, nhưng không coi đó là báo cáo SQL thật đã hoàn tất.

### FIX-05 — Giảng viên: bấm xem chi tiết điểm sinh viên

**Owner:** Tưởng Hiếu Kiên (@TuongHiuKin). **Reviewer:** Lê Văn Khang (@Khangla12). **GitHub Issue:** [#20](https://github.com/TuongHiuKin/AIVES-AI-powered-Viva-Exam-System/issues/20). **Ưu tiên:** P1; scope quyền là điều kiện trước nghiệm thu tích hợp. **Liên quan:** G6-02/06/09.

**Phạm vi file:** `Views/ClassReports/Index.cshtml`, `ClassReportsController.cs`, `ClassReportService.cs`; action/ViewModel/View chi tiết mới; service/read contract dùng chung và tests dưới ExamReporting. Dùng component TV4, không đồng thời sửa ruột component khi chưa thống nhất.

**Checklist triển khai:**

- [ ] Thêm nút “Xem chi tiết” trên từng hàng sinh viên. Đề xuất trang chi tiết riêng với nút quay lại giữ lớp, đợt thi và bộ lọc lượt; modal chỉ thay thế nếu nhóm chọn.
- [ ] Endpoint nhận mã sinh viên/lớp/đợt và tùy chọn lượt để chọn dữ liệu; danh tính GV phải lấy từ principal đã map, không tin `teacherId` từ URL/hidden field.
- [ ] BLL kiểm tra GV được phân công lớp/học phần/đợt, SV thuộc roster/phạm vi đó, lượt thuộc đúng SV và đợt trước khi trả nội dung. Không chỉ kiểm tra “lớp có tồn tại”.
- [ ] Áp dụng cùng danh tính/scope cho Index, chi tiết và ExportGradeSheet để không còn cửa truy cập bằng cách thay teacherId. Cho controller lấy lookup qua service.
- [ ] Chi tiết có thông tin SV/lớp/đợt, tổng hợp và danh sách lượt; mở được từng lượt để xem điểm/kết quả từng câu, câu trả lời, ghi chú, đánh giá/rubric qua component TV4. Hỗ trợ số lượt thực tế, không giới hạn cứng 2 lượt trong chi tiết.
- [ ] Thống nhất nguồn điểm và precision hiển thị với TV4; không viết công thức trung bình/rubric mới trong view hoặc tính lại khác ở endpoint chi tiết. Nêu rõ điểm thô/thang chuẩn hóa nếu có.
- [ ] Không mở rộng quyền StudentReportsController bằng cách thêm Lecturer rồi truyền studentId tự do. Dùng use case GV có scope riêng trước khi reuse phần đọc/tính/hiển thị.
- [ ] ID sai/không tồn tại/ngoài phạm vi và chưa thi/chờ chấm/lỗi nguồn có trạng thái phù hợp, không trả success rỗng để che lỗi. Không giả định chỉ dựa trên attempt là đã có roster đầy đủ.

**Tiêu chí nghiệm thu:**

- [ ] Từ lớp được giao, bấm SV A/B mở đúng người, đúng đợt/lượt; quay lại giữ bộ lọc; trên mobile nút vẫn dùng được.
- [ ] Thay teacherId/classId/studentId/examId/attemptId không đọc được ngoài quyền; SV và anonymous không gọi được endpoint GV. Chính sách Admin theo contract riêng, không mặc nhiên cấp mọi lớp.
- [ ] Điểm bảng lớp, trang chi tiết và phần dùng chung khớp theo cùng quy tắc/thang/rounding được chốt; có ca trên 2 lượt, pending và mode PassFail.
- [ ] Có test BLL scope/quan hệ dữ liệu, HTTP authorization và UI smoke; label mock rõ trong harness, không tuyên bố AI/SQL thật.

**Phụ thuộc:** identity contract TV3, nguồn roster/assignment được xác nhận, component/đọc điểm TV4. Có thể xây luồng và unit test bằng fixture riêng trước; tích hợp thật chờ adapter dữ liệu đã duyệt.

## 5. Điều phối file chung và thứ tự triển khai

| Điểm dễ conflict | Owner điều phối đề xuất | Quy tắc bàn giao |
| --- | --- | --- |
| AccountController | TV3 | TV1 chỉ Index; TV2 chỉ khóa/xóa; TV3 chỉ Auth. Tích hợp từng vùng, không ghi đè toàn file |
| Account/Index + Account ViewModels | TV1 | TV2 gửi phần action/JS/modal và trường trạng thái; TV1 ghép vào layout/filter |
| SystemAccount service/repository/DAO/interface | TV1 | TV2 bàn giao thay đổi mutation; TV3 dùng contract reset/identity tách biệt nếu phù hợp. Không sửa signature chung âm thầm |
| Entities/DbContext/schema | TV1 làm đầu mối với chủ dữ liệu | Chốt mapping và quyền thay đổi trước; không mặc định TV1 được chạy SQL |
| Auth/claims/policy/Program/_Layout | TV3 | Nhận yêu cầu DI/quyền từ TV2/4/5; chỉ ghép trong scope được giao, không bypass auth |
| Exam DTO/interfaces và nguồn điểm dùng chung | TV4 | TV5 đề xuất contract chi tiết/roster; chốt cùng trước khi hai bên code |
| Component/CSS chi tiết báo cáo | TV4 | TV5 gọi component đã thống nhất; CSS riêng phạm vi báo cáo |

1. **Chốt đầu nối:** tên 5 người; role/identity; nguồn MSSV/lớp/roster/assignment; semantics khóa/xóa; email/token; owner file chung. Các phương án cần schema/Auth mới ghi rõ phạm vi xin giao, không tự coi đã duyệt vì có trong kế hoạch.
2. **Làm phần độc lập:** TV1 sort/filter và layout; TV2 modal/hành vi khóa/xóa; TV3 login UI và thiết kế reset/identity; TV4 layout/component; TV5 route/service contract và scope tests. Chỉ chạy song song ở vùng file đã tách, không có 5 tác nhân cùng ghi một file.
3. **Bàn giao sớm:** TV1/TV2 chốt query quản trị; TV3 bàn giao identity trước; TV4 bàn giao component trước; TV5 nối bảng lớp → chi tiết.
4. **Ghép và kiểm tra:** không đóng các phần phụ thuộc dữ liệu thật/Auth/email bằng kết quả mock. Không cần đợi toàn bộ chức năng thi/AI ngoài các issue này để kiểm thử phần đã giao.

### Các quyết định còn mở cần ghi trong issue trước khi sửa phần phụ thuộc

| Quyết định | Người đề xuất giải quyết | Có thể làm trước |
| --- | --- | --- |
| Account ↔ Student/Teacher, ý nghĩa role Staff hiện có | TV3 + TV1 + chủ dự án | Login UI, layout, unit test contract; không cấp quyền Student suy đoán |
| MSSV lấy ở đâu, lớp hành chính/học phần, nhiều lớp | TV1 + chủ dữ liệu | Sort/filter và cột có empty state; chưa xác nhận dữ liệu thật |
| Khóa dùng IsDeleted hay trạng thái riêng; xóa vật lý có được dùng với dữ liệu thi | TV2 + TV1 | Sửa nhãn/modal/test; chưa đổi schema hoặc mở rộng quyền xóa |
| Email provider, reset token store/thời hạn, phiên cũ, Admin cấu hình | TV3 + chủ dự án | Login UX và thiết kế luồng; không giả gửi mail |
| Assignment/roster và adapter kết quả thật | TV5 + TV1/TV3 | Service tests bằng fixture; không mở quyền production theo fixture |

## 6. Kiểm thử và bàn giao

Mỗi issue ghi: snapshot, file sửa, yêu cầu đã đáp ứng, test đã chạy và kết quả, phần mock/thật, blocker, ảnh UI khi có thay đổi hiển thị. Trạng thái dùng: Done / Partial / Blocked / Needs manual test, kèm lý do; phần chưa kiểm chứng không đánh Done.

- Sau sửa code: build Release với SDK/package hiện có; chạy unit tests đúng module trước, các test hồi quy liên quan sau khi ghép.
- Lệnh mẫu build không restore: `dotnet build AIVESSystem.sln --configuration Release --no-restore`. Nếu thiếu assets/SDK thì ghi blocker; không tự cài thêm hoặc đổi package/framework.
- Lệnh unit test mẫu sau khi đã kiểm tra fixture/setup: `dotnet test AIVES.Tests/AIVES.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName~SystemAccountServiceTests|FullyQualifiedName~AuthServiceTests|FullyQualifiedName~AuthenticationPresentationTests|FullyQualifiedName~AIVES.Tests.ExamReporting"`. Bổ sung tên lớp test mới thực tế, kiểm tra số test > 0; đây là hướng dẫn cho lần triển khai, không phải lệnh đã chạy trong review này.
- Đọc test setup và môi trường trước khi chạy. Không tự bật `AIVES.Tests/Data` có ghi DB, migration, seed, chạy SQL scripts hoặc thao tác trên DB thật.
- Test cookie/Authorize/antiforgery cần bằng chứng HTTP hoặc smoke được phép. Gọi action trực tiếp hoặc kiểm tra attribute chỉ chứng minh một phần.
- Kiểm tra UI: login đúng/sai/quên mật khẩu; Admin lọc/khóa/xóa; SV đọc báo cáo; GV vào chi tiết/quay lại; ca không dữ liệu và sai quyền.
- Giữ MVC → BLL → DAL → Repository/DAO → DbContext cho dữ liệu thật; không dùng mock production hay hard-code identity để làm test pass.

**Kết luận snapshot:** nhiều phần UI/CRUD đã có, nhưng các yêu cầu mới chưa được đáp ứng đầy đủ. Báo cáo đang nối mock và chưa ràng buộc mã SV/GV với principal nên chưa thể xác nhận sẵn sàng tích hợp thật. Đây không phải kết luận các nhánh teammate chưa làm; chỉ mô tả checkout đã đọc.

## 7. Prompt mẫu dùng với AGY

Copy mẫu dưới đây, thay mã issue và tên owner; giao từng issue với phạm vi rõ. Tài liệu không tự cấp quyền publish Git/issue hay chạy DB.

```text
Triển khai issue [FIX-01 / FIX-02 / FIX-03 / FIX-04 / FIX-05] trong file
docs/AGY-ISSUES-5-MEMBERS-2026-10-01.md.
Owner thực tế: [tên]. Reviewer: [tên].
Phạm vi được giao: [các vùng file của issue, đầu nối/schema/Auth đã được chốt nếu có].
Quyết định đã chốt và dependency đã có: [điền nguồn/contract/nhánh/commit].

Trước khi sửa, đọc AGENTS.md, skill funews-architecture, business-rules,
ARCHITECTURE, G6-IMPLEMENTATION-HANDOFF và nội dung issue. Xác minh branch,
commit, git status và code hiện tại; số dòng trong tài liệu chỉ là snapshot.
Bảo toàn thay đổi teammate. Tận dụng code đã có, không viết lại chỉ vì tài liệu
dùng tên đề xuất. Không tự làm cả 5 issue hoặc các task G6 ngoài phạm vi.

Thực hiện checklist và tiêu chí nghiệm thu của issue được giao. Phối hợp owner
file chung trước khi đổi contract; tiếp tục phần độc lập nếu còn dependency.
Thiếu nguồn dữ liệu hoặc quyết định schema/Auth thì ghi rõ phần phụ thuộc,
không bịa MSSV/lớp, tự map Staff thành Student, hard-code identity hoặc đăng ký
mock vào production. Không xóa module News, đổi stack/package hay chạy DB scripts.

Build/test an toàn phần sửa, kiểm tra UI và quyền khi có điều kiện. Báo cáo
file sửa, test pass/fail/skip, phần thật/mock, tiêu chí còn thiếu và blocker.
Không tự commit/push/PR/merge/tạo issue. Nếu sau đó được yêu cầu Git, đọc skill
team-git-workflow; nhánh theo <member-name>/<feature-name> với tên người thật.
```
