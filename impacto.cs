
using System.Collections.Generic;
using UnityEngine;

public class impacto : MonoBehaviour
{
    [Tooltip("Tags que devem disparar o log 'acertou'")]
    public string[] targetTags = new string[] { "Enemy" };

    [Header("Knockback")]
    [Tooltip("Força do knockback (velocidade aplicada) no objeto atingido")]
    public float knockbackForce = 5f;
    [Tooltip("Componente vertical adicionado à direção do knockback")]
    public float upwardModifier = 0.3f;

    [Tooltip("Se true, cada alvo só será afetado uma vez enquanto a hitbox estiver ativa")]
    public bool singleHitPerActivation = true;

    private HashSet<GameObject> hitHistory = new HashSet<GameObject>();

    // Método usado pelo Hitbox para registrar um acerto manual/externo
    public void RegisterHit(GameObject target, Vector3 sourcePosition)
    {
        if (target == null || !IsTargetTag(target))
            return;

        if (singleHitPerActivation && hitHistory.Contains(target))
            return;

        hitHistory.Add(target);
        Debug.Log("acertou: " + target.name);
        ApplyKnockback(target, sourcePosition);
    }

    // Limpa histórico (chamar no início de cada ativação da hitbox)
    public void ClearHits()
    {
        hitHistory.Clear();
    }

    // Expõe verificação de tag para outros componentes (ex.: Hitbox)
    public bool IsTargetTag(GameObject go)
    {
        if (targetTags == null || targetTags.Length == 0)
            return false;

        foreach (var tag in targetTags)
        {
            if (!string.IsNullOrEmpty(tag) && go.CompareTag(tag))
                return true;
        }
        return false;
    }

    private void ApplyKnockback(GameObject target, Vector3 sourcePosition)
    {
        if (target == null)
            return;

        Vector3 dir = (target.transform.position - sourcePosition).normalized;
        dir.y += upwardModifier;
        dir.Normalize();

        Rigidbody rb = target.GetComponent<Rigidbody>() ?? target.GetComponentInParent<Rigidbody>();

        if (rb != null && !rb.isKinematic)
        {
            // Aplique a velocidade diretamente (evita múltiplos impulsos indesejados)
            rb.velocity = dir * knockbackForce;
            return;
        }

        var cc = target.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(dir * (knockbackForce * 0.1f));
            return;
        }

        target.SendMessage("ApplyKnockback", dir * knockbackForce, SendMessageOptions.DontRequireReceiver);
    }
}