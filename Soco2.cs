csharp Assets\scripts\Soco2.cs
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

    void Start()
    {
        mainCamera = Camera.main;
        if (origin == null)
            origin = transform;
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
        lastPunchTime = Time.time;

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
            // envia mensagem de dano
            hit.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // se o alvo tem um componente `impacto`, delega para ele (centraliza o knockback)
            var impactoComp = hit.collider.gameObject.GetComponentInParent<impacto>();
            if (impactoComp != null)
            {
                impactoComp.RegisterHit(hit.collider.gameObject, origin.position);
            }
            else
            {
                // fallback: aplica força diretamente se não houver `impacto`
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);
            }

            // instancia efeito de impacto se fornecido
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}