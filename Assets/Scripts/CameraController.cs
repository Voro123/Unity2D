using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followObj;
    public float smooth; // ��ֵ����
    public int playingBgmID; // ���ڲ��ŵ�bgmID
    public float bgmChangeSpeed; // bgm�л��ٶ�
    public AudioClip[] audios; // bgm
    private AudioSource audioController; // ��ƵԴ
    private float targetVolume; // Ŀ������
    private bool chancingBgm; // ָʾbgm�Ƿ������л�
    private int targetBgmID = 0; // ���ڽ���Ŀ��bgmID
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
