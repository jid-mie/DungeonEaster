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

        [Header("Attack Cooldowns")]
        [SerializeField] private float attackCooldown = 0.28f;
        private float currentAttackCooldown = 0f;

        [Header("Combo State")]
        private int comboStep = 0;
        private float comboResetTimer = 0f;
        [SerializeField] private float comboWindow = 0.85f;

        [Header("Skill Cooldowns")]
        [SerializeField] private float skillCooldown = 3.5f;
        private float currentSkillCooldown = 0f;

        public bool CanAttack => currentAttackCooldown <= 0f;
        public bool CanUseSkill => currentSkillCooldown <= 0f;
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
            if (currentAttackCooldown > 0f)
            {
                currentAttackCooldown -= Time.deltaTime;
            }

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
        public bool PerformLightAttack(WeaponType weapon, Vector2 direction)
        {
            if (currentAttackCooldown > 0f) return false;
            currentAttackCooldown = attackCooldown;

            if (weapon == WeaponType.Sword)
            {
                comboStep = (comboStep % 3) + 1;
                comboResetTimer = comboWindow;

                float damageMultiplier = (comboStep == 3) ? 1.6f : 1.0f;
                float finalDamage = swordBaseDamage * damageMultiplier;
                float reachMultiplier = (comboStep == 3) ? 1.35f : 1.0f;
                float finalReach = swordReach * reachMultiplier;
                float knockback = comboStep == 3 ? 4.5f : 2.0f;

                StartCoroutine(DelayedSwordSlash(direction, finalReach, finalDamage, knockback, 0.08f, comboStep == 3));
                Debug.Log($"[WeaponSystem] Kiếm Combo đòn #{comboStep}! Hướng: {direction}, Sát thương: {finalDamage}");
            }
            else if (weapon == WeaponType.Bow)
            {
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed, bowBaseDamage, 1, false, 0.10f));
                Debug.Log($"[WeaponSystem] Cung Bắn Nhanh! Hướng: {direction}, Sát thương: {bowBaseDamage}");
            }

            return true;
        }

        // --- HEAVY ATTACK ---
        public bool PerformHeavyAttack(WeaponType weapon, Vector2 direction)
        {
            if (currentAttackCooldown > 0f) return false;
            currentAttackCooldown = attackCooldown * 1.3f;

            if (weapon == WeaponType.Sword)
            {
                float heavyDamage = swordBaseDamage * 2.2f;
                StartCoroutine(DelayedSwordSlash(direction, swordReach * 1.45f, heavyDamage, 6f, 0.10f, true));
                Debug.Log($"[WeaponSystem] Kiếm Chém Cleave! Hướng: {direction}, Sát thương: {heavyDamage}");
            }
            else if (weapon == WeaponType.Bow)
            {
                float chargedDamage = bowBaseDamage * 2.5f;
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed * 1.35f, chargedDamage, 2, false, 0.15f));
                Debug.Log($"[WeaponSystem] Cung Gồng Bắn Mạnh! Hướng: {direction}, Sát thương: {chargedDamage}");
            }

            return true;
        }

        // --- WEAPON SKILL ---
        public bool PerformSkillAttack(WeaponType weapon, Vector2 direction)
        {
            if (currentSkillCooldown > 0f)
            {
                Debug.LogWarning($"[WeaponSystem] Kỹ năng đang hồi! Còn {currentSkillCooldown:F1}s.");
                return false;
            }

            currentSkillCooldown = skillCooldown;
            currentAttackCooldown = 0.35f;

            if (weapon == WeaponType.Sword)
            {
                float skillDamage = swordBaseDamage * 2.8f;
                StartCoroutine(DelayedWhirlwind(skillDamage, 0.10f));
                Debug.Log("[WeaponSystem] Kỹ năng Kiếm: Lốc Xoáy Cuồng Phong 360 độ!");
            }
            else if (weapon == WeaponType.Bow)
            {
                float skillDamage = bowBaseDamage * 3.2f;
                StartCoroutine(DelayedArrowShot(direction, arrowSpeed * 1.5f, skillDamage, 999, true, 0.12f));
                Debug.Log($"[WeaponSystem] Kỹ năng Cung: Mũi Tên Xuyên Thấu! Hướng: {direction}, Sát thương: {skillDamage}");
            }

            return true;
        }

        private System.Collections.IEnumerator DelayedSwordSlash(Vector2 direction, float reach, float damage, float knockback, float delay, bool isHeavy)
        {
            yield return new WaitForSeconds(delay);
            if (isHeavy)
            {
                SoundManager.Instance?.PlaySwordHeavy();
                CameraFollow.Instance?.TriggerScreenShake(0.18f, 0.22f);
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

            // Tâm lốc xoáy đặt ngay tại thân trên / ngực nhân vật (+0.45f Y)
            Vector2 center = (Vector2)transform.position + new Vector2(0f, 0.45f);

            if (swordSlashPrefab != null)
            {
                var mySr = GetComponent<SpriteRenderer>();
                int sortOrder = (mySr != null ? mySr.sortingOrder : 1000) + 20;

                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (swordReach * 0.75f);
                    GameObject spinSlash = Instantiate(swordSlashPrefab, center + offset, Quaternion.Euler(0, 0, angle));
                    spinSlash.transform.localScale = Vector3.one * (swordReach * 1.3f);

                    var slashSr = spinSlash.GetComponent<SpriteRenderer>();
                    if (slashSr != null) slashSr.sortingOrder = sortOrder;
                }
            }

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, swordReach * 1.9f, enemyLayers);
            foreach (var col in hitEnemies)
            {
                var enemy = col.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    Vector2 knockbackDir = ((Vector2)enemy.transform.position - center).normalized;
                    if (knockbackDir == Vector2.zero) knockbackDir = Vector2.right;
                    enemy.TakeDamage(damage, knockbackDir * 7f);
                }
            }
        }

        private void ApplyMeleeDamage(Vector2 direction, float reach, float damage, float knockback)
        {
            // Điểm xuất phát đòn chém: ngang ngực nhân vật (+0.45f Y), lệch về phía vung kiếm
            float facingX = direction.x >= 0 ? 1f : -1f;
            Vector2 characterChest = (Vector2)transform.position + new Vector2(facingX * 0.25f, 0.45f);
            Vector2 attackOrigin = characterChest + direction.normalized * (reach * 0.65f);

            if (swordSlashPrefab != null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                GameObject slash = Instantiate(swordSlashPrefab, attackOrigin, Quaternion.Euler(0, 0, angle));

                // Giữ vòng cung vung chém tự nhiên từ trên xuống dưới
                float scaleX = reach * 0.85f;
                float scaleY = direction.x < 0 ? -scaleX : scaleX;
                slash.transform.localScale = new Vector3(scaleX, scaleY, 1f);

                // Ưu tiên vẽ vệt chém đè lên trên nhân vật
                var slashSr = slash.GetComponent<SpriteRenderer>();
                if (slashSr != null)
                {
                    var mySr = GetComponent<SpriteRenderer>();
                    slashSr.sortingOrder = (mySr != null ? mySr.sortingOrder : 1000) + 20;
                }
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, reach * 0.75f, enemyLayers);

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
            // Điểm bắn tên xuất phát từ bàn tay giương cung (+0.45f Y, lệch về phía tay cầm cung)
            float facingX = direction.x >= 0 ? 1f : -1f;
            Vector3 spawnPos = transform.position + new Vector3(facingX * 0.35f, 0.45f, 0f) + (Vector3)(direction.normalized * 0.4f);
            GameObject arrowObj = null;

            GameObject targetPrefab = (isSkill && magicArrowPrefab != null) ? magicArrowPrefab : arrowPrefab;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);

            if (targetPrefab != null)
            {
                arrowObj = Instantiate(targetPrefab, spawnPos, arrowRotation);
            }
            else
            {
                arrowObj = new GameObject("Arrow");
                arrowObj.transform.position = spawnPos;
                arrowObj.transform.rotation = arrowRotation;
                var prj = arrowObj.AddComponent<Projectile>();
                var col = arrowObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.2f;
                var sr = arrowObj.AddComponent<SpriteRenderer>();
                sr.color = isSkill ? Color.cyan : Color.yellow;
            }

            var arrowSr = arrowObj.GetComponent<SpriteRenderer>();
            if (arrowSr != null)
            {
                var mySr = GetComponent<SpriteRenderer>();
                arrowSr.sortingOrder = (mySr != null ? mySr.sortingOrder : 1000) + 20;
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
