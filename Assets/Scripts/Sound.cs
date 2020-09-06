using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public float m_thunderVolume;
    public float m_growlVolume;
    public AudioClip[] m_thunderSounds;
    public AudioClip m_growlSound;
    public AudioClip m_meowSound;

    public AudioSource m_soundSource;

    private void Start()
    {
        StartCoroutine(PlayThunderSounds());
    }

    private IEnumerator PlayThunderSounds()
    {
        float timeBetweenThunder;
        while (true)
        {
            timeBetweenThunder = Random.Range(10, 40);

            yield return new WaitForSeconds(timeBetweenThunder);
            m_soundSource.PlayOneShot(m_thunderSounds[Random.Range(0, m_thunderSounds.Length - 1)], m_thunderVolume);
        }
    }

    public void PlayGrowlSound()
    {
        m_soundSource.PlayOneShot(m_growlSound, m_growlVolume);
    }

    public void PlayMeowSound()
    {
        m_soundSource.PlayOneShot(m_meowSound, 1);
    }
}
