using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    int emo_i = 0;

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

    public NPCVoice npcVoice;

    public int GetNextNumber(int N)
    {
        int result = currentNumber;
        currentNumber = (currentNumber + 1) % (N + 1); // Increase and reset if exceeds N
        Debug.Log(result);
        return result;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.SetFloat("speedMultiplier", speed);
    }

    void Update()
    {
        anim.SetFloat("Side", Input.GetAxis("Horizontal")); //movimento lateral com setas <- ->
        anim.SetFloat("Speed", Input.GetAxis("Vertical")); //movimento vertical com setas p cima e p baixo
    }

    public void ChangeEmotionFromSpeech(string AIResponse) //muda emo��es de acordo com a fala da IA, muda pelo npc voice
    {
        AIResponse = AIResponse.ToLower();

        //-------------------------------EMO��ES DE FELICIDADE, SUCESSO------------------------------------
        if (AIResponse.Contains("muito bem") || AIResponse.Contains("bom trabalho") ||
            AIResponse.Contains("boa performance") || AIResponse.Contains("�timo") ||
            AIResponse.Contains("excelente"))
        {
            anim.SetBool("Win", true);
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //dancinha b�sica
        if (AIResponse.Contains("mandou bem") || AIResponse.Contains("arrasou") || AIResponse.Contains("incr�vel") ||
            AIResponse.Contains("fant�stico") || AIResponse.Contains("sensacional") ||
            AIResponse.Contains("palmas pra voc�") || AIResponse.Contains("estou orgulhoso"))
        {
            animationName = "Dance0";
            robotColorManager.isRainbowCycles = true;
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //dan�a que gira gira gira
        if (AIResponse.Contains("parab�ns") || AIResponse.Contains("que maravilha") || AIResponse.Contains("voc� conseguiu") ||
            AIResponse.Contains("arrasou demais") || AIResponse.Contains("inacredit�vel") ||
            AIResponse.Contains("incr�vel demais") || AIResponse.Contains("que orgulho") || AIResponse.Contains("� isso!") ||
            AIResponse.Contains("uau"))
        {
            animationName = "Dance1";
            robotColorManager.isRainbowCycles = true;
            setEmotion(1);
            StartCoroutine(PlayAnimationMultipleTimes());
        }


        //----------------------------------------OUTRAS---------------------------------

        //N�o sabe
        if (AIResponse.Contains("eu n�o sei") || AIResponse.Contains("n�o entendi"))
        {
            anim.SetBool("DontKnow", true);
            setEmotion(3);
        }

        //Sauda��o e despedida
        if (AIResponse.Contains("ol�") || AIResponse.Contains("bem vindo") || AIResponse.Contains("oi") ||
            AIResponse.Contains("bom dia") || AIResponse.Contains("boa tarde") || AIResponse.Contains("boa noite") ||
            AIResponse.Contains("tchau") || AIResponse.Contains("at� mais") || AIResponse.Contains("at� logo") ||
            AIResponse.Contains("at� breve") || AIResponse.Contains("at� a pr�xima") || AIResponse.Contains("nos vemos em breve") ||
            AIResponse.Contains("volte sempre") || AIResponse.Contains("at� depois") || AIResponse.Contains("foi um prazer lhe ajudar") || AIResponse.Contains("foi um prazer te ajudar") ||
            AIResponse.Contains("foi um prazer ajudar") || AIResponse.Contains("tenha bons estudos!") ||
            AIResponse.Contains("seja bem-vindo") || AIResponse.Contains("seja bem-vinda"))
        {
            anim.SetBool("Hello", true);
            setEmotion(0);
        }

        //procurando
        if (AIResponse.Contains("voc� deve encontrar") || AIResponse.Contains("voc� deve descobrir") || AIResponse.Contains("voc� deve procurar"))
        {
            anim.SetBool("LookingFor", true);
            setEmotion(4);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            anim.SetBool("Talk", true); //falandooo
            ToggleObjectActiveState();
            setEmotion(0);
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

    public void Speech3End()
    {
        ToggleObjectActiveState();
        Debug.Log("Anitions is ended!");
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

        //Verifica se o NPC ainda est� falando
        if (npcVoice != null && npcVoice.isSpeaking)
        {
            anim.SetBool("Talk", true);
            setEmotion(0);
        }
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
            Debug.LogError("Target Object �� ��������!");
        }
    }

    void resetEmo()
    {
        setEmotion(0);
        anim.SetBool("reset", true);
    }

}

/* ------------------------------------------------EMO��ES/MOVIMENTA��ES RETIRADAS AT� O MOMENTO POR FALTA DE CONTEXTO P UTILIZA��O
 
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
   

//ao pressionar shift + alguma a��o de movimento, ele corre, aumenta velocidade
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


//troca de emo��o ao digitar colchete, passa de emo��o por emo��o
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


//rob� tapeado
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