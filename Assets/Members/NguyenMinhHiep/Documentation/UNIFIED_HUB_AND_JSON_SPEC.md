# TÀI LIỆU KỸ THUẬT: HỆ THỐNG DỮ LIỆU CHUNG, UNIFIED CANVAS HUB & CƠ SỞ DỮ LIỆU JSON (NGUYỄN MINH HIỆP)

Tài liệu này mô tả chi tiết các thành phần được xây dựng trong thư mục `Assets/Members/NguyenMinhHiep/` nhằm hợp nhất trạng thái người chơi, giao diện HUD/Inventory cho toàn bộ 3 Scene (**Dungeon**, **Village**, **Restaurant**) và chuẩn hóa danh mục vật phẩm bằng JSON theo báo cáo `TaiLieu.md`.

---

## 1. Cấu trúc thư mục & File triển khai

| Thành phần | Đường dẫn file | Vai trò chính |
| :--- | :--- | :--- |
| **JSON Catalog** | `Assets/Members/NguyenMinhHiep/Data/items_catalog.json` *(và bản sao tại `Resources/Data/items_catalog.json`)* | Định nghĩa toàn bộ **27 vật phẩm** (13 Nguyên liệu Bảng 7.1, 10 Món ăn Bảng 7.2, 4 Tiêu hao chiến đấu Bảng 7.7) và **2 Vũ khí × 4 Cấp độ** (Bảng 7.5). |
| **JSON Database Loader** | `Assets/Members/NguyenMinhHiep/Scripts/Data/GameItemDatabase.cs` | Đọc và phân tích `items_catalog.json`, hỗ trợ tra cứu theo ID chính thức hoặc bí danh (`aliases`) giữa các scene, tự động gắn Sprite icon hoặc sinh icon pixel dự phòng. |
| **Class Chung Lưu Trạng Thái** | `Assets/Members/NguyenMinhHiep/Scripts/Core/SharedGameDataManager.cs` | Singleton `DontDestroyOnLoad` lưu trữ xuyên suốt 3 Scene: **Tiền (Gold)**, **Danh tiếng (Reputation)**, **Máu (HP)**, **Thể lực (Stamina)**, **Vũ khí (Kiếm & Cung Lv.1–4)**, **Balo (10+ ô)** và **Quick Bar (4 ô)**, đồng bộ 2 chiều với các script của Hải, Hiến, Hậu. |
| **Bộ Dựng UI Gỗ 9-Slice** | `Assets/Members/NguyenMinhHiep/Scripts/UI/HiepUIKit.cs` | Kế thừa chuẩn format của `NgoVanHien` (`TavernSprites.cs` + `UIKit.cs`): khung viền pixel 9-slice, tông màu gỗ ấm (`WoodDark`, `WoodMid`, `WoodLight`) và giấy da (`Parchment`). |
| **HUD Chung Toàn Game** | `Assets/Members/NguyenMinhHiep/Scripts/UI/UnifiedHubHUD.cs` | Hiển thị góc trên trái (Tên Scene, Thanh Máu, Thanh Thể Lực, Tiền Vàng, Danh Tiếng), góc dưới trái (Vũ khí Kiếm/Cung, cấp độ, sát thương, phím `[Q]`), dưới giữa (Quick Bar 4 ô `[1-4]` + `[R]`), góc trên phải (Bảng hướng dẫn phím tắt). |
| **Bảng Hòm Đồ Chung** | `Assets/Members/NguyenMinhHiep/Scripts/UI/UnifiedInventoryPanel.cs` | Cửa sổ Túi đồ bên phải màn hình (`[I]` / `[Esc]`): Lưới Balo 5 cột, 4 ô Quick Bar, thanh tóm tắt Tiền/Danh tiếng/Rèn Vũ khí, thẻ Giấy Da xem chi tiết vật phẩm từ JSON, kèm **Cửa sổ Tra cứu Danh mục JSON** để lấy nhanh vật phẩm test. |
| **Master Canvas Hub** | `Assets/Members/NguyenMinhHiep/Scripts/UI/UnifiedGameHubCanvas.cs` | Canvas `DontDestroyOnLoad` duy nhất cho cả 3 Scene, tự động khởi chạy khi vào Play Mode (`[RuntimeInitializeOnLoadMethod]`) và tự động ẩn các HUD rời rạc cũ trong từng Scene. |
| **Công cụ Editor** | `Assets/Members/NguyenMinhHiep/Editor/UnifiedHubSetupTool.cs` | Menu `DungeonEater/NguyenMinhHiep/...` hỗ trợ gắn Hub vào Scene hiện tại, tích hợp tự động vào cả 3 Scene, hoặc kiểm tra dữ liệu JSON. |

---

## 2. Phím tắt điều khiển trên Unified Canvas Hub (3 Scene)

- **`[I]` hoặc `[Esc]`**: Mở / Đóng Bảng Túi Đồ & Kho Nguyên Liệu Chung (trong Dungeon sẽ tự động cảnh báo nếu quái đang áp sát truy đuổi theo mục 3.2.1 báo cáo).
- **`[Q]`**: Hoán đổi Vũ khí đang sử dụng (**Kiếm Thợ Săn** ↔ **Cung Tầm Xa**).
- **`[R]`**: Uống nhanh **Bình Máu Hồi Phục** (+20 HP).
- **`[1]` – `[4]`**: Sử dụng vật phẩm tại ô Quick Bar tương ứng (Bình máu, Lựu đạn nổ, Lựu đạn băng, Lựu đạn độc).
- **`[Tab]`**: Chuyển ô chọn trên thanh Quick Bar.
- **Chuột trái (LMB) trong Túi đồ**: Chọn 1 ô để xem thông tin JSON; nhấp ô thứ 2 để di chuyển/gộp stack/đổi chỗ.
- **Chuột phải (RMB) trong Túi đồ**: Gắn nhanh vật phẩm từ Balo xuống Quick Bar hoặc tháo từ Quick Bar về Balo.
