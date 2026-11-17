# BẢNG PHÂN CÔNG CÔNG VIỆC ĐỒ ÁN CUỐI KỲ
**Môn học:** Phát triển trò chơi (Game Development)  
**Tên đề tài:** Core Breach (Top-down Twin-stick Shooter Roguelite)  
**Quy mô nhóm:** 2 Thành viên  

---

## I. THÔNG TIN THÀNH VIÊN VÀ TỔNG QUAN VAI TRÒ

| STT | Họ và Tên | MSSV | Vai trò chính | Tỷ lệ đóng góp |
|:---:|:---|:---:|:---|:---:|
| 1 | **Thành viên A** | *(Điền MSSV)* | **Trưởng nhóm** - Kiến trúc hệ thống, Lập trình Gameplay Lõi & Tích hợp | **50%** |
| 2 | **Thành viên B** | *(Điền MSSV)* | **Thành viên** - Thiết kế Màn chơi, AI Combat, Giao diện UI/UX & Đa phương tiện | **50%** |

---

## II. BẢNG PHÂN CÔNG CHI TIẾT THEO HẠNG MỤC ĐỒ ÁN

| Hạng mục / Tiêu chí đồ án | Nội dung công việc cụ thể | Phụ trách chính | Hỗ trợ / Phối hợp | Kết quả bàn giao |
|---|---|:---:|:---:|---|
| **1. Ý tưởng & Game Concept** | Soạn thảo 9 mục Game Concept theo chuẩn yêu cầu của Giảng viên; thiết kế 3 cơ chế đột phá (Overheat, Run Modifiers, Hack Terminal). | Thành viên A | Thành viên B | File đề xuất Game Concept đã duyệt |
| **2. Điều khiển & Nhân vật chính (Unit-7)** | Lập trình di chuyển 8 hướng (`PlayerMovement`), Dash né đòn, xoay ngắm chuột độc lập (`PlayerAim`), hệ thống Máu & Giáp (`PlayerHealth`). | Thành viên A | Thành viên B | `Player.prefab`, các script Player hoàn chỉnh |
| **3. Vũ khí & Cơ chế Overheat** | Lập trình 2 khe vũ khí đổi bằng phím R (`WeaponController`), cơ chế Nạp nhiệt tự khóa 2s khi đạt 100% (`HeatSystem`), quản lý đạn (`Bullet`). | Thành viên A | Thành viên B | Hệ thống súng Laser Rifle, đạn va chạm |
| **4. Kẻ địch tự động (AI)** | Lập trình FSM cho kẻ địch cận chiến (`Sentry Drone`), ụ súng tầm xa xoay ngắm (`Turret Bot`) kèm cơ chế Hack Terminal. | Thành viên B | Thành viên A | `Enemy.cs`, `TurretBot.cs`, prefab Drone & Turret |
| **5. Trùm Mini-Boss (Core Guardian)** | Thiết kế và lập trình FSM 3 pha của Boss chuyển tiếp theo % HP (Bắn chùm đạn liên thanh → Triệu hồi đệ tử Drone → Quá tải cuồng nộ lao xả thân). | Thành viên A | Thành viên B | `CoreGuardianBoss.cs`, Prefab Boss |
| **6. Thiết kế Phòng & Bản đồ (Tilemap)** | Tạo bố cục phòng chiến đấu, đặt các điểm spawn hợp lý, paint Tilemap sàn/tường và cấu hình `TilemapCollider2D` chống lọt map. | Thành viên B | Thành viên A | `RoomCombat_01.prefab`, Palette gạch nền |
| **7. Quản lý Phòng & Tiến trình (Depth)** | Lập trình vòng lặp phòng ngẫu nhiên theo độ sâu (`RoomManager`, `RoomController`), mở cửa Exit khi quét sạch quái (`RoomExit`). | Thành viên B | Thành viên A | Hệ thống chuyển tầng vô tận |
| **8. Quản lý Game & Lưu trữ dữ liệu** | Xây dựng `GameManager`, tính điểm, thu thập Scrap vĩnh viễn, lưu High-score và Meta-progression bằng `PlayerPrefs`. | Thành viên A | Thành viên B | Lưu trữ kỷ lục và Scrap chơi lại |
| **9. Cơ chế đặc trưng (Run Modifier)** | Lập trình hệ thống random lỗi hệ thống đầu lượt ("Trọng lực yếu", "Mất điện", "Quá tải x2 Scrap") (`RunModifierSystem`). | Thành viên A | Thành viên B | `RunModifierSystem.cs` |
| **10. Camera 2D** | Lập trình Camera bám mượt mà theo Unit-7 (`CameraFollow`), giới hạn khung nhìn trong phòng. | Thành viên A | Thành viên B | `CameraFollow.cs` trên Main Camera |
| **11. Giao diện người dùng (UI/HUD & Menu)** | Thiết kế Canvas HUD (HP bar, Armor bar, Heat bar nhấp nháy, Scrap, Depth), Main Menu và Pause Menu (phím ESC). | Thành viên B | Thành viên A | `PlayerHUD.cs`, `PauseMenu.cs`, UI Canvas |
| **12. Âm thanh (Audio)** | Thu thập và xử lý các file SFX (.wav); lập trình `AudioManager` kích hoạt âm thanh bắn, né, trúng đạn, chết, quá nhiệt (đạt ≥ 5 loại âm thanh). | Thành viên B | Thành viên A | `AudioManager.cs` và bộ sound clips |
| **13. Hiệu ứng hình ảnh (VFX)** | Tạo hiệu ứng hạt Particle (tóe tia lửa khi đạn va chạm tường/quái, khói cảnh báo Overheat, nổ mảnh kim loại). | Thành viên B | Thành viên A | Prefab VFX đạn và va chạm |
| **14. Quản trị Git & Đóng gói Build** | Quản lý repository GitLab, giải quyết conflict, dọn dẹp thư mục rác (Library, Temp), build bản chạy thử Windows (.exe). | Thành viên A | Thành viên B | Nhánh Git cập nhật liên tục, bản Build |
| **15. Báo cáo Word & Tài liệu nộp** | Soạn thảo quyển báo cáo Word (9 mục chuẩn: Lý thuyết, sơ đồ UML UseCase/Class/Sequence/Activity, Hiện thực, Kết quả). | Thành viên B | Thành viên A | File báo cáo `BaoCao_CoreBreach.docx` |
| **16. Poster & Video giới thiệu** | Thiết kế Poster game (`poster.jpg` + `.psd`/`.ai`); quay và dựng Video giới thiệu (`intro.mp4`, 2–3p) và Video demo chơi game (≤15p). | Cả hai cùng thực hiện (A quay demo code, B dựng intro & poster) | Toàn nhóm | Poster chuẩn kích thước, 2 file Video HD 720p |

---

## III. CAM KẾT VÀ QUY TẮC LÀM VIỆC CỦA NHÓM
1. **Kỷ luật Git:** Mỗi thành viên chủ động commit và push bài lên nhánh phân công tối thiểu **1 lần/tuần** (vào trước 23:59 Thứ 6 hàng tuần), tuyệt đối không để trống quá 14 ngày nhằm tránh bị trừ điểm hoặc cấm thi theo quy định của môn học.
2. **Kiểm thử chéo (Cross-testing):** Sau khi Thành viên A hoàn thành hệ thống code, Thành viên B kiểm thử trực tiếp trên Scene phòng; ngược lại khi Thành viên B thiết kế xong map và UI, Thành viên A tiến hành tích hợp và kiểm tra hiệu năng.
3. **Trung thực học thuật:** 100% các module logic cốt lõi do nhóm tự viết trên Unity C#, không sao chép source code trên mạng; các tài nguyên hình ảnh, âm thanh tham khảo đều có ghi chú nguồn rõ ràng trong file `readme.txt`.
