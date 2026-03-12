using System.Collections;
using UnityEngine;

public class Soco : MonoBehaviour
{
    [Tooltip("GameObject que contém a hitbox (ex.: filho com Collider). Deixe desativado no Inspector.")]
    public GameObject hitbox;

    [Tooltip("Tempo em segundos que a hitbox permanece ativa")]
    public float hitboxActiveTime = 0.2f;

    [Tooltip("Tempo adicional antes do próximo ataque (prevenção de múltiplos cliques)")]
    public float attackCooldown = 0.3f;

    [Tooltip("Opcional: Animator para sincronizar animação de ataque")]
    public Animator animator;
    [Tooltip("Nome do trigger do Animator (se usado)")]
    public string attackTrigger = "Attack";

    private bool canAttack = true;

    void Start()
    {
        if (hitbox != null)
            hitbox.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(DoAttack());
        }
    }

    private IEnumerator DoAttack()
    {
        canAttack = false;

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        if (hitbox != null)
            hitbox.SetActive(true);

        yield return new WaitForSeconds(hitboxActiveTime);

        if (hitbox != null)
            hitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}