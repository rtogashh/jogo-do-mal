using UnityEngine;
using UnityEngine.SceneManagement;

public class PredioFinal : MonoBehaviour
{
    [SerializeField] private string sceneName = "Menu";
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        if(collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
