# AIVES (AI-powered Viva Exam System) — Thư Mục Tài Liệu Kỹ Thuật (Project Documentation)

Thư mục này chứa toàn bộ tài liệu đặc tả nghiệp vụ, yêu cầu chức năng, mô hình miền, kiến trúc hệ thống, thiết kế CSDL, bảo mật, API và ma trận truy vết của dự án **AIVES**, được biên soạn theo chuẩn **BMAD Method**.

---

## 📑 Mục Lục Tài Liệu (Table of Contents)

| STT | Tên Tài Liệu | File Liên Kết | Tóm Tắt Nội Dung |
|:---:|---|---|---|
| **01** | **Yêu Cầu Sản Phẩm & Nghiệp Vụ (PRD)** | [01-prd-requirements.md](01-prd-requirements.md) | 30 Yêu cầu Chức năng (`FR-001` - `FR-030`), 8 Yêu cầu Phi chức năng (`NFR-001` - `NFR-008`), 8 Quy tắc Nghiệp vụ (`BR-001` - `BR-008`), Rủi ro & Nghiệm thu. |
| **02** | **Tác Tử & 14 Ca Sử Dụng (Use Cases)** | [02-actors-and-use-cases.md](02-actors-and-use-cases.md) | 14 Ca sử dụng chi tiết (`UC-01` - `UC-14`): Tạo câu hỏi, RAG, bốc đề, Viva voice loop, hỏi xoáy, gợi ý điểm, giảng viên chốt điểm, khiếu nại. |
| **03** | **Mô Hình Miền & Bất Biến Nghiệp Vụ** | [03-domain-model.md](03-domain-model.md) | Sơ đồ Domain Class Diagram, vòng đời trạng thái bài thi, 6 Bất biến nghiệp vụ bất khả xâm phạm (`INV-1` - `INV-6`). |
| **04** | **Kiến Trúc Hệ Thống & Các Bản Ghi ADR** | [04-system-architecture.md](04-system-architecture.md) | Mô hình C4 (System Context C1, Container C2), phong cách Modular Monolith, WebSockets thời gian thực, 6 bản ghi quyết định kỹ thuật (`ADR-01` - `ADR-06`). |
| **05** | **Kiến Trúc Đường Ống AI & Hàng Rào An Toàn** | [05-ai-pipeline-and-guardrails.md](05-ai-pipeline-and-guardrails.md) | 3 Tuyến đường ống AI (RAG từ tài liệu, Viva voice loop gần thực, Chấm điểm Rubric) và 5 Hàng rào an toàn học thuật (`GR-01` - `GR-05`). |
| **06** | **Thiết Kế Cơ Sở Dữ Liệu (Database Design)** | [06-database-design.md](06-database-design.md) | Sơ đồ ERD, DDL PostgreSQL 16 + `pgvector`, chiến lược Snapshot bảo toàn lịch sử bài thi, cơ chế Indexing tối ưu tra cứu $O(1)$ và Cosine Similarity. |
| **07** | **Bảo Mật, Quyền Riêng Tư & Mô Hình Đe Dọa** | [07-security-and-privacy.md](07-security-and-privacy.md) | Phân loại dữ liệu 4 cấp độ, Mô hình STRIDE 11 mối đe dọa (T-01 đến T-11), phòng chống Prompt Injection, mã hóa Presigned S3 URLs, Audit Log Insert-Only. |
| **08** | **Đặc Tả Giao Diện Lập Trình (API Specs)** | [08-api-design.md](08-api-design.md) | 12 REST API endpoints chuẩn hóa và cấu trúc thông điệp hai chiều kênh WebSocket `WSS /ws/v1/viva-session`. |
| **09** | **Ma Trận Truy Vết 6 Chiều (Traceability Matrix)** | [09-traceability-matrix.md](09-traceability-matrix.md) | Ma trận liên kết 6 chiều: $\text{Feature Group} \rightarrow \text{FR} \rightarrow \text{Use Case} \rightarrow \text{Entity} \rightarrow \text{API} \rightarrow \text{Component}$. |

---

## 📦 Tài Liệu Phụ Trợ & Dữ Liệu Mẫu (Supporting Artifacts)

- [**project-context.md**](project-context.md): Bản tổng hợp ngữ cảnh kỹ thuật dành cho lập trình viên và AI Coding Agents trước khi bắt đầu code.
- [**addendum-data-contract.md**](addendum-data-contract.md): Đặc tả cấu trúc JSON Schema chuẩn của thực thể bài thi `VivaAttemptRecord`.
- [**mock-viva-attempts.json**](mock-viva-attempts.json): Dữ liệu mẫu thực tế của 2 sinh viên (1 bài đạt 8.5/10 phản biện tốt câu hỏi xoáy, 1 bài đạt 5.5/10) để nạp trực tiếp vào Frontend/Backend test ngay.
