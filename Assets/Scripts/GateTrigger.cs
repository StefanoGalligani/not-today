using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public NpcController aldous;
    public GameObject closed;
    public GameObject opened;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Francis")) {
            aldous.triggerActions();
            Destroy(closed.gameObject);
            opened.SetActive(true);
            opened.GetComponent<AudioSource>().Play();
            Destroy(gameObject);
        } else if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<CharacterController>().checkHasKey()){
            aldous.triggerActions();
            Destroy(closed.gameObject);
            opened.SetActive(true);
            opened.GetComponent<AudioSource>().Play();
            collision.GetComponent<CharacterController>().keyImage.enabled = false;
            Destroy(gameObject);
        }
    }
}
