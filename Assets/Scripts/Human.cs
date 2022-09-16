using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Human : MonoBehaviour
{
    public float speed;
    public string charName;
    public TextMeshProUGUI nameTxt;
    public AudioSource audDeath;
    public AudioClip deathSound;
    public int money;

    protected bool alive = true;
    protected bool sedated = false;
    protected bool moving = false;
    protected bool lastHorizontal = true;
    protected int x;
    protected int y;
    protected Vector2Int lastDir;
    protected Animator anim;
    protected AudioSource aud;

    protected void Start()
    {
        anim = GetComponent<Animator>();
        aud = GetComponent<AudioSource>();
        nameTxt.text = charName;
    }

    protected void moveStart()
    {
        lastDir = new Vector2Int(x, y);
        Vector2Int newDir = checkWall(x, y);
        x = newDir.x;
        y = newDir.y;
        if (x != 0 && y != 0)
        {
            if (lastHorizontal)
            {
                lastHorizontal = false;
                //x = 0;
            }
            else
            {
                lastHorizontal = true;
                //y = 0;
            }
        }
        if (x != 0 || y != 0)
        {
            if (!aud.isPlaying && speed > 0)
                aud.Play();
            moving = true;
            if (x != 0 && y != 0)
                lastDir = new Vector2Int(newDir.x, 0);
            else
            {
                lastDir = newDir;
                anim.SetInteger("dirX", newDir.x);
                anim.SetInteger("dirY", newDir.y);
            }
            StartCoroutine(move(x, y));
        }
    }

    protected Vector2Int checkWall(int x, int y)
    {
        if (!CompareTag("ClipWalls"))
        {
            float dist = 1;
            if (x != 0 && y != 0) dist = 1.41f;
            Vector2 dir = new Vector2(x, y);
            Vector2 dirX = new Vector2(x, 0);
            Vector2 dirY = new Vector2(0, y);
            LayerMask mask = ~LayerMask.GetMask("Human", "Trigger");
            RaycastHit2D hit;
            if (CompareTag("Player"))
                mask = ~LayerMask.GetMask("Trigger");
            Vector3 pos1 = transform.position + new Vector3(1 / 32f, .1f * y, 0);
            Vector3 pos2 = transform.position + new Vector3(1 / 32f, - .1f * y, 0);
            if ((hit = Physics2D.Raycast(pos1, dir, dist, mask)) || (hit = Physics2D.Raycast(pos2, dir, dist, mask)))
            {
                if (!hit.collider.CompareTag("Dead") && !hit.collider.CompareTag("Sedated"))
                {
                    bool orthogonalFree = true;
                    if (Physics2D.Raycast(pos1, dirX, Mathf.Abs(x), mask))
                    {
                        orthogonalFree = false;
                        x = 0;
                    }
                    if (Physics2D.Raycast(pos1, dirY, Mathf.Abs(y), mask))
                    {
                        orthogonalFree = false;
                        y = 0;
                    }
                    if (orthogonalFree)
                    {
                        x = 0;
                    }
                }
            }
        }
        return new Vector2Int(x, y);
    }

    protected IEnumerator move(float x, float y)
    {
        Vector2 startPos = transform.position;
        Vector2 destination = startPos + new Vector2(x, y);
        float totalTime = 1 / speed;
        if (x != 0 && y != 0)
            totalTime *= 1.41f;
        float timePassed = totalTime;
        while (timePassed > 0 && speed > 0)
        {
            transform.position = startPos + ((destination - startPos) * (1 - timePassed / totalTime));
            timePassed -= Time.deltaTime;
            yield return null;
        }
        transform.position = destination;
        moving = false;
    }

    protected void kill(Vector2 dir, bool player = false)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1, LayerMask.GetMask("Human", "Baron"));
        if (hit) {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Baron"))
            {
                FindObjectOfType<GameManager>().setBaronKilled();
            }
            if (player)
            {
                FindObjectOfType<GameManager>().increaseKilled();
                if (hit.transform.gameObject.CompareTag("John"))
                {
                    GetComponent<CharacterController>().mario.triggerActions();
                }
            }
            money += hit.transform.GetComponent<Human>().die();
        }
    }

    public int die()
    {
        if (alive)
        {
            if (GetComponent<NpcController>() != null)
            {
                GetComponent<NpcController>().hideDialog();
                GetComponent<NpcController>().shop = false;
                GetComponent<NpcController>().gamble = false;
                GetComponent<NpcController>().removeGuard();
            }
            FindObjectOfType<GameManager>().increaseDead();
            FindObjectOfType<GameManager>().logMessage(name + " died", Color.red);
            gameObject.layer = LayerMask.NameToLayer("Human");
            gameObject.tag = "Dead";
            GetComponent<SpriteRenderer>().sortingOrder = 3;
            alive = false;
            nameTxt.color = Color.red;
            GetComponent<SpriteRenderer>().color = Color.grey;
            transform.rotation = Quaternion.Euler(0, 0, -90);
            anim.SetInteger("speed", 0);
            aud.Stop();

            if (GetComponent<CharacterController>() != null)
            {
                StartCoroutine(playDeath());
                GetComponent<CharacterController>().setInputActive(false);
                FindObjectOfType<GameManager>().endGame();
            }
            else
            {
                audDeath.PlayOneShot(deathSound);
            }

            return money;
        }
        return 0;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Trap")
        {
            die();
            Destroy(collision.gameObject);
        }
        if (collision.tag == "Treasure")
        {
            if (GetComponent<CharacterController>()) {
                GetComponent<CharacterController>().playMoney();
            }
            money += FindObjectOfType<GameManager>().treasureMoney;
            Destroy(collision.gameObject);
        }
    }

    IEnumerator playDeath()
    {
        aud.PlayOneShot(deathSound);
        yield return new WaitForSeconds(.2f);
        aud.Stop();
    }
}
