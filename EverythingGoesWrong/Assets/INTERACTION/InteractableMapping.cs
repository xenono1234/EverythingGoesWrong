using UnityEngine;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [System.Serializable]
    public struct AnimatorTriggerPair
    {
        public Animator targetAnimator;
        public string triggerName;
    }

    [System.Serializable]
    public struct TaggedGameObject
    {
        public string tag;
        public GameObject associatedUI;
        public List<AnimatorTriggerPair> animations; // Allows multiple animator-trigger pairs
    }

    [SerializeField] private List<TaggedGameObject> taggedUIElements;

    private GameObject currentActiveUI;
    private GameObject currentInteractable;
    private GameObject selectedInteractable;
    private TaggedGameObject? selectedTaggedObject;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        // Ensure all UI elements start disabled
        foreach (var taggedUI in taggedUIElements)
        {
            if (taggedUI.associatedUI != null)
            {
                taggedUI.associatedUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        HandleInteractionRaycast();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            selectedInteractable = currentInteractable;

            // Specific handling for Reactor interactions
            if (currentInteractable.CompareTag("Reactor"))
            {
                ReactorController reactorController = currentInteractable.GetComponent<ReactorController>();
                if (reactorController != null)
                {
                    if (reactorController.IsMalfunctioning)
                    {
                        reactorController.FixReactor();
                    }
                    else
                    {
                        Debug.Log("Reactor is already fixed.");
                    }
                }
                else
                {
                    Debug.LogWarning("No ReactorController found on Reactor object.");
                }
            }
            // Specific handling for Monster interactions
            else if (currentInteractable.CompareTag("Monster"))
            {
                MonsterController monsterController = currentInteractable.GetComponent<MonsterController>();
                if (monsterController != null)
                {
                    if (monsterController.IsAngry)
                    {
                        monsterController.FeedMonster();
                    }
                    else
                    {
                        Debug.Log("Monster is already calm.");
                    }
                }
                else
                {
                    Debug.LogWarning("No MonsterController found on Monster object.");
                }
            }

            // Old functionality: Trigger animations based on the taggedUIElements list
            foreach (var taggedUI in taggedUIElements)
            {
                if (taggedUI.tag == currentInteractable.tag)
                {
                    // Trigger all animations for this interactable
                    foreach (var animPair in taggedUI.animations)
                    {
                        if (animPair.targetAnimator != null && !string.IsNullOrEmpty(animPair.triggerName))
                        {
                            animPair.targetAnimator.SetTrigger(animPair.triggerName);
                            Debug.Log($"Triggered animation '{animPair.triggerName}' on animator '{animPair.targetAnimator.name}' for interactable: {currentInteractable.name}");
                        }
                    }
                    selectedTaggedObject = taggedUI;
                    break;
                }
            }
        }
    }

    private void HandleInteractionRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            if (currentInteractable != hit.collider.gameObject)
            {
                currentInteractable = hit.collider.gameObject;
                UpdateUIBasedOnTag(currentInteractable.tag);
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                DisableAllUI();
                currentInteractable = null;
            }
        }
    }

    private void UpdateUIBasedOnTag(string tag)
    {
        DisableAllUI();

        foreach (var taggedUI in taggedUIElements)
        {
            if (taggedUI.tag == tag && taggedUI.associatedUI != null)
            {
                taggedUI.associatedUI.SetActive(true);
                currentActiveUI = taggedUI.associatedUI;
                break;
            }
        }
    }

    private void DisableAllUI()
    {
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI = null;
        }
    }

    public void OnAnimationEventChangeColor()
    {
        if (selectedInteractable != null && selectedTaggedObject.HasValue)
        {
            Renderer renderer = selectedInteractable.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(Random.value, Random.value, Random.value);
                Debug.Log($"Changed color of {selectedInteractable.name} to {renderer.material.color}");
            }
            else
            {
                Debug.LogWarning("Selected interactable does not have a Renderer component.");
            }
        }
        else
        {
            Debug.LogWarning("No selected interactable or tagged object to change color.");
        }
    }
}
