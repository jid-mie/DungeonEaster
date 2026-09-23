# 🎮 DUNGEON EATER - BẢN DEMO GAME PLAYABLE

> Dự án bài tập lớn môn **Thiết kế và phát triển Game** (Trường ĐH Thủy Lợi)  
> Sinh viên thực hiện: Nguyễn Văn Hải, Nguyễn Minh Hiệp, Ngô Văn Hiến, Trần Hữu Hậu.

---

## 🌟 Giới thiệu tổng quan

**Dungeon Eater** là tựa game kết hợp độc đáo giữa **Action Roguelite / Hack & Slash** góc nhìn Top-down và **Quản lý nhà hàng (Restaurant Management)** theo phong cách Pixel Art cổ điển.

### 🔄 Vòng lặp cốt lõi (Core Gameplay Loop):
$$\text{Săn quái (Dungeon)} \longrightarrow \text{Thu nguyên liệu} \longrightarrow \text{Nấu món ăn} \longrightarrow \text{Phục vụ thực khách} \longrightarrow \text{Nhận Vàng & Danh tiếng} \longrightarrow \text{Nâng cấp} \longrightarrow \text{Tiến sâu hơn}$$

---

## 🚀 Cách chạy bản Demo

Game được xây dựng bằng công nghệ Web Game Engine thuần (HTML5 Canvas 2D + Web Audio API synthesizer), không cần cài đặt dependencies phức tạp.

### Cách 1: Mở trực tiếp bằng trình duyệt
Mở trực tiếp file `index.html` bằng Google Chrome, Microsoft Edge, Brave hoặc Firefox.

### Cách 2: Chạy qua Local HTTP Server (Khuyên dùng)
Mở terminal tại thư mục `/home/jid-mie/Game` và chạy:
```bash
python3 -m http.server 8080
```
Sau đó truy cập trình duyệt tại địa chỉ: **`http://localhost:8080`**

---

## 🕹 Hệ thống điều khiển (Controls)

| Phím bấm | Hành động | Chi tiết cơ chế |
|---|---|---|
| **W, A, S, D** | Di chuyển | Hỗ trợ di chuyển 8 hướng mượt mà |
| **Chuột trái (LMB) / J** | Đánh thường (Light Attack) | **Kiếm**: Chuỗi combo 3 đòn liên hoàn chém vệt sáng<br>**Cung**: Bắn mũi tên nhanh tầm xa |
| **Chuột phải (RMB) / K** | Đòn mạnh (Heavy Attack) | **Kiếm**: Chém hình cung lớn (Cleave) sát thương cao, knockback mạnh<br>**Cung**: Gồng bắn mũi tên xuyên phá cực mạnh |
| **Space (Phím cách)** | Lướt né đòn (Dash) | Có thời gian miễn nhiễm sát thương (**i-frame**) và hiệu ứng bóng mờ (ghost trail) |
| **Q** | Đổi vũ khí | Chuyển đổi qua lại tức thì giữa **Kiếm cận chiến** và **Cung tầm xa** |
| **E** | Kỹ năng vũ khí (Skill) | **Kiếm**: Lốc xoáy cuồng phong (Whirlwind / Spin Slash) quét sạch quái xung quanh<br>**Cung**: Tên xuyên thấu (Piercing Arrow) xuyên qua mọi kẻ địch |
| **F** | Tương tác / Nhặt đồ | Nhặt thịt, nấm rơi trong dungeon / Tương tác bếp nấu, phục vụ khách |
| **Esc** | Tạm dừng / Đóng Menu | Đóng bảng công thức hoặc nâng cấp |

---

## ⚔️ Các tính năng trong bản Demo

### 1. Pha 1: Hầm ngục Dungeon
- **3 loại Creep**:
  - **Slime (Cận chiến)**: Nhảy vồ cắn với vệt đỏ cảnh báo (**Telegraph 0.4s**). Rơi *Thịt quái* hoặc *Nấm*.
  - **Spore Shroom (Tầm xa)**: Canh khoảng cách và bắn đạn nấm độc có đường ngắm báo trước (**Telegraph 0.5s**). Rơi *Nấm hang sâu*.
  - **Bomb Beetle (Cảm tử)**: Lao đến người chơi, đếm ngược chớp đỏ rồi phát nổ diện rộng (**Kamikaze**). Rơi *Lõi bọ nổ*.
- **Boss cuối: Vua Lợn Rừng Đột Biến (King Boar)**:
  - Lượng máu lớn kèm thanh HP trên đầu.
  - Kỹ năng húc càn xuyên phòng có cảnh báo vùng đỏ và kỹ năng dậm đất chấn động.
  - Hạ gục Boss sẽ rơi *Sườn Vua Thú* quý hiếm và mở **Cổng dịch chuyển về làng Dun**.
- **Cảm giác chiến đấu (Combat Feel)**: Hit-stop ($40-80\text{ms}$), rung lắc màn hình (screen shake), vệt chém pixel và knockback.

### 2. Pha 2: Nhà hàng Làng Dun
- **Bếp nấu (Kitchen Counter)**:
  - Nấu 4 công thức đặc biệt từ nguyên liệu săn được: *Thịt xiên nướng quái thú*, *Súp nấm hang sâu*, *Bít tết bùng nổ*, và *Đại tiệc Vua Thú*.
- **Phục vụ khách (Serving)**:
  - Khách vào bàn gọi món hiển thị trên bong bóng chat kèm thanh kiên nhẫn (**Patience Bar**).
  - Phục vụ đúng giờ: Nhận Vàng, Danh tiếng (Reputation) và khách thả tim.
  - Để khách chờ quá lâu: Khách giận dữ bỏ về và trừ điểm danh tiếng.

### 3. Pha 3: Nâng cấp (Meta Progression)
- Nâng cấp sức mạnh Kiếm và Cung.
- Mở rộng sức chứa Balo (Backpack slot).
- Kê thêm bàn ăn cho nhà hàng để phục vụ nhiều khách cùng lúc.
- Nâng cấp Lò nấu ma thuật giúp tăng thời gian kiên nhẫn của thực khách.

