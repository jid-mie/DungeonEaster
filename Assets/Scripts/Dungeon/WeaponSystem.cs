using UnityEngine;
using DungeonEater.Core;
using DungeonEater.Dungeon.Enemies;

namespace DungeonEater.Dungeon
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Sword Settings")]
        [SerializeField] private float swordBaseDamage = 20f;
        [SerializeField] private float swordReach = 1.4f;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private GameObject swordSlashPrefab;

        [Header("Bow Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private GameObject magicArrowPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float arrowSpeed = 16f;
        [SerializeField] private float bowBaseDamage = 18f;

        [Header("Combo State")]
        private int comboStep = 0;
        private float comboResetTimer = 0f;
        [SerializeField] private float comboWindow = 0.8f;

        [Header("Skill Cooldowns")]
        [SerializeField] private float skillCooldown = 3.5f;
        private float currentSkillCooldown = 0f;
        public float SkillCooldownProgress => Mathf.Clamp01(currentSkillCooldown / skillCooldown);

        private void Awake()
        {
            if (enemyLayers.value == 0)
            {
                enemyLayers = ~LayerMask.GetMask("Player", "Ignore Raycast");
            }
            if (firePoint == null)
            {
                var fp = transform.Find("FirePoint");
                if (fp != null) firePoint = fp;
            }
        }

        private void Update()
        {
            if (comboResetTimer > 0f)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f) comboStep = 0;
            }

            if (currentSkillCooldown > 0f)
            {
                currentSkillCooldown -= Time.deltaTime;
            }
        }

        // --- LIGHT ATTACK ---
        public void PerformLightAttack(WeaponType weapon, Vector2 direction)
        {
            if (weapon == WeaponType.Sword)
            {
                comboStep = (comboStep % 3) + 1;
                comboResetTimer = comboWindow;

                float damageMultiplier = (comboStep == 3) ? 1.6f : 1.0f;
                float finalDamage = swordBaseDamage * damageMultiplier;
                float knockback = comboStep == 3 ? 4f : 1.5f;

                // Đồng bộ chính xác với Frame vung kiếm của hoạt ảnh (0.1s delay)
                StartCoroutine(DelayedSwordSlash(direction, swordReach, finalDamage, knockback, 0.1f, false));
                Debug.Log($"[WeaponSystem] Kiếm Combo đòn #{comboStep}! Sát thương: {finalDamage}");
            }
            else if (weapon == WeaponType.Bow)
            {
                // Đồng bộ chính xác với Frame buông dây cung thả tên (0.18s delay)
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed, bowBaseDamage, 1, false, 0.18f));
                Debug.Log($"[WeaponSystem] Cung Bắn Nhanh! Sát thương: {bowBaseDamage}");
            }
        }

        // --- HEAVY ATTACK ---
        public void PerformHeavyAttack(WeaponType weapon, Vector2 direction)
        {
            if (weapon == WeaponType.Sword)
            {
                // Cleave chém hình cung lớn
                float heavyDamage = swordBaseDamage * 2.2f;
                StartCoroutine(DelayedSwordSlash(direction, swordReach * 1.4f, heavyDamage, 6f, 0.12f, true));
                Debug.Log($"[WeaponSystem] Kiếm Chém Cleave diện rộng! Sát thương: {heavyDamage}");
            }
            else if (weapon == WeaponType.Bow)
            {
                // Charged Shot (gồng căng dây cung buông tên ở 0.22s)
                float chargedDamage = bowBaseDamage * 2.5f;
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed * 1.3f, chargedDamage, 2, false, 0.22f));
                Debug.Log($"[WeaponSystem] Cung Gồng Bắn Mạnh! Sát thương: {chargedDamage}");
            }
        }

        // --- WEAPON SKILL ---
        public void PerformSkillAttack(WeaponType weapon, Vector2 direction)
        {
            if (currentSkillCooldown > 0f)
            {
                Debug.LogWarning($"[WeaponSystem] Kỹ năng đang hồi! Còn {currentSkillCooldown:F1}s.");
                return;
            }

            currentSkillCooldown = skillCooldown;

            if (weapon == WeaponType.Sword)
            {
                // Whirlwind Spin Slash (Lốc xoáy 360 độ sau khi xoay người)
                float skillDamage = swordBaseDamage * 2.8f;
                StartCoroutine(DelayedWhirlwind(skillDamage, 0.12f));
                Debug.Log("[WeaponSystem] Kỹ năng Kiếm: Lốc Xoáy Cuồng Phong!");
            }
            else if (weapon == WeaponType.Bow)
            {
                // Piercing Arrow (Tên xuyên thấu vô hạn)
                float skillDamage = bowBaseDamage * 3.2f;
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed * 1.5f, skillDamage, 999, true, 0.2f));
                Debug.Log($"[WeaponSystem] Kỹ năng Cung: Mũi Tên Xuyên Thấu! Sát thương: {skillDamage}");
            }
        }

        private System.Collections.IEnumerator DelayedSwordSlash(Vector2 direction, float reach, float damage, float knockback, float delay, bool isHeavy)
        {
            yield return new WaitForSeconds(delay);
            if (isHeavy)
            {
                SoundManager.Instance?.PlaySwordHeavy();
                CameraFollow.Instance?.TriggerScreenShake(0.15f, 0.2f);
            }
            else
            {
                SoundManager.Instance?.PlaySwordSlash();
            }
            ApplyMeleeDamage(direction, reach, damage, knockback);
        }

        private System.Collections.IEnumerator DelayedArrowShot(Vector2 direction, float speed, float damage, int pierce, bool isSkill, float delay)
        {
            yield return new WaitForSeconds(delay);
            SoundManager.Instance?.PlayBowShoot();
            if (isSkill)
            {
                CameraFollow.Instance?.TriggerScreenShake(0.15f, 0.2f);
            }
            SpawnArrow(direction, speed, damage, pierce, isSkill);
        }

        private System.Collections.IEnumerator DelayedWhirlwind(float damage, float delay)
        {
            yield return new WaitForSeconds(delay);
            SoundManager.Instance?.PlaySwordHeavy();
            CameraFollow.Instance?.TriggerScreenShake(0.25f, 0.35f);

            if (swordSlashPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (swordReach * 0.7f);
                    GameObject spinSlash = Instantiate(swordSlashPrefab, (Vector2)transform.position + offset, Quaternion.Euler(0, 0, angle));
                    spinSlash.transform.localScale = Vector3.one * (swordReach * 1.2f);
                }
            }

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, swordReach * 1.8f, enemyLayers);
            foreach (var col in hitEnemies)
            {
                var enemy = col.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeDamage(damage, knockbackDir * 7f);
                }
            }
        }

        private void ApplyMeleeDamage(Vector2 direction, float reach, float damage, float knockback)
        {
            Vector2 attackOrigin = (Vector2)transform.position + direction.normalized * (reach * 0.6f);

            if (swordSlashPrefab != null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                GameObject slash = Instantiate(swordSlashPrefab, attackOrigin, Quaternion.Euler(0, 0, angle));
                slash.transform.localScale = Vector3.one * (reach * 0.8f);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, reach * 0.7f, enemyLayers);

            foreach (var col in hits)
            {
                var enemy = col.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, direction.normalized * knockback);
                }
            }
        }

        private void SpawnArrow(Vector2 direction, float speed, float damage, int pierceCount, bool isSkill)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.4f, 0) + (Vector3)(direction.normalized * 0.45f);
            GameObject arrowObj = null;

            GameObject targetPrefab = (isSkill && magicArrowPrefab != null) ? magicArrowPrefab : arrowPrefab;

            if (targetPrefab != null)
            {
                arrowObj = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Fallback runtime primitive if prefab not assigned yet
                arrowObj = new GameObject("Arrow");
                arrowObj.transform.position = spawnPos;
                var prj = arrowObj.AddComponent<Projectile>();
                var col = arrowObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.2f;
                var sr = arrowObj.AddComponent<SpriteRenderer>();
                sr.color = isSkill ? Color.cyan : Color.yellow;
            }

            var projectile = arrowObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, speed, damage, pierceCount, isSkill);
            }
        }

        public void AddSwordDamageBonus(float bonus) => swordBaseDamage += bonus;
        public void AddBowDamageBonus(float bonus) => bowBaseDamage += bonus;
    }
}

