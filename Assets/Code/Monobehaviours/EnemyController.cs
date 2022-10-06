using UnityEngine;
using UnityEngine.AI;

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
    private int zombieGroanCounter = 1500; 
    private int randomOffset;
    [SerializeField] private SfxClips enemySfx;

    private NavMeshAgent agent;
    private Transform goal;
    [SerializeField] private WeaponBehaviour playerWeaponBehaviour;

    private int maxHealth = 10;
    [HideInInspector] public int currentHealth;

    private void Start()
    {
        //playerTransform = PlayerController.Instance.transform;
        currentHealth = maxHealth;
        randomOffset = Random.Range(0, zombieGroanCounter - 1);
        GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);
        agent = GetComponent<NavMeshAgent>();
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
                break;
        }
    }

    private void FixedUpdate()
    {
        fixedUpdateCounter++;
        switch (state)
        {
            case EnemyState.Passive:
                if (fixedUpdateCounter % (zombieGroanCounter + randomOffset) == 0 && Random.Range(0f, 1f) > 0f)
                    GetComponent<AudioSource>().PlayOneShot(enemySfx.clips[2]);
                goal = PlayerController.Instance.transform;
                agent.destination = goal.position;
                break;
            case EnemyState.Aggressive:
                break;
        }
    }

    private void Die()
    {
        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(enemySfx.clips[1]);
        state = EnemyState.Dead;
        GetComponent<NavMeshAgent>().enabled = false;
        //transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        // play death animation
        // drop loot or give exp...
    }
}