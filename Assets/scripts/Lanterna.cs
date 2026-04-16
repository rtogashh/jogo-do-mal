using UnityEngine;

public class Lanterna : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public GameObject lanterna;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            if (lanterna != null)
            {
                lanterna.SetActive(!lanterna.activeSelf);
            }
        }
    }
}
