using UnityEngine;

//The fact that when using Stop Action : Destroy in Partical System 
// it only remove the component Partical System and the Audio source still in gameobject
// So we need to use this to destroy it
public class SoundAutoDestroy : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Lấy AudioSource từ GameObject
        audioSource = GetComponent<AudioSource>();

        // Phát âm thanh
        if (audioSource)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        // Kiểm tra nếu âm thanh đã ngừng
        if (audioSource && !audioSource.isPlaying)
        {
            Destroy(gameObject); // Xóa GameObject
        }
    }
}