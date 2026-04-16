using UnityEngine;
using UnityEngine.InputSystem;

public class LookMouse : MonoBehaviour
{
    public Transform cameraTransform;
    [SerializeField] float look_Sensitivity;
    float pitch;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cameraTransform = GetComponentInChildren<Camera>().transform; 
    }

    // Update is called once per frame
    void Update()
    {
        MovimentoMouse();
    }

    void MovimentoMouse()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float yaw = mouseDelta.x * look_Sensitivity;
        transform.Rotate(Vector3.up* yaw);

        pitch -= mouseDelta.y * look_Sensitivity;
        pitch = Mathf.Clamp(pitch, -89f, +89);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}
