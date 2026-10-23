**TRƯỜNG ĐẠI HỌC THỦY LỢI**

**KHOA CÔNG NGHỆ THÔNG TIN**

# **BÁO CÁO BÀI TẬP LỚN**

## **MÔN: THIẾT KẾ VÀ PHÁT TRIỂN GAME**

# **ĐỀ TÀI: DUNGEON EATER**

**Giáo viên hướng dẫn:** Trương Xuân Nam

**Lớp:** 65CNTT

Hà Nội, tháng 9 năm 2026

| STT | Mã sinh viên | Họ và tên       | Vai trò / Nhiệm vụ |
| :-: | :------------: | :----------------- | :-------------------- |
|  1  |   2351060441   | Nguyễn Văn Hải  | Trưởng nhóm        |
|  2  |   2351060445   | Nguyễn Minh Hiệp | Thành viên          |
|  3  |   2351060444   | Ngô Văn Hiến    | Thành viên          |
|  4  |   2351060443   | Trần Hữu Hậu    | Thành viên          |

# **PHẦN 1: HIGH CONCEPT & GIỚI THIỆU NHÓM**

## **1.1 High Concept**

* **Tên Game:** Dungeon Eater
* **High concept:** Game roguelike chặt chém góc nhìn topdown, nhập vai đầu bếp săn tìm nguyên liệu quý hiếm trong dungeon, chế biến các món ăn độc đáo từ chúng để phục vụ thực khách và tái đầu tư lợi nhuận nhằm nâng cấp quán ăn lẫn trang bị chiến đấu.

## **1.2 Nhóm làm game: Nhóm DungeonEater**

| STT | Họ và tên       | Vai trò / Nhiệm vụ                                                                                                                   |
| :-: | :----------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  1  | Nguyễn Văn Hải  | Thảo luận ý tưởng game  Đóng góp ý tưởng vào High Concept,  Viết một phần Doc: phần 2, 1 Thiết kế tổng quan về game |
|  2  | Nguyễn Minh Hiệp | Thảo luận ý tưởng game  Đóng góp ý tưởng vào High Concept,  Viết một phần Doc: phần 1                                   |
|  3  | Ngô Văn Hiến    | Thảo luận ý tưởng game  Đóng góp ý tưởng vào High Concept,                                                                  |
|  4  | Trần Hữu Hậu    | Thảo luận ý tưởng game,  Đóng góp ý tưởng vào High Concept,                                                                 |

# **PHẦN 2: TỔNG QUAN VỀ GAME (GAME OVERVIEW)**

## **2.1 Thông tin chung**

* **Thể loại (Genre):** Action Roguelite / Hack & Slash,  Restaurant Management.
* **Chủ đề (Theme):** Fantasy, Dungeon Explorer
* **Yếu tố**:
  * Game sẽ mang hơi hướng giả tưởng (fantasy), chỉ có hai loại chủng tộc: người và quái vật
  * Con người tồn tại ngoài dungeon, quái vật tồn tại trong dungeon
* **Phong cách đồ họa (Style):** Pixel Art
* **Trình tự chơi (Sequence):** 2 pha nối tiếp nhau
  * Roguelike: Chiến đấu trong dungeon
  * Quản lý: nhà hàng, nấu ăn phục vụ thực khách
  * Vòng lặp cốt lõi của game là: Săn quái → Thu nguyên liệu → Nấu món → Phục vụ → Nhận Gold/Reputation → Nâng cấp → Tiến sâu hơn.

## **2.2 Phân loại người chơi mục tiêu**

* Phân loại người chơi hướng tới: game thủ hardcore yêu thích lối chơi game roguelike, cày cuốc

## **2.3 Luồng chơi tổng thể (Game Flow)**

Bắt đầu game, người chơi được nhập vai vào nhân vật chính vô danh, tỉnh dậy trong dungeon với kiếm và cung sẵn trên người, nhân vật chính không biết minh là ai, vừa đói vừa khát. Đây sẽ là giai đoạn hướng dẫn về cơ chế combat, dungeon của game (pha chiến đấu \- dungeon), trong quá trình này, nhân vật chính sẽ nhận ra món ăn nấu từ nguyên liệu rơi ra từ quái rất ngon

Sau khi hoàn thành hướng dẫn combat đầu game, nhân vật chính sẽ đi ra ngoài dungeon, bước chân ra thế giới bên ngoài. Ngay gần dungeon là một ngôi làng hẻo lánh, nghèo nàn, không có nhiều liên hệ với thế giới bên ngoài. Lúc này ta sẽ đến gặp người trong làng, mang những món nguyên liệu ta đã nhặt được trong dungeon ra bán, không ai đủ tiền để mua số hàng đó của nhân vật chính. Sau đó, trưởng làng sẽ đến gặp nhân vật chính và ngỏ ý đổi một căn nhà hoang trong làng lấy số nguyên liệu nhân vật chính nhặt được từ dungeon

Người chơi sẽ đồng ý dẫn đến mở ra pha kinh doanh (nhà hàng). Qua đó, vòng lặp đánh dungeon \-\> kinh doanh nhà hàng ăn \-\> nâng cấp chính thức bắt đầu

Luồng game tổng quát
![][image3]

## **2.4 Phong cách Trải nghiệm (Look & Feel)**

* **Visuals:**
  * Tông màu chủ đạo: tươi sáng khi ở ngoài dungeon (màu xanh lá cây), u tối khi tiến vào dungeon (màu tím), cổ điển khi ở trong nhà hàng (màu nâu gỗ)
  * Hiệu ứng đồ họa: Hiệu ứng đòn đánh trong combat, cho người chơi cảm giác lực mỗi khi ra đòn
* **Audio:**
  * Nhạc nền nhẹ nhàng, nhỏ giúp người chơi tập trung vào game
  * Hiệu ứng âm thanh
    * Hiệu ứng ân thanh khi đánh trúng đích, khi nhận sát thương: nặng nề, tạo cảm giác lực
    * Hiệu ứng âm thanh khi nấu ăn, phục vụ khách, nâng cấp đồ: vui tươi, mang cảm giác thành tựu

## **2.5 Các khía cạnh tác động đến người chơi**

* **Chiến thuật / Chiến lược:** Tính toán khi giao chiến về lượng máu, số lượng định, số chiến lợi phẩm đã thu gom được, nếu người chơi chết, những có tỷ lệ đồ bị mất
* **Cảm xúc / Tinh thần:**
  * Cảm giác kịch tính khi sinh tồn và thỏa mãn khi chinh phục dungeon.
  * Cảm giác thư thái khi nấu ăn phục vụ khách trong nhà hàng

## **2.6 Mục tiêu trải nghiệm & Lồng ghép màn chơi**

### **2.6.1. Mục tiêu trải nghiệm:**

Tạo trải nghiệm “hunter-chef”: người chơi vừa có cảm giác mạnh mẽ khi săn quái, vừa có cảm giác tính toán khi biến chiến lợi phẩm thành món ăn và phát triển nhà hàng. Game hướng tới cảm giác tò mò, thỏa mãn khi hạ elite/boss, bận rộn nhưng vui khi phục vụ và rõ ràng về tiến bộ sau mỗi vòng chơi.

**2.6.2. Trải nghiệm được lồng vào game:**

| Khoảnh khắc       | Cảm xúc mục tiêu    | Cách thể hiện                                                         |
| :------------------ | :---------------------- | :----------------------------------------------------------------------- |
| Bước vào dungeon | Tò mò \+ căng thẳng | Ánh sáng lạnh, ambience tối, âm thanh xa, room chưa lộ toàn bộ. |
| Hạ elite/boss      | Bùng nổ \+ thỏa mãn | Hit-stop ngắn, drop hiếm, VFX loot nổi bật.                          |
| Chuẩn bị menu     | Tính toán nhẹ        | Công thức rõ, hiển thị lợi nhuận dự kiến                        |
| Giờ phục vụ      | Bận rộn nhưng vui    | Order icon, timer dễ đọc, phản ứng của khách rõ.                 |
| Mua nâng cấp      | Cảm giác tiến bộ    | So sánh before/after về damage, capacity, patience.                    |

# **PHẦN 3: GAMEPLAY & MECHANICS**

## **3.1 Gameplay (Cách chơi)**

Gameplay trong game đi theo vòng lặp:

* **Pha 1: chiến đấu (dungeon)**
  * Hunt: di chuyển, tấn công, né trong dungeon để nhận nguyên liệu rơi ra từ quái và boss
  * Rương báu: chứa công thức nấu ăn
  * Loot: lựa chọn vật phẩm cần nhặt dựa trên giới hạn balo.
* **Pha 2: nhà hàng**
  * Cook: chọn công thức dựa trên tồn kho và giá bán, sau đó craft món ăn.
  * Serve: giao món theo order đến bàn và thời gian patience.
* **Pha trung gian:**
  * Upgrade: dùng Gold và vật liệu để nâng vũ khí, balo, nội thất nhà hàng.
  * Run tiếp theo: quay lại dungeon với sức mạnh/hiệu quả tốt hơn.
* **Cơ chế chuyển pha:** Mô phỏng một ngày
  * Giai đoạn 1 (Quãng nghỉ): Bắt đầu một ngày, trời sáng, người chơi xuất hiện trong phòng ngủ của nhà hàng. Người chơi có thể tương tác với npc quanh làng, mua đồ từ trưởng làng, nâng cấp nội thất nhà hàng, trang bị.
  * Giai đoạn 2 (Pha chiến đấu): Người chơi tiến vào dungeon sẽ chuyển sang giai đoạn 2 ngay lập tức. Nếu người chơi thoát ra khỏi dungeon ngay lập tức \-\> buổi chiều \-\> sang giai đoạn 3\. Nếu người chơi tiếp tục thám hiểm dungeon và sống sót trở ra \-\> buổi chiều \-\> sang giai đoạn 3\. Nếu người chơi chết trong lúc thám hiểm dungeon \-\> tỉnh dậy ở cửa dungeon \-\> buổi chiều \-\> sang giai đoạn 3
  * Giai đoạn 3 (Quãng nghỉ):  Người chơi có thể tương tác với npc quanh làng, mua đồ từ trưởng làng, nâng cấp nội thất nhà hàng, trang bị. Nếu người chơi trở về nhà hàng và tương tác với quầy thanh toán \-\> sang giai đoạn 4
  * Giai đoạn 4 (Pha nhà hàng): người chơi nấu ăn và phục phụ cho đến khi hết khách hoặc hết thời gian tối đa là 10 phút \-\> sang giai đoạn 5
  * Giai đoạn 5 (Quãng nghỉ): Trời tối, người chơi có thể tương tác với npc quanh làng, mua đồ từ trưởng làng, nâng cấp nội thất nhà hàng, trang bị. Nếu người chơi tương tác với chiếc giường trong phòng ngủ của nhà hàng \-\> lưu game \-\> sang giai đoạn 1

Qua các vòng lặp, một số hint về câu chuyện có thể sẽ được bật mí theo:

* Tỉ lệ: số vòng lặp càng nhiều \-\> tỷ lệ xuất hiện hint càng lớn, ví dụ có khả năng có vị khách sau khi ăn xong sẽ bắt chuyện và trả lời một số bí ẩn về ngôi làng hay về dungeon, về thế giới \- lore game
* Thỏa mãn một số điểu kiện để mở hint, ví dụ như làm các nhiệm vụ phụ

## **3.2 Mechanics (Quy luật trong Game)**

### **3.2.1. Hệ thống Combat:**

* Điều khiển:
  * WASD di chuyển 8 hướng;
  * LMB/J Light Attack; RMB/K Heavy hoặc Charged Attack;
  * Space Dash;
  * Q đổi vũ khí
  * R dùng item
  * E Weapon Skill;
  * F Interact/Pickup
  * Tab chuyển quick item
  * I hành trang (inventory)
  * M mở map
  * Esc Pause.
* Máu (HP): Player có HP cố định 100
* Phòng thủ: Dash là công cụ phòng thủ chính, có i-frame ngắn.
* Thể lực: Player có thể lực cố định 100, Dash mất thể lực 20, Tự hồi phục với tốc độ $10\ điểm/giây$ sau $0.5$ giây không tiêu tốn thể lực.
* Cảnh báo đòn đánh: Mọi đòn nguy hiểm của quái phải có telegraph tối thiểu khoảng 0,35–0,6 giây.
* Phản hồi va chạm: Hit feedback gồm flash/tint, hit sound, knockback nhẹ hoặc hit-stop 40–80 ms cho heavy hit.

**Hệ thống Vũ khí:** Cho phép thay đổi linh hoạt qua lại giữa combat.

* Item: Máu: hồi 20 máu mỗi lần uống, mỗi lần uống mất 1 lọ, mua từ trưởng làng

| Vũ khí | Vai trò                      | Light       | Heavy             | Skill                  | Trade-off                                   |
| :------- | :---------------------------- | :---------- | :---------------- | :--------------------- | :------------------------------------------ |
| Kiếm    | Cận chiến, an toàn vừa    | Combo 3 hit | Cleave hình cung | Whirlwind / Spin Slash | DPS ổn nhưng phải áp sát.              |
| Cung     | Tầm xa, kiểm soát vị trí | Quick Shot  | Charged Shot      | Piercing Arrow         | An toàn hơn nhưng dễ hụt và cần aim. |

#### Hệ thống Quái vật:

* Crep: 3 loại tầm xa, cận chiến, đột kích rải khắp dungeon.
* Boss: Xuất hiện ở cuối dungeon với lượng HP và bộ kỹ năng đặc biệt.

#### Hệ thống Inventory:

* Common Ingredients, Elite Material và Boss Material được thu thập trong dungeon, mất 50% khi chết trong dungeon.
* Balo có giới hạn slot; người chơi phải bỏ hoặc ưu tiên loot khi đầy.
* Inventory mặc định có 10 ô hành trang, mỗi item chứa được 5 stacks khi bắt đầu game, \+5 ô mỗi khi hạ một boss (qua 1 màn dungeon)
* Inventory có hàng quick item, vĩnh viễn chỉ có 4 ô, kéo thả item từ ô hành trang xuống ô quick item để xác lập quick item, RMB (chuột phải) để tháo quick item đã xác lập, quick item đã xác lập vẫn tồn tại trong ô hành trang
* Không thể mở inventory khi đang bị quái target

#### Map:

* Nhấn M để mở map
* Không thể mở map khi đang bị quái target

### **3.2.2. Hệ thống Restaurant:**

* Nội thất bao gồm:
  * Quầy bếp
  * Quầy thanh toán
  * 4 bộ bàn ghế, mỗi bộ gồm 1 bàn và 2 ghế
  * Tủ đồ
  * Hàng ghế chờ
* Món ăn có 3 nấc: Hoàn hảo, tốt, tệ
* Công thức (recipe) gồm tên món, danh sách nguyên liệu \+ số lượng, giá bán
* Chọn recipe đã mở và đủ nguyên liệu.
* Chọn số lượng và xem trước nguyên liệu tiêu hao cùng doanh thu dự kiến.
* Tạo món ăn ngay (có thể mở rộng cooking minigame sau).
* Ca phục vụ kéo dài 10 phút, tối đa 16 khách; khách có order icon và patience timer.
* Cơ chế reputation: tất cả mọi khách ban đầu có mặc định 20 điểm reputation

| NPC            | Reputation                                                                                                  | Tiền công                                                                                                                                                                                                |
| :------------- | :---------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Dân làng     | Ngồi chờ bàn: \-0.2 điểm/s Ngồi chờ món: \-0.2 điểm/s Món ăn: hoàn hảo \+3, tốt \+0, tệ \-3 | reputation\>= 15 là 100% giá món 15 \>  reputation \> 7 là 50% giá món 7 \>= reputation \> 0 là 25% giá món repution \= 0 là 0% giá món Thương nhân sẽ trả giá gấp x1.5 lần dân làng |
| Thương nhân | Ngồi chờ bàn: \-0.5 điểm/s Ngồi chờ món: \-0.5 điểm/s Món ăn: hoàn hảo \+5, tốt \+0, tệ \-5 |                                                                                                                                                                                                            |

#### Hệ thống Economy:

* Gold nhận từ bán món và không mất khi chết trong dungeon.
* Reputation nhận từ kết quả phục vụ và không mất khi chết.

## **3.3 Tùy chọn Game (Game Options)**

*(Chưa cập nhật)*

## **3.4 Giới hạn & Rào cản (Constraints)**

*(Chưa cập nhật)*

# **PHẦN 4: STORY, SETTING & CHARACTERS**

## **4.1 Cốt truyện (Story)**

Ngôi làng Londo từng là một ngôi làng xinh đẹp, bao quanh bởi khu rừng và các ngọn núi cao, muốn từ làng tiến ra thế giới bên ngoài cực kì khó khăn do địa hình hiểm trở, do đó, việc liên hệ giữa làng với thế giới bên ngoài là vô cùng hạn chế, chỉ thỉnh thoảng có một vài thương nhân lui đến tìm kiếm các cơ hội làm ăn

Một ngày nọ, trong khu rừng gần làng xuất hiện một dungeon, âm khí từ dugeon tràn ra và bao phủ khắp làng khiến sản vật của ngôi làng dần thối nát, cánh đồng và các cây lương thực chết khô. Ngôi làng trước giờ mang màu sắc rực rỡ của cây trái nay bị thay thế bằng một màu xanh, nhưng màu xanh đó không tượng trưng cho sự no đủ mà là màu xanh của cây cỏ dại. Do ám khí từ dungeon, các loại cây dại phát triển mạnh mẽ, chúng tiêu diệt các các cánh đồng của làng, biến Londo từ một ngôi làng tràn đầy sức sống trở nên xơ xác, nghèo đói

Sau một thời gian, nhân vật chính xuất hiện từ trong lòng dungeon, định cư tại ngôi làng và mở một nhà hàng ăn. Nhân vật chính dùng món ăn nấu từ những nguyên liệu của dungeon để cứu rỗi dân làng, các thương nhân từ thế giới bên ngoài cũng tìm đến

Nhân vật chính không biết mình là ai, qua cuộc sống ở làng và các cuộc thám hiểm vào dungeon, nhân vật chính nhận ra bản thân là tạo vật được vị thần tồn tại ở đáy của dungeon tạo ra, hắn tên Gwyn. Nhân vật chính nhận ra bản thân được tạo thành từ nguồn sống của ngôi làng, chính là căn nguyên dẫn ngôi làng đến cảng nghèo đói, xơ xác và ám khí từ dungeon chính là chiếc vòi hút sinh khí để Gwyn tạo ra anh

Nhân vật chính đứng trước hai lựa chọn. Thứ nhất, chọn tiêu diệt vĩnh viễn Gwyn trong trận chiến cuối cùng, nhân vật chính và dungeon cũng chết theo, trò chơi kết thúc. Thứ hai, chọn phong ấn vị thần Gwyn, nhân việt chính và dungeon vẫn sẽ tồn lại, nhân vật chính tiếp tục vận hành quán ăn, tiếp tục lừa dân làng tin rằng “Bạn chính là vị cứu tinh”

## **4.2 Bối cảnh**

Hai bản đồ:

* Bản đồ 1: Ngôi làng gồm có quán ăn của nhân vật chính, nhà dân, dân làng, người chơi vận hành quán ăn để thu tiền, mức độ hài lòng của khách và nâng đồ, nội thất
* Bản đồ 2: Dungeon gồm các tầng dungeon để người chơi thám hiểm, càng tiến xâu, người chơi càng thu được nhiều nguyên liệu ngon và tìm hiểu về lore game

## **4.3 Nhân vật (Characters)**

* **Nhân vật chính:** Nhân vật người chơi nhập vai
* **Dân làng (NPC làng):**
  * Trưởng làng Godwin: lão già
  * Gia đình 1: chồng Sam, vợ Nana, con gái Leaf
  * Gia đình 2: chồng Gam, vợ Ana, con trai Vincent
  * Gia đình 3: chồng Dam, vợ Sana, con gái Vini
* **Khách (NPC khách):** Các thương nhân từ thế giới bên ngoài
* **Kẻ địch:**
  * Bộ xương khô, có trang bị cung, dùng cung để tấn công
  * Slime, các cục nhầy nhụa nhiều màu sắc, tấn công bằng cách lao đến đối thủ
  * Thây ma: các xác xống dùng kiếm để tấn công
* **Boss:**
  * Tầng 1: con rắn khổng lồ
  * Tầng 2: con lợn rừng khổng lồ
  * Tầng 3: người đá khổng lồ
  * Tầng 4: dual boss, 2 hiệp sĩ
  * Tầng 5: Gwyn, thần sáng tạo

# **PHẦN 5: THIẾT KẾ MÀN CHƠI (LEVEL DESIGN)**

## **5.1. Pha kinh doanh (nhà hàng)**

Màn chơi nhà hàng:

* Tóm tắt: màn chơi ở chế độ dễ, tạo cảm giác dễ chơi để người chơi chill sau khi tập trung chiến đấu ở dungeon
* Điều kiện yêu cầu: không có điều kiện yêu cầu cho màn này
* Chi tiết có thể xảy ra:
  * Khách là dân làng sẽ đến quán
  * Khách là thương nhân sẽ đến quán
  * Khách cho manh mối về lore game (có thể là thương nhân hoặc dân làng)
* Phần thưởng khi qua màn: Điểm reputation, tiền vàng, manh mối về cốt truyện game
* Trải nghiệm của người chơi khi qua màn:
* Động lực để qua màn sau: Phục vụ khách càng tốt thì vàng và điểm raputation nhận được càng nhiều
* Map: Nhà hàng

## **5.2. Pha chiến đấu (dungeon)**

Màn chơi tầng 1 dungeon:

* Tóm tắt: màn chơi ở chế độ dễ, tạo cảm giác dễ chơi để người chơi bắt đầu. Kích thích sự hứng thú, khám phá của người chơi. Người chơi được trải nghiệm nhân vật trong game, khung cảnh, âm thanh, hình ảnh trong game. Màn chơi với độ khó không cao giúp người chơi dễ dàng vượt qua tạo cảm giác chiến thắng, từ đó đưa người chơi đến với những màn chơi khó hơn.
* Điều kiện yêu cầu: không có điều kiện yêu cầu cho màn này
* Chi tiết có thể xảy ra:
  * Sau khi hạ quái vật có thể rơi ra vật phẩm,
  * Bị quái vật tấn công thì nhân vật sẽ mất máu,
  * Bị nhân vật tấn công thì quái sẽ mất máu,
  * Nhân vật đạp bẫy thì mất máu
  * Khi máu của nhân vật về mức 0 thì nhân vật sẽ chết,
  * Khi máu của quái vật về mức 0 thì quái vật sẽ chết,
  * Cách qua màn: đánh bại boss rắn khổng lồ
* Phần thưởng khi qua màn: Nguyên liệu hiếm để nấu ăn, nâng cấp trang bị, nội thất nhà hàng và vàng để nâng cấp trang bị, nội thất nhà hàng
* Trải nghiệm của người chơi khi qua màn: người chơi được trải nghiệm cảm giác nhập vai và chiến đấu với quái vật, bước đầu tạo cảm giác nhập vai, dẫn dắt người chơi đến với những quái vật khó hơn phía sau.
* Động lực để qua màn sau: tạo sự kích thích từ hình ảnh và âm thanh, tạo sự ham muốn tiến đến những màn khó hơn phía sau. Nhận những phần thưởng để có thể chiến đấu với những quái vật phía sau
* Map: Ngục tối

Màn chơi tầng 2 dungeon:

* Tóm tắt: đầm lầy độc khiến người chơi mất máu mỗi 2s, di chuyển chậm hơn, quái đông hơn và máu trâu hơn x1.5 lần
* Điều kiện yêu cầu: qua màn 1
* Chi tiết có thể xảy ra:
  * Sau khi hạ quái vật có thể rơi ra vật phẩm,
  * Bị quái vật tấn công thì nhân vật sẽ mất máu,
  * Bị nhân vật tấn công thì quái sẽ mất máu,
  * Nhân vật đạp bẫy thì mất máu
  * Nhân vật đạp phải vũng đầm lầy độc trên sàn mất máu mỗi 2s, làm chậm x2 lần
  * Khi máu của nhân vật về mức 0 thì nhân vật sẽ chết,
  * Khi máu của quái vật về mức 0 thì quái vật sẽ chết,
  * Cách qua màn: đánh bại boss lợn rừng khổng lồ
* Phần thưởng khi qua màn: Nguyên liệu hiếm để nấu ăn, nâng cấp trang bị, nội thất nhà hàng và vàng để nâng cấp trang bị, nội thất nhà hàng
* Trải nghiệm của người chơi khi qua màn: khó, thử thách
* Động lực để qua màn sau: cảm giác sung sướng khi qua màn
* Map: Đầm lầy độc

Màn chơi tầng 3 dungeon:

* Tóm tắt: băng giá làm tốc độ ra đòn giảm x2 lần, quái đông hơn và máu trâu hơn x1.5 lần
* Điều kiện yêu cầu: qua màn 2
* Chi tiết có thể xảy ra:
  * Sau khi hạ quái vật có thể rơi ra vật phẩm,
  * Bị quái vật tấn công thì nhân vật sẽ mất máu,
  * Bị nhân vật tấn công thì quái sẽ mất máu,
  * Nhân vật đạp bẫy thì mất máu
  * Tốc độ đánh giảm x2 lần
  * Khi máu của nhân vật về mức 0 thì nhân vật sẽ chết,
  * Khi máu của quái vật về mức 0 thì quái vật sẽ chết,
  * Cách qua màn: đánh bại boss người đá khổng lồ
* Phần thưởng khi qua màn: Nguyên liệu hiếm để nấu ăn, nâng cấp trang bị, nội thất nhà hàng và vàng để nâng cấp trang bị, nội thất nhà hàng
* Trải nghiệm của người chơi khi qua màn: khó, thử thách
* Động lực để qua màn sau: cảm giác sung sướng khi qua màn
* Map: Băng tuyết

Màn chơi tầng 4 dungeon:

* Tóm tắt: Cột lửa thiêu đốt ¼ máu của người chơi mỗi lần dính phải, quái đông hơn và máu trâu hơn x1.5 lần
* Điều kiện yêu cầu: qua màn 3
* Chi tiết có thể xảy ra:
  * Sau khi hạ quái vật có thể rơi ra vật phẩm,
  * Bị quái vật tấn công thì nhân vật sẽ mất máu,
  * Bị nhân vật tấn công thì quái sẽ mất máu,
  * Nhân vật đạp bẫy thì mất máu
  * Cột lửa phun từ nền nhà làm người chơi mất máu nếu chạm vào
  * Khi máu của nhân vật về mức 0 thì nhân vật sẽ chết,
  * Khi máu của quái vật về mức 0 thì quái vật sẽ chết,
  * Cách qua màn: đánh bại boss hai hiệp sĩ
* Phần thưởng khi qua màn: Nguyên liệu hiếm để nấu ăn, nâng cấp trang bị, nội thất nhà hàng và vàng để nâng cấp trang bị, nội thất nhà hàng
* Trải nghiệm của người chơi khi qua màn: khó, thử thách
* Động lực để qua màn sau: cảm giác sung sướng khi qua màn
* Map: Lửa

Màn chơi tầng 5 dungeon:

* Tóm tắt: quái và bẫy rất mạnh, ăn 1 chiêu là nằm, quái đông hơn và máu trâu hơn x1.5 lần
* Điều kiện yêu cầu: qua màn 4
* Chi tiết có thể xảy ra:
  * Sau khi hạ quái vật có thể rơi ra vật phẩm,
  * Bị quái vật tấn công thì nhân vật sẽ mất máu,
  * Bị nhân vật tấn công thì quái sẽ mất máu,
  * Nhân vật đạp bẫy thì mất máu
  * Khi máu của nhân vật về mức 0 thì nhân vật sẽ chết,
  * Khi máu của quái vật về mức 0 thì quái vật sẽ chết,
  * Cách qua màn: đánh bại boss Gwyn
* Phần thưởng khi qua màn: Nguyên liệu hiếm để nấu ăn, nâng cấp trang bị, nội thất nhà hàng và vàng để nâng cấp trang bị, nội thất nhà hàng hoặc kết thúc game
* Trải nghiệm của người chơi khi qua màn: khó, thử thách
* Động lực để qua màn sau: cảm giác sung sướng khi qua màn
* Map: Ngục tối

# **PHẦN 6: GIAO DIỆN & ÂM THANH (UI/UX & AUDIO)**

## **6.1 Giao diện người dùng (UI)**

* **Màn hình HUD:** Thanh HP/MP, bản đồ nhỏ, danh sách phím tắt, chỉ số.
* **Hệ thống Menu:** Menu chính, Pause Menu, Inventory, Game Over.
* **Chế độ Camera:** Top-down / Third-person...

## **6.2 Hệ thống Điều khiển (Control System)**

Phím bấm (Bàn phím \+ Chuột / Tay cầm). Bảng tra cứu phím nóng.

## **6.3 Âm thanh (Audio)**

Bảng tổng hợp nhạc nền (BGM) và hiệu ứng âm thanh (SFX).

## **6.4 Hệ thống Trợ giúp (Tutorial & Help)**

Hướng dẫn tân thủ và các mẹo trong game.

# **PHẦN 7: GAME MATRIX**

## **7.1. Bảng ma trận nguyên liệu**

| STT          | Tên nguyên liệu                   | Tầng xuất hiện | Nguồn gốc / Kẻ địch rơi ra   | Phân loại             | Giới hạn xếp chồng (Stack) | Ghi chú & Công dụng                                                |
| :----------- | :----------------------------------- | :---------------- | :--------------------------------- | :---------------------- | :----------------------------- | :-------------------------------------------------------------------- |
| **1**  | **Nấm hang phát quang**      | Tầng 1           | Nhặt ở môi trường / Cây nấm | Thường                | 10                             | Tỏa sáng xanh dịu, vị ngọt giòn thanh đạm.                    |
| **2**  | **Chất nhầy dẻo**           | Tầng 1           | Rơi từ Slime thường            | Thường                | 10                             | Dai, đàn hồi, dùng làm chất tạo đông cho món ăn.           |
| **3**  | **Mẩu xương vụn**          | Tầng 1, 2        | Rơi từ Bộ xương khô          | Thường                | 10                             | Đập dập ninh lấy tủy hầm xúp.                                  |
| **4**  | **Thịt nạc Rắn khổng lồ** | Tầng 1           | Rơi từ Boss Rắn                 | **Hiếm (Boss)**  | 10                             | Thớ thịt dày, chắc, thơm nồng.                                  |
| **5**  | **Thớ thịt khô**            | Tầng 2, 4        | Rơi từ Thây ma                  | Thường                | 10                             | Thịt sấy tự nhiên trong ngục tối, mằn mặn.                    |
| **6**  | **Củ rêu lọc độc**        | Tầng 2           | Rơi từ Slime Độc               | Thường                | 10                             | Rễ thảo mộc đầm lầy, trung hòa ám khí độc hại.            |
| **7**  | **Nạc vai heo rừng**         | Tầng 2           | Rơi từ Boss Lợn Rừng           | **Hiếm (Boss)**  | 10                             | Tỷ lệ mỡ nạc đan xen béo ngậy.                                 |
| **8**  | **Tinh thể tuyết vụn**      | Tầng 3           | Nhặt ở nền băng Tầng 3        | Thường                | 10                             | Bụi băng vĩnh cửu, giữ nhiệt lạnh cực lâu.                   |
| **9**  | **Dịch Slime đông lạnh**   | Tầng 3           | Rơi từ Slime Băng               | Thường                | 10                             | Sền sệt, mát lạnh như kem tuyết.                                |
| **10** | **Tủy đá nguyên khối**    | Tầng 3           | Rơi từ Boss Người Đá         | **Hiếm (Boss)**  | 10                             | Khoáng thạch mềm như bơ nằm trong ngực quái đá.             |
| **11** | **Hỏa thạch vụn**           | Tầng 4           | Rơi từ Quái rực lửa Tầng 4   | Thường                | 10                             | Tỏa nhiệt lượng cao, dùng áp chảo tái món ăn.               |
| **12** | **Trứng thiết giáp quái**  | Tầng 4           | Rơi từ Boss Đôi Hiệp Sĩ      | **Hiếm (Boss)**  | 10                             | Vỏ cứng ánh kim, lòng đỏ bùi béo.                             |
| **13** | **Hạt nhân Sinh Mệnh**      | Tầng 5           | Rơi từ Boss Cuối Gwyn           | **Huyền thoại** | 1                              | Nguồn năng lượng tạo nên dungeon và ký ức nhân vật chính. |

## **7.2. Bảng ma trận món ăn**

| STT          | Tên món ăn                                | Tầng mở khóa | Công thức nguyên liệu yêu cầu                                                                                                                      | Giá bán cơ bản (Gold) | Món thường / Món Boss |
| :----------- | :------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------ | :------------------------ |
| **1**  | **Xúp Nấm Dạ Quang**                | Tầng 1         | • 2× Nấm hang phát quang • 1× Mẩu xương vụn                                                                                                    | **30**              | Món thường             |
| **2**  | **Thạch Slime Mềm**                  | Tầng 1         | • 3× Chất nhầy dẻo                                                                                                                                  | **45**              | Món thường             |
| **3**  | **Thịt Nướng Độc Mãng Xà**      | Tầng 1         | • 1× Thịt nạc Rắn khổng lồ • 2× Nấm hang phát quang                                                                                           | **130**             | **Món Boss**       |
| **4**  | **Thịt Xông Khói Tro Tàn**         | Tầng 2         | • 2× Thớ thịt khô • 1× Mẩu xương vụn                                                                                                          | **60**              | Món thường             |
| **5**  | **Thịt Heo Rừng Hầm Đầm Lầy**    | Tầng 2         | • 1× Nạc vai heo rừng • 2× Củ rêu lọc độc                                                                                                     | **180**             | **Món Boss**       |
| **6**  | **Kem Đá Pha Lê Băng**             | Tầng 3         | • 2× Tinh thể tuyết vụn • 2× Dịch Slime đông lạnh                                                                                             | **85**              | Món thường             |
| **7**  | **Tủy Đá Hấp Rêu Cổ**            | Tầng 3         | • 1× Tủy đá nguyên khối • 2× Tinh thể tuyết vụn                                                                                              | **240**             | **Món Boss**       |
| **8**  | **Thịt Tái Bốc Lửa**               | Tầng 4         | • 2× Thớ thịt khô • 1× Hỏa thạch vụn                                                                                                           | **120**             | Món thường             |
| **9**  | **Bữa Tiệc Thiết Kỵ Hoàng Gia**   | Tầng 4         | • 1× Trứng thiết giáp quái • 2× Hỏa thạch vụn                                                                                                 | **350**             | **Món Boss**       |
| **10** | **Canh Khởi Nguyên (Mỹ Vị Cuối)** | Tầng 5         | • 1× Hạt nhân Sinh Mệnh • 1× Thịt nạc Rắn khổng lồ • 1× Nạc vai heo rừng • 1× Tủy đá nguyên khối • 1× Trứng thiết giáp quái | **1000**            | **Món Cuối**      |

## **7.3. Bảng ma trận quái thường(CREEPS BALANCE MATRIX)**

*Quy ước:*

* Quái thường chỉ có đúng 3 chủng loại: **Slime** (đột kích/nhảy vồ), **Bộ Xương Khô** (tầm xa/bắn cung), và **Thây Ma** (cận chiến/dùng kiếm).
* Các đòn đánh chỉ trừ thẳng vào HP của người chơi, **hoàn toàn không có hiệu ứng** (không độc, không làm chậm, không choáng, không thiêu đốt).
* Độ trâu (HP) và Sát thương (ATK) tăng tiến theo cấp số nhân qua từng tầng (x1.5 lần qua mỗi tầng ngục theo thiết kế màn chơi ở Phần 5).

| STT          | Tên Quái vật                  | Tầng xuất hiện | Vai trò / Cách tấn công | Máu (HP) | Sát thương (ATK) | Tốc độ chạy | Tầm đánh & Tần suất ra đòn | Hành vi cơ bản                                                             |
| :----------- | :------------------------------- | :---------------- | :-------------------------- | :-------- | :------------------ | :-------------- | :-------------------------------- | :---------------------------------------------------------------------------- |
| **1**  | **Slime Nhầy**            | Tầng 1           | Đột kích                 | 40        | 8                   | 2.5 u/s         | Cận chiến (1.0s / đòn)        | Nhảy chồm về phía trước gây sát thương khi vào cự ly 2 units.     |
| **2**  | **Bộ Xương Cung Thủ**  | Tầng 1           | Tầm xa                     | 30        | 10                  | 2.0 u/s         | Bắn xa 8 units (1.5s / tên)     | Giữ khoảng cách và giương cung bắn thẳng vào vị trí người chơi. |
| **3**  | **Thây Ma Cận Chiến**   | Tầng 1           | Cận chiến                 | 60        | 12                  | 1.8 u/s         | Cận chiến (1.2s / chém)        | Áp sát và chém kiếm thẳng mặt người chơi.                           |
| **4**  | **Slime Đầm Lầy**       | Tầng 2           | Đột kích                 | 60        | 12                  | 2.5 u/s         | Cận chiến (1.0s / đòn)        | Tăng 1.5x máu so với T1; nhảy chồm gây dame thuần.                     |
| **5**  | **Bộ Xương Đầm Lầy** | Tầng 2           | Tầm xa                     | 45        | 15                  | 2.0 u/s         | Bắn xa 8 units (1.5s / tên)     | Mũi tên gây dame lớn hơn, không kèm độc.                             |
| **6**  | **Thây Ma Đầm Lầy**    | Tầng 2           | Cận chiến                 | 90        | 18                  | 1.8 u/s         | Cận chiến (1.2s / chém)        | Máu trâu x1.5 lần, chém dame nặng tay hơn.                              |
| **7**  | **Slime Băng Giá**       | Tầng 3           | Đột kích                 | 90        | 18                  | 2.5 u/s         | Cận chiến (1.0s / đòn)        | Nhảy chồm gây dame vật lý, không kèm hiệu ứng đóng băng.          |
| **8**  | **Bộ Xương Băng Giá** | Tầng 3           | Tầm xa                     | 70        | 22                  | 2.0 u/s         | Bắn xa 8 units (1.5s / tên)     | Bắn cung gây dame thuần.                                                   |
| **9**  | **Thây Ma Băng Giá**    | Tầng 3           | Cận chiến                 | 135       | 26                  | 1.8 u/s         | Cận chiến (1.2s / chém)        | Tăng chỉ số máu và lực chém kiếm.                                     |
| **10** | **Slime Rực Lửa**        | Tầng 4           | Đột kích                 | 135       | 26                  | 2.5 u/s         | Cận chiến (1.0s / đòn)        | Đốm lửa đỏ nhưng chỉ gây dame va chạm, không gây cháy.            |
| **11** | **Bộ Xương Rực Lửa**  | Tầng 4           | Tầm xa                     | 105       | 32                  | 2.0 u/s         | Bắn xa 8 units (1.5s / tên)     | Bắn tên sát thương cao.                                                  |
| **12** | **Thây Ma Rực Lửa**     | Tầng 4           | Cận chiến                 | 200       | 38                  | 1.8 u/s         | Cận chiến (1.2s / chém)        | Cực trâu, chém đòn mạnh mẽ.                                            |
| **13** | **Slime Cổ Đại**        | Tầng 5           | Đột kích                 | 200       | 38                  | 2.5 u/s         | Cận chiến (1.0s / đòn)        | Biến thể quái mạnh nhất tầng cuối.                                     |
| **14** | **Bộ Xương Cổ Đại**  | Tầng 5           | Tầm xa                     | 160       | 45                  | 2.0 u/s         | Bắn xa 8 units (1.5s / tên)     | Bắn tên uy lực cực mạnh.                                                 |
| **15** | **Thây Ma Cổ Đại**     | Tầng 5           | Cận chiến                 | 300       | 55                  | 1.8 u/s         | Cận chiến (1.2s / chém)        | Sát thương chém cực cao, ăn 2 nhát là mất gần hết máu.            |

## **7.4. Bảng ma trận boss (BOSS BALANCE MATRIX)**

*Quy ước:*

* Xuất hiện ở căn phòng cuối cùng của mỗi tầng ngục.
* Sở hữu lượng HP vượt trội và các đòn đánh uy lực diện rộng (AoE), đòi hỏi người chơi dùng Dash né chiêu chuẩn xác qua báo hiệu (Telegraph).

| Tầng ngục       | Tên Boss                             | Danh hiệu / Hình thái      | Máu (HP)                                 | Sát thương (ATK) | Tốc độ chạy | Chuỗi Kỹ năng Đòn đánh                                                                                                                                           | Tỷ lệ Rơi Nguyên Liệu Boss           |
| :---------------- | :------------------------------------ | :---------------------------- | :---------------------------------------- | :------------------ | :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------- |
| **Tầng 1** | **Rắn Khổng Lồ**             | Chúa tể hang tối           | **450**                             | **20 \- 25**  | 3.2 u/s         | • Quét đuôi 180° phạm vi 3 units (20 DMG). • Cắn bổ nhào về phía trước (25 DMG).                                                                          | 100% rơi: 1× Thịt nạc Rắn khổng lồ |
| **Tầng 2** | **Lợn Rừng Khổng Lồ**       | Bá chủ đầm lầy           | **750**                             | **28 \- 35**  | 4.5 u/s         | • Lao húc thẳng đường dài (35 DMG). • Dậm chân chấn động mặt đất xung quanh (28 DMG).                                                                   | 100% rơi: 1× Nạc vai heo rừng         |
| **Tầng 3** | **Người Đá Khổng Lồ**     | Hộ vệ băng nguyên         | **1200**                            | **40 \- 50**  | 1.6 u/s         | • Nện 2 tay xuống đất tạo sóng lan tỏa (45 DMG). • Quét tay hình nón phía trước (40 DMG).                                                                | 100% rơi: 1× Tủy đá nguyên khối    |
| **Tầng 4** | **Đôi Hiệp Sĩ (Dual Boss)** | Song kiếm cấm vệ           | **Mỗi con 600** *(Tổng: 1200\)* | **35 \- 45**  | 3.8 u/s         | • Hiệp sĩ Kiếm: Chém 3 nhát liên hoàn áp sát (35 DMG/hit). • Hiệp sĩ Giáo: Đâm thẳng tầm xa xuyên thấu (45 DMG).                                    | 100% rơi: 1× Trứng thiết giáp quái  |
| **Tầng 5** | **Gwyn**                        | Thần sáng tạo (Boss Cuối) | **2500**                            | **55 \- 75**  | 3.5 u/s         | • Bắn chùm đạn ma pháp toàn sàn (55 DMG). • Triệu hồi cột năng lượng dội từ trần nhà (75 DMG). • Chém sóng kiếm càn quét diện rộng (65 DMG). | 100% rơi: 1× Hạt nhân Sinh Mệnh      |

## **7.5. Bảng ma trận trang bị**

*Quy ước:*

* Tối đa 4 cấp độ (Lv.1 đến Lv.4), mỗi lần nâng cấp thuần tăng chỉ số sát thương.
* Mở khóa lần lượt sau khi hạ Boss Tầng 1, Boss Tầng 2 và Boss Tầng 3\.

| Tên Trang bị  | Cấp độ                 | Chi phí nâng cấp                                                        | Sát thương đánh thường (Light Attack) DOCX | Sát thương đòn mạnh (Heavy Attack) DOCX | Sát thương Kỹ năng (Weapon Skill) DOCX | Ghi chú điều kiện  |
| :-------------- | :------------------------ | :------------------------------------------------------------------------- | :------------------------------------------------ | :-------------------------------------------- | :------------------------------------------ | :--------------------- |
| **Kiếm** | **Lv.1** *(Gốc)* | 0                                                                          | Combo 3 hit: 15 / 15 / 25 DMG                     | 40 DMG                                        | 60 DMG (\$5 \\times 12\$ DMG)               | Mặc định đầu game |
|                 | **Lv.2**            | • 150 Gold • 1× Thịt nạc Rắn khổng lồ • 3× Mẩu xương vụn     | Combo 3 hit: 22 / 22 / 38 DMG                     | 55 DMG                                        | 85 DMG (\$5 \\times 17\$ DMG)               | Hạ Boss Tầng 1       |
|                 | **Lv.3**            | • 450 Gold • 1× Nạc vai heo rừng • 4× Củ rêu lọc độc           | Combo 3 hit: 32 / 32 / 55 DMG                     | 80 DMG                                        | 125 DMG (\$5 \\times 25\$ DMG)              | Hạ Boss Tầng 2       |
|                 | **Lv.4** *(Max)*  | • 950 Gold • 1× Tủy đá nguyên khối • 5× Tinh thể tuyết vụn    | Combo 3 hit: 48 / 48 / 80 DMG                     | 120 DMG                                       | 180 DMG (\$5 \\times 36\$ DMG)              | Hạ Boss Tầng 3       |
| **Cung**  | **Lv.1** *(Gốc)* | 0                                                                          | 12 DMG / mũi tên                                | 20 – 50 DMG (tùy sạc)                      | 45 DMG                                      | Mặc định đầu game |
|                 | **Lv.2**            | • 150 Gold • 1× Thịt nạc Rắn khổng lồ • 3× Nấm hang phát quang | 18 DMG / mũi tên                                | 30 – 70 DMG                                  | 65 DMG                                      | Hạ Boss Tầng 1       |
|                 | **Lv.3**            | • 450 Gold • 1× Nạc vai heo rừng • 4× Củ rêu lọc độc           | 26 DMG / mũi tên                                | 45 – 100 DMG                                 | 100 DMG                                     | Hạ Boss Tầng 2       |
|                 | **Lv.4** *(Max)*  | • 950 Gold • 1× Tủy đá nguyên khối • 5× Tinh thể tuyết vụn    | 40 DMG / mũi tên                                | 65 – 145 DMG                                 | 150 DMG                                     | Hạ Boss Tầng 3       |

## **7.6. Bảng ma trận nội thất**

*Quy ước:*

* Tối đa 3 cấp độ (Lv.1 đến Lv.3).
* Chỉ số mặc định: Dân làng mất \$-0.2\\text{ điểm/s}\$, Thương nhân mất \$-0.5\\text{ điểm/s}\$ khi chờ đợi.
* Cơ chế duy nhất: Giảm trừ điểm mất Reputation khi khách phải chờ bàn hoặc chờ món.

| STT         | Nội thất nâng cấp       | Cấp độ                 | Chi phí nâng cấp (Gold) | Hiệu ứng cơ chế (Giảm tốc độ trừ Reputation)                                             | Điều kiện mở khóa          |
| :---------- | :-------------------------- | :------------------------ | :------------------------- | :------------------------------------------------------------------------------------------------ | :------------------------------ |
| **1** | **Bàn ghế ăn**     | **Lv.1** *(Gốc)* | 0                          | Không giảm (mất nguyên bản \$-0.2\$ hoặc \$-0.5\\text{ điểm/s}\$)                         | Có sẵn 4 bộ ban đầu        |
|             |                             | **Lv.2**            | 250                        | Giảm **15%** tốc độ trừ Reputation khi khách ngồi chờ món                          | Vượt qua Tầng 1 Dungeon      |
|             |                             | **Lv.3** *(Max)*  | 650                        | Giảm **30%** tốc độ trừ Reputation khi khách ngồi chờ món                          | Vượt qua Tầng 3 Dungeon      |
| **2** | **Ghế chờ**         | **Lv.1** *(Gốc)* | 0                          | Không giảm (mất nguyên bản \$-0.2\$ hoặc \$-0.5\\text{ điểm/s}\$)                         | Có sẵn 1 hàng ghế           |
|             |                             | **Lv.2**            | 200                        | Giảm **15%** tốc độ trừ Reputation khi khách chờ bàn trống                         | Vượt qua Tầng 1 Dungeon      |
|             |                             | **Lv.3** *(Max)*  | 550                        | Giảm **30%** tốc độ trừ Reputation khi khách chờ bàn trống                         | Vượt qua Tầng 2 Dungeon      |
| **3** | **Quầy thu ngân**   | **Lv.1** *(Gốc)* | 0                          | Không giảm                                                                                      | Mặc định đầu game          |
|             |                             | **Lv.2**            | 300                        | Giảm thêm **10%** tốc độ trừ Reputation toàn quán                                   | Vượt qua Tầng 2 Dungeon      |
|             |                             | **Lv.3** *(Max)*  | 850                        | Giảm thêm **20%** tốc độ trừ Reputation toàn quán                                   | Vượt qua Tầng 4 Dungeon      |
| **4** | **Không gian quán** | **Lv.1** *(Gốc)* | 0                          | Không giảm                                                                                      | Mặc định đầu game          |
|             |                             | **Lv.2**            | 400                        | Tốc độ trừ Reputation của Thương nhân giảm từ \$-0.5 \\rightarrow \-0.35\\text{ đ/s}\$ | Tích lũy đạt 200 Reputation |
|             |                             | **Lv.3** *(Max)*  | 900                        | Tốc độ trừ Reputation của Thương nhân giảm tiếp về \$-0.25\\text{ đ/s}\$              | Tích lũy đạt 400 Reputation |

## **7.7. Bảng ma trận item**

*Quy ước:*

* Nguồn mua: Mua trực tiếp từ NPC Trưởng làng Godwin tại làng vào các quãng nghỉ (Giai đoạn 1, 3, 5).
* Cách sử dụng: Kéo thả từ Balo vào thanh Quick Item (tối đa 4 ô), bấm Tab để chuyển đổi và bấm R để kích hoạt/ném.
* Không phải nguyên liệu nấu ăn, không dùng để chế biến món ăn.

| STT         | Tên Vật phẩm                 | Phân loại                                      | Giá mua (Gold) | Giới hạn xếp chồng (Stack) | Cơ chế hoạt động & Chỉ số chi tiết                                                                                                                                                                                                               | Điều kiện mở bán tại Trưởng làng |
| :---------- | :------------------------------ | :----------------------------------------------- | :-------------- | :----------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------- |
| **1** | **Bình Máu Hồi Phục** | Hồi phục                                       | **25**    | 5                              | • Hồi ngay lập tức **20 HP**. • Thời gian uống: 0.3s (nhân vật khựng nhẹ).                                                                                                                                                              | Có sẵn từ đầu game                   |
| **2** | **Lựu Đạn Nổ**        | Sát thương diện rộng (AoE)                  | **40**    | 5                              | • Ném theo đường vòng cung tối đa 7m. • Gây **50 Damage vật lý** trong bán kính nổ 2.5m. • Đẩy lùi nhẹ các quái nhỏ ra xa tâm nổ.                                                                                         | Mở bán sau khi hạ Boss Tầng 1         |
| **3** | **Lựu Đạn Băng**      | Khống chế                                      | **50**    | 5                              | • Nổ phát tán khí lạnh bán kính 2.5m. • Gây **15 Damage** và **Đóng băng bất động** toàn bộ quái dính chiêu trong **2.0 giây** (Boss chỉ bị trói chân 1.0 giây).                                           | Mở bán sau khi hạ Boss Tầng 2         |
| **4** | **Lựu Đạn Độc**      | Sát thương theo thời gian (DoT) & Làm chậm | **45**    | 5                              | • Tạo một vùng khói độc bán kính 3.0m tồn tại trong **4.0 giây**. • Quái đứng trong vùng bị **Làm chậm 40%** tốc độ di chuyển. • Gây **8 Damage/0.5s** (tổng cộng 64 Damage nếu đứng trọn thời gian). | Mở bán sau khi hạ Boss Tầng 1         |

[image3]: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAloAAAD1CAYAAACIjlv4AABLcUlEQVR4Xu3dBbQsR/X2YdzdJXgI7m7B3Z3gbiEE1xAgECxIcHdIgiS4u7u7u7s7/a2n/mvP17eZOfecM3bPnPe3Vq+p7mnv6t1v7aradaQuhBBCCCHMhSMNF4QQQgghhNkQoRVCCCGEMCcitEIIIYQQ5kSEVgghhBDCnIjQCiGEEEKYExFaIYQQQghzIkIrhBBCCGFORGiFEEIIIcyJCK0QQgghhDkRoRVCCCGEMCcitEIIIYQQ5kSEVgghhBDCnIjQCiGEEEKYExFaIYQQQghzIkIrhBBCCGFORGiFEEIIIcyJCK0QQgghhDkRoRVCCCGEMCcitMJEjnSkI2XKlClTphlNYXuSJx8mMjQSmTJlypRp81PYnuTJh4nEQIQQwnTEjoY8+TCRGIgQQpiO2NGQJx8mEgMRQgjTETsa8uTDRGIgQghhOmJHQ558mEgMRAghTEfsaMiTDxOJgQghhOmIHQ158mEiMRAhhDAdsaMhTz5MJAYihBCmI3Y05MmHicRALI+XvvSl3alOdarh4l2Wxz3ucd25znWu4eIQtj2xoyFPPkwkBmK5fPzjH+++/OUvDxfvsrz+9a/vfvOb3wwX75R///vfw0UhrAyxoyFPPkwkBmJ2nOlMZ+qOe9zjju7lK17xiu7Yxz52d+UrX3m0zgMe8IDuC1/4QvfjH/+4u+Y1r9ntvffe3SUucYnuoIMOav/77xjHOEb30Y9+dLTNn//85+7e9753SxM5N7jBDdo6ZzzjGbtvf/vbo/WKpz3tad0pTnGKbp999hkte8Mb3tC99a1vbekvfelL3WUuc5nuKEc5SnfRi160+8tf/jJaz3GufvWrd/e73/26C1/4wm274ve//313vOMdrzvZyU42WnajG92oO85xjtO96EUvGi0bcrWrXa372Mc+1vbrfjz0oQ9ty3/wgx+0bU3/+Mc/BlvtyLOf/ezhoonsueee7Tj2e9SjHrU76UlP2j34wQ8errYhXv7yl3ef//znh4tnwi1vecvuBCc4QffPf/5z+NdOedjDHtY94QlPGM3bh2d4tKMdrd2DD37wg721w7yIHQ158mEiMRCz45SnPGX7uBW/+tWv2n31wf/vf//blp361Kdu3p3jH//43YlOdKK27NBDD23rPf3pT++++MUvtrT/n/GMZ7T///SnPzVB9p///KdtQzwQPQSY9Szv86QnPal9uO2HcMMRRxzRvelNb+p++tOftuV77LFHW+7/85///C1N+PnvF7/4RZu/9a1v3eY/97nPtfk6T9M3v/nNtuza1752m3/e857X5oec+9znboINl7vc5dq6pz3tadv8gQceONrfWkKLqDStl4tc5CI7HIfQNU/4boY//OEPTayd5zznGf41E3772992n/nMZ4aL182JT3zi7oADDthh2Tvf+c52ze973/t2WB7mQ+VjU9ie5MmHicRAzI6h0EJ5uL7//e83z9F1rnOd7gMf+EBbtvvuu7d1VB2WMOBt6j+TX/7yl01Q3eMe9+gOPvjgtozHpjj88MNHIq4gtAgw6xJcILTe9ra3Na+S5Y961KPaciLtXe96V/evf/2rebj8V6LnyU9+cpu/8Y1v3ObPd77zdde73vXasvKirCW0Xve617X/PvvZz7b5K17xiqPrcgzetP4xCdNLX/rS3S1ucYvuAhe4QDsv7dj8z0PDq/Wtb32ru9vd7tbd6U53auLnkY985OCo/19oneY0p2nz973vfdv8Ax/4wG633XZrou0mN7lJ8wjiQQ96UNuGKHzIQx4y2o/7zUvJ++d/z4IY9oxvdrObdSc5yUm6V73qVe3cee1ufvObd1e96lWbMAYv3w1veMN2vnWevIruGY/hXe9617bsHOc4R3f0ox+9iWAi130n6tyDu9/97m0dXkzrPOUpT2nnJK/97Gc/a//tv//+7fp+97vftfk6jmUltJ7//Oe3+zUU5WE2VL42he1JnnyYSAzE7BgntFQLubeq84idV77ylc1TZVkJLeLBvGq5T33qU00Q1DM573nP2/3973/v9t133/axtcw+iQAff14inqo+PEX+O8tZztLWv8td7tK9+c1v7t7xjneMlvHu3PSmN+0ueclLNiGhaq+OWUKrzvOCF7xgmye0PvnJT7ZlZz3rWduytYTWpS51qfZfVU0SIeVdIsIc1zVX3iN8iBeoYrX8r3/9a/ut++raPvzhD7dztHycp6uElupT95bgMs8Ld8ITnrCl3TO/RGndj0q/5jWv6b73ve+1tHv03ve+t6Wvda1rtecoTQyqQlUNerGLXaw78pGP3I7tv9vf/vbdT37yk5Z2vYTXPe95z+6Pf/xjW+aZEpXEl6q+M5zh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/vO27Mc//nF3n/vcp3vHO97Rlp/vfOdry+95z3u2+X333Xe0vz6HH354+5+ovO51r9vSN7nJTbo73/nOLX2xi11suMlO2W+//dq9+M9//jP8ayEQxuc973mbWCS05YWPfexjo/+POOKI7ilPeUq7vpvf/Oa9LTdHPQvToljckboIreKud71rd+9737tl8Atd6ELtpZk1F7zgBbv999+/ibhvfOMbw7/DDFjGCxvCPFlvnr7Xve7V1ul7c2o7BTofR+nLXe5y3YUvfOGWfv/739/WI87M3/SmN23zCpwnPvGJ27KrX/3qo/0VX/7yl9t/PsDEDRHynOc8Z/T/cY973PY/O3ePe9yjpS3jIYFz6Aut5z3veW35KU95yjbPO9TnGc94Rlt+tatdrc07T/NHP/rRW8GY4DNPFPz+979v6ROe8IRt3V//+tfdSU960raMUMO3vvWt0bEJChCn97///ZsQtdx3AMSXeb9DePP8R/zhBje4QZt/1rOe1eaPecxjtvn6nnzxi19swuwPf/jDaB/OnyeOdwjve9/72jZHOcpRRmLx05/+dPeqV72qefzcQ54uIui5z31u2ycv3Ite9KLmfeJd46376Ec/2tbnvYP9ywO8iXAO1nnBC17Q/e53v2veyE9+8pPt/rknzuGBD3xg8/SVuC2PINzrIx/5yN2tbnWr0bLNUs/CtCgWd6QuQgsymRJUTV7yypzr4ac//Wl7cWsyPw4v1He+851RCWoallXS2dVZxgsbwjxZb54uodUvJPa3I2akea2ICOkrXelK3S1ucYtWCDRfQusOd7hDqzq0jEAaUkLr+Mc/fveIRzyipX20i77QUog9wxnO0OZvfOMbt/+HQuuiF71oO49jHetYbX5nQkttQ21bwqiuEzVPZFmXkDnqUY/axAuh4rx222230XkSRuxyX2jxKDmns5/97G1+KLSI1/IiEicoofWEJzyh3T/HrHO+zW1u04TpZz7zmbbOz372s5FQs4y4veQlLzkSjqZ//etf3f3ud7/27Igc50+8uYa6RveGIJN+wAMe0Lx20mc5y1m6U5/61M0jJ+27RJz5z7nwSl7gAhdo88779Kc/fUsTnNe61rVaWvUg6jwJ8v71R2itk1lf3J577tkyE9exh4eLX/ziLUMrfcg8F7nIRVpGp8atWy+3DOlFO9GJTjQSEo997GPbsoc+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
