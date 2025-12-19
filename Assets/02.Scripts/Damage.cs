using UnityEngine;

// 구조체: 
public struct Damage
{
    public float Value;
    public Vector3 HitDirection;
    public Vector3 HitPoint;
    public Vector3 Normal;
    public GameObject Who;
    public bool Critical;
}
