using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]

public class Movement : MonoBehaviour
{
    CharacterController controller;
    Vector2 moveInput;
    Vector3 verticalVelocity;
    [SerializeField] float valorGravidade;
    bool isGrounded;
    [SerializeField] float speed;
    [SerializeField] float jumpHeight;
    [SerializeField] private Transform cameraTransform;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if(cameraTransform == null) cameraTransform = transform;
    }

    void Update()
    {
        Movimento();
    }

    void Movimento()
    {
        isGrounded = controller.isGrounded;
        if(isGrounded && verticalVelocity.y < 0 )
        {
            verticalVelocity.y = -2f;
        }
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right*moveInput.x;
        verticalVelocity.y = valorGravidade * Time.deltaTime;
        Vector3 finalVelocity = moveDirection * speed;

        verticalVelocity.y += valorGravidade * Time.deltaTime;
        finalVelocity.y = verticalVelocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            //v=sqrt(h*-2*g)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2 * valorGravidade);
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        Vector3 origin = cameraTransform.position;
        Vector3 direcao = cameraTransform.forward;

        if (Physics.Raycast(origin, direcao, out RaycastHit hit, 100f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.DrawRay(origin, direcao * hit.distance, Color.yellow);
                hit.collider.GetComponent<Renderer>().material.color = Color.red;
                Debug.Log("Acertou a desgraça");
            }
            else
            {
                Debug.DrawRay(origin, direcao * hit.distance, Color.yellow);
            }
        }
    }
}
