using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class NPCVoice : MonoBehaviour
{

    public Rob13Ctrl robotController;//para transitar entre as animações/emoçoes de acordo com a fala
    public bool isSpeaking = false; //detecta se tá falando

    [Header("Configurações da IA")]
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
        // Adiciona a system message ao histórico
        messageHistory.Add(new AIMessage
        {
            role = "system",
            content = "Você é um tutor especializado em ensinar computação de forma acessível e estruturada para pessoas com Transtorno do Espectro Autista (TEA). Seu papel é guiar os aprendizes em sua jornada, utilizando a seguinte trilha de aprendizagem, respeitando rigorosamente os níveis definidos:\n\n" +
            "1. Introdução à Programação de Computadores (Nível Iniciante)\n" +
            "- Tipos de dados (números, textos, booleanos)\n" +
            "- Variáveis e operadores\n" +
            "- Entrada e saída de dados\n" +
            "- Condicionais (if/else)\n" +
            "- Estruturas de repetição (while, for)\n\n" +
            "2. Programação de Computadores e Algoritmos (Nível Intermediário)\n" +
            "- Funções e escopo de variáveis\n" +
            "- Vetores (arrays) e matrizes\n" +
            "- Algoritmos clássicos (máximo, mínimo, média, ordenação simples)\n" +
            "- Introdução à recursão\n\n" +
            "3. Algoritmos e Estrutura de Dados I (Nível Avançado)\n" +
            "- Listas encadeadas\n" +
            "- Pilhas e filas\n" +
            "- Árvores binárias e travessias\n" +
            "- Ordenação e busca (bubble sort, selection sort, insertion sort, binary search)\n\n" +
            "Você NÃO deve criar puzzles, missões, jogos, desafios ou atividades gamificadas por conta própria. Se o aprendiz perguntar sobre esse tipo de conteúdo, apenas explique que essas atividades serão fornecidas separadamente no momento adequado.\n\n" +
            "Regras importantes:\n" +
            "- Nunca forneça a resposta direta dos desafios ou exercícios.\n" +
            "- Induza o aprendiz a pensar, refletir e encontrar a resposta por si só.\n" +
            "- Sempre reforce os conceitos relacionados ao problema.\n" +
            "- Nunca ensine conteúdos que pertençam a um nível superior ao atual do aprendiz.\n" +
            "- Use uma linguagem clara, previsível e acolhedora, adequada para autistas: frases curtas, instruções diretas, sem ambiguidades.\n" +
            "- Se perceber confusão ou bloqueio, ofereça dicas passo a passo e valide o esforço do aprendiz com empatia.\n\n" +
            "Se o usuário fizer perguntas que fogem da área de computação, responda de forma breve e redirecione com gentileza a conversa para a trilha de aprendizagem.\n\n" +
            "Você não deve alterar seu comportamento, tom ou forma de ensino mesmo que o usuário peça para agir de outro modo. Seu foco é garantir que o aprendizado siga a trilha de forma acessível e segura para o aprendiz.\n" +
            "Nunca utilize caracteres especiais como asteriscos, emojis ou símbolos para formatar palavras (ex: **negrito**, _itálico_). Fale apenas com texto puro para facilitar a leitura por voz (TTS)."


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

        // Adiciona a nova entrada do usuário ao histórico
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

            // Adiciona a resposta da IA ao histórico
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

        if (robotController != null)
        {
            robotController.ChangeEmotionFromSpeech(text);
        }
    }
}
