# Checklist review — Nhóm 6: Phản hồi & báo cáo

Cập nhật ngày 29/09/2026 theo định hướng mới nhất của chủ dự án. Đây là tiêu chí cho AGY review code đang phát triển, **không phải xác nhận hệ thống đã hoàn thành**, cũng không phải lệnh triển khai hay thay đổi DB.

## 1. Mục tiêu hiện tại và nguồn yêu cầu

Chủ dự án xác nhận:
- Hệ thống sẽ tiếp tục bổ sung các nhóm chức năng khác.
- Main flow cần ưu tiên demo trước là **nhóm 6 — Phản hồi & báo cáo**.
- Các chức năng phụ đang xây dựng nhằm cung cấp đầu vào và quyền truy cập cho nhóm 6.
- Một số phần đồng đội đang hoàn thiện/chưa push; không đánh đồng thiếu code tích hợp với sai hướng kiến trúc hoặc lỗi cá nhân.

Nội dung nhóm 6 trong ảnh người dùng cung cấp:
1. Sinh viên xem lại báo cáo sau khi thi: điểm từng câu, nhận xét của AI.
2. Giảng viên xem thống kê toàn lớp: câu hỏi khó nhất, tỷ lệ trả lời tốt, phân bố điểm, vẽ biểu đồ ma trận/tương đương.

Checklist lấy hai yêu cầu này và xác nhận mới của người dùng làm mục tiêu review nghiệp vụ. Không tự mở rộng thành yêu cầu hoàn thành toàn hệ thống thi trước khi kiểm tra nhóm 6.

### Nguồn hiện hành và lịch sử

- [business-rules.md](business-rules.md): nguồn duy nhất của DEC-01..DEC-12 đã duyệt.
- [ARCHITECTURE.md](ARCHITECTURE.md): nền kỹ thuật, hiện trạng code và hướng tích hợp.
- [TASKS.md](TASKS.md): các task G6, phụ thuộc và ownership chưa được gán lại.
- [G6-IMPLEMENTATION-HANDOFF.md](G6-IMPLEMENTATION-HANDOFF.md): phương án package/file scope, contract và cấu hình fixture đề xuất; không phải quyền tự implement.
- [AGENTS.md](../AGENTS.md) và [funews-architecture](../.agents/skills/funews-architecture/SKILL.md) đã đồng bộ mục tiêu nhóm 6; giữ tên skill để tương thích.
- `docs/archive/` lưu nội dung News cũ, không còn là hướng dẫn nghiệp vụ nhóm 6. Bảo toàn module News, không tự xóa/rewrite.
- Không tự coi contract đề xuất là schema đã có, fixture là cấu hình GV đã duyệt, hoặc đồng đội đã nhận task chưa gán.
- Yêu cầu hiện tại là định hình trước; prompt review không cấp quyền triển khai. Package A/B/C chỉ được làm khi có yêu cầu và phạm vi tương ứng.

## 2. Đánh giá lại những tiêu chí cũ lệch định hướng

| Tiêu chí trong checklist cũ | Xử lý cho review nhóm 6 | Lý do |
| --- | --- | --- |
| News Article Management là main flow | Thay bằng báo cáo cá nhân và thống kê lớp | Đây là mục tiêu demo mới đã xác nhận |
| Staff quản lý News, Lecturer chỉ đọc Active | Không áp; kiểm tra Sinh viên/Giảng viên và quyền dữ liệu thi | Không tự đồng nhất Staff với Sinh viên hay gán mã role 1/2 |
| Report chỉ dành cho Admin, lọc News.CreatedDate | Không dùng làm tiêu chí nhóm 6 | Giảng viên cần thống kê lớp; sinh viên cần kết quả cá nhân |
| News/Category/Tag/NewsTag và SC-01..SC-11 | Ngoài tiêu chí nghiệm thu nhóm 6, trừ khi chứng minh có quan hệ thật với luồng này | Schema tin tức không mặc nhiên là schema kết quả thi |
| Kiểm tra TagId, Category đang dùng, News Active/Inactive | Không phải điều kiện đạt demo báo cáo thi | Là rule của module khác |
| Bắt buộc popup Create/Update News, Account, Category | Không áp làm cổng nghiệm thu nhóm 6 | Ảnh yêu cầu nhóm 6 không quy định các popup này |
| Phải hoàn thành mọi module trước khi đánh giá main flow | Bỏ | Cho phép review từng phần và dùng mock dependency còn thiếu |
| Báo cáo phải dùng ngày Việt Nam/News.CreatedDate theo BR-15 | Không chuyển nguyên rule sang thi | Phải xác định ngày/lần thi/phạm vi báo cáo theo contract mới |
| Hash mật khẩu, server validation, authorization, không lộ dữ liệu | Giữ trong phần chức năng phụ thực sự được dùng | Đây là yêu cầu an toàn và kiểm soát truy cập |
| MVC → Service → Repository → DAO → DbContext | Giữ làm nền kỹ thuật hiện hành, không tự đổi stack | Đổi mục tiêu nghiệp vụ không tự cho phép bỏ ranh giới lớp |
| Mock pass nghĩa là hệ thống đã hoàn thành | Cấm | Chỉ chứng minh đúng phần code thật được thực thi |

Không xóa hay đánh hỏng module News chỉ vì ngoài mục tiêu này. Ghi riêng **“chưa liên quan đến main flow nhóm 6”**; không tính module đó là đã đáp ứng báo cáo thi.

## 3. Phạm vi và nguyên tắc review công bằng

### Trạng thái mỗi tiêu chí

| Status | Ý nghĩa |
| --- | --- |
| PASS | Có bằng chứng đạt trong phạm vi đã ghi rõ: static/mock/HTTP/UI/SQL |
| PARTIAL | Có phần đúng nhưng chưa nối đủ hoặc còn điều kiện chưa đáp ứng |
| FAIL | Code đã có nhưng hành vi sai yêu cầu; có bằng chứng/tái hiện |
| NOT IMPLEMENTED | Chưa có implementation trong snapshot review; ghi owner, tình trạng đang làm/chưa push/chưa rõ |
| NEEDS MANUAL TEST | Chưa xác minh được hành vi cần chạy thực tế |
| NOT APPLICABLE | Ngoài phạm vi demo hoặc phần đang review; nêu lý do |

Tách thêm cột **Phụ thuộc**: đã tích hợp / đang làm / chưa push / chưa rõ. “Đang làm” phải có nguồn xác nhận, không tự giả định mọi file rỗng là đồng đội đã làm.

Tách thêm cột **Ảnh hưởng demo**: chặn demo / có thể thử bằng mock / không ảnh hưởng. Thiếu dependency không phải lỗi cá nhân nhưng vẫn có thể chặn demo ngày mai.

### Mức nghiêm trọng của lỗi

- BLOCKING: code/phần tích hợp hiện có làm main flow không chạy, build fail do source hoặc vi phạm nền kỹ thuật bắt buộc.
- HIGH: sai quyền, lộ báo cáo người khác, sai điểm/tính thống kê, hiển thị nhận xét nhầm câu/lần thi.
- MEDIUM: lỗi xử lý trạng thái/UX/thiết kế ảnh hưởng sử dụng nhưng không thuộc mức trên.
- LOW: vấn đề nhỏ về nhãn/tài liệu/maintainability.
- Chưa có code, chưa chốt công thức hoặc thiếu môi trường: ghi đúng trạng thái và rủi ro demo, không tự gọi là bug BLOCKING của một thành viên.

Kết luận phải trả lời riêng:
1. Hướng nghiệp vụ/kiến trúc có đúng không?
2. Code đã có chạy đúng không?
3. Demo nhóm 6 có chạy xuyên suốt được chưa, với phần nào thật/phần nào mock?
4. Hệ thống đầy đủ còn thiếu gì?

## 4. Luồng demo cần truy vết

### Nhánh Sinh viên

Đăng nhập/danh tính sinh viên → xem số lượt hoàn thành/số lượt tối đa → mở từng lượt của mình → điểm hoặc Đạt/Không đạt từng câu và nhận xét AI → báo cáo tổng hợp, gồm điểm trung bình qua các lượt đã có kết quả đầy đủ ở chế độ điểm.

### Nhánh Giảng viên

Đăng nhập/danh tính giảng viên → chọn phạm vi lớp/kỳ thi theo giao diện thực có → lấy kết quả của đúng phạm vi → tính câu khó nhất, tỷ lệ trả lời tốt, phân bố điểm → hiển thị bảng và biểu đồ ma trận/tương đương.

Không bắt buộc tạo màn hình chọn riêng nếu ứng dụng đã xác định phạm vi rõ ràng qua route/trang hiện có. Phải kiểm soát ID/phạm vi ở backend.

Chuỗi kỹ thuật khi có persistence:
View → Controller → Service → Repository → DAO → DbContext → DB.

Trong test review có thể thay Repository/dependency nguồn kết quả bằng fake. Phải ghi rõ đoạn chuỗi không được chạy, không tuyên bố đã xác minh DB nếu chỉ mock.

## 5. Chức năng phụ: cần gì trước, cái gì có thể chờ?

| Thành phần | Vai trò với nhóm 6 | Khi chưa có code |
| --- | --- | --- |
| Danh tính và phân quyền | Phân biệt người xem, ngăn xem sai dữ liệu | Có thể mock principal trong test; không gọi đó là login/authorization HTTP đã đạt |
| Sinh viên, giảng viên, lớp và quan hệ truy cập | Xác định chủ báo cáo và phạm vi thống kê | Dùng fixture có ID/quan hệ rõ; ghi contract/owner đang chờ |
| Lần thi, câu hỏi, kết quả từng câu | Đầu vào bắt buộc của báo cáo | Dùng dữ liệu mẫu có liên kết đúng; không cần UI CRUD tất cả thực thể để unit-test báo cáo |
| Thu âm/nhận dạng tiếng nói/tổ chức thi | Nguồn tạo câu trả lời | Có thể chưa tích hợp nếu mục tiêu demo chỉ là báo cáo; ghi hạn chế |
| AI chấm điểm/nhận xét trực tiếp | Nguồn tạo kết quả đánh giá | Có thể dùng kết quả mẫu để kiểm thử; không khẳng định AI thật đã chạy |
| Report cá nhân, tính thống kê, biểu đồ | Chính là nhóm 6 | Không mock toàn bộ logic này rồi tuyên bố main flow đã hoạt động |
| Module khác chưa liên quan | Mở rộng hệ thống sau | Không bắt hoàn thành để nghiệm thu phần nhóm 6 |

Mock phục vụ review đã được yêu cầu. **Việc giảng viên có chấp nhận đầu vào giả lập trong buổi demo hay không cần xác nhận**, không coi checklist này là sự chấp thuận thay giảng viên.

## 6. Checklist báo cáo cá nhân của Sinh viên

| ID | Tiêu chí | Ca kiểm tra tối thiểu |
| --- | --- | --- |
| G6-S01 | Xác định sinh viên thật từ authentication và quyền xem lần thi | Thay ID sinh viên/lần thi trong URL/payload không xem được của người khác |
| G6-S02 | Báo cáo thuộc đúng sinh viên, lần thi, câu hỏi | Hai sinh viên, mỗi người có dữ liệu khác; thêm nhiều lần thi để phát hiện trộn |
| G6-S03 | Hiển thị điểm/điểm tối đa hoặc Đạt/Không đạt theo chế độ giảng viên cấu hình | Không nhầm điểm câu với tổng điểm; không tự đổi Đạt thành điểm hoặc giả thang 10 |
| G6-S04 | Nhận xét AI gắn đúng câu/lần thi | Nhận xét khác nhau cho các câu; không lấy nhận xét đầu tiên dùng cho mọi dòng |
| G6-S05 | Chưa chấm/chưa có nhận xét có trạng thái rõ | Không tự coi null là điểm 0 hoặc bịa nhận xét để trang trông hoàn chỉnh |
| G6-S06 | Chế độ điểm có tổng từng lượt và trung bình các lượt hoàn thành có điểm đầy đủ | Chia cho số lượt có kết quả được dùng, không chia số lượt tối đa; không lấy lượt cao nhất thay trung bình |
| G6-S07 | Trạng thái rỗng, ID không tồn tại, lỗi nguồn dữ liệu xử lý rõ | Không crash, không giả success, không lộ chi tiết nội bộ |
| G6-S08 | UI thực sự gọi backend và render kết quả | Đổi fixture đầu vào làm kết quả thay đổi; không chỉ HTML số cố định |
| G6-S09 | Hiển thị số lượt hoàn thành/tối đa và trạng thái từng lượt | Bỏ dở không tăng số lượt hoàn thành; không ghi đè kết quả lượt cũ |
| G6-S10 | Chế độ Đạt/Không đạt có kết quả từng lượt theo settings và tỷ lệ đạt ghi rõ mẫu số | Kiểm tra tỷ lệ câu đạt tối thiểu và tiêu chí bắt buộc; không tự áp ngưỡng cố định |

## 7. Checklist thống kê lớp của Giảng viên

| ID | Tiêu chí | Ca kiểm tra tối thiểu |
| --- | --- | --- |
| G6-L01 | Backend kiểm tra role và phạm vi lớp được phép theo chính sách xác nhận | Sinh viên gọi URL thống kê bị chặn; giảng viên không đọc ngoài phạm vi |
| G6-L02 | Phạm vi lớp/kỳ thi/lần thi nhất quán giữa bảng và biểu đồ | Fixture ít nhất hai lớp để phát hiện dữ liệu lẫn |
| G6-L03 | Câu khó nhất có tỷ lệ không đạt cao nhất, hiển thị mọi câu đồng hạng | Dùng tỷ lệ, không dùng số sai tuyệt đối hoặc điểm trung bình thấp nhất |
| G6-L04 | Tỷ lệ trả lời tốt có ngưỡng, tử số, mẫu số, cách xử lý thiếu điểm rõ | Test đúng/sát ngưỡng, mẫu số 0, câu chưa chấm |
| G6-L05 | Biểu đồ lớp mặc định dựa trên điểm trung bình các lượt của mỗi sinh viên | X = điểm trung bình, Y = số sinh viên; mỗi sinh viên tính một lần; xem từng lượt phải có nhãn riêng |
| G6-L06 | Biểu đồ ma trận hoặc dạng tương đương có ý nghĩa rõ | Trục, ô/màu/legend/đơn vị nhất quán; không bắt buộc một thư viện hoặc heatmap cụ thể |
| G6-L07 | Thống kê và biểu đồ dùng cùng phạm vi/cách tính | Số trong bảng khớp biểu đồ; đổi dữ liệu/bộ lọc thì cả hai cập nhật |
| G6-L08 | Tỷ lệ theo lượt được chọn; gộp mọi lượt thì đổi đơn vị sang lượt trả lời | Không gọi số lượt là số sinh viên; chưa chấm không là sai; khác thang cần chuẩn hóa trước trung bình |
| G6-L09 | Không có dữ liệu vẫn dùng được | Không chia 0/NaN, không hiện “0%” như kết luận khi thực tế chưa có mẫu |
| G6-L10 | Có thể giải thích và tái tính bằng tay một bộ mẫu | Kết quả backend khớp phép tính độc lập, không kiểm tra bằng cách lặp lại chính thuật toán đang review |

### Quyết định đã duyệt và chi tiết kỹ thuật

Đọc DEC-01..DEC-12 tại [business-rules.md](business-rules.md). Không lặp công thức khác trong checklist.

Bốn điểm cuối đã duyệt: bỏ dở không tính lượt hoàn thành; ngưỡng đạt do GV cấu hình ở chế độ điểm; phân biệt một lượt với gộp lượt; cả lượt Đạt/Không đạt theo tỷ lệ tối thiểu và tiêu chí bắt buộc. Điểm trung bình dùng các lượt có kết quả đầy đủ, không chia số lượt tối đa hoặc lấy điểm cao nhất thay thế.

[Handoff](G6-IMPLEMENTATION-HANDOFF.md) cung cấp contract đề xuất và DEMO-01 để viết test. Thang chuẩn hóa/rounding/histogram ở đó là phương án kỹ thuật và fixture, chưa phải cấu hình production đã được giảng viên duyệt. Mức mock được chấp nhận khi demo cần xác nhận riêng.

## 8. Dữ liệu, contract và kỹ thuật cần review

- [ ] Tìm đúng model/DTO/API hiện có cho báo cáo. Những khái niệm sinh viên/lớp/lần thi/câu hỏi/điểm/nhận xét là nhu cầu thông tin, **không phải lệnh tạo sẵn bảng/class theo tên cụ thể**.
- [ ] Có cách truy vết từ kết quả đến đúng người, câu và lần thi; không dùng NewsTitle/Tag để giả thành dữ liệu thi mà không có semantics/contract phù hợp.
- [ ] Ghi rõ nguồn dữ liệu thật, mock, API teammate chưa tích hợp hoặc chưa xác định.
- [ ] Controller chỉ điều phối input/ModelState/quyền và gọi Service; Service chứa cách tính/nghiệp vụ; không tính khác nhau ở UI và backend.
- [ ] Giữ chiều MVC → BLL → DAL; không Controller → DB/Repository/DAO. Không tự đổi stack vì đổi main flow.
- [ ] Với phần DAL hiện hành, giữ Repository → DAO; Singleton không giữ DbContext scoped; không thêm lớp trùng chỉ để đủ tên.
- [ ] Không tự áp schema/role/rule News vào domain thi. Mã role và DTO mới phải có contract rõ, không tự sửa CHECK/FK để code chạy.
- [ ] Input ID/bộ lọc được validate; query giới hạn quyền ở backend, không chỉ lọc trên giao diện.
- [ ] Không hard-code identity production, điểm thống kê hay nhận xét giả; fixture/mock phải nhận diện và tách riêng.
- [ ] Form ghi dữ liệu, nếu nằm trong phạm vi, có antiforgery và validation; không yêu cầu thêm CRUD chỉ để có test.
- [ ] Credential/connection/AI key từ configuration phù hợp; không lộ secret hoặc dữ liệu cá nhân trong log/response.
- [ ] DI, route, ViewModel, View và endpoint thực khớp; không coi method tồn tại là flow đã nối.
- [ ] Async/cancellation/lifetime hợp lý; lỗi nguồn dữ liệu không bị nuốt rồi báo thành công.
- [ ] Có test sát module, build đúng snapshot; không thay assertion để hợp thức hóa sai kết quả.

## 9. Mock và bằng chứng kiểm thử

### Bộ mẫu đề xuất — chỉ là fixture test

Dùng 2 lớp, 2 giảng viên, ít nhất 3 sinh viên; có dữ liệu ngoài phạm vi được xem. Mỗi báo cáo có liên kết rõ với lần thi và câu hỏi, nhận xét riêng.

Ví dụ tính tay theo DEC (giá trị cấu hình fixture minh họa, không phải ngưỡng bắt buộc mọi kỳ thi):

- Giảng viên test cấu hình mỗi câu tối đa 10, ngưỡng đạt >= 6. Chọn lượt 1: Q1 của A/B/C là 8/4/6, Q2 là 4/2/6.
- Q1 đạt 2/3, không đạt 1/3; Q2 đạt 1/3, không đạt 2/3. Q2 khó nhất vì tỷ lệ không đạt cao hơn, không phải vì so điểm trung bình.
- Thêm câu có cùng tỷ lệ không đạt 2/3 để test đồng hạng; thêm câu chưa ai được chấm để test không xếp hạng.
- Tổng lượt 1 của A/B/C là 12/6/12 trên thang 20. Lượt 2 có tổng 16/10/12, cùng thang 20. Trung bình A/B/C là 14/8/12; biểu đồ có X=8,12,14 và Y=1 ở mỗi điểm, tổng 3 sinh viên, không phải 6 lượt.
- A được phép 3 lượt, đã hoàn thành 2: hiển thị 2/3; trung bình là 14, không chia (12+16) cho 3. Thử lượt bỏ dở và request hoàn thành lặp để không tăng đếm sai.
- Test chế độ Đạt/Không đạt riêng: fixture GV yêu cầu >=75% câu đạt và một tiêu chí bắt buộc. 3/4 câu đạt nhưng trượt tiêu chí bắt buộc thì cả lượt không đạt. 3/4 và thỏa tiêu chí thì đạt.
- Nhận xét từng câu khác nhau; thêm lớp khác, lượt chờ chấm, các thang điểm khác và ca gộp lượt để bắt sai scope/đơn vị/normalization.

Không hard-code giá trị fixture/kết quả vào Service hoặc biểu đồ. Bảng điểm trung bình và thống kê câu hỏi có chế độ xem khác nhau phải ghi nhãn rõ, không ép cùng tập dữ liệu khi một bên gộp lượt còn bên kia chọn một lượt.

### Quy tắc harness

1. Giữ nguyên implementation đang review; chỉ fake dependency chưa có/nguồn kết quả.
2. Nếu chưa có chính Service tính thống kê hoặc View báo cáo, ghi NOT IMPLEMENTED. Không viết thay toàn bộ rồi cho PASS code gốc.
3. Fake bám contract thật; nếu phải giả định contract, ghi rõ chưa xác nhận tích hợp.
4. Kiểm tra tham số, scope, số lần gọi và kết quả; operation chưa hỗ trợ không mặc định trả success.
5. Test direct action không thực thi model binding/authorization middleware/antiforgery; không dùng nó làm bằng chứng phân quyền HTTP đã đạt.
6. Mock Repository không kiểm chứng SQL, FK, transaction hay truy vấn production.
7. Compile Razor không kiểm chứng tương tác/biểu đồ; cần browser check hoặc ghi NEEDS MANUAL TEST.
8. Login giả trong test không phải login thật; nhận xét fixture không phải AI đang chấm thật.
9. Tách thống kê test hiện có, test bổ sung, test skip/fail và lỗi môi trường. Dữ liệu nhạy cảm không được dùng làm fixture.
10. Report nêu cách thay mock bằng implementation teammate: interface/DTO, DI, owner và test tích hợp cần chạy lại.

## 10. Review phần đồng đội đang làm và phân công

Không giữ nguyên bảng TV1 DAL/TV3 News/TV5 Admin Report như trách nhiệm đã xác nhận của domain thi. Tên thành viên/nhánh mới phải được kiểm tra. Đề xuất dưới đây là **nhóm trách nhiệm review, chưa phải giao việc chính thức**:

| Nhóm trách nhiệm | Phần cần bàn giao | Owner thực tế | Điểm nối |
| --- | --- | --- | --- |
| Dữ liệu/contract | Liên kết người–lớp–lần thi–câu–kết quả | Xác minh TV/nhánh | Nguồn dữ liệu cho hai báo cáo |
| Auth/phạm vi truy cập | Danh tính Sinh viên/Giảng viên, chính sách lớp | Xác minh TV/nhánh | Bảo vệ endpoint và query scope |
| Báo cáo cá nhân | Điểm từng câu, nhận xét, trạng thái | Xác minh TV/nhánh | Contract kết quả và UI |
| Thống kê lớp/biểu đồ | Công thức, tập dữ liệu, bảng/biểu đồ | Xác minh TV/nhánh | Cùng filter/scope/đơn vị |
| Tích hợp/test/demo | Ghép phần thật/mock, build, role test, kịch bản | Chủ dự án điều phối, xác minh người làm | Không gán mọi lỗi cho người tích hợp |

Với mỗi phần chưa có, AGY điền:
- Phần nào thiếu, owner/nhánh và nguồn xác nhận đang làm.
- Contract đã có hay chưa; code tiêu thụ có phù hợp không.
- Mock đã thử gì, kết quả và giới hạn.
- Có chặn demo không; có thể demo với mock được công khai hay cần code thật.
- Test phải chạy lại khi đồng đội push.

## 11. Cổng sẵn sàng demo nhóm 6

Không bắt mọi chức năng của dự án đầy đủ. Đánh giá từng mục:
- [ ] Có lối vào màn hình và danh tính đúng cho hai vai trò; nếu là test identity thì công khai giới hạn.
- [ ] Sinh viên mở được báo cáo đúng lần thi của mình, thấy điểm và nhận xét từng câu.
- [ ] Giảng viên mở được thống kê đúng lớp/phạm vi.
- [ ] Các chỉ số được tính từ dữ liệu đầu vào, công thức có căn cứ hoặc giả định demo đã xác nhận.
- [ ] Bảng và biểu đồ khớp; có thể đổi một đầu vào và thấy kết quả thay đổi đúng.
- [ ] Không truy cập chéo dữ liệu ngoài quyền; có ca URL/ID trực tiếp.
- [ ] Không dữ liệu/chưa chấm/lỗi tải có trạng thái rõ.
- [ ] Build/test phần được demo có bằng chứng; UI được chạy thử.
- [ ] Liệt kê rõ code thật, dữ liệu mẫu, dependency giả và phần chưa tích hợp.
- [ ] Các phần mock được phép cho buổi demo đã được xác nhận; không trình bày là AI/DB thật.

Kết luận riêng: **sẵn sàng demo thật**, **sẵn sàng demo giới hạn với mock được chấp nhận**, hoặc **chưa sẵn sàng**, kèm nguyên nhân. Không suy ra “hệ thống hoàn thành” từ việc demo giới hạn đạt.

## 12. Quy trình, an toàn và đầu ra của AGY

1. Xác định workspace, branch, commit, base, git status và nhánh teammate cần xem. Không review nhầm checkout cũ; nếu chưa xác minh remote thì nói rõ snapshot.
2. Đọc checklist cùng business-rules, architecture và handoff; kiểm tra snapshot đang dùng đã có tài liệu mới chưa, không áp tài liệu News trong archive làm yêu cầu nhóm 6.
3. Đọc code và lần theo flow thật; tên method/class/file chỉ báo cáo khi tồn tại, không invent.
4. Build/tests an toàn trên snapshot phù hợp. Với solution .NET hiện có:
   `dotnet build AIVESSystem.sln --configuration Release`
   `dotnet test AIVESSystem.sln --configuration Release --no-build`
   Kiểm tra test setup/biến môi trường trước; không chạy SQL write tests, seed hay migration mặc định. SDK local có thể dùng nếu đã có.
5. Không tự cài dependency/framework, sửa source, schema hoặc rule files; không commit/push/merge/issue/PR. Chỉ sửa khi được giao riêng.
6. Không tìm ngoài workspace hoặc thư mục PRN222 khác. Giữ teammate changes, không reset/clean/stash để thuận tiện.
7. Được tạo test/mock tách riêng trong workspace theo yêu cầu review; báo file tạo và dọn đúng artifact của mình, không xóa file người khác.
8. Không sửa cảnh báo bằng cách đổi mục tiêu test. Không nói đã test khi chỉ đọc code.
9. Báo cáo không lộ token/password/cookie/connection string có secret.

### Định dạng báo cáo bắt buộc

- Phạm vi/snapshot, mục tiêu nhóm 6, những gì dùng thật/mock.
- Kết luận hướng đi và sẵn sàng demo, tách khỏi mức hoàn thiện toàn hệ thống.
- Lỗi có bằng chứng trước; tiếp theo là thiếu implementation, dependency chưa push và quyết định chưa chốt.
- Ma trận:

| ID | Yêu cầu | Status | Bằng chứng file:line/method/test | Owner | Phụ thuộc | Ảnh hưởng demo | Severity nếu là bug | Hướng xử lý |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |

- Trace hai nhánh Sinh viên/Giảng viên; nêu điểm đứt và boundary mock.
- Bảng test: đầu vào, kỳ vọng, thực tế, code thật, mock, giới hạn; kết quả build/pass/fail/skip.
- Những tiêu chí tài liệu/code còn lệch nhóm 6, không tự sửa hoặc coi là đã đồng bộ.
- Fix order: chặn demo → quyền và tính đúng → điểm nối → UI → ngoài phạm vi để sau.
- File tạo/thay đổi và xác nhận không ghi DB/publish trái quyền.

## 13. Prompt giao AGY

```text
Review độc lập code AIVES trong workspace hiện tại.

Nhánh/commit: [điền hoặc xác minh]
Các nhánh teammate cần đối chiếu: [điền nếu có]
Phần teammate đang làm/chưa push: [điền nếu biết, không tự đoán owner]

Mục tiêu mới đã được chủ dự án xác nhận: demo NHÓM 6 — PHẢN HỒI & BÁO CÁO
trước, không yêu cầu toàn bộ hệ thống hoàn thành. Sinh viên xem điểm từng câu
và nhận xét AI; giảng viên xem câu khó nhất, tỷ lệ trả lời tốt, phân bố điểm,
biểu đồ ma trận/tương đương. Các chức năng phụ phục vụ luồng này.

Đọc docs/CODE-REVIEW-CHECKLIST.md, business-rules.md, ARCHITECTURE.md,
TASKS.md và G6-IMPLEMENTATION-HANDOFF.md cùng AGENTS/skill kiến trúc đã cập nhật.
Không áp tài liệu News trong docs/archive làm tiêu chí nhóm 6.
Giữ quy tắc an toàn; không tự đổi stack, schema, role mapping hay phục hồi code cũ.

Chỉ review, không sửa implementation, DB, rules hoặc tự triển khai phần thiếu.
Không cài package, commit/push/merge hoặc tạo/sửa issue/PR. Chỉ trong workspace.
Build/test an toàn nếu có thể; không chạy test ghi DB khi chưa được phép.

Dùng mock tách riêng cho dependency/đầu vào đang chờ đồng đội; giữ code báo cáo
và tính thống kê thật. Nếu chính phần báo cáo chưa có, ghi NOT IMPLEMENTED,
không viết mock thay nó rồi cho PASS. Không quy thiếu code người khác thành
lỗi của thành viên đang review, nhưng phải nói rõ ảnh hưởng đến demo.

Áp dụng DEC-01..DEC-12 đã duyệt trong docs/business-rules.md: lượt hoàn thành/tối đa, hai chế độ
chấm theo settings GV, trung bình các lượt có kết quả đầy đủ, độ khó theo tỷ lệ
không đạt, và phân biệt số sinh viên với số lượt trả lời. Không quay về cách
chọn lượt cao nhất hoặc dùng điểm trung bình để xếp độ khó. Giá trị settings
fixture phải có nhãn; chi tiết làm tròn/thang chuẩn hóa còn cần xác định thì
báo rõ. Không mặc định mock được giảng viên chấp nhận cho demo.

Báo cáo tiếng Việt theo mục 12: evidence, status, owner/phụ thuộc, lỗi thật,
phần đang làm, manual checks, flow thật/mock, build/test và thứ tự xử lý.
Trả lời: hướng đi đúng chưa, phần nào hỗ trợ nhóm 6, còn thiếu gì để demo,
phần nào có thể chờ teammate, phần nào lệch mục tiêu và cần chốt lại.
```
