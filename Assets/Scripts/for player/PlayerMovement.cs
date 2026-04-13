using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float crouchSpeed = 3f;
    public float crawlSpeed = 1.5f;

    [Header("Posture Heights")]
    public float standingHeight = 1.5f;
    public float crouchHeight = 1.2f;
    public float crawlHeight = 0.6f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraHeightOffset = 1.6f;
    public float cameraSmoothSpeed = 10f;

    private CharacterController controller;
    private float currentSpeed;
    private float currentCameraHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        
        // Get camera reference if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Initialize camera height
        currentCameraHeight = cameraHeightOffset;
    }

    void Update()
    {
        HandleMovement();
        HandlePosture();
        UpdateCameraPosition();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Get camera direction (ignoring Y for horizontal movement)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        // Calculate movement direction based on camera direction
        Vector3 moveDirection = (cameraRight * x + cameraForward * z).normalized;

        // Move player horizontally
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    void HandlePosture()
    {
        if (Input.GetKey(KeyCode.LeftControl)) // Crawl
        {
            controller.height = crawlHeight;
            controller.center = new Vector3(0, crawlHeight / 2f, 0);
            currentSpeed = crawlSpeed;
            currentCameraHeight = 0.3f; // Lower camera for crawling
        }
        else if (Input.GetKey(KeyCode.C)) // Crouch
        {
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
            currentSpeed = crouchSpeed;
            currentCameraHeight = 0.8f; // Medium camera height for crouching
        }
        else // Stand
        {
            controller.height = standingHeight;
            controller.center = new Vector3(0, standingHeight / 2f, 0);
            currentSpeed = walkSpeed;
            currentCameraHeight = 1.6f; // Normal camera height for standing
        }
    }

    void UpdateCameraPosition()
    {
        // Calculate camera position based on player position and posture
        // Camera is positioned at player's head level
        Vector3 cameraPosition = transform.position + new Vector3(
            0f,
            controller.center.y + currentCameraHeight,
            0f
        );

        // Smoothly move camera to target position
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            cameraPosition,
            cameraSmoothSpeed * Time.deltaTime
        );
    }

    // Optional: Draw gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1.6f, 0), 0.3f);
    }
}