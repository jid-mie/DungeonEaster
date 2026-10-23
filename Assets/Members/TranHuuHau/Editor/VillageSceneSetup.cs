using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DungeonEater.TranHuuHau.Data;
using DungeonEater.TranHuuHau.Inventory;
using DungeonEater.TranHuuHau.NPC;
using DungeonEater.TranHuuHau.Player;
using DungeonEater.TranHuuHau.UI;

namespace DungeonEater.TranHuuHau.EditorTools
{
    /// <summary>
    /// Editor Tool dựng Scene Ngôi Làng (Village_Hau.unity) cho Trần Hữu Hậu:
    /// 1. Copy map Village chất lượng cao từ Hải.
    /// 2. Bố trí 10 NPC theo đúng báo cáo GDD:
    ///    - Trưởng làng Godwin
    ///    - Gia đình 1: Sam, Nana, Leaf
    ///    - Gia đình 2: Gam, Ana, Vincent
    ///    - Gia đình 3: Dam, Sana, Vini
    /// 3. Thiết lập Player với Hòm đồ (10 ô, 5 stack), Quick item (4 ô), Vàng (10,000), Danh tiếng (1,000).
    /// 4. Thiết lập tương tác phím [E] cho cả 10 NPC và Nhà Trưởng Làng (ElderHouse).
    /// 5. Thiết lập Cửa hàng Trưởng làng 2 cột: Cửa hàng bên trái, Giỏ đồ bên phải.
    /// </summary>
    public static class VillageSceneSetup
    {
        private const string RootDir = "Assets/Members/TranHuuHau";
        private const string SceneDir = RootDir + "/Scenes";
        public const string TargetScenePath = SceneDir + "/Village_Hau.unity";
        private const string SourceScenePath = "Assets/Members/NguyenVanHai/Scenes/Village.unity";
        private const string SpriteDir = "Assets/Sprites/Detailed";

        [MenuItem("Dungeon Eater/Hậu - Làng/Tạo scene Village_Hau (từ map của Hải)")]
        public static void CreateSceneFromMap()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Directory.CreateDirectory(SceneDir);

            // Copy scene từ map của Hải nếu chưa có hoặc muốn làm mới
            if (!File.Exists(TargetScenePath))
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
                {
                    Debug.LogError($"[VillageSceneSetup] Không thể copy scene từ {SourceScenePath}");
                    return;
                }
                AssetDatabase.Refresh();
            }

            var scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            SetupSceneContent(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=#86efac><b>[VillageSceneSetup] ĐÃ TẠO THÀNH CÔNG SCENE {TargetScenePath}! Bấm Play để trải nghiệm.</b></color>");
        }

        [MenuItem("Dungeon Eater/Hậu - Làng/Cập nhật scene đang mở")]
        public static void UpdateCurrentOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            SetupSceneContent(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[VillageSceneSetup] Đã cập nhật xong scene: {scene.name}");
        }

        /// <summary>
        /// Phương thức hỗ trợ chạy tự động qua Unity -batchmode -executeMethod
        /// </summary>
        public static void BatchBuild()
        {
            CreateSceneFromMap();
        }

        private static void SetupSceneContent(Scene scene)
        {
            // 1. Cấu hình EventSystem
            EnsureEventSystem();

            // 2. Cấu hình Player & Camera
            var playerObj = SetupPlayer();

            // 3. Cấu hình Nhà Trưởng Làng (ElderHouse)
            SetupElderHouse();

            // 4. Bố trí 10 NPC theo tài liệu
            SetupTenNPCs();

            // 5. Cấu hình Canvas UI (VillageHUD, DialogueUI, ShopUI, InventoryUI)
            SetupUICanvas();
        }

        private static void EnsureEventSystem()
        {
            var es = Object.FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }
        }

        private static GameObject SetupPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("ChiefPlayer");
            }
            if (player == null)
            {
                player = GameObject.Find("Player_HunterChef");
            }

            if (player == null)
            {
                // Tạo mới nếu chưa có
                player = new GameObject("Player_Hau");
                player.tag = "Player";
                player.transform.position = new Vector3(-13.46f, 0.07f, 0f);

                var sr = player.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Detailed/Customer_Adventurer_idle.png");
                sr.sortingOrder = 10;
            }
            else
            {
                player.tag = "Player";
            }

            // Hủy component ChiefPlayer cũ nếu có để tránh xung đột 2 script điều khiển
            var chief = player.GetComponent<ChiefPlayer>();
            if (chief != null)
            {
                chief.enabled = false;
                Object.DestroyImmediate(chief, true);
            }

            // Cấu hình Rigidbody2D chuẩn cho 2D Top-down (Khử rung màn hình)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Đảm bảo không lật sprite thủ công
            var srPlayer = player.GetComponent<SpriteRenderer>();
            if (srPlayer != null) srPlayer.flipX = false;

            // Gắn BoxCollider2D nếu chưa có
            var col = player.GetComponent<Collider2D>();
            if (col == null)
            {
                var bc = player.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(0.8f, 0.8f);
            }

            // Gắn Controller của Hậu
            var controller = player.GetComponent<VillagePlayerController>();
            if (controller == null) controller = player.AddComponent<VillagePlayerController>();

            // Gán Animator Controller chuẩn của Hậu (HauChef.controller)
            var anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Members/TranHuuHau/Animations/HauChef.controller");
                if (ac != null) anim.runtimeAnimatorController = ac;
            }

            // Gắn Wallet (10,000 Vàng, 1,000 Danh tiếng)
            var wallet = player.GetComponent<VillageWallet>();
            if (wallet == null) wallet = player.AddComponent<VillageWallet>();

            // Gắn Inventory (10 ô, 5 stack, 4 quick slots)
            var inv = player.GetComponent<VillageInventory>();
            if (inv == null) inv = player.AddComponent<VillageInventory>();

            // Tách Camera độc lập (không làm con của Player) để khử hoàn toàn rung giật va chạm vật lý
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(null);
                cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.65f, -10f);
                cam.transform.rotation = Quaternion.identity;
            }

            return player;
        }

        private static void SetupElderHouse()
        {
            var elderHouse = GameObject.Find("ElderHouse");
            if (elderHouse == null)
            {
                Debug.LogWarning("[VillageSceneSetup] Không tìm thấy GameObject 'ElderHouse', tạo điểm trigger tại (8.8, -2.23)");
                elderHouse = new GameObject("ElderHouse");
                elderHouse.transform.position = new Vector3(8.8f, -2.23f, 0f);
            }

            // Gắn Trigger cho Nhà Trưởng Làng
            var trigger = elderHouse.GetComponent<ElderHouseTrigger>();
            if (trigger == null) trigger = elderHouse.AddComponent<ElderHouseTrigger>();

            // Kiểm tra door child nếu có
            var door = elderHouse.transform.Find("door");
            if (door != null)
            {
                var doorTrigger = door.GetComponent<ElderHouseTrigger>();
                if (doorTrigger == null) doorTrigger = door.gameObject.AddComponent<ElderHouseTrigger>();
            }

            // Tạo thêm điểm Trigger ngay trước bậc thềm lối vào cửa (8.8, -4.3)
            var entrance = elderHouse.transform.Find("ElderHouse_EntranceTrigger");
            GameObject entranceGo;
            if (entrance != null)
            {
                entranceGo = entrance.gameObject;
            }
            else
            {
                entranceGo = new GameObject("ElderHouse_EntranceTrigger");
                entranceGo.transform.SetParent(elderHouse.transform, false);
            }
            entranceGo.transform.position = new Vector3(8.8f, -4.3f, 0f);

            var entranceTrigger = entranceGo.GetComponent<ElderHouseTrigger>();
            if (entranceTrigger == null) entranceTrigger = entranceGo.AddComponent<ElderHouseTrigger>();
        }

        private static void SetupTenNPCs()
        {
            // Tạo thư mục / container chứa các NPC cho gọn gàng
            var npcRoot = GameObject.Find("Village_NPCs");
            if (npcRoot == null)
            {
                npcRoot = new GameObject("Village_NPCs");
            }

            // Sprite Portraits & Characters
            var elderPortrait = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/NPC_Elder_Portrait.png");
            var adventurerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_Adventurer_idle.png");
            var farmerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_Farmer_idle.png");
            var merchantSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_Merchant_idle.png");
            var oldLadySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_OldLady_idle.png");
            var travelerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_Traveler_idle.png");
            var villagerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Customer_Villager_idle.png");

            // Tải Sprite cơ thể đầy đủ của Trưởng Làng Godwin từ spritesheet (Elder_Idle_0)
            var allElderAssets = AssetDatabase.LoadAllAssetsAtPath($"{SpriteDir}/NPC_Elder_Animations.png");
            Sprite elderBodySprite = null;
            if (allElderAssets != null)
            {
                foreach (var a in allElderAssets)
                {
                    if (a is Sprite sp && sp.name == "Elder_Idle_0")
                    {
                        elderBodySprite = sp;
                        break;
                    }
                }
            }
            if (elderBodySprite == null)
            {
                elderBodySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/NPC_Elder_Animations.png");
            }

            // Tải Animator Controller cho Trưởng Làng để có animation thở / cử động tự nhiên
            var elderAnimController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Elder/NPC_Elder_Animator.controller");

            // 1. Trưởng làng Godwin - Đứng ngay trước sân nhà Trưởng Làng (ElderHouse: x=8.8, y=-2.23)
            // Đặt ở vị trí x=7.6, y=-4.9 trên khoảng sân trống trước hiên nhà, có cơ thể đầy đủ và Animator
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Elder_Godwin",
                new Vector3(7.6f, -4.9f, 0f),
                VillageDialogueData.GetElderGodwin(),
                elderBodySprite, elderPortrait, 0.85f, elderAnimController);

            // ================= GIA ĐÌNH 1 (Đứng trước cửa nhà House_2: x=-18.75, y=4.4) =================
            // Walkway trước nhà ở khoảng y=1.0 đến 1.6, hoàn toàn nằm ngoài collider của nhà (collider kết thúc tại y=2.31)
            // 2. Chồng Sam (Tiều phu / Nông dân)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family1_Sam",
                new Vector3(-20.2f, 1.6f, 0f),
                VillageDialogueData.GetSam(),
                farmerSprite, farmerSprite, 0.58f);

            // 3. Vợ Nana (Nội trợ)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family1_Nana",
                new Vector3(-17.3f, 1.6f, 0f),
                VillageDialogueData.GetNana(),
                oldLadySprite, oldLadySprite, 0.55f);

            // 4. Con gái Leaf (Bé gái nhỏ)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family1_Leaf",
                new Vector3(-18.75f, 1.0f, 0f),
                VillageDialogueData.GetLeaf(),
                villagerSprite, villagerSprite, 0.42f);

            // ================= GIA ĐÌNH 2 (Đứng trước cửa nhà House_2 (1): x=-8.49, y=4.4) =================
            // 5. Chồng Gam (Cựu thợ rèn / Dân quân)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family2_Gam",
                new Vector3(-9.9f, 1.6f, 0f),
                VillageDialogueData.GetGam(),
                adventurerSprite, adventurerSprite, 0.58f);

            // 6. Vợ Ana (Người dệt vải)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family2_Ana",
                new Vector3(-7.1f, 1.6f, 0f),
                VillageDialogueData.GetAna(),
                villagerSprite, villagerSprite, 0.55f);

            // 7. Con trai Vincent (Thiếu niên tập kiếm)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family2_Vincent",
                new Vector3(-8.49f, 1.0f, 0f),
                VillageDialogueData.GetVincent(),
                adventurerSprite, adventurerSprite, 0.46f);

            // ================= GIA ĐÌNH 3 (Đứng trước cửa nhà House_0: x=8.8, y=4.59) =================
            // 8. Chồng Dam (Gác cổng làng)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family3_Dam",
                new Vector3(7.4f, 1.7f, 0f),
                VillageDialogueData.GetDam(),
                merchantSprite, merchantSprite, 0.58f);

            // 9. Vợ Sana (Thầy thuốc thảo mộc)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family3_Sana",
                new Vector3(10.2f, 1.7f, 0f),
                VillageDialogueData.GetSana(),
                farmerSprite, farmerSprite, 0.55f);

            // 10. Con gái Vini (Bé gái nhút nhát)
            CreateOrUpdateNPC(npcRoot.transform, "NPC_Family3_Vini",
                new Vector3(8.8f, 1.0f, 0f),
                VillageDialogueData.GetVini(),
                travelerSprite, travelerSprite, 0.42f);

            Debug.Log("[VillageSceneSetup] Đã bố trí đủ 10 NPC dạng người đầy đủ chân tay, đứng ngay trước cửa nhà của họ!");
        }

        private static GameObject CreateOrUpdateNPC(Transform parent, string goName, Vector3 pos, DialogueEntry dialogue, Sprite charSprite, Sprite portrait, float scale = 1f, RuntimeAnimatorController animController = null)
        {
            var t = parent.Find(goName);
            GameObject go;
            if (t != null)
            {
                go = t.gameObject;
            }
            else
            {
                go = new GameObject(goName);
                go.transform.SetParent(parent, false);
            }

            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            if (charSprite != null) sr.sprite = charSprite;
            sr.sortingLayerName = "Entities";
            sr.sortingOrder = 5000 + Mathf.RoundToInt(-pos.y * 100);

            var col = go.GetComponent<CircleCollider2D>();
            if (col == null) col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.offset = new Vector2(0f, 0.35f);
            col.isTrigger = false;

            if (animController != null)
            {
                var anim = go.GetComponent<Animator>();
                if (anim == null) anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = animController;
            }
            else
            {
                var anim = go.GetComponent<Animator>();
                if (anim != null) Object.DestroyImmediate(anim, true);
            }

            var npc = go.GetComponent<VillageNPC>();
            if (npc == null) npc = go.AddComponent<VillageNPC>();
            npc.Setup(dialogue.speakerName, dialogue.speakerRole, dialogue.lines, portrait);

            // Xóa nameplate cũ nếu có để Nameplate được tạo mới sạch sẽ chuẩn vị trí
            var oldNameplate = go.transform.Find("Nameplate");
            if (oldNameplate != null)
            {
                Object.DestroyImmediate(oldNameplate.gameObject, true);
            }

            return go;
        }

        private static void SetupUICanvas()
        {
            var canvasObj = GameObject.Find("Village_Hau_Canvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Village_Hau_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Gắn các UI Panels vào từng GameObject con riêng biệt để độc lập và an toàn
            EnsureUIChild<VillageHUD>(canvasObj.transform, "HUD_Panel");
            EnsureUIChild<VillageDialogueUI>(canvasObj.transform, "Dialogue_Panel");
            EnsureUIChild<VillageShopUI>(canvasObj.transform, "Shop_Panel");
            EnsureUIChild<VillageInventoryUI>(canvasObj.transform, "Inventory_Panel");
        }

        private static T EnsureUIChild<T>(Transform parent, string name) where T : MonoBehaviour
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                child = go.transform;
            }
            var comp = child.GetComponent<T>();
            if (comp == null) comp = child.gameObject.AddComponent<T>();
            return comp;
        }
    }
}
