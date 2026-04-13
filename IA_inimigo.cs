using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA simples de inimigo com 2 status:
/// - Patrol: ronda uma área definida (pontos aleatórios dentro de um raio ou lista de waypoints)
/// - Chase: segue o player quando ele entra em uma distância definida
/// </summary>
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

    // Estado interno
    public Estado estadoAtual { get; private set; } = Estado.Patrol;

    // Controle de patrulha
    private Vector3 patrolCenter;
    private Vector3 currentWaypoint;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;

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
    }

    void Update()
    {
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
        if (dir.sqrMagnitude < 0.0001f) return;

        Vector3 move = dir.normalized * speed * Time.deltaTime;
        transform.position += move;

        // Rotaciona suavemente para a direção de movimento
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private Vector3 GetRandomPointInRadius()
    {
        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        Vector3 point = patrolCenter + new Vector3(rand.x, 0f, rand.y);
        return point;
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
```

O que foi feito (breve):
- Substituí o stub por uma IA com dois estados: `Patrol` e `Chase`.
- Permite configurar via Inspector: alvo (`player`), distância de detecção, velocidades, patrulha por pontos fixos ou aleatória dentro de um raio, tempo de espera em cada ponto.
- Implementação simples e compatível com Unity sem depender de NavMesh ou Rigidbody (movimento por `transform.position`), fácil de integrar e ajustar.
- Tenta localizar o player automaticamente pelo tag `"Player"` se `player` não for atribuído.

Sugestões de uso:
- Atribua o `Transform` do player no Inspector ou marque o player com a tag `Player`.
- Ajuste `followDistance`, `patrolRadius`, `patrolSpeed` e `chaseSpeed` conforme necessidade.
- Se preferir waypoints fixos, desligue `useRandomPatrol` e adicione `Transform`s na lista `waypoints`.