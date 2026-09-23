using UnityEngine;
using UnityEngine.UI;
using DungeonEater.Restaurant;

namespace DungeonEater.UI
{
    public class CookingUI : MonoBehaviour
    {
        [SerializeField] private Transform recipeCardContainer;
        [SerializeField] private GameObject recipeCardPrefab;

        private void OnEnable()
        {
            RefreshRecipeList();
        }

        public void RefreshRecipeList()
        {
            if (recipeCardContainer == null || CookingSystem.Instance == null) return;

            // Xóa các card cũ
            foreach (Transform child in recipeCardContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo các card công thức
            foreach (var recipe in CookingSystem.Instance.Recipes)
            {
                if (recipeCardPrefab != null)
                {
                    var card = Instantiate(recipeCardPrefab, recipeCardContainer);
                    var titleText = card.transform.Find("Title")?.GetComponent<Text>();
                    var descText = card.transform.Find("Desc")?.GetComponent<Text>();
                    var priceText = card.transform.Find("Price")?.GetComponent<Text>();
                    var cookBtn = card.transform.Find("CookButton")?.GetComponent<Button>();

                    if (titleText) titleText.text = recipe.name;
                    if (descText) descText.text = recipe.description;
                    if (priceText) priceText.text = $"+{recipe.goldPrice} Gold";

                    if (cookBtn != null)
                    {
                        cookBtn.interactable = CookingSystem.Instance.CanCook(recipe);
                        cookBtn.onClick.AddListener(() =>
                        {
                            CookingSystem.Instance.Cook(recipe.id);
                            RefreshRecipeList();
                        });
                    }
                }
            }
        }
    }
}

