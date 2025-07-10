using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    /*    public float mouseSensitivity = 100f;

        public Transform playerBody;

        private float xRotation = 0f;
        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerBody.Rotate(Vector3.up * mouseX);
        }*/

    void Start()
    {
        // 게임 시작 시 커서 잠금
        LockCursor();
    }

    void Update()
    {
        // ESC 키로 커서 잠금 해제/재잠금
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        // 게임 화면 클릭 시 다시 잠금
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;  // 커서를 화면 중앙에 고정
        Cursor.visible = false;                    // 커서 숨기기
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;    // 커서 자유롭게 이동
        Cursor.visible = true;                     // 커서 보이기
    }

    // 게임이 포커스를 잃었을 때 (Alt+Tab 등)
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            LockCursor();
        }
    }
}
