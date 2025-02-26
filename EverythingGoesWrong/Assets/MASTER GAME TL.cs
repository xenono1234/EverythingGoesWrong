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

    private void Start()
    {
        if (reactorCountdownText != null)
            reactorCountdownText.text = "Reactor is ok";
        if (monsterCountdownText != null)
            monsterCountdownText.text = "Monster is happy";

        StartCoroutine(ReactorCycle());
        StartCoroutine(MonsterCycle());
    }

    // Reactor cycle: safe period, problem period, then reset.
    private IEnumerator ReactorCycle()
    {
        while (true)
        {
            // Safe state
            float safeTime = Random.Range(reactorSafeTimeMin, reactorSafeTimeMax);
            if (reactorCountdownText != null)
                reactorCountdownText.text = "Reactor is ok";
            yield return new WaitForSeconds(safeTime);

            // Reactor goes bad
            SetReactorState(true);
            float timeRemaining = reactorCountdownTime;
            while (timeRemaining > 0)
            {
                if (reactorCountdownText != null)
                    reactorCountdownText.text = timeRemaining.ToString("F0") + " seconds until explosion";
                yield return new WaitForSeconds(1f);
                timeRemaining--;

                // Reactor fixed before explosion
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
            // Reset reactor for next cycle.
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

            // Monster becomes angry
            SetMonsterMood(true);
            float timeRemaining = monsterCountdownTime;
            while (timeRemaining > 0)
            {
                if (monsterCountdownText != null)
                    monsterCountdownText.text = timeRemaining.ToString("F0") + " seconds until escape";
                yield return new WaitForSeconds(1f);
                timeRemaining--;

                // Monster fed before escape
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
            // Reset monster for next cycle.
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
        else Debug.LogWarning("MonsterController missing.");
    }

    // Directly sets reactor state.
    public void SetReactorState(bool malfunction)
    {
        if (reactor != null)
        {
            reactor.SetReactorState(malfunction);
            Debug.Log("Reactor: " + (malfunction ? "Bad" : "Ok"));
        }
        else Debug.LogWarning("ReactorController missing.");
    }
}
