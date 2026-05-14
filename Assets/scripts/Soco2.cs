using UnityEngine;

public class Soco2 : MonoBehaviour
{
    [Header("Configuração do soco")]
    [SerializeField] private float range = 2f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask hittableLayers = ~0;
    [SerializeField] private float impactForce = 3f;
    [SerializeField] private float cooldown = 0.5f;

    [Header("Referências (opcionais)")]
    [SerializeField] private Transform origin; // ponto de onde o ray parte; se null usa o transform deste objeto
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private Animator animator;
    [SerializeField] private string punchTrigger = "Punch";
    [SerializeField] private AudioClip punchSound;

    private Camera mainCamera;
    private float lastPunchTime = -Mathf.Infinity;

    // Referência ao sistema de stamina (se existir no player)
    private Stamina staminaComp;

    void Start()
    {
        mainCamera = Camera.main;
        if (origin == null)
            origin = transform;

        // tenta obter Stamina no mesmo objeto ou em parents
        staminaComp = GetComponent<Stamina>() ?? GetComponentInParent<Stamina>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastPunchTime + cooldown)
        {
            DoPunch();
        }
    }

    private void DoPunch()
    {
        // Se houver componente de stamina, tenta consumir antes de executar o soco
        if (staminaComp != null)
        {
            if (!staminaComp.TryConsume(staminaComp.AttackCost))
                return; // bloco se não tiver stamina suficiente
        }

        lastPunchTime = Time.time;

        Debug.Log("Soco realizado.");

        // animação e som (se presente)
        if (animator != null && !string.IsNullOrEmpty(punchTrigger))
            animator.SetTrigger(punchTrigger);

        if (punchSound != null)
            AudioSource.PlayClipAtPoint(punchSound, origin.position);

        // cria o raio a partir da câmera usando posição do mouse
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.5f);

        if (Physics.Raycast(ray, out hit, range, hittableLayers))
        {
            // envia mensagem "TakeDamage" se existir algum receptor no objeto atingido
            hit.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // aplica for�a se houver Rigidbody (fallback somente quando não houver 'impacto' delegado)
            var impactoComp = hit.collider.gameObject.GetComponentInParent<impacto>();
            if (impactoComp == null)
            {
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);
            }
            else
            {
                // delega ao sistema de impacto (caso exista) para evitar duplicação de efeitos
                impactoComp.RegisterHit(hit.collider.gameObject, origin.position);
            }

            // instancia efeito de impacto se fornecido
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}
