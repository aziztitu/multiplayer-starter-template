using UnityEngine;
using UnityEngine.UI;

public class LightCyclePlayerHUD : MonoBehaviour
{
    public Image JumpIcon;
    public float disabledOpacity = 0.5f;

    LightCycle currentLightCycle
    {
        get
        {
            if (LightCycleLevelManager.Instance.IsSpectating)
            {
                return LightCycleLevelManager.Instance.spectatingLightCycle;
            }

            return LightCycle.LocalClientInstance;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentLightCycle)
        {
            return;
        }

        var color = JumpIcon.color;
        color.a = currentLightCycle.HasPowerUp_Jump.Value ? 1f : disabledOpacity;
        JumpIcon.color = color;
    }
}
