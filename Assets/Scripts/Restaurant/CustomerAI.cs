using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Restaurant
{
    public class CustomerAI : MonoBehaviour
    {
        [Header("Order Info")]
        [SerializeField] private string wantedDishId = "skewer";
        [SerializeField] private string wantedDishName = "Thịt xiên nướng dã thú";

        [Header("Patience Settings")]
        [SerializeField] private float maxPatience = 25f; // 25 giây chờ đợi
        private float currentPatience;

        [Header("State")]
        private bool isEating = false;
        private float eatingDuration = 3f;
        private DiningTable assignedTable;

        public string WantedDishId => wantedDishId;
        public string WantedDishName => wantedDishName;
        public float PatienceRatio => Mathf.Clamp01(currentPatience / maxPatience);
        public bool IsEating => isEating;

        public void Initialize(DiningTable table, Recipe orderRecipe, float extraPatience = 0f)
        {
            assignedTable = table;
            wantedDishId = orderRecipe.id;
            wantedDishName = orderRecipe.name;
            maxPatience += extraPatience;
            currentPatience = maxPatience;
            isEating = false;

            table.AssignCustomer(this);
            Debug.Log($"👥 [Khách hàng] Ngồi vào Bàn {table.TableId}, gọi món: {wantedDishName}");
        }

        private void Update()
        {
            if (isEating)
            {
                eatingDuration -= Time.deltaTime;
                if (eatingDuration <= 0f)
                {
                    // Ăn xong vui vẻ ra về
                    FinishAndLeave();
                }
                return;
            }

            // Giảm dần thanh kiên nhẫn
            currentPatience -= Time.deltaTime;

            if (currentPatience <= 0f)
            {
                // Khách đợi quá lâu giận dữ bỏ về!
                LeaveAngry();
            }
        }

        /// <summary>Phát ra khi phục vụ thành công: (khách, vàng, danh tiếng, chất lượng món).</summary>
        public static event System.Action<CustomerAI, int, int, DishQuality> OnServed;
        /// <summary>Phát ra khi khách hết kiên nhẫn bỏ về.</summary>
        public static event System.Action<CustomerAI> OnLeftAngry;

        public bool TryServe()
        {
            if (isEating) return false;

            if (CookingSystem.Instance != null && CookingSystem.Instance.ConsumePreparedDish(wantedDishId, out DishQuality quality))
            {
                var recipe = CookingSystem.Instance.GetRecipe(wantedDishId);
                isEating = true;

                assignedTable.PlaceDish(wantedDishId);
                SoundManager.Instance?.PlayServeSuccess();
                SoundManager.Instance?.PlayCoin();

                int basePrice = recipe != null ? recipe.goldPrice : 25;
                int baseRep = recipe != null ? recipe.repBonus : 5;

                // Giá theo chất lượng món + tiền boa nếu phục vụ nhanh (còn nhiều kiên nhẫn)
                int tip = Mathf.RoundToInt(basePrice * 0.2f * PatienceRatio);
                int goldEarned = Mathf.RoundToInt(basePrice * CookingSystem.GoldMultiplier(quality)) + tip;
                int repEarned = Mathf.Max(1, Mathf.RoundToInt(baseRep * CookingSystem.RepMultiplier(quality)));

                GameManager.Instance?.AddGold(goldEarned);
                GameManager.Instance?.AddReputation(repEarned);
                OnServed?.Invoke(this, goldEarned, repEarned, quality);

                Debug.Log($"✨ [Phục vụ] Bàn {assignedTable.TableId} đã nhận món {wantedDishName} ({CookingSystem.QualityName(quality)})! Thu nhập +{goldEarned} Gold (boa {tip}), +{repEarned} Danh tiếng.");
                return true;
            }

            Debug.LogWarning($"⚠️ [Phục vụ] Chưa có món {wantedDishName} sẵn sàng! Hãy vào bếp nấu trước.");
            return false;
        }

        private void FinishAndLeave()
        {
            Debug.Log($"❤ [Khách hàng] Rất hài lòng với bữa ăn và rời quán.");
            assignedTable.ClearTable();
            Destroy(gameObject);
        }

        private void LeaveAngry()
        {
            if (assignedTable == null)
            {
                Destroy(gameObject);
                return;
            }

            Debug.LogWarning($"💢 [Khách hàng] Quá thất vọng vì đợi lâu và bực tức bỏ về! (-3 Danh tiếng)");
            SoundManager.Instance?.PlayCustomerAngry();
            GameManager.Instance?.ReduceReputation(3);
            OnLeftAngry?.Invoke(this);

            assignedTable.ClearTable();
            Destroy(gameObject);
        }
    }
}

