using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NguyenVanHai.Data
{
    [Serializable]
    public class PlayerConfigDto
    {
        public float maxHealth = 100f;
        public float maxStamina = 100f;
        public float dashStaminaCost = 20f;
        public float staminaRegenRate = 10f;
        public float staminaRegenDelay = 0.5f;
        public float moveSpeed = 5f;
        public float dashSpeed = 13f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 0.4f;
        public float swordAttackRange = 1.3f;
        public float swordArcAngle = 180f;
        public float swordLightDamage = 20f;
        public float swordComboBonusPerHit = 5f;
        public float swordHeavyDamage = 40f;
        public float swordHeavyRangeMultiplier = 1.35f;
        public float swordHeavyArcAngle = 200f;
        public float swordSkillDamage = 35f;
        public float swordSkillRangeMultiplier = 1.5f;
        public float comboResetTime = 0.8f;
        public float arrowSpeed = 14f;
        public float maxChargeTime = 1.2f;
        public float bowQuickDamage = 15f;
        public float bowChargeMinDamage = 20f;
        public float bowChargeMaxDamage = 50f;
        public float bowSkillDamage = 40f;
        public int potionCount = 3;
        public float potionHealAmount = 20f;
        public float interactionDistance = 0.7f;
        public float interactionRadius = 0.4f;
    }

    [Serializable]
    public class TrapConfigDto
    {
        public float spikeDamage = 15f;
        public float spikeCycleTime = 2.5f;
        public float spikeWarningTime = 0.4f;
        public float spikeActiveTime = 1.0f;
        public float standingTickInterval = 0.4f;
        public float poisonDamagePerTick = 5f;
        public float poisonTickInterval = 2.0f;
        public float slowMultiplier = 0.5f;
    }

    [Serializable]
    public class EnemyConfigDto
    {
        public string id = "";
        public string[] aliases;
        public string displayName = "";
        public float maxHealth = 40f;
        public float moveSpeed = 2.4f;
        public float attackDamage = 8f;
        public float detectRange = 8f;
        public float attackRange = 2.0f;
        public float attackCooldown = 2.2f;
        public float telegraphTime = 0.80f;
        public float restAfterAttack = 1.25f;
        public float lungeSpeed = 7.0f;
        public float lungeDuration = 0.28f;
        public float clawAttackRange = 1.3f;
        public float clawArcAngle = 180f;
        public float minKeepDistance = 4.0f;
        public float maxShootDistance = 8.0f;
        public float projectileSpeed = 5.5f;
        public string dropItemId = "slime_gel";
        public string dropItemName = "Chất nhầy dẻo";
        public int dropCount = 1;
    }

    [Serializable]
    public class BossSkillDto
    {
        public string name = "";
        public float damage = 25f;
        public float speed = 8f;
        public float rangeOrRadius = 3f;
        public float widthOrAngle = 1.5f;
        public int projectileCount = 1;
        public int phase2Count = 1;
        public float spawnInterval = 0.5f;
        public float arcOrDropHeight = 3.5f;
        public float telegraphTime = 1.0f;
        public float activeDuration = 0.5f;
        public float recoveryTime = 1.8f;
        public float cooldown = 5.0f;
    }

    [Serializable]
    public class BossConfigDto
    {
        public string id = "";
        public string[] aliases;
        public string displayName = "";
        public string title = "";
        public int floor = 1;
        public float maxHealth = 500f;
        public float moveSpeed = 2.8f;
        public float attackDamage = 25f;
        public float detectRange = 15f;
        public float attackRange = 3.5f;
        public float attackCooldown = 2.5f;
        public float minMoveBetweenSkills = 1.5f;
        public float preferredMinRange = 3.0f;
        public float preferredMaxRange = 8.5f;
        public string dropItemId = "snake_meat";
        public string dropItemName = "Nguyên liệu Boss";
        public int dropCount = 1;
        public BossSkillDto skill1 = new BossSkillDto();
        public BossSkillDto skill2 = new BossSkillDto();
        public BossSkillDto skill3 = new BossSkillDto();
        public BossSkillDto skill4 = new BossSkillDto();
    }

    [Serializable]
    public class DungeonCombatCatalogRoot
    {
        public string version = "1.0.0";
        public string description = "";
        public PlayerConfigDto player = new PlayerConfigDto();
        public TrapConfigDto traps = new TrapConfigDto();
        public List<EnemyConfigDto> enemies = new List<EnemyConfigDto>();
        public List<BossConfigDto> bosses = new List<BossConfigDto>();
    }

    /// <summary>
    /// Bộ nạp dữ liệu JSON trung tâm cho toàn bộ thông số chiến đấu trong Dungeon:
    /// - Đọc từ: Assets/Members/NguyenVanHai/Data/dungeon_combat_config.json
    ///   (và fallback qua Resources/Data/dungeon_combat_config.json khi đóng gói Build).
    /// - Cung cấp API tĩnh cho toàn bộ script của NguyenVanHai và Backend của NguyenMinhHiep gọi trực tiếp.
    /// </summary>
    public static class DungeonCombatDatabase
    {
        private const string DIRECT_JSON_RELATIVE_PATH = "Members/NguyenVanHai/Data/dungeon_combat_config.json";
        private const string RESOURCES_JSON_PATH = "Data/dungeon_combat_config";

        private static bool isLoaded = false;
        private static DungeonCombatCatalogRoot catalog = new DungeonCombatCatalogRoot();
        private static readonly Dictionary<string, EnemyConfigDto> enemyById = new Dictionary<string, EnemyConfigDto>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BossConfigDto> bossById = new Dictionary<string, BossConfigDto>(StringComparer.OrdinalIgnoreCase);

        public static DungeonCombatCatalogRoot Catalog
        {
            get
            {
                EnsureLoaded();
                return catalog;
            }
        }

        public static void EnsureLoaded(bool forceReload = false)
        {
            if (isLoaded && !forceReload) return;

            string jsonText = null;
            string fullFilePath = Path.Combine(Application.dataPath, DIRECT_JSON_RELATIVE_PATH);

            if (File.Exists(fullFilePath))
            {
                try
                {
                    jsonText = File.ReadAllText(fullFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DungeonCombatDatabase] Không thể đọc file trực tiếp {fullFilePath}: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(jsonText))
            {
                TextAsset ta = Resources.Load<TextAsset>(RESOURCES_JSON_PATH);
                if (ta != null)
                {
                    jsonText = ta.text;
                }
            }

            if (!string.IsNullOrEmpty(jsonText))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<DungeonCombatCatalogRoot>(jsonText);
                    if (parsed != null)
                    {
                        catalog = parsed;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DungeonCombatDatabase] Lỗi phân tích JSON dungeon_combat_config.json: {ex.Message}");
                }
            }

            RebuildLookupTables();
            isLoaded = true;
        }

        private static void RebuildLookupTables()
        {
            enemyById.Clear();
            bossById.Clear();

            if (catalog.enemies != null)
            {
                foreach (var e in catalog.enemies)
                {
                    if (e == null || string.IsNullOrEmpty(e.id)) continue;
                    enemyById[e.id.Trim()] = e;
                    if (e.aliases != null)
                    {
                        foreach (var a in e.aliases)
                        {
                            if (!string.IsNullOrEmpty(a)) enemyById[a.Trim()] = e;
                        }
                    }
                }
            }

            if (catalog.bosses != null)
            {
                foreach (var b in catalog.bosses)
                {
                    if (b == null || string.IsNullOrEmpty(b.id)) continue;
                    bossById[b.id.Trim()] = b;
                    if (b.aliases != null)
                    {
                        foreach (var a in b.aliases)
                        {
                            if (!string.IsNullOrEmpty(a)) bossById[a.Trim()] = b;
                        }
                    }
                }
            }
        }

        public static PlayerConfigDto GetPlayerConfig()
        {
            EnsureLoaded();
            return catalog.player ?? new PlayerConfigDto();
        }

        public static TrapConfigDto GetTrapConfig()
        {
            EnsureLoaded();
            return catalog.traps ?? new TrapConfigDto();
        }

        public static EnemyConfigDto GetEnemy(string idOrAlias)
        {
            if (string.IsNullOrEmpty(idOrAlias)) return null;
            EnsureLoaded();
            return enemyById.TryGetValue(idOrAlias.Trim(), out var dto) ? dto : null;
        }

        public static BossConfigDto GetBoss(string idOrAlias)
        {
            if (string.IsNullOrEmpty(idOrAlias)) return null;
            EnsureLoaded();
            return bossById.TryGetValue(idOrAlias.Trim(), out var dto) ? dto : null;
        }

        public static BossConfigDto GetBossByFloor(int floor)
        {
            EnsureLoaded();
            if (catalog.bosses == null) return null;
            foreach (var b in catalog.bosses)
            {
                if (b != null && b.floor == floor) return b;
            }
            return null;
        }
    }
}

