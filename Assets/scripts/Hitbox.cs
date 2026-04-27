using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [Tooltip("Script que processa o impacto (knockback, tags)")]
    public impacto impactoScript;

    [Tooltip("Se true, cada alvo só recebe 1 hit enquanto a hitbox estiver ativa")]
    public bool singleHitPerActivation = true;

    private HashSet<GameObject> hitHistory = new HashSet<GameObject>();
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("Hitbox precisa de um Collider.");

        // Garanta que a hitbox seja trigger
        col.isTrigger = true;

        // Garanta que exista um Rigidbody kinematic para eventos de trigger confiáveis
        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (impactoScript == null)
            impactoScript = GetComponentInParent<impacto>();
    }

    void OnEnable()
    {
        ClearHits();
        // Sincroniza transformações e faz a varredura inicial (caso já haja sobreposição)
        Physics.SyncTransforms();
        CheckInitialOverlaps();
    }

    public void ClearHits()
    {
        hitHistory.Clear();
        impactoScript?.ClearHits();
    }

    private void HandleHit(Collider other, Vector3 contactPoint)
    {
        if (other == null || other.gameObject == gameObject)
            return;

        // Determina a "entidade" real a ser marcada no histórico:
        // prioriza o GameObject que contém o componente `vida`, senão usa o root.
        GameObject otherEntity = GetEntityRoot(other.gameObject);
        if (otherEntity == null) return;

        if (singleHitPerActivation && hitHistory.Contains(otherEntity))
            return;

        if (impactoScript == null)
            impactoScript = GetComponentInParent<impacto>();

        if (impactoScript == null)
            return;

        if (!impactoScript.IsTargetTag(otherEntity))
            return;

        hitHistory.Add(otherEntity);
        impactoScript.RegisterHit(otherEntity, contactPoint);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleHit(other, other.ClosestPoint(transform.position));
    }

    void OnTriggerStay(Collider other)
    {
        HandleHit(other, other.ClosestPoint(transform.position));
    }

    // Verificação imediata ao ativar: usa QueryTriggerInteraction.Collide para garantir leitura de triggers
    public void CheckInitialOverlaps()
    {
        if (col == null)
            return;

        // BoxCollider otimizado
        if (col is BoxCollider box)
        {
            Vector3 worldCenter = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
            var hits = Physics.OverlapBox(worldCenter, halfExtents, box.transform.rotation, ~0, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (h.gameObject == gameObject) continue;
                HandleHit(h, h.ClosestPoint(worldCenter));
            }
            return;
        }

        // SphereCollider
        if (col is SphereCollider sphere)
        {
            Vector3 worldCenter = sphere.transform.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, Mathf.Max(sphere.transform.lossyScale.y, sphere.transform.lossyScale.z));
            var hits = Physics.OverlapSphere(worldCenter, radius, ~0, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                if (h == null) continue;
                if (h.gameObject == gameObject) continue;
                HandleHit(h, h.ClosestPoint(worldCenter));
            }
            return;
        }

        // Fallback: usa bounds (inclui MeshCollider etc.)
        var fallbackHits = Physics.OverlapSphere(col.bounds.center, Mathf.Max(col.bounds.extents.x, Mathf.Max(col.bounds.extents.y, col.bounds.extents.z)), ~0, QueryTriggerInteraction.Collide);
        foreach (var h in fallbackHits)
        {
            if (h == null) continue;
            if (h.gameObject == gameObject) continue;
            HandleHit(h, h.ClosestPoint(col.bounds.center));
        }
    }

    // Retorna o GameObject que representa a entidade alvo:
    // - primeiro procura um componente `vida` em parents (entidade lógica);
    // - se não houver, usa transform.root.
    private GameObject GetEntityRoot(GameObject go)
    {
        if (go == null) return null;

        var vidaComp = go.GetComponentInParent<vida>();
        if (vidaComp != null)
            return vidaComp.gameObject;

        return go.transform.root != null ? go.transform.root.gameObject : go;
    }
}
