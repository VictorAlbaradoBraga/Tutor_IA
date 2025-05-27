using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ChatBox : MonoBehaviour
{
    public int maxMessages = 25;
    public GameObject chatPanel;
    public GameObject textObject; // Prefab básico de texto
    public TMP_InputField chatField;

    [SerializeField]
    private List<Message> messageList = new List<Message>();
    private NPCVoice npcVoice;

    void Start()
    {
        npcVoice = FindAnyObjectByType<NPCVoice>();
        if (npcVoice == null)
        {
            Debug.LogError("NPCVoice não encontrado na cena!");
        }
    }

    void Update()
    {
        if (chatField.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            string userMessage = chatField.text;
            AddMessageToChat("Você: " + userMessage, Color.cyan); // Mensagem do usuário em azul
            chatField.text = "";

            // Envia para a IA processar
            npcVoice.StartSpeechToIAProcess(userMessage);
        }
    }

    // Método público para adicionar mensagens de qualquer origem
    public void AddMessageToChat(string text, Color color)
    {
        if (messageList.Count >= maxMessages)
        {
            DestroyOldestMessage();
        }

        GameObject newTextObj = Instantiate(textObject, chatPanel.transform);
        TMP_Text textComponent = newTextObj.GetComponent<TMP_Text>();

        textComponent.text = text;
        textComponent.color = color;

        Message newMessage = new Message
        {
            text = text,
            textObject = textComponent
        };

        messageList.Add(newMessage);
        
    }

    private void DestroyOldestMessage()
    {
        if (messageList.Count == 0) return;

        Destroy(messageList[0].textObject.gameObject);
        messageList.RemoveAt(0);
    }



    // Método para a IA enviar respostas
    public void AddAIResponse(string response)
    {
        AddMessageToChat("IA: " + response, Color.white); // Mensagem da IA em verde
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public TMP_Text textObject;
}