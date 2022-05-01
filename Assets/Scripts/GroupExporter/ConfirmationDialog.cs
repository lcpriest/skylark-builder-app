using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    public static ConfirmationDialog Instance;

    public GameObject m_Dialog;
    public GameObject m_Input;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void Update()
    {
#if EBS_NEW_INPUT_SYSTEM
        if (Keyboard.current.enterKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Return))
#endif
        {
            OnConfirmExport();
        }

#if EBS_NEW_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            OnCancelExport();
        }
    }

    public void ShowDialog(bool show = true)
    {
        gameObject.SetActive(show);

        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Demo_InputHandler.Instance.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Demo_InputHandler.Instance.enabled = true;
        }
    }

    public void OnConfirmExport()
    {
        Debug.Log("OnConfirmExport");
        ShowDialog(false);
        
        BuilderBehaviour.Instance.SaveGroup(m_Input.GetComponent<TMP_InputField>().text);
    }

    public void OnCancelExport()
    {
        Debug.Log("OnCancelExport");
        ShowDialog(false);
    }
}
