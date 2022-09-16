using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterController : Human
{
    public GameObject mouseTrigger;
    public NpcController mario;
    public NpcController janis;
    public GameObject trap;
    public TextMeshProUGUI moneyTxt;
    public AudioClip moneySound;
    public AudioClip trapSound;
    public AudioClip antidoteSound;

    public Image antidoteImage;
    public Image keyImage;
    public Image defuserImage;
    public Image medicineImage;
    public Image knifeImage;
    public Image sedativeImage;
    public Image mouseImage;

    public GameObject interactImage;
    public AudioSource subAudio;

    bool inputActive = true;
    int itemSelection = 0;

    bool hasAntidote = false;
    bool hasMouse = false;
    bool hasKnife = false;
    bool hasKey = false;
    bool hasMedicine = false;
    bool hasTrapDefuser = false;
    bool hasSedative = false;

    void Update()
    {
        moneyTxt.text = money.ToString();
        if (inputActive)
        {
            if (!moving)
            {
                x = 0;
                y = 0;
                if (Input.GetKey(KeyCode.W))
                {
                    y += 1;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    x -= 1;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    y -= 1;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    x += 1;
                }
                if (x != 0 || y != 0)
                {
                    anim.SetInteger("dirX", x);
                    anim.SetInteger("dirY", y);
                    anim.SetInteger("speed", (int)speed);
                    moveStart();
                }
                else
                {
                    anim.SetInteger("speed", 0);
                    aud.Stop();
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0)
            {
                changeSelection(-1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0)
            {
                changeSelection(1);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                useItem();
            }
            if (checkShop())
            {
                interactImage.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    shopRequest();
                }
            }
            else
            {
                interactImage.SetActive(false);
            }
        }
        else
        {
            interactImage.SetActive(false);
        }
    }

    void useItem()
    {
        if (itemSelection == 1 && hasKnife)
        {
            kill(lastDir, true);
        }
        if (itemSelection == 2 && hasSedative)
        {
            if (sedateNpc(lastDir))
            {
                changeSelection(1);
                hasSedative = false;
                sedativeImage.enabled = false;
            }
        }
        if (itemSelection == 3 && hasMouse && (checkWall(lastDir.x, lastDir.y) == lastDir))
        {
            changeSelection(1);
            hasMouse = false;
            Instantiate(mouseTrigger, transform.position + (Vector3Int)lastDir, Quaternion.identity);
        }
    }

    void changeSelection(int n)
    {
        int itemsBin = 0;
        if (hasKnife) itemsBin += 1;
        if (hasSedative) itemsBin += 2;
        if (hasMouse) itemsBin += 4;

        if (itemsBin == 7)
        {
            itemSelection = itemSelection + n;
            if (itemSelection == 0) itemSelection = 3;
            if (itemSelection == 4) itemSelection = 1;
        }
        else if (itemsBin == 0 || itemsBin == 1 || itemsBin == 2) itemSelection = itemsBin;
        else if (itemsBin == 3 && n != 0) itemSelection = 3 - itemSelection;
        else if (itemsBin == 4) itemSelection = 3;
        else if ((itemsBin == 5 || itemsBin == 6) && n != 0) itemSelection = itemsBin - 1 - itemSelection;

        switch (itemSelection)
        {
            case 0:
                knifeImage.enabled = false;
                sedativeImage.enabled = false;
                mouseImage.enabled = false;
                break;
            case 1:
                knifeImage.enabled = true;
                sedativeImage.enabled = false;
                mouseImage.enabled = false;
                break;
            case 2:
                knifeImage.enabled = false;
                sedativeImage.enabled = true;
                mouseImage.enabled = false;
                break;
            case 3:
                knifeImage.enabled = false;
                sedativeImage.enabled = false;
                mouseImage.enabled = true;
                break;
        }
    }

    void setSelection(int s)
    {
        itemSelection = s;
        switch (itemSelection)
        {
            case 0:
                knifeImage.enabled = false;
                sedativeImage.enabled = false;
                mouseImage.enabled = false;
                break;
            case 1:
                knifeImage.enabled = true;
                sedativeImage.enabled = false;
                mouseImage.enabled = false;
                break;
            case 2:
                knifeImage.enabled = false;
                sedativeImage.enabled = true;
                mouseImage.enabled = false;
                break;
            case 3:
                knifeImage.enabled = false;
                sedativeImage.enabled = false;
                mouseImage.enabled = true;
                break;
        }
    }

    bool sedateNpc(Vector2Int dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1, LayerMask.GetMask("Human", "Baron"));
        if (hit)
        {
            money += hit.transform.GetComponent<NpcController>().sedate();
            playMoney();
            return true;
        }
        return false;
    }

    bool checkShop()
    {
        RaycastHit2D h;
        h = Physics2D.Raycast(transform.position, lastDir, 1.3f);
        if (h && h.collider.gameObject.GetComponent<NpcController>() != null)
        {
            if (h.collider.gameObject.GetComponent<NpcController>().shop || h.collider.gameObject.GetComponent<NpcController>().gamble)
            {
                if (h.collider.gameObject.GetComponent<NpcController>().isAlive() && !h.collider.gameObject.GetComponent<NpcController>().isSedated()){
                    return true;
                }
            }
        }
        return false;
    }

    void shopRequest()
    {
        RaycastHit2D h;
        h = Physics2D.Raycast(transform.position, lastDir, 1);
        if (h && h.collider.gameObject.GetComponent<NpcController>() != null)
        {
            if (h.collider.gameObject.GetComponent<NpcController>().shop)
            {
                setInputActive(false);
                FindObjectOfType<GameManager>().openShop(h.collider.gameObject.GetComponent<NpcController>());
            }
            if (h.collider.gameObject.GetComponent<NpcController>().gamble)
            {
                if (money >= h.collider.gameObject.GetComponent<NpcController>().moneyPay)
                {
                    money -= h.collider.gameObject.GetComponent<NpcController>().moneyPay;
                    setInputActive(false);
                    FindObjectOfType<GameManager>().openGamble(h.collider.gameObject.GetComponent<NpcController>());
                }
                else
                {
                    FindObjectOfType<GameManager>().playWrong();
                    FindObjectOfType<GameManager>().logMessage("Not enough money", Color.magenta);
                }
            }
        }
    }

    public void boughtItem(int id)
    {
        switch (id)
        {
            case 0:
                hasKnife = true;
                setSelection(1);
                break;
            case 1:
                hasKey = true;
                keyImage.enabled = true;
                break;
            case 2:
                hasMedicine = true;
                medicineImage.enabled = true;
                break;
            case 3:
                hasTrapDefuser = true;
                defuserImage.enabled = true;
                break;
            case 4:
                hasSedative = true;
                setSelection(2);
                break;
        }
    }

    new protected void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.tag == "Antidote")
        {
            subAudio.PlayOneShot(antidoteSound);
            hasAntidote = true;
            Destroy(collision.gameObject);
            antidoteImage.enabled = true;
        }
        if (collision.tag == "Mouse")
        {
            hasMouse = true;
            setSelection(3);
            Destroy(collision.gameObject);
        }
        if (collision.tag == "TrapTrigger" && hasTrapDefuser && trap != null)
        {
            subAudio.PlayOneShot(trapSound);
            Destroy(trap);
            hasTrapDefuser = false;
            defuserImage.enabled = false;
        }
        if (collision.tag == "MedicineTrigger" && hasMedicine && janis.isAlive())
        {
            janis.triggerActions();
            hasMedicine = false;
            medicineImage.enabled = false;
            money += 2;
            subAudio.PlayOneShot(moneySound);
        }
        if (collision.tag == "End")
        {
            if (hasAntidote)
            {
                FindObjectOfType<GameManager>().saved();
            }
        }
    }

    public bool checkHasKey()
    {
        return hasKey;
    }

    public void setInputActive(bool active)
    {
        inputActive = active;
    }

    public bool isAlive()
    {
        return alive;
    }

    public void playMoney()
    {
        subAudio.PlayOneShot(moneySound);
    }

    public void gameEnded()
    {
        setInputActive(false);
        if (alive) aud.Stop();
        anim.SetInteger("speed", 0);
    }
}
