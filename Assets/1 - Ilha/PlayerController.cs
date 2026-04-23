using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //[SerializeField] float jumpHeight;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;

    [Header("Velocidade")]
    [SerializeField] float speed;
    [SerializeField] public float Walking = 6f;
    [SerializeField] public float Running = 12f;
    [SerializeField] public float Crouching = 2f;

    [Header("Altura")]
    [SerializeField] public float defaultHeight = 2f;
    [SerializeField] public float crouchHeight = 1f;

    [Header("Booleanas")]
    bool canMove = true;
    bool agachado = false;
    bool correndo = false;

    [Header("Gravidade")]
    [SerializeField] float gravity = -10f;
    //[SerializeField] float gravityIntensifier = 1.3f;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float drag = 5f;// Quanto maior, mais rápido o impulso para
    public float dashCooldown = 1.5f; // Tempo de espera entre os impulsos
    private float nextDashTime = 0f;  // Quando o próximo impulso será permitido
    private Vector3 impact = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        speed = Walking;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (canMove)
            //moveInput = value.Get<Vector2>();
            moveInput = context.ReadValue<Vector2>();
        //Debug.Log($"Move Imput: {moveInput}");
    }

    /*public void OnJump(InputAction.CallbackContext context)
    {
        //Debug.Log($"Jumping {context.performed} - Is Grounded: {controller.isGrounded}");
        if (context.performed && controller.isGrounded)
        {
            //Debug.Log("We are supposed to jump");
            if (canMove)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }
        }
    }*/

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= nextDashTime)
        {
            nextDashTime = Time.time + dashCooldown;
            Vector3 direction = -transform.forward;

            // Aplica a força inicial
            impact += direction.normalized * dashForce;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        //Debug.Log("Sprint");
        if (context.performed && agachado == false && canMove)
        {
            correndo = true;
            speed = Running;
        }
        else if (context.canceled && agachado == false && canMove)
        {
            //Debug.Log("É pra andar");
            correndo = false;
            speed = Walking;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        //Debug.Log("Crounch");
        if (context.performed && correndo == false && canMove)
        {
            agachado = true;
            controller.height = crouchHeight;
            speed = Crouching;
        }
        else if (context.canceled && correndo == false && canMove)
        {
            agachado= false;
            controller.height = defaultHeight;
            speed = Walking;
        }
    }

    void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.right * move.x + transform.forward * move.z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        /*if (!controller.isGrounded && velocity.y <0)
        {
            velocity.y = gravity * gravityIntensifier;
        }*/

        // Código pro Dash
        Vector3 move1 = Vector3.zero;
        if (impact.magnitude > 0.2f) // 2. Aplicar dissipação do impulso (Efeito de rasto/atrito)
        {
            controller.Move(impact * Time.deltaTime); // Move o personagem pelo impacto atual
        }
        impact = Vector3.Lerp(impact, Vector3.zero, drag * Time.deltaTime); // Reduz o impacto gradualmente para zero
    }
}
