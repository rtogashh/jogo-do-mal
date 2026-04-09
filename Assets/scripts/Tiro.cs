using UnityEngine;
using UnityEngine.InputSystem;

public class Tiro : MonoBehaviour
{
    public GameObject projetil;
    [SerializeField] Transform Muzzle;
    [SerializeField] float speed;
    
    public void onTiro(InputValue value)
    {
        if(value.isPressed)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject clone_Projetil = Instantiate(projetil, Muzzle.position, Muzzle.rotation);

        Rigidbody rb = projetil.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.linearVelocity = Muzzle.forward * speed;
        }
    }
}
