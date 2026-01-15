using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Summonable Unit Data", fileName = "SummonableUnitData")]
public class SummonableUnitData : ScriptableObject
{
    public string unitName;
    public Sprite unitImage;
    public int duration;
    public RuntimeAnimatorController animationController;
    public Vector3 targetPosition;

}