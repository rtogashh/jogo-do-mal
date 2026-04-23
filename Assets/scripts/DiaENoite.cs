using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DiaENoite : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float dayDurationInSeconds = 60f; // Tempo para um dia completo
    //private float rotationSpeed = 360f;
    [SerializeField] private Light luzinha;
    //bool noite = false;

    private void Start()
    {
        luzinha = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around the Y-axis (Vector3.up)
        // Time.deltaTime ensures a constant speed regardless of frame rate
        //transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        //transform.Rotate(rotationSpeed * Time.deltaTime,0,0);
        float rotationSpeed = 360f / dayDurationInSeconds;
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        float dotProduct = Vector3.Dot(transform.forward, Vector3.down);
        luzinha.intensity = Mathf.Max(0, dotProduct);
        //if (luzinha.intensity > 0) noite = false;
        //else if (luzinha.intensity < 0) noite = true;
    }

    /*void AjustesDia()
    {
        if (noite == false)
        {
            //Environment reflections Intensity Multiplier = 1
            //Fog = true
        }
    }
    void AjustesNoite()
    {
        if (noite == false)
        {
            //Environment reflections Intensity Multiplier = 0
            //Fog = false
        }
    }*/
}
