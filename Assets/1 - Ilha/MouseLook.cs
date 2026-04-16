using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    private PlayerControls controls;
    [SerializeField] private float mouseSensitivity;
    private Vector2 mouseLook;
    private float xRotation = 0f;
    private Transform playerbody;

    void Awake()
    {
        playerbody = transform.parent;

        controls = new PlayerControls();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();
    }

    void Look()
    {
        mouseLook = controls.Gameplay.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * mouseSensitivity * Time.deltaTime;
        float moyseY = mouseLook.y * mouseSensitivity * Time.deltaTime;

        xRotation -= moyseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerbody.Rotate(Vector3.up * mouseX);
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
}
