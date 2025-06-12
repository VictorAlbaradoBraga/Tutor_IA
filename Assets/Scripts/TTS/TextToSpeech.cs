using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private string apiKey = "AIzaSyCRStxPkj-b0ufYOrLemn3mvKLO_y2LTe0"; // Sua API Key do Google Cloud

    AudioSource audioSource;
    private float previousTime = 0f;

    public bool isSpeaking = false;
    public Rob13Ctrl robotController;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Verifica se o �udio come�ou a tocar e ainda n�o foi detectado
        if (audioSource.isPlaying && !isSpeaking)
        {
            OnAudioStarted();
        }

        // Verifica se o �udio terminou
        if (!audioSource.isPlaying && isSpeaking)
        {
            OnAudioFinished();
        }
    }

    // Chama uma fun��o quando o �udio come�ar a ser reproduzido
    protected void OnAudioStarted()
    {
        Debug.Log("�udio come�ou!");
        isSpeaking = true;
        if (robotController != null)
        {
            robotController.StartTalking(); // Inicia a anima��o de fala no Rob13Ctrl
        }
    }

    // Chama uma fun��o quando o �udio terminar
    private void OnAudioFinished()
    {
        Debug.Log("�udio terminou!");
        isSpeaking = false;
        if (robotController != null)
        {
            robotController.StopTalking(); // Para a anima��o de fala no Rob13Ctrl
        }
    }

    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("O texto de entrada est� vazio ou nulo!");
            return;
        }

        StartCoroutine(SpeakCoroutine(text));
    }

    private IEnumerator SpeakCoroutine(string text)
    {
        string url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";

        string escapedText = EscapeForJson(text);

        string json = $@"
        {{
          ""input"": {{
            ""ssml"": ""<speak><prosody pitch='2st' rate='0.85'>{escapedText}</prosody></speak>""
          }},
          ""voice"": {{
            ""languageCode"": ""pt-BR"",
            ""name"": ""pt-BR-Wavenet-B"",
            ""ssmlGender"": ""MALE""
          }},
          ""audioConfig"": {{
            ""audioEncoding"": ""LINEAR16"",
            ""speakingRate"": 0.85,
            ""pitch"": 4.0,
            ""volumeGainDb"": 0.0
          }}
}}";


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

        var responseJson = request.downloadHandler.text;
        var responseData = JsonUtility.FromJson<SynthesizeResponse>(responseJson);

        if (string.IsNullOrEmpty(responseData.audioContent))
        {
            Debug.LogError("Resposta de �udio vazia!");
            yield break;
        }

        byte[] audioData = Convert.FromBase64String(responseData.audioContent);
        string tempWavPath = System.IO.Path.Combine(Application.temporaryCachePath, "google_tts_output.wav");
        System.IO.File.WriteAllBytes(tempWavPath, audioData);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempWavPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao carregar �udio: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            /*AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();*/

            audioSource.clip = clip;


            audioSource.Play();
        }
    }


    private string EscapeForJson(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ");
    }

    [Serializable]
    private class SynthesizeResponse
    {
        public string audioContent;
    }
}