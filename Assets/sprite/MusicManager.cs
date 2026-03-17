using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource bgm;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            bgm = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgm.clip == clip && bgm.isPlaying) return; // 已经在放了就不重复
        bgm.clip = clip;
        bgm.loop = true;
        bgm.Play();
    }

    public void StopBGM()
    {
        bgm.Stop();
    }

    public void SetVolume(float volume)
    {
        bgm.volume = Mathf.Clamp01(volume);
    }

    public void PauseBGM()
    {
        bgm.Pause();
    }

    public void ResumeBGM()
    {
        bgm.UnPause();
    }
}