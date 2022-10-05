using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class WeaponBehaviour : MonoBehaviour
{
    public WeaponData weaponData;
    private int hurtboxLayer = 6;
    private LayerMask hurtboxLayerMask;
    private Transform playerTransform;
    [SerializeField] private GameObject swordSparks;
    [SerializeField] private GameObject bloodSplat;

    private void Start()
    {
        playerTransform = PlayerController.Instance.transform;
        hurtboxLayerMask = (1 << hurtboxLayer);
    }

    public void HitAnimationEvent()
    {
        //Instantiate(bloodSplat, playerTransform.position + weaponData.range * playerTransform.forward, Quaternion.identity);
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out var hitInfo, weaponData.range, hurtboxLayerMask))
        {
            Debug.DrawRay(playerTransform.position, playerTransform.forward * weaponData.range, Color.green, 1f);
            hitInfo.transform.parent.GetComponent<EnemyController>().TakeDamage(weaponData.attackDamage);
            //if (hitInfo.transform.parent.GetComponent<EnemyController>().state != EnemyController.EnemyState.Dead)
            GetComponent<AudioSource>().PlayOneShot(transform.parent.parent.GetComponent<PlayerController>().playerSfx.clips[2]);
            Instantiate(bloodSplat, hitInfo.point, Quaternion.identity);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, playerTransform.forward * weaponData.range, Color.red, 1f);
        }
    }
}
