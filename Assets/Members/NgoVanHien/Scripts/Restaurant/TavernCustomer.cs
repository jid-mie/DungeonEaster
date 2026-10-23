using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    public enum CustomerState
    {
        Entering,   // đi từ cửa tới bàn
        Waiting,    // đứng chờ tại bàn, hiện icon món + thanh kiên nhẫn
        Eating,     // đã nhận món, đĩa hiện trên bàn
        Leaving     // đi ra cửa (vui vẻ hoặc giận dữ)
    }

    public enum ServeResult
    {
        Served,
        NotWaiting,
        WrongDish,
        DishNotInInventory
    }

    public struct ServeOutcome
    {
        public ServeResult result;
        public int gold;
        public int tip;
        public int reputation;
    }

    /// <summary>
    /// Thực khách: vào quán -> tới bàn trống -> đứng chờ với order icon và patience timer ->
    /// nhận món (trả vàng + danh tiếng, tip nếu phục vụ nhanh) -> ăn -> ra về.
    /// Hết kiên nhẫn: giận dữ bỏ về và trừ danh tiếng (README mục 5).
    /// </summary>
    public class TavernCustomer : MonoBehaviour, IInteractable
    {
        // Cùng công thức với YSortOrder (base 5000, precision 100) để đứng đúng lớp với bàn/người chơi
        private const int SortBase = 5000;
        private const int SortPrecision = 100;

        [SerializeField] private float moveSpeed = 2.6f;
        [SerializeField] private float eatDuration = 4f;

        [Header("Thưởng / phạt (giá trị tạm, chờ cân bằng)")]
        [Tooltip("Còn >= tỉ lệ kiên nhẫn này khi nhận món thì khách boa thêm")]
        [SerializeField, Range(0f, 1f)] private float tipPatienceThreshold = 0.5f;
        [Tooltip("Tiền boa = % giá món")]
        [SerializeField, Range(0f, 1f)] private float tipPercent = 0.2f;
        [SerializeField] private int angryReputationPenalty = 3;

        private CustomerProfile profile;
        private RecipeData order;
        private TavernTable table;
        private TavernShift shift;

        private SpriteRenderer body;
        private OrderBubble bubble;
        private float bodyBaseY;

        private readonly List<Vector3> path = new List<Vector3>();
        private List<Vector3> exitPath;
        private int pathIndex;
        private float patienceLeft;
        private float eatTimer;

        public CustomerState State { get; private set; }
        public CustomerProfile Profile => profile;
        public RecipeData Order => order;
        public TavernTable Table => table;
        public float PatienceRatio => profile == null ? 0f : Mathf.Clamp01(patienceLeft / profile.patience);

        public static TavernCustomer Create(CustomerProfile profile, RecipeData order, TavernTable table,
            List<Vector3> entryPath, TavernShift shift, float visualScale)
        {
            var go = new GameObject($"Customer_{profile.displayName}");
            go.transform.position = entryPath.Count > 0 ? entryPath[0] : table.SeatPosition;
            var customer = go.AddComponent<TavernCustomer>();
            customer.Setup(profile, order, table, entryPath, shift, visualScale);
            return customer;
        }

        private void Setup(CustomerProfile p, RecipeData o, TavernTable t, List<Vector3> entryPath, TavernShift s, float visualScale)
        {
            profile = p;
            order = o;
            table = t;
            shift = s;
            patienceLeft = p.patience;

            int sortingLayer = t.TableRenderer != null ? t.TableRenderer.sortingLayerID : TavernSprites.SortingLayerOrDefault("Entities");

            // Body con, dịch lên nửa chiều cao để transform gốc = chân nhân vật (pivot sprite đang ở giữa)
            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(transform, false);
            body = bodyGo.AddComponent<SpriteRenderer>();
            body.sprite = p.idleSprite;
            body.sortingLayerID = sortingLayer;
            bodyGo.transform.localScale = Vector3.one * visualScale;
            float bodyHeight = p.idleSprite != null ? p.idleSprite.bounds.size.y * visualScale : 1.5f;
            bodyBaseY = p.idleSprite != null ? -p.idleSprite.bounds.min.y * visualScale : 0.75f;
            bodyGo.transform.localPosition = new Vector3(0f, bodyBaseY, 0f);

            var bubbleGo = new GameObject("OrderBubble");
            bubbleGo.transform.SetParent(transform, false);
            bubbleGo.transform.localPosition = new Vector3(0f, bodyHeight + 0.6f, 0f);
            bubbleGo.transform.localScale = Vector3.one * 0.85f;
            bubble = bubbleGo.AddComponent<OrderBubble>();
            bubble.Build(TavernSprites.SortingLayerOrDefault("Overhead"), 100);

            // cửa -> lối đi chung -> cạnh bàn -> chỗ ngồi; ra về đi ngược lại
            var route = new List<Vector3>(entryPath);
            t.AppendApproachPath(route);
            exitPath = new List<Vector3>(route);
            exitPath.Reverse();

            path.Clear();
            path.AddRange(route);
            path.Add(t.SeatPosition);
            pathIndex = 0;

            t.Reserve(this);
            State = CustomerState.Entering;
            UpdateSorting();

            // Collider trigger ở layer Interactable -> ChiefPlayer bấm F khi quay mặt vào khách
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) gameObject.layer = layer;
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            float width = p.idleSprite != null ? p.idleSprite.bounds.size.x * visualScale * 0.6f : 0.6f;
            col.size = new Vector2(width, bodyHeight * 0.8f);
            col.offset = new Vector2(0f, bodyHeight * 0.45f);
        }

        public void Interact(ChiefPlayer player)
        {
            if (table != null && State != CustomerState.Leaving) table.Interact(player);
        }

        public string GetInteractPrompt()
        {
            return table != null ? table.GetInteractPrompt() : "";
        }

        private void Update()
        {
            switch (State)
            {
                case CustomerState.Entering:
                    if (FollowPath()) Sit();
                    break;

                case CustomerState.Waiting:
                    patienceLeft -= Time.deltaTime;
                    bubble.SetPatience(PatienceRatio);
                    if (patienceLeft <= 0f) LeaveAngry();
                    break;

                case CustomerState.Eating:
                    eatTimer -= Time.deltaTime;
                    if (eatTimer <= 0f)
                    {
                        table.HideDish();
                        StartLeaving();
                    }
                    break;

                case CustomerState.Leaving:
                    if (FollowPath())
                    {
                        if (shift != null) shift.NotifyCustomerGone(this);
                        Destroy(gameObject);
                    }
                    break;
            }
        }

        private void LateUpdate()
        {
            UpdateSorting();
        }

        /// <summary>Đi theo path, trả về true khi tới điểm cuối.</summary>
        private bool FollowPath()
        {
            if (pathIndex >= path.Count) return true;

            Vector3 target = path[pathIndex];
            Vector3 pos = transform.position;
            Vector3 next = Vector3.MoveTowards(pos, target, moveSpeed * Time.deltaTime);
            transform.position = next;

            float dx = target.x - pos.x;
            if (Mathf.Abs(dx) > 0.01f) body.flipX = dx < 0f;

            // nhún nhẹ khi đi bộ
            body.transform.localPosition = new Vector3(0f, bodyBaseY + Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.06f, 0f);

            if ((next - target).sqrMagnitude < 0.0004f)
            {
                pathIndex++;
                if (pathIndex >= path.Count)
                {
                    body.transform.localPosition = new Vector3(0f, bodyBaseY, 0f);
                    return true;
                }
            }
            return false;
        }

        private void Sit()
        {
            State = CustomerState.Waiting;
            patienceLeft = profile.patience;
            body.flipX = false;
            bubble.Show(order.Icon);
            bubble.SetPatience(1f);
        }

        /// <param name="fromSlot">Ô hòm đồ lấy món ra (-1: lấy ở bất kỳ ô nào có món).</param>
        public ServeOutcome TryServe(ItemData dish, int fromSlot = -1)
        {
            var outcome = new ServeOutcome();
            if (State != CustomerState.Waiting)
            {
                outcome.result = ServeResult.NotWaiting;
                return outcome;
            }
            if (dish != order.dish)
            {
                outcome.result = ServeResult.WrongDish;
                return outcome;
            }
            var inventory = PlayerInventory.Instance;
            bool removed = inventory != null && (fromSlot >= 0 ? inventory.TryRemoveAt(fromSlot, 1) : inventory.TryRemove(dish, 1));
            if (!removed)
            {
                outcome.result = ServeResult.DishNotInInventory;
                return outcome;
            }

            outcome.result = ServeResult.Served;
            outcome.gold = order.goldPrice;
            outcome.tip = PatienceRatio >= tipPatienceThreshold ? Mathf.RoundToInt(order.goldPrice * tipPercent) : 0;
            outcome.reputation = order.reputation;

            if (PlayerWallet.Instance != null)
            {
                PlayerWallet.Instance.AddGold(outcome.gold + outcome.tip);
                PlayerWallet.Instance.AddReputation(outcome.reputation);
            }

            var sound = DungeonEater.Core.SoundManager.Instance;
            if (sound != null)
            {
                sound.PlayServeSuccess();
                sound.PlayCoin();
            }

            State = CustomerState.Eating;
            eatTimer = eatDuration;
            bubble.Hide();
            table.ShowDish(dish);
            if (profile.eatingSprite != null) body.sprite = profile.eatingSprite;

            string tipText = outcome.tip > 0 ? $" (+{outcome.tip} boa)" : "";
            FloatingText.Spawn(bubble.transform.position, $"+{outcome.gold + outcome.tip} vàng{tipText}\n+{outcome.reputation} danh tiếng",
                new Color(1f, 0.85f, 0.3f));

            if (shift != null) shift.NotifyServed(this, outcome);
            return outcome;
        }

        private void LeaveAngry()
        {
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddReputation(-angryReputationPenalty);

            var sound = DungeonEater.Core.SoundManager.Instance;
            if (sound != null) sound.PlayCustomerAngry();

            body.color = new Color(1f, 0.65f, 0.6f);
            FloatingText.Spawn(bubble.transform.position, $"Bực mình!\n-{angryReputationPenalty} danh tiếng", new Color(1f, 0.4f, 0.35f));
            if (shift != null) shift.NotifyAngry(this, angryReputationPenalty);
            StartLeaving();
        }

        private void StartLeaving()
        {
            State = CustomerState.Leaving;
            bubble.Hide();
            table.Release(this);

            path.Clear();
            path.AddRange(exitPath);
            pathIndex = 0;
        }

        private void UpdateSorting()
        {
            if (body == null) return;
            body.sortingOrder = SortBase + Mathf.RoundToInt(-transform.position.y * SortPrecision);
        }

        private void OnDestroy()
        {
            if (table != null) table.Release(this);
        }
    }
}
