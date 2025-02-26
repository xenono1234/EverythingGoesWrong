using UnityEngine;

public class ReactorController : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private string reactorParameter = "IsMalfunctioning";

    private bool isMalfunctioning = false;

    public bool IsMalfunctioning {
        get { return isMalfunctioning; }
    }

    // Explosion fragments (pre-existing objects in the scene)
    public GameObject[] explosionFragments;
    public float explosionForce = 300f;
    public float explosionRadius = 5f;

    // Optional: Character Controller (if assigned, it will be disabled first)
    public CharacterController characterController;

    public void SetReactorState(bool malfunction)
    {
        isMalfunctioning = malfunction;
        if (animator != null)
        {
            animator.SetBool(reactorParameter, malfunction);
        }
        else
        {
            Debug.LogWarning("Animator not assigned in ReactorController.");
        }
    }

    public void FixReactor()
    {
        SetReactorState(false);
        Debug.Log("Reactor fixed.");
    }

    public void Explode()
    {
        Debug.Log("Reactor exploded!");

        // Disable character controller if assigned
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("Character Controller disabled before explosion.");
        }

        foreach (GameObject fragment in explosionFragments)
        {
            if (fragment != null)
            {
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = fragment.AddComponent<Rigidbody>();
                }

                // Calculate force direction (Upwards & Outwards)
                Vector3 explosionDirection = (fragment.transform.position - transform.position).normalized;
                explosionDirection.y = Mathf.Abs(explosionDirection.y) + 0.5f; // Ensure upward force

                rb.AddForce(explosionDirection * explosionForce);
            }
        }

        // Optionally, disable the reactor
        gameObject.SetActive(false);
    }
}
