using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateOnKeypress : MonoBehaviour
{
    public KeyCode ActivationKey = KeyCode.LeftControl;
    public int PriorityBoostAmount = 10;
    public GameObject Reticle;

    Cinemachine.CinemachineVirtualCameraBase vcam;
    bool boosted = false;
    bool pressed = false;

    void Start()
    {
        vcam = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
    }

    void Update()
    {
        if (vcam != null)
        {
            if (pressed)
            {
                if (!boosted)
                {
                    vcam.Priority += PriorityBoostAmount;
                    boosted = true;
                }
            }
            else if (boosted)
            {
                vcam.Priority -= PriorityBoostAmount;
                boosted = false;
            }
        }
        if (Reticle != null)
            Reticle.SetActive(boosted);
    }

    void OnAim(InputValue value) {
        pressed = value.isPressed;
    }
}
