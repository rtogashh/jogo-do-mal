using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IA_inimigo : MonoBehaviour
{
    public enum Estado { Patrol, Chase, Attack }

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

    [Header("Ataque")]
    [Tooltip("Alcance em que o inimigo tenta atacar o player.")]
    public float attackRange = 1.5f;
    [Tooltip("Tempo entre ataques (segundos).")]
    public float attackCooldown = 1.2f;
    [Tooltip("Dano aplicado pelo soco.")]
    public int attackDamage = 10;
    [Tooltip("Força do knockback aplicada ao player.")]
    public float attackForce = 5f;
    [Tooltip("Trigger do Animator para o ataque (opcional).")]
    public string attackTrigger = "Attack";
    [Tooltip("Tempo de wind-up antes do impacto efetivo (segundos).")]
    public float attackWindup = 0.15f;

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

    // Ataque
    private bool isAttacking = false;

    // Animator (opcional) - usado apenas se atribuído via Inspector em vida/impacto; aqui é opcional
    [Header("Referências (opcionais)")]
    public Animator animator;

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

        // Se não foi atribuído Animator explicitamente, tenta pegar no mesmo GameObject
        if (animator == null)
            animator = GetComponent<Animator>();
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

        // Se estiver atacando, não execute movimento (mas permita rotação para mirar)
        if (isAttacking)
        {
            if (player != null)
                RotateTowards(player.position);
            return;
        }

        // Atualiza referência do player caso tenha sido adicionada depois
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        // Decide estado baseado na distância ao player
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= attackRange)
                estadoAtual = Estado.Attack;
            else if (distToPlayer <= followDistance)
                estadoAtual = Estado.Chase;
            else
                estadoAtual = Estado.Patrol;
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
            case Estado.Attack:
                // tenta iniciar ataque
                TryAttack();
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
        // Não se move se está atacando ou desabilitado
        if (isAttacking || isDisabled) return;

        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f; // mantém movimento no plano horizontal
        if (dir.sqrMagnitude < 0.0001f) return;

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

    private void TryAttack()
    {
        if (isAttacking || isDisabled || player == null) return;

        // Start attack coroutine
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // rotaciona para o player imediatamente
        RotateTowards(player.position);

        // animação de ataque (wind-up)
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        // tempo até o impacto efetivo (permite sincronizar animação)
        if (attackWindup > 0f)
            yield return new WaitForSeconds(attackWindup);

        PerformAttack();

        // espera cooldown antes de poder atacar novamente
        float remaining = Mathf.Max(0f, attackCooldown - attackWindup);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        isAttacking = false;
    }

    private void PerformAttack()
    {
        if (player == null) return;

        // aplica dano via SendMessage (compatível com seu componente `vida`)
        player.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);

        // aplica knockback físico ao Rigidbody do player (se existir)
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        dir = dir.normalized;
        Vector3 force = dir * attackForce + Vector3.up * (attackForce * 0.25f);

        var playerRb = player.GetComponent<Rigidbody>() ?? player.GetComponentInParent<Rigidbody>();
        if (playerRb != null && !playerRb.isKinematic)
        {
            playerRb.AddForce(force, ForceMode.Impulse);
            return;
        }

        // se for CharacterController, tenta mover levemente
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(force * 0.1f);
            return;
        }

        // fallback: message para handler customizado
        player.gameObject.SendMessage("ApplyKnockback", force, SendMessageOptions.DontRequireReceiver);
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if ((dir.sqrMagnitude < 0.0001f)) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        if (rb != null && !rb.isKinematic)
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.deltaTime));
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void HandleDamageTaken()
    {
        // Ativa desabilitação temporária (stun) para permitir knockback físico do atacante
        isDisabled = true;
        disableTimer = disableOnHitDuration;

        // NÃO zere rb.velocity aqui — isso impediria knockback físico
    }

    void OnDrawGizmosSelected()
    {
        // Desenha raio de detecção, raio de patrulha e alcance de ataque no editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? patrolCenter : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Desenha waypoint atual
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(currentWaypoint, 0.2f);
    }
}


