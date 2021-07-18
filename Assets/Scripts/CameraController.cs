using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followObj;
    public float smooth; // 差值比例
    public int playingBgmID; // 正在播放的bgmID
    public float bgmChangeSpeed; // bgm切换速度
    public AudioClip[] audios; // bgm
    private AudioSource audioController; // 音频源
    private float targetVolume; // 目标音量
    private bool chancingBgm; // 指示bgm是否正在切换
    private int targetBgmID = 0; // 正在渐变目标bgmID
    // Start is called before the first frame update
    void Start()
    {
        audioController = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 position = Vector3.Lerp(transform.position, followObj.position, smooth);
        position.z = transform.position.z;
        transform.position = position;
        BgmHandler();
    }

    void BgmHandler()
    {
        if (!chancingBgm) { return; }
        audioController.volume = Mathf.Lerp(audioController.volume, targetVolume, bgmChangeSpeed);
        if (targetVolume == 0 && audioController.volume < 0.1)
        {
            audioController.clip = audios[targetBgmID];
            audioController.Play();
            targetVolume = 1;
        }
        else if (targetVolume == 1 && audioController.volume > 0.9)
        {
            chancingBgm = false;
        }
    }

    public void TransBgm(int id)
    {
        chancingBgm = true;
        targetBgmID = id;
        targetVolume = 0;
    }
}
