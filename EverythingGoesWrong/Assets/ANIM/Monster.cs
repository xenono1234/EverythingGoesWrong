using UnityEngine;

public class MonsterController : MonoBehaviour
{
    // Reference to the Animator component.
    public Animator animator;

    // The parameter name used in the Animator to control mood.
    // True = Angry, False = Happy.
    [SerializeField] private string moodParameter = "IsAngry";

    private bool isAngry = false;

    // Public getter to check the current mood.
    public bool IsAngry {
        get { return isAngry; }
    }

    // Attack simulation: assign attack fragments in the Inspector.
    public GameObject[] attackFragments;
    public float attackForce = 300f;
    public float attackRadius = 5f;

    // Call this method to set the monster's mood.
    // Pass true to make the monster angry, false to make it happy.
    public void SetMood(bool angry)
    {
        isAngry = angry;
        if (animator != null)
        {
            animator.SetBool(moodParameter, angry);
        }
        else
        {
            Debug.LogWarning("Animator not assigned in MonsterController.");
        }
    }

    // Call this method to feed the monster, calming it down.
    public void FeedMonster()
    {
        SetMood(false);
        Debug.Log("Monster fed and calmed down.");
    }

    // Simulate an attack: instantiate attack fragments and apply random force.
    public void Attack()
    {
        Debug.Log("Monster is attacking!");
        foreach (GameObject fragment in attackFragments)
        {
            if (fragment != null)
            {
                GameObject frag = Instantiate(fragment, transform.position, Random.rotation);
                Rigidbody rb = frag.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = frag.AddComponent<Rigidbody>();
                }
                Vector3 randomDir = Random.onUnitSphere;
                rb.AddForce(randomDir * attackForce);
            }
        }
        // Optionally, trigger further logic (for example, damaging the player).
    }
}
