using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // ⭐ Chỉ dùng Instance để đảm bảo chỉ có 1 Music Manager ⭐
    public static MusicManager Instance;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic; // Bài nhạc nền duy nhất cho toàn bộ Scene

    private AudioSource audioSource;

    void Awake()
    {
        // Đảm bảo chỉ có một thể hiện (Instance) của MusicManager
        if (Instance == null)
        {
            Instance = this;
            // Tùy chọn: DontDestroyOnLoad(gameObject); nếu muốn nhạc giữ xuyên scene
        }
        else
        {
            Destroy(gameObject);
        }

        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Phát bài nhạc nền ngay lập tức
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;    // Lặp lại liên tục
            audioSource.volume = 1.0f;  // Âm lượng đầy đủ
            audioSource.Play();
        }
    }

    // Đã loại bỏ tất cả các hàm và logic liên quan đến chuyển nhạc (Update, Fade, PlayBossMusic, v.v.)
}