using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{

    public Image StaminaBar;
    public float stamina, MaxStamina;
    public float AttackCost;
    public float running;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            stamina -= AttackCost;
            if(stamina < 0 ) stamina = 0;
            StaminaBar.fillAmount = stamina / MaxStamina;
        }
    }
}
