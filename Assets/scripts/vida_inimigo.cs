using System;
using UnityEngine;
using UnityEngine.UI;

public class vida_inimigo : MonoBehaviour
{
    [Header("Configuração de Vida (Inimigo)")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f; // segundos após receber dano

    [Header("Referências (opcionais)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Die";
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Slider healthBar; // suporta 0..1 ou 0..maxHealth

    [Header("Drops / Recompensa")]
    [Tooltip("Prefab a instanciar quando morrer (opcional).")]
    public GameObject lootPrefab;
    [Range(0f, 1f)]
    public float lootDropChance = 0.25f;
    [Tooltip("Delay antes de destruir o inimigo (segundos). 0 = destrói imediatamente após morte.")]
    public float destroyDelay = 2f;

    private int currentHealth;
    private float lastDamageTime = -Mathf.Infinity;
    private bool isDead = false;

    // Evento para notificar que recebeu dano (útil para IA reagir)
    public event Action OnDamageTaken;

    // Evento específico para morte de inimigo
    public event Action OnEnemyDied;

    // Cache da IA caso queira desativá-la ao morrer
    private IA_inimigo iaComp;

    void Awake()
    {
        if (currentHealth <= 0 || currentHealth > maxHealth)
            currentHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);

        Debug.Log($"[vida_inimigo] Awake: {gameObject.name} health inicial = {currentHealth}/{maxHealth}");
    }

    void Start()
    {
        UpdateHealthUI();
        iaComp = GetComponentInParent<IA_inimigo>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (currentHealth <= 0 && !isDead)
            Die();
    }

    // Principal API para dano (compatível com seu uso atual)
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (Time.time < lastDamageTime + invulnerabilityDuration) return;

        lastDamageTime = Time.time;

        int applied = Mathf.Max(0, amount);
        currentHealth -= applied;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[vida_inimigo] {gameObject.name} recebeu dano: {applied}. Vida atual: {currentHealth}/{maxHealth}");

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (damageSound != null)
            AudioSource.PlayClipAtPoint(damageSound, transform.position);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        UpdateHealthUI();

        if (applied > 0)
            OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    // Sobrecarga para compatibilidade SendMessage/object
    public void TakeDamage(object damageObj)
    {
        if (damageObj == null) return;

        if (damageObj is int i)
            TakeDamage(i);
        else if (damageObj is float f)
            TakeDamage(Mathf.CeilToInt(f));
        else if (int.TryParse(damageObj.ToString(), out int parsed))
            TakeDamage(parsed);
        else
            Debug.LogWarning($"[vida_inimigo] TakeDamage recebeu um objeto inválido: {damageObj}");
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth += Mathf.Max(0, amount);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateHealthUI();
        if (currentHealth <= 0 && !isDead)
            Die();
    }

    private void UpdateHealthUI()
    {
        if (healthBar == null) return;

        if (healthBar.maxValue <= 1f)
            healthBar.value = currentHealth / (float)maxHealth;
        else
        {
            healthBar.maxValue = Mathf.Max(healthBar.maxValue, maxHealth);
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.LogError($"[vida_inimigo] {gameObject.name} morreu.");

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Tenta desativar IA para evitar que continue movendo ou atacando
        if (iaComp != null)
            iaComp.enabled = false;

        // Drop opcional
        if (lootPrefab != null && UnityEngine.Random.value <= lootDropChance)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }

        OnEnemyDied?.Invoke();

        // Mantemos física/colisores intactos; opcionalmente destruímos o objeto após delay
        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
        else
            Destroy(gameObject);
    }

    public bool IsDead() => isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}