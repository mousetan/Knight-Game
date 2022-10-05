using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Passive,
        Aggressive,
        Dead
    }

    [HideInInspector] public EnemyState state;
    private int fixedUpdateCounter = 0;
    private int zombieGroanCount = 200; // divide by 50 to get number of seconds, min value = 150
    private int randomOffset;
    [SerializeField] private SfxClips enemySfx;
    //private Transform playerTransform;
    [SerializeField] private WeaponBehaviour playerWeaponBehaviour;

    private int maxHealth = 10;
    [HideInInspector] public int currentHealth;

    private void Start()
    {
        //playerTransform = PlayerController.Instance.transform;
        currentHealth = maxHealth;
        randomOffset = Random.Range(0, zombieGroanCount-1);
        GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth > 0) 
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
        }
    }

    private void Update()
    {
        switch (state)
        {
            case EnemyState.Passive:
                break;
            case EnemyState.Aggressive:
                //MoveInPlayerDirectionUntilInRange();
                break;
        }
    }

    private void FixedUpdate()
    {
        fixedUpdateCounter++;
        switch (state)
        {
            case EnemyState.Passive:
                if (fixedUpdateCounter % (zombieGroanCount + randomOffset) == 0 && Random.Range(0f,1f) > 0f)
                    GetComponent<AudioSource>().PlayOneShot(enemySfx.clips[2]);
                break;
            case EnemyState.Aggressive:
                //MoveInPlayerDirectionUntilInRange();
                break;
        }
    }

    private void Die()
    {
        GetComponent<AudioSource>().PlayOneShot(enemySfx.clips[1]);
        state = EnemyState.Dead;
        Debug.Log("Zombie Died!");
        //transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        // play death animation
        // drop loot or give exp...
    }

    //private void MoveInPlayerDirectionUntilInRange()
    //{
    // // use navmeshes instead, set goal, etc... https://docs.unity3d.com/Manual/nav-NavigationSystem.html
    //}
}