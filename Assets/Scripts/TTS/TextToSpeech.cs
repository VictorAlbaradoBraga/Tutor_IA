using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private string apiKey = "AIzaSyCRStxPkj-b0ufYOrLemn3mvKLO_y2LTe0"; // Insira sua API Key do Google Cloud aqui

    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("O texto de entrada está vazio ou nulo!");
            return;
        }

        Debug.Log("Texto a ser enviado: " + text);
        StartCoroutine(SpeakCoroutine(text));
    }

    private IEnumerator SpeakCoroutine(string text)
    {
        string url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";

        // Cria o objeto de dados para envio
        DataToSend dataToSend = new DataToSend
        {
            input = new Input { text = text },
            voice = new Voice
            {
                languageCode = "pt-BR",
                name = "pt-BR-Neural2-B",
                ssmlGender = "MALE"
            },
            audioConfig = new AudioConfig
            {
                audioEncoding = "LINEAR16" // WAV
            }
        };

        string json = JsonUtility.ToJson(dataToSend);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro no TTS: {request.error}\n{request.downloadHandler.text}");
            yield break;
        }

        // Parse da resposta
        var responseJson = request.downloadHandler.text;
        var responseData = JsonUtility.FromJson<SynthesizeResponse>(responseJson);

        if (string.IsNullOrEmpty(responseData.audioContent))
        {
            Debug.LogError("Resposta de áudio vazia!");
            yield break;
        }

        // Decodifica Base64
        byte[] audioData = Convert.FromBase64String(responseData.audioContent);

        // Salva temporariamente
        string tempWavPath = System.IO.Path.Combine(Application.temporaryCachePath, "google_tts_output.wav");
        System.IO.File.WriteAllBytes(tempWavPath, audioData);

        // Carrega o WAV
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempWavPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao carregar áudio: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = clip;
            LipSyncFromClip lipsync = GetComponent<LipSyncFromClip>();
            if (lipsync != null)
            {
                lipsync.StartLipSync(clip); // passa o áudio direto pra gerar o movimento!
            }

            audioSource.Play();
        }
    }

    [Serializable]
    private class DataToSend
    {
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;
    }

    [Serializable]
    private class Input
    {
        public string text;
    }

    [Serializable]
    private class Voice
    {
        public string languageCode;
        public string name;
        public string ssmlGender;
    }

    [Serializable]
    private class AudioConfig
    {
        public string audioEncoding;
    }

    [Serializable]
    private class SynthesizeResponse
    {
        public string audioContent;
    }
}
