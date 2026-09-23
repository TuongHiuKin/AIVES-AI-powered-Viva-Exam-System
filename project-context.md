# AIVES (AI-powered Viva Exam System) — Project Technical Context
**Mã tài liệu:** `AIVES-PROJECT-CONTEXT`  
**Phiên bản:** `1.0.0` (Planning & Architecture Milestone)  
**Ngày hoàn tất:** `2026-09-22`  
**Trạng thái:** `READY FOR REVIEW — NO IMPLEMENTATION CODE WRITTEN`

---

## 1. Tổng Quan Dự Án & Sứ Mệnh (Executive Summary)

AIVES là nền tảng thi vấn đáp (*viva / oral exam*) thông minh ứng dụng AI dành cho bậc đại học.
Hệ thống giải quyết 5 bài toán lớn của thi vấn đáp truyền thống:
1. Giảm thiểu áp lực thời gian của giảng viên khi phải phỏng vấn hàng trăm sinh viên liên tục.
2. Chuẩn hóa ngân hàng câu hỏi theo Thang nhận thức Bloom (*Remember, Understand, Apply, Analyze*).
3. Chuẩn hóa barem chấm điểm đa tiêu chí bằng Rubric.
4. Lõi Giám khảo ảo AI hỏi đáp bằng giọng nói tiếng Việt/Anh hai chiều và thực hiện **Hỏi xoáy thích ứng (Adaptive Follow-up)** theo thời gian gần thực.
5. Bảo đảm tính minh bạch và lưu vết bằng chứng học thuật bất biến (audio, transcript, log điểm AI vs điểm Giảng viên) phục vụ công tác thanh tra và giải quyết khiếu nại.

### Nguyên lý kiến trúc cốt lõi: Human-in-the-Loop
- AI **chỉ đóng vai trò phỏng vấn viên và trợ lý gợi ý điểm**.
- **Giảng viên là người duy nhất nắm quyền phê duyệt câu hỏi và chốt điểm số chính thức.** Nghiêm cấm mọi luồng tự động công bố điểm từ AI mà không qua xác nhận của giảng viên.

---

## 2. Bản Đồ Tài Liệu Kỹ Thuật Dự Án (Documentation Index)

Toàn bộ tài liệu phân tích, kiến trúc và giao ước dữ liệu đã được hoàn thiện đầy đủ và lưu tại:

| Mã số tài liệu | Tên tài liệu & Đường dẫn | Nội dung tóm tắt |
|---|---|---|
| **DOC-01-PRD** | [01-prd-requirements.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/prds/prd-aives/01-prd-requirements.md) | Đặc tả chi tiết 30 Yêu cầu Chức năng (`FR-001` - `FR-030`), 8 Yêu cầu Phi chức năng (`NFR-001` - `NFR-008`), 8 Quy tắc Nghiệp vụ (`BR-001` - `BR-008`), Rủi ro & Nghiệm thu. |
| **DOC-02-UC** | [02-actors-and-use-cases.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/prds/prd-aives/02-actors-and-use-cases.md) | Đặc tả 14 Ca sử dụng trọng yếu (`UC-01` - `UC-14`) với đầy đủ Main Flow, Alternative Flow, Exception Flow và liên kết FR. |
| **DOC-03-DM** | [03-domain-model.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/03-domain-model.md) | Mô hình miền thực thể, quan hệ, lực số, vòng đời trạng thái (`Lifecycle`) và 6 Bất biến nghiệp vụ bắt buộc (`INV-1` - `INV-6`). |
| **DOC-04-ARCH** | [04-system-architecture.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/04-system-architecture.md) | Kiến trúc C4 (System Context C1, Container C2), phong cách Modular Monolith, điều phối luồng Viva qua WebSockets, và 6 bản ghi ADR (`ADR-01` - `ADR-06`). |
| **DOC-05-AI** | [05-ai-pipeline-and-guardrails.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/05-ai-pipeline-and-guardrails.md) | 3 Tuyến đường ống AI (RAG sinh câu hỏi, Lõi Viva thời gian gần thực, Chấm điểm Rubric) và 5 Hàng rào an toàn học thuật (`GR-01` - `GR-05`). |
| **DOC-06-DB** | [06-database-design.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/06-database-design.md) | Thiết kế CSDL vật lý PostgreSQL 16 + `pgvector`, DDL Tables, chiến lược Snapshot bảo toàn lịch sử bài thi, Indexing. |
| **DOC-07-SEC** | [07-security-and-privacy.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/07-security-and-privacy.md) | Phân loại dữ liệu 4 cấp độ, Mô hình đe dọa 11 nguy cơ (T-01 đến T-11), biện pháp phòng chống Prompt Injection, mã hóa Presigned S3 URLs. |
| **DOC-08-API** | [08-api-design.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/planning-artifacts/architecture/08-api-design.md) | Đặc tả chi tiết 12 REST API endpoints và giao thức thông điệp hai chiều kênh WebSocket `WSS /ws/v1/viva-session`. |
| **DOC-09-RTM** | [09-traceability-matrix.md](file:///D:/School/AIVES_AI_powered_Viva_Exam_System/_bmad-output/specs/09-traceability-matrix.md) | Ma trận truy vết 6 chiều (Feature $\rightarrow$ FR $\rightarrow$ Use Case $\rightarrow$ Entity $\rightarrow$ API $\rightarrow$ Component) và NFR Traceability. |

---

## 3. Tech Stack Đã Quyết Định (Architecture Baseline)

- **Frontend:** Next.js / React (SPA) + TailwindCSS, tích hợp Web Audio API và Web MediaStream Recording.
- **Backend:** Modular Monolith (FastAPI / Python hoặc NestJS / Node.js) tối ưu xử lý I/O bất đồng bộ.
- **Real-time Viva Engine:** Python Async WebSocket Server + Silero-VAD (Voice Activity Detection).
- **Cơ sở dữ liệu:** PostgreSQL 16 tích hợp `pgvector` (quản lý cả dữ liệu quan hệ và vector embeddings cho RAG).
- **In-Memory & Task Queue:** Redis (lưu trạng thái phiên thi thời gian thực) + Celery / Worker bất đồng bộ.
- **Lưu trữ tệp:** MinIO (Local) / AWS S3 (Cloud) lưu trữ slide giáo trình và file ghi âm bài thi.
- **AI Integrations (Adapter Pattern):**
  - STT: OpenAI Whisper / Faster-Whisper / FPT.AI Speech-to-Text (có Domain Vocabulary Boosting).
  - TTS: FPT.AI / Edge-TTS / Google Cloud Text-to-Speech (giọng đọc tiếng Việt tự nhiên).
  - LLM: Google Gemini Pro/Flash hoặc OpenAI GPT-4o-mini qua cấu trúc prompt phòng thủ.

---

## 4. Chính Sách Quản Trị Cho Giai Đoạn Tiếp Theo (Next Steps Policy)

- **Tuyệt đối KHÔNG viết mã nguồn (code implementation) khi chưa có sự phê duyệt của người dùng đối với bộ tài liệu trên.**
- Sau khi người dùng duyệt bộ tài liệu:
  1. Chạy kỹ năng `bmad-create-epics-and-stories` để bóc tách 30 FR thành các User Story có Acceptance Criteria rõ ràng.
  2. Chạy `bmad-sprint-planning` để lập kế hoạch Sprint (ưu tiên Sprint 1: Dựng Data Schema, Mock Dataset và Chức năng số 6 Báo cáo/Phản hồi).
  3. Chỉ khi đó mới chuyển giao cho Developer Agent (Amelia 💻 - `bmad-build`) để triển khai code theo quy luật Test-First.
