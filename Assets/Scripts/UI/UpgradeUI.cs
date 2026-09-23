using UnityEngine;
using UnityEngine.UI;
using DungeonEater.Upgrades;
using DungeonEater.Core;

namespace DungeonEater.UI
{
    public class UpgradeUI : MonoBehaviour
    {
        [SerializeField] private Transform upgradeContainer;
        [SerializeField] private GameObject upgradeCardPrefab;

        private void OnEnable()
        {
            RefreshUpgradeList();
        }

        public void RefreshUpgradeList()
        {
            if (upgradeContainer == null || UpgradeManager.Instance == null) return;

            foreach (Transform child in upgradeContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in UpgradeManager.Instance.Upgrades)
            {
                if (upgradeCardPrefab != null)
                {
                    var card = Instantiate(upgradeCardPrefab, upgradeContainer);
                    var titleText = card.transform.Find("Title")?.GetComponent<Text>();
                    var descText = card.transform.Find("Desc")?.GetComponent<Text>();
                    var priceText = card.transform.Find("Price")?.GetComponent<Text>();
                    var buyBtn = card.transform.Find("BuyButton")?.GetComponent<Button>();

                    if (titleText) titleText.text = $"{item.title} (Cấp {item.currentLevel}/{item.maxLevel})";
                    if (descText) descText.text = item.description;

                    bool isMaxed = item.IsMaxed();
                    int price = item.GetNextPrice();

                    if (priceText) priceText.text = isMaxed ? "TỐI ĐA" : $"{price} Vàng";

                    if (buyBtn != null)
                    {
                        bool canAfford = GameManager.Instance != null && GameManager.Instance.Gold >= price && !isMaxed;
                        buyBtn.interactable = canAfford;
                        buyBtn.onClick.AddListener(() =>
                        {
                            UpgradeManager.Instance.PurchaseUpgrade(item.id);
                            RefreshUpgradeList();
                        });
                    }
                }
            }
        }
    }
}

