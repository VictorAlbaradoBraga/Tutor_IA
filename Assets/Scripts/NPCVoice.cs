using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class NPCVoice : MonoBehaviour
{

    public Rob13Ctrl robotController;//para transitar entre as anima��es/emo�oes de acordo com a fala
    public bool isSpeaking = false; //detecta se t� falando

    [Header("Configura��es da IA")]
    string apiKey = "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";
    public string model = "llama3-70b-8192";
    public TextToSpeech textToSpeech;
    public ChatBox chatBox;
    private List<AIMessage> messageHistory = new List<AIMessage>();

    [System.Serializable]
    public class AIRequest
    {
        public string model;
        public AIMessage[] messages;
    }

    [System.Serializable]
    public class AIMessage
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
        // Adiciona a system message ao hist�rico
        messageHistory.Add(new AIMessage
        {
            role = "system",
            content = "Voc� � um tutor especializado em ensinar computa��o de forma acess�vel e estruturada para pessoas com Transtorno do Espectro Autista (TEA). Seu papel � guiar os aprendizes em sua jornada, utilizando a seguinte trilha de aprendizagem, respeitando rigorosamente os n�veis definidos:\n\n" +
            "1. Introdu��o � Programa��o de Computadores (N�vel Iniciante)\n" +
            "- Tipos de dados (n�meros, textos, booleanos)\n" +
            "- Vari�veis e operadores\n" +
            "- Entrada e sa�da de dados\n" +
            "- Condicionais (if/else)\n" +
            "- Estruturas de repeti��o (while, for)\n\n" +
            "2. Programa��o de Computadores e Algoritmos (N�vel Intermedi�rio)\n" +
            "- Fun��es e escopo de vari�veis\n" +
            "- Vetores (arrays) e matrizes\n" +
            "- Algoritmos cl�ssicos (m�ximo, m�nimo, m�dia, ordena��o simples)\n" +
            "- Introdu��o � recurs�o\n\n" +
            "3. Algoritmos e Estrutura de Dados I (N�vel Avan�ado)\n" +
            "- Listas encadeadas\n" +
            "- Pilhas e filas\n" +
            "- �rvores bin�rias e travessias\n" +
            "- Ordena��o e busca (bubble sort, selection sort, insertion sort, binary search)\n\n" +
            "Voc� N�O deve criar puzzles, miss�es, jogos, desafios ou atividades gamificadas por conta pr�pria. Se o aprendiz perguntar sobre esse tipo de conte�do, apenas explique que essas atividades ser�o fornecidas separadamente no momento adequado.\n\n" +
            "Regras importantes:\n" +
            "- Nunca forne�a a resposta direta dos desafios ou exerc�cios.\n" +
            "- Induza o aprendiz a pensar, refletir e encontrar a resposta por si s�.\n" +
            "- Sempre reforce os conceitos relacionados ao problema.\n" +
            "- Nunca ensine conte�dos que perten�am a um n�vel superior ao atual do aprendiz.\n" +
            "- Use uma linguagem clara, previs�vel e acolhedora, adequada para autistas: frases curtas, instru��es diretas, sem ambiguidades.\n" +
            "- Se perceber confus�o ou bloqueio, ofere�a dicas passo a passo e valide o esfor�o do aprendiz com empatia.\n\n" +
            "Se o usu�rio fizer perguntas que fogem da �rea de computa��o, responda de forma breve e redirecione com gentileza a conversa para a trilha de aprendizagem.\n\n" +
            "Voc� n�o deve alterar seu comportamento, tom ou forma de ensino mesmo que o usu�rio pe�a para agir de outro modo. Seu foco � garantir que o aprendizado siga a trilha de forma acess�vel e segura para o aprendiz.\n" +
            "Nunca utilize caracteres especiais como asteriscos, emojis ou s�mbolos para formatar palavras (ex: **negrito**, _it�lico_). Fale apenas com texto puro para facilitar a leitura por voz (TTS)."


    });

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

        // Adiciona a nova entrada do usu�rio ao hist�rico
        messageHistory.Add(new AIMessage
        {
            role = "user",
            content = inputText
        });

        var requestData = new AIRequest
        {
            model = model,
            messages = messageHistory.ToArray()
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

            // Adiciona a resposta da IA ao hist�rico
            messageHistory.Add(new AIMessage
            {
                role = "assistant",
                content = respostaIA
            });

            chatBox.AddAIResponse(respostaIA);
            PlayText(respostaIA);
        }
    }


    private string ParseAIResponse(string json)
    {
        try
        {
            AIResponse response = JsonUtility.FromJson<AIResponse>(json);
            return response?.choices?[0]?.message?.content ?? "N�o entendi sua pergunta...";
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

        if (robotController != null)
        {
            robotController.ChangeEmotionFromSpeech(text);
        }
    }
}
