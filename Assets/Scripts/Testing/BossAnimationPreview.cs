using UnityEngine;

namespace DungeonEater
{
    /// <summary>
    /// Gắn vào Boss_KingBoar để xem trước từng animation bằng nút bấm trên màn hình.
    /// Phím tắt: 1=Idle, 2=Stomp, 3=Charge, 4=Defeat
    /// </summary>
    public class BossAnimationPreview : MonoBehaviour
    {
        private Animator animator;
        private string currentAnim = "Idle";

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) PlayAnim("Idle");
            if (Input.GetKeyDown(KeyCode.Alpha2)) PlayAnim("Stomp");
            if (Input.GetKeyDown(KeyCode.Alpha3)) PlayAnim("Charge");
            if (Input.GetKeyDown(KeyCode.Alpha4)) PlayAnim("Defeat");
        }

        private void PlayAnim(string stateName)
        {
            if (animator == null) return;
            currentAnim = stateName;

            switch (stateName)
            {
                case "Idle":
                    animator.Play("Idle", 0, 0f);
                    break;
                case "Stomp":
                    animator.SetTrigger("stomp");
                    break;
                case "Charge":
                    animator.SetTrigger("charge");
                    break;
                case "Defeat":
                    animator.SetTrigger("defeat");
                    break;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 400, 180), GUI.skin.box);
            GUILayout.Label("<b>🐗 BOSS KING BOAR - Animation Preview</b>");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUI.color = currentAnim == "Idle" ? Color.cyan : Color.white;
            if (GUILayout.Button("[1] Idle\nThở Phì Phò", GUILayout.Height(40)))
                PlayAnim("Idle");

            GUI.color = currentAnim == "Stomp" ? Color.red : Color.white;
            if (GUILayout.Button("[2] Stomp\nDậm Đất Nổ", GUILayout.Height(40)))
                PlayAnim("Stomp");

            GUI.color = currentAnim == "Charge" ? Color.yellow : Color.white;
            if (GUILayout.Button("[3] Charge\nHúc Càn", GUILayout.Height(40)))
                PlayAnim("Charge");

            GUI.color = currentAnim == "Defeat" ? Color.gray : Color.white;
            if (GUILayout.Button("[4] Defeat\nGục Ngã", GUILayout.Height(40)))
                PlayAnim("Defeat");

            GUILayout.EndHorizontal();

            GUI.color = Color.white;
            GUILayout.Space(5);
            string animColor = currentAnim switch
            {
                "Stomp" => "red",
                "Charge" => "yellow",
                "Defeat" => "gray",
                _ => "cyan"
            };
            GUILayout.Label($"<b>Đang phát:</b> <color={animColor}>{currentAnim}</color>");
            GUILayout.Label("<i>Bấm phím [1] [2] [3] [4] hoặc nút trên để đổi animation</i>");
            GUILayout.EndArea();
        }
    }
}

