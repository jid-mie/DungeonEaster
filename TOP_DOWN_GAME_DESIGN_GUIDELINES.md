# TOP-DOWN GAME DESIGN GUIDELINES

> **Mục đích:** Tài liệu quy chuẩn để AI agent thiết kế, tạo, tích hợp, đánh giá và mở rộng asset/gameplay cho game top-down.
>
> **Profile mặc định của dự án tham chiếu:** *Dungeon Eater* — pixel-art action roguelite / hack-and-slash kết hợp quản lý nhà hàng.
>
> **Cách dùng:** Đưa toàn bộ file này vào context của agent trước khi giao tác vụ thiết kế nhân vật, quái, vũ khí, item, tileset, map, UI, VFX, animation hoặc gameplay.

---

## 0. Quy tắc ưu tiên và cách agent phải làm việc

### 0.1 Thứ tự ưu tiên

Khi có mâu thuẫn, áp dụng theo thứ tự sau:

1. Yêu cầu trực tiếp mới nhất của người dùng.
2. Game Design Document (GDD), tài liệu concept/art bible và các quyết định đã được duyệt của dự án.
3. Quy chuẩn riêng của game trong file này.
4. Quy chuẩn top-down chung và đề xuất mặc định của agent.
5. Sở thích cá nhân của agent.

Không được tự ý đổi gameplay, phong cách hình ảnh, phím điều khiển, camera, kích thước lưới hoặc cấu trúc dự án chỉ để phù hợp với sở thích của agent.

### 0.2 Bắt buộc kiểm tra trước khi thực hiện

Trước khi sửa repository, agent phải:

- Kiểm tra engine, version, cấu trúc thư mục, convention đặt tên, hệ thống input, camera, animation, physics/collision và pipeline import asset đang dùng.
- Tìm asset, prefab/scene, script, data file và tài liệu hiện có để tái sử dụng; không tạo bản trùng nếu đã có bản tương đương.
- Xác định rõ deliverable: concept/specification, image prompt, sprite sheet, tileset, prefab, data definition hay code tích hợp.
- Đọc GDD và phân biệt **yêu cầu đã chốt** với **đề xuất chưa được duyệt**.
- Nếu thiếu thông tin không cản trở tiến độ, dùng mặc định trong file này và ghi rõ giả định. Nếu thiếu thông tin làm thay đổi kiến trúc hoặc tạo ra asset phải làm lại, hỏi một câu ngắn để xác nhận.

### 0.3 Khi hoàn tất một tác vụ

Agent phải báo cáo:

- Những gì đã tạo hoặc sửa và đường dẫn cụ thể.
- Các quyết định/giả định đã sử dụng.
- Những phần đã tích hợp vào game, những phần mới chỉ là mockup/concept.
- Kiểm tra đã thực hiện: kích thước, alpha, frame layout, import settings, collision, sorting, build/test nếu có.
- Việc còn thiếu hoặc rủi ro; không được tuyên bố đã tích hợp/chạy thành công nếu chưa kiểm tra.

### 0.4 Nguyên tắc không được vi phạm

- Không dùng hình đẹp nhưng khó đọc trong gameplay để thay thế khả năng nhận diện.
- Không tạo sprite sheet mà không nêu kích thước frame, số hàng/cột, hướng nhìn và thứ tự animation.
- Không để visual sprite, hitbox và điểm tương tác lệch nhau một cách khó hiểu.
- Không đặt collider trên toàn bộ hình minh họa nếu collider đó làm va chạm thiếu công bằng.
- Không dùng màu sắc làm phương tiện duy nhất để phân biệt loại vật phẩm/nguy hiểm.
- Không tự thêm rarity, kỹ năng, chỉ số, công thức chế biến hoặc luật mất đồ nếu chưa gắn chúng với thiết kế hệ thống hiện hữu.
- Không chỉnh sửa/xóa asset đang dùng mà chưa kiểm tra tham chiếu.
- Không sao chép nguyên mẫu nhận diện độc quyền từ game khác. Có thể học nguyên tắc, không sao chép sprite, logo, nhân vật hoặc UI cụ thể.

---

## 1. Phân loại thiết kế game top-down

“Top-down” mô tả hướng nhìn/camera, không phải một thể loại gameplay duy nhất. Khi thiết kế, trước hết phải chỉ rõ **kiểu camera + kiểu di chuyển + kiểu chiến đấu + cấu trúc màn chơi**.

### 1.1 Các kiểu camera và trình bày hình ảnh

| Kiểu | Đặc điểm | Điểm mạnh | Rủi ro | Phù hợp khi |
|---|---|---|---|---|
| **True top-down / overhead** | Nhìn gần thẳng từ trên xuống; mặt nhân vật thường ít thấy | Dễ đọc mặt bằng, phù hợp dungeon, stealth, strategy | Khó thể hiện chiều cao, mặt và hướng quay nhân vật | Cần đọc bố cục phòng, vật cản, khu vực nguy hiểm |
| **2D top-down orthographic** | Sprite 2D trên mặt phẳng, camera orthographic; hướng Bắc/Nam/Đông/Tây hoặc 8 hướng | Dễ làm pixel art, camera ổn định, phù hợp tilemap | Cần xử lý che khuất và sắp xếp trước/sau | Action RPG, roguelite, dungeon crawler |
| **Oblique / 3/4 top-down** | Camera nghiêng vừa phải; nhìn được mặt trên và một phần mặt bên | Tạo chiều sâu, nhân vật/đồ vật dễ có khối | Tile/collision/sort phức tạp hơn top-down thẳng | Game hành động có môi trường nhiều vật thể |
| **Isometric** | Mặt bằng thể hiện theo trục xiên, thường dùng lưới diamond hoặc isometric | Đẹp, có chiều sâu kiến trúc | Di chuyển, chọn ô, occlusion và va chạm khó hơn; art phải nhất quán góc chiếu | Tactics, RPG, xây dựng, ARPG phong cách isometric |
| **2.5D top-down** | Môi trường 3D nhưng camera nhìn từ trên/xiên; có thể dùng sprite hoặc model 3D | Ánh sáng, chiều cao và hiệu ứng không gian linh hoạt | Tốn công shader, ánh sáng, camera và tối ưu hơn 2D | Dự án cần khối 3D nhưng giữ trải nghiệm top-down |

**Quy tắc:** Không trộn top-down thẳng với isometric nếu chưa định nghĩa góc chiếu. Một bộ tileset/isometric không thể dùng tùy tiện với map top-down vuông mà vẫn giữ đúng cảm nhận phối cảnh.

### 1.2 Các kiểu gameplay phổ biến

| Kiểu gameplay | Điều khiển thường gặp | Yêu cầu thiết kế nổi bật |
|---|---|---|
| **Top-down action RPG** | Di chuyển 4/8 hướng, tấn công, kỹ năng, tương tác | Nhịp chiến đấu rõ, hitbox công bằng, chỉ số và nâng cấp dễ hiểu |
| **Twin-stick shooter** | Một stick/bộ phím di chuyển, một stick/chuột ngắm | Hướng bắn độc lập với hướng di chuyển, đạn/projectile rõ, tránh rối khi đông địch |
| **Hack-and-slash / action roguelite** | Di chuyển, combo, heavy attack, dash/dodge, skill | Đòn đánh có wind-up/active/recovery; telegraph của địch; build đa dạng; vòng lặp run/reward |
| **Dungeon crawler / room-based** | Di chuyển qua phòng/nút hành lang, mở cửa sau khi thắng | Phòng có mục đích, nhịp độ giữa giao tranh và nghỉ, đường đi và khóa/mở rõ ràng |
| **Top-down tactical / grid-based** | Chọn ô hoặc hành động theo lượt | Quy tắc lượt, phạm vi, tầm nhìn, đường đi và trạng thái phải đọc dễ |
| **Survival / crafting** | Di chuyển liên tục, thu thập, chế tạo, quản lý tài nguyên | Vòng lặp tài nguyên, khả năng đọc vật phẩm, áp lực thời gian/rủi ro |
| **Management / sim top-down** | Chọn đối tượng, kéo-thả, menu và hành động ngắn | Đọc trạng thái, luồng công việc, phản hồi khi hoàn thành, chỉ số kinh tế |
| **Hybrid action + management** | Chuyển giữa điều khiển nhân vật và UI/quản lý | Hai pha phải bổ trợ nhau; phần thưởng ở pha này tạo mục tiêu ở pha kia |

### 1.3 Kiểu phù hợp với Dungeon Eater

Theo GDD hiện có, định hướng là:

- Camera: **2D top-down/overhead**, pixel art. Cần xác nhận góc nhìn cuối cùng từ scene/game prototype trước khi vẽ tileset hàng loạt.
- Gameplay chiến đấu: **action roguelite + hack-and-slash**, chiến đấu chủ yếu theo thời gian thực.
- Cấu trúc khám phá: dungeon có phòng/đường đi, quái thường, elite/boss, loot, vật cản và lựa chọn route.
- Pha ngoài dungeon: quản lý/cooking/serving nhà hàng.
- Vòng lặp cốt lõi: **Hunt → Loot → Cook → Serve → Gold/Reputation → Upgrade → Run tiếp**.
- Chủ đề: fantasy; con người bên ngoài dungeon, quái vật trong dungeon.
- Màu sắc theo ngữ cảnh: ngoài dungeon xanh lá/tươi sáng; trong dungeon tím/tối; nhà hàng nâu gỗ/cổ điển.

Không để pha quản lý sử dụng một style khác biệt hoàn toàn với pha dungeon. Hai pha có thể có palette khác nhau nhưng vẫn phải chia sẻ ngôn ngữ pixel, ánh sáng, outline và mức chi tiết.

---

## 2. Art Direction / Art Bible

### 2.1 Chốt thông số trước khi tạo asset

Trước khi agent vẽ hoặc tạo asset hàng loạt, cần có một bản **Art Bible** ghi các thông số sau:

- Góc camera: top-down thẳng, 3/4 hay isometric.
- Engine và pipeline import.
- Kích thước tile nền, ví dụ 16×16 hoặc 32×32 px.
- Kích thước nhân vật cơ sở và tỷ lệ với tile; đây là kích thước vùng vẽ, không phải kích thước collider.
- Độ phân giải tham chiếu của camera/game view.
- Palette chung, màu outline, hướng sáng và quy tắc bóng đổ.
- Mức chi tiết: số màu tương đối, độ tương phản, mật độ texture.
- Quy tắc anti-aliasing: pixel art nên dùng cạnh pixel rõ; không tạo pixel art bằng cách làm mờ/resize bilinear.
- Quy tắc alpha/transparency, padding/extrusion và tên file.
- Tỷ lệ UI icon so với sprite trong gameplay.

Nếu dự án chưa chốt, các giá trị sau chỉ là **mặc định đề xuất**, không phải sự thật bắt buộc của GDD:

| Hạng mục | Mặc định đề xuất |
|---|---|
| Tile grid | 16×16 px hoặc 32×32 px; chọn một grid chính |
| Nhân vật thường | Khoảng 24–40 px chiều cao sprite, tùy độ chi tiết |
| Quái lớn/boss | Có thể lớn hơn 1,5–3 lần nhân vật nhưng cần kiểm tra trong camera thực tế |
| Độ phân giải tham chiếu | Chọn theo target screen; ví dụ 320×180 hoặc 640×360 cho phong cách pixel art |
| Filter | Point/nearest-neighbor nếu mục tiêu là pixel art sắc nét |
| Compression | Kiểm tra để tránh làm bệt hoặc tạo artefact ở viền pixel |
| Pivot nhân vật | Gần chân/điểm tiếp xúc mặt đất |

Không tự động chốt 16×16 hoặc 32×32 nếu repository đã có bộ asset chuẩn. Tính nhất quán quan trọng hơn con số cụ thể.

### 2.2 Nguyên tắc thống nhất phong cách

Mọi asset mới phải tuân thủ:

1. **Silhouette trước chi tiết:** Nhân vật, quái và item phải phân biệt được ở kích thước gameplay và khi giảm độ sáng.
2. **Palette có kiểm soát:** Mỗi asset cần màu nền, màu khối chính, màu bóng và màu highlight rõ ràng. Tránh quá nhiều màu gần giống nhau.
3. **Nguồn sáng nhất quán:** Xác định một hướng sáng chính cho từng môi trường. VFX có thể tự phát sáng nhưng không nên khiến bóng đổ của cả cảnh mâu thuẫn.
4. **Outline nhất quán:** Chọn outline tối, outline theo màu hoặc không outline; không thay đổi tùy tiện giữa nhân vật, enemy và prop.
5. **Độ chi tiết theo vai trò:** Boss có thể nhiều chi tiết hơn quái thường; item rơi cần ít chi tiết hơn icon inventory nếu icon hiển thị nhỏ.
6. **Tương phản theo chức năng:** Nguy hiểm, đòn đánh, loot quan trọng và điểm tương tác phải nổi bật hơn chi tiết trang trí.
7. **Không gây nhiễu gameplay:** Tránh pattern nền quá mạnh, màu nền trùng enemy hoặc hiệu ứng che mất telegraph.
8. **Tỉ lệ thống nhất:** Nhân vật và prop phải đúng tỉ lệ tương quan trong cùng một scene.
9. **Không resize tự do:** Nếu bắt buộc đổi kích thước, kiểm tra pixel grid, outline, pivot, animation và camera sau khi resize.
10. **Asset tham chiếu rõ ràng:** Duy trì một bảng sprite mẫu để agent so sánh style trước khi tạo asset mới.

### 2.3 Palette theo khu vực Dungeon Eater

| Khu vực | Hướng palette | Mục tiêu cảm xúc | Điều cần tránh |
|---|---|---|---|
| Ngoài dungeon / làng Dun | Xanh lá, đất, vàng nhạt, ánh sáng ấm | Khám phá, hy vọng, không khí sinh hoạt | Không quá bão hòa khiến UI và NPC chìm |
| Dungeon | Tím, xanh lạnh, xám đá, vùng sáng hạn chế | Bí ẩn, căng thẳng, nguy hiểm | Không tối đến mức mất enemy/loot/đường đi |
| Nhà hàng | Nâu gỗ, màu thức ăn ấm, ánh đèn vàng | An toàn, ấm áp, bận rộn nhưng vui | Không làm icon order/food thiếu tương phản |
| Elite/Boss | Dùng màu cảnh làm nền; chọn màu accent riêng | Đe dọa và dễ nhận diện | Không dùng chỉ màu để báo attack; phải có hình/animation/VFX |

Đây là palette direction, chưa phải mã màu đã chốt. Nếu agent đề xuất mã màu cụ thể, hãy tạo swatch/palette sheet và chờ duyệt trước khi áp dụng toàn dự án.

---

## 3. Thiết kế nhân vật người chơi (Player Character)

### 3.1 Character brief bắt buộc

Mỗi nhân vật phải có một brief ngắn chứa:

- Tên/ID và vai trò trong gameplay.
- Fantasy/ý niệm cốt lõi của nhân vật.
- Silhouette đặc trưng ở kích thước nhỏ.
- Tỉ lệ cơ thể, chiều cao sprite và kích thước collider.
- Đặc điểm nhận diện từ xa: mũ, tóc, áo choàng, vũ khí, màu accent hoặc hình dạng đặc biệt.
- Hướng nhìn và cách hiển thị 4/8 hướng.
- Vũ khí/trang bị được phép hiển thị hoặc thay thế.
- Bảng animation bắt buộc.
- Pivot, điểm spawn đạn, điểm đánh melee, điểm tương tác.
- Các trạng thái gameplay: idle, move, attack, dash, hurt, death, interact.
- Liên kết đến data/stat và prefab/scene hiện hữu nếu có.

### 3.2 Nhận diện và silhouette

- Nhân vật phải khác enemy thường ở hình khối, màu sắc hoặc vũ khí.
- Khi tô toàn bộ sprite thành một màu đen, người xem vẫn nên phân biệt được dáng chính và hướng nhìn.
- Vũ khí không được nhập nhằng với thân người ở frame tấn công.
- Đầu, thân, chân và tay phải đủ rõ để thể hiện hướng di chuyển trong camera thực tế.
- Không dùng chi tiết nhỏ làm đặc điểm duy nhất; đặc điểm nhận diện cần đọc được ở kích thước native trong game.
- Nếu sprite dùng bóng đổ để chỉ vị trí đứng, bóng cần gắn với chân/mặt đất, không gắn tùy tiện vào phần đầu.

### 3.3 Pivot, hitbox và điểm tương tác

- **Visual sprite** là vùng vẽ; **collider/hitbox** là vùng logic. Hai vùng có thể khác nhau.
- Đặt pivot gần điểm bàn chân hoặc điểm chạm đất để hỗ trợ di chuyển và Y-sorting.
- Collider nhân vật thường bám theo thân dưới/chân, không bao toàn bộ tóc, vũ khí hoặc hiệu ứng.
- Đòn melee cần vùng đánh riêng; không dùng collider di chuyển của nhân vật như hitbox vĩnh viễn.
- Projectile spawn point phải đúng hướng bắn và không xuất hiện từ giữa bản đồ hoặc xuyên qua tường không chủ ý.
- Điểm tương tác với cửa, loot, bàn hoặc NPC phải có phạm vi kiểm tra rõ ràng.
- Kiểm tra các trường hợp nhân vật đi sát tường, qua góc, bị nhiều enemy vây, dash qua cửa và đứng sau prop.

### 3.4 Animation set tối thiểu

Không phải nhân vật nào cũng cần mọi animation, nhưng mỗi state được gameplay gọi đến phải có animation hoặc fallback rõ ràng.

| Animation/State | Mục tiêu | Tiêu chí |
|---|---|---|
| Idle | Cho biết nhân vật đang đứng | Loop mượt, không rung sprite/pivot |
| Walk/Run | Thể hiện 4/8 hướng di chuyển | Chân và hướng thân đúng; tốc độ frame hợp với move speed |
| Light attack | Đòn đánh thường nhanh/combo | Đọc được hướng đánh, có startup/contact/recovery |
| Heavy/Charge | Đòn nặng hoặc tích lực | Có anticipation rõ; frame trúng đòn đủ dễ đọc |
| Bow quick shot | Bắn nhanh | Hướng cung và projectile cùng hướng; tránh projectile xuất hiện trước release |
| Bow charged shot | Bắn tích lực | Có trạng thái charge/release; có phản hồi thị giác và âm thanh |
| Weapon skill | Kỹ năng riêng | Phân biệt với attack thường, đúng thời lượng/cooldown |
| Dash | Né nhanh | Pose và VFX rõ; không che thông tin i-frame và hướng đi |
| Hurt | Nhận sát thương | Ngắn, rõ; không quá dài làm mất điều khiển nếu design không yêu cầu |
| Death/knockout | Chết/kết thúc run | Có điểm kết thúc rõ, không tiếp tục nhận input ngoài chủ đích |
| Interact/Pickup | Nhặt đồ hoặc tương tác | Không làm nhân vật đứng ở tư thế khó đọc quá lâu |

**Hướng sprite:**

- Tối thiểu 4 hướng nếu game cần đi lên/xuống/trái/phải.
- Dùng 8 hướng khi animation và combat thực sự cần phân biệt góc chéo; nếu chỉ có 4 bộ hình, có thể dùng flip ngang cho trái/phải khi hình không bất đối xứng.
- Nếu nhân vật dùng chuột để aim 360 độ, phải chốt cách thể hiện hướng ngắm: aim độc lập bằng vũ khí/upper body, 8 hướng, hay góc tự do. Không tự ghép 8 hướng di chuyển với aim 360 mà không kiểm tra khả năng animation.

### 3.5 Quy tắc combat riêng theo GDD Dungeon Eater

GDD hiện mô tả:

- Di chuyển WASD 8 hướng.
- Light attack bằng LMB/J; heavy/charged attack bằng RMB/K.
- Dash bằng Space; weapon skill bằng Q/E; F dùng cho tương tác/nhặt đồ; Esc pause.
- Kiếm: combo 3 hit, heavy cleave hình cung, skill dạng whirlwind/spin slash.
- Cung: quick shot, charged shot, piercing arrow.
- Dash là phòng thủ chính và có i-frame ngắn.
- Đòn nguy hiểm của enemy phải có telegraph khoảng **0,35–0,6 giây** tối thiểu theo GDD.
- Heavy hit có thể có hit-stop khoảng **40–80 ms** theo GDD; cần kiểm tra cảm giác thật trong build, không áp dụng máy móc cho mọi đòn.

Agent không được vô tình thay đổi mapping điều khiển hoặc bỏ telegraph khi tạo animation/logic.

### 3.6 Stat và progression

Mọi stat cần định nghĩa đơn vị, giới hạn, nguồn thay đổi và chỗ hiển thị UI:

- HP / Max HP.
- ATK hoặc damage base.
- DEF hoặc damage mitigation.
- Movement speed.
- Attack interval / attack speed nếu có.
- Dash distance, cooldown, i-frame duration.
- Skill cooldown/resource cost.
- Crit, status effects hoặc resistance chỉ thêm khi đã có design.

Không tự nghĩ giá trị cân bằng cuối cùng từ cảm tính. Dùng data table/config để dễ cân bằng, giữ giá trị prototype được đánh dấu là tạm.

---

## 4. Thiết kế enemy và boss

### 4.1 Mỗi enemy phải khác nhau về hành vi

Không tạo nhiều enemy chỉ khác màu nhưng cùng nhịp đánh, trừ khi đó là biến thể có mục đích rõ ràng.

Các vai trò nền tảng:

- **Melee chaser:** áp sát và tấn công cận chiến.
- **Ranged attacker:** giữ khoảng cách, bắn đạn hoặc tạo vùng nguy hiểm.
- **Exploder/suicide:** tiếp cận hoặc báo hiệu rồi phát nổ; cần cửa sổ né rõ.
- **Elite:** biến thể có áp lực cao hơn, pattern dễ nhận diện và phần thưởng tương xứng.
- **Boss:** nhiều phase/pattern, telegraph đặc trưng và khoảng trống để phản công.

GDD Dungeon Eater nêu ba nhóm creep cơ bản: cận chiến, tầm xa và nổ tự sát; boss xuất hiện cuối dungeon với HP cao và kỹ năng đặc biệt. Agent nên dùng chúng làm bộ enemy đầu tiên trước khi mở rộng roster.

### 4.2 Enemy specification bắt buộc

Mỗi enemy cần có:

1. ID/tên, vai trò và cấp độ nguy hiểm.
2. Silhouette và điểm nhận diện.
3. HP, damage, move speed, aggro range, attack range.
4. Hành vi di chuyển: chase, strafe, retreat, patrol, orbit hoặc đứng vị trí.
5. Attack pattern: wind-up, telegraph, active frame/duration, recovery, cooldown.
6. Hình thức báo đòn: animation, vùng sàn, projectile, âm thanh hoặc icon; phải đọc được khi combat đông.
7. Hitbox/hurtbox và quy tắc knockback/stagger.
8. Animation: idle/move/attack/telegraph/hurt/death (tùy vai trò).
9. Loot table, loại nguyên liệu và tỉ lệ rơi trong data; không hardcode trong animation.
10. SFX/VFX gắn với state cụ thể.
11. Cách enemy tương tác với tường, cửa, vật cản và nhóm enemy.
12. Cách test và các trường hợp lỗi có thể gặp.

### 4.3 Nguyên tắc combat readability

- Telegraph phải tương ứng chính xác với vùng sát thương và thời điểm đòn đánh có hiệu lực.
- Màu sắc chỉ là một kênh cảnh báo; kết hợp hình dạng vùng đánh, animation, âm thanh hoặc chuyển động.
- Tránh đặt vùng sát thương cùng màu/tương phản với mặt sàn.
- Cho người chơi cơ hội phản ứng hợp lý; nếu không thể né, phải thể hiện rõ đó là luật của đòn đánh chứ không phải lỗi collision.
- Projectile có tốc độ/đường bay rõ; tránh sprite nhỏ cùng màu nền hoặc mất dấu trên VFX.
- Hạn chế nhiều telegraph chồng kín toàn bộ phòng, trừ khi đó là một pha có chủ đích và có lối thoát.
- Knockback và hit-stop phải giúp cảm nhận va chạm chứ không làm game mất kiểm soát.
- Khi test, xác minh hình ảnh telegraph, thời gian logic và hitbox khớp nhau; không kiểm tra visual riêng lẻ.

### 4.4 Boss design

Mỗi boss nên có:

- Một fantasy rõ (ví dụ độc, đá, nấm, lửa, thịt/đồ ăn fantasy), hình dáng dễ nhớ.
- 2–4 pattern cốt lõi ở bản đầu, thay vì quá nhiều chiêu ít khác biệt.
- Mỗi pattern có tell → attack → recovery.
- Một cơ hội để người chơi gây damage sau khi né/đọc đúng pattern.
- Quy tắc phase change và phản hồi thị giác/âm thanh.
- Arena đủ không gian cho dash, projectile và di chuyển vòng quanh.
- Loot/reward gắn với độ khó; boss material phục vụ công thức/nâng cấp có mục đích.
- Test ở kích thước camera gameplay thật: boss lớn không được che toàn bộ đạn, vùng đánh hoặc player.

---

## 5. Thiết kế vũ khí

### 5.1 Vũ khí phải tạo lựa chọn chiến thuật

Khác biệt giữa hai vũ khí không chỉ nằm ở damage. Ít nhất phải khác nhau ở một vài yếu tố:

- Tầm đánh.
- Tốc độ ra đòn và recovery.
- Mức độ rủi ro khi tấn công.
- Khả năng đánh nhiều mục tiêu.
- Khả năng xử lý địch nhanh/địch đông.
- Độ chính xác/đòi hỏi ngắm.
- Khả năng xuyên, đẩy lùi hoặc khống chế.
- Chi phí/cooldown và cơ hội dùng skill.

### 5.2 Bảng định nghĩa vũ khí

| Thuộc tính | Mô tả |
|---|---|
| `id` | ID ổn định, duy nhất |
| `displayName` | Tên hiển thị |
| `weaponType` | sword, bow, staff... |
| `attackMode` | melee, projectile, charge, area... |
| `baseDamage` | Damage cơ sở |
| `startup` | Thời gian từ lúc input đến lúc đòn có thể trúng |
| `activeDuration` | Thời gian hitbox/projectile hoạt động |
| `recovery` | Thời gian hồi sau đòn |
| `range` | Tầm đánh, dùng cùng đơn vị world nhất quán |
| `area` | Kiểu/vùng ảnh hưởng |
| `cooldown` | Cooldown nếu có |
| `stamina/resourceCost` | Chỉ dùng nếu game có hệ tài nguyên đó |
| `knockback` | Knockback nếu có |
| `upgradePath` | Quy tắc nâng cấp hoặc tham chiếu data nâng cấp |
| `vfx/sfx` | Asset hiệu ứng gắn đúng thời điểm |
| `animationRefs` | Các animation liên quan |

Tên trường có thể đổi theo coding convention hiện hữu; không bắt buộc áp dụng nguyên tên này vào một codebase đã có schema khác.

### 5.3 Kiểm tra chất lượng vũ khí

- Hướng vung/bắn khớp với hướng input và hình ảnh.
- Hitbox không gây damage ở trước/sau frame tấn công dự kiến.
- VFX không làm che telegraph hoặc nhân vật.
- Không xuyên tường trừ khi đây là luật được ghi rõ.
- Nhiều mục tiêu trúng một đòn được xử lý nhất quán.
- Nâng cấp vũ khí thay đổi đúng stat/hiệu ứng đã mô tả.
- Có phản hồi rõ khi đòn trúng, hụt hoặc bị chặn.

---

## 6. Thiết kế item, loot, inventory và economy

### 6.1 Phân loại item

Các nhóm tiêu chuẩn có thể dùng:

- **Ingredient / nguyên liệu:** vật liệu lấy từ enemy, dùng trong recipe.
- **Elite Material:** nguyên liệu hiếm từ elite.
- **Boss Material:** vật liệu boss dùng cho recipe/nâng cấp đặc biệt.
- **Consumable:** vật phẩm dùng một lần, nếu game hỗ trợ.
- **Equipment:** trang bị có chỉ số.
- **Currency:** Gold hoặc loại tiền tệ được thiết kế.
- **Unlock/Recipe:** công thức hoặc vật phẩm mở khóa.
- **Quest/Lore item:** vật phẩm nhiệm vụ hoặc thông tin cốt truyện.
- **Restaurant upgrade material:** tài nguyên nâng cấp bếp/bàn ghế/balo nếu được dùng trong hệ thống.

Chỉ tạo những category thực sự cần thiết; không thêm loại item để lấp đầy danh sách.

### 6.2 Item design brief

Mỗi item cần xác định:

- ID ổn định, tên hiển thị và mô tả ngắn.
- Category, rarity/tier nếu có.
- Icon trong inventory và sprite rơi trên mặt đất (có thể khác nhau).
- Silhouette và màu accent để dễ phân biệt ở kích thước nhỏ.
- Nguồn rơi/nguồn kiếm.
- Số lượng stack tối đa hoặc quy tắc không stack.
- Số slot/khối lượng chiếm trong balo, nếu có.
- Cách dùng: recipe, sell, upgrade, quest hoặc equipment.
- Giá bán/giá trị kinh tế nếu được phép bán.
- Có mất khi chết hay không.
- Âm thanh/VFX khi nhặt/dùng nếu cần.
- Liên kết đến recipe, enemy loot table hoặc hệ upgrade.

### 6.3 Icon và độ hiếm

- Icon phải nhận diện được ở kích thước UI nhỏ mà không cần đọc tooltip.
- Không dùng chi tiết mảnh làm đặc điểm chính; kiểm tra ở tỷ lệ hiển thị thật.
- Nếu có rarity, sử dụng đồng bộ màu/viền/shape trên inventory, drop và tooltip.
- Không chỉ dựa vào màu; kết hợp viền, ký hiệu, hình dạng hoặc text label để tăng khả năng tiếp cận.
- Icon của nhóm nguyên liệu có họa tiết chung để thể hiện cùng hệ, nhưng mỗi item cần một khác biệt hình dạng/đặc điểm rõ.
- Đừng nhầm icon item với tile trang trí cùng palette.

### 6.4 Loot và inventory

GDD Dungeon Eater xác định balo có giới hạn slot; người chơi phải ưu tiên hoặc bỏ loot khi đầy. Hệ thống cần thiết kế rõ:

- Khi inventory đầy, có báo rõ và cho lựa chọn phù hợp; không âm thầm xóa loot.
- Nếu game cho phép bỏ item, hiện tên/icon/số lượng item sẽ bỏ và xác nhận khi hành động khó đảo ngược.
- Phân biệt item đã nhặt và item chỉ đang nằm trên sàn.
- Hiển thị số lượng, stack, slot đang dùng/còn trống.
- Nhặt đồ phải có feedback hình ảnh/âm thanh và cập nhật UI không trễ.
- Tạo drop table trong data/config, không gắn drop logic trực tiếp vào sprite hoặc animation.
- Các drop quan trọng cần đủ độ tương phản với nền và không nằm dưới VFX quá lâu.

**Quy tắc tử vong theo tài liệu hiện tại:** GDD nói người chơi có thể mất một tỷ lệ vật phẩm khi chết, nhưng Gold và Reputation không mất khi chết. Agent phải giữ Gold/Reputation không bị mất và không tự đoán tỷ lệ/nhóm item rơi lại. Nếu hệ thống death-loss chưa có con số cuối cùng, dùng config tạm và ghi rõ chờ duyệt.

### 6.5 Economy và liên kết giữa dungeon với nhà hàng

Mỗi item/reward cần tạo ra một quyết định có ý nghĩa:

- Bán nguyên liệu hay giữ để nấu món tốt hơn?
- Mang item hiếm nào khi balo gần đầy?
- Nâng cấp combat hay nâng cấp nhà hàng/balo?
- Món ăn nào tối ưu doanh thu/reputation trong giới hạn nguyên liệu?

Với recipe, cần thể hiện: đầu vào, số lượng tiêu hao, số món tạo ra, thời gian nếu có, doanh thu dự kiến, lợi ích/reputation và điều kiện unlock. Không được có item rơi ra nhưng không dùng ở bất kỳ hệ thống nào nếu không cố ý là lore/collectible.

### 6.6 Recipe/food item schema đề xuất

```yaml
id: recipe_monster_stew
name: Monster Stew
unlocked: false
inputs:
  - itemId: common_monster_meat
    quantity: 2
  - itemId: dungeon_herb
    quantity: 1
output:
  itemId: monster_stew
  quantity: 1
economy:
  sellPrice: 0 # giá demo; cần cân bằng theo game
  reputationValue: 0 # giá trị demo; cần cân bằng theo game
tags: [stew, monster, beginner]
```

Đây là minh họa schema, không phải số liệu cân bằng đã duyệt. Khi codebase đã có format JSON/ScriptableObject/resource khác, giữ format đó.

---

## 7. Thiết kế map, tileset và level

### 7.1 Phân biệt các thành phần

- **Tile:** một đơn vị đồ họa theo grid, thường dùng cho sàn/tường.
- **Tileset:** tập tile đồng nhất về phong cách và quy luật ghép nối.
- **Tilemap:** bố cục các tile tạo thành bề mặt/mặt bằng.
- **Prop:** vật thể đặt vào map, có thể trang trí, chặn đường hoặc tương tác.
- **Room/Encounter:** không gian có chức năng gameplay cụ thể.
- **Level/Run layout:** tập phòng/hành lang/điểm sự kiện kết thành một run.
- **Collision/navigation data:** dữ liệu logic cho vật cản, vùng đi được, spawn và pathfinding; không đồng nhất với hình nền.

### 7.2 Layer đề xuất

Tên cụ thể tùy engine, nhưng nên tách logic khỏi hình ảnh. Ví dụ:

1. `Ground_Base` — nền sàn chính.
2. `Ground_Detail` — vết nứt, cỏ, vệt bẩn; không tham gia collision.
3. `Ground_Decal` — vết máu, rune, cảnh báo mặt đất.
4. `Collision_Walls` — collider tường/vật cản.
5. `Props_Behind` — props hiển thị sau nhân vật.
6. `Actors` — player/enemy/NPC, sort theo vị trí chân.
7. `Props_Front` — tán cây, mái, cột hoặc phần che trước nhân vật.
8. `VFX_World` — projectile, hit sparks, telegraph, loot sparkle.
9. `Overlay/UI` — HUD, inventory, popup; tách khỏi world camera.

Không phải mọi map đều cần đủ các layer trên, nhưng agent phải xác định rõ tile nào chỉ là visual và tile/prop nào có collision.

### 7.3 Tile design checklist

Mỗi tileset cần có:

- Sàn cơ bản và biến thể để tránh pattern lặp quá đều.
- Viền/cạnh tường, góc trong/ngoài và đoạn nối đủ để ghép kín.
- Cửa, lối đi, ngưỡng chuyển phòng nếu level dùng chúng.
- Tile trang trí có/không collision được phân biệt rõ.
- Đất/nền ở các trạng thái phù hợp: bình thường, bẩn, nứt, ướt/độc nếu gameplay dùng.
- Quy tắc góc sáng, shadow, outline và scale giống nhau.
- Padding để tránh texture bleeding nếu dùng atlas/spritesheet.
- Kiểm tra seam ở mọi hướng ghép, không chỉ tile đứng riêng.
- Collision shape phù hợp; tile trang trí không được chặn player nếu không chủ đích.

### 7.4 Nguyên tắc bố cục map top-down

- Lối đi chính phải đọc được ngay; décor không làm người chơi nhầm đường đi.
- Tách không gian **đi được**, **không đi được**, **nguy hiểm** và **tương tác được** bằng silhouette, độ tương phản hoặc feedback nhất quán.
- Cửa/exit phải dễ tìm; vật thể tương tác nên có prompt/highlight khi ở tầm tương tác.
- Đặt landmark để người chơi định hướng: cột, giếng, tượng, bếp, cổng, dấu hiệu màu hoặc hình khối độc đáo.
- Dùng khoảng trống để combat; đừng lấp toàn bộ arena bằng props nhỏ.
- Thiết kế đường thoát khi player bị nhiều enemy vây hoặc tránh projectile.
- Tường, cột, thùng, bàn và cây cần có footprint/collider được kiểm tra trong gameplay.
- Không để props che player, enemy, loot hoặc telegraph ở các vị trí quan trọng.
- Bẫy phải có dấu hiệu nhận biết trước khi gây damage, trừ khi luật game cố ý tạo bẫy bất ngờ và đã được duyệt.
- Kiểm tra góc hẹp: player/enemy không được kẹt do collider chồng hoặc pathfinding không thể đi qua.
- Nếu map procedural, phải test seed/rule generation, tính liên thông và khả năng tiếp cận các room bắt buộc.

### 7.5 Room design và encounter pacing

Mỗi phòng cần một mục đích rõ ràng, ví dụ:

- Intro/tutorial — dạy một mechanic duy nhất, ít áp lực.
- Basic combat — kết hợp player kit với 1 loại enemy.
- Mixed encounter — yêu cầu ưu tiên mục tiêu, ví dụ ranged + melee.
- Loot/reward room — cho lựa chọn và tái tập trung.
- Hazard room — dạy đọc bẫy/địa hình.
- Elite room — khó hơn, reward đủ hấp dẫn.
- Boss arena — dành không gian cho pattern, telegraph và né.
- Transition/rest room — cho nghỉ/đổi vũ khí/đọc hint nếu game hỗ trợ.

Một run mẫu nên luân phiên mức áp lực. Không thiết kế mọi phòng thành giao tranh tối đa. Cần có lúc khám phá, lúc chiến đấu, lúc đưa ra quyết định loot và lúc nhận phần thưởng.

### 7.6 Room specification template

```yaml
roomId: dungeon_01_room_03
roomType: mixed_encounter
purpose: Teach the player to prioritize ranged enemies
entryConditions: []
exitConditions:
  - encounterCleared
layout:
  gridWidth: 0 # điền theo map thực tế
  gridHeight: 0 # điền theo map thực tế
playerSpawn: {x: 0, y: 0}
exits: []
encounter:
  waves: []
loot: []
hazards: []
landmarks: []
navChecks:
  - allRequiredAreasReachable
  - exitReachableAfterCombat
notes: "Giá trị 0 là placeholder; không dùng nguyên trạng trong game."
```

### 7.7 Che khuất và Y-sorting

Trong top-down 2D, thứ tự vẽ thường cần dựa vào vị trí mặt đất/chân, không phải tâm hình sprite. Khi nhân vật đi phía sau cột/cây, phần đó phải che đúng. Khi đi phía trước, nhân vật phải vẽ lên trên.

- Chọn một quy tắc sorting nhất quán và test bằng scene có player, enemy, cột cao, cây, mái, bàn và loot.
- Đặt pivot/sort point ở chân hoặc điểm chạm đất đối với actor.
- Tách phần nền và phần foreground của prop lớn nếu player cần đi sau/trước nó.
- Dùng Sorting Group hoặc cơ chế tương đương để các phần của cùng nhân vật không bị trộn sai thứ tự với sprite khác.
- Với Unity 2D, có thể dùng Sorting Layers, Order in Layer và Custom Axis Y-sorting; hãy kiểm tra theo tài liệu Unity phiên bản của dự án.
- Không giải quyết mọi lỗi occlusion bằng cách tăng Order in Layer thủ công cho từng object nếu vị trí thay đổi theo thời gian.

### 7.8 Collision và navigation

- Collision phải phục vụ gameplay, không cần trùng đường viền nghệ thuật từng pixel.
- Dùng collider đơn giản, ổn định; tránh hàng trăm collider rời không cần thiết.
- Collider map và collider actor cần nhất quán về đơn vị world.
- Tilemap collider có thể được kết hợp/merge khi engine hỗ trợ để giảm số shape; đánh giá bằng profiling nếu map lớn.
- Tách collider của tường, hazard, interaction trigger và decoration.
- Pathfinding của enemy phải nhận biết lối hẹp/cửa và tránh mục tiêu không thể đến.
- Đừng để visual nói rằng có đường đi nhưng collider chặn, hoặc visual là tường nhưng actor đi xuyên qua.

---

## 8. Prop, pickup, cửa, bẫy và vật thể tương tác

Mỗi prop cần được phân loại một cách rõ ràng:

- **Pure decoration:** chỉ trang trí, không collider/interaction.
- **Solid obstacle:** chặn di chuyển.
- **Occluding prop:** che nhân vật theo chiều sâu; có thể đồng thời chặn di chuyển.
- **Interactable:** cần nút tương tác hoặc điều kiện cụ thể.
- **Breakable:** có HP/trạng thái vỡ/drop nếu design có hỗ trợ.
- **Hazard:** gây damage hoặc áp dụng status.
- **Pickup:** vật thể có thể nhặt.
- **Transition:** cửa/cổng đưa sang khu vực khác.

### 8.1 Interaction contract

Mỗi vật thể tương tác phải định nghĩa:

- Phạm vi tương tác.
- Điều kiện tương tác.
- Input (đối chiếu mapping chung).
- Feedback khi có thể/không thể tương tác.
- Hành động xảy ra và trạng thái sau đó.
- Có bị khóa bởi combat, inventory, quest hay state khác hay không.
- Accessibility/readability: icon/prompt không bị che, kích thước chữ đủ đọc.

Không cho quá nhiều object cùng hiện prompt chồng lên nhau; cần ưu tiên object gần nhất hoặc một luật ưu tiên rõ ràng.

### 8.2 Vật thể nguy hiểm

- Vùng nguy hiểm phải nhận diện được trước khi active.
- Telegraph và sát thương phải khớp tọa độ, thời gian, scale.
- Cung cấp feedback khi player bị trúng: hit flash/tint, âm thanh, knockback/hit-stop theo design.
- Hiệu ứng đẹp không được che vùng nguy hiểm hoặc làm khó xác định vì sao player mất HP.

---

## 9. UI/UX và thông tin trong game

### 9.1 HUD pha dungeon

Dựa theo GDD, HUD có thể cần HP, minimap (nếu game dùng), phím tắt và chỉ số. Chỉ hiển thị những thứ người chơi cần ra quyết định ngay lúc đó.

- HP phải đủ nổi bật và cập nhật rõ khi bị đánh.
- Kỹ năng/cooldown phải biểu đạt sẵn sàng, đang hồi hoặc chưa đủ điều kiện.
- Thể hiện số slot balo đã dùng/còn trống.
- Loot quan trọng có thể dùng icon nổi nhưng không spam hiệu ứng.
- Telegraph/boss health không bị che bởi HUD.
- Text đủ tương phản với nền ở vùng sáng và tối.
- Trạng thái nguy hiểm không chỉ dựa vào màu sắc.

### 9.2 UI pha nhà hàng

GDD nêu ca phục vụ 10 phút, tối đa 20 khách, order icon và patience timer. UI cần bảo đảm:

- Nhìn được khách nào đang cần món nào.
- Order, món ăn, số lượng và bàn/khách đích không nhập nhằng.
- Patience timer đọc nhanh; màu sắc kết hợp hình thức/thanh/tốc độ animation.
- Màn hình chuẩn bị menu cho thấy nguyên liệu bị tiêu hao và doanh thu dự kiến.
- Phân biệt recipe đã mở, chưa mở, đủ nguyên liệu, thiếu nguyên liệu.
- Nâng cấp có before/after: ví dụ capacity, số bàn, hiệu quả phục vụ hoặc chỉ số đã được hệ thống duyệt.
- Trong lúc ca phục vụ diễn ra, tránh popup lớn chặn các order đang hoạt động.

### 9.3 Menus và trạng thái

Các màn hình/trạng thái được GDD đề cập: main menu, pause, inventory, game over; các route khác cần bám sản phẩm thực tế.

Mỗi UI phải có:

- Trạng thái mặc định, loading nếu có, empty, error và disabled.
- Điều kiện hiển thị/nút được bật.
- Hành động khi bấm và feedback khi xong.
- Luồng quay lại/đóng rõ ràng.
- Hành vi phù hợp cả chuột và bàn phím/tay cầm nếu được hỗ trợ.
- Không mất dữ liệu do người dùng đóng nhầm UI, trừ khi luật game đã cảnh báo.

### 9.4 Feedback khi thao tác

- Nhặt item: icon/text nhỏ, sound, inventory update.
- Tấn công: hit spark, âm thanh, reaction/knockback hoặc hit-stop phù hợp.
- Không gây damage: khác biệt rõ với đánh trúng nếu cần thiết.
- Dùng skill/dash: feedback sẵn sàng, cooldown hoặc hết điều kiện.
- Mua upgrade: hiện chi phí, kết quả và chỉ số thay đổi.
- Recipe: hiển thị nguyên liệu tiêu hao và sản lượng trước khi xác nhận.
- Lỗi: giải thích nguyên nhân và bước tiếp theo, không chỉ hiện “Failed”.

---

## 10. VFX và Audio

### 10.1 VFX taxonomy

Nên tổ chức theo chức năng để không nhầm hiệu ứng:

- `VFX_Attack` — vệt chém, muzzle flash, projectile trail.
- `VFX_Hit` — impact/hit spark, hit flash.
- `VFX_Status` — poison, burn, slow, stun nếu game hỗ trợ.
- `VFX_Telegraph` — warning circle/cone/line.
- `VFX_Dash` — afterimage/speed line.
- `VFX_Loot` — drop sparkle/rare drop beacon.
- `VFX_Environment` — bụi, khói, ambient particles.
- `VFX_Restaurant` — hơi nóng, chopping, cook/serve success.
- `VFX_Boss` — intro, phase transition, signature skill.

### 10.2 Quy tắc VFX

- VFX phải khớp gameplay event, không chỉ chạy theo thời gian tùy tiện.
- Telegraph luôn có độ tương phản tốt hơn particles trang trí.
- Giới hạn độ sáng/độ lớn để không che player, projectile, loot và đường đi.
- Effect kết thúc đúng lúc; dọn object/particle để tránh tích tụ.
- Pixel art VFX phải cùng độ phân giải/độ sắc nét với art còn lại hoặc có chủ đích rõ ràng.
- Kiểm tra nhiều VFX cùng lúc và trên nền sáng/tối.
- Hit-stop chỉ áp dụng cho những hit được design; không làm đứng cứng cả trận vô lý.

### 10.3 Audio

GDD định hướng nhạc nền nhẹ để tập trung; combat có hit/receive-damage nặng và có lực; nấu ăn, phục vụ, nâng cấp vui tươi, tạo cảm giác thành tựu.

Mỗi event audio nên quy định:

- Event/state kích hoạt.
- Clip hoặc reference asset.
- Volume group/bus.
- Pitch/variation nếu có.
- Cooldown/voice limit để âm thanh không spam.
- Ưu tiên khi nhiều sound cùng phát.

Phải phân biệt ít nhất: player attack, enemy attack/telegraph quan trọng, hit, player damaged, death, pickup, rare loot, cooking completion, customer order/served và upgrade success khi các hệ thống này có mặt.

---

## 11. Unity-specific implementation notes (chỉ áp dụng khi dự án dùng Unity)

Đây là gợi ý kỹ thuật, không thay thế convention hiện có của repository.

### 11.1 Pixel art rendering

- Dùng camera/pipeline phù hợp với 2D; nếu yêu cầu pixel art sắc nét và ổn định khi đổi độ phân giải, đánh giá package **2D Pixel Perfect Camera** phù hợp với Unity version và Render Pipeline dự án.
- Kiểm tra reference resolution, pixel-per-unit, sprite import, filter mode, compression và scaling.
- Test camera khi di chuyển: tránh sprite bị nhòe, rung hoặc scale không nhất quán.
- Không tự thêm package nếu có xung đột với Render Pipeline; xác minh compatibility của đúng phiên bản Unity trước.

### 11.2 Sprite sorting

- Xác định Sorting Layers (ví dụ Ground, PropsBack, Actors, PropsFront, VFX, UI) theo nhu cầu dự án.
- Với top-down 2D, đánh giá Y-sort/Custom Axis sort để object ở phía dưới màn hình có thể vẽ phía trước object ở phía trên.
- Đặt sprite sort point/pivot đúng vị trí logic.
- Dùng Sorting Group cho nhân vật/prop có nhiều renderer để tránh phần con của hai actor bị xen kẽ sai.
- Tạo scene kiểm thử sorting chuyên dụng gồm cây/cột cao, player, enemy, loot và projectile.

### 11.3 Tilemap và collision

- Tách Tilemap visual và collision nếu điều đó giúp quản lý rõ hơn.
- Chỉ bật collision cho loại tile cần chặn di chuyển.
- Dùng Tilemap Collider 2D và cân nhắc Composite Collider 2D để gộp shape khi phù hợp; profile trên map thực tế.
- Collider không cần bám từng pixel của sprite; hình collider đơn giản và ổn định thường phù hợp hơn.
- Đặt physics layer matrix rõ ràng để projectile/player/enemy/loot/trigger tương tác đúng.
- Không trộn Collider 2D với Collider 3D.

### 11.4 Input, animation và gameplay state

- Giữ input mapping ở một nơi có thể tra cứu; không hardcode cùng một phím ở nhiều script không liên quan.
- Tách trạng thái nhân vật khỏi logic vẽ sprite.
- Đòn đánh nên có state/window rõ: startup, active, recovery; hit detection chỉ hoạt động trong cửa sổ đúng.
- Đảm bảo dash/i-frame không phụ thuộc nhầm vào một animation frame duy nhất nếu FPS/anim speed có thể đổi; ưu tiên logic state/timer xác định.
- State bị interrupt (hurt, death, stun, pause) phải có luật rõ về attack/dash.
- Dùng ScriptableObject hoặc data format hiện hữu để chứa item, enemy, weapon, recipe stats khi phù hợp; không tạo framework data mới nếu dự án đã có hệ thống tương đương.

---

## 12. Asset specifications và naming convention

### 12.1 Tài liệu đi kèm mỗi asset

Mỗi asset hoặc batch asset phải đi kèm metadata đủ để tích hợp:

- Tên file/ID.
- Kích thước canvas và frame.
- Số hàng/cột, frame count, frame order.
- Hướng (N/S/E/W hoặc N/NE/E/SE/S/SW/W/NW) nếu có directional animation.
- FPS/loop/ping-pong/one-shot.
- Pivot/anchor.
- Transparent background hay opaque.
- Collider/interaction notes.
- Nguồn palette/style reference.
- License/nguồn nếu asset bên ngoài.
- Trạng thái: concept, approved, production, integrated.

### 12.2 Naming convention đề xuất

Điều chỉnh theo convention codebase; nếu chưa có, có thể dùng:

- `CHR_Player_Idle_S.png`
- `CHR_Player_Walk_S.png`
- `ENM_CreepMelee_Attack_E.png`
- `ENM_CreepRanged_Telegraph_S.png`
- `BOSS_Dungeon01_Death.png`
- `WPN_Sword_LightAttack_S.png`
- `ITM_MonsterMeat_Icon.png`
- `ITM_EliteCore_World.png`
- `TILE_Dungeon_Floor_A.png`
- `TILE_Dungeon_Wall_InnerCorner.png`
- `PROP_Dungeon_Pillar.png`
- `VFX_Hit_Heavy_01.png`
- `UI_Icon_Inventory.png`
- `SFX_Player_SwordHit_01.wav`

Quy tắc:

- Dùng ID ổn định, tránh đặt ID theo tên file dịch/ngôn ngữ hiển thị.
- Không dùng khoảng trắng, ký tự đặc biệt hoặc tên mơ hồ như `final_final2_new`.
- Không đổi tên asset đã được tham chiếu mà chưa cập nhật mọi reference.
- Asset variants dùng suffix có quy luật: `_A`, `_B`, `_01`, `_02`, hoặc tên material/biome rõ ràng.

### 12.3 Deliverable cho sprite sheet

Khi giao sprite sheet, agent phải ghi:

```text
Asset: CHR_Player_Walk
Canvas: <width>x<height>
Frame size: <frameWidth>x<frameHeight>
Grid: <columns>x<rows>
Directions: <explicit order>
FPS: <number>
Loop: true/false
Pivot: <normalized or pixel coordinates>
Background: transparent
Import/filter: <settings>
Notes: <known limitations>
```

Không bắt buộc phải dùng sprite sheet; nhiều engine/pipeline dùng frame file rời. Quy định này là yêu cầu mô tả asset để agent và người tích hợp không phải đoán.

---

## 13. Quy trình agent thiết kế/tạo/tích hợp asset

### Phase A — Analyze

1. Đọc task và GDD.
2. Kiểm tra engine/version/repo và asset cùng loại hiện có.
3. Xác định gameplay purpose và người chơi cần nhận diện điều gì.
4. Ghi yêu cầu bắt buộc, giả định và câu hỏi còn mở.
5. Xác định kích thước, camera, pivot, naming, animation và integration target.

### Phase B — Design spec

1. Viết brief ngắn.
2. Chốt silhouette/shape language, palette và reference.
3. Liệt kê trạng thái/animation hoặc layer/tile variants cần thiết.
4. Định nghĩa hitbox/collider/interaction và data fields.
5. Nêu các trường hợp gameplay cần test.
6. Nếu task lớn, tạo một asset mẫu để xác nhận style trước khi sản xuất hàng loạt.

### Phase C — Produce

1. Tạo asset đúng grid, frame size, màu, pivot và naming.
2. Kiểm tra transparent edges/padding, frame alignment và ghost pixels.
3. Không nhúng text UI vào sprite nếu text cần dịch hoặc thay đổi runtime.
4. Không bake gameplay state vào hình nếu nó cần thay đổi động (HP bar, cooldown, item count), trừ khi đó là art concept.
5. Không tạo asset không được task yêu cầu chỉ vì thấy “đẹp hơn”.

### Phase D — Integrate

1. Import theo pipeline hiện hữu.
2. Cập nhật prefab/scene/data references khi đã xác định đúng nơi.
3. Gắn animation/VFX/SFX vào state/event đúng.
4. Thiết lập collision/sorting/layer.
5. Đảm bảo object có lifecycle: spawn, active, disabled/despawn/destroy đúng.
6. Chạy build/test trong phạm vi khả dụng.

### Phase E — Validate

1. Xem asset ở kích thước thực trong camera game, không chỉ zoom lớn trong editor.
2. Test gameplay nền sáng/tối và tình huống nhiều object.
3. Xác minh input/state timing/hitbox nếu asset liên quan combat.
4. Kiểm tra UI khi có nhiều item/khách/địch.
5. Kiểm tra console/log, missing reference và cảnh báo import.
6. Báo cáo kết quả và phần chưa test.

---

## 14. Checklist nghiệm thu theo loại asset

### 14.1 Player character

- [ ] Đúng phong cách, camera, palette và kích thước dự án.
- [ ] Silhouette nhận diện được ở native resolution.
- [ ] Đủ hướng di chuyển cần thiết; flip ngang không làm sai chi tiết bất đối xứng.
- [ ] Có đầy đủ animation state cần gameplay gọi đến.
- [ ] Pivot và collider/hurtbox/hitbox được định nghĩa riêng.
- [ ] Attack/dash/hurt/death không tạo trạng thái kẹt hoặc animation không kết thúc.
- [ ] Y-sorting và occlusion hoạt động khi đi quanh prop.
- [ ] Không có frame lệch grid, nhòe hoặc flicker ngoài ý muốn.

### 14.2 Enemy / boss

- [ ] Khác biệt rõ về vai trò và hành vi.
- [ ] Telegraph phù hợp với timing/vùng sát thương.
- [ ] Có startup/active/recovery/cooldown được xác định.
- [ ] Collision, knockback, hurt/death state hoạt động.
- [ ] Loot table/data có thể cân bằng mà không sửa art.
- [ ] VFX không che player hoặc warning.
- [ ] Enemy không kẹt tường/không thể tiếp cận mục tiêu.
- [ ] Boss arena/pattern có lối né khả thi.

### 14.3 Weapon / projectile

- [ ] Hướng, range, shape và timing khớp thiết kế.
- [ ] Không gây damage ngoài cửa sổ hợp lệ.
- [ ] Không xuyên collider không chủ ý.
- [ ] Có feedback hit/miss/impact phù hợp.
- [ ] Projectile được dọn đúng thời điểm và có giới hạn lifetime.
- [ ] Upgrade liên kết đúng data và UI.

### 14.4 Item / icon / loot

- [ ] Nhận diện được khi hiển thị nhỏ.
- [ ] Metadata có ID/tên/category/nguồn/usage/stack rule rõ.
- [ ] World pickup và inventory icon đúng liên kết.
- [ ] Có feedback nhặt và trạng thái inventory đầy.
- [ ] Quy tắc mất khi chết rõ; Gold/Reputation giữ nguyên theo GDD hiện tại.
- [ ] Item có mục đích gameplay, recipe/economy/use case được xác định.

### 14.5 Tileset / map / room

- [ ] Tiles ghép không seam, lệch hoặc lộ nền ngoài ý muốn.
- [ ] Sàn, tường, decoration và hazard được phân biệt.
- [ ] Collision đúng; lối đi chính không bị chặn ngoài ý muốn.
- [ ] Y-sorting/foreground hoạt động ở nhiều vị trí.
- [ ] Có spawn, exits, encounter, loot và landmark khi cần.
- [ ] Có đủ khoảng trống cho combat/né.
- [ ] Các route cần thiết reachable.
- [ ] Map không chứa props gây che khuất combat không công bằng.
- [ ] Camera và độ tương phản ổn ở kích thước game thực.

### 14.6 UI / VFX / Audio

- [ ] Thông tin quan trọng đọc được trong tình huống đông.
- [ ] State loading/empty/error/disabled có xử lý nếu cần.
- [ ] Telegraph và HP/interaction không bị che.
- [ ] VFX khớp event và tự kết thúc/dọn dẹp.
- [ ] Audio có mức ưu tiên/volume/voice limit phù hợp.
- [ ] Không phát âm thanh/hiệu ứng lặp liên tục ngoài chủ đích.

---

## 15. Các test gameplay tối thiểu

### Combat test room

Tạo hoặc dùng một scene/phòng test để kiểm tra:

- Player đi 8 hướng, không tăng tốc chéo ngoài ý muốn.
- Kiếm light combo 3 hit; heavy cleave; skill.
- Cung quick shot, charged shot và piercing arrow.
- Dash có đúng khoảng cách, cooldown và i-frame.
- Ba archetype enemy: melee, ranged, exploder.
- Telegraph ở thời gian theo GDD; kiểm tra khi có nhiều enemy.
- Damage, knockback, hit-stop, death và loot.
- Player/enemy đi trước/sau prop theo vị trí.
- Inventory đầy và nhặt/bỏ loot.

### Map test room

- Đi qua cửa, góc hẹp, hành lang và cạnh collider.
- Đi phía sau và phía trước cột/cây/bàn.
- Kiểm tra các tile cạnh/góc, collision và lối đi.
- Kiểm tra projectile với tường/cửa.
- Kiểm tra spawn point không chồng vào vật cản/enemy.

### Restaurant test scene

- Recipe thiếu/đủ nguyên liệu.
- Preview nguyên liệu tiêu hao và doanh thu trước khi nấu.
- Customer/order icon/patience timer.
- Tối đa tình huống 20 khách theo GDD nếu prototype hỗ trợ.
- Serve đúng/sai khách và feedback.
- Upgrade thay đổi đúng các thông số đã mô tả.

### Death/reward test

- Xác nhận Gold và Reputation không bị mất khi chết.
- Xác nhận luật mất đồ chỉ áp dụng đúng nhóm item/tỷ lệ đã cấu hình.
- Xác nhận reward/run result không cộng hai lần sau load/retry.
- Xác nhận scene chuyển tiếp không làm mất inventory ngoài chủ đích.

---

## 16. Ưu tiên phát triển cho Dungeon Eater

Để tránh tạo art rời rạc trước khi gameplay rõ, ưu tiên sản xuất theo thứ tự:

1. **Art Bible + scale test:** chốt camera/góc nhìn, grid, player size, palette và độ phân giải tham chiếu.
2. **Player combat kit:** player idle/move, kiếm/cung, attack/dash/hurt/death; xác minh điều khiển và hitbox.
3. **Dungeon kit tối thiểu:** floor/wall/corner/door, một nhóm props, collision/sorting.
4. **Ba enemy cơ bản:** melee, ranged, exploder; có telegraph/timing/loot.
5. **Pickup + inventory UI:** nguyên liệu, giới hạn balo, Gold/Reward feedback.
6. **Một room combat hoàn chỉnh:** spawn, encounter, exit, reward; chơi được từ đầu đến cuối.
7. **Elite/boss prototype:** chỉ sau khi combat room cơ bản công bằng và ổn định.
8. **Restaurant art/UI:** bàn, ghế, bếp, khách, recipe, order, patience timer, serving feedback.
9. **Upgrade/progression:** liên kết Gold/ingredients/materials với lựa chọn nâng cấp.
10. **Polish pass:** VFX/audio, variation tiles, ambient details, thêm enemy/room khi core loop đã kiểm thử.

Đây là thứ tự đề xuất để giảm việc phải làm lại; có thể thay đổi nếu kế hoạch sprint/repository đã được duyệt quy định khác.

---

## 17. Prompt ngắn để giao tác vụ cho agent

Sử dụng mẫu này, thay phần trong ngoặc vuông:

```text
Đọc TOP_DOWN_GAME_DESIGN_GUIDELINES.md và GDD/tài liệu dự án trước khi làm.

TASK: [mô tả một deliverable cụ thể]
GAMEPLAY PURPOSE: [asset này giúp người chơi hiểu/làm điều gì]
TARGET: [Unity scene/prefab/data/asset path hoặc output format]
STYLE: [dùng Art Bible hiện hữu; nếu chưa chốt, ghi giả định trước]
REQUIREMENTS: [kích thước/frame/state/direction/stat/collision/UI]
CONSTRAINTS: Không đổi gameplay/GDD ngoài phạm vi task; không tạo asset trùng; giữ naming/data convention của repo.

WORKFLOW:
1. Kiểm tra repository và asset tương tự.
2. Tóm tắt yêu cầu, file liên quan và các giả định quan trọng.
3. Thực hiện thay đổi nhỏ nhất đáp ứng task.
4. Kiểm tra visual ở native resolution và kiểm tra integration/collision/sorting/state nếu liên quan.
5. Chạy test/build khả dụng; báo rõ test nào chưa chạy.
6. Tổng kết file đã đổi, kết quả, giả định và blocker.

ACCEPTANCE CRITERIA:
- [điều kiện cụ thể 1]
- [điều kiện cụ thể 2]
- [điều kiện cụ thể 3]
```

---

## 18. Nguồn và phạm vi

### 18.1 Nguồn dự án

- **`Dungeon Eater.pdf`**, báo cáo bài tập lớn môn Thiết kế và Phát triển Game, Đại học Thủy Lợi, tháng 9/2026. Các yêu cầu về thể loại, pixel art, palette môi trường, combat, phím điều khiển, enemy, loot, economy, restaurant và game flow trong file này được lấy từ tài liệu đó.
- Phần tài liệu Dungeon Eater còn để trống/ở mức khái quát (ví dụ detailed level design, game options, một số constraints và game balance matrix). File này **không giả vờ rằng những phần đó đã được chốt**; các schema/giá trị mẫu trong hướng dẫn chỉ là ví dụ hoặc đề xuất.

### 18.2 Tài liệu kỹ thuật tham khảo

- [Unity — 2D Pixel Perfect](https://docs.unity.cn/Packages/com.unity.2d.pixel-perfect@5.1/manual/index.html): camera/pixel-perfect rendering và reference resolution.
- [Unity — 2D sorting](https://docs.unity.com/en-us/engine/6000.7/manual/unity2d/sprite/sort-sprites/2d-renderer-sorting): Sorting Layers, Order in Layer, distance/custom-axis sorting và thứ tự render.
- [Unity — Tilemap Collider 2D](https://docs.unity.com/en-us/engine/7000.0/manual/unity2d/tilemaps/tilemap-collider-2d/2d): collider cho tilemap và cách kết hợp collider shapes.
- [Unity — 2D in Unity: Tech Art Essentials](https://learn.unity.com/article/2d-in-unity-tech-art-essentials): nền tảng kỹ thuật art 2D, Y-sorting và Sorting Groups.

Các tài liệu kỹ thuật này chỉ áp dụng trực tiếp nếu repository dùng Unity. Agent phải dùng tài liệu tương ứng với phiên bản engine/package thực tế và kiểm tra compatibility trước khi chỉnh project settings.

---

## 19. Nguyên tắc cuối cùng

**Một asset đạt yêu cầu không chỉ là asset đẹp; nó phải đọc được trong camera gameplay, tương thích với art bible, hoạt động đúng với collision/sorting/state, tạo phản hồi có ý nghĩa và có thể được tích hợp/bảo trì trong repository.**

Khi phải đánh đổi giữa chi tiết trang trí và khả năng đọc gameplay, ưu tiên khả năng đọc gameplay. Khi phải đánh đổi giữa thêm nội dung và kiểm chứng chất lượng, ưu tiên một lát cắt nhỏ nhưng chơi/test được từ đầu đến cuối.
