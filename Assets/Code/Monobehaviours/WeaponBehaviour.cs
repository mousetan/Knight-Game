using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class WeaponBehaviour : MonoBehaviour
{
    public WeaponData weaponData;
    private int hurtboxLayer = 6;
    private LayerMask hurtboxLayerMask;
    private Transform eyeTransform;
    [SerializeField] private GameObject swordSparks;
    [SerializeField] private GameObject bloodSplat;

    private void Start()
    {
        eyeTransform = PlayerController.Instance.transform.GetChild(0);
        hurtboxLayerMask = (1 << hurtboxLayer);
    }

    public void HitAnimationEvent()
    {
        //Instantiate(bloodSplat, playerTransform.position + weaponData.range * playerTransform.forward, Quaternion.identity);
        if (Physics.Raycast(eyeTransform.position, eyeTransform.forward, out var hitInfo, weaponData.range, hurtboxLayerMask))
        {
            Debug.DrawRay(eyeTransform.position, eyeTransform.forward * weaponData.range, Color.green, 1f);
            hitInfo.transform.parent.GetComponent<EnemyController>().TakeDamage(weaponData.attackDamage);
            //if (hitInfo.transform.parent.GetComponent<EnemyController>().state != EnemyController.EnemyState.Dead)
            GetComponent<AudioSource>().PlayOneShot(transform.parent.parent.GetComponent<PlayerController>().playerSfx.clips[2]);
            Instantiate(bloodSplat, hitInfo.point, Quaternion.identity);
        }
        else if (Physics.Raycast(eyeTransform.position, eyeTransform.forward, out var hitInfoWall, weaponData.range))
        {
            GetComponent<AudioSource>().PlayOneShot(transform.parent.parent.GetComponent<PlayerController>().playerSfx.clips[1]);
            //Instantiate(swordSparks, hitInfoWall.point, Quaternion.identity);
            Debug.DrawRay(eyeTransform.position, eyeTransform.forward * weaponData.range, Color.green, 1f);
        }
        else
        {
            Debug.DrawRay(eyeTransform.position, eyeTransform.forward * weaponData.range, Color.red, 1f);
        }
    }
}
