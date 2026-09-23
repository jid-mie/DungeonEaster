---
name: unity-mcp
description: Quy trình làm việc và hướng dẫn Antigravity giao tiếp trực tiếp với Unity Editor qua giao thức MCP (Model Context Protocol). Hỗ trợ kiểm tra Scene, tạo GameObject, sửa Component và đọc Console Logs.
---

# Unity MCP Integration Guide

Kỹ năng này hướng dẫn AI (Google Antigravity) cách kết nối và điều khiển trực tiếp Unity Editor qua Model Context Protocol (MCP).

---

## 1. Kiến trúc kết nối

```
[Google Antigravity AI]
          ↕ (Giao thức MCP / JSON-RPC)
[mcp-unity Bridge / Local Server :8080]
          ↕ (WebSocket / Editor IPC)
[Unity Editor - MCP Plugin (MCPForUnity)]
```

- Cấu hình server được đăng ký tại: `.agents/mcp_config.json`
- Unity lắng nghe tại cổng `8080` (hoặc cổng cấu hình trong `Window -> MCP for Unity`).

---

## 2. Các công cụ MCP hỗ trợ

Khi Unity MCP được kích hoạt, các công cụ sau sẽ sẵn sàng để gọi:

1. **`get_hierarchy`**:
   - Lấy toàn bộ cây đối tượng (Scene Hierarchy) trong Scene đang mở của Unity.
   - Giúp AI định vị chính xác GameObject nào đã có, GameObject nào còn thiếu.

2. **`get_game_object`**:
   - Tham số: `name` (string) hoặc `instance_id` (int).
   - Đọc chi tiết các Component đính kèm, vị trí Transform (Position, Rotation, Scale), và giá trị các trường SerializedFields.

3. **`create_game_object`**:
   - Tham số: `name`, `parent` (optional), `components` (mảng tên Component: `["SpriteRenderer", "Rigidbody2D"]`), `primitive_type` (optional).
   - Tự động tạo đối tượng mới vào Scene.

4. **`modify_game_object`**:
   - Chỉnh sửa giá trị thuộc tính, đổi layer, tag hoặc gán Script vào đối tượng.

5. **`get_console_logs`**:
   - Lấy danh sách Logs, Warnings, Errors mới nhất từ Unity Console.
   - Giúp AI tự động phát hiện lỗi compile hoặc runtime exception (NullReferenceException, MissingReferenceException) và tiến hành tự sửa code C# ngay lập tức.

6. **`execute_menu_item`**:
   - Chạy các lệnh menu Unity, ví dụ: `File/Save`, `Assets/Refresh`, `Dungeon Eater/Auto Setup Scene`.

7. **`play_mode`**:
   - Tham số: `state` (`"play"` hoặc `"stop"`).
   - Tự động bấm nút Play để kiểm thử game hoặc Stop khi kiểm thử xong.

---

## 3. Quy trình làm việc chuẩn giữa AI và Developer

1. **Bước 1**: Developer mở Unity Editor với dự án `DungeonEater_Unity`.
2. **Bước 2**: Đảm bảo package `unity-mcp` (từ `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`) đã được nạp qua Unity Package Manager và trạng thái hiển thị **Connected** tại menu `Window -> MCP for Unity`.
3. **Bước 3**: Developer chỉ cần đưa ra yêu cầu bằng lời nói:
   - *"Kiểm tra xem Scene hiện tại có Player chưa?"* $\to$ AI gọi `get_hierarchy`.
   - *"Gán script PlayerController vào Player"* $\to$ AI gọi `modify_game_object`.
   - *"Kiểm tra xem Unity có đang báo lỗi gì không"* $\to$ AI gọi `get_console_logs` và tự sửa lỗi trong code C#.

