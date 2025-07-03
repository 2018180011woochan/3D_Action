using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;      

    [Header("�÷��̾� �̵� ����")]
    public float moveSpeed = 6f;         
    public float gravity = -9.81f;     
    public float turnSmoothTime = 0.1f;    
    public float jumpHeight = 1.5f;       
}
