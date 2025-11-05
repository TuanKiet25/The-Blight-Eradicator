# Diệt Mầm Bệnh (The Blight Eradicator)

Diệt Mầm Bệnh là một trò chơi hành động-platformer 2D nơi người chơi vào vai Kael — một thợ săn quái vật đơn độc — nhiệm vụ là thâm nhập vào Khu Rừng Thép (Ironwood Forest) và tiêu diệt nguồn gốc của "Bệnh Dịch Rỉ Sét" (Rust Blight).

## Tóm tắt cốt truyện

Thế giới đang bị đe dọa bởi "Bệnh Dịch Rỉ Sét" — một căn bệnh biến động vật và con người thành những sinh vật kim loại hung dữ. Dịch bệnh bùng phát từ Khu Rừng Thép.

Kael, một thợ săn bệnh nhiễm chuyên nghiệp, được người bảo vệ thành trì (Keeper of the Citadel) giao nhiệm vụ thâm nhập vào lòng rừng để tiêu diệt Alpha Blight — Quái Vật Đầu Đàn, được cho là nguồn gốc phát tán dịch bệnh.

Khi Kael tiến sâu vào Lõi Rừng Thép, anh phải đối mặt với các kẻ thù mạnh hơn như Iron Golems và Steel Wraiths, bẫy môi trường, và những thử thách làm suy yếu năng lượng. Ở cao trào, Kael phát hiện Alpha Blight đang bị trói trong một khu vực kín và có dấu hiệu chịu đựng thay vì hung dữ — một phát hiện khiến Kael phải đặt câu hỏi về bản chất thực sự của dịch bệnh.

## Điểm nổi bật (Gameplay)

- Thể loại: Platformer hành động 2D.
- Nhân vật chính: Kael — có khả năng chạy, nhảy (double-jump), dash và tấn công cận chiến.
- Hệ thống năng lượng: Dash và chiêu thức tiêu tốn năng lượng; năng lượng hồi dần theo thời gian.
- Hệ thống mạng sống: Máu, số mạng (lives) và UI hiển thị (hp bar, energy bar, heart sprites).
- Kẻ địch đa dạng: kẻ địch cơ bản (di chuyển/tấn công), kẻ địch charge (phát hiện và lao nhanh), trùm cấp cao (Alpha Blight).
- Dialogue / Intro: Hệ thống thoại với portrait, chuyển cảnh sang Level_1 khi thoại kết thúc.

## Công nghệ & phiên bản (nổi bật)

![Unity](https://img.shields.io/badge/Unity-6000.2.3f1-1e90ff?logo=unity&logoColor=ffffff) ![URP v17.2.0](https://img.shields.io/badge/URP-17.2.0-8a2be2) ![Input System v1.14.2](https://img.shields.io/badge/Input%20System-1.14.2-2b9348) ![Cinemachine v3.1.5](https://img.shields.io/badge/Cinemachine-3.1.5-ff7f50) ![TextMeshPro](https://img.shields.io/badge/TextMeshPro-included-0057ff) ![2D Tools](https://img.shields.io/badge/2D--Tools-included-6a5acd)

- Unity Editor: m_EditorVersion: 6000.2.3f1 (mở project bằng phiên bản tương thích từ ProjectSettings/ProjectVersion.txt)
- Renderer: Universal Render Pipeline (com.unity.render-pipelines.universal v17.2.0)
- Input package: com.unity.inputsystem v1.14.2 (mã hiện tại dùng Input "legacy")
- Cinemachine: com.unity.cinemachine v3.1.5
- 2D packages: 2D Animation, Tilemap, SpriteShape, PSD Importer, v.v.
- UI: Unity uGUI + TextMeshPro
- Scripting: C# (Unity scripting API — theo phiên bản Editor)

## Yêu cầu & hướng dẫn mở project

1. Cài Unity Editor phiên bản tương thích với `ProjectVersion.txt`. Nếu Editor báo khác biệt, hãy dùng Unity Hub để cài chính xác hoặc chọn phiên bản gần nhất.
2. Mở project bằng Unity Hub: chọn folder dự án `The-Blight-Eradicator`.
3. Mở Scene khởi động (kiểm tra thư mục `Assets/Scenes` để biết scene nào là Main/Intro). `DialogueManager` mặc định tải `Level_1` khi thoại kết thúc — đảm bảo scene `Level_1` tồn tại trong Build Settings.
4. Chạy trong Editor (Play) để kiểm tra gameplay. Kiểm tra Inspector cho các reference UI (hpSlider, energySlider, attackPoint, animator, colliders) — nếu thiếu reference, script có thể throw null.

## Điều khiển (mặc định từ code)

- Di chuyển trái/phải: Input trục "Horizontal" (`A`/`D` hoặc `Left`/`Right` arrow tuỳ Input settings).
- Nhảy: nút "Jump" (mặc định `Space`).
- Double-jump: `Space` 2 lần.
- Chạy: giữ `Left Shift`.
- Dash: hiện đang dùng phím `W` (có thể đổi trong code sang phím khác).
- Tấn công: Fire1 (mặc định `chuột trái`). 
- Debug: nhấn `K` để nhận 10 sát thương (theo code PlayerController).

## Cấu trúc dự án (tóm tắt)

- `Assets/Scripts/` — chứa `PlayerController`, `EnemyController`, `Enemy1Controller`, `DialogueManager`, `MainMenu` và các script khác.
- `Packages/manifest.json` — các package đang dùng (URP, Input System, Cinemachine, 2D packages, v.v.).
- `ProjectSettings/ProjectVersion.txt` — phiên bản Editor được lưu trong project.

## Kiểm tra nhanh bạn có thể chạy

1. Mở `Assets/Scenes` trong Unity -> xác định scene khởi động (MainMenu hoặc Intro) và thêm vào Build Settings nếu chưa có.
2. Chạy Play, kiểm tra Player movement, nhảy, dash, tấn công và UI.
3. Kiểm tra `PlayerPrefs` key `LastSavedScene` để xác thực Continue button.

## Nhóm phát triển

Dự án này được phát triển bởi một nhóm sinh viên từ Đại học FPT (FPT University) trong khuôn khổ khóa học/đồ án. Nhóm chịu trách nhiệm về thiết kế gameplay, lập trình, và tích hợp tài sản.
