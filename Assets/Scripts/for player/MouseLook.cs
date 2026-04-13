using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 200f;
    public Transform playerBody;

    [Header("Camera Settings")]
    public float xRotation = 0f;
    public float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate player body (yaw)
        yRotation += mouseX;
        playerBody.Rotate(Vector3.up * mouseX);

        // Apply camera rotation
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}