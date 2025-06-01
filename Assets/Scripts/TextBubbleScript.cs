using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TextBubbleScript : MonoBehaviour
{
    public TextMeshProUGUI textBubble;
    [TextArea] public string[] fullText;
    public float delay = 0.05f;
    private int textCount = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (fullText.Length > 0)
        {
            typingCoroutine = StartCoroutine(ShowText(fullText[textCount]));
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isTyping)
            {
                // Skip typing, show full text instantly
                StopCoroutine(typingCoroutine);
                textBubble.text = fullText[textCount];
                isTyping = false;
            }
            else
            {
                // Move to next line
                textCount++;
                if (textCount < fullText.Length)
                {
                    typingCoroutine = StartCoroutine(ShowText(fullText[textCount]));
                }
                else
                {
                    SceneManager.LoadScene("MainScene"); // need to build webgl scenes first
                }
            }
        }
    }
    IEnumerator ShowText(string line)
    {
        isTyping = true;
        textBubble.text = "";
        foreach (char c in line)
        {
            textBubble.text += c;
            yield return new WaitForSeconds(delay);
        }
        isTyping = false;
    }
}
