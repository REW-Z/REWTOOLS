
#if REW_LEGACY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGM : MonoBehaviour
{
    private AudioSource audioSource;

    public string bgmName;

    private void Awake()
    {
        this.audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        //设置初始BGM
        if(audioSource .clip == null)
        {
            audioSource.clip = ResourceBundle.Load<AudioClip>("Sounds/" + bgmName);
            audioSource.Play();
        }

        //音量初始化
        if(SoundPlayer.BGMOn)
        {
            this.GetComponent<AudioSource>().volume = 1f;
        }
        else
        {
            this.GetComponent<AudioSource>().volume = 0f;
        }
    }
}


#endif