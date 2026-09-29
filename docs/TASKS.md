# Tasks — Ưu tiên nhóm 6: Phản hồi & báo cáo thi vấn đáp

Cập nhật 29/09/2026 sau khi chủ dự án duyệt quy tắc lượt thi, hai chế độ đánh giá và bốn điểm làm rõ. Đây là **kế hoạch triển khai/bàn giao**, không phải báo cáo code đã hoàn thành hay danh sách issue đã được tạo.

## 1. Mục tiêu, phạm vi và nguồn quy tắc

Main flow ưu tiên:
- Sinh viên xem báo cáo từng lượt thi và kết quả tổng hợp của mình.
- Giảng viên xem thống kê lớp/học phần được phân công: câu trả lời tốt, câu khó, phân bố điểm và biểu đồ.
- Chức năng phụ cung cấp danh tính, cấu hình và kết quả cho luồng này. Những nhóm chức năng khác tiếp tục phát triển sau, không bắt buộc xong hết để test nhóm 6.

Nguồn nghiệp vụ mới: [business-rules.md](business-rules.md), DEC-01..DEC-12. Phạm vi file, contract và cấu hình demo đề xuất tại [G6-IMPLEMENTATION-HANDOFF.md](G6-IMPLEMENTATION-HANDOFF.md). [Checklist](CODE-REVIEW-CHECKLIST.md) quy định test, mock và cách đánh giá công bằng. Không nhân bản công thức khác ở từng task.

### Tài liệu cũ và ranh giới thay đổi

AGENTS.md, skill funews-architecture, business-rules.md và ARCHITECTURE.md đã được đồng bộ hướng nhóm 6 ở bước chuẩn bị tài liệu. Nội dung News cũ được giữ trong docs/archive chỉ để bảo trì/tra cứu; không tự dùng News/Category/Tag làm dữ liệu thi. Việc đồng bộ tài liệu không chứng minh source/DB đã chuyển đổi.

Giữ nền kỹ thuật hiện hành MVC .NET 8 → BLL → DAL, Repository → DAO → scoped DbContext → SQL Server; Controller không gọi DB. Không tự đổi stack, rewrite schema, role code hoặc xóa module News. Đọc skill kiến trúc để bảo toàn kỹ thuật/an toàn; yêu cầu nghiệp vụ mới của người dùng được thể hiện bằng DEC.

Bảng task News trước đây không còn là kế hoạch main flow nhóm 6; có thể tra bản trước trong Git history khi cần đối chiếu. Không tái sử dụng mã NEWS/CAT/R01 cũ cho nghiệp vụ khác, không tự đóng/sửa các issue News hiện có. Kết quả test DAL News trước đây không chứng minh báo cáo thi đã đạt.

## 2. Những điều đã chốt để triển khai

| Chủ đề | Quy tắc cần dùng | Nguồn |
| --- | --- | --- |
| Giới hạn lượt | Chỉ hoàn thành mới tính lượt; bỏ dở/hủy riêng; đủ số lượt không mở mới; mỗi lượt giữ kết quả riêng | DEC-01 |
| Chấm điểm | GV đặt rubric, điểm tối đa, ngưỡng đạt; AI chấm theo cấu hình | DEC-02/03 |
| Đạt/Không đạt | GV đặt tiêu chí câu và tỷ lệ câu đạt tối thiểu/điều kiện bắt buộc cho cả lượt | DEC-02/04 |
| Báo cáo cá nhân | Chi tiết từng lượt; tổng điểm lượt và trung bình các lượt có kết quả đầy đủ; không chia số lượt tối đa | DEC-05/10 |
| Biểu đồ lớp | X = điểm trung bình của sinh viên, Y = số sinh viên; không đếm mỗi lượt là một người | DEC-06 |
| Thống kê câu | Theo lượt được chọn; gộp lượt đổi đơn vị thành lượt trả lời; mẫu số chỉ gồm kết quả hợp lệ | DEC-07/08 |
| Độ khó | Tỷ lệ không đạt cao nhất, giữ tất cả đồng hạng; không dùng số sai tuyệt đối hoặc điểm trung bình | DEC-09 |
| Chưa có điểm | Hoàn thành thi khác hoàn thành chấm; không biến pending thành 0/sai, mẫu số 0 chưa kết luận | DEC-10 |
| Quyền dữ liệu | SV chỉ xem mình; GV chỉ lớp/học phần được phân công | DEC-11 |
| Khác thang/settings | Chuẩn hóa trước trung bình khi cần; giữ cấu hình dùng ở từng lượt | DEC-12 |

Chi tiết như giá trị threshold từng đợt, thang chuẩn hóa, precision/làm tròn, khoảng histogram phải được ghi trong contract/config test trước nghiệm thu; không tự coi fixture là mặc định nghiệp vụ.

## 3. Trạng thái bàn giao và owner

Tài liệu chưa xác minh ai đang có code nhóm 6 ở nhánh nào. Mặc định các task bên dưới có trạng thái **CHƯA XÁC MINH IMPLEMENTATION**, không phải “chưa ai làm”. Chủ dự án đối chiếu teammate/issue để điền:

| Task | Owner thực tế | Nhánh/commit/issue | Đã có | Đang làm/chưa push | Dependency | Ảnh hưởng demo |
| --- | --- | --- | --- | --- | --- | --- |
| Điền mã G6 bên dưới | Chưa gán | Chưa xác minh | Bằng chứng | Nguồn xác nhận | Thật/mock/chưa có | Chặn/có mock/không ảnh hưởng |

Không tự gán lại TV3/TV4/TV5 theo bảng News cũ. Mỗi task có một owner chính và reviewer, còn các điểm nối cần hai bên xác nhận.

## 4. Danh sách task và quan hệ phụ thuộc

Các mã G6 là task của kế hoạch mới, không phải issue GitHub đã tạo.

| ID | Công việc | Đầu ra chính | Phụ thuộc |
| --- | --- | --- | --- |
| G6-00 | Chốt contract và snapshot triển khai | DTO/interface, settings, scope, owner/nhánh, chi tiết còn mở | DEC đã duyệt |
| G6-01 | Dữ liệu và đọc kết quả | Liên kết người–lớp–đợt–lượt–câu–đánh giá; query có scope | G6-00 |
| G6-02 | Auth và phạm vi truy cập | Danh tính SV/GV, bảo vệ báo cáo và lớp | G6-00 |
| G6-03 | Cấu hình đánh giá | Số lượt, hai chế độ, rubric/ngưỡng/tiêu chí, phiên bản dùng cho lượt | G6-00; G6-01/02 để tích hợp |
| G6-04 | Vòng đời và giới hạn lượt | Đang làm/hoàn thành/bỏ dở, đếm đúng, chống đếm lặp | G6-01/02/03 |
| G6-05 | Tiếp nhận kết quả đánh giá | Điểm/kết quả đạt/nhận xét đúng câu/lượt; pending/error rõ | G6-00/01/03; G6-04 khi tích hợp |
| G6-06 | Báo cáo cá nhân từng lượt | Danh sách lượt, điểm/đạt/nhận xét từng câu, quyền sở hữu | G6-01/02/05 |
| G6-07 | Tổng hợp kết quả cá nhân | Tổng lượt, trung bình điểm; kết quả Đạt/Không đạt đúng settings | G6-03/05/06 |
| G6-08 | Thống kê câu hỏi | Tỷ lệ đạt/không đạt, tốt nhất/khó nhất, scope và đơn vị đúng | G6-01/02/05 |
| G6-09 | Bảng điểm lớp và biểu đồ | Điểm từng lượt/trung bình mỗi SV, phân bố và biểu đồ cột | G6-07/08; G6-02 |
| G6-10 | Fixtures và tests độc lập | Bộ mẫu tính tay, fake contract, test quyền/công thức/ca biên | G6-00; bổ sung cùng G6-01..09 |
| G6-11 | Tích hợp và kịch bản demo | Startup/DI/routes, hai vai trò, UI thật, danh sách thật/mock | G6-06..10 và dependencies cần demo |
| G6-12 | Đồng bộ hướng dẫn agent/tài liệu | Bản docs/rules đã chuẩn bị; xác minh áp dụng đúng snapshot và thay đổi sau tích hợp | Review tài liệu/contract |

Không phải thứ tự làm tuần tự toàn bộ: sau G6-00, nhóm có thể làm báo cáo/tính toán trên fake contract trong khi data/auth đang phát triển. G6-10 bắt đầu sớm, không chờ UI xong. Không mock toàn bộ G6-06..09 rồi tuyên bố main flow đã hoạt động.

## 5. Đặc tả task và tiêu chí nghiệm thu

### G6-00 — Contract và kế hoạch tích hợp

- Inventory code/nhánh đã có, xác định phần thật và phần teammate đang làm; không mặc định checkout hiện tại là toàn bộ công việc nhóm.
- Chốt thông tin cần truyền: định danh SV/GV/lớp/đợt/lượt/câu, rubric/settings, điểm tối đa, điểm/kết quả đạt, nhận xét, trạng thái thi và chấm.
- Phân biệt dữ liệu input với kết quả tính toán, đơn vị sinh viên với lượt trả lời.
- Ghi semantics filter chọn lượt và gộp lượt; nếu câu hỏi/rubric thay đổi, contract phải phân biệt phiên bản để không trộn sai.
- Chốt chi tiết kỹ thuật còn mở ở checklist; không invent tên bảng/class như thể đã có.
- Done: có contract để bên nguồn và bên báo cáo cùng sử dụng, mỗi dependency có owner/nhánh hoặc ghi chưa xác định.

### G6-01 — Data và persistence/read contract

- Đối chiếu model/schema thật, xác định thiếu gì so với G6-00; schema mới/chuyển đổi cần review và quyền riêng, không tự sửa DB AIVES.
- Query báo cáo giữ đúng identity/lớp/đợt/lượt/câu và điểm/nhận xét tương ứng.
- Không dùng model News làm kết quả thi bằng cách đổi nhãn hiển thị.
- Phân biệt trạng thái chưa chấm, giữ lịch sử từng lượt/settings; không ghi đè lượt trước khi thi lại.
- Done: query/DTO có test mapping, missing ID, scope và duplicate JOIN. Test mock không thay bằng chứng SQL; nếu chưa có DAL thật, ghi chờ tích hợp.

### G6-02 — Auth và authorization

- SV chỉ xem kết quả bản thân; GV chỉ xem lớp/học phần được phân công; kiểm tra cả role lẫn quyền dữ liệu.
- ID principal không lấy từ hidden field để cấp quyền; request đổi ID/filter không vượt scope.
- Tận dụng authentication hiện có nếu phù hợp; không tự map Staff=Sinh viên hoặc đổi CHECK role.
- Done: test đúng/sai vai trò, SV A đọc B, GV đọc lớp không được giao, anonymous, logout. Unit principal mock phải ghi giới hạn; HTTP test riêng cho middleware.

### G6-03 — Settings của giảng viên

- Cấu hình số lượt tối đa, chế độ chấm; chế độ điểm có rubric/điểm tối đa/ngưỡng đạt; chế độ đạt có tiêu chí và tỷ lệ tối thiểu/điều kiện bắt buộc cho toàn lượt.
- Validation phù hợp đơn vị: số lượt nguyên dương; điểm tối đa dương; threshold trong thang/đơn vị đã quy định; tỷ lệ trong khoảng hợp lệ.
- Chỉ GV có quyền phạm vi tương ứng mới thay settings. Lượt cũ giữ cấu hình đã dùng.
- Không bắt phải hoàn thành UI quản trị settings toàn hệ thống để test Report; fixture settings có cấu trúc được dùng trong test, còn UI thực thiếu phải ghi rõ.
- Done: cùng input/settings cho kết quả nhất quán; thiếu hoặc sai settings không tự rơi về ngưỡng hard-code.

### G6-04 — Lượt thi

- Hiển thị số hoàn thành/tối đa; từ chối bắt đầu mới khi đủ lượt.
- Bỏ dở/hủy không tăng số hoàn thành. Chấm chưa xong không làm lượt đã hoàn thành biến thành chưa thi.
- Số đếm lấy từ trạng thái lượt hoặc được cập nhật nhất quán với lịch sử, không biến độc lập dễ sai.
- Request hoàn thành lặp không tăng hai lần; kiểm tra cạnh tranh mở/hoàn thành nhiều request để không vượt giới hạn. Cách xử lý nhiều lượt đang mở phải ghi trong contract, không tự thêm chính sách chưa thống nhất.
- Done: có test đủ/chưa đủ lượt, bỏ dở, hoàn thành lặp và chờ chấm.

### G6-05 — Nguồn kết quả chấm

- Nhận dữ liệu đúng câu/lượt/người cùng rubric đã dùng; điểm không vượt giới hạn, kết quả/nhận xét có nguồn rõ.
- AI thực thi chuẩn GV; không tự đưa ra chuẩn đạt khác. Lượt review không yêu cầu hoàn thành AI/thu âm nếu chỉ đang kiểm thử báo cáo bằng đầu vào giả.
- Có trạng thái pending/failed/đã có kết quả theo contract, không bịa điểm hoặc nhận xét khi chấm lỗi.
- Chế độ điểm phân loại đạt bằng threshold GV; chế độ đạt dùng tiêu chí GV.
- Done: contract dùng được với fixture và adapter thật; không nhầm dữ liệu mẫu là kết quả AI thật.

### G6-06 — Báo cáo cá nhân từng lượt

- Đầu vào: SV xác thực và lượt được chọn; truy vấn đúng ownership.
- Hiển thị hoàn thành/tối đa, danh sách lượt; mỗi lượt có câu hỏi/câu trả lời, điểm kèm thang hoặc Đạt/Không đạt, nhận xét AI.
- Chưa chấm/mất ID/không có dữ liệu có trạng thái rõ; không để tất cả dòng dùng cùng nhận xét.
- Done: hai SV, nhiều lượt/câu có dữ liệu khác nhau vẫn đúng mapping; đổi ID không đọc được người khác; UI gọi backend thật.

### G6-07 — Tổng hợp cá nhân

- Chế độ điểm: tổng lượt = tổng điểm câu; trung bình các lượt đã hoàn thành có kết quả đầy đủ; nếu khác thang, chuẩn hóa trước.
- Lượt pending không coi là 0: báo “đã hoàn thành X lượt; trung bình dựa trên Y lượt có điểm đầy đủ”.
- Không chia số lượt tối đa, không chọn điểm cao nhất thay trung bình.
- Chế độ Đạt/Không đạt: kết luận mỗi lượt theo tỷ lệ câu đạt tối thiểu và tiêu chí bắt buộc; báo tỷ lệ có nhãn/mẫu số rõ. Không tự đổi thành điểm trung bình.
- Done: đối chiếu phép tính tay, chưa thi/đang chấm, khác thang, sát threshold và tiêu chí bắt buộc không đạt.

### G6-08 — Thống kê câu

- Đầu vào: GV, phạm vi lớp/đợt, lượt được chọn; có thể hỗ trợ chế độ gộp lượt với nhãn riêng.
- Một lượt được chọn: mỗi SV tối đa một kết quả hợp lệ trên câu; chưa trả lời/chưa chấm không thành sai.
- Tính tỷ lệ đạt/không đạt theo DEC-07; gộp lượt theo DEC-08, không lẫn mẫu số.
- Tốt nhất theo tỷ lệ đạt; khó nhất theo tỷ lệ không đạt cao nhất; giữ các câu đồng hạng; mẫu số 0 không xếp hạng.
- Done: test nhóm có kích thước khác nhau để phát hiện đếm số sai tuyệt đối; test duplicate JOIN, các ngưỡng, mode score/pass-fail và scope khác lớp.

### G6-09 — Bảng điểm lớp và biểu đồ

- Dùng kết quả G6-07 cho mỗi SV: mỗi người một điểm trung bình khi đủ dữ liệu.
- Biểu đồ cột/histogram: X=điểm trung bình, Y=số SV; bảng và biểu đồ cùng scope/thang/rounding/bin rules.
- Có bảng từng lượt để đối chiếu; nếu có lựa chọn biểu đồ từng lượt thì ghi nhãn rõ, không tráo với biểu đồ trung bình.
- Không trộn mode Đạt/Không đạt vào biểu đồ điểm; nếu thêm biểu đồ tỷ lệ phải chốt riêng trục/đơn vị, không suy ra là yêu cầu bắt buộc mới.
- Done: số SV trong các cột khớp số SV đủ điều kiện; biên khoảng không đếm đôi; empty state; thay fixture/filter làm bảng và biểu đồ cập nhật.

### G6-10 — Bộ test và mock

- Dùng bộ mẫu ở checklist mục 9; test độc lập kết quả tính tay, hai lớp, nhiều SV, nhiều lượt, pending, bỏ dở, hai chế độ chấm.
- Mock nguồn kết quả/dependency chưa push theo interface, giữ nguyên Service/Controller đang review.
- Không mock logic thống kê để đánh PASS logic đó. Không coi direct action test là authorization HTTP, không coi compile View là UI đã chạy.
- Done: ghi lệnh/snapshot, pass/fail/skip, giới hạn evidence và test cần chạy lại sau tích hợp. SQL write/seed cần quyền riêng, mặc định không chạy.

### G6-11 — Demo tích hợp

- Ghép registrations/routes/layout có chủ đích, không chọn toàn một phía rồi mất logic teammate.
- Kịch bản SV: đăng nhập → lượt hoàn thành/tối đa → chi tiết từng lượt → tổng hợp trung bình/đạt theo chế độ.
- Kịch bản GV: scope lớp → thống kê câu theo lượt → câu khó/tốt → bảng điểm lớp/biểu đồ trung bình.
- Kiểm tra request sai quyền, chưa có dữ liệu, pending và thay dữ liệu đầu vào để chứng minh tính toán thật.
- Done: build/test + browser smoke, danh sách phần thật/mock và giới hạn đã công khai. Mock được phép trong test không có nghĩa giảng viên chấp nhận demo; xác nhận riêng.
- Không đợi mọi module ngoài nhóm 6 xong. Kết luận sẵn sàng thật / demo giới hạn với mock được chấp nhận / chưa sẵn sàng.

### G6-12 — Đồng bộ tài liệu/agent rules

- Đã chuẩn bị AGENTS/skill/business-rules/architecture/checklist/tasks và handoff theo mục tiêu nhóm 6; News cũ có bản lưu.
- Bước này chỉ sửa tài liệu. Chưa implement, chưa xác nhận contract được teammate nhận, chưa gán owner hoặc duyệt production settings/schema.
- Trước khi AGY làm code, kiểm tra snapshot có đúng tài liệu mới và người dùng đã giao package/file scope chưa.
- Sau tích hợp cập nhật mapping/implementation status từ bằng chứng, không đưa code chưa có vào sơ đồ như đã hoạt động.
- Giữ quy tắc Git/an toàn/kiến trúc. Nếu tiếp tục sửa skill thì dùng skill-creator và validate.

## 6. Điểm phải phối hợp, đề xuất nhóm 5 người

Đây là phương án chia nhóm trách nhiệm để chủ dự án gán người thực tế, **chưa thay phân công đã có trên GitHub**.

| Nhóm phụ trách đề xuất | Task chính | Bàn giao/phối hợp |
| --- | --- | --- |
| Data/contract | G6-00/01 | Chốt cấu trúc với mọi bên, persistence và nguồn fixture |
| Auth/settings/lượt thi | G6-02/03/04 | Phối hợp Data và nguồn chấm; có thể cần chia nhỏ vì khối lượng lớn |
| Kết quả và báo cáo cá nhân | G6-05/06/07 | Nhận contract chấm, cung cấp tổng/trung bình cho thống kê |
| Thống kê và biểu đồ | G6-08/09 | Dùng cùng scope/settings với cá nhân; đối chiếu công thức |
| Tích hợp/demo | Điều phối G6-10/11/12 | Mỗi người vẫn viết test module của mình; không dồn toàn bộ test cho người thứ 5 |

Chủ dự án gán người theo code họ đang làm, không bắt teammate bỏ công việc hiện có khi chưa đối chiếu. Program/Auth/shared layout/DTO và schema cần owner rõ, không sửa song song không phối hợp.

## 7. Thứ tự ưu tiên và cách dùng mock

1. G6-00: inventory và contract; xác nhận branch/owner, giá trị settings demo và các chi tiết precision/normalization còn mở.
2. Làm song song nguồn dữ liệu/Auth/settings với báo cáo cá nhân/thống kê trên fixture hợp đồng.
3. Nối điểm/nhận xét → từng lượt → trung bình → thống kê câu → bảng/biểu đồ; giữ rõ bộ lọc một lượt so với nhiều lượt.
4. G6-10 chạy cùng quá trình, không chờ cuối; G6-11 kiểm thử HTTP/UI của phần đã nối.
5. Đồng bộ hướng dẫn agent; các module ngoài nhóm 6/AI trực tiếp tiếp tục sau nếu chưa thuộc demo, ghi rõ trạng thái.

Thiếu teammate code không tự là FAIL cá nhân. Nhưng thiếu chính báo cáo/thống kê/UI thì mock input không làm phần đó thành hoàn thành. Đánh giá ảnh hưởng demo độc lập với trạng thái task.

## 8. Tiêu chí Done và báo cáo bàn giao

Mỗi task cần:
- Owner/reviewer, nhánh/commit, phạm vi file, dependency, phần thật/mock và nguồn xác nhận phần đang làm.
- Trace UI → Controller → Service → Repository → DAO → DB, hoặc chỉ rõ boundary bị mock.
- Server validation/authorization đúng, tests có assertion, kết quả build và manual UI liên quan.
- Bằng chứng đủ cho tiêu chí của task, không dùng số file hoặc zero-test run làm kết luận.
- Vấn đề còn lại, ảnh hưởng demo, test phải chạy sau khi teammate push.
- Không tự commit/push/merge hoặc ghi DB ngoài yêu cầu; giữ nguyên teammate changes.

Dùng status PASS/PARTIAL/FAIL/NOT IMPLEMENTED/NEEDS MANUAL TEST/NOT APPLICABLE theo checklist. Build/test pass trên module News cũ không chuyển thành PASS cho nhóm 6. Tài liệu này không tự tạo issue, không xác nhận phân công mới đã được các thành viên nhận.
