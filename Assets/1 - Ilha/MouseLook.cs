using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 mouseLook;
    private float xRotation = 0f;

    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float verticalLimit;

    [Header ("corpo do Player")]
    [SerializeField] private Transform playerbody;

    [Header("Suavização")]
    public float smoothTime = 0.03f; // Quanto menor, mais responsivo. Quanto maior, mais "pesado".
    private Vector2 currentMouseLook;
    private Vector2 appliedMouseDelta;
    private Vector2 smoothV;

    [Header("CameraTilt")]
    [SerializeField] private float tiltAmount;
    [SerializeField] private float tiltSpeed;
    private float currentTilt = 0f;
    private float targetTilt = 0f;



    void Awake()
    {
        //playerbody = transform.parent;

        controls = new PlayerControls();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        // Se mover para a DIREITA (x > 0), inclina para a ESQUERDA (Z positivo)
        // Se mover para a ESQUERDA (x < 0), inclina para a DIREITA (Z negativo)
        // Geralmente usamos o inverso para dar sensação de inércia
        targetTilt = -moveInput.x * tiltAmount;
    }

    void Update()
    {
        Look();

        // 2. Suavizamos a inclinação atual em direção ao alvo
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        // 3. Aplicamos ao seu código de rotação existente
        transform.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
    }

    void Look()
    {
        // 1. Pegamos o valor bruto do input
        Vector2 targetMouseLook = controls.Gameplay.Look.ReadValue<Vector2>();

        // 2. Suavizamos o valor do input antes de aplicar a sensibilidade
        currentMouseLook = Vector2.SmoothDamp(currentMouseLook, targetMouseLook, ref smoothV, smoothTime);

        // 3. Agora usamos o valor suavizado para os cálculos
        float mouseX = currentMouseLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentMouseLook.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        transform.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
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
