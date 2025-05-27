using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class NPCVoice : MonoBehaviour
{
    [Header("Configurações da IA")]
    string apiKey = "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";
    public string model = "llama3-70b-8192";
    public TextToSpeech textToSpeech;
    public ChatBox chatBox;

    [System.Serializable]
    private class AIRequest
    {
        public string model;
        public AIMessage[] messages;
    }

    [System.Serializable]
    private class AIMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class AIResponse
    {
        public AIChoice[] choices;
    }

    [System.Serializable]
    private class AIChoice
    {
        public AIMessage message;
    }
    void Start()
    {
        if (chatBox == null)
        {
            chatBox = FindAnyObjectByType<ChatBox>();
        }
    }

    public void StartSpeechToIAProcess(string recognizedSpeech)
    {
        if (!string.IsNullOrEmpty(recognizedSpeech))
        {
            StartCoroutine(SendToIA(recognizedSpeech));
        }
    }

    IEnumerator SendToIA(string inputText)
    {
        string apiUrl = "https://api.groq.com/openai/v1/chat/completions";


        var requestData = new AIRequest
        {
            model = model,
            messages = new AIMessage[] {
                new AIMessage { role = "user", content = inputText }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro na IA: {request.error}");
            chatBox.AddAIResponse("Desculpe, tive um problema...");
        }
        else
        {
            string respostaIA = ParseAIResponse(request.downloadHandler.text);
            chatBox.AddAIResponse(respostaIA);
            PlayText(respostaIA);
        }
    }

    private string ParseAIResponse(string json)
    {
        try
        {
            AIResponse response = JsonUtility.FromJson<AIResponse>(json);
            return response?.choices?[0]?.message?.content ?? "Não entendi sua pergunta...";
        }
        catch
        {
            return "Houve um erro ao processar a resposta...";
        }
    }

    private void PlayText(string text)
    {
        if (textToSpeech != null)
        {
            textToSpeech.Speak(text);
        }
    }
}