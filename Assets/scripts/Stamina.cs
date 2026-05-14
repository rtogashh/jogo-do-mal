            using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("UI")]
    public Image StaminaBar;

    [Header("Valores")]
    public float MaxStamina = 100f;
    [Tooltip("Custo de stamina por ataque")]
    public float AttackCost = 15f;
    [Tooltip("Regeneração em pontos por segundo")]
    public float regenRate = 1f;

    private float stamina;
    private int lastLoggedInt = -1;

    void Start()
    {
        stamina = MaxStamina;
        UpdateUI();
        lastLoggedInt = Mathf.FloorToInt(stamina);
    }

    void Update()
    {
        
        if (stamina < MaxStamina)
        {
            float prevInt = Mathf.FloorToInt(stamina);
            stamina += regenRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, MaxStamina);
            int newInt = Mathf.FloorToInt(stamina);
            if (newInt > prevInt)
            {
                Debug.Log($"Stamina regenerou: +{newInt - prevInt}. Atual: {Mathf.RoundToInt(stamina)}/{Mathf.RoundToInt(MaxStamina)}");
                lastLoggedInt = newInt;
            }
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (StaminaBar != null)
            StaminaBar.fillAmount = Mathf.Clamp01(stamina / MaxStamina);
    }

    
    public bool TryConsume(float amount)
    {
        amount = Mathf.Max(0f, amount);
        if (stamina >= amount)
        {
            stamina -= amount;
            stamina = Mathf.Clamp(stamina, 0f, MaxStamina);
            UpdateUI();
            Debug.Log($"Stamina reduzida: -{amount}. Atual: {Mathf.RoundToInt(stamina)}/{Mathf.RoundToInt(MaxStamina)}");
            return true;
        }

        Debug.Log("Soco cancelado: stamina insuficiente.");
        return false;
    }

    
    public float CurrentStamina => stamina;
}
