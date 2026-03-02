using UnityEngine;
using TMPro; 

public class LoginUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField idInput;
    public TMP_InputField pwInput;

    [Header("팝업창 연결")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText; 

    public void OnClickLoginButton()
    {
        string id = idInput.text;
        string pw = pwInput.text;

        // 빈칸 방지턱
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            ShowPopup("아이디와 비밀번호를 모두 입력해주세요!");
            return;
        }

        NetworkManager.Instance.SendLoginPacket(id, pw);
    }

    public void ShowPopup(string message)
    {
        popupText.text = message;
        popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}