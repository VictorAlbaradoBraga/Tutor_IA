using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class Rob13Ctrl : MonoBehaviour
{
    public float speed = 1.0f;
    public float run = 0;
    public float velocity = 0;
    public float fallSpeed = 10f;
    public float gravity = 20f;
    float runVelocity = 1f;

    public GameObject MouthEmo, MouthSpeech;

    public Rob13ColorManager robotColorManager;
    public EmotionChanger emotionChanger;
    // public PushDetection pushDetection;

    [Header("Repeat time for some animations")]
    public int playCount = 2; // Cyclyc Animations repeat time
    private int currentPlayCount = 0;

    private int currentNumber = 0; //
    int N = 2;

    private string animationName = "YourAnimationName";
    private bool battleIsActive = false;
    private bool isPushing = false;
    public string pushableTag = "Pushable";
    private bool isFalling = false;
    private Vector3 moveDirection;

    Animator anim;
    CharacterController controller;

    /*
     * Emotions list with ID
         0.Neutral
         1.Happy
         2.Sad
         3.Distrust
         4.Wonder
         5.Death
         6.Disgust
         7.Evil
         8.Cry
         9.Love
     */

    //itens para animações de fala e etc
    private bool isTalking = false; // Para controlar o estado da fala
    private bool isAnimationPlaying = false; // Para garantir que a animação de fala só comece após outras animações
    public TextToSpeech txtToSpeech;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.SetFloat("speedMultiplier", speed);
    }

    void Update()
    {
       // Lógica para parar a animação de fala quando o áudio terminar
        if (isTalking && !txtToSpeech.isSpeaking)
        {
            anim.SetBool("TalkInLoop", false);
            ToggleObjectActiveState();
            isAnimationPlaying = false;
            isTalking = false;
        }

    }

    public void StartTalking()
    {
        isTalking = true;  // Ativa o estado de fala
        if (!isAnimationPlaying)
        {
            isTalking = true;
            anim.SetBool("TalkInLoop", true); // <- aqui ativa a animação em loop
            ToggleObjectActiveState();
            setEmotion(0);
        }
    }

    public void StopTalking()
    {
        isTalking = false; // Desativa o estado de fala
        anim.SetBool("TalkInLoop", false);
        ToggleObjectActiveState();
        isAnimationPlaying = false; // Define que não há animação em execução
    }

    public void ChangeEmotionFromSpeech(string AIResponse) //muda emoções de acordo com a fala da IA, muda pelo npc voice
    {
        AIResponse = AIResponse.ToLower();


        //Saudação e despedida
        if (ContainsInFirstSentence(AIResponse, "olá") || ContainsInFirstSentence(AIResponse, "bem vindo") || ContainsInFirstSentence(AIResponse, "oi") ||
            ContainsInFirstSentence(AIResponse, "bom dia") || ContainsInFirstSentence(AIResponse, "boa tarde") || ContainsInFirstSentence(AIResponse, "boa noite") ||
            ContainsInFirstSentence(AIResponse, "tchau") || ContainsInFirstSentence(AIResponse, "até mais") || ContainsInFirstSentence(AIResponse, "até logo") ||
            ContainsInFirstSentence(AIResponse, "até breve") || ContainsInFirstSentence(AIResponse, "até a próxima") || ContainsInFirstSentence(AIResponse, "nos vemos em breve") ||
            ContainsInFirstSentence(AIResponse, "volte sempre") || ContainsInFirstSentence(AIResponse, "até depois") || ContainsInFirstSentence(AIResponse, "foi um prazer lhe ajudar") || ContainsInFirstSentence(AIResponse, "foi um prazer te ajudar") ||
            ContainsInFirstSentence(AIResponse, "foi um prazer ajudar") || ContainsInFirstSentence(AIResponse, "tenha bons estudos!") ||
            ContainsInFirstSentence(AIResponse, "seja bem-vindo") || ContainsInFirstSentence(AIResponse, "seja bem-vinda"))
        {
            anim.SetBool("Hello", true);
            setEmotion(0);
        }

        //-------------------------------EMOÇÕES DE FELICIDADE, SUCESSO------------------------------------
        if (AIResponse.Contains("muito bem") || AIResponse.Contains("bom trabalho") ||
            AIResponse.Contains("boa performance") || AIResponse.Contains("ótimo") ||
            AIResponse.Contains("excelente"))
        {
            anim.SetBool("Win", true);
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //dancinha básica
        if (AIResponse.Contains("mandou bem") || AIResponse.Contains("arrasou") || AIResponse.Contains("incrível") ||
            AIResponse.Contains("fantástico") || AIResponse.Contains("sensacional") ||
            AIResponse.Contains("palmas pra você") || AIResponse.Contains("estou orgulhoso"))
        {
            animationName = "Dance0";
            robotColorManager.isRainbowCycles = true;
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //dança que gira gira gira
        if (AIResponse.Contains("parabéns") || AIResponse.Contains("que maravilha") || AIResponse.Contains("você conseguiu") ||
            AIResponse.Contains("arrasou demais") || AIResponse.Contains("inacreditável") ||
            AIResponse.Contains("incrível demais") || AIResponse.Contains("que orgulho") || AIResponse.Contains("é isso!") ||
            AIResponse.Contains("uau"))
        {
            animationName = "Dance1";
            robotColorManager.isRainbowCycles = true;
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //----------------------------------------OUTRAS---------------------------------

        //Não sabe
        if (AIResponse.Contains("eu não sei") || AIResponse.Contains("não entendi"))
        {
            anim.SetBool("DontKnow", true);
            setEmotion(3);
        }

        //procurando
        if (AIResponse.Contains("você deve encontrar") || AIResponse.Contains("você deve descobrir") || AIResponse.Contains("você deve procurar"))
        {
            anim.SetBool("LookingFor", true);
            setEmotion(4);
        }

        //negando
        if (AIResponse.Contains("não é bem isso, mas você está no caminho certo!") ||
            AIResponse.Contains("não é bem isso, vamos revisar juntos.") ||
            AIResponse.Contains("boa tentativa, mas acho que você pode chegar mais perto com uma dica.") ||
            AIResponse.Contains("ainda não acertou, mas continue") ||
            AIResponse.Contains("ainda não acertou, mas continue tentando! posso te dar uma pista.") ||
            AIResponse.Contains("errado, mas tudo bem!") ||
            AIResponse.Contains("quase!") ||
            AIResponse.Contains("isso não está exatamente certo") ||
            AIResponse.Contains("não exatamente") ||
            AIResponse.Contains("não está certo") ||
            AIResponse.Contains("não foi dessa vez") ||
            AIResponse.Contains("tem um erro") ||
            AIResponse.Contains("resposta incorreta") ||
            AIResponse.Contains("não confere"))
        {
            anim.SetBool("No", true);
        }


    }

    public void setEmotion(int emoNumber)
    {
        if (battleIsActive)
        {
            emoNumber = 7;
        }
        robotColorManager.ChangeBodyColor(emoNumber);
        emotionChanger.SetEmotionEyes(emoNumber);
        emotionChanger.SetEmotionMouth(emoNumber);
    }

    IEnumerator PlayAnimationMultipleTimes()
    {
        for (int i = 0; i < playCount; i++)
        {
            anim.SetBool(animationName, true);
            yield return new WaitForSeconds(playCount);
        }
        anim.SetBool(animationName, false);
        robotColorManager.isRainbowCycles = false;
        anim.SetBool("reset", true);
        resetEmo();
        Debug.Log("Animation Done");
    }

    void ToggleObjectActiveState()
    {
        if ((MouthEmo != null) && (MouthSpeech != null))
        {
            bool isActive = MouthEmo.activeSelf;
            bool isActiveS = MouthSpeech.activeSelf;
            MouthEmo.SetActive(!isActive);
            MouthSpeech.SetActive(!isActiveS);
        }
        else
        {
            Debug.LogError("Target Object íå íàçíà÷åí!");
        }
    }

    void resetEmo()
    {
        setEmotion(0);
        anim.SetBool("reset", true);
    }

    //p pegar a primeira sentença de uma frase p saber se executa o aceno de tchau e oi
    public bool ContainsInFirstSentence(string text, string word)
    {
        string[] sentences = text.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        if (sentences.Length > 0)
        {
            return sentences[0].Contains(word);
        }

        return false;
    }

}

/* ------------------------------------------------EMOÇÕES/MOVIMENTAÇÕES RETIRADAS ATÉ O MOMENTO POR FALTA DE CONTEXTO P UTILIZAÇÃO
 
//raiva
if (Input.GetKeyDown(KeyCode.Alpha1))
{
     anim.SetBool("Angry", true);
     setEmotion(7);
}

//chorando
if (Input.GetKeyDown(KeyCode.Alpha2))
{
     anim.SetBool("Cry", true);
     setEmotion(8);
}

//apaixonado
if (Input.GetKeyDown(KeyCode.Alpha3))
{
     anim.SetBool("Thumb", true);
     setEmotion(9);
}
      
//rindo
if (Input.GetKeyDown(KeyCode.Alpha7))
{
      anim.SetBool("Laught", true);
      setEmotion(1);
}
   

//ao pressionar shift + alguma ação de movimento, ele corre, aumenta velocidade
if (Input.GetKey(KeyCode.LeftShift) && (run < 1))
{
     run += Time.deltaTime * runVelocity;
     anim.SetFloat("run", run);
}
else
{
    if (run > 0)
    {
         run -= Time.deltaTime * runVelocity;
    }
    anim.SetFloat("run", run);
}


if (pushDetection != null && pushDetection.isPushing)
{
    if (Input.GetAxis("Vertical")>0.1)
    {
        anim.SetBool("Push", true);
        setEmotion(6);
    }
    if (Input.GetAxis("Vertical") < 0.1)
    {
        anim.SetBool("Push", false);
        resetEmo();
    }
}


//troca de emoção ao digitar colchete, passa de emoção por emoção
if (Input.GetKeyDown(KeyCode.RightBracket))
{
    emo_i++;
    if (emo_i == 10) { emo_i = 0; }
    setEmotion(emo_i);
}
if (Input.GetKeyDown(KeyCode.LeftBracket))
{
    emo_i--;
    if (emo_i == 0) { emo_i = 10; }
    setEmotion(emo_i - 1);
}


//pular
if (Input.GetKeyDown(KeyCode.Space))
{
    anim.SetBool("Jump", true);
    //anim.SetInteger("vary", GetNextNumber(2));
}

//morrer e cair
if (Input.GetKeyDown(KeyCode.U))
{
    anim.SetBool("FallFront", true);
    setEmotion(5);
}
if (Input.GetKeyDown(KeyCode.I))
{
    anim.SetBool("FallBack", true);
    setEmotion(5);
}


//robô tapeado
if (Input.GetKeyDown(KeyCode.H))
{
     anim.SetBool("Hit", true);
     anim.SetInteger("vary", GetNextNumber(3));
     setEmotion(0);
}

//anda de ladinho p esquerda
if (Input.GetKeyDown(KeyCode.Q))
{
            anim.SetBool("StrafeLeft", true);
}
if (Input.GetKeyUp(KeyCode.Q))
{
            anim.SetBool("StrafeLeft", false);
}

//anda de ladinho p direita
if (Input.GetKeyDown(KeyCode.E))
{
     anim.SetBool("StrafeRight", true);
}
if (Input.GetKeyUp(KeyCode.E))
{
    anim.SetBool("StrafeRight", false);
}
*/