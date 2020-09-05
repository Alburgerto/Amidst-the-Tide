using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class Game : MonoBehaviour
{
    public TextMeshProUGUI m_textBox;
    public Image m_blackPanel;
    public TextAsset m_dialogueFile;
    public Transform m_boat;
    public Button m_button;
    public Vector2 m_boatInitialGoalPosition;
    public Vector2 m_boatFinalGoalPosition;

    public float m_zoomScaleSpeed;
    public float m_zoomPositionSpeed;
    public float m_fadePanelTime;
    public float m_boatInitialGoalScale;
    public float m_boatFinalGoalScale;
    public float m_lineDelayTime;
    public float m_charDelayTime;
    public float m_lineFadeTime;
    public float m_buttonDistance;
    public float m_zoomTime;
    public float m_initialZoomTime;

    private bool m_zoomCoroutineRunning;
    private float m_boatStartYPosition;
    private float m_boatStartScale;
    private int m_narratorLines; // Zoom boat in/out a fraction per narrator line displayed
    private string[] m_dialogueLines;
    private string m_lineToDisplay;
    private Button m_pressedButton = null;
    private List<Button> m_choiceButtons = new List<Button>();
    private List<string> m_choiceTexts = new List<string>();

    void Start()
    {
        m_zoomCoroutineRunning = false;
        m_boatStartYPosition = m_boat.position.y;
        m_boatStartScale = m_boat.localScale.x;
        SetupLines();

        StartCoroutine(RunGame());
    }

    private void SetupLines()
    {
        m_dialogueLines = m_dialogueFile.text.Split("&".ToCharArray()).Where(line => !string.IsNullOrEmpty(line)).ToArray(); // Filters empty lines

        bool inputLine = false;
        for (int i = 0; i < m_dialogueLines.Length; ++i)
        {
            if (m_dialogueLines[i][1] == '\n') // Ignore newline at the beginning of the line, if any
            {
                m_dialogueLines[i] = m_dialogueLines[i].Substring(2);
            }

            if (m_dialogueLines[i].StartsWith("***"))
            {
                if (inputLine) { continue; }
                inputLine = true;
            }
            else
            {
                inputLine = false;
            }
            ++m_narratorLines;
        }
    }

    private bool IsNotNullOrEmpty(string l_line)
    {
        return string.IsNullOrEmpty(l_line);
    }

    private IEnumerator RunGame()
    {
        //yield return new WaitForSeconds(2.5f);
        yield return FadeOutPanel(m_fadePanelTime);
        //yield return new WaitForSeconds(5);
        yield return TimedZoomOnBoat(m_boatInitialGoalPosition, m_boatInitialGoalScale, m_initialZoomTime);

        yield return RunDialogue();
        yield return TheEnd();
    }

    private IEnumerator RunDialogue()
    {
        bool awaitingChoice = false;
        int narratorLinesDisplayed = 0;
        for (int i = 0; i < m_dialogueLines.Length; ++i)
        {
            string displayedLine = m_dialogueLines[i];
            
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
                if (m_zoomCoroutineRunning)
                {
                    //StopCoroutine(ZoomOnBoat(goalScale, goalXPosition, goalYPosition, m_zoomTime));
                    StopCoroutine(ZoomOnBoat(narratorLinesDisplayed));
                }
                StartCoroutine(ZoomOnBoat(narratorLinesDisplayed));
                yield return FadeText(true, m_textBox);
                ++narratorLinesDisplayed;
                yield return new WaitForSeconds(m_lineDelayTime);
                yield return AwaitInput(); // Don't move on to next line until user presses any key

                if (i+1 < m_dialogueLines.Length && !m_dialogueLines[i+1].Contains("***")) // Don't remove text if waiting for player choice
                {
                    yield return FadeText(false, m_textBox);
                    yield return new WaitForSeconds(m_lineDelayTime); // Wait different time than m_lineDisplayTime to show a new line?
                }
            }
        }
    }

    // Zooms out of the boat/character reducing scale and modifying Y position
    private IEnumerator ZoomOnBoat(int l_narratorLine)
    {
        m_zoomCoroutineRunning = true;
        Vector3 initialPosition = m_boat.position;
        float completion = (float)l_narratorLine / m_narratorLines;
        float goalScale = Mathf.Lerp(m_boat.localScale.x, m_boatFinalGoalScale, completion);
        Vector3 goalPosition = Vector3.Lerp(initialPosition, m_boatFinalGoalPosition, completion);

        while (m_boat.localScale.x > goalScale || m_boat.position.x > goalPosition.x || m_boat.position.y > goalPosition.y)
        {
            if (m_boat.localScale.x > goalScale)
            {
                m_boat.localScale = Vector3.MoveTowards(m_boat.localScale, new Vector3(goalScale, goalScale, 1), Time.deltaTime * m_zoomScaleSpeed);
            }
            if (m_boat.position.x > goalPosition.x || m_boat.position.y > goalPosition.y)
            {
                Vector3 newPosition = new Vector3(goalPosition.x, goalPosition.y, initialPosition.z);
                m_boat.position = Vector3.MoveTowards(m_boat.position, newPosition, Time.deltaTime * m_zoomPositionSpeed);
            }
            yield return new WaitForSeconds(0.1f);
        }
        m_zoomCoroutineRunning = false;
    }

    private IEnumerator TimedZoomOnBoat(Vector2 l_goalPosition, float l_goalScale, float l_time)
    {
        float time = Time.time;
        float until = time + l_time;
        float completion;
        Vector3 newPosition = new Vector3(l_goalPosition.x, l_goalPosition.y, m_boat.position.z);
        Vector3 newScale = new Vector3(l_goalScale, l_goalScale, 1);

        while (Time.time < until)
        {
            completion = (Time.time - time) / l_time * Time.deltaTime;
            m_boat.localScale = Vector3.Lerp(m_boat.localScale, newScale, completion);
            m_boat.position = Vector3.Lerp(m_boat.position, newPosition, completion);
            yield return null;
        }
    }

    private IEnumerator FadeOutPanel(float l_fadePanelTime)
    {
        float time = Time.time;
        float until = time + l_fadePanelTime;
        Color color = m_blackPanel.color;
        while (Time.time < until)
        {
            color.a = Mathf.Lerp(1, 0, (Time.time - time) / l_fadePanelTime);
            m_blackPanel.color = color;
            yield return null;
        }
        color.a = 0;
        m_blackPanel.color = color;
    }

    // Runs after every line of text has been shown
    private IEnumerator TheEnd()
    {
        Camera.main.gameObject.GetComponent<Sound>().PlayGrowlSound();
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
