---
title: "AIVES — 09. End-to-End Requirements Traceability Matrix (RTM)"
document_id: "AIVES-DOC-09-RTM"
status: "draft"
version: "1.0.0"
created: "2026-09-22"
updated: "2026-09-22"
project: "AIVES (AI-powered Viva Exam System)"
author: "ANKIN (via BMAD QA & Architect Winston)"
---

# AIVES (AI-powered Viva Exam System)
## Ma Trận Truy Vết Yêu Cầu 6 Chiều (Traceability Matrix)
**Mã tài liệu:** `AIVES-DOC-09-RTM`

---

## 1. Mục Đích & Nguyên Tắc Truy Vết (Traceability Purpose)

Trong một đồ án kỹ thuật phần mềm học thuật, **Ma trận truy vết (Requirements Traceability Matrix - RTM)** là công cụ quan trọng nhất để chứng minh:
1. Mọi yêu cầu nghiệp vụ đều được hiện thực hóa thành Use Case, Mô hình dữ liệu, Giao diện API và Thành phần kiến trúc cụ thể.
2. Không có thành phần code hoặc endpoint API nào được sinh ra vô căn cứ (*no orphan code / endpoints*).
3. Đảm bảo 100% phạm vi nghiệp vụ được kiểm thử và xác thực xuyên suốt toàn bộ vòng đời phát triển.

### Ma trận liên kết 6 chiều:
$$\text{Feature Group} \longrightarrow \text{FR (Yêu cầu chức năng)} \longrightarrow \text{Use Case} \longrightarrow \text{Domain Entity} \longrightarrow \text{API Endpoint} \longrightarrow \text{Architecture Component}$$

---

## 2. Bảng Ma Trận Truy Vết Tổng Thể (Comprehensive 6-Dimensional RTM)

| Nhóm Tính Năng (Feature Group) | Mã Yêu Cầu (FR-ID) & Tên Yêu Cầu | Ca Sử Dụng (Use Case) | Thực Thể Miền (Domain Entity) | Điểm Cuối API (API Endpoint) | Thành Phần Kiến Trúc (Arch Component) |
|---|---|---|---|---|---|
| **FG-1: Ngân hàng câu hỏi & Rubric** | **FR-001**: Tạo câu hỏi thủ công | UC-01 | `Question`, `QuestionBank` | `POST /api/v1/questions` | `MonolithBackend`, `RelationalDB` |
| | **FR-002**: Nhập câu hỏi hàng loạt | UC-01 | `Question`, `QuestionBank` | `POST /api/v1/questions/batch-import` | `MonolithBackend`, `RelationalDB` |
| | **FR-003**: AI sinh câu hỏi RAG | UC-02 | `CourseMaterial`, `Chunk`, `Question` | `POST .../questions/ai-generate` (API-03) | `TaskQueue`, `LLM`, `RelationalDB (pgvector)` |
| | **FR-004**: Gán nhãn Bloom | UC-01, UC-02 | `Question.bloomLevel` | `POST /api/v1/questions` | `MonolithBackend`, `RelationalDB` |
| | **FR-005**: Thiết lập Rubric | UC-01, UC-02 | `Rubric`, `RubricCriterion` | `POST /api/v1/rubrics` | `MonolithBackend`, `RelationalDB` |
| | **FR-006**: Phê duyệt câu hỏi AI | UC-03 | `Question.status` | `PATCH .../questions/{id}/review` (API-04) | `MonolithBackend`, `RelationalDB` |
| **FG-2: Kỳ thi & Lịch thi** | **FR-007**: Tạo ca thi vấn đáp | UC-04 | `Exam`, `ExamSession` | `POST .../exams/{id}/sessions` (API-05) | `MonolithBackend`, `RelationalDB` |
| | **FR-008**: Quản lý danh sách thí sinh | UC-04 | `StudentExamAttempt` | `POST .../sessions/{id}/students` | `MonolithBackend`, `RelationalDB` |
| | **FR-009**: Thuật toán bốc đề ngẫu nhiên | UC-05 | `AttemptQuestion` | Trigger tại WebSocket Start | `VivaWorker`, `RelationalDB` |
| | **FR-010**: Khóa đề Snapshot | UC-05 | `AttemptQuestion`, `AttemptRubricSnapshot` | Internal DB Transaction | `VivaWorker`, `RelationalDB` |
| **FG-3: Lõi Phỏng Vấn AI (Viva Core)** | **FR-011**: TTS đọc câu hỏi giọng nói | UC-06 | `DialogueTurn.aiPromptText` | `AI_QUESTION_START` (API-06) | `VivaWorker`, `TTS Provider`, `WebSPA` |
| | **FR-012**: STT thu âm & bóc băng | UC-07 | `DialogueTurn.studentTranscript` | `STUDENT_AUDIO_CHUNK` (API-06) | `VivaWorker`, `STT Provider`, `WebSPA` |
| | **FR-013**: VAD phát hiện ngắt câu | UC-07 | `DialogueTurn.pauseDurationSeconds` | `STUDENT_SPEECH_FINISHED` (API-06) | `VivaWorker (Silero-VAD)`, `WebSPA` |
| | **FR-014**: Hỏi xoáy thích ứng | UC-08 | `DialogueTurn.turnType = FOLLOW_UP` | `AI_FOLLOWUP_START` (API-06) | `VivaWorker`, `LLM Provider` |
| | **FR-015**: Kiểm soát thời gian Turn | UC-06, UC-07 | `DialogueTurn`, `ExamSession` | Server-side Timer | `VivaWorker`, `Redis`, `WebSPA` |
| | **FR-016**: Đóng gói kết thúc ca thi | UC-07 | `StudentExamAttempt.status = SUBMITTED` | `EXAM_COMPLETED` (API-06) | `VivaWorker`, `RelationalDB` |
| **FG-4: Hỗ trợ Chấm điểm AI** | **FR-017**: So khớp Rubric & Điểm gợi ý | UC-09 | `AttemptRubricSnapshot.aiSuggestedPoints` | Background Task trigger | `VivaWorker`, `LLM Provider`, `RelationalDB` |
| | **FR-018**: Sinh nhận xét điểm mạnh/yếu | UC-09 | `FinalGrade.summaryFeedback`, `aiReasoning` | Background Task trigger | `VivaWorker`, `LLM Provider`, `RelationalDB` |
| | **FR-019**: Màn hình GV chấm điểm | UC-10 | `AttemptQuestion`, `AttemptRubricSnapshot` | `GET .../grading/attempts/{id}` (API-07) | `MonolithBackend`, `RelationalDB`, `WebSPA` |
| | **FR-020**: GV chốt điểm chính thức | UC-11 | `FinalGrade`, `AttemptRubricSnapshot` | `POST .../finalize` (API-08) | `MonolithBackend`, `RelationalDB` |
| **FG-5: Giám sát, Bằng chứng & Audit** | **FR-021**: Ghi âm âm thanh phiên thi | UC-07 | `DialogueTurn.audioRecordingUrl` | Stream S3 Upload | `VivaWorker`, `ObjectStore (MinIO/S3)` |
| | **FR-022**: Nhật ký kiểm toán bất biến | UC-10, UC-11 | `ExamAuditLog` | Trigger / Insert-Only DB | `MonolithBackend`, `RelationalDB` |
| | **FR-023**: Xử lý khiếu nại điểm | UC-12 | `GradeAppeal` | `POST /api/v1/appeals` (API-10) | `MonolithBackend`, `RelationalDB` |
| **FG-6: Phản hồi & Báo cáo** | **FR-024**: Báo cáo cá nhân Sinh viên | UC-12 | `FinalGrade`, `AttemptQuestion`, `DialogueTurn` | `GET /api/v1/results/my-report` (API-09) | `MonolithBackend`, `RelationalDB`, `WebSPA` |
| | **FR-025**: Thống kê lớp học GV | UC-13 | `StudentExamAttempt`, `FinalGrade` | `GET .../analytics/sessions/{id}` (API-11) | `MonolithBackend`, `RelationalDB`, `WebSPA` |
| | **FR-026**: Phân tích Bloom & Câu khó | UC-13 | `AttemptQuestion.snapshotBloomLevel` | `GET .../analytics/sessions/{id}` (API-11) | `MonolithBackend`, `RelationalDB` |
| | **FR-027**: Xuất bảng điểm Excel/PDF | UC-13 | `FinalGrade`, `User`, `Course` | `GET .../export-excel` (API-12) | `MonolithBackend (openpyxl/exceljs)` |
| **FG-7: Quản trị hệ thống** | **FR-028**: Quản trị tài khoản & RBAC | UC-14 | `User`, `User.role` | `GET/POST /api/v1/users` | `MonolithBackend`, `RelationalDB` |
| | **FR-029**: Phân công môn học GV | UC-14 | `CourseLecturer` | `POST .../courses/{id}/lecturers` | `MonolithBackend`, `RelationalDB` |
| | **FR-030**: Cấu hình hệ thống & STT | UC-14 | `SystemConfig` (Environment/DB) | `PUT /api/v1/admin/configs` | `MonolithBackend`, `RelationalDB` |

---

## 3. Ma Trận Truy Vết Yêu Cầu Phi Chức Năng (NFR Traceability)

| Mã NFR | Nội Dung Yêu Cầu Phi Chức Năng | Kỹ Thuật / Giải Pháp Đáp Ứng | Thành Phần Kiến Trúc Phụ Trách | Kiểm Thử / Xác Thực |
|---|---|---|---|---|
| **NFR-001** | Độ trễ hội thoại viva $\le 2.0\text{s}$ (Max $3.5\text{s}$) | WebSockets nhị phân, Silero-VAD ngắt lượt chuẩn, Prompting LLM streaming súc tích. | `VivaWorker`, `WebSockets`, `STT/TTS Providers` | Ghi log thời gian tại từng trạm (Latency breakdown audit log). |
| **NFR-002** | Độ chính xác STT tiếng Việt (WER $< 15\%$) | Domain Vocabulary Boosting (nạp danh mục từ khóa môn học vào request STT). | `VivaWorker`, `STT Adapter` | Chạy bộ test âm thanh 100 câu có ground-truth transcript. |
| **NFR-003** | Bảo mật PII & Mã hóa âm thanh | TLS 1.3, AES-256 At-Rest, Presigned URL S3 hết hạn 15 phút, Ẩn danh PII gửi sang LLM. | `APIGateway`, `ObjectStore`, `MonolithBackend` | Kiểm tra quét bảo mật (Security vulnerability scan). |
| **NFR-004** | Chịu lỗi & Không mất dữ liệu thi | Lưu vết trạng thái theo từng turn vào DB/Redis; hỗ trợ kết nối lại phiên thi trong 5 phút. | `VivaWorker`, `RelationalDB`, `Redis` | Kiểm thử ngắt mạng client (Chaos reconnection test). |
| **NFR-005** | An toàn học thuật & Chống vượt quyền AI | CSDL ràng buộc điểm chính thức bắt buộc phải có `lecturer_id`; thẻ phân lập Prompt Injection. | `RelationalDB Triggers`, `AI Prompt Templates` | Kiểm thử tấn công Prompt Injection và Bypass DB. |
| **NFR-006** | Hỗ trợ 50 phiên thi đồng thời | Kiến trúc Asynchronous I/O (FastAPI/Node async event loop), xử lý audio streaming non-blocking. | `VivaWorker`, `Redis`, `APIGateway` | Kiểm thử tải đồng thời (Load testing qua k6 / Locust). |
| **NFR-007** | Tương thích trình duyệt phổ biến | Sử dụng chuẩn W3C Web Audio API & MediaRecorder tiêu chuẩn. | `WebSPA (Next.js/React)` | Kiểm thử trên Chrome, Edge, Firefox, Safari. |
| **NFR-008** | Nhật ký kiểm toán bất biến | Bảng `exam_audit_logs` cấp quyền `INSERT-ONLY`; thu hồi quyền `UPDATE/DELETE`. | `RelationalDB` | Cố gắng thực hiện câu lệnh `DELETE/UPDATE` trên DB user ứng dụng. |
