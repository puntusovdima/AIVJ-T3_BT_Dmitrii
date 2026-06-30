using UnityEngine;

public class RelaxState : MonoBehaviour
{
    private SkyrimGuardAgent agent;
    private float dialogueTimer = 0f;

    private readonly string[] idleDialogues = new string[]
    {
        "I used to be an adventurer like you, then I took an arrow in the knee.",
        "Got to thinking, maybe I'm the Dragonborn, and I just don't know it yet?",
        "No lollygaggin'.",
        "A warm bed and a cold mug of mead... that's what I need.",
        "Mmm... this sweetroll is delicious. Glad nobody stole it.",
        "Hail, Summoner. Conjure me up a warm bed, would you?"
    };

    void OnEnable()
    {
        agent = GetComponent<SkyrimGuardAgent>();
        Debug.Log("<color=green>[State: Relax]</color> Guard sits down by the campfire. 'Ah, finally, a break.'");
        dialogueTimer = Random.Range(1f, 3f);
    }

    void Update()
    {
        // Guard stays stationary during their break, resting their feet.
        
        dialogueTimer -= Time.deltaTime;
        if (dialogueTimer <= 0)
        {
            string line = idleDialogues[Random.Range(0, idleDialogues.Length)];
            if (line.Contains("sweetroll"))
            {
                Debug.Log("<color=green>[Relax]</color> *Guard munches on a sweetroll happily*");
            }
            Debug.Log($"<color=green>[Relax Dialogue]</color> Guard: \"{line}\"");
            dialogueTimer = Random.Range(6f, 12f);
        }
    }

    void OnDisable()
    {
        Debug.Log("<color=green>[State: Relax]</color> Guard stood up and left relax state.");
    }
}
