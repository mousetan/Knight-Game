using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Passive,
        Aggresive
    }

    private EnemyState state;
    [SerializeField] private SfxClips enemySfx;
    //private Transform playerTransform;
    [SerializeField] private WeaponBehaviour playerWeaponBehaviour;

    private int maxHealth = 10;
    [HideInInspector] public int currentHealth;

    private void Start()
    {
        //playerTransform = PlayerController.Instance.transform;
        currentHealth = maxHealth;
    }


    private void OnEnable()
    {
        //playerWeaponBehaviour = PlayerController.Instance.transform.GetChild(0).GetComponentInChildren<WeaponBehaviour>();
        playerWeaponBehaviour.hitEvent += TakeDamage;
    }

    private void TakeDamage(int damage)
    {
        if (currentHealth > 0) 
        {
            GetComponent<AudioSource>().PlayOneShot(enemySfx.clips[0]);
            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
        }
    }


    private void OnDisable()
    {
        playerWeaponBehaviour.hitEvent -= TakeDamage;
    }

    private void Update()
    {
        switch (state)
        {
            case EnemyState.Passive:
                break;
            case EnemyState.Aggresive:
                //MoveInPlayerDirectionUntilInRange();
                break;
        }
    }

    private void Die()
    {
        transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        // do death stuff:
        // play sfx
        // play death animation
        // drop loot or give exp...
    }

    //private void MoveInPlayerDirectionUntilInRange()
    //{
    // // use navmeshes instead, set goal, etc... https://docs.unity3d.com/Manual/nav-NavigationSystem.html
    //}

}