# Data Core JSON Specification

## Phạm vi

`NguyenMinhHiep` chỉ sở hữu dữ liệu lõi và API dữ liệu. Không có Canvas, HUD, inventory UI dùng chung, scene bootstrap hoặc Editor tool tự động chèn object vào scene.

## Nguồn dữ liệu

- File chỉnh sửa chính: `Assets/Members/NguyenMinhHiep/Data/items_catalog.json`.
- Bản đóng gói cho runtime: `Assets/Members/NguyenMinhHiep/Resources/Data/items_catalog.json`.
- Hai file phải giống nhau. `GameItemDatabase` đọc bản Data trước, sau đó mới fallback sang Resources.

## Định nghĩa item

Mỗi item có `id` ổn định và các thuộc tính: `aliases`, `displayName`, `category`, `subCategory`, `rarity`, `floor`, `dropSource`, `maxStack`, `buyPrice`, `sellPrice`, thuộc tính hiệu ứng (`healAmount`, `damageAmount`, `effectRadius`, `effectDuration`), `unlockCondition`, `iconPath`, `fallbackColorHex`, `description` và `recipeIngredients`.

ID dùng trong gameplay phải là ID chuẩn trong JSON. Alias chỉ dùng để tương thích khi đọc dữ liệu cũ.

## API đọc dữ liệu

```csharp
GameItemDatabase.GetItem("ing_glow_mushroom");
GameItemDatabase.GetItemsByCategory("Dish");
GameItemDatabase.GetWeapon("weapon_sword");
GameItemDatabase.NormalizeItemId("cave_mushroom");
```

## Quy tắc tích hợp

- Gameplay gọi API dữ liệu; không tạo ScriptableObject item trùng với catalog.
- UI của từng thành viên tự sở hữu và tự lắng nghe event cần thiết.
- Không gọi hàm tạo hub hoặc setup scene từ backend.
- `SharedGameDataManager` giữ state/API JSON nhưng không tự bind, polling hoặc sửa object thuộc scene khác.
