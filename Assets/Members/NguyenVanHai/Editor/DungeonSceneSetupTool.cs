using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NguyenVanHai.Combat;
using NguyenVanHai.Enemies;
using NguyenVanHai.Dungeon;
using NguyenVanHai.UI;

namespace NguyenVanHai.EditorTools
{
    /// <summary>
    /// Tool tự động gán toàn bộ Script, Collider, Rigidbody và Animator vào các GameObject trong scene Dungeon.unity.
    /// Giúp hiển thị trực quan 100% trong Inspector của Unity Editor mà bạn không phải kéo thả tay.
    /// Chạy tự động khi compile hoặc bấm menu: Dungeon Eater > Hải - Setup Scene Dungeon.
    /// </summary>
    [InitializeOnLoad]
    public static class DungeonSceneSetupTool
    {
        private const string DungeonScenePath = "Assets/Members/NguyenVanHai/Scenes/Dungeon.unity";

        static DungeonSceneSetupTool()
        {
            EditorApplication.delayCall += AutoSetupIfDungeonOpen;
        }

        private static void AutoSetupIfDungeonOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name == "Dungeon" || activeScene.path.Contains("Dungeon.unity"))
            {
                SetupCurrentDungeonScene();
            }
        }

        [MenuItem("Dungeon Eater/Hải - Gán toàn bộ Script vào Scene Dungeon")]
        public static void SetupCurrentDungeonSceneMenu()
        {
            SetupCurrentDungeonScene();
        }

        public static void SetupCurrentDungeonScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Dungeon" && !scene.path.Contains("Dungeon.unity"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    scene = EditorSceneManager.OpenScene(DungeonScenePath);
                }
                else return;
            }

            Debug.Log("<color=cyan>[DungeonSetup] Bắt đầu tự động gán toàn bộ Script vào GameObject trong Scene Dungeon...</color>");

            // 1. Gắn DungeonGameManager
            SetupManagerObject();

            // 2. Cấu hình HunterPlayer
            SetupHunterPlayer();

            // 3. Gán script AI cho quái vật (Slime, Nấm, Bọ, Boss)
            SetupAllEnemies();

            // 4. Gán script cho toàn bộ Cửa
            SetupAllDoors();

            // 5. Đánh dấu và Lưu scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=green>[DungeonSetup] HOÀN TẤT! Đã gán đầy đủ toàn bộ Script vào GameObject trong Scene Dungeon.unity.</color>");
        }

        private static void SetupManagerObject()
        {
            var mgr = Object.FindObjectOfType<DungeonGameManager>();
            GameObject mgrGo;
            if (mgr == null)
            {
                mgrGo = new GameObject("DungeonGameManager");
                mgr = mgrGo.AddComponent<DungeonGameManager>();
            }
            else
            {
                mgrGo = mgr.gameObject;
            }

            if (mgrGo.GetComponent<DungeonHUD>() == null) mgrGo.AddComponent<DungeonHUD>();
            if (mgrGo.GetComponent<DungeonInventoryUI>() == null) mgrGo.AddComponent<DungeonInventoryUI>();
            if (mgrGo.GetComponent<DungeonEndScreens>() == null) mgrGo.AddComponent<DungeonEndScreens>();
        }

        private static void SetupHunterPlayer()
        {
            var player = Object.FindObjectOfType<HunterPlayer>();
            if (player == null) return;

            var arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Detailed/VFX_Arrow_Normal_0.png");
            var hunterController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Members/NguyenVanHai/Animations/Player/Hunter/Hunter.controller");

            var serializedObj = new SerializedObject(player);
            if (arrowSprite != null)
            {
                var arrowProp = serializedObj.FindProperty("defaultArrowSprite");
                if (arrowProp != null) arrowProp.objectReferenceValue = arrowSprite;
            }
            serializedObj.ApplyModifiedProperties();

            var anim = player.GetComponent<Animator>();
            if (anim != null && hunterController != null && anim.runtimeAnimatorController == null)
            {
                anim.runtimeAnimatorController = hunterController;
            }
        }

        private static void SetupAllEnemies()
        {
            var slimeController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Members/NguyenVanHai/Animations/Enemy/Slime/slime.controller");
            var mushroomController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Members/NguyenVanHai/Animations/Enemy/Mushroom/mushroom.controller");
            var bugController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Members/NguyenVanHai/Animations/Enemy/Bug/bug.controller");

            var sporeBulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Detailed/VFX_Poison_Spore_0.png");

            var allSR = Object.FindObjectsOfType<SpriteRenderer>(true);
            foreach (var sr in allSR)
            {
                var go = sr.gameObject;
                string objName = go.name.ToLower();
                string spriteName = sr.sprite != null ? sr.sprite.name.ToLower() : "";

                // Bỏ qua Player
                if (go.GetComponent<HunterPlayer>() != null || objName.Contains("hunter")) continue;

                // --- 1. Slime ---
                if (objName.Contains("slime") || spriteName.Contains("slime"))
                {
                    EnsureComponent<DamageFlash>(go);
                    EnsureRigidbody(go);
                    var col = EnsureCollider<CircleCollider2D>(go);
                    col.radius = 0.45f;

                    if (go.GetComponent<SlimeAI>() == null && go.GetComponent<EnemyBase>() == null)
                    {
                        go.AddComponent<SlimeAI>();
                    }

                    var anim = go.GetComponent<Animator>();
                    if (anim != null && slimeController != null && anim.runtimeAnimatorController == null)
                    {
                        anim.runtimeAnimatorController = slimeController;
                    }
                }
                // --- 2. Nấm (Spore / Mushroom) ---
                else if (objName.Contains("mushroom") || objName.Contains("spore") || spriteName.Contains("spore") || spriteName.Contains("mushroom"))
                {
                    EnsureComponent<DamageFlash>(go);
                    EnsureRigidbody(go);
                    var col = EnsureCollider<CircleCollider2D>(go);
                    col.radius = 0.5f;

                    var sporeAI = go.GetComponent<SporeShooterAI>();
                    if (sporeAI == null && go.GetComponent<EnemyBase>() == null)
                    {
                        sporeAI = go.AddComponent<SporeShooterAI>();
                    }

                    if (sporeAI != null && sporeBulletSprite != null)
                    {
                        var so = new SerializedObject(sporeAI);
                        var prop = so.FindProperty("sporeBulletSprite");
                        if (prop != null) prop.objectReferenceValue = sporeBulletSprite;
                        so.ApplyModifiedProperties();
                    }

                    var anim = go.GetComponent<Animator>();
                    if (anim != null && mushroomController != null && anim.runtimeAnimatorController == null)
                    {
                        anim.runtimeAnimatorController = mushroomController;
                    }
                }
                // --- 3. Bọ (Beetle / Bug) ---
                else if (objName.Contains("beetle") || objName.Contains("bug") || spriteName.Contains("beetle") || spriteName.Contains("bug") || objName.Contains("nhen"))
                {
                    EnsureComponent<DamageFlash>(go);
                    EnsureRigidbody(go);
                    var col = EnsureCollider<CircleCollider2D>(go);
                    col.radius = 0.45f;

                    if (go.GetComponent<BeetleMeleeAI>() == null && go.GetComponent<EnemyBase>() == null)
                    {
                        go.AddComponent<BeetleMeleeAI>();
                    }

                    var anim = go.GetComponent<Animator>();
                    if (anim != null && bugController != null && anim.runtimeAnimatorController == null)
                    {
                        anim.runtimeAnimatorController = bugController;
                    }
                }
                // --- 4. Boss King Boar ---
                else if (objName.Contains("boar") || objName.Contains("boss") || spriteName.Contains("boar"))
                {
                    EnsureComponent<DamageFlash>(go);
                    EnsureRigidbody(go);
                    var col = EnsureCollider<CircleCollider2D>(go);
                    col.radius = 1.2f;

                    if (go.GetComponent<BossKingBoarAI>() == null && go.GetComponent<EnemyBase>() == null)
                    {
                        go.AddComponent<BossKingBoarAI>();
                    }
                }
            }
        }

        private static void SetupAllDoors()
        {
            var doorFrames = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                doorFrames[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Members/NguyenVanHai/Sprites/RafaelMatos/ERW - Village(interiors)/assets/animations/doors-d1-opening-h.png");
            }

            var allTrans = Object.FindObjectsOfType<Transform>(true);
            foreach (var t in allTrans)
            {
                if (t.name.Contains("doors-d1-opening-h") || (t.name.ToLower().Contains("door") && !t.name.ToLower().Contains("floor")))
                {
                    var door = t.GetComponent<DungeonDoor>();
                    if (door == null)
                    {
                        door = t.gameObject.AddComponent<DungeonDoor>();
                    }
                }
            }
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        private static Rigidbody2D EnsureRigidbody(GameObject go)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return rb;
        }

        private static T EnsureCollider<T>(GameObject go) where T : Collider2D
        {
            var col = go.GetComponent<T>();
            if (col == null) col = go.AddComponent<T>();
            return col;
        }
    }
}
