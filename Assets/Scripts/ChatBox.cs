using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ChatBox : MonoBehaviour
{
    public int maxMessages = 25;
    public GameObject chatPanel;
    public GameObject textObject; // Prefab b�sico de texto
    public TMP_InputField chatField;

    [SerializeField]
    private List<Message> messageList = new List<Message>();
    private NPCVoice npcVoice;

    void Start()
    {
        npcVoice = FindAnyObjectByType<NPCVoice>();
        if (npcVoice == null)
        {
            Debug.LogError("NPCVoice n�o encontrado na cena!");
        }
    }

    void Update()
    {
        if (chatField.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            string userMessage = chatField.text;
            AddMessageToChat("Voc�: " + userMessage, Color.cyan); // Mensagem do usu�rio em azul
            chatField.text = "";

            // Envia para a IA processar
            npcVoice.StartSpeechToIAProcess(userMessage);
        }
    }

    // M�todo p�blico para adicionar mensagens de qualquer origem
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



    // M�todo para a IA enviar respostas
    public void AddAIResponse(string response)
    {
        AddMessageToChat("IA: " + response, Color.white); // Mensagem da IA em verde
    }
    // Retorna o hist�rico de mensagens em formato para a IA
    public List<NPCVoice.AIMessage> GetFormattedMessages()
    {
        List<NPCVoice.AIMessage> aiMessages = new List<NPCVoice.AIMessage>();

        foreach (Message msg in messageList)
        {
            if (msg.text.StartsWith("Voc�: "))
            {
                aiMessages.Add(new NPCVoice.AIMessage
                {
                    role = "user",
                    content = msg.text.Substring(6) // Remove "Voc�: "
                });
            }
            else if (msg.text.StartsWith("IA: "))
            {
                aiMessages.Add(new NPCVoice.AIMessage
                {
                    role = "assistant",
                    content = msg.text.Substring(4) // Remove "IA: "
                });
            }
        }

        return aiMessages;
    }

}

[System.Serializable]
public class Message
{
    public string text;
    public TMP_Text textObject;
}