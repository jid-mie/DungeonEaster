using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonEater.TranHuuHau.Common;

namespace DungeonEater.TranHuuHau.Data
{
    [Serializable]
    public class VillageItemData
    {
        public string id;
        public string displayName;
        public string category;
        [TextArea] public string description;
        public int buyPrice;
        public int sellPrice;
        public int maxStack = 5; // Mặc định 5 stack theo GDD mục 3.2.1
        public Sprite icon;

        public VillageItemData() { }

        public VillageItemData(string id, string displayName, string category, string description, int buyPrice, int sellPrice, int maxStack, Sprite icon = null)
        {
            this.id = id;
            this.displayName = displayName;
            this.category = category;
            this.description = description;
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
            this.maxStack = maxStack;
            this.icon = icon;
        }

        // ================= CATALOG CÁC VẬT PHẨM MẪU CHUẨN GDD =================

        private static List<VillageItemData> shopItems;
        private static Dictionary<string, VillageItemData> itemDatabase;

        public static List<VillageItemData> GetShopCatalog()
        {
            if (shopItems == null)
            {
                shopItems = new List<VillageItemData>
                {
                    new VillageItemData(
                        "potion_heal",
                        "Bình Máu Hồi Phục",
                        "Hồi phục",
                        "Hồi ngay lập tức 20 HP. Thời gian uống 0.3s. Vật phẩm thiết yếu khi xuống hầm ngục.",
                        25, 15, 5, HauUITheme.PotionIcon
                    ),
                    new VillageItemData(
                        "grenade_explo",
                        "Lựu Đạn Nổ",
                        "Sát thương AoE",
                        "Ném tối đa 7m. Gây 50 Dame vật lý diện rộng bán kính 2.5m, đẩy lùi quái nhỏ.",
                        40, 25, 5, HauUITheme.BombIcon
                    ),
                    new VillageItemData(
                        "grenade_ice",
                        "Lựu Đạn Băng",
                        "Khống chế",
                        "Phát tán khí lạnh bán kính 2.5m. Gây 15 Dame & đóng băng quái trong 2.0s.",
                        50, 30, 5, HauUITheme.IceIcon
                    ),
                    new VillageItemData(
                        "grenade_poison",
                        "Lựu Đạn Độc",
                        "DoT & Làm chậm",
                        "Tạo khói độc bán kính 3m trong 4s. Làm chậm quái 40%, gây 8 Dame/0.5s.",
                        45, 25, 5, HauUITheme.PoisonIcon
                    ),
                    new VillageItemData(
                        "mat_mushroom",
                        "Nấm Hang Phát Quang",
                        "Nguyên liệu",
                        "Nguyên liệu Tầng 1: Tỏa sáng xanh dịu, vị ngọt giòn thanh đạm, dùng nấu xúp.",
                        15, 10, 5, HauUITheme.MushroomIcon
                    ),
                    new VillageItemData(
                        "mat_bone",
                        "Mẩu Xương Vụn",
                        "Nguyên liệu",
                        "Nguyên liệu Tầng 1, 2: Rơi từ Skeleton. Đập dập ninh lấy tủy hầm xúp bồi bổ.",
                        15, 10, 5, HauUITheme.BoneIcon
                    )
                };
            }
            return shopItems;
        }

        public static VillageItemData GetItem(string id)
        {
            if (itemDatabase == null)
            {
                itemDatabase = new Dictionary<string, VillageItemData>();
                foreach (var item in GetShopCatalog())
                {
                    itemDatabase[item.id] = item;
                }

                // Thêm các item khác không bán ở shop nhưng có thể nhặt/rơi từ dungeon
                itemDatabase["mat_slime"] = new VillageItemData(
                    "mat_slime",
                    "Chất Nhầy Dẻo",
                    "Nguyên liệu",
                    "Rơi từ Slime thường. Dai, đàn hồi, dùng làm chất tạo đông món Thạch Slime.",
                    20, 12, 5, HauUITheme.PoisonIcon
                );
                itemDatabase["mat_snake"] = new VillageItemData(
                    "mat_snake",
                    "Thịt Nạc Rắn Khổng Lồ",
                    "Nguyên liệu Boss",
                    "Rơi từ Boss Rắn Tầng 1. Thớ thịt dày chắc thơm nồng, dùng chế biến Thịt nướng Độc mãng xà.",
                    80, 50, 5, HauUITheme.BombIcon
                );
            }

            return itemDatabase.ContainsKey(id) ? itemDatabase[id] : null;
        }
    }
}
