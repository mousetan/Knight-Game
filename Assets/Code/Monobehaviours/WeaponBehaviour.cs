using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class WeaponBehaviour : MonoBehaviour
{
    public WeaponData weaponData;
    private int hurtboxLayer = 6;
    private LayerMask hurtboxLayerMask;
    private Transform playerTransform;

    //public event UnityAction<int, > hitEvent = delegate { };

    private void Start()
    {
        playerTransform = PlayerController.Instance.transform;
        hurtboxLayerMask = (1 << hurtboxLayer);
    }

    public void HitAnimationEvent()
    {
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out var hitInfo, weaponData.range, hurtboxLayerMask))
        {
            Debug.DrawRay(playerTransform.position, playerTransform.forward * weaponData.range, Color.green, 1f);
            //Debug.Log("Hit: " + hitInfo.ToString());
            hitInfo.transform.parent.GetComponent<EnemyController>().TakeDamage(weaponData.attackDamage);
            //hitEvent?.Invoke(weaponData.attackDamage);
        }
        else
        {
            //Debug.Log("Missed!");
            Debug.DrawRay(playerTransform.position, playerTransform.forward * weaponData.range, Color.red, 1f);
        }
    }
}
