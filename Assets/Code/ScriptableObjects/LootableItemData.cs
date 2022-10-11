using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Lootable Item Data")]
public class LootableItemData : ScriptableObject
{
    public string itemName;
    public Texture2D itemIcon;
    public AudioClip pickupSound;
    public AudioClip useSound;
    public GameObject prefab;
}
