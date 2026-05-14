using UnityEngine;

[DisallowMultipleComponent]
public class Bloqueio : MonoBehaviour
{
    [Header("Configuração de Bloqueio")]
    [Tooltip("Se true, redução de dano é porcentagem (0..1). Se false, redução é valor absoluto.")]
    public bool usePercent = true;

    [Tooltip("Se usePercent = true: porcentagem de dano reduzido (0.5 = 50%). Se false: quantidade fixa subtraída do dano.")]
    [Range(0f, 1f)]
    public float damageReductionPercent = 0.5f;

    [Tooltip("Custo de stamina por segundo enquanto estiver segurando o bloqueio.")]
    public float blockStaminaPerSecond = 10f;

    [Tooltip("Key usada para bloquear. Por padrão botão direito do mouse.")]
    public int blockMouseButton = 1; // 1 = right mouse button

    // Estado interno
    private bool isBlocking = false;

    // Referências
    private Stamina staminaComp;

    void Start()
    {
        // tenta obter Stamina no mesmo objeto ou em parents
        staminaComp = GetComponent<Stamina>() ?? GetComponentInParent<Stamina>();
    }

    void Update()
    {
        bool wantBlock = Input.GetMouseButton(blockMouseButton);

        if (wantBlock)
        {
            // calcula custo deste frame
            float costThisFrame = blockStaminaPerSecond * Time.deltaTime;

            if (staminaComp != null)
            {
                // tenta consumir; se não tiver stamina suficiente, cancela o bloqueio
                if (staminaComp.TryConsume(costThisFrame))
                {
                    if (!isBlocking)
                    {
                        isBlocking = true;
                        Debug.Log("Bloqueio ativado.");
                    }
                }
                else
                {
                    if (isBlocking)
                    {
                        isBlocking = false;
                        Debug.Log("Stamina insuficiente — bloqueio desativado.");
                    }
                }
            }
            else
            {
                // sem componente de Stamina -> permite bloquear sem custo
                if (!isBlocking)
                {
                    isBlocking = true;
                    Debug.Log("Bloqueio ativado (sem Stamina).");
                }
            }
        }
        else
        {
            if (isBlocking)
            {
                isBlocking = false;
                Debug.Log("Bloqueio solto.");
            }
        }
    }

    // Chamado por `vida` antes de aplicar dano. Retorna o dano modificado.
    public int ModifyDamageByBlock(int incomingDamage)
    {
        if (!isBlocking || incomingDamage <= 0)
            return incomingDamage;

        int modified;
        if (usePercent)
        {
            float kept = Mathf.Clamp01(1f - damageReductionPercent);
            modified = Mathf.Max(0, Mathf.RoundToInt(incomingDamage * kept));
        }
        else
        {
            modified = Mathf.Max(0, incomingDamage - Mathf.RoundToInt(damageReductionPercent * 100f)); // caso usePercent=false, interprete damageReductionPercent como 0..1 escala -> multiplicador por 100
            // Observação: se preferir um valor absoluto, altere para um campo `public int damageReductionFlat`.
        }

        Debug.Log($"Bloqueio: dano reduzido de {incomingDamage} para {modified}.");
        return modified;
    }

    // Exposição de leitura para outros componentes (ex.: UI)
    public bool IsBlocking() => isBlocking;
}
