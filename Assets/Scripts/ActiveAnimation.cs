using UnityEngine;

public class ActiveAnimation : MonoBehaviour
{
    [SerializeField] private Animator[] itemAnimators;
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải Player không và đã kích hoạt chưa
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // Đánh dấu đã kích hoạt


            // Dùng vòng lặp 'foreach' để ra lệnh cho TẤT CẢ các Animator
            foreach (Animator anim in itemAnimators)
            {
                // Kích hoạt cùng một Trigger (ví dụ: "Activate")
                anim.SetTrigger("Activate");
            }
        }
    }
}
