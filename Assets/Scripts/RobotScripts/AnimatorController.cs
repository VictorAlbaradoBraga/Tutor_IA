using UnityEngine;
using System.Collections; // Necess�rio para IEnumerator

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

        if (respostaIA.Contains("oi") || respostaIA.Contains("ol�") || respostaIA.Contains("tudo bem"))
        {
            animator.SetTrigger("Wave");
            StartCoroutine(ComebackToIdle(10f)); //tempo para voltar p idle
        }
        else if (respostaIA.Contains("tchau") || respostaIA.Contains("at� mais") || respostaIA.Contains("at� logo")
            || respostaIA.Contains("at� breve"))
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
