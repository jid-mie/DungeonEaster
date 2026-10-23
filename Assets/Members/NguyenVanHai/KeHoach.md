# KẾ HOẠCH NÂNG CẤP HỆ THỐNG DUNGEON COMBAT SCENE
**Scene mục tiêu:** `Assets/Members/NguyenVanHai/Scenes/Dungeon.unity`  
**Quy chuẩn áp dụng:** `TaiLieu.md` (Game Action Roguelite + Nhà hàng)  
**Phạm vi can thiệp:** Chỉ sửa đổi nội bộ trong thư mục `Assets/Members/NguyenVanHai/`, tuyệt đối không gây ảnh hưởng ra bên ngoài.

---

## 1. TỔNG QUAN YÊU CẦU & THỐNG NHẤT NGUYÊN BẢN

### 1.1. Ma trận Kẻ địch (Đã cập nhật theo sprite khả dụng)
Dựa theo xác nhận từ Designer/Lập trình viên và bảng cân bằng `TaiLieu.md` (Mục 7.3 & 7.4):
1. **Slime Nhầy (Đột kích / Nhảy vồ):**
   - Sprite: `Assets/Sprites/Detailed/Monster_Slime.png`
   - Chỉ số: HP: 40 (T1) / 60 (T2), Tốc độ: 2.5 u/s, ATK: 8 (T1) / 12 (T2).
   - Hành vi: Di chuyển tuần tra/áp sát, khi vào bán kính 2m sẽ telegraph (nhấp nháy/thu mình 0.4s) rồi phóng chồm về phía người chơi.
   - Rơi đồ: 100% rơi `Chất nhầy dẻo` (tối đa 10 stack).
2. **Nấm Bào Tử (Thay thế Bộ Xương Cung Thủ - Tầm xa):**
   - Sprite: `Assets/Sprites/Detailed/Monster_Spore.png`
   - Chỉ số: HP: 30 (T1) / 45 (T2), Tốc độ: 2.0 u/s, ATK: 10 (T1) / 15 (T2).
   - Hành vi: Giữ khoảng cách 5–8m với người chơi, telegraph (phình to bào tử 0.5s), bắn đạn bào tử độc (`VFX_Poison_Spore_0..3`) về phía người chơi (tần suất 1.5s/viên).
   - Rơi đồ: `Mẩu xương vụn` / `Nấm hang phát quang` (tối đa 10 stack).
3. **Bọ Thiết Giáp (Thay thế Thây Ma Cận Chiến - Cận chiến Cào chém):**
   - Sprite: `Assets/Sprites/Detailed/Monster_Beetle.png`
   - Chỉ số: HP: 60 (T1) / 90 (T2), Tốc độ: 1.8 u/s, ATK: 12 (T1) / 18 (T2).
   - Hành vi: Đuổi bám trực tiếp, khi áp sát cự ly 1.2m sẽ telegraph (giương càng 0.4s) rồi vung đòn cào xé hình vòng cung (`VFX_Slash_Arc_0..3`).
   - Rơi đồ: `Thớ thịt khô` (tối đa 10 stack).
4. **Boss Tầng: King Boar (Lợn Rừng Khổng Lồ):**
   - Sprite: `Assets/Sprites/Detailed/Boss_KingBoar_Animations.png` & `Monster_Boss_KingBoar.png`.
   - Chỉ số: HP: 750, Tốc độ: 4.5 u/s.
   - Bộ kỹ năng:
     + Kỹ năng 1 (Lao húc): Telegraph báo hướng đỏ 0.6s, lao thẳng gây 35 DMG, làm rung màn hình.
     + Kỹ năng 2 (Dậm đất chấn động AoE): Dậm mạnh chân xuống đất gây sóng xung kích 28 DMG bán kính 3.5m (`VFX_Explosion_0..3`).
   - Rơi đồ: 100% rơi `Nạc vai heo rừng` (Nguyên liệu Boss).

### 1.2. Quyết định Thống nhất về Kích thước & Tỉ lệ (Scale & Proportions)
- **Xác nhận từ Trưởng nhóm / Designer:** Toàn bộ kích thước và tỉ lệ hiện trạng trong scene (HunterPlayer, Slime, Nấm Spore, Bọ Beetle, Bẫy sàn, Map) đã được căn chỉnh chuẩn xác trên màn hình Game View.
- **Nguyên tắc thực thi:** **GIỮ NGUYÊN 100% KÍCH THƯỚC VÀ TRANSFORM SCALE** hiện có trong scene, tuyệt đối không tự ý co giãn, thay đổi scale hoặc PPU của các đối tượng đồ họa. Mọi logic di chuyển, tầm đánh (Attack Range), hitbox va chạm sẽ lập trình tương thích hoàn hảo theo đúng kích thước thực tế này.

### 1.3. Thống nhất về Animation & Cơ chế Chiến đấu
- **Animator của Hunter Player:** Đồng bộ hoàn toàn với [Hunter.controller](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Animations/Player/Hunter/Hunter.controller) và các clip tấn công 4 hướng (`AttackDown`, `AttackUp`, `AttackLeft`, `AttackRight`) thông qua các tham số: `MoveX`, `MoveY`, `IsMoving`, `IsAttack`.
- **Animator của Quái vật:** Tích hợp trực tiếp với các Animator Controller & Clip đã cập nhật:
  + Nấm (Mushroom): Đồng bộ với [mushroom.controller](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Animations/Enemy/Mushroom/mushroom.controller) (`MoveX`, `MoveY`, `IsMoving`, `IsAttack`, `Idle`, `Walk`, `Attack`, `attacked`).
  + Slime & Bọ: Đồng bộ trực tiếp với các clip animation (`slimeidle`, `slimewalk`, `slimeattack`, `slimeAttacked` và `bugidle`, `bugwalk`, `bugattack`, `attacked`).
- **Cơ chế Bắn Cung (Bow Shooting):** Chưa có clip giương cung -> Khi ở chế độ Cung (phím Q), nhấp chuột trái (LMB) hoặc gồng bắn sẽ sinh mũi tên VFX ([VFX_Arrow_Normal_0.png](file:///d:/UnityProject/DungeonEaster/Assets/Sprites/Detailed/VFX_Arrow_Normal_0.png)) tự động xoay và bay chuẩn xác theo hướng con trỏ chuột (`ScreenToWorldPoint(Input.mousePosition)`).

---

## 2. KIẾN TRÚC HỆ THỐNG NÂNG CẤP CHI TIẾT

```
Assets/Members/NguyenVanHai/
├── Script/
│   ├── Combat/
│   │   ├── DamageFlash.cs         (Hiệu ứng nhấp nháy trắng đỏ khi trúng đòn)
│   │   ├── ArrowProjectile.cs     (Mũi tên cung của Hunter bay theo hướng chuột)
│   │   ├── SporeProjectile.cs     (Đạn nấm độc của Spore bắn thẳng player)
│   │   └── LootDrop.cs            (Item rớt trên sàn, bay về phía player khi bấm F hoặc tự hút)
│   ├── Enemies/
│   │   ├── EnemyBase.cs           (Lớp cơ sở máu, Animator driver, State Machine)
│   │   ├── SlimeAI.cs             (Logic nhảy vồ áp sát)
│   │   ├── SporeShooterAI.cs      (Logic bắn tầm xa giữ khoảng cách)
│   │   ├── BeetleMeleeAI.cs       (Logic cào chém cận chiến)
│   │   └── BossKingBoarAI.cs      (State machine Boss: Húc, Dậm đất, Cảnh báo AoE)
│   ├── Dungeon/
│   │   ├── DungeonDoor.cs         (Cửa ngục: Bấm phím F để mở và Disable toàn bộ GameObject cửa)
│   │   ├── DungeonGameManager.cs  (Điều phối luồng Dungeon, Wave, Victory/Defeat)
│   │   └── TrapHazard.cs          (Bẫy gai & đầm lầy độc trừ máu + làm chậm)
│   └── UI/
│       ├── DungeonHUD.cs          (Thanh máu HP, Thể lực Stamina, Vũ khí Q, Wave/Boss Bar)
│       ├── DungeonInventoryUI.cs  (Kế thừa thiết kế từ Restaurant_Hien, 10 slot túi + 4 quick slot)
│       └── DungeonEndScreens.cs   (Màn hình GameOver / Victory bằng sprite chuẩn)
```

### 2.1. Nâng cấp Player Combat (`HunterPlayer.cs`)
- Đã có khung điều khiển 8 hướng WASD, Dash i-frame 0.2s, 100 HP, 100 Stamina.
- Hoàn thiện:
  + Gắn kết nối với `DungeonHUD` (đồng bộ HP, Stamina, số bình máu R, trạng thái vũ khí Q).
  + Khi cầm Kiếm: Kích hoạt `IsAttack = true` gọi các clip tấn công 4 hướng của Hunter và sinh vệt chém `VFX_Slash_Arc`.
  + Khi cầm Cung: Tính toán hướng chuột, bắn mũi tên `ArrowProjectile` bay thẳng về phía con trỏ chuột.
  + Kiểm tra điều kiện "Không cho mở Inventory / Map khi đang bị quái target" theo mục 3.2.1 `TaiLieu.md`.

### 2.2. Hệ thống HUD & Inventory (Tham chiếu từ `Restaurant_Hien.unity`)
- **HUD (Thanh trạng thái chiến đấu):**
  + Tận dụng `Assets/Sprites/Detailed/UI_Kit_Sheet.png` kết hợp kỹ thuật Procedural Pixel 9-Slice (như `TavernSprites.cs`):
    * Thanh Máu (HP): Tối đa 100, thanh trượt màu đỏ viền đá tối gothic.
    * Thanh Thể Lực (Stamina): Tối đa 100, thanh trượt màu vàng chanh / xanh ngọc, tự hồi phục 10 điểm/s sau 0.5s không dash.
    * Icon Vũ khí hiện tại (Kiếm / Cung) + Phím tắt chỉ dẫn (Q: Đổi, LMB: Đánh nhẹ, RMB: Đánh mạnh, E: Kỹ năng).
    * Hàng Quick Item 4 ô (phím 1–4, R dùng bình máu).
    * Thanh Máu Boss (Boss Health Bar) xuất hiện ở đỉnh màn hình khi bước vào phòng Boss.
- **Inventory (Hòm đồ chiến đấu):**
  + Dựng tự động bằng code uGUI chuẩn mực (như `InventoryPanel.cs` của Ngô Văn Hiến):
    * 10 ô hành trang (Bag Slots) dạng lưới $5 \times 2$.
    * 4 ô Quick Slots đặt bên dưới (chuẩn 4 ô theo mục 3.2.1 `TaiLieu.md`).
    * Kéo thả hoặc click chuột phải (RMB) để gắn/tháo quick item.
    * Hiển thị mô tả item, số stack (tối đa 5 stack khi bắt đầu).
    * Tương thích 2 chiều: Tự nhận `PlayerInventory.Instance` nếu đi từ Scene Làng vào, hoặc tự sinh bộ túi đồ độc lập để test trực tiếp trong scene `Dungeon.unity`.

### 2.3. Cơ chế Cửa Dungeon (`doors-d1-opening-h_0`)
- Gắn script `DungeonDoor.cs` vào các đối tượng cửa:
  + Khi Player lại gần trong bán kính tương tác ($1.8$m): Hiển thị gợi ý UI nhỏ **"Nhấn [F] Mở cửa"**.
  + Khi người chơi nhấn phím **F**: Chạy hiệu ứng mở cửa (các frame sprite `_0` ➔ `_3`) rồi **DISABLE TOÀN BỘ GAMEOBJECT CỬA** (`gameObject.SetActive(false)`) theo đúng đề xuất của bạn, giúp lối đi thông thoáng 100% không lo vướng collider.

---

## 3. CÁC GIẢI PHÁP KỸ THUẬT SAU CÙNG ĐÃ THỐNG NHẤT

1. **Cơ chế Mở Cửa (Door Interaction):**
   - **Giải pháp sau cùng:** Nhấn phím **F** khi đứng gần cửa để mở. Cửa sẽ chạy frame mở và gọi `gameObject.SetActive(false)` để vô hiệu hóa hoàn toàn đối tượng cửa, giải phóng lối đi.
2. **Cơ chế Chết / Thắng (Victory & Defeat Flow):**
   - **Giải pháp sau cùng:** Khi test độc lập tại scene `Dungeon.unity`:
     + Khi HP = 0: Hiện màn hình Game Over (`UI_Screen_GameOver.png`) kèm nút **"Thử lại màn (Retry)"** để reload scene mượt mà.
     + Khi diệt xong Boss King Boar: Rơi 100% *Nạc vai heo rừng*, hiện portal chiến thắng và popup Victory (`UI_Screen_Victory.png`) kèm nút **"Chơi lại" / "Về làng"**.
3. **Tương thích Hệ thống Dữ liệu Inventory:**
   - **Giải pháp sau cùng:** Tự động kết nối với `PlayerInventory.Instance` của dự án nếu đi từ scene Làng sang; tự động fallback tạo bộ hòm đồ độc lập (10 ô hành trang + 4 ô quick item + 3 bình máu khởi đầu) nếu mở chạy trực tiếp tại scene `Dungeon.unity`.
4. **Tuyệt đối không can thiệp bên ngoài:**
   - Giữ nguyên 100% cấu hình Package của dự án và các thiết lập Camera gốc, toàn bộ mã nguồn và tài nguyên chỉnh sửa chỉ nằm bên trong `Assets/Members/NguyenVanHai/`.
