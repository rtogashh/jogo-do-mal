using UnityEngine;

[DisallowMultipleComponent]
public class ColetavelCura : MonoBehaviour
{
    [Header("Cura")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private bool apenasJogador = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Feedback (opcional)")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float destroyDelay = 0.1f; // pequeno delay para permitir efeitos

    // collider usado EXPLICITAMENTE como trigger para detectar a coleta.
    // não alteraremos colliders que pertençam a outros Rigidbodies.
    private Collider triggerCollider;
    private bool coletado = false;

    void Awake()
    {
        // tenta obter um collider existente neste GameObject
        var existing = GetComponent<Collider>();

        if (existing == null)
        {
            // nenhum collider — cria um SphereCollider de trigger seguro
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            return;
        }

        // se o collider existente estiver ligado a um Rigidbody que não seja deste GameObject,
        // NÃO altere esse collider (pode estar sendo usado para física). Em vez disso, crie
        // um collider separado para detecção.
        var attachedRb = existing.attachedRigidbody;
        if (attachedRb != null && attachedRb.gameObject != gameObject)
        {
            // cria um collider dedicado para detecção (trigger)
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            // opcional: ajustar radius/center se necessário
        }
        else
        {
            // é seguro usar/alterar o collider existente
            triggerCollider = existing;
            triggerCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (coletado) return;

        if (apenasJogador && !other.CompareTag(playerTag))
            return;

        // tenta obter componente de vida diretamente
        var vidaComp = other.GetComponent<vida>();
        if (vidaComp == null)
        {
            // se não houver, tenta em parents (caso o collider esteja em child)
            vidaComp = other.GetComponentInParent<vida>();
        }

        if (vidaComp != null)
        {
            AplicarColeta(vidaComp);
        }
    }

    private void AplicarColeta(vida vidaComp)
    {
        coletado = true;

        // cura o jogador (método existente em `vida`)
        vidaComp.Heal(healAmount);

        // feedback visual/sonoro
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // desativa visual e somente os colliders pertencentes a este coletável
        DesativarVisualEColisor();

        // destrói o objeto depois de um pequeno delay para permitir reprodução de som/efeito
        Destroy(gameObject, destroyDelay);
    }

    private void DesativarVisualEColisor()
    {
        // desativa renderers filhos (pertencentes ao coletável)
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // desativa apenas os colliders que pertencem a este GameObject (inclui filhos do coletável)
        foreach (var c in GetComponentsInChildren<Collider>())
        {
            // evita tocar em colliders que possam pertencer a outros objetos por acidente
            if (c.gameObject == gameObject || c.transform.IsChildOf(transform))
                c.enabled = false;
        }

        // opcional: desativa luzes/particles se houver
        foreach (var p in GetComponentsInChildren<ParticleSystem>())
            p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}