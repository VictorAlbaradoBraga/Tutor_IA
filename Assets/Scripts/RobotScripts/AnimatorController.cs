using UnityEngine;
using System.Collections; // Necessário para IEnumerator

public class AnimatorController : MonoBehaviour
{
    public Animator animator;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
    }

    public void TextToAnimation(string respostaIA)
    {
        respostaIA = respostaIA.ToLower();

        if (respostaIA.Contains("oi") || respostaIA.Contains("olá") || respostaIA.Contains("tudo bem"))
        {
            animator.SetTrigger("Wave");
            StartCoroutine(ComebackToIdle(10f)); //tempo para voltar p idle
        }
        else if (respostaIA.Contains("tchau") || respostaIA.Contains("até mais") || respostaIA.Contains("até logo")
            || respostaIA.Contains("até breve"))
        {
            animator.SetTrigger("Walk");
            StartCoroutine(ComebackToIdle(15f)); // StartCoroutine roda tarefas com espera sem pausar o jogo
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }

    private IEnumerator ComebackToIdle(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Idle");
    }

}
