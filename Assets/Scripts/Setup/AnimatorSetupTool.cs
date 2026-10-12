#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace DungeonEater.Setup
{
    /// <summary>
    /// Editor Script tự động tạo Animator Controller chuẩn 4-direction cho Hunter và Chef.
    /// Chạy từ menu: DungeonEater > Setup > Create Animator Controllers
    /// </summary>
    public static class AnimatorSetupTool
    {
        // ─── Cấu hình sprite sheet ─────────────────────────────────────────────
        // Hunter: 1024x1024, 8 cols x 6 rows => frame 128x128
        // Row 0: Walk Down  (8 frame)
        // Row 1: Walk Up    (8 frame)
        // Row 2: Walk Left  (8 frame)
        // Row 3: Walk Right (8 frame)
        // Row 4: Attack Down (4 frame)
        // Row 5: Attack Side (4 frame)

        // Chef: 1024x1024, 6 cols x 6 rows => frame 170x170 (approx)
        // Row 0: Walk Down, Row 1: Walk Up, Row 2: Walk Left, Row 3: Walk Right
        // Row 4: Chop (4 frame), Row 5: Serve (4 frame)

        private const string HunterSpritePath  = "Assets/Sprites/Detailed/Hunter_Combat_4Dir_Sheet.png";
        private const string ChefSpritePath    = "Assets/Sprites/Detailed/Hero_Chef_4Dir_Sheet.png";
        private const string OutputDir         = "Assets/Animations";

        [MenuItem("DungeonEater/Setup/Create Animator Controllers")]
        public static void CreateAll()
        {
            AssetDatabase.Refresh();

            CreateHunterController();
            CreateChefController();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AnimatorSetup] ✅ Tạo Animator Controllers thành công! Xem trong Assets/Animations/");
        }

        // ──────────────────────────────────────────────────────────────────────
        // HUNTER COMBAT CONTROLLER
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateHunterController()
        {
            string dir = OutputDir + "/Hunter";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var sprites = AssetDatabase.LoadAllAssetsAtPath(HunterSpritePath);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[AnimatorSetup] Không tìm thấy sprites tại: {HunterSpritePath}\nHãy slice sprite sheet trước (xem DungeonEater > Setup > Slice Sprite Sheets)");
            }

            // Tạo AnimationClips
            AnimationClip idleDown   = CreateSpriteClip("Hunter_Idle_Down",   sprites, 0, 1, 8f, true);
            AnimationClip idleUp     = CreateSpriteClip("Hunter_Idle_Up",     sprites, 8, 1, 8f, true);
            AnimationClip idleLeft   = CreateSpriteClip("Hunter_Idle_Left",   sprites, 16, 1, 8f, true);
            AnimationClip idleRight  = CreateSpriteClip("Hunter_Idle_Right",  sprites, 24, 1, 8f, true);

            AnimationClip walkDown   = CreateSpriteClip("Hunter_Walk_Down",   sprites, 0, 8, 10f, true);
            AnimationClip walkUp     = CreateSpriteClip("Hunter_Walk_Up",     sprites, 8, 8, 10f, true);
            AnimationClip walkLeft   = CreateSpriteClip("Hunter_Walk_Left",   sprites, 16, 8, 10f, true);
            AnimationClip walkRight  = CreateSpriteClip("Hunter_Walk_Right",  sprites, 24, 8, 10f, true);

            AnimationClip attackDown = CreateSpriteClip("Hunter_Attack_Down", sprites, 32, 4, 14f, false);
            AnimationClip attackSide = CreateSpriteClip("Hunter_Attack_Side", sprites, 40, 4, 14f, false);
            AnimationClip hurt       = CreateSpriteClip("Hunter_Hurt",        sprites, 0,  2, 10f, false);

            // Lưu clips
            AnimationClip[] allClips = { idleDown, idleUp, idleLeft, idleRight,
                                         walkDown, walkUp, walkLeft, walkRight,
                                         attackDown, attackSide, hurt };
            foreach (var clip in allClips)
                if (clip != null) SaveAsset(clip, $"{dir}/{clip.name}.anim");

            // Tạo Animator Controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/Hunter_AnimatorController.controller");

            // Thêm Parameters
            controller.AddParameter("dirX",       AnimatorControllerParameterType.Float);
            controller.AddParameter("dirY",       AnimatorControllerParameterType.Float);
            controller.AddParameter("isMoving",   AnimatorControllerParameterType.Bool);
            controller.AddParameter("isDashing",  AnimatorControllerParameterType.Bool);
            controller.AddParameter("swordAttack",AnimatorControllerParameterType.Trigger);
            controller.AddParameter("bowShoot",   AnimatorControllerParameterType.Trigger);
            controller.AddParameter("hurt",       AnimatorControllerParameterType.Trigger);

            var rootSM = controller.layers[0].stateMachine;

            // ── States ─────────────────────────────────────────────────────────
            var stIdleDown   = AddState(rootSM, "Idle_Down",   idleDown,   new Vector3(-300, 0));
            var stIdleUp     = AddState(rootSM, "Idle_Up",     idleUp,     new Vector3(-300, 60));
            var stIdleLeft   = AddState(rootSM, "Idle_Left",   idleLeft,   new Vector3(-300, 120));
            var stIdleRight  = AddState(rootSM, "Idle_Right",  idleRight,  new Vector3(-300, 180));

            var stWalkDown   = AddState(rootSM, "Walk_Down",   walkDown,   new Vector3(100, 0));
            var stWalkUp     = AddState(rootSM, "Walk_Up",     walkUp,     new Vector3(100, 60));
            var stWalkLeft   = AddState(rootSM, "Walk_Left",   walkLeft,   new Vector3(100, 120));
            var stWalkRight  = AddState(rootSM, "Walk_Right",  walkRight,  new Vector3(100, 180));

            var stAttackDown = AddState(rootSM, "Attack_Down", attackDown, new Vector3(400, 60));
            var stAttackSide = AddState(rootSM, "Attack_Side", attackSide, new Vector3(400, 120));
            var stHurt       = AddState(rootSM, "Hurt",        hurt,       new Vector3(400, 180));

            // Default state
            rootSM.defaultState = stIdleDown;

            // ── Transitions: Idle <-> Walk theo direction ──────────────────────
            // Idle Down -> Walk Down (isMoving=true AND dirY < -0.5)
            AddTransition(stIdleDown, stWalkDown, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.If, 0),
                ("dirY", AnimatorConditionMode.Less, -0.5f)
            });
            AddTransition(stWalkDown, stIdleDown, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.IfNot, 0),
                ("dirY", AnimatorConditionMode.Less, -0.5f)
            });

            // Idle Up <-> Walk Up
            AddTransition(stIdleUp, stWalkUp, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.If, 0),
                ("dirY", AnimatorConditionMode.Greater, 0.5f)
            });
            AddTransition(stWalkUp, stIdleUp, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.IfNot, 0),
                ("dirY", AnimatorConditionMode.Greater, 0.5f)
            });

            // Idle Left <-> Walk Left
            AddTransition(stIdleLeft, stWalkLeft, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.If, 0),
                ("dirX", AnimatorConditionMode.Less, -0.5f)
            });
            AddTransition(stWalkLeft, stIdleLeft, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.IfNot, 0),
                ("dirX", AnimatorConditionMode.Less, -0.5f)
            });

            // Idle Right <-> Walk Right
            AddTransition(stIdleRight, stWalkRight, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.If, 0),
                ("dirX", AnimatorConditionMode.Greater, 0.5f)
            });
            AddTransition(stWalkRight, stIdleRight, new (string, AnimatorConditionMode, float)[]
            {
                ("isMoving", AnimatorConditionMode.IfNot, 0),
                ("dirX", AnimatorConditionMode.Greater, 0.5f)
            });

            // ── AnyState -> Attack / Hurt (trigger) ───────────────────────────
            AddAnyTransition(rootSM, stAttackDown, "swordAttack", 0.1f);
            AddAnyTransition(rootSM, stAttackSide, "bowShoot",    0.1f);
            AddAnyTransition(rootSM, stHurt,       "hurt",        0.05f);

            // Attack / Hurt -> Idle Down (exit time)
            AddExitTransition(stAttackDown, stIdleDown, 0.9f);
            AddExitTransition(stAttackSide, stIdleDown, 0.9f);
            AddExitTransition(stHurt,       stIdleDown, 0.9f);

            EditorUtility.SetDirty(controller);
            Debug.Log($"[AnimatorSetup] ✅ Hunter_AnimatorController tạo xong tại {dir}");
        }

        // ──────────────────────────────────────────────────────────────────────
        // CHEF CONTROLLER
        // ──────────────────────────────────────────────────────────────────────
        private static void CreateChefController()
        {
            string dir = OutputDir + "/Chef";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var sprites = AssetDatabase.LoadAllAssetsAtPath(ChefSpritePath);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[AnimatorSetup] Không tìm thấy sprites tại: {ChefSpritePath}");
            }

            AnimationClip walkDown  = CreateSpriteClip("Chef_Walk_Down",  sprites, 0,  6, 10f, true);
            AnimationClip walkUp    = CreateSpriteClip("Chef_Walk_Up",    sprites, 6,  6, 10f, true);
            AnimationClip walkLeft  = CreateSpriteClip("Chef_Walk_Left",  sprites, 12, 6, 10f, true);
            AnimationClip walkRight = CreateSpriteClip("Chef_Walk_Right", sprites, 18, 6, 10f, true);
            AnimationClip chop      = CreateSpriteClip("Chef_Chop",       sprites, 24, 4, 10f, false);
            AnimationClip serve     = CreateSpriteClip("Chef_Serve",      sprites, 28, 4, 10f, false);

            AnimationClip[] allClips = { walkDown, walkUp, walkLeft, walkRight, chop, serve };
            foreach (var clip in allClips)
                if (clip != null) SaveAsset(clip, $"{dir}/{clip.name}.anim");

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{dir}/Chef_AnimatorController.controller");
            controller.AddParameter("dirX",     AnimatorControllerParameterType.Float);
            controller.AddParameter("dirY",     AnimatorControllerParameterType.Float);
            controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isChop",   AnimatorControllerParameterType.Trigger);
            controller.AddParameter("isServe",  AnimatorControllerParameterType.Trigger);

            var rootSM = controller.layers[0].stateMachine;

            var stWalkDown  = AddState(rootSM, "Walk_Down",  walkDown,  new Vector3(-200, 0));
            var stWalkUp    = AddState(rootSM, "Walk_Up",    walkUp,    new Vector3(-200, 60));
            var stWalkLeft  = AddState(rootSM, "Walk_Left",  walkLeft,  new Vector3(-200, 120));
            var stWalkRight = AddState(rootSM, "Walk_Right", walkRight, new Vector3(-200, 180));
            var stChop      = AddState(rootSM, "Chop",       chop,      new Vector3(200, 60));
            var stServe     = AddState(rootSM, "Serve",      serve,     new Vector3(200, 120));

            rootSM.defaultState = stWalkDown;

            AddTransition(stWalkDown, stWalkUp,    new (string, AnimatorConditionMode, float)[] { ("dirY", AnimatorConditionMode.Greater, 0.5f) });
            AddTransition(stWalkDown, stWalkLeft,  new (string, AnimatorConditionMode, float)[] { ("dirX", AnimatorConditionMode.Less, -0.5f) });
            AddTransition(stWalkDown, stWalkRight, new (string, AnimatorConditionMode, float)[] { ("dirX", AnimatorConditionMode.Greater, 0.5f) });
            AddTransition(stWalkUp,   stWalkDown,  new (string, AnimatorConditionMode, float)[] { ("dirY", AnimatorConditionMode.Less, -0.5f) });

            AddAnyTransition(rootSM, stChop,  "isChop",  0.1f);
            AddAnyTransition(rootSM, stServe, "isServe", 0.1f);
            AddExitTransition(stChop,  stWalkDown, 0.9f);
            AddExitTransition(stServe, stWalkDown, 0.9f);

            EditorUtility.SetDirty(controller);
            Debug.Log($"[AnimatorSetup] ✅ Chef_AnimatorController tạo xong tại {dir}");
        }

        // ──────────────────────────────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Tạo AnimationClip từ range của sprite sheet đã được slice.</summary>
        private static AnimationClip CreateSpriteClip(string name, Object[] sprites, int startIdx, int count, float fps, bool loop)
        {
            var clip = new AnimationClip { name = name, frameRate = fps };
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = loop });

            if (sprites == null || sprites.Length == 0) return clip; // Trả về clip rỗng nếu chưa slice

            var keyframes = new ObjectReferenceKeyframe[count];
            int spriteCount = 0;
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i] is Sprite) spriteCount++;

            // Lấy sprites theo thứ tự tên (Unity sort theo sprite_0, sprite_1,...)
            var spriteList = new System.Collections.Generic.List<Sprite>();
            foreach (var s in sprites) if (s is Sprite sp) spriteList.Add(sp);
            // Sort theo tên để đảm bảo thứ tự đúng
            spriteList.Sort((a, b) => {
                int numA = ExtractNumber(a.name), numB = ExtractNumber(b.name);
                return numA.CompareTo(numB);
            });

            for (int i = 0; i < count; i++)
            {
                int idx = startIdx + i;
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time  = i / fps,
                    value = (idx < spriteList.Count) ? spriteList[idx] : (spriteList.Count > 0 ? spriteList[0] : null)
                };
            }

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            return clip;
        }

        private static int ExtractNumber(string name)
        {
            var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+$");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        private static AnimatorState AddState(AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos)
        {
            var state = sm.AddState(name, pos);
            if (clip != null) state.motion = clip;
            return state;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to,
            (string param, AnimatorConditionMode mode, float threshold)[] conditions,
            float exitTime = 0f, bool hasExitTime = false)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = hasExitTime;
            t.exitTime    = exitTime;
            t.duration    = 0.05f;
            foreach (var c in conditions)
                t.AddCondition(c.mode, c.threshold, c.param);
        }

        private static void AddAnyTransition(AnimatorStateMachine sm, AnimatorState to, string trigger, float duration = 0.1f)
        {
            var t = sm.AddAnyStateTransition(to);
            t.hasExitTime = false;
            t.duration    = duration;
            t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime    = exitTime;
            t.duration    = 0.1f;
        }

        private static void SaveAsset(Object asset, string path)
        {
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
