using UnityEngine;

public class FusionHandleAnimationProxy : MonoBehaviour
{
    public FusionatorController fusionatorController;

    public void HandleFusionEnd()
    {
        fusionatorController.HandleFusionEnd();
    }
}
