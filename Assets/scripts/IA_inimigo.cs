using System.Collections.Generic;
using UnityEngine;

public class IA_inimigo : MonoBehaviour
{
    public enum Estado { Patrol, Chase }

    [Header("Alvo")]
    [Tooltip("Transform do player. Se não atribuído, tenta encontrar GameObject com tag 'Player'.")]
    public Transform player;

    [Header("Detecção")]
    [Tooltip("Distância (unidades) para começar a perseguir o player.")]
    public float followDistance = 8f;

    [Header("Movimento")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    [Tooltip("Rotação suave ao se mover (graus por segundo).")]
    public float rotationSpeed = 720f;

    [Header("Patrulha")]
    [Tooltip("Se true, gera pontos aleatórios dentro do raio; caso contrário, usa waypoints listados.")]
    public bool useRandomPatrol = true;
    [Tooltip("Raio (em unidades) ao redor do centro para gerar pontos de patrulha.")]
    public float patrolRadius = 6f;
    [Tooltip("Pontos de patrulha fixos (opcional). Usado quando useRandomPatrol == false.")]
    public List<Transform> waypoints = new List<Transform>();
    [Tooltip("Tempo que o inimigo espera ao chegar no ponto (segundos).")]
    public float waitTimeAtWaypoint = 1f;
    [Tooltip("Tolerância para considerar que chegou no ponto.")]
    public float waypointTolerance = 0.3f;

    [Header("Stun ao receber dano")]
    [Tooltip("Tempo em segundos que a IA fica desabilitada ao receber dano.")]
    public float disableOnHitDuration = 1f;

    // Estado interno
    public Estado estadoAtual { get; private set; } = Estado.Patrol;

    // Controle de patrulha
    private Vector3 patrolCenter;
    private Vector3 currentWaypoint;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;

    // Controle de desabilitação temporária (stun)
    private bool isDisabled = false;
    private float disableTimer = 0f;

    // Referência ao componente de vida para assinar evento de dano
    private vida vidaComp;

    // Rigidbody (se houver) para respeitar física e evitar conflito com transform.position
    private Rigidbody rb;

    void Start()
    {
        patrolCenter = transform.position;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        // Inicializa primeiro waypoint
        if (useRandomPatrol)
            currentWaypoint = GetRandomPointInRadius();
        else if (waypoints != null && waypoints.Count > 0)
            currentWaypoint = waypoints[0].position;
        else
            currentWaypoint = transform.position;

        // Procura componente de vida no próprio GameObject ou em parents e assina evento
        vidaComp = GetComponentInParent<vida>();
        if (vidaComp != null)
            vidaComp.OnDamageTaken += HandleDamageTaken;

        // Cache do Rigidbody (procura também em parents caso o RB esteja no root)
        rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
    }

    void OnDestroy()
    {
        if (vidaComp != null)
            vidaComp.OnDamageTaken -= HandleDamageTaken;
    }

    void Update()
    {
        // Se desabilitado por dano, contabiliza tempo e não executa comportamentos
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
                isDisabled = false;
            else
                return; // mantém IA inativa enquanto durar o stun
        }

        // Atualiza referência do player caso tenha sido adicionada depois
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        // Decide estado baseado na distância ao player
        if (player != null && Vector3.Distance(transform.position, player.position) <= followDistance)
        {
            estadoAtual = Estado.Chase;
        }
        else
        {
            estadoAtual = Estado.Patrol;
        }

        // Executa comportamento conforme estado
        switch (estadoAtual)
        {
            case Estado.Chase:
                ChasePlayer();
                break;
            case Estado.Patrol:
                Patrol();
                break;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        MoveTowards(player.position, chaseSpeed);
    }

    private void Patrol()
    {
        // Se usando waypoints fixos e lista vazia, ficar parado
        if (!useRandomPatrol && (waypoints == null || waypoints.Count == 0))
            return;

        // Chegou no waypoint atual?
        float dist = Vector3.Distance(transform.position, currentWaypoint);
        if (dist <= waypointTolerance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                waitTimer = 0f;
                // Próximo waypoint
                if (useRandomPatrol)
                {
                    currentWaypoint = GetRandomPointInRadius();
                }
                else
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                    currentWaypoint = waypoints[currentWaypointIndex].position;
                }
            }
        }
        else
        {
            MoveTowards(currentWaypoint, patrolSpeed);
        }
    }

    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f; // mantém movimento no plano horizontal
                    if (dir.sqrMagnitude < 0.0001f)  return;

        Vector3 move = dir.normalized * speed * Time.deltaTime;

        if (rb != null && !rb.isKinematic)
        {
            // Usa MovePosition/MoveRotation para não conflitar com física
            rb.MovePosition(rb.position + move);

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.deltaTime));
            return;
        }

        // Fallback: movimento por transform (sem Rigidbody)
        transform.position += move;

        // Rotaciona suavemente para a direção de movimento
        Quaternion targetRotTransform = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotTransform, rotationSpeed * Time.deltaTime);
    }

    private Vector3 GetRandomPointInRadius()
    {
        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        Vector3 point = patrolCenter + new Vector3(rand.x, 0f, rand.y);
        return point;
    }

    private void HandleDamageTaken()
    {
        // Apenas ativa o stun/desabilitação para que a física possa aplicar knockback
        isDisabled = true;
        disableTimer = disableOnHitDuration;

        // NÃO zere rb.velocity nem chame rb.Sleep() — isso cancela o knockback.
        // Se quiser reduzir reação excessiva, use um drag temporário ou ajuste knockbackForce.
    }

    void OnDrawGizmosSelected()
    {
        // Desenha raio de detecção e raio de patrulha no editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? patrolCenter : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        // Desenha waypoint atual
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(currentWaypoint, 0.2f);
    }
}


//O que foi feito (breve):
//-Substitu� o stub por uma IA com dois estados: `Patrol` e `Chase`.
//- Permite configurar via Inspector: alvo(`player`), dist�ncia de detec��o, velocidades, patrulha por pontos fixos ou aleat�ria dentro de um raio, tempo de espera em cada ponto.
//- Implementa��o simples e compat�vel com Unity sem depender de NavMesh ou Rigidbody (movimento por `transform.position`), f�cil de integrar e ajustar.
//- Tenta localizar o player automaticamente pelo tag `"Player"` se `player` n�o for atribu�do.
//
//Sugest�es de uso:
//-Atribua o `Transform` do player no Inspector ou marque o player com a tag `Player`.
//- Ajuste `followDistance`, `patrolRadius`, `patrolSpeed` e `chaseSpeed` conforme necessidade.
//- Se preferir waypoints fixos, desligue `useRandomPatrol`  e adicione `Transform`s na lista `waypoints`.