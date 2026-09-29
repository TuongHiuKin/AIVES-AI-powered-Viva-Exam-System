# Business rules — AIVES, nhóm 6: Phản hồi & báo cáo

Nguồn nghiệp vụ hiện hành theo các xác nhận của chủ dự án ngày 29/09/2026. Main flow: báo cáo cá nhân của sinh viên theo lượt thi; thống kê lớp và biểu đồ của giảng viên. Các nhóm chức năng khác được bổ sung sau.

Đây là **quy tắc đã duyệt**, không phải khẳng định implementation/schema nhóm 6 đã tồn tại. Nền kỹ thuật không tự thay đổi theo quyết định nghiệp vụ.

### Quyết định nghiệp vụ đã duyệt — bản cập nhật 29/09/2026

Các mã DEC dưới đây là nguồn tiêu chí cho task nhóm 6. Quyết định mới thay cách chọn “lượt chính thức/cao nhất” trước đây cho biểu đồ tổng hợp; không tiếp tục sử dụng quy tắc cũ đó làm mặc định.

| Mã | Nội dung đã chốt |
| --- | --- |
| DEC-01 | Giảng viên cấu hình số lượt tối đa cho đợt thi. Theo dõi số lượt hoàn thành/tối đa; chỉ hoàn thành mới tính một lượt, bỏ dở/hủy có trạng thái riêng. Đủ lượt thì không được bắt đầu lượt mới. Mỗi lượt giữ kết quả riêng, không ghi đè. |
| DEC-02 | Hai chế độ: chấm điểm theo rubric/format, điểm tối đa từng câu; hoặc Đạt/Không đạt theo tiêu chí do giảng viên thiết lập. AI áp dụng chuẩn giảng viên, không tự đặt chuẩn. |
| DEC-03 | Trong chế độ điểm, giảng viên cấu hình ngưỡng đạt theo rubric để phân loại đạt/không đạt cho thống kê câu hỏi; không mặc định 5, 6 hoặc một ngưỡng do AI tự chọn. |
| DEC-04 | Trong chế độ Đạt/Không đạt, kết luận toàn lượt theo tỷ lệ câu đạt tối thiểu và các tiêu chí bắt buộc nếu có, do giảng viên cấu hình. Không tự đổi Đạt/Không đạt thành điểm số. |
| DEC-05 | Báo cáo cá nhân: danh tính, lượt đã hoàn thành/tối đa, danh sách từng lượt; mỗi lượt có câu hỏi/câu trả lời, điểm hoặc kết quả đạt, nhận xét AI. Chế độ điểm: tổng lượt = tổng điểm câu; trung bình = tổng điểm các lượt có kết quả đầy đủ / số lượt đó. Không chia số lượt được phép, không tính chưa thi/chưa chấm là 0. |
| DEC-06 | Biểu đồ lớp chế độ điểm: trục X là điểm trung bình qua các lượt của sinh viên, trục Y là số sinh viên có điểm đó; một sinh viên tính một lần. Có bảng điểm từng lượt riêng, ghi rõ nếu chọn xem phân bố của một lượt thay vì trung bình. |
| DEC-07 | Thống kê câu hỏi mặc định theo lượt được chọn (ví dụ lượt số 1 của mỗi sinh viên). Tỷ lệ đạt = số sinh viên đạt câu Q / số sinh viên có câu trả lời Q đã được đánh giá hợp lệ × 100%. Tỷ lệ không đạt dùng số không đạt trên cùng mẫu số. Sinh viên không có lượt/câu đó không tự thành sai. |
| DEC-08 | Nếu gộp nhiều lượt, tử số/mẫu số đếm lượt trả lời đã được đánh giá và nhãn phải ghi “tỷ lệ lượt trả lời”, không phải “tỷ lệ sinh viên”. Mỗi câu trả lời trong một lượt chỉ được tính một lần dù query JOIN nhiều dòng tiêu chí. |
| DEC-09 | Câu trả lời tốt xếp theo tỷ lệ đạt giảm dần; câu khó nhất có tỷ lệ không đạt cao nhất. Hiển thị tất cả câu đồng hạng. Không thay bằng số người sai tuyệt đối hoặc điểm trung bình thấp nhất. |
| DEC-10 | Chưa chấm/không có dữ liệu là trạng thái riêng, không tự thành 0/không đạt. Mẫu số 0 hiển thị chưa đủ dữ liệu, không xếp câu đó là dễ nhất/khó nhất. Lượt hoàn thành nhưng còn chờ chấm vẫn tính vào số lần đã thi, chưa dùng trong trung bình điểm; báo rõ trung bình hiện tại đang dựa trên bao nhiêu lượt. |
| DEC-11 | Sinh viên chỉ xem kết quả của mình; giảng viên giới hạn ở lớp/học phần được phân công. Kiểm tra backend, không chỉ menu. Không tự chuyển role Staff của News thành Sinh viên. |
| DEC-12 | Khi các lượt khác tổng điểm tối đa, đưa về cùng thang trước khi lấy trung bình; hiển thị thang rõ ràng. Giữ rubric/settings dùng cho mỗi lượt để sửa settings về sau không âm thầm thay ý nghĩa kết quả cũ. |

Bốn điểm được duyệt cuối: bỏ dở không tính lượt hoàn thành (DEC-01); ngưỡng đạt do giảng viên cấu hình ở chế độ điểm (DEC-03); phân biệt thống kê một lượt với gộp lượt (DEC-07/08); tỷ lệ đạt tối thiểu và điều kiện bắt buộc cho toàn lượt (DEC-04).

### Chi tiết triển khai còn phải ghi rõ, không thay đổi quyết định đã duyệt

- Giá trị settings cụ thể của từng đợt thi phải có, không tự chọn ngưỡng/thang toàn hệ thống. Nếu cần chuẩn hóa, chốt thang hiển thị chung, precision/làm tròn và khoảng histogram; không làm tròn trung gian để đổi kết luận đạt.
- Với chế độ Đạt/Không đạt, báo cáo phải ghi rõ tỷ lệ đang đếm câu hay lượt; không gọi là điểm trung bình số. Nếu cần biểu đồ tỷ lệ riêng thì chốt trục/đơn vị trước, không tự giả yêu cầu mới.
- Contract phải phân biệt hoàn thành thi với hoàn thành chấm; dùng khái niệm trạng thái hiện có hoặc đề xuất mapping, không tự thêm schema trong lượt review.
- Thỏa thuận mức mock được chấp nhận khi demo với giảng viên chấm; việc duyệt business rule không tự xác nhận cho phép demo bằng mock.

## Áp dụng và giới hạn

- Chuẩn và rubric do giảng viên cấu hình; AI áp dụng, không tự quyết định chuẩn đạt.
- Các giá trị test như ngưỡng 60%, tỷ lệ lượt đạt 75%, scale 10 chỉ là fixture, không thay quyết định giảng viên.
- Contract/settings đề xuất tại [G6-IMPLEMENTATION-HANDOFF.md](G6-IMPLEMENTATION-HANDOFF.md) giúp triển khai/test. Phần còn chờ owner hoặc thang/rounding production phải ghi rõ, không tự coi đã được người dùng duyệt.
- [TASKS.md](TASKS.md) mô tả bàn giao; [CODE-REVIEW-CHECKLIST.md](CODE-REVIEW-CHECKLIST.md) mô tả bằng chứng nghiệm thu.
- Quy tắc News trước đây được giữ ở [bản lưu News](archive/NEWS-business-rules.md). Chúng chỉ phục vụ bảo toàn module News, không là tiêu chí báo cáo thi. Việc cập nhật tài liệu không chuyển đổi dữ liệu News thành Exam, không sửa SQL hoặc cấp quyền chạy scripts.
