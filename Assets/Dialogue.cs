using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextAsset m_dialogueFile;
    public float m_lineDisplayTime;
    public float m_charDelayTime;
    public float m_lineFadeOutTime;

    private TextMeshProUGUI m_textBox;
    private string[] m_dialogueLines;


    // Start is called before the first frame update
    void Start()
    {
        m_textBox = GetComponent<TextMeshProUGUI>();
        m_dialogueLines = m_dialogueFile.text.Split('\n');
        StartCoroutine(RunDialogue());
    }

    private IEnumerator RunDialogue()
    {
        foreach (string line in m_dialogueLines)
        {
            foreach (char character in line)
            {
                m_textBox.text += character;
                yield return new WaitForSeconds(m_charDelayTime);
            }
            yield return new WaitForSeconds(m_lineDisplayTime); // WAIT FOR INPUT?
            yield return FadeOutText();
            yield return new WaitForSeconds(m_lineDisplayTime); // Wait different time than m_lineDisplayTime to show a new line?
        }
    }

    private IEnumerator FadeOutText()
    {
        float time = Time.time;
        while (m_textBox.alpha > 0)
        {
            m_textBox.alpha = Mathf.Lerp(1, 0, (Time.time - time) / m_lineFadeOutTime);
            yield return new WaitForSeconds(0.05f);
        }
        m_textBox.text = "";
        m_textBox.alpha = 1;
    }
}
