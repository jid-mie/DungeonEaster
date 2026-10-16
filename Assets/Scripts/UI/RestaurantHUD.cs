using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonEater.Core;
using DungeonEater.Dungeon;
using DungeonEater.NPC;
using DungeonEater.Restaurant;

namespace DungeonEater.UI
{
    /// <summary>
    /// HUD quán ăn: trạng thái ca phục vụ, kho nguyên liệu / món sẵn sàng, bong bóng order
    /// trên đầu khách (icon món + thanh kiên nhẫn), bưng món bằng [F] và các nút điều hướng.
    /// </summary>
    public class RestaurantHUD : MonoBehaviour
    {
        [SerializeField] private float serveRange = 1.8f;

        private CookingSystem cookingSystem => CookingSystem.Instance;
        private CustomerSpawner spawner;
        private KitchenStation station;
        private DiningTable[] tables;
        private readonly List<CustomerAI> customers = new List<CustomerAI>();
        private float refreshTimer;

        private string toast;
        private Color toastColor;
        private float toastTime;

        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle smallStyle;
        private Texture2D panelTex;
        private Texture2D pixel;

        private void Start()
        {
            spawner = FindObjectOfType<CustomerSpawner>();
            station = FindObjectOfType<KitchenStation>();
            tables = FindObjectsOfType<DiningTable>();

            CustomerAI.OnServed += HandleServed;
            CustomerAI.OnLeftAngry += HandleLeftAngry;
        }

        private void OnDestroy()
        {
            CustomerAI.OnServed -= HandleServed;
            CustomerAI.OnLeftAngry -= HandleLeftAngry;
        }

        private void HandleServed(CustomerAI customer, int gold, int rep, DishQuality quality)
        {
            Color c = quality == DishQuality.Perfect ? new Color(0.55f, 1f, 0.45f) :
                      quality == DishQuality.Burnt ? new Color(1f, 0.6f, 0.4f) : new Color(1f, 0.88f, 0.4f);
            ShowToast($"Phục vụ {customer.WantedDishName} ({CookingSystem.QualityName(quality)})   +{gold} vàng   +{rep} danh tiếng", c);
        }

        private void HandleLeftAngry(CustomerAI customer)
        {
            ShowToast("Khách chờ quá lâu đã bỏ về!   -3 danh tiếng", new Color(1f, 0.45f, 0.4f));
        }

        public void ShowToast(string message, Color color)
        {
            toast = message;
            toastColor = color;
            toastTime = 2.6f;
        }

        private void Update()
        {
            if (toastTime > 0f) toastTime -= Time.deltaTime;

            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = 0.25f;
                customers.Clear();
                customers.AddRange(FindObjectsOfType<CustomerAI>());
            }

            if (KitchenScreen.IsOpen) return;

            if (Input.GetKeyDown(KeyCode.F) && (station == null || !station.PlayerInRange))
            {
                TryServeNearestTable();
            }
        }

        private void TryServeNearestTable()
        {
            var player = PlayerController.Instance;
            if (player == null || tables == null) return;

            DiningTable best = null;
            float bestDist = serveRange;
            foreach (var table in tables)
            {
                if (table == null || !table.IsOccupied || table.CurrentCustomer.IsEating) continue;
                float d = Vector2.Distance(player.transform.position, table.transform.position);
                if (d <= bestDist)
                {
                    best = table;
                    bestDist = d;
                }
            }
            if (best == null) return;

            var customer = best.CurrentCustomer;
            if (!customer.TryServe())
            {
                ShowToast($"Chưa có món \"{customer.WantedDishName}\" — vào bếp nấu trước!", new Color(1f, 0.7f, 0.4f));
            }
        }

        private void StartShift()
        {
            if (spawner == null) return;
            spawner.StartShift();
            SoundManager.Instance?.PlayServeSuccess();
            ShowToast("Quán mở cửa! Khách đang tới...", new Color(1f, 0.88f, 0.4f));
        }

        // ================================================================ GUI

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelTex = MakeTex(new Color(0.12f, 0.07f, 0.04f, 0.86f));
            pixel = MakeTex(Color.white);

            panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 10, 10) };
            panelStyle.normal.background = panelTex;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
            labelStyle.normal.textColor = new Color(0.95f, 0.88f, 0.75f);
            titleStyle = new GUIStyle(labelStyle) { fontSize = 17, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = new Color(1f, 0.82f, 0.4f);
            smallStyle = new GUIStyle(labelStyle) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, wordWrap = true };
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void OnGUI()
        {
            if (KitchenScreen.IsOpen) return;
            EnsureStyles();

            DrawOrderBubbles();
            DrawStatusPanel();
            DrawStockPanel();
            DrawToast();
            DrawBottomBar();
        }

        private void DrawStatusPanel()
        {
            GUILayout.BeginArea(new Rect(15, 15, 390, 150), panelStyle);
            GUILayout.Label("QUÁN ĂN DUNGEON EATER — LÀNG DUN", titleStyle);
            int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
            int rep = GameManager.Instance != null ? GameManager.Instance.Reputation : 0;
            GUILayout.Label($"Vàng: <color=#FFD45A><b>{gold}</b></color>      Danh tiếng: <color=#8FE3FF><b>{rep}</b></color>", labelStyle);

            if (spawner != null && spawner.IsShiftActive)
            {
                int t = Mathf.CeilToInt(spawner.RemainingShiftTime);
                GUILayout.Label($"Ca phục vụ: <b>{t / 60:00}:{t % 60:00}</b>   •   Khách: <b>{spawner.CustomersSpawned}/{spawner.MaxCustomers}</b>", labelStyle);
            }
            else
            {
                GUILayout.Label("<color=#FFB070>Quán đang đóng cửa</color> — chuẩn bị món rồi mở cửa", labelStyle);
            }
            GUILayout.Label("<color=#BBA890>WASD di chuyển • [F] ở lò: vào bếp • [F] cạnh bàn: bưng món</color>", labelStyle);
            GUILayout.EndArea();
        }

        private void DrawStockPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 285, 15, 270, 196), panelStyle);
            GUILayout.Label("NGUYÊN LIỆU", titleStyle);
            var inv = InventoryManager.Instance;
            if (inv != null)
            {
                GUILayout.Label($"Thịt quái: <b>{inv.GetItemCount(IngredientType.MonsterMeat)}</b>    Nấm hang: <b>{inv.GetItemCount(IngredientType.CaveMushroom)}</b>", labelStyle);
                GUILayout.Label($"Lõi bọ nổ: <b>{inv.GetItemCount(IngredientType.BombCore)}</b>    Sườn Vua Thú: <b>{inv.GetItemCount(IngredientType.BossRib)}</b>", labelStyle);
            }
            GUILayout.Label("MÓN SẴN SÀNG", titleStyle);
            GUILayout.BeginHorizontal();
            if (cookingSystem != null)
            {
                foreach (var r in cookingSystem.Recipes)
                {
                    var sprite = KitchenUIKit.DishSprite(r.id);
                    GUILayout.BeginVertical(GUILayout.Width(56));
                    Rect rect = GUILayoutUtility.GetRect(48, 42);
                    if (sprite != null) GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit);
                    GUILayout.Label($"<b>{cookingSystem.GetPreparedCount(r.id)}</b>", smallStyle);
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawOrderBubbles()
        {
            var cam = Camera.main;
            if (cam == null || cookingSystem == null) return;

            foreach (var c in customers)
            {
                if (c == null) continue;
                Vector3 sp = cam.WorldToScreenPoint(c.transform.position + Vector3.up * 0.95f);
                if (sp.z < 0f) continue;
                float x = sp.x, y = Screen.height - sp.y;

                if (c.IsEating)
                {
                    GUI.Label(new Rect(x - 50, y - 12, 100, 24), "<color=#9CFF8A><b>Ngon quá!</b></color>", smallStyle);
                    continue;
                }

                var bubble = new Rect(x - 34, y - 58, 68, 66);
                GUI.color = new Color(0.98f, 0.92f, 0.78f, 0.95f);
                GUI.DrawTexture(bubble, pixel);
                GUI.color = new Color(0.25f, 0.14f, 0.08f);
                GUI.DrawTexture(new Rect(bubble.x, bubble.yMax - 2, bubble.width, 2), pixel);
                GUI.color = Color.white;

                var dish = KitchenUIKit.DishSprite(c.WantedDishId);
                if (dish != null) GUI.DrawTexture(new Rect(bubble.x + 6, bubble.y + 4, 56, 46), dish.texture, ScaleMode.ScaleToFit);

                // Thanh kiên nhẫn: xanh -> vàng -> đỏ
                float p = c.PatienceRatio;
                var barBg = new Rect(bubble.x + 6, bubble.yMax - 13, 56, 8);
                GUI.color = new Color(0.15f, 0.08f, 0.05f);
                GUI.DrawTexture(barBg, pixel);
                GUI.color = p > 0.5f ? Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.4f, 0.9f, 0.3f), (p - 0.5f) * 2f)
                                     : Color.Lerp(new Color(0.95f, 0.25f, 0.2f), new Color(1f, 0.85f, 0.2f), p * 2f);
                if (p < 0.25f && Mathf.Repeat(Time.time * 4f, 1f) > 0.5f) GUI.color = Color.white;
                GUI.DrawTexture(new Rect(barBg.x + 1, barBg.y + 1, (barBg.width - 2) * p, barBg.height - 2), pixel);
                GUI.color = Color.white;

                if (cookingSystem.GetPreparedCount(c.WantedDishId) > 0)
                {
                    GUI.Label(new Rect(x - 60, y - 80, 120, 20), "<color=#9CFF8A><b>Có sẵn — bấm F</b></color>", smallStyle);
                }
            }
        }

        private void DrawToast()
        {
            if (toastTime <= 0f || string.IsNullOrEmpty(toast)) return;
            var style = new GUIStyle(titleStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            style.normal.textColor = new Color(toastColor.r, toastColor.g, toastColor.b, Mathf.Clamp01(toastTime / 0.5f));
            var rect = new Rect((Screen.width - 640) / 2f, 70, 640, 40);
            GUI.color = new Color(1, 1, 1, Mathf.Clamp01(toastTime / 0.5f));
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.color = Color.white;
            GUI.Label(rect, toast, style);
        }

        private void DrawBottomBar()
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2f - 340, Screen.height - 82, 680, 70), panelStyle);
            GUILayout.BeginHorizontal();

            GUI.color = new Color(1f, 0.6f, 0.3f);
            if (GUILayout.Button("VÀO BẾP\n(Nấu món)", buttonStyle, GUILayout.Height(48)))
            {
                KitchenScreen.Open();
            }

            bool shiftActive = spawner != null && spawner.IsShiftActive;
            GUI.enabled = spawner != null && !shiftActive;
            GUI.color = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button(shiftActive ? "ĐANG PHỤC VỤ\n(Ca 10 phút)" : "MỞ CỬA\n(Đón khách)", buttonStyle, GUILayout.Height(48)))
            {
                StartShift();
            }
            GUI.enabled = true;

            GUI.color = new Color(0.6f, 0.9f, 0.6f);
            if (GUILayout.Button("RA LÀNG\n(Làng Dun)", buttonStyle, GUILayout.Height(48)))
            {
                SceneManager.LoadScene("VillageLevel");
            }

            GUI.color = new Color(1f, 0.45f, 0.35f);
            if (GUILayout.Button("HẦM NGỤC\n(Săn nguyên liệu)", buttonStyle, GUILayout.Height(48)))
            {
                SceneManager.LoadScene("DungeonLevel");
            }

            GUI.color = Color.white;
            if (GUILayout.Button("MENU CHÍNH", buttonStyle, GUILayout.Height(48)))
            {
                SceneManager.LoadScene("TitleScreen");
            }

            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            GUILayout.EndArea();
        }
    }
}
