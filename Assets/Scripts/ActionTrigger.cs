using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTrigger : MonoBehaviour
{
    public string requiredTag;
    public NpcController character;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(requiredTag))
        {
            if (character.guard)
                character.removeGuard();
            else
                character.triggerActions();
            Destroy(transform.gameObject);
        }
    }
}
