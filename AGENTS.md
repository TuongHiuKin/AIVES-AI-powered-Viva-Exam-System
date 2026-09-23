<!-- bmad:context -->
<!-- Verified 2026-09-22 against initial repository state. Managed by bmad-project-context; edits inside this block are replaced on refresh. Keep anything you want preserved outside the markers. -->

## AIVES (AI-powered Viva Exam System)

Hệ thống thi vấn đáp thông minh ứng dụng AI (giám khảo ảo phỏng vấn bằng giọng nói, hỏi xoáy thích ứng, hỗ trợ chấm điểm theo Rubric, và báo cáo phân tích học tập).
Planning artifacts lưu tại `_bmad-output/planning-artifacts/`, specifications và contracts lưu tại `_bmad-output/specs/`.

## Policy

- **NGUYÊN TẮC BẮT BUỘC**: Tuyệt đối KHÔNG viết mã nguồn (code implementation) khi giai đoạn Planning và Architecture chưa được định nghĩa đầy đủ và được người dùng phê duyệt.
- Tuân thủ nghiêm ngặt vòng đời của BMAD Method: Phân tích (Analysis) -> Lập kế hoạch (Planning) -> Kiến trúc hệ thống (Architecture) -> Bóc tách Epics & Stories -> Triển khai (Implementation / Build).
- Toàn bộ quyết định kiến trúc, Data Model, API Contract và Mock Data phải được tài liệu hóa trong `_bmad-output/` trước khi lập trình bất kỳ tính năng nào.
- Mọi quyết định thiết kế quan trọng phải được ghi vết vào `.memlog.md` qua `memlog.py append`.
- Ngôn ngữ giao tiếp và tài liệu kỹ thuật là Tiếng Việt (`Vietnamese`).

## Where things are

- Tài liệu PRD/BRD: `_bmad-output/planning-artifacts/prds/`
- Data Contract & Mock Dataset: `_bmad-output/planning-artifacts/prds/prd-aives-2026-09-22/addendum-data-contract.md`
- BMAD Core & Config: `_bmad/` và `.agent/skills/`
- Bộ tiêu chuẩn BA Blueprint: `BA-Blueprint-Agent/`

## Running and verifying

- Mọi script Python của BMAD chạy thông qua `uv` (`uv run <script>`).
- Kiểm tra trạng thái BMAD: `npx bmad-method status`.

## Known pitfalls

- Nhảy vào code giao diện hoặc backend trước khi chốt Data Contract sẽ dẫn đến mâu thuẫn dữ liệu giữa Lõi AI (Nhóm 3), Chấm điểm (Nhóm 4) và Báo cáo (Nhóm 6).
- Tự động hóa hoàn toàn việc chấm điểm mà bỏ qua vai trò duyệt của Giảng viên (Human-in-the-loop) sẽ vi phạm nguyên tắc bảo đảm liêm chính học thuật.

<!-- /bmad:context -->
