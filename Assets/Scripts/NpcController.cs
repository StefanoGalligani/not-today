using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NpcController : Human
{
    public string dialog;
    public AudioClip dialogSound;
    public Vector2Int startDir;
    public bool shop;
    public bool gamble;
    public int gambleSolution;
    public int moneyPay;
    public int moneyWin;
    public string gambleText;
    public string loseDialog;
    public bool sick;
    public bool guard;
    public bool stopToTalk;
    public Image cone;
    public GameObject bubble;
    public Item[] items;
    public Action[] actions;
    public Action[] triggeredActions;

    bool ignoreBodies = false;
    bool panic = false;
    bool showingDialog = false;
    bool stopMovement = false;

    int actionIndex;
    bool completed = true;

    bool secondaryComplete = true;
    int secIndex = 0;
    bool secActionDone = true;

    bool stopCurrentAction = false;

    bool poisoned = false;
    bool isTalking = false;
    Vector2Int panicDir;

    new protected void Start()
    {
        base.Start();
        anim.SetInteger("dirX", startDir.x);
        anim.SetInteger("dirY", startDir.y);
        lastDir = startDir;
        bubble = Instantiate(bubble, nameTxt.transform.parent);
        bubble.SetActive(false);
        if (CompareTag("Elaine"))
        {
            bubble.transform.RotateAround(transform.position, Vector3.forward, -90);
            bubble.transform.GetChild(0).Rotate(Vector3.forward, 180);
        }
    }

    void Update()
    {
        if (alive && !sedated)
        {
            if (Time.timeScale <= .5f)
                aud.Stop();
            if (!panic)
            {
                if (secondaryComplete)
                {
                    if (completed)
                    {
                        if (actions.Length == 0)
                        {
                            anim.SetInteger("speed", 0);
                            if (aud.isPlaying && !isTalking) aud.Stop();
                        }
                        else
                        {
                            if (actionIndex < actions.Length)
                            {
                                completed = false;
                                Action a = actions[actionIndex];
                                executeAction(a, true);
                            }
                            else
                            {
                                actionIndex = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (secActionDone)
                    {
                        if (secIndex < triggeredActions.Length)
                        {
                            secActionDone = false;
                            Action a = triggeredActions[secIndex];
                            executeAction(a, false);
                        }
                        else
                        {
                            secondaryComplete = true;
                        }
                    }
                }
                if (!ignoreBodies && detectBody())
                {
                    dialog = "I found a body!";
                    FindObjectOfType<GameManager>().logMessage(name + " - " + "There's a body here!", nameTxt.color);
                    if (showingDialog) {
                        hideDialog();
                        showDialog();
                    }
                    startPanic();
                }
                if (guard && detectPlayer())
                {
                    dialog = "Intruder! Close the gate!";
                    if (FindObjectOfType<GameManager>())
                        FindObjectOfType<GameManager>().logMessage(name + " : " + dialog, nameTxt.color);
                    if (showingDialog)
                    {
                        hideDialog();
                        showDialog();
                    }
                    StartCoroutine(FindObjectOfType<GameManager>().closeGate());
                    guard = false;
                    cone.enabled = false;
                    speed = 0;
                    anim.SetInteger("speed", 0);
                    if (!isTalking) aud.Stop();
                    stopCurrentAction = true;
                }
            }
            else
            {
                if (completed)
                {
                    if (panicDir != null && (panicDir.x != 0 || panicDir.y != 0))
                    {
                        completed = false;
                        Action a = new Action();
                        a.dir = panicDir;
                        a.magnitude = 1;
                        executeAction(a, true);
                    }
                    else
                    {
                        aud.Stop();
                    }
                }
            }
        }
    }

    public void removeGuard()
    {
        if (guard)
        {
            dialog = "Mr Francis' friends are always welcome";
            aud.Stop();
            if (showingDialog)
            {
                hideDialog();
                showDialog();
            }
            guard = false;
            cone.enabled = false;
            speed = 0;
            anim.SetInteger("speed", 0);
            stopCurrentAction = true;
        }
    }

    void executeAction(Action a, bool primary = true)
    {
        switch (a.action)
        {
            case actionType.move:
                StartCoroutine(moveAction(a, panic, primary));
                break;
            case actionType.wait:
                StartCoroutine(waitAction(a, primary));
                break;
            case actionType.kill:
                kill(a.dir);
                actionDone(primary);
                break;
            case actionType.changeDialog:
                dialog = a.dialog;
                if (showingDialog)
                {
                    hideDialog();
                    showDialog();
                }
                actionDone(primary);
                break;
            case actionType.setShop:
                if (a.magnitude > 0 && items.Length > 0) shop = true;
                else shop = false;
                actionDone(primary);
                break;
            case actionType.setStopToTalk:
                if (a.magnitude > 0) stopToTalk = true;
                else stopToTalk = false;
                actionDone(primary);
                break;
            case actionType.setIgnoreBodies:
                if (a.magnitude > 0) ignoreBodies = true;
                else ignoreBodies = false;
                actionDone(primary);
                break;
            case actionType.setGuard:
                if (a.magnitude > 0)
                {
                    guard = true;
                    cone.enabled = true;
                }
                else
                {
                    aud.Stop();
                    dialog = "Welcome to the palace";
                    if (showingDialog)
                    {
                        hideDialog();
                        showDialog();
                    }
                    guard = false;
                    cone.enabled = false;
                    speed = 0;
                    anim.SetInteger("speed", 0);
                    stopCurrentAction = true;
                    x = 0;
                    y = 0;
                }
                actionDone(primary);
                break;
            case actionType.changeSpeed:
                speed = a.magnitude;
                anim.SetInteger("speed", (int)speed);
                if (speed == 0)
                {
                    stopCurrentAction = true;
                    aud.Stop();
                }
                actionDone(primary);
                break;
            case actionType.die:
                StartCoroutine(dieAction(a, primary));
                break;
            case actionType.heal:
                sick = false;
                actionDone(primary);
                break;
            case actionType.setPanic:
                startPanic();
                actionDone(primary);
                break;
            case actionType.changePrice:
                if (items.Length > a.dir.x)
                {
                    items[a.dir.x].price = a.magnitude;
                }
                actionDone(primary);
                break;
            case actionType.poison:
                poison();
                actionDone(primary);
                break;

        }
    }

    void startPanic()
    {
        if (!panic)
        {
            tag = "ClipWalls";
            speed = 10;
            panic = true;
            stopToTalk = false;
            shop = false;
            stopMovement = false;
            ignoreBodies = true;
            stopCurrentAction = true;
        }
    }

    public void triggerActions()
    {
        secondaryComplete = false;
        secIndex = 0;
    }

    IEnumerator moveAction(Action a, bool panicking, bool primary = true)
    {
        if (speed > 0)
        {
            Vector2Int dir = a.dir;
            int movesRemainig = a.magnitude;
            while (movesRemainig > 0 && alive && !sedated)
            {
                if (!stopCurrentAction)
                {
                    if (!stopMovement)
                    {
                        x = dir.x;
                        y = dir.y;
                        moveStart();
                        if (anim != null)
                        {
                            anim.SetInteger("dirX", x);
                            anim.SetInteger("dirY", y);
                            anim.SetInteger("speed", (int)speed);
                        }
                        if (guard)
                        {
                            float angle = Vector2.SignedAngle(new Vector2(0, -1), lastDir);
                            cone.transform.localRotation = Quaternion.Euler(0, 0, 30 + angle);
                        }
                        while (moving)
                        {
                            yield return null;
                        }
                        movesRemainig--;
                    }
                    else
                    {
                        anim.SetInteger("speed", 0);
                        if (aud.isPlaying && !isTalking) aud.Stop();
                        yield return null;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        actionDone(primary);
    }

    IEnumerator waitAction(Action a, bool primary = true)
    {
        anim.SetInteger("speed", 0);
        if (!poisoned && aud.isPlaying && !isTalking)
            aud.Stop();
        float timeRemaining = a.magnitude;
        while (timeRemaining > 0 && !stopCurrentAction)
        {
            timeRemaining -= Time.deltaTime;
            if (!poisoned && aud.isPlaying && !isTalking)
                aud.Stop();
            yield return new WaitForSeconds(0);
        }
        actionDone(primary);
    }

    IEnumerator dieAction(Action a, bool primary = true)
    {
        actionDone(primary);
        float timeRemaining = a.magnitude;
        yield return new WaitForSeconds(timeRemaining);
        if (sick) die();
    }

    void actionDone(bool primary = true)
    {
        if (primary)
        {
            completed = true;
            actionIndex++;
        }
        else
        {
            secIndex++;
            secActionDone = true;
        }
        stopCurrentAction = false;
    }

    public int sedate()
    {
        if (alive && !sedated)
        {
            dialog = "zzz...";
            if (aud.isPlaying && !isTalking) aud.Stop();
            sedated = true;
            nameTxt.color = new Color(.85f, .76f, 0);
            hideDialog();
            showDialog();
            anim.SetInteger("speed", 0);

            gameObject.layer = LayerMask.NameToLayer("Human");
            tag = "Sedated";
            GetComponent<SpriteRenderer>().sortingOrder = 3;
            transform.rotation = Quaternion.Euler(0, 0, -90);
            rotateSpeech();
            return money;
        }
        return 0;
    }

    public void poison()
    {
        nameTxt.color = new Color(0, .42f, .16f);
        anim.SetInteger("speed", 0);
        poisoned = true;
            
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        transform.rotation = Quaternion.Euler(0, 0, -90);
        aud.PlayOneShot(deathSound);
    }

    public void rotateSpeech()
    {
        bubble.transform.RotateAround(transform.position, Vector3.forward, 90);
    }

    bool detectBody()
    {
        float raycastRange = 5;
        int angle = 60;
        RaycastHit2D h = new RaycastHit2D();
        while (raycastRange > 0 && (!h || !h.collider.CompareTag("Dead")))
        {
            Vector2 dir = lastDir;
            int newAngle = (int)((raycastRange / 4 - .75f) * angle);
            dir = Quaternion.Euler(0, 0, newAngle) * dir;
            h = Physics2D.Raycast(transform.position, dir, 4.5f);
            raycastRange--;
        }
        return h && h.collider.CompareTag("Dead");
    }

    bool detectPlayer()
    {
        float raycastRange = 5;
        int angle = 60;
        RaycastHit2D h = new RaycastHit2D();
        while (raycastRange > 0 && (!h || !h.collider.CompareTag("Player")))
        {
            Vector2 dir = lastDir;
            int newAngle = (int)((raycastRange / 4 - .75f) * angle);
            dir = Quaternion.Euler(0, 0, newAngle) * dir;
            h = Physics2D.Raycast(transform.position, dir, 3.5f);
            raycastRange--;
        }
        return h && h.collider.CompareTag("Player");
    }

    public void showDialog()
    {
        if (alive && !showingDialog)
        {
            showingDialog = true;
            if (dialog != "")
            {
                if (stopToTalk) stopMovement = true;
                isTalking = true;
                StartCoroutine(stopTalking());
                aud.PlayOneShot(dialogSound);
                bubble.SetActive(true);
                bubble.GetComponentInChildren<TextMeshProUGUI>().text = dialog;
                //if (FindObjectOfType<GameManager>())
                //    FindObjectOfType<GameManager>().logMessage(name + " - " + dialog, nameTxt.color);
                StartCoroutine(scaleDialog());
            }
        }
    }

    IEnumerator scaleDialog()
    {
        float timeLeft = 0;
        while (timeLeft < .2f)
        {
            bubble.transform.localScale = new Vector3(timeLeft * 5, timeLeft * 5, 1);
            timeLeft += Time.deltaTime;
            yield return null;
        }
        bubble.transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator stopTalking()
    {
        yield return new WaitForSeconds(.2f);
        isTalking = false;
    }

    public void hideDialog()
    {
        if (showingDialog)
        {
            stopMovement = false;
            bubble.SetActive(false);
            showingDialog = false;
        }
    }

    new protected void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject.CompareTag("DialogTrigger"))
        {
            showDialog();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (panic)
        {
            if (collision.gameObject.CompareTag("Panic"))
            {
                if (panicDir == null || panicDir != collision.gameObject.GetComponent<PanicTrigger>().dir)
                {
                    panicDir = collision.gameObject.GetComponent<PanicTrigger>().dir;
                }
            }
            if (collision.gameObject.CompareTag("Hide"))
            {
                nameTxt.enabled = false;
                dialog = "";
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DialogTrigger"))
        {
            hideDialog();
        }
        if (collision.gameObject.CompareTag("Panic"))
        {
            if (panicDir == collision.gameObject.GetComponent<PanicTrigger>().dir)
            {
                panicDir = new Vector2Int(0, 0);
            }
        }
    }

    public bool isAlive()
    {
        return alive;
    }

    public bool isSedated()
    {
        return sedated;
    }
}
