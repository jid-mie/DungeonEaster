# HỆ THỐNG BOSS CÁC TẦNG DUNGEON - NGUYỄN MINH HIỆP
**Dự án:** Dungeon Eater (Game Action Roguelite & Restaurant Management)  
**Tác giả phân hệ:** Nguyễn Minh Hiệp (`Assets/Members/NguyenMinhHiep/`)  
**Quy chuẩn áp dụng:** `TaiLieu.md` (Báo cáo BTL - Mục 4.3, 5.2, 7.2, 7.3, 7.4, 7.5)

---

## 1. TỔNG QUAN HIỆN TRẠNG & DANH SÁCH BOSS THEO TÀI LIỆU

Theo tài liệu thiết kế trò chơi `TaiLieu.md` (Mục 7.4 Bảng ma trận Boss), Dungeon gồm 5 tầng ngục với 5 đại Boss canh giữ:

| Tầng | Tên Boss | Danh hiệu | Máu (HP) | ATK | Tốc độ | Trạng thái hiện tại | Phân hệ phụ trách |
| :---: | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **Tầng 1** | **Rắn Khổng Lồ** | Chúa tể hang tối | **450** | 20 - 25 | 3.2 u/s | ✅ **Đã hoàn thiện** | `Assets/Members/NguyenMinhHiep/` |
| **Tầng 2** | **Lợn Rừng Khổng Lồ (King Boar)** | Bá chủ đầm lầy | **750** | 28 - 35 | 4.5 u/s | ✅ Đã có từ trước | `Assets/Members/NguyenVanHai/` |
| **Tầng 3** | **Người Đá Khổng Lồ (Stone Golem)** | Hộ vệ băng nguyên | **1200** | 40 - 50 | 1.6 u/s | ✅ **Đã hoàn thiện** | `Assets/Members/NguyenMinhHiep/` |
| **Tầng 4** | **Đôi Hiệp Sĩ (Dual Boss)** | Song kiếm cấm vệ | **Mỗi con 600** *(Tổng 1200)* | 35 - 45 | 3.8 u/s | ✅ **Đã hoàn thiện** | `Assets/Members/NguyenMinhHiep/` |
| **Tầng 5** | **Gwyn** *(Boss Cuối)* | Thần sáng tạo | **2500** | 55 - 75 | 3.5 u/s | ✅ **Đã hoàn thiện** | `Assets/Members/NguyenMinhHiep/` |

---

## 2. CHI TIẾT BỘ CHỈ SỐ & CƠ CHẾ CHIẾN ĐẤU CỦA 4 BOSS MỚI

### 2.1. Boss Tầng 1: Rắn Khổng Lồ (Chúa tể hang tối)
* **Script:** `GiantSnakeBoss.cs`
* **Sprite:** `Boss_GiantSnake.png` (256×256)
* **Chỉ số:** HP: 450, Tốc độ: 3.2 u/s, Tầm phát hiện: 15m.
* **Bộ chiêu thức:**
  1. *Quét Đuôi (Tail Sweep 180°):* Telegraph quạt góc 180° bán kính 3.2m trong 0.4s. Quét mạnh hất văng người chơi, gây 20 DMG.
  2. *Cắn Bổ Nhào (Poison Lunge):* Telegraph đường thẳng dài 4.5m trong 0.5s. Lao vọt tới tốc độ 9.0 u/s gây 25 DMG.
  3. *Phun Độc Tầm Xa (Poison Spit):* Khi người chơi giữ cự ly > 7m, rắn phun cầu độc ma quái bay tới gây 18 DMG.
* **Chiến lợi phẩm (100% rơi):** 1× `snake_meat` ("Thịt nạc Rắn khổng lồ").
* **Mở khóa nhà hàng:** Chế biến món **Thịt Nướng Độc Mãng Xà** (130 Gold).
* **Mở khóa nâng cấp:** Cho phép nâng cấp Kiếm & Cung lên **Lv.2**; mở bán Lựu đạn nổ và Lựu đạn độc; mở thêm +5 ô hành trang.

---

### 2.2. Boss Tầng 3: Người Đá Khổng Lồ (Hộ vệ băng nguyên)
* **Script:** `StoneGolemBoss.cs`
* **Sprite:** `Boss_StoneGolem.png` (256×256)
* **Chỉ số:** HP: 1200, Tốc độ: 1.6 u/s, Tầm phát hiện: 16m.
* **Nội tại - Giáp Đá (Stone Armor):** Giảm 20% sát thương nhận vào khi HP > 50%. Kháng 80% lực đẩy lùi (Knockback).
* **Bộ chiêu thức:**
  1. *Nện Đất Chấn Động AoE (Double Ground Slam):* Giơ 2 tay lên cao, telegraph vòng tròn bán kính 4.5m trong 0.8s. Nện mạnh tạo sóng xung kích băng giá gây 45 DMG.
  2. *Quét Tay Hình Nón (Cone Arm Sweep 120°):* Telegraph hình nón cự ly 2.8m trong 0.5s. Càn quét cánh tay đá gây 40 DMG.
  3. *Ném Tảng Đá Lớn (Rock Boulder Throw):* Nhấc tảng đá ném tầm xa gây 35 DMG khi người chơi kiting ở xa.
* **Chiến lợi phẩm (100% rơi):** 1× `golem_core` ("Tủy đá nguyên khối").
* **Mở khóa nhà hàng:** Chế biến món **Tủy Đá Hấp Rêu Cổ** (240 Gold).
* **Mở khóa nâng cấp:** Cho phép nâng cấp Kiếm & Cung lên **Lv.4 (Cấp tối đa)**; mở thêm +5 ô hành trang.

---

### 2.3. Boss Tầng 4: Đôi Hiệp Sĩ (Song kiếm cấm vệ - Dual Boss)
* **Script:** `DualKnightsBoss.cs` (gồm 2 thành viên: `SwordKnightMember.cs` & `SpearKnightMember.cs`).
* **Sprite:** `Boss_SwordKnight.png` & `Boss_SpearKnight.png` (200×256).
* **Chỉ số:** Mỗi hiệp sĩ 600 HP (Tổng: 1200 HP), Tốc độ: 3.8 u/s.
* **Bộ chiêu thức phối hợp:**
  1. *Hiệp sĩ Kiếm (Sword Knight):* Áp sát lướt chém combo 3 nhát liên hoàn, mỗi nhát 35 DMG.
  2. *Hiệp sĩ Giáo (Spear Knight):* Giữ cự ly tầm trung (3-5m), telegraph tia đâm đỏ dài 5.5m trong 0.5s, sau đó đâm thương cực nhanh với tốc độ 12.0 u/s gây 45 DMG xuyên thấu.
* **Cơ chế Phối hợp (Dual Synergy - Enrage):**
  * Đồng bộ thanh máu đôi trên giao diện `BossHealthBarUI`.
  * Khi 1 trong 2 con chết trước, con còn lại sẽ ngay lập tức **HÓA CUỒNG**: hồi phục 200 HP, tăng 35% tốc độ di chuyển và tăng 30% sát thương đòn đánh.
* **Chiến lợi phẩm (100% rơi):** 1× `knight_egg` ("Trứng thiết giáp quái").
* **Mở khóa nhà hàng:** Chế biến món **Bữa Tiệc Thiết Kỵ Hoàng Gia** (350 Gold).

---

### 2.4. Boss Tầng 5: Gwyn - Thần Sáng Tạo (Final Boss)
* **Script:** `GwynFinalBoss.cs`
* **Sprite:** `Boss_Gwyn.png` (256×256)
* **Chỉ số:** HP: 2500, Tốc độ: 3.5 u/s, Tầm phát hiện: 25m.
* **Bộ chiêu thức Thần Thánh:**
  1. *Mưa Đạn Ma Pháp (Magic Bullet Hell):* Phóng loạt 12-16 viên đạn ma pháp tỏa đều 360° bay khắp phòng đấu, mỗi viên gây 55 DMG.
  2. *Cột Năng Lượng Thiên Phạt (Celestial Pillar):* Tạo 4-6 vòng tròn đỏ cảnh báo đuổi sát bước chân người chơi, sau 0.75s giáng cột sét thần thánh từ trần nhà gây 75 DMG.
  3. *Sóng Kiếm Càn Quét (Crescent Sword Wave):* Vung đại kiếm thần thánh phóng 2 lưỡi sóng kiếm trăng khuyết khổng lồ bay xuyên sàn đấu, gây 65 DMG.
  4. *Phase 2 (HP < 50% - Divine Ascendance):* Tự động dịch chuyển về trung tâm, kích hoạt hào quang thần uy, thời gian hồi chiêu giảm 35%, tăng số lượng đạn ma pháp và cột sét.
* **Chiến lợi phẩm (100% rơi):** 1× `life_core` ("Hạt nhân Sinh Mệnh" - Cấp bậc Huyền thoại).
* **Kết thúc game:** Đánh bại Gwyn mở ra chiến thắng hoàn tất cốt truyện game.

---

## 3. HƯỚNG DẪN SỬ DỤNG & TEST TRONG UNITY

Toàn bộ hệ thống Boss đi kèm công cụ Editor tự động hóa nằm trên thanh Menu của Unity:

### Cách 1: Thử nghiệm trong Scene hiện hành
Trên thanh công cụ Unity Editor, chọn menu:
* `DungeonEater > NguyenMinhHiep > Spawn Boss Tang 1 (Ran Khong Lo)`
* `DungeonEater > NguyenMinhHiep > Spawn Boss Tang 3 (Nguoi Da Khong Lo)`
* `DungeonEater > NguyenMinhHiep > Spawn Boss Tang 4 (Doi Hiep Si)`
* `DungeonEater > NguyenMinhHiep > Spawn Boss Tang 5 (Gwyn Than Sang Tao)`

Hệ thống sẽ tự động khởi tạo Boss tại vị trí con trỏ SceneView, gắn SpriteRenderer, Collider2D, Rigidbody2D, Script AI và tự động gắn thanh máu `BossHealthBarUI` lên màn hình.

### Cách 2: Tạo Scene Đấu Trường Thử Nghiệm Riêng
Chọn menu:
* `DungeonEater > NguyenMinhHiep > Create Boss Testing Arena Scene`
Hệ thống sẽ tạo ra scene `Assets/Members/NguyenMinhHiep/Scenes/BossArena_Hiep.unity` để bạn có thể tự do đặt các Boss và test đòn đánh đối kháng với người chơi.

