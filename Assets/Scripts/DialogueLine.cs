using UnityEngine;
[System.Serializable]
public struct DialogueLine
{
    public string speakerName; // Tên người nói (ví dụ: "Kael")

    // Đánh dấu xem có phải là Người nói A không (người bên trái)
    public bool isSpeakerA;

    [TextArea(3, 10)] // Giúp ô gõ text trong Inspector to hơn
    public string lineText; // Nội dung câu thoại  
}
