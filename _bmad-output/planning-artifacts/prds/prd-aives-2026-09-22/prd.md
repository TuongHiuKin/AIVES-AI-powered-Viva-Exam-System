---
title: "AIVES — AI-powered Viva Exam System (BRD / PRD)"
status: "draft"
created: "2026-09-22"
updated: "2026-09-22"
author: "ANKIN (via BMad PM John & Analyst Mary)"
project: "AIVES_AI_powered_Viva_Exam_System"
---

# PRD / BRD: Hệ Thống Thi Vấn Đáp Thông Minh Ứng Dụng AI (AIVES)
**Tên tiếng Anh:** AI-powered Viva Exam System (AIVES)  
**Tài liệu:** Business & Product Requirements Document (BRD/PRD)  
**Phiên bản:** 1.0 (Draft)

---

## 0. Mục Đích Tài Liệu (Document Purpose)
Tài liệu này đặc tả toàn diện yêu cầu nghiệp vụ (Business Requirements) và yêu cầu sản phẩm (Product Requirements) cho hệ thống **AIVES**. Tài liệu phục vụ cho:
- **Đội ngũ phát triển (Developers, AI Engineers):** Hiểu rõ phạm vi, các ca sử dụng, giao ước dữ liệu và luồng nghiệp vụ.
- **Giảng viên và Nhà trường (Stakeholders):** Thẩm định tính khả thi, tính minh bạch học thuật và giá trị đổi mới trong thi cử.
- **Chuyên viên QA/Kiểm thử:** Làm căn cứ thiết kế Test Cases và Acceptance Criteria.

Tài liệu đặc biệt đào sâu vào **Nhóm chức năng 6 (Phản hồi & Báo cáo)** và thiết lập các giao ước dữ liệu tiên quyết (Data Contracts) từ các nhóm chức năng 1, 2, 3, 4, 5 để phục vụ chiến lược triển khai độc lập (Decoupled / Contract-First Development).

---

## 1. Tầm Nhìn & Bối Cảnh (Vision & Problem Statement)

### 1.1. Nỗi đau thực tế (Problem Statement)
Thi vấn đáp (*viva / oral exam*) là hình thức đánh giá năng lực tư duy phản biện, lập luận và bảo vệ đồ án hiệu quả nhất. Tuy nhiên, hình thức này đang đối mặt với 4 rào cản nghiêm trọng:
1. **Tốn kém nguồn lực giảng viên:** Để vấn đáp 100 sinh viên (mỗi em 15 phút), hội đồng cần từ 25 - 30 giờ làm việc liên tục, dẫn đến quá tải và mệt mỏi.
2. **Thiếu chuẩn hóa:** Độ khó câu hỏi và mức độ khắt khe của thang điểm bị dao động lớn giữa các phòng thi, các ca thi và giữa các giảng viên khác nhau.
3. **Không thể mở rộng (Scalability bottleneck):** Rất khó áp dụng thi vấn đáp cho các môn đại cương hoặc chuyên ngành có sĩ số từ 500 - 1000 sinh viên.
4. **Thiếu bằng chứng khách quan khi khiếu nại điểm:** Khi sinh viên thắc mắc điểm số, hội đồng thường không có bản bóc băng chi tiết (*transcript*) hay barem chấm điểm từng câu để đối chiếu giải trình.

### 1.2. Tầm nhìn giải pháp AIVES (Solution Vision)
AIVES là nền tảng thi vấn đáp thông minh ứng dụng AI:
- **Giám khảo ảo AI (Virtual AI Examiner):** Tự động đặt câu hỏi bằng giọng nói (TTS), lắng nghe sinh viên trả lời (STT), và quan trọng nhất là **sinh câu hỏi hỏi xoáy / đào sâu thích ứng (Adaptive Follow-up)** theo thời gian gần thực dựa trên câu trả lời của sinh viên.
- **Hỗ trợ chấm điểm Rubric (AI-Assisted Grading):** AI phân tích transcript, đối chiếu Rubric để đề xuất điểm và nhận xét chi tiết; giảng viên giữ quyền quyết định cuối cùng (**Human-in-the-loop**).
- **Lưu vết minh bạch:** Ghi âm, ghi hình, lưu trọn vẹn lịch sử đối thoại làm bằng chứng học thuật bất biến.
- **Phản hồi sâu sắc & Thống kê diện rộng:** Sinh viên nhận báo cáo chi tiết điểm mạnh/yếu; Giảng viên nhận phân tích phổ điểm, chất lượng câu hỏi theo thang Bloom và xuất bảng điểm chuẩn trường học.

---

## 2. Đối Tượng Người Dùng & Hành Trình Trải Nghiệm (Target Users & Journeys)

### 2.1. Jobs To Be Done (JTBD)
- **Giảng viên (Lecturer / Examiner):**
  - *Functional:* Muốn tạo ngân hàng câu hỏi nhanh chóng từ slide/giáo trình, tổ chức thi tự động cho hàng trăm sinh viên mà không tốn công phỏng vấn từng người.
  - *Trust & Control:* Muốn AI chỉ là trợ lý chấm điểm gợi ý, bản thân vẫn giữ toàn quyền kiểm soát và phê duyệt điểm số cuối cùng.
  - *Analytical:* Muốn nhìn thấy ngay câu hỏi nào sinh viên hay trả lời sai nhất để cải tiến bài giảng.
- **Sinh viên (Student / Candidate):**
  - *Functional:* Muốn trải nghiệm thi vấn đáp công bằng, tự nhiên qua giọng nói tiếng Việt, không bị áp lực tâm lý từ giám khảo khó tính.
  - *Transparency:* Muốn nhận được phản hồi chi tiết sau khi thi (tại sao được điểm đó, sai ở đâu) thay vì chỉ nhận một con số điểm vô hồn.
- **Phòng Đào tạo / Trưởng Bộ môn (Admin / Academic Head):**
  - *Compliance:* Muốn có bằng chứng đầy đủ (transcript + audio) khi xử lý đơn khiếu nại điểm của sinh viên.
  - *Integration:* Muốn xuất bảng điểm khớp với mẫu chuẩn Excel của nhà trường để nhập vào hệ thống quản lý đào tạo (SIS).

### 2.2. Hành Trình Người Dùng Then Chốt (Key User Journeys)

- **UJ-1: Giảng viên chuẩn bị ngân hàng câu hỏi & kỳ thi**
  - Giảng viên tải lên slide bài giảng PDF, AI chạy RAG sinh ra bộ câu hỏi vấn đáp kèm mức độ Bloom và tiêu chí Rubric. Giảng viên duyệt/sửa câu hỏi, sau đó tạo phòng thi và danh sách sinh viên.
- **UJ-2: Sinh viên trải nghiệm phỏng vấn vấn đáp với AI**
  - Sinh viên vào phòng thi ảo, kiểm tra micro. AI đọc câu hỏi chính bằng giọng nói tiếng Việt tự nhiên. Sinh viên trả lời bằng giọng nói. AI phân tích ngữ nghĩa, phát hiện sinh viên giải thích thiếu một khái niệm cốt lõi, liền lập tức "hỏi xoáy": *"Bạn vừa nhắc đến thuật ngữ X, vậy trong trường hợp Y thì X hoạt động như thế nào?"*. Sinh viên trả lời tiếp. Sau 2-3 câu hỏi, buổi thi kết thúc.
- **UJ-3: Giảng viên thẩm định và chốt điểm (Human-in-the-loop Grading)**
  - Giảng viên mở màn hình chấm thi, xem danh sách sinh viên. Bấm vào một sinh viên, hệ thống hiển thị toàn bộ transcript hỏi - đáp kèm điểm AI gợi ý theo từng tiêu chí Rubric. Giảng viên nghe lại 1 đoạn audio nghi vấn, điều chỉnh điểm từ 7.5 lên 8.0, bấm **"Chốt điểm & Công bố"**.
- **UJ-4: Sinh viên xem báo cáo phản hồi cá nhân (Feature 6.1)**
  - Sau khi điểm được công bố, sinh viên nhận thông báo, mở trang báo cáo cá nhân. Sinh viên thấy điểm tổng, bảng điểm chi tiết từng câu, đọc nhận xét của AI về điểm mạnh/điểm yếu và các phần kiến thức cần ôn luyện lại.
- **UJ-5: Giảng viên xem thống kê toàn lớp & xuất bảng điểm (Feature 6.2)**
  - Kết thúc đợt thi, Giảng viên vào Dashboard thống kê, xem biểu đồ phổ điểm hình chuông, xem danh sách top 3 câu hỏi có tỷ lệ trả lời đúng thấp nhất (để phụ đạo thêm trên lớp), và bấm **"Xuất bảng điểm Excel theo mẫu trường"**.

---

## 3. Thuật Ngữ & Từ Điển Nghiệp Vụ (Glossary)

- **Viva Exam (Thi vấn đáp):** Hình thức thi vấn đáp trực tiếp qua đối thoại ngôn ngữ giữa giám khảo và thí sinh.
- **Bloom Taxonomy (Thang Bloom):** Khung phân loại nhận thức gồm 6 cấp độ: *Nhớ (Remember), Hiểu (Understand), Vận dụng (Apply), Phân tích (Analyze), Đánh giá (Evaluate), Sáng tạo (Create)*. Trong AIVES ưu tiên từ cấp 2 đến cấp 4.
- **Rubric:** Bảng tiêu chí đánh giá gồm danh sách các tiêu chí thành phần (ví dụ: *Độ chính xác khái niệm, Khả năng lập luận phản biện, Độ trôi chảy/thuật ngữ*) kèm trọng số và mô tả mức độ đạt được.
- **Adaptive Follow-up (Hỏi xoáy / Đào sâu thích ứng):** Khả năng của LLM sinh câu hỏi nối tiếp dựa trên ngữ cảnh câu trả lời vừa phát ra của sinh viên để kiểm tra tính hiểu sâu hoặc phát hiện gian lận học vẹt.
- **Speech-to-Text (STT):** Công nghệ nhận dạng giọng nói chuyển thành văn bản tiếng Việt.
- **Text-to-Speech (TTS):** Công nghệ tổng hợp giọng nói tiếng Việt đọc câu hỏi thi.
- **Transcript:** Toàn văn văn bản bóc băng cuộc hội thoại giữa AI và sinh viên, gắn nhãn thời gian từng lượt nói (*turn*).
- **AI Suggested Score:** Điểm số và nhận xét do AI đề xuất dựa trên đối chiếu transcript với Rubric.
- **Final Grade:** Điểm chính thức do giảng viên xác nhận hoặc điều chỉnh sau khi review.
- **Grade Appeal (Khiếu nại điểm):** Quy trình sinh viên yêu cầu phúc khảo, đính kèm bằng chứng transcript và ghi âm.

---

## 4. Đặc Tả Chi Tiết Tính Năng (Features & Functional Requirements)

### 4.1. Nhóm 1: Quản Lý Ngân Hàng Câu Hỏi & Rubric
**Mô tả:** Cung cấp công cụ xây dựng, chuẩn hóa kho câu hỏi và barem chấm điểm đa tiêu chí.

- **FR-1 [Tạo & Quản lý câu hỏi thủ công]:** Giảng viên có thể thêm, sửa, xóa, tìm kiếm câu hỏi; gắn thẻ môn học, chủ đề, thời lượng trả lời tối đa (giây) và số lượt hỏi xoáy tối đa (max turns).
- **FR-2 [AI Sinh câu hỏi RAG từ tài liệu]:** Hệ thống cho phép tải tài liệu bài giảng (PDF, DOCX, PPTX), thực hiện trích xuất và sinh câu hỏi theo chủ đề được chỉ định.
- **FR-3 [Phân loại Bloom & Gán Rubric]:** Mỗi câu hỏi bắt buộc được gán nhãn cấp độ Bloom và liên kết với một bảng Rubric gồm ít nhất 2 tiêu chí thành phần kèm trọng số điểm.
- **FR-4 [Quy trình duyệt câu hỏi]:** Câu hỏi do AI sinh ra phải ở trạng thái `PENDING_REVIEW`. Chỉ câu hỏi được giảng viên bấm `APPROVED` mới được đưa vào ngân hàng thi chính thức.

---

### 4.2. Nhóm 2: Quản Lý Kỳ Thi & Lịch Thi
**Mô tả:** Thiết lập cấu hình ca thi, phân bổ đề thi ngẫu nhiên thích ứng và điều phối danh sách thí sinh.

- **FR-5 [Tạo ca thi]:** Giảng viên tạo kỳ thi gắn với môn học, xác định danh sách sinh viên (nhập từ Excel/CSV), khung giờ thi và số lượng câu hỏi chính mỗi thí sinh phải trả lời (mặc định: 2 - 3 câu).
- **FR-6 [Bốc đề ngẫu nhiên thông minh]:** Hệ thống tự động chọn tổ hợp câu hỏi ngẫu nhiên cho từng sinh viên, đảm bảo các sinh viên thi liên tiếp không bị lặp lại bộ câu hỏi giống nhau và cân bằng độ khó theo thang Bloom.
- **FR-7 [Cấp mã truy cập phiên thi]:** Mỗi sinh viên nhận một mã phòng thi / token định danh duy nhất cho phiên thi của mình.

---

### 4.3. Nhóm 3: Lõi Phỏng Vấn AI (AI Viva Core - Cốt Lõi Dự Án)
**Mô tả:** Giám khảo ảo AI tương tác âm thanh hai chiều theo thời gian gần thực với thí sinh.

- **FR-8 [Đọc câu hỏi qua TTS]:** AI chuyển câu hỏi thành giọng đọc tiếng Việt rõ ràng, tự nhiên, hỗ trợ tạm dừng và phát lại nếu sinh viên yêu cầu (tối đa 1 lần phát lại).
- **FR-9 [Thu âm & Bóc băng STT]:** Hệ thống ghi nhận âm thanh trả lời của sinh viên và chuyển đổi thành văn bản tiếng Việt theo thời gian gần thực (Near Real-time STT).
- **FR-10 [Phát hiện kết thúc câu trả lời]:** Hệ thống tự động phát hiện sinh viên đã nói xong (Voice Activity Detection - VAD) khi có khoảng lặng liên tục từ 2.5 - 3 giây, hoặc sinh viên bấm nút "Hoàn thành câu trả lời".
- **FR-11 [Sinh câu hỏi đào sâu thích ứng - Adaptive Follow-up]:** Dựa trên transcript câu trả lời và đáp án chuẩn/rubric:
  - Nếu câu trả lời chưa rõ ràng, thiếu ý quan trọng hoặc có mâu thuẫn logic: AI kích hoạt câu hỏi hỏi xoáy để làm rõ.
  - Số lượt hỏi xoáy bị giới hạn bởi cấu hình của câu hỏi (mặc định tối đa: 1 - 2 lượt).
- **FR-12 [Kiểm soát thời lượng phiên thi]:** Đếm ngược thời gian cho từng lượt trả lời. Tự động chuyển câu nếu hết thời gian quy định (Timeout).

---

### 4.4. Nhóm 4: Hỗ Trợ Chấm Điểm Bằng AI (AI-Assisted Grading & Human-in-the-Loop)
**Mô tả:** Đối chiếu transcript với Rubric, đưa ra gợi ý chấm điểm và tạo không gian làm việc cho giảng viên chốt điểm.

- **FR-13 [Chấm điểm tự động theo Rubric]:** LLM phân tích toàn bộ chuỗi hội thoại (câu hỏi chính + câu trả lời + các lượt hỏi xoáy), đối chiếu từng tiêu chí Rubric để sinh ra:
  - Điểm gợi ý cho từng tiêu chí thành phần (AI Suggested Sub-scores).
  - Điểm tổng gợi ý (AI Suggested Total Score).
- **FR-14 [Sinh nhận xét điểm mạnh / điểm yếu]:** AI tạo bản phân tích định tính:
  - Điểm mạnh (`strengths`): Ý tưởng hay, giải thích đúng trọng tâm.
  - Điểm yếu & Ý thiếu (`weaknesses` / `missing_concepts`): Khái niệm sai lệch hoặc bỏ sót.
  - Gợi ý bổ sung (`recommendations`).
- **FR-15 [Màn hình chấm thi Giảng viên - Human-in-the-loop]:** Giảng viên duyệt danh sách bài thi, xem song song Transcript, Audio player và Bảng điểm gợi ý của AI. Giảng viên có quyền sửa đổi điểm số từng tiêu chí và thêm nhận xét cá nhân.
- **FR-16 [Trạng thái chốt điểm & Khóa dữ liệu]:** Điểm số trải qua các trạng thái: `AI_SUGGESTED` (AI vừa chấm xong) $\rightarrow$ `IN_REVIEW` (Giảng viên đang xem) $\rightarrow$ `FINALIZED` (Giảng viên đã chốt) $\rightarrow$ `PUBLISHED` (Công bố cho sinh viên). Khi đã `PUBLISHED`, sinh viên mới được xem báo cáo.

---

### 4.5. Nhóm 5: Giám Sát, Ghi Âm & Lưu Vết Minh Bạch (Audit Trail)
**Mô tả:** Bảo toàn bằng chứng kỳ thi phục vụ thanh tra đào tạo và giải quyết khiếu nại.

- **FR-17 [Ghi âm toàn bộ phiên thi]:** Lưu trữ file âm thanh chất lượng chuẩn, chia nhỏ theo từng turn hỏi-đáp hoặc lưu trọn vẹn cả phiên thi.
- **FR-18 [Lưu vết bất biến - Immutable Audit Log]:** Lưu lại lịch sử chỉnh sửa điểm của giảng viên (Ai đã sửa điểm, điểm cũ là bao nhiêu, điểm mới là bao nhiêu, lý do sửa điểm).
- **FR-19 [Bảo mật & Phân quyền truy cập bằng chứng]:** File ghi âm và transcript được mã hóa lưu trữ; chỉ sinh viên sở hữu và giảng viên phụ trách môn học mới có quyền truy cập.

---

### 4.6. Nhóm 6: Phản Hồi & Báo Cáo (Feedback & Reporting - Trọng Tâm Triển Khai)
**Mô tả:** Cung cấp cái nhìn đa chiều về kết quả học tập cho người học và chất lượng kỳ thi cho nhà trường.

#### Phân hệ 6.1: Báo cáo phản hồi cho Sinh viên (Student View)
- **FR-20 [Xem bảng điểm & Nhận xét chi tiết]:** Khi điểm ở trạng thái `PUBLISHED`, sinh viên xem được:
  - Điểm tổng kết chính thức (thang 10, thang chữ).
  - Bảng điểm chi tiết theo từng câu hỏi và từng tiêu chí Rubric.
  - Nhận xét chi tiết của AI và giảng viên (Điểm sáng, Lỗ hổng kiến thức, Lời khuyên học tập).
  - Xem lại toàn văn Transcript lời thoại của buổi thi.
- **FR-21 [Nộp đơn khiếu nại điểm - Grade Appeal]:** Sinh viên có thể bấm "Khiếu nại điểm" cho một câu hỏi cụ thể, nhập lý do. Hệ thống tự động liên kết transcript và đoạn audio liên quan chuyển vào hộp thư xử lý khiếu nại của Giảng viên.

#### Phân hệ 6.2: Thống kê & Báo cáo cho Giảng viên (Lecturer & Admin View)
- **FR-22 [Thống kê tổng quan lớp học]:** Dashboard hiển thị:
  - Phổ điểm phân bố (Histogram / Bell Curve).
  - Điểm trung bình (Mean), Trung vị (Median), Điểm cao nhất, Điểm thấp nhất.
  - Tỷ lệ Đạt (Pass rate) / Không đạt (Fail rate).
- **FR-23 [Phân tích câu hỏi khó & Năng lực Bloom]:**
  - Danh sách Top câu hỏi khó nhất lớp (dựa trên tỷ lệ đạt điểm dưới 50%).
  - Biểu đồ năng lực theo thang Bloom: So sánh kết quả sinh viên ở mức độ *Hiểu* vs *Vận dụng* vs *Phân tích*.
- **FR-24 [Chỉ số đồng thuận AI vs Giảng viên (AI Concordance Index)]:** Thống kê tỷ lệ phần trăm giảng viên giữ nguyên điểm AI gợi ý so với số lần giảng viên can thiệp chỉnh sửa, và độ lệch điểm trung bình (Mean Absolute Error). Dùng làm căn cứ hiệu chuẩn Rubric.
- **FR-25 [Xuất bảng điểm Excel & Báo cáo PDF]:**
  - Xuất bảng điểm lớp học ra file Excel theo đúng định dạng mẫu của phòng đào tạo (Mã SV, Họ và tên, Lớp, Điểm từng câu, Điểm tổng, Chữ ký).
  - Xuất bản PDF báo cáo tóm tắt kỳ thi phục vụ lưu trữ khoa/bộ môn.

---

### 4.7. Nhóm 7: Quản Trị Hệ Thống (System Administration)
**Mô tả:** Quản lý quyền hạn và cấu hình kỹ thuật hạ tầng.

- **FR-26 [Quản lý người dùng & Phân quyền RBAC]:** Phân biệt 3 vai trò: `STUDENT`, `LECTURER`, `ADMIN`.
- **FR-27 [Phân công môn học]:** Phân quyền giảng viên quản lý câu hỏi và kỳ thi theo từng mã môn học cụ thể.
- **FR-28 [Cấu hình Engine STT/TTS]:** Cho phép chọn mô hình STT (Whisper, FPT.AI, Viettel AI...) và cấu hình giọng đọc TTS tiếng Việt theo ngữ điệu vùng miền (Bắc/Nam).

---

## 5. Các Giới Hạn & Phạm Vi Không Làm Trong V1 (Non-Goals)

- **NON-GOAL 1 [Chấm điểm tự động hoàn toàn không có con người]:** AIVES kiên quyết từ chối việc công bố điểm tự động mà không qua sự phê duyệt của giảng viên (bắt buộc Human-in-the-loop để đảm bảo trách nhiệm đạo đức AI).
- **NON-GOAL 2 [Phân tích cảm xúc & ngôn ngữ cơ thể qua Camera]:** Trong phiên bản v1, hệ thống chỉ tập trung vào âm thanh, ngữ nghĩa văn bản và bóc băng lời nói; không phân tích biểu cảm khuôn mặt hoặc ánh mắt để tránh sai số và phán xét thiên kiến.
- **NON-GOAL 3 [Tích hợp trực tiếp ERP đào tạo trường học thời gian thực]:** Hệ thống chưa đồng bộ tự động 2 chiều qua API trường mà cung cấp chức năng Xuất/Nhập Excel chuẩn mẫu.

---

## 6. Phạm Vi MVP (MVP Scope Definition)

### 6.1. In Scope cho MVP (Ưu tiên phát triển)
1. **Module 6 (Báo cáo & Phản hồi):** Hoàn thiện đầy đủ Dashboard sinh viên, Dashboard thống kê giảng viên và Engine xuất file Excel/PDF sử dụng dữ liệu Mock Dataset.
2. **Data Contract & Mock Service:** Bộ khung dữ liệu JSON hoàn chỉnh cho phép các module kết nối không ma sát.
3. **Module 3 (AI Viva Core tối giản):** Giám khảo ảo hỏi đáp giọng nói tiếng Việt cơ bản (hỗ trợ 1 lượt hỏi xoáy).
4. **Module 4 (Chấm điểm Rubric):** LLM đối chiếu rubric và đưa ra gợi ý chấm điểm cho giảng viên duyệt.

### 6.2. Out of Scope cho MVP (Dành cho v2)
- Tự động sinh câu hỏi RAG từ file bài giảng phức tạp (v1 dùng nhập câu hỏi thủ công hoặc import Excel).
- Phân tích video giám sát gian lận góc nghiêng.

---

## 7. Yêu Cầu Phi Chức Năng (Cross-Cutting NFRs)

- **NFR-1 [Độ trễ phản hồi hội thoại (Turn Latency)]:** Thời gian tính từ lúc sinh viên dứt lời (VAD ngắt) $\rightarrow$ STT bóc băng $\rightarrow$ LLM sinh câu hỏi đào sâu $\rightarrow$ TTS bắt đầu phát âm thanh không vượt quá **2.5 giây** để duy trì nhịp đối thoại tự nhiên.
- **NFR-2 [Độ chính xác STT tiếng Việt chuyên ngành]:** Đạt Word Error Rate (WER) dưới 15% đối với các thuật ngữ kỹ thuật phổ biến của môn học.
- **NFR-3 [Bảo mật & Quyền riêng tư (Privacy & Security)]:** Dữ liệu âm thanh của sinh viên phải được lưu trữ trong Bucket an toàn, mã hóa ở trạng thái nghỉ (AES-256) và truyền tải qua HTTPS/TLS 1.3.
- **NFR-4 [Tính mở rộng (Scalability)]:** Hệ thống hỗ trợ tối thiểu 50 phiên thi viva diễn ra đồng thời (concurrent sessions) mà không làm suy giảm độ trễ STT/TTS.

---

## 8. Chỉ Số Thành Công & Đo Lường (Success Metrics)

- **SM-1 [Tiết kiệm thời gian giảng viên]:** Giảm ít nhất **70%** thời gian ngồi phỏng vấn trực tiếp của giảng viên cho mỗi đợt thi.
- **SM-2 [Mức độ hài lòng của sinh viên]:** Điểm đánh giá độ rõ ràng và hữu ích của Báo cáo phản hồi (Feature 6.1) đạt $\ge 4.2 / 5.0$.
- **SM-3 [Tỷ lệ đồng thuận chấm điểm (Concordance Rate)]:** Điểm AI đề xuất và điểm giảng viên chốt chênh lệch không quá 1.0 điểm trên thang 10 ở $\ge 85\%$ số bài thi.
- **SM-C1 [Counter-Metric - Tránh gian lận thời gian]:** Không tối ưu tốc độ kết thúc bài thi bằng cách cắt bớt lượt hỏi xoáy của AI nếu câu trả lời của sinh viên còn mơ hồ.

---

## 9. Danh Mục Câu Hỏi Mở & Giả Định (Open Questions & Assumptions)

### 9.1. Giả định (Assumptions)
- `[ASSUMPTION-1]`: Môi trường thi của sinh viên có đường truyền Internet ổn định và micro hoạt động tốt.
- `[ASSUMPTION-2]`: Nhà trường chấp nhận bảng điểm xuất ra dưới dạng file Excel chuẩn theo định dạng các phòng đào tạo đại học Việt Nam đang dùng.
- `[ASSUMPTION-3]`: Giảng viên là người chịu trách nhiệm pháp lý cuối cùng về điểm số của sinh viên trước hội đồng khoa.

### 9.2. Câu hỏi mở (Open Questions)
- `[QUESTION-1]`: Trường có quy định thời hạn tối đa bao nhiêu ngày sinh viên được phép gửi đơn khiếu nại (Grade Appeal) sau khi công bố điểm? (Đề xuất: 3 ngày làm việc).
- `[QUESTION-2]`: File ghi âm phiên thi cần được lưu trữ trong bao lâu trước khi xóa hoặc chuyển sang cold storage? (Đề xuất: Tối thiểu 1 học kỳ).
