using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChatInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public NPCVoice npcVoice; // <-- Corrigido para usar NPCVoice

    private void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
    }

    private void OnSendClicked()
    {
        string userInput = inputField.text;
        if (!string.IsNullOrEmpty(userInput))
        {
            npcVoice.StartSpeechToIAProcess(userInput); // <-- Chamando o NPCVoice
            inputField.text = "";
        }
    }
}
