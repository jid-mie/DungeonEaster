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
  + Khi cầm Kiếm: Kích hoạt `IsAttack = true`, quét vùng sát thương hình **bán nguyệt $180^\circ$** theo hướng nhìn (4 hướng Down/Up/Left/Right bao trọn không gian $360^\circ$) và tự động sinh hiệu ứng vệt chém trăng khuyết phát sáng ([VFX_Slash_Arc_0..3](file:///d:/UnityProject/DungeonEaster/Assets/Sprites/Detailed/VFX_Slash_Arc_0.png)).
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

---

## 4. HƯỚNG DẪN GẮN COMPONENT THỦ CÔNG & THIẾT KẾ COMBAT QUÁI (TƯ VẤN CHUYÊN GIA)

### 4.1. Bảng Ánh Xạ Gắn Script / Component Thủ Công Vào Scene
Đã gỡ bỏ toàn bộ script setup scene tự động (`DungeonSceneSetupTool.cs`) để người lập trình tự chủ động thao tác trong Hierarchy của `Dungeon.unity`:

| Đối tượng (GameObject) trong Scene | Component cần gắn | Cấu hình tham số quan trọng trong Inspector |
| :--- | :--- | :--- |
| **`HunterPlayer`** (Nhân vật chính) | 1. `Rigidbody2D`<br>2. `CapsuleCollider2D`<br>3. `Animator`<br>4. `YSortOrder`<br>5. `HunterPlayer` (Script) | - **Rigidbody2D:** Body Type = Dynamic, Gravity Scale = 0, Collision Detection = Continuous, Constraints = Freeze Rotation Z.<br>- **CapsuleCollider2D:** Direction = Vertical, Size = (0.72, 0.28), Offset = (0.12, -0.05).<br>- **Animator:** Controller = `Hunter.controller`.<br>- **HunterPlayer:** Max Health = 100, Max Stamina = 100, Move Speed = 5, Dash Speed = 13, Dash Stamina Cost = 20, Sword Light/Heavy = 20/40, Bow Light/Heavy = 15/40. |
| **`DungeonGameManager`** (Empty GO tại gốc scene) | 1. `DungeonGameManager` (Script)<br>2. `DungeonHUD` (Script)<br>3. `DungeonInventoryUI` (Script)<br>4. `DungeonEndScreens` (Script) | - Điều phối toàn bộ vòng lặp Dungeon, tự động vẽ HUD (HP, Stamina, Quick slots, Bình máu), Inventory túi đồ 10 ô phím Tab/B, và Màn hình Game Over / Victory độc lập không phụ thuộc bên ngoài. |
| **Quái Slime** (Các con trong group `slime`: `Monsters_4Dir_Sheet_0...`) | 1. `Rigidbody2D`<br>2. `CircleCollider2D`<br>3. `Animator`<br>4. `DamageFlash`<br>5. `SlimeAI` (Script)<br>6. `YSortOrder` | - **Rigidbody2D:** Body Type = Dynamic, Gravity = 0, Freeze Rotation Z.<br>- **CircleCollider2D:** Radius = 0.45, isTrigger = false.<br>- **Animator:** Controller = `slime.controller`.<br>- **SlimeAI:** Max Health = 40, Move Speed = 2.5, Attack Damage = 8, Attack Range = 2.0, Telegraph Time = 0.4, Lunge Speed = 7.5, Drop Item = `slime_gel` ("Chất nhầy dẻo"). |
| **Quái Bọ Thiết Giáp** (Các con trong group `Nhen`: `Monsters_4Dir_Sheet_8...`) | 1. `Rigidbody2D`<br>2. `CircleCollider2D`<br>3. `Animator`<br>4. `DamageFlash`<br>5. `BeetleMeleeAI` (Script)<br>6. `YSortOrder` | - **Rigidbody2D:** Body Type = Dynamic, Gravity = 0, Freeze Rotation Z.<br>- **CircleCollider2D:** Radius = 0.45, isTrigger = false.<br>- **Animator:** Controller = `bug.controller`.<br>- **BeetleMeleeAI:** Max Health = 60, Move Speed = 1.8, Attack Damage = 12, Claw Attack Range = 1.2, Telegraph Time = 0.4, Slash Radius = 1.0, Drop Item = `dried_meat` ("Thớ thịt khô"). |
| **Quái Nấm Bào Tử** (Các con trong group `Nam`: `Monsters_4Dir_Sheet_16...`) | 1. `Rigidbody2D`<br>2. `CircleCollider2D`<br>3. `Animator`<br>4. `DamageFlash`<br>5. `SporeShooterAI` (Script)<br>6. `YSortOrder` | - **Rigidbody2D:** Body Type = Dynamic, Gravity = 0, Freeze Rotation Z.<br>- **CircleCollider2D:** Radius = 0.45, isTrigger = false.<br>- **Animator:** Controller = `mushroom.controller`.<br>- **SporeShooterAI:** Max Health = 30, Move Speed = 2.0, Attack Damage = 10, Min Keep Distance = 4.0, Max Shoot Distance = 8.0, Telegraph Duration = 0.45, Drop Item = `bone_fragment` ("Mẩu xương vụn"). |
| **Boss King Boar** (Khu vực Boss room) | 1. `Rigidbody2D`<br>2. `CircleCollider2D` / `CapsuleCollider2D`<br>3. `DamageFlash`<br>4. `BossKingBoarAI` (Script)<br>5. `YSortOrder` | - **Rigidbody2D:** Body Type = Dynamic, Gravity = 0, Freeze Rotation Z.<br>- **Collider:** Radius = 1.0.<br>- **BossKingBoarAI:** Max Health = 750, Move Speed = 3.5, Charge Speed = 9.0 (35 DMG, Telegraph 0.6s), Slam Radius = 3.5 (28 DMG AoE, Telegraph 0.5s), Drop Item = `boar_shoulder` ("Nạc vai heo rừng"). |
| **Cửa Dungeon** (Các cửa `doors-d1-opening-h_0...`) | 1. `BoxCollider2D`<br>2. `DungeonDoor` (Script) | - **BoxCollider2D:** isTrigger = false (chặn đi qua khi cửa đóng).<br>- **DungeonDoor:** Interaction Radius = 1.8. Khi người chơi đứng gần nhấn [F], cửa phát hiệu ứng mở và tự động `gameObject.SetActive(false)` để dọn trống hoàn toàn lối đi. |
| **Bẫy Gai Sàn** (Các ô `floor_spikes_anim_f0...`) | 1. `BoxCollider2D`<br>2. `Trap` (Script) | - **BoxCollider2D:** isTrigger = true.<br>- **Trap:** Spike Damage = 10, Retract Duration = 1.5s, Active Duration = 1.0s. Tự động chồi gai gây sát thương chu kỳ. |

---

### 4.2. Đánh Giá Chuyên Sâu Animation (Unity System & Game Design)
1. **Dưới góc độ Chuyên gia Hệ thống Unity (Engine Architecture):**
   - **Lỗi `Loop Time: 0` trên clip Idle/Walk:** Clip `slimeidle.anim` hiện đang để `m_LoopTime: 0`. Trong Unity, khi quái đứng yên, clip idle chạy hết 1 vòng sẽ dừng khựng vĩnh viễn ở frame cuối. **Khắc phục:** Tích chọn `Loop Time = true` trên Inspector của file animation.
   - **Transition Duration 0.25s trong 2D Sprite:** Các transition từ AnyState trong `slime.controller` đang để mặc định 0.25s. Do 2D sprite không thể blend khung xương, 0.25s này chỉ gây delay đơ hình trước khi chuyển động tác. **Khắc phục:** Chỉnh toàn bộ `Transition Duration = 0` và bỏ check `Has Exit Time`.
   - **`Can Transition To Self` trên AnyState:** Đang bật `true` trên AnyState transitions kèm biến bool (`IsMoving`). Điều này khiến Animator re-enter state ở mỗi frame, dẫn tới sprite bị giật về frame 0 liên tục. **Khắc phục:** Bỏ chọn (uncheck) `Can Transition To Self`.
   - **Đồng bộ hóa tên tham số:** Trong `bug.controller` đang là `isAttacked` (chữ i thường), trong khi `slime.controller` là `IsAttacked` (chữ I hoa). Nên thống nhất viết hoa chữ cái đầu.

2. **Dưới góc độ Chuyên gia Thiết kế Game (Combat Feel & Game Juice):**
   - **Vấn đề Clip Đánh 1 Frame (0.016s):** Các clip tấn công hiện tại của quái chỉ có 1 frame. Một đòn đánh chuẩn trong Action Roguelite cần tuân thủ **Quy tắc 3 Nhịp**:
     + *Nhịp 1: Anticipation (Báo hiệu / Gồng đòn - 0.35s đến 0.5s):* Quái nén người, phình to hoặc giương móng vuốt để người chơi kịp nhận biết và bấm Space Dash né (có 0.2s bất tử i-frame).
     + *Nhịp 2: Active Hitbox (Vung đòn - 0.1s):* Đòn đánh chạm người chơi và tính sát thương.
     + *Nhịp 3: Recovery (Hồi thế / Sơ hở - 0.3s):* Quái đứng khựng sau đòn vung, tạo cơ hội phản công.
   - **Giải pháp Code-driven Juice bù đắp sprite:** Các script AI (`SlimeAI.cs`, `BeetleMeleeAI.cs`, `SporeShooterAI.cs`) đã tích hợp thuật toán co dãn Squash & Stretch bằng code (Slime dẹt người 0.4s trước khi phóng, Nấm phình to 1.2x trước khi bắn, Bọ nháy đỏ trước khi cào) để tạo cảm giác giương đòn trực quan, rõ ràng cho người chơi.

---

### 4.3. Đề Xuất Thiết Kế Cơ Chế Chiến Đấu Quái Vật (Monster Combat Design)
1. **Phân tầng vai trò khắc chế (Combat Archetypes):**
   - **Slime (Đột kích / Pouncer):** Chuyên lao vồ chớp nhoáng khi vào tầm 2m. Buộc người chơi phải tập trung quan sát nhịp co mình để phản xạ Dash né đòn.
   - **Bọ Thiết Giáp (Đấu sĩ / Frontline Bruiser):** Máu dày, áp sát cào chém vòng cung. Đóng vai trò khiên chắn che đạn cho quái phía sau, buộc người chơi phải giữ khoảng cách hoặc dùng cung tỉa trước.
   - **Nấm Bào Tử (Pháo thủ / Artillery Kiter):** Giữ khoảng cách 5–8m, chủ động lùi lại nếu Player áp sát dưới 4m. Bắn cầu độc tím bay chậm tạo áp lực né đạn liên tục.
   - **Boss King Boar (Trùm càn quét):** Luân phiên giữa Lao húc thẳng đường dài (nếu né được Boss đâm vào tường choáng 1.2s) và Dậm đất sóng chấn động AoE bán kính 3.5m khi người chơi chém giáp lá cà quá lâu.
2. **Cơ chế Token Tấn Công (Attack Token System):**
   - Giới hạn tối đa 2 quái cận chiến được kích hoạt trạng thái tấn công trong cùng một thời điểm, các quái còn lại sẽ di chuyển lượn vòng bao vây nhằm tránh dồn sát thương phi lý (burst damage) khiến người chơi bốc hơi không kịp phản ứng.
3. **Phản hồi va chạm (Impact Feedback):**
   - Kết hợp `DamageFlash` (nháy trắng 0.08s), đẩy lùi (Knockback) và âm thanh/vfx chém để đòn đánh có trọng lượng và cảm giác đã tay (Combat Juice).

---

### 4.4. Xử Lý Lỗi Đi Xuyên Tường & Nâng Cấp VFX Đòn Đánh Quái (Bọ & Nấm)
1. **Nguyên Nhân Quái Bọ Có Collider Nhưng Vẫn Đi Xuyên Tường:**
   - **Nguyên nhân 1 (Chính): Thiếu `Rigidbody2D` trên Quái:** Trong Unity 2D Physics, nếu một đối tượng di chuyển chỉ có `Collider2D` mà **không có `Rigidbody2D`**, Unity coi nó là *Static Collider*. Physics Engine không tính toán va chạm vật lý giữa 2 collider tĩnh (hoặc collider không có Rigidbody).
     * *Khắc phục:* Gắn `Rigidbody2D` vào con bọ (`Body Type = Dynamic`, `Gravity Scale = 0`, `Constraints = Freeze Rotation Z`).
   - **Nguyên nhân 2: Collider bị tích ô `Is Trigger`:** Nếu ô `Is Trigger` trên `CircleCollider2D` của con bọ (hoặc của tường) bị tích chọn, collider sẽ trở thành cảm biến xuyên thấu, không cản tường được.
     * *Khắc phục:* Bỏ tích chọn `Is Trigger` (để `isTrigger = false`).
   - **Nguyên nhân 3: Tilemap Tường (Wall) trong Scene chưa có `TilemapCollider2D`:** GameObject `Wall` trong Hierarchy cần có component `TilemapCollider2D` để toàn bộ ô gạch tường có thể cản vật lý.
2. **Nâng Cấp Đòn Đánh Quái Bọ (Hình Bán Nguyệt 180° + VFX Vệt Cào):**
   - Đã nâng cấp [BeetleMeleeAI.cs](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Script/Enemies/BeetleMeleeAI.cs): Đòn cào chuyển sang quét hình bán nguyệt $180^\circ$ theo hướng lao tới Player.
   - Hỗ trợ mảng `Claw Slash Sprites` (kéo 4 sprite `VFX_Slash_Arc_0..3` vào Inspector): Tự động đổi màu vệt chém sang đỏ cam hung tợn (`slashColor`) và xoay góc chuẩn xác về phía người chơi.
3. **Nâng Cấp VFX Bắn Bào Tử Độc Của Nấm (Ranged VFX):**
   - Đã nâng cấp [SporeShooterAI.cs](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Script/Enemies/SporeShooterAI.cs) và [SporeProjectile.cs](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Script/Combat/SporeProjectile.cs).
   - Hỗ trợ mảng `Spore Anim Sprites`: Khi kéo 4 sprite từ `Assets/Sprites/Detailed/VFX_Poison_Spore_0..3` vào Inspector của Nấm, quả cầu độc khi bắn ra sẽ tự động xoay cuộn 4 frame animation lượn sóng ma mị trên đường bay.
4. **Xử Lý Lỗi Hiện Prompt 'Nhấn [F] Mở Cửa' Xuyên Tường:**
   - **Nguyên nhân:** Cửa ngục `doors-d1-opening-h_0` đặt tại vách ngăn giữa 2 phòng trước đây chỉ kiểm tra khoảng cách hình tròn $2.0\text{m}$ mà không kiểm tra hướng nhìn của Hunter và không kiểm tra tường chắn, khiến đứng sát vách tường phòng bên cạnh vẫn bị hiện prompt mở cửa.
   - **Khắc phục:** Đã nâng cấp [DungeonDoor.cs](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Script/Dungeon/DungeonDoor.cs):
     + Giảm bán kính tương tác chuẩn về $1.3\text{m}$ (1 ô gạch trước mặt).
     + Bắt buộc Hunter phải quay mặt về phía cánh cửa (`Vector2.Dot > 0.4`).
     + Thêm `Physics2D.Linecast` phát hiện vật cản: nếu có bức tường ngăn cách giữa Hunter và Cửa thì tuyệt đối không hiển thị prompt.
5. **Nâng Cấp Bẫy Gai (Sprites Trạng Thái & Khắc Phục Lỗi Đứng Yên Không Trừ Máu):**
   - **Thêm Sprite Trạng Thái Kích Hoạt:** [Trap.cs](file:///d:/UnityProject/DungeonEaster/Assets/Members/NguyenVanHai/Script/Dungeon/Trap.cs) đã bổ sung các trường:
     + `activeSprite`: Sprite khi bẫy gai chồi lên kích hoạt gây sát thương (bạn có thể kéo sprite kích hoạt vào đây).
     + `warningSprite`: Sprite cảnh báo nhú gai nhẹ 0.4s.
     + `retractedSprite`: Sprite khi gai thụt xuống an toàn.
     + `popUpAnimationFrames`: Chuỗi frame hoạt ảnh chồi gai nếu có.
   - **Khắc Phục Lỗi Đứng Yên Không Trừ Máu:**
     + *Nguyên nhân:* Unity 2D Physics tự động đưa Rigidbody2D vào trạng thái **Sleep** khi đứng yên không di chuyển, khiến hàm `OnTriggerStay2D` bị ngưng gọi!
     + *Khắc phục:* Trong `Trap.cs`, khi bẫy chồi gai (`isSpikeActive = true`), hệ thống dùng `trapCol.OverlapCollider` quét trực tiếp đối tượng đang đứng đè lên bẫy tại thời điểm gai nhô lên để trừ máu ngay lập tức (không phụ thuộc vào việc nhân vật có di chuyển hay Rigidbody đang sleep). Đồng thời đặt `rb.sleepingMode = RigidbodySleepingMode2D.NeverSleep` trên `HunterPlayer`.

6. **Thi?t L?p H? Th?ng Cung & T�n (Bow & Arrow System):**
   - **T�i nguy�n Sprite:**
     + weapon_bow.png (Cung ? tr?ng th�i thu?ng).
     + weapon_bow_2.png (Cung khi giuong d�y / k�o cang t? l?c).
     + VFX_Arrow_Normal_0.png (Mui t�n co b?n - Quick Shot LMB).
     + VFX_Arrow_Magic_0.png (Mui t�n ph�p xuy�n th?u - Charged Shot RMB / Skill E).
   - **Co ch? hi?n th? vu kh� (Bow Visual & Pivot):**
     + T? d?ng sinh BowVisual g?n v�o Player khi chuy?n sang Cung (ph�m Q). ?n khi d�ng Ki?m.
     + Cung t? xoay 360 d? theo con tr? chu?t. T? d?ng l?t tr?c Y khi ng?m sang tr�i d? kh�ng b? ngu?c.
     + T? d?ng d?i sang sprite weapon_bow_2 khi gi? RMB t? l?c, h?i v? weapon_bow khi b?n.
   - **Co ch? d?n bay:**
     + Sorting Layer Projectiles_VFX (order 6000) ch?ng ch�m du?i s�n g?ch.

   - **S?a L?i B?n Cung (�?n Bi?n M?t & Vung Ki?m):**
     + B? qua va ch?m gi?a Player v� Arrow b?ng Physics2D.IgnoreCollision v� ki?m tra HunterPlayer trong ArrowProjectile (kh?c ph?c l?i mui t�n va v�o ch�nh Player v� t? h?y ngay khi b?n sang tr�i/ph?i/du?i).
     + S?a ph�p chi?u t?a d? chu?t ScreenToWorldPoint(mouseScreen.z) chu?n theo d? s�u camera.
     + Lo?i b? trigger ch�m ki?m khi dang ? ch? d? Cung (ch? xoay ngu?i UpdateAnimator theo hu?ng b?n).
     + T? d?ng sinh ho?c li�n k?t BowVisual k�m sprite fallback ch?ng t�nh tr?ng cung v� h�nh.
