using UnityEngine;
using System;
using System.Collections.Generic;
using DungeonEater.Core;
using DungeonEater.Dungeon;
using DungeonEater.Restaurant;

namespace DungeonEater.Upgrades
{
    [Serializable]
    public class UpgradeItem
    {
        public string id;
        public string title;
        public string description;
        public int currentLevel = 1;
        public int maxLevel = 5;
        public int basePrice = 50;

        public int GetNextPrice() => basePrice * currentLevel;
        public bool IsMaxed() => currentLevel >= maxLevel;
    }

    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrades List")]
        [SerializeField] private List<UpgradeItem> upgrades = new List<UpgradeItem>();
        public List<UpgradeItem> Upgrades => upgrades;

        public event Action OnUpgradePurchased;

        private void Awake()
        {
            Instance = this;
            InitializeDefaultUpgrades();
        }

        private void InitializeDefaultUpgrades()
        {
            upgrades.Clear();

            upgrades.Add(new UpgradeItem
            {
                id = "sword",
                title = "Nâng cấp Kiếm Răng Quái",
                description = "Tăng +6 sát thương cơ bản và mở rộng tầm chém lốc xoáy.",
                currentLevel = 1,
                maxLevel = 5,
                basePrice = 50
            });

            upgrades.Add(new UpgradeItem
            {
                id = "bow",
                title = "Nâng cấp Cung Gân Thú",
                description = "Tăng +5 sát thương cung và tốc độ bay của mũi tên.",
                currentLevel = 1,
                maxLevel = 5,
                basePrice = 50
            });

            upgrades.Add(new UpgradeItem
            {
                id = "bag",
                title = "Mở rộng Túi Da Thợ Săn",
                description = "Tăng thêm +4 ô chứa đồ nguyên liệu trong Dungeon.",
                currentLevel = 1,
                maxLevel = 4,
                basePrice = 60
            });

            upgrades.Add(new UpgradeItem
            {
                id = "tables",
                title = "Kê thêm Bàn Ghế Gỗ Sồi",
                description = "Tăng thêm số lượng bàn ăn đón tiếp nhiều khách hơn.",
                currentLevel = 1,
                maxLevel = 3,
                basePrice = 100
            });
        }

        public bool PurchaseUpgrade(string upgradeId)
        {
            var item = upgrades.Find(u => u.id == upgradeId);
            if (item == null || item.IsMaxed()) return false;

            int price = item.GetNextPrice();
            if (GameManager.Instance == null || !GameManager.Instance.SpendGold(price))
            {
                Debug.LogWarning($"[UpgradeManager] Không đủ vàng mua {item.title}! Cần {price} Vàng.");
                return false;
            }

            item.currentLevel++;
            ApplyUpgradeEffect(item.id, item.currentLevel);

            SoundManager.Instance?.PlayCoin();
            Debug.Log($"🎉 [UpgradeManager] Nâng cấp thành công {item.title} lên Cấp {item.currentLevel}!");
            OnUpgradePurchased?.Invoke();
            return true;
        }

        private void ApplyUpgradeEffect(string id, int level)
        {
            switch (id)
            {
                case "sword":
                    var weaponSys = FindObjectOfType<WeaponSystem>();
                    weaponSys?.AddSwordDamageBonus(6f);
                    break;

                case "bow":
                    var weaponSysBow = FindObjectOfType<WeaponSystem>();
                    weaponSysBow?.AddBowDamageBonus(5f);
                    break;

                case "bag":
                    InventoryManager.Instance?.IncreaseCapacity(4);
                    break;

                case "tables":
                    Debug.Log("[Upgrade] Mở thêm bàn ăn cho nhà hàng!");
                    break;
            }
        }
    }
}

