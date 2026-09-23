---
title: "AIVES — 01. Product Requirements Document (PRD)"
document_id: "AIVES-PRD-01"
status: "draft"
version: "1.0.0"
created: "2026-09-22"
updated: "2026-09-22"
project: "AIVES (AI-powered Viva Exam System)"
author: "ANKIN (via BMAD PM & BA Agents)"
---

# AIVES (AI-powered Viva Exam System)
## Tài Liệu Đặc Tả Yêu Cầu Sản Phẩm & Nghiệp Vụ (PRD / BRD)
**Mã tài liệu:** `AIVES-DOC-01-PRD`

---

## 1. Tuyên Bố Bài Toán & Mục Tiêu (Problem Statement & Goals)

### 1.1. Bối cảnh & Nỗi đau nghiệp vụ (Problem Statement)
Hình thức thi vấn đáp (*viva / oral exam*) giữ vai trò thiết yếu trong giáo dục đại học (đặc biệt trong đánh giá đồ án tốt nghiệp, thi thực hành chuyên sâu và phỏng vấn năng lực) nhờ khả năng đo lường tư duy phản biện, lập luận độc lập và chống gian lận học vẹt. Tuy nhiên, quy trình tổ chức thi vấn đáp truyền thống đang gặp phải 5 nút thắt nghiêm trọng:
1. **Quá tải áp lực cho Giảng viên:** Để vấn đáp 100–200 sinh viên (mỗi sinh viên 15 phút), hội đồng giảng viên phải làm việc liên tục 25–50 giờ, gây mệt mỏi và làm suy giảm độ tập trung ở các ca thi cuối.
2. **Thiếu chuẩn hóa câu hỏi & độ khó:** Các phòng thi hoặc ca thi khác nhau nhận bộ câu hỏi có độ khó không tương đồng; giảng viên khó duy trì cùng một thước đo câu hỏi đào sâu (*follow-up*) cho toàn bộ sinh viên.
3. **Thiếu nhất quán trong thang chấm Rubric:** Việc chấm điểm vấn đáp trực tiếp thường bị ảnh hưởng bởi yếu tố tâm lý cảm tính, thiên kiến hoặc sự khác biệt về độ khắt khe giữa các giám khảo.
4. **Nút thắt mở rộng quy mô (Scalability Bottleneck):** Không thể áp dụng thi vấn đáp định kỳ cho các môn học có sĩ số lớn (300–1000 sinh viên) do giới hạn số lượng giảng viên và phòng thi vật lý.
5. **Thiếu bằng chứng khách quan khi sinh viên khiếu nại điểm:** Khi có khiếu nại phúc khảo, nhà trường và bộ môn không có dữ liệu lưu vết khách quan (không có bản ghi âm đồng bộ, không có bóc băng văn bản chi tiết từng câu hỏi - câu trả lời) để giải trình.

### 1.2. Mục tiêu sản phẩm (Product Goals)
- **G-1 (Chuẩn hóa quy trình thi vấn đáp):** Số hóa và tự động hóa toàn bộ vòng đời kỳ thi viva từ tạo câu hỏi, phân bổ ca thi, phỏng vấn giọng nói, gợi ý chấm điểm đến công bố kết quả.
- **G-2 (Giám khảo ảo AI thích ứng):** Cung cấp Lõi phỏng vấn AI có khả năng đặt câu hỏi chính và thực hiện **hỏi xoáy thích ứng (Adaptive Follow-up)** bằng giọng nói tiếng Việt/Anh tự nhiên dựa trên chất lượng câu trả lời của sinh viên.
- **G-3 (Bảo đảm vai trò làm chủ của Giảng viên - Human-in-the-Loop):** AI chỉ đóng vai trò trợ lý đề xuất điểm và nhận xét dựa trên Rubric; Giảng viên là người duy nhất nắm quyền phê duyệt câu hỏi và chốt điểm số chính thức.
- **G-4 (Minh bạch hóa & Lưu vết bất biến):** Ghi vết đầy đủ mọi tương tác (âm thanh, transcript, gợi ý của AI, thao tác sửa điểm của giảng viên) làm bằng chứng cho kiểm định chất lượng và giải quyết khiếu nại.
- **G-5 (Nâng cao chất lượng dạy và học):** Cung cấp phản hồi cá nhân hóa sâu sắc cho sinh viên và báo cáo phân tích phổ điểm, độ khó câu hỏi theo thang Bloom cho giảng viên.

---

## 2. Các Bên Liên Quan & Vai Trò Người Dùng (Stakeholders & Actors)

| Bên liên quan / Tác tử | Vai trò trong hệ thống | Mục tiêu cốt lõi |
|---|---|---|
| **Giảng viên (Lecturer)** | `LECTURER` | Tạo ngân hàng câu hỏi, duyệt câu hỏi RAG, cấu hình ca thi, duyệt điểm số AI đề xuất và chốt điểm chính thức, xem thống kê lớp học. |
| **Sinh viên (Student)** | `STUDENT` | Tham gia phiên thi vấn đáp bằng giọng nói với Giám khảo ảo AI, xem báo cáo phản hồi chi tiết sau khi điểm được công bố, gửi khiếu nại điểm nếu có căn cứ. |
| **Quản trị viên (Administrator)** | `ADMIN` | Quản trị tài khoản, phân quyền giảng viên theo môn học, cấu hình tham số hệ thống (STT/TTS engines, quota, chính sách lưu trữ). |
| **Hệ thống AI (AI Subsystems)** | `SYSTEM_AI` | Thực thi RAG sinh câu hỏi, Speech-to-Text, Text-to-Speech, phân tích ngữ nghĩa câu trả lời, sinh câu hỏi đào sâu và gợi ý chấm điểm Rubric. |
| **Phòng Đào tạo / Khoa (Academic Board)** | `STAKEHOLDER` | Tiếp nhận bảng điểm xuất ra theo mẫu chuẩn để cập nhật vào hệ thống quản lý đào tạo (SIS), kiểm toán bằng chứng khi có khiếu nại. |

---

## 3. Phạm Vi Dự Án (Scope & Out of Scope)

### 3.1. Trong phạm vi (In Scope)
- Quản lý ngân hàng câu hỏi, phân loại Bloom (*Remember, Understand, Apply, Analyze*), liên kết Rubric tiêu chí.
- Trích xuất tài liệu môn học (slides, giáo trình PDF/DOCX) và dùng RAG hỗ trợ giảng viên sinh câu hỏi nháp.
- Phân bổ đề thi ngẫu nhiên/thích ứng tránh trùng lặp giữa các thí sinh thi liên tiếp trong ca thi.
- Lõi vấn đáp thời gian gần thực: TTS phát câu hỏi $\rightarrow$ Thu âm $\rightarrow$ STT tiếng Việt/Anh $\rightarrow$ Phát hiện ngắt câu (VAD) $\rightarrow$ LLM sinh câu hỏi hỏi xoáy thích ứng.
- Chấm điểm gợi ý theo Rubric đa tiêu chí, bóc tách điểm mạnh, điểm yếu, khái niệm bỏ sót, ghi nhận tín hiệu phụ (thời gian trả lời, độ trôi chảy).
- Giao diện làm việc của giảng viên (Human-in-the-loop review): Nghe lại audio, đọc transcript, sửa điểm, ghi chú và chốt điểm (`FINALIZED`/`PUBLISHED`).
- Báo cáo phản hồi chi tiết cho sinh viên và Dashboard thống kê lớp học cho giảng viên (phổ điểm, câu hỏi khó, phân tích Bloom, xuất Excel/PDF).
- Ghi âm, lưu trữ transcript, audit log phục vụ quy trình khiếu nại điểm.

### 3.2. Ngoài phạm vi (Out of Scope)
- **Tự động hóa chấm điểm 100% không có con người:** Hệ thống nghiêm cấm việc công bố điểm trực tiếp từ AI mà không qua giảng viên phê duyệt.
- **Nhận diện cảm xúc & phân tích ánh mắt qua Video (Computer Vision Proctoring):** Phiên bản v1 tập trung hoàn toàn vào âm thanh và ngữ nghĩa ngôn ngữ; không dùng camera để chấm biểu cảm để tránh thiên kiến.
- **Tích hợp đồng bộ tự động 2 chiều thời gian thực với hệ thống SIS trường học:** v1 hỗ trợ chuẩn hóa định dạng Xuất / Nhập thông qua file bảng điểm Excel/CSV chuẩn của trường.

---

## 4. Các Quy Tắc Nghiệp Vụ Cốt Lõi (Business Rules)

- **BR-001 (Human-in-the-Loop về Nội dung):** Mọi câu hỏi do AI sinh ra (qua RAG hoặc Prompt) đều ở trạng thái `PENDING_REVIEW` và không được phép đưa vào ca thi chính thức nếu chưa có xác nhận `APPROVED` từ giảng viên.
- **BR-002 (Human-in-the-Loop về Điểm số):** Điểm số và nhận xét do AI đề xuất (`AI_SUGGESTED`) chỉ mang tính chất khuyến nghị. Điểm số chỉ trở thành điểm chính thức (`FINAL_GRADE`) khi được giảng viên xác nhận `FINALIZED`.
- **BR-003 (Chính sách công bố điểm):** Sinh viên không được phép xem điểm số hoặc nhận xét khi bài thi đang ở trạng thái `AI_SUGGESTED` hoặc `IN_REVIEW`. Sinh viên chỉ được truy cập báo cáo khi trạng thái chuyển sang `PUBLISHED`.
- **BR-004 (Giới hạn vòng lặp hỏi xoáy - Adaptive Follow-up Boundary):** Mỗi câu hỏi chính chỉ được phép có tối đa số lượt hỏi xoáy $N_{max}$ (mặc định $N_{max} = 2$) để đảm bảo không vượt quá thời lượng ca thi của từng sinh viên.
- **BR-005 (Bảo toàn bằng chứng bất biến - Immutability of Viva Trace):** Transcript hội thoại và file âm thanh nguyên bản của phiên thi một khi đã hoàn thành (`COMPLETED`) là dữ liệu chỉ đọc (read-only), không một vai trò nào (kể cả Admin) được phép chỉnh sửa nội dung văn bản bóc băng hay cắt ghép âm thanh.
- **BR-006 (Ghi vết điều chỉnh điểm - Audit Logging):** Nếu giảng viên điều chỉnh điểm số khác so với điểm AI đề xuất, hệ thống bắt buộc phải ghi log: Giá trị điểm cũ, Giá trị điểm mới, Định danh giảng viên, Dấu thời gian, và Lý do điều chỉnh.
- **BR-007 (Phân bổ câu hỏi không trùng lặp):** Thuật toán bốc đề cho các sinh viên thi trong cùng một ca thi phải ưu tiên lựa chọn câu hỏi sao cho hai sinh viên thi kế tiếp nhau không nhận cùng một bộ câu hỏi chính.
- **BR-008 (Snapshot Rubric & Câu hỏi tại thời điểm thi):** Khi sinh viên bắt đầu thi, nội dung câu hỏi và barem Rubric tại thời điểm đó phải được lưu vết dưới dạng snapshot bất biến; mọi sửa đổi của giảng viên vào ngân hàng câu hỏi sau này không làm ảnh hưởng đến căn cứ chấm điểm của bài thi đã diễn ra.

---

## 5. Đặc Tả Yêu Cầu Chức Năng (Functional Requirements)

### 5.1. Nhóm 1: Quản Lý Ngân Hàng Câu Hỏi & Rubric (Question Bank & Rubric Management)

#### FR-001: Tạo và quản lý câu hỏi thủ công
- **Mô tả:** Giảng viên có thể tạo mới, chỉnh sửa, lưu trữ và xóa câu hỏi vấn đáp thủ công; gắn câu hỏi với môn học, chủ đề, thời lượng trả lời dự kiến (giây) và số lượt hỏi xoáy tối đa.
- **Tiêu chuẩn kiểm thử (Acceptance Criteria):**
  - Hệ thống lưu thành công câu hỏi với đầy đủ môn học, chủ đề, nội dung câu hỏi.
  - Cho phép gắn số lượt hỏi xoáy tối đa từ 0 đến 3 (mặc định là 1).

#### FR-002: Nhập câu hỏi hàng loạt (Batch Import)
- **Mô tả:** Giảng viên có thể nhập danh sách câu hỏi và barem điểm từ file mẫu định dạng Excel (.xlsx) hoặc CSV.
- **Tiêu chuẩn kiểm thử:**
  - Hệ thống kiểm tra tính hợp lệ dữ liệu (validation), báo lỗi chi tiết theo từng dòng nếu thiếu trường bắt buộc, và import thành công các dòng hợp lệ.

#### FR-003: Sinh câu hỏi tự động từ tài liệu môn học (RAG-based Question Generation)
- **Mô tả:** Giảng viên có thể tải lên tài liệu môn học (PDF, DOCX, PPTX). Hệ thống lập chỉ mục (chunking, embedding) và cho phép giảng viên yêu cầu AI sinh câu hỏi theo chủ đề hoặc chương mục mong muốn.
- **Tiêu chuẩn kiểm thử:**
  - AI sinh ra câu hỏi kèm theo đoạn trích dẫn nguồn (citation) từ tài liệu làm căn cứ.
  - Câu hỏi được sinh ra tự động lưu ở trạng thái `PENDING_REVIEW`.

#### FR-004: Gắn nhãn cấp độ nhận thức theo Thang Bloom
- **Mô tả:** Mỗi câu hỏi bắt buộc được gán nhãn chính xác một trong các cấp độ Bloom: *Nhớ (Remember), Hiểu (Understand), Vận dụng (Apply), Phân tích (Analyze)*.
- **Tiêu chuẩn kiểm thử:**
  - Hệ thống không cho phép lưu câu hỏi chính thức nếu thiếu nhãn Bloom.
  - AI có khả năng gợi ý nhãn Bloom phù hợp dựa trên nội dung câu hỏi.

#### FR-005: Thiết lập và liên kết Rubric chấm điểm
- **Mô tả:** Giảng viên định nghĩa bảng Rubric gồm nhiều tiêu chí thành phần (ví dụ: *Độ chính xác khái niệm, Khả năng lập luận phản biện, Độ mạch lạc và thuật ngữ*). Mỗi tiêu chí có trọng số (%) và thang điểm tối đa.
- **Tiêu chuẩn kiểm thử:**
  - Tổng trọng số của các tiêu chí thành phần trong một câu hỏi bắt buộc phải bằng 100%.
  - Câu hỏi phải được gắn với ít nhất 1 tiêu chí Rubric trước khi đưa vào kỳ thi.

#### FR-006: Quy trình thẩm định & phê duyệt câu hỏi (Lecturer Approval Workflow)
- **Mô tả:** Giảng viên có giao diện rà soát danh sách câu hỏi do AI sinh ra, có thể chỉnh sửa nội dung, sửa tiêu chí rubric, từ chối (loại bỏ) hoặc phê duyệt (`APPROVED`) để đưa vào ngân hàng chính thức.
- **Tiêu chuẩn kiểm thử:**
  - Chỉ những câu hỏi có trạng thái `APPROVED` mới xuất hiện trong bộ chọn đề thi của Nhóm 2.

---

### 5.2. Nhóm 2: Quản Lý Kỳ Thi & Lịch Thi (Exam & Schedule Management)

#### FR-007: Tạo và cấu hình ca thi vấn đáp
- **Mô tả:** Giảng viên tạo kỳ thi, gắn với môn học, xác định khung thời gian bắt đầu/kết thúc, thời lượng tối đa cho mỗi sinh viên (ví dụ: 15 phút/thí sinh), số lượng câu hỏi chính bắt buộc (ví dụ: 2 câu).
- **Tiêu chuẩn kiểm thử:**
  - Hệ thống lưu cấu hình ca thi và kiểm tra ràng buộc: Thời lượng thi của sinh viên phải phù hợp với tổng thời lượng dự kiến của các câu hỏi chính và câu hỏi xoáy.

#### FR-008: Quản lý danh sách thí sinh và phân bổ ca thi
- **Mô tả:** Giảng viên nhập danh sách sinh viên tham gia thi (từ file Excel hoặc chọn từ danh sách lớp). Hệ thống phân bổ khung giờ thi hoặc cấp mã truy cập phiên thi duy nhất cho từng thí sinh.
- **Tiêu chuẩn kiểm thử:**
  - Mỗi thí sinh có một mã định danh phiên thi (`access_token` / `attempt_id`) duy nhất, ngăn chặn việc đăng nhập trùng lặp.

#### FR-009: Thuật toán chọn đề thi ngẫu nhiên & thích ứng
- **Mô tả:** Khi sinh viên bắt đầu thi, hệ thống tự động chọn ngẫu nhiên bộ câu hỏi từ ngân hàng câu hỏi đã được duyệt của môn học, bảo đảm cân bằng theo tỷ lệ cấp độ Bloom đã cấu hình và giảm thiểu tối đa việc trùng lặp câu hỏi giữa các thí sinh thi liên tiếp.
- **Tiêu chuẩn kiểm thử:**
  - Hai sinh viên thi kế tiếp nhau trong cùng ca thi không nhận bộ câu hỏi chính giống nhau 100%.
  - Bộ câu hỏi chọn ra tuân thủ đúng cấu hình số lượng câu hỏi của từng mức độ Bloom.

#### FR-010: Khóa đề và lưu vết Snapshot phiên thi
- **Mô tả:** Ngay khi đề thi được bốc cho sinh viên, hệ thống lưu bản sao snapshot bất biến của các câu hỏi và rubric tương ứng gắn chặt với `ExamAttempt` của sinh viên đó.
- **Tiêu chuẩn kiểm thử:**
  - Việc sửa đổi câu hỏi trong ngân hàng sau thời điểm bốc đề không làm biến đổi nội dung snapshot của bài thi đang diễn ra.

---

### 5.3. Nhóm 3: Lõi Phỏng Vấn AI (AI Viva Interview Engine - Core Feature)

#### FR-011: Đọc câu hỏi bằng giọng nói (Text-to-Speech)
- **Mô tả:** Giám khảo ảo AI chuyển đổi văn bản câu hỏi thành giọng nói tiếng Việt (hoặc tiếng Anh) tự nhiên, rõ ràng, phát qua trình duyệt của sinh viên.
- **Tiêu chuẩn kiểm thử:**
  - Sinh viên nghe được âm thanh rõ ràng; giao diện hiển thị trạng thái "AI đang nói".
  - Cho phép sinh viên yêu cầu AI phát lại câu hỏi tối đa 1 lần nếu gặp sự cố âm thanh.

#### FR-012: Thu âm câu trả lời & Bóc băng gần thực (Near-Real-time Speech-to-Text)
- **Mô tả:** Hệ thống thu nhận tín hiệu micro của sinh viên và chuyển đổi giọng nói thành văn bản tiếng Việt theo thời gian thực hoặc ngay sau khi sinh viên kết thúc phát biểu.
- **Tiêu chuẩn kiểm thử:**
  - Bóc băng chính xác thuật ngữ chuyên ngành môn học theo từ điển ngữ cảnh (Contextual Vocabulary).
  - Bản bóc băng gắn liền nhãn thời gian bắt đầu và kết thúc của lượt nói.

#### FR-013: Phát hiện khoảng lặng và kết thúc lượt trả lời (Voice Activity Detection - VAD)
- **Mô tả:** Hệ thống tự động nhận biết sinh viên đã trả lời xong khi xuất hiện khoảng lặng liên tục (ví dụ: 3 giây), hoặc cho phép sinh viên chủ động bấm nút "Tôi đã trả lời xong".
- **Tiêu chuẩn kiểm thử:**
  - Hệ thống không ngắt lời khi sinh viên chỉ tạm dừng suy nghĩ dưới 2.5 giây.
  - Kích hoạt tiến trình phân tích câu trả lời ngay khi VAD xác nhận ngắt lượt.

#### FR-014: Quyết định và sinh câu hỏi hỏi xoáy thích ứng (Adaptive Follow-up Generation)
- **Mô tả:** Dựa trên câu hỏi chính, barem rubric, và transcript câu trả lời vừa thu được:
  - Nếu câu trả lời thiếu ý trọng tâm, mơ hồ, hoặc mâu thuẫn: AI sinh câu hỏi hỏi xoáy ngắn gọn, tập trung vào điểm còn yếu để sinh viên làm rõ.
  - Nếu câu trả lời đã hoàn hảo hoặc đã đạt số lượt hỏi xoáy tối đa: AI kết thúc câu hỏi hiện tại và chuyển sang câu hỏi tiếp theo.
- **Tiêu chuẩn kiểm thử:**
  - Câu hỏi hỏi xoáy bám sát ngữ cảnh câu trả lời của sinh viên, không lặp lại nguyên văn câu hỏi chính.
  - Tổng số lượt hỏi xoáy không vượt quá giới hạn $N_{max}$ của câu hỏi đó.

#### FR-015: Kiểm soát thời gian từng lượt trả lời (Turn Timeout Management)
- **Mô tả:** Hệ thống đếm ngược thời gian trả lời cho từng lượt hỏi. Nếu sinh viên không trả lời trước khi hết giờ (timeout), hệ thống ghi nhận câu trả lời rỗng và chuyển bước.
- **Tiêu chuẩn kiểm thử:**
  - Đồng hồ đếm ngược hiển thị trực quan; tự động lưu và chuyển câu khi hết thời gian.

#### FR-016: Kết thúc phiên thi và đóng gói dữ liệu
- **Mô tả:** Khi sinh viên trả lời hết các câu hỏi chính và câu hỏi xoáy (hoặc hết thời lượng ca thi), hệ thống phát thông báo kết thúc phiên thi, chuyển trạng thái bài thi sang `SUBMITTED` và kích hoạt tiến trình chấm điểm tự động.
- **Tiêu chuẩn kiểm thử:**
  - Trạng thái phiên thi chuyển từ `IN_PROGRESS` sang `SUBMITTED`; sinh viên không thể gửi thêm âm thanh.

---

### 5.4. Nhóm 4: Hỗ Trợ Chấm Điểm Bằng AI (AI-Assisted Grading & Human-in-the-Loop)

#### FR-017: Đối chiếu Transcript với Rubric & Gợi ý điểm số
- **Mô tả:** Mô hình AI phân tích toàn bộ chuỗi đối thoại (câu hỏi chính + câu trả lời + các lượt hỏi xoáy) của từng câu, đối chiếu từng tiêu chí Rubric để sinh ra điểm gợi ý thành phần và điểm gợi ý tổng.
- **Tiêu chuẩn kiểm thử:**
  - Điểm gợi ý cho từng tiêu chí không vượt quá điểm tối đa của tiêu chí đó.
  - Tổng điểm gợi ý nằm trong thang điểm quy định (0.0 đến 10.0).

#### FR-018: Sinh nhận xét định tính chi tiết (Strengths, Weaknesses, Missing Concepts)
- **Mô tả:** AI trích xuất bằng chứng từ transcript và sinh nhận xét có cấu trúc:
  - Điểm mạnh (`strengths`): Ý tưởng chính xác, ví dụ minh họa tốt.
  - Điểm yếu & Ý còn thiếu (`weaknesses` / `missing_concepts`): Khái niệm bị hiểu sai hoặc bỏ sót.
  - Ghi nhận tín hiệu phụ: Thời gian phản hồi, độ trôi chảy / lưu loát.
- **Tiêu chuẩn kiểm thử:**
  - Nhận xét viện dẫn được câu nói cụ thể của sinh viên làm bằng chứng thay vì nhận xét chung chung.

#### FR-019: Giao diện thẩm định điểm của Giảng viên (Lecturer Grading Review)
- **Mô tả:** Giảng viên truy cập màn hình chấm thi, xem toàn bộ transcript đồng bộ với trình phát âm thanh (Audio Player), xem bảng điểm gợi ý và nhận xét của AI. Giảng viên có quyền:
  - Giữ nguyên hoặc chỉnh sửa điểm từng tiêu chí Rubric.
  - Chỉnh sửa hoặc bổ sung nhận xét cá nhân của giảng viên.
- **Tiêu chuẩn kiểm thử:**
  - Mọi thao tác sửa đổi điểm số của giảng viên đều được lưu lại mà không ghi đè mất điểm số ban đầu do AI gợi ý.

#### FR-020: Phê duyệt điểm chính thức & Quản lý trạng thái (Final Grade Approval)
- **Mô tả:** Giảng viên xác nhận chốt điểm (`FINALIZED`) và quyết định thời điểm công bố điểm (`PUBLISHED`) cho sinh viên.
- **Tiêu chuẩn kiểm thử:**
  - Điểm số chỉ được xem là điểm hợp lệ chính thức khi có xác nhận của giảng viên.
  - Hệ thống khóa quyền chỉnh sửa khi kỳ thi đã chuyển sang trạng thái lưu trữ chính thức.

---

### 5.5. Nhóm 5: Giám Sát, Bằng Chứng & Minh Bạch (Monitoring, Evidence & Transparency)

#### FR-021: Ghi âm và lưu trữ âm thanh phiên thi
- **Mô tả:** Toàn bộ âm thanh phát ra từ AI và lời nói của sinh viên được thu âm và lưu trữ an toàn trong kho lưu trữ dữ liệu tập trung, phân đoạn theo từng câu hỏi.
- **Tiêu chuẩn kiểm thử:**
  - File âm thanh có thể phát lại được với chất lượng rõ ràng; liên kết chính xác với ID của từng câu hỏi trong phiên thi.

#### FR-022: Nhật ký lưu vết kiểm toán bất biến (Immutable Audit Log)
- **Mô tả:** Hệ thống tự động ghi nhật ký mọi sự kiện quan trọng trong phiên thi: thời điểm bắt đầu/kết thúc, IP truy cập, câu hỏi được chọn, transcript bóc băng, điểm AI gợi ý, thay đổi điểm của giảng viên kèm lý do.
- **Tiêu chuẩn kiểm thử:**
  - Dữ liệu audit log không thể bị chỉnh sửa hay xóa bởi người dùng thông thường.

#### FR-023: Quản lý và xử lý đơn khiếu nại điểm (Grade Appeal Workflow)
- **Mô tả:** Cho phép sinh viên gửi yêu cầu khiếu nại điểm đối với câu hỏi cụ thể kèm lý do. Hệ thống tự động đính kèm transcript, audio và barem chấm gửi tới Giảng viên phụ trách để xem xét phúc khảo.
- **Tiêu chuẩn kiểm thử:**
  - Giảng viên nhận được thông báo khiếu nại, xem lại đúng phân đoạn bằng chứng bị khiếu nại và cập nhật kết luận phúc khảo.

---

### 5.6. Nhóm 6: Phản Hồi & Báo Cáo (Feedback & Reporting)

#### FR-024: Báo cáo phản hồi chi tiết cho Sinh viên (Student View)
- **Mô tả:** Sau khi điểm được `PUBLISHED`, sinh viên có thể xem:
  - Điểm tổng kết chính thức và điểm chữ.
  - Điểm chi tiết từng câu hỏi theo tiêu chí Rubric.
  - Nhận xét chi tiết về điểm mạnh, điểm yếu và kiến thức cần ôn luyện lại.
  - Xem lại transcript cuộc hội thoại.
- **Tiêu chuẩn kiểm thử:**
  - Sinh viên chỉ xem được báo cáo của chính mình; không xem được điểm của sinh viên khác.
  - Không hiển thị báo cáo nếu giảng viên chưa công bố (`PUBLISHED`).

#### FR-025: Bảng điều khiển phân tích lớp học cho Giảng viên (Class Analytics Dashboard)
- **Mô tả:** Giảng viên xem các chỉ số thống kê toàn diện của ca thi/lớp học:
  - Biểu đồ phân bố điểm số (Histogram / Bell curve), điểm trung bình, trung vị, độ lệch chuẩn.
  - Tỷ lệ Đạt (Pass) / Không đạt (Fail).
  - Tỷ lệ đồng thuận giữa AI và Giảng viên (AI Concordance Rate).
- **Tiêu chuẩn kiểm thử:**
  - Thống kê tự động cập nhật ngay khi giảng viên hoàn tất chấm điểm các bài thi trong ca.

#### FR-026: Phân tích độ khó câu hỏi & Năng lực theo thang Bloom
- **Mô tả:** Hệ thống tự động xếp hạng các câu hỏi có tỷ lệ sinh viên trả lời kém nhất; trực quan hóa năng lực của sinh viên theo các mức độ nhận thức Bloom (*Nhớ vs Hiểu vs Vận dụng vs Phân tích*).
- **Tiêu chuẩn kiểm thử:**
  - Hiển thị danh sách Top 5 câu hỏi khó nhất để giảng viên nắm bắt lỗ hổng kiến thức chung của lớp.

#### FR-027: Xuất bảng điểm và báo cáo chuẩn đào tạo (Grade Export Engine)
- **Mô tả:** Hệ thống hỗ trợ xuất bảng điểm ra file định dạng Excel (.xlsx) theo đúng mẫu chuẩn của phòng đào tạo (Mã SV, Họ tên, Điểm thành phần, Điểm tổng kết) và xuất báo cáo tóm tắt ra file PDF.
- **Tiêu chuẩn kiểm thử:**
  - File Excel xuất ra mở được trên Microsoft Excel, đúng cấu trúc cột và định dạng số thập phân chuẩn.

---

### 5.7. Nhóm 7: Quản Trị Hệ Thống & Cấu Hình (System Administration)

#### FR-028: Quản lý người dùng & Phân quyền dựa trên vai trò (RBAC)
- **Mô tả:** Quản trị viên quản lý danh sách tài khoản, tạo mới, khóa/mở khóa tài khoản, phân quyền vai trò (`ADMIN`, `LECTURER`, `STUDENT`).
- **Tiêu chuẩn kiểm thử:**
  - Người dùng chỉ truy cập được các chức năng được phân quyền tương ứng với vai trò.

#### FR-029: Phân công môn học và quyền hạn giảng viên
- **Mô tả:** Quản trị viên phân công giảng viên phụ trách các môn học cụ thể. Giảng viên chỉ có quyền tạo câu hỏi, tổ chức thi và chấm điểm trên các môn học được phân công.
- **Tiêu chuẩn kiểm thử:**
  - Giảng viên A không thể xem hoặc can thiệp vào kỳ thi/ngân hàng câu hỏi của môn học do Giảng viên B phụ trách nếu không được phân quyền.

#### FR-030: Cấu hình tham số hệ thống & Ngôn ngữ (System & Language Configuration)
- **Mô tả:** Quản trị viên có thể cấu hình lựa chọn dịch vụ STT/TTS, cấu hình ngôn ngữ thi mặc định (Tiếng Việt / Tiếng Anh) và điều chỉnh các ngưỡng tham số (ngưỡng VAD silence timeout, thời lượng thi tối đa).
- **Tiêu chuẩn kiểm thử:**
  - Thay đổi cấu hình có hiệu lực tức thì đối với các phiên thi mới tạo mà không cần khởi động lại toàn bộ hệ thống.

---

## 6. Yêu Cầu Phi Chức Năng (Non-Functional Requirements)

### NFR-001: Độ trễ hội thoại thời gian thực (Conversational Latency)
- **Đặc tả:** Tổng độ trễ tính từ lúc sinh viên dứt lời (VAD phát hiện im lặng) $\rightarrow$ STT bóc băng $\rightarrow$ LLM sinh câu hỏi tiếp theo $\rightarrow$ TTS phát ra âm thanh đầu tiên phải đạt mức:
  - Mục tiêu (Target): $\le 2.0$ giây.
  - Ngưỡng tối đa cho phép (Hard Limit): $\le 3.5$ giây trong điều kiện mạng bình thường.
- **Phương pháp đo lường:** Ghi log dấu thời gian tại từng trạm xử lý trong AI pipeline (VAD finish, STT complete, LLM first token, TTS audio chunk delivered).

### NFR-002: Độ chính xác nhận dạng giọng nói tiếng Việt (Speech Recognition Accuracy)
- **Đặc tả:** Mô hình STT phải đạt tỷ lệ lỗi từ (Word Error Rate - WER) dưới 15% đối với các câu trả lời chứa thuật ngữ kỹ thuật của môn học trong điều kiện âm thanh phòng thi tiêu chuẩn.
- **Phương pháp đo lường:** Đánh giá benchmark trên tập test audio 100 câu trả lời kỹ thuật có kèm ground-truth transcript.

### NFR-003: Bảo mật & Quyền riêng tư dữ liệu giáo dục (Data Privacy & Encryption)
- **Đặc tả:** 
  - Toàn bộ dữ liệu truyền tải (âm thanh, văn bản) bắt buộc mã hóa qua HTTPS/TLS 1.3 và Secure WebSockets (WSS).
  - Dữ liệu âm thanh và transcript lưu trữ ở trạng thái nghỉ (At-Rest) phải được mã hóa theo chuẩn AES-256.
  - Sinh viên chỉ có quyền truy cập dữ liệu của chính mình; quyền truy cập file ghi âm của giảng viên bị giới hạn trong phạm vi môn học phụ trách.

### NFR-004: Khả năng chịu lỗi & Không mất mát dữ liệu thi (Fault Tolerance & Reliability)
- **Đặc tả:** Nếu xảy ra sự cố gián đoạn mạng hoặc lỗi sập dịch vụ AI trong lúc sinh viên đang thi:
  - Hệ thống phải lưu vết tức thời câu trả lời của các câu hỏi trước đó vào cơ sở dữ liệu.
  - Cho phép sinh viên kết nối lại (reconnect) trong vòng 5 phút để tiếp tục làm câu hỏi đang dở mà không phải thi lại từ đầu.
- **Phương pháp đo lường:** Kiểm thử giả lập ngắt mạng client (Chaos testing) trong lúc đang thi viva.

### NFR-005: Tính kiểm toán & An toàn học thuật AI (AI Safety & Auditability)
- **Đặc tả:** Mọi nội dung do AI sinh ra (câu hỏi, điểm gợi ý, nhận xét) phải được đánh dấu nhãn nguồn gốc (`origin: AI_GENERATED`). Nghiêm cấm mọi tiến trình tự động chuyển điểm AI thành điểm chính thức nếu không có chữ ký số hoặc thao tác phê duyệt của giảng viên.

### NFR-006: Khả năng mở rộng đồng thời (Scalability)
- **Đặc tả:** Kiến trúc hệ thống phải hỗ trợ tối thiểu **50 phiên thi viva tương tác âm thanh đồng thời** (concurrent viva sessions) trên cấu hình máy chủ mục tiêu mà không làm tăng độ trễ NFR-001 quá 20%.

### NFR-007: Tính tương thích đa nền tảng (Compatibility)
- **Đặc tả:** Giao diện Web của sinh viên phải hoạt động mượt mà trên các trình duyệt hiện đại phổ biến (Google Chrome, Microsoft Edge, Mozilla Firefox, Apple Safari) hỗ trợ chuẩn HTML5 Web Audio API và MediaStream Recording API mà không cần cài đặt plugin ngoài.

### NFR-008: Kiểm soát truy cập & Nhật ký bất biến (Authorization & Immutable Audit)
- **Đặc tả:** Mọi thao tác tạo, sửa, xóa câu hỏi, can thiệp điểm số bắt buộc phải được ghi vào bảng Audit Log với định danh người thực hiện, địa chỉ IP và dấu thời gian chuẩn UTC. Dữ liệu này chỉ cho phép chèn (`INSERT-ONLY`), không cho phép cập nhật (`UPDATE`) hay xóa (`DELETE`).

---

## 7. Các Giả Định, Rủi Ro & Biện Pháp Giảm Thiểu

### 7.1. Danh mục giả định (Assumptions)
- `[ASSUMPTION — NEEDS CONFIRMATION: DATA_RETENTION]`: Dữ liệu âm thanh và transcript của sinh viên được lưu trữ tối thiểu trong thời gian **1 học kỳ (6 tháng)** phục vụ khiếu nại, sau đó được nén lưu trữ lạnh (Cold Storage) hoặc xóa theo chính sách của nhà trường.
- `[ASSUMPTION — NEEDS CONFIRMATION: APPEAL_WINDOW]`: Thời hạn tối đa sinh viên được phép nộp đơn khiếu nại điểm là **3 ngày làm việc** kể từ thời điểm giảng viên bấm công bố điểm (`PUBLISHED`).
- `[ASSUMPTION — NEEDS CONFIRMATION: GRADING_SCALE]`: Hệ thống áp dụng thang điểm 10 tiêu chuẩn (lẻ đến 0.25 hoặc 0.1 điểm), hỗ trợ quy đổi sang thang điểm chữ (A, B, C, D, F) và thang điểm 4.0 theo quy chế đào tạo tín chỉ.
- `[ASSUMPTION — NEEDS CONFIRMATION: AUTH_METHOD]`: Phiên bản thử nghiệm sử dụng xác thực tài khoản nội bộ (JWT); kiến trúc được thiết kế dạng module mở để sẵn sàng cắm nối với Google Workspace / Microsoft 365 Education của trường học.

### 7.2. Phân tích rủi ro & Biện pháp giảm thiểu (Risks & Mitigations)

| Rủi ro (Risk) | Mức độ | Biện pháp giảm thiểu (Mitigation Strategy) |
|---|---|---|
| **R-1: AI sinh câu hỏi lạc đề hoặc ảo giác (Hallucination)** | Cao | Bắt buộc áp dụng RAG với bộ lọc trích xuất nguồn từ giáo trình; bắt buộc giảng viên phải bấm duyệt (`APPROVED`) trước khi câu hỏi được đưa vào ngân hàng thi. |
| **R-2: STT nhận diện sai thuật ngữ chuyên ngành tiếng Việt** | Cao | Nạp danh mục từ điển thuật ngữ môn học (*Domain Terminology Prompting / Vocabulary Boosting*) vào request của STT engine; hiển thị transcript để giảng viên đối chiếu âm thanh khi chấm. |
| **R-3: Sinh viên khiếu nại vì AI chấm quá khắt khe hoặc thiên lệch** | Rất cao | Áp dụng triệt để nguyên tắc Human-in-the-loop: Điểm AI chỉ là gợi ý, giảng viên là người chốt điểm cuối cùng; sinh viên được cung cấp toàn văn transcript và barem Rubric để minh bạch hóa. |
| **R-4: Nghẽn mạng hoặc đứt kết nối trong lúc thi vấn đáp** | Trung bình | Lưu trạng thái theo từng turn; cung cấp cơ chế khôi phục phiên (session recovery) cho phép sinh viên kết nối lại trong thời gian cho phép. |
| **R-5: Chi phí gọi API AI (LLM / STT / TTS) tăng cao khi mở rộng** | Trung bình | Tối ưu prompt súc tích, lưu cache embedding câu hỏi, áp dụng mô hình mã nguồn mở tối ưu (Whisper local, TTS chuyên dụng) để giảm phụ thuộc API trả phí ngoài. |

---

## 8. Tiêu Chuẩn Nghiệm Thu Tổng Thể (Overall Acceptance Criteria)
Hệ thống AIVES được xem là hoàn thành yêu cầu giai đoạn PRD khi:
1. Đạt 100% độ bao phủ (coverage) cho 30 yêu cầu chức năng (`FR-001` đến `FR-030`).
2. Luồng viva tương tác âm thanh hai chiều giữa sinh viên và AI hoạt động ổn định trong phạm vi độ trễ $\le 3.5$ giây.
3. Không có bất kỳ điểm số nào được công bố cho sinh viên mà thiếu thao tác duyệt của giảng viên.
4. Xuất được file bảng điểm Excel khớp hoàn toàn với cấu trúc mẫu của phòng đào tạo.
