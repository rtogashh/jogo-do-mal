using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Head Bob Settings")]
    public float walkingBobSpeed = 14f;
    public float bobAmount = 0.05f; // Quão alto/baixo a câmera vai
    private float defaultYPos = 0;
    private float timer;

    [Header("Corrida (Sprint)")]
    public float sprintBobSpeed = 22f; // Velocidade bem mais rápida que a de caminhada
    [SerializeField] private bool isSprinting = false;

    [Header("Suavização do Bob")]
    public float smoothReturnSpeed = 10f;


    private Vector2 moveInput;
    private float lerpedBobSpeed;

    void Start()
    {
        // Salva a posição inicial local da câmera
        defaultYPos = transform.localPosition.y;
    }

    void Update()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        HandleHeadBob();

        /*float targetSpeed = isSprinting ? sprintBobSpeed : walkingBobSpeed;
        lerpedBobSpeed = Mathf.Lerp(lerpedBobSpeed, targetSpeed, Time.deltaTime * 10f);
        timer += Time.deltaTime * lerpedBobSpeed;*/
    }

    public void OnMove(InputValue value)
    {
        // Salva o valor do movimento (WASD)
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        // Lê o valor como float (1 para apertado, 0 para solto)
        float sprintValue = value.Get<float>();

        // Se for maior que 0, está correndo
        isSprinting = sprintValue > 0;

        // Debug para testar no Console se está alternando
        // Debug.Log("Correndo: " + isSprinting);
    }

    public void OnDash(InputValue value)
    {

    }

    void HandleHeadBob()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // 1. Define a velocidade atual baseada no estado de corrida
            float currentBobSpeed = isSprinting ? sprintBobSpeed : walkingBobSpeed;

            // 2. Opcional: Aumentar o balanço (altura) também na corrida
            float currentBobAmount = isSprinting ? bobAmount * 1.5f : bobAmount;

            timer += Time.deltaTime * currentBobSpeed;

            float newY = defaultYPos + Mathf.Sin(timer) * currentBobAmount;
            float newX = Mathf.Cos(timer / 2) * bobAmount; // O X balança na metade da velocidade do Y
            transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
        }
        else
        {
            // Reset suave quando parado...
            timer = 0;
            Vector3 resetPos = new Vector3(transform.localPosition.x, defaultYPos, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, resetPos, Time.deltaTime * 5f);
        }
    }
}
