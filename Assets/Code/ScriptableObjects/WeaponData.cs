using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public float range;
    public float attackCooldown;
    public int attackDamage;
    public GameObject prefab;
}
