using UnityEngine;
using TMPro;
using System.Collections;

public class MasterGamePlan : MonoBehaviour
{
    public MonsterController monster;
    public ReactorController reactor;

    // Reactor safe time range and countdown duration
    public float reactorSafeTimeMin = 5f;
    public float reactorSafeTimeMax = 15f;
    public float reactorCountdownTime = 5f;

    // Monster safe time range and countdown duration
    public float monsterSafeTimeMin = 5f;
    public float monsterSafeTimeMax = 15f;
    public float monsterCountdownTime = 5f;

    public TMP_Text reactorCountdownText;
    public TMP_Text monsterCountdownText;

    // Research mechanic variables
    [Tooltip("Time in minutes that the player must spend inside the control room to complete research.")]
    public float researchDurationMinutes = 3f; // X minutes for research
    public TMP_Text researchTimerText;
    [Tooltip("Reference to the player GameObject.")]
    public GameObject player;

    // New game objects:
    [Tooltip("Game object to be activated after research is complete.")]
    public GameObject researchReward;
    [Tooltip("Bonus game object to be enabled when the player enters the research reward's collider.")]
    public GameObject bonusObject;

    private float researchTimeRemaining;
    private bool isPlayerInControlRoom = false;
    private bool researchCompleted = false;
    private bool bonusActivated = false;

    private void Start()
    {
        // Initialize reactor and monster texts.
        if (reactorCountdownText != null)
            reactorCountdownText.text = "Reactor is ok";
        if (monsterCountdownText != null)
            monsterCountdownText.text = "Monster is happy";

        // Initialize research timer (convert minutes to seconds).
        researchTimeRemaining = researchDurationMinutes * 60f;
        if (researchTimerText != null)
            researchTimerText.text = researchDurationMinutes + " mins left for completing research";

        // Ensure new game objects are inactive at the start.
        if (researchReward != null)
            researchReward.SetActive(false);
        if (bonusObject != null)
            bonusObject.SetActive(false);

        StartCoroutine(ReactorCycle());
        StartCoroutine(MonsterCycle());
    }

    private void Update()
    {
        if (researchTimerText == null)
            return;

        // Update the research timer if research is still in progress.
        if (researchTimeRemaining > 0)
        {
            int minutesLeft = Mathf.FloorToInt(researchTimeRemaining / 60f);
            int secondsLeft = Mathf.FloorToInt(researchTimeRemaining % 60f);

            if (isPlayerInControlRoom)
            {
                researchTimerText.text = minutesLeft + " mins " + secondsLeft + " secs left for completing research";
                researchTimeRemaining -= Time.deltaTime;
                if (researchTimeRemaining < 0)
                    researchTimeRemaining = 0;
            }
            else
            {
                researchTimerText.text = "To continue research, go into the control area. " 
                    + minutesLeft + " mins " + secondsLeft + " secs left.";
            }
        }
        else
        {
            researchTimerText.text = "Research complete!";
            if (!researchCompleted)
            {
                researchCompleted = true;
                if (researchReward != null)
                {
                    researchReward.SetActive(true);
                    Debug.Log("Research reward activated.");
                }
            }
        }

        // Check if the player has entered the researchReward's collider to enable bonusObject.
        if (researchCompleted && !bonusActivated && researchReward != null && bonusObject != null)
        {
            Collider rewardCollider = researchReward.GetComponent<Collider>();
            if (rewardCollider != null && rewardCollider.bounds.Contains(player.transform.position))
            {
                bonusActivated = true;
                bonusObject.SetActive(true);
                Debug.Log("Player entered research reward area. Bonus object activated.");
            }
        }
    }

    // Reactor cycle: safe period, problem period, then reset.
    private IEnumerator ReactorCycle()
    {
        while (true)
        {
            float safeTime = Random.Range(reactorSafeTimeMin, reactorSafeTimeMax);
            if (reactorCountdownText != null)
                reactorCountdownText.text = "Reactor is ok";
            yield return new WaitForSeconds(safeTime);

            SetReactorState(true);
            float timeRemaining = reactorCountdownTime;
            while (timeRemaining > 0)
            {
                if (reactorCountdownText != null)
                    reactorCountdownText.text = timeRemaining.ToString("F0") + " seconds until explosion";
                yield return new WaitForSeconds(1f);
                timeRemaining--;

                if (!reactor.IsMalfunctioning)
                {
                    if (reactorCountdownText != null)
                    {
                        reactorCountdownText.text = "Reactor Fixed";
                        yield return new WaitForSeconds(2f);
                        reactorCountdownText.text = "Reactor is ok";
                    }
                    break;
                }
            }
            if (timeRemaining <= 0 && reactor.IsMalfunctioning)
            {
                if (reactorCountdownText != null)
                    reactorCountdownText.text = "BOOM!";
                reactor.Explode();
                yield return new WaitForSeconds(2f);
            }
            SetReactorState(false);
        }
    }

    // Monster cycle: safe period, problem period, then reset.
    private IEnumerator MonsterCycle()
    {
        while (true)
        {
            float safeTime = Random.Range(monsterSafeTimeMin, monsterSafeTimeMax);
            if (monsterCountdownText != null)
                monsterCountdownText.text = "Monster is happy";
            yield return new WaitForSeconds(safeTime);

            SetMonsterMood(true);
            float timeRemaining = monsterCountdownTime;
            while (timeRemaining > 0)
            {
                if (monsterCountdownText != null)
                    monsterCountdownText.text = timeRemaining.ToString("F0") + " seconds until escape";
                yield return new WaitForSeconds(1f);
                timeRemaining--;

                if (!monster.IsAngry)
                {
                    if (monsterCountdownText != null)
                    {
                        monsterCountdownText.text = "This monster seems to eat a lot!";
                        yield return new WaitForSeconds(2f);
                        monsterCountdownText.text = "Monster is happy";
                    }
                    break;
                }
            }
            if (timeRemaining <= 0 && monster.IsAngry)
            {
                if (monsterCountdownText != null)
                    monsterCountdownText.text = "ATTACK!";
                monster.Attack();
                yield return new WaitForSeconds(2f);
            }
            SetMonsterMood(false);
        }
    }

    // Directly sets monster mood.
    public void SetMonsterMood(bool angry)
    {
        if (monster != null)
        {
            monster.SetMood(angry);
            Debug.Log("Monster: " + (angry ? "Angry" : "Happy"));
        }
        else
        {
            Debug.LogWarning("MonsterController missing.");
        }
    }

    // Directly sets reactor state.
    public void SetReactorState(bool malfunction)
    {
        if (reactor != null)
        {
            reactor.SetReactorState(malfunction);
            Debug.Log("Reactor: " + (malfunction ? "Bad" : "Ok"));
        }
        else
        {
            Debug.LogWarning("ReactorController missing.");
        }
    }

    // Detect when the player enters the control room trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            isPlayerInControlRoom = true;
            Debug.Log("Player entered the control area.");
        }
    }

    // Detect when the player exits the control room trigger.
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            isPlayerInControlRoom = false;
            Debug.Log("Player left the control area.");
        }
    }
}
