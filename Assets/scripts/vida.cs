using System;
using UnityEngine;
using UnityEngine.UI;

public class vida : MonoBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f; // segundos após receber dano

    [Header("Referências (opcionais)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deathTrigger = "Die";
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private Slider healthBar; // se usado, suporta tanto 0..1 quanto 0..maxHealth

    private int currentHealth;
    private float lastDamageTime = -Mathf.Infinity;
    private bool isDead = false;

    void Awake()
    {
        // Garante que a vida seja inicializada corretamente antes de OnEnable/Start.
        if (currentHealth <= 0 || currentHealth > maxHealth)
            currentHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);

        Debug.Log($"[vida] Awake: {gameObject.name} health inicial = {currentHealth}/{maxHealth}");
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void OnEnable()
    {
        // Sincroniza estado caso o objeto seja reativado
        if (currentHealth <= 0 && !isDead)
            Die();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (Time.time < lastDamageTime + invulnerabilityDuration) return;

        lastDamageTime = Time.time;
        int applied = Mathf.Max(0, amount);
        currentHealth -= applied;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[vida] {gameObject.name} recebeu dano: {applied}. Vida atual: {currentHealth}/{maxHealth}");

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
            animator.SetTrigger(hitTrigger);

        if (damageSound != null)
            AudioSource.PlayClipAtPoint(damageSound, transform.position);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

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
            Debug.LogWarning($"[vida] TakeDamage recebeu um objeto inválido: {damageObj}");
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

        Debug.LogError($"[vida] {gameObject.name} morreu. Stack trace para diagnóstico:\n{Environment.StackTrace}");

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // NÃO modificar Rigidbody/Collider aqui — mantemos física do objeto intacta

        // Opcional: desativa este componente para cessar processamento de vida
        enabled = false;
    }

    public bool IsDead() => isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}
