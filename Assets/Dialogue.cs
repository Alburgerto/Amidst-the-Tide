using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI m_textBox;
    public TextAsset m_dialogueFile;
    public Button m_button;
    public float m_lineDelayTime;
    public float m_charDelayTime;
    public float m_lineFadeTime;
    public float m_buttonDistance;
    
    private string[] m_dialogueLines;
    private string m_lineToDisplay;
    private Button m_pressedButton = null;
    private List<Button> m_choiceButtons = new List<Button>();
    private List<string> m_choiceTexts = new List<string>();

    void Start()
    {
        m_dialogueLines = m_dialogueFile.text.Split("&".ToCharArray());
        StartCoroutine(RunDialogue());
    }

    private IEnumerator RunDialogue()
    {
        bool awaitingChoice = false; 
        for (int i = 0; i < m_dialogueLines.Length; ++i)
        {
            string displayedLine = m_dialogueLines[i];
            if (string.IsNullOrEmpty(displayedLine)) { continue; }
            if (displayedLine[1] == '\n') // Ignore newline at the beginning of the line, if any
            {
                displayedLine = displayedLine.Substring(2);
            }

            // User choices
            if (displayedLine.StartsWith("***") && displayedLine.Contains("|"))
            {
                awaitingChoice = true;
                Button button = Instantiate(m_button, GetComponentInParent<Transform>());
                Vector2 buttonPosition = button.transform.position + Vector3.down * m_choiceButtons.Count * m_buttonDistance;
                button.transform.position = buttonPosition;
                button.onClick.AddListener(() => SetNarratorText(button));
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                StartCoroutine(FadeText(true, buttonText));

                displayedLine = displayedLine.Substring(4); // Remove "***" from button text
                buttonText.text = displayedLine.Split('|')[0];
                m_choiceTexts.Add(displayedLine.Split('|')[1]);
                m_choiceButtons.Add(button);
                yield return new WaitForSeconds(0.5f); // Time in between button creation and displayings
            }
            else // Narrator lines
            {
                if (awaitingChoice) // First narrator line after finishing reading all user choice lines
                {
                    --i; // Read again narator line in order to display it after displaying response to user choice
                    awaitingChoice = false;
                    yield return MakeChoice();
                    displayedLine = m_lineToDisplay;

                    StartCoroutine(FadeText(false, m_textBox));
                    StartCoroutine(ClearButtons());
                    m_lineToDisplay = "";
                    m_pressedButton = null;
                    m_choiceTexts.Clear();
                    yield return new WaitForSeconds(1.5f);
                }
                
                m_textBox.text = displayedLine;
                yield return FadeText(true, m_textBox);
                yield return AwaitInput(); // Don't move on to next line until user presses any key

                if (i+1 < m_dialogueLines.Length && !m_dialogueLines[i+1].Contains("***")) // Don't remove text if waiting for player choice
                {
                    yield return FadeText(false, m_textBox);
                    yield return new WaitForSeconds(m_lineDelayTime); // Wait different time than m_lineDisplayTime to show a new line?
                }

            }
            
        }
        yield return TheEnd();
    }

    // Runs after every line of text has been shown
    private IEnumerator TheEnd()
    {
        yield return null;
    }

    private IEnumerator ClearButtons()
    {
        foreach (Button displayedButton in m_choiceButtons)
        {
            StartCoroutine(FadeText(false, displayedButton.GetComponentInChildren<TextMeshProUGUI>()));
        }
        yield return new WaitForSeconds(m_lineFadeTime);
        foreach (Button displayedButton in m_choiceButtons)
        {
            Destroy(displayedButton.gameObject);
        }

        m_choiceButtons.Clear();
    }

    // Set text in the narrator textbox
    private void SetNarratorText(Button l_button)
    {
        if (m_choiceButtons.Contains(l_button))
        {
            m_lineToDisplay = m_choiceTexts[m_choiceButtons.IndexOf(l_button)];
            m_pressedButton = l_button;
        }
    }

    private IEnumerator MakeChoice()
    {
        while (m_pressedButton == null)
        {
            yield return null;
        }
    }

    private IEnumerator AwaitInput()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
    }

    // Fades in/out (true/false) text from narrator textbox or from any button
    private IEnumerator FadeText(bool l_fadeIn, TextMeshProUGUI l_text)
    {
        float start = Time.time;
        float until = Time.time + m_lineFadeTime;
        float alphaFrom = l_fadeIn ? 0 : 1;
        float alphaGoal = 1 - alphaFrom;
        while (Time.time < until)
        {
            l_text.alpha = Mathf.Lerp(alphaFrom, alphaGoal, (Time.time - start) / m_lineFadeTime);
            yield return new WaitForSeconds(0.05f);
        }
        l_text.alpha = alphaGoal;
    }

}
