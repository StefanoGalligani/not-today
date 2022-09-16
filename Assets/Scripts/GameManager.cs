using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int totalTime;
    public TextMeshProUGUI timeLabel;
    public int treasureMoney;
    public Canvas ui;
    public Canvas end;
    public AudioSource soundtrack;

    public GameObject gate;
    public GameObject shopPanel;
    public GameObject gamblePanel;
    public Image mapImage;
    public GameObject logPanel;
    public GameObject logText;
    public AudioClip confirm;
    public AudioClip openSound;
    public AudioClip back;
    public AudioClip wrong;
    public AudioClip countdown;

    public TextMeshProUGUI deathsTxt;
    public TextMeshProUGUI killsTxt;
    public Toggle baronTgl;
    public Toggle elaineTgl;

    public Sprite musicNormal;
    public Sprite musicStopped;
    public Image musicImage;

    public AudioClip goodEnding;
    public AudioClip badEnding;
    public AudioClip angelEnding;
    public AudioClip devilEnding;

    AudioSource aud;
    TMP_InputField inputGamble;
    CharacterController player;
    int peopleDead = 0;
    int peopleKilled = 0;
    bool npcSaved = false;
    bool baronKilled = false;
    bool running = true;
    float timeLeft;
    bool gateClosed = false;
    bool shopOpen = false;
    NpcController npcShop;
    Item[] shopItems;
    int selectedItem;

    bool gambleOpen = false;
    NpcController npcGamble;
    int moneyWin;
    int solution;

    bool mapOpen = false;
    int logTotal = 0;
    bool ended = false;
    bool canPlay = true;
    bool soundtrackPlaying = true;

    string lastMessage;

    void Start()
    {
        aud = GetComponent<AudioSource>();
        timeLeft = totalTime;
        player = FindObjectOfType<CharacterController>();
        StartCoroutine(clockManager());
        int totRewind = PlayerPrefs.GetInt("rewinds");
        PlayerPrefs.SetInt("rewinds", totRewind + 1);
    }

    private void Update()
    {
        if (shopOpen)
        {
            shopInputUpdate();
        }
        else if (gambleOpen)
        {
            gambleInputUpdate();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                backToMenu();
            }
            if (Input.GetKeyDown(KeyCode.Tab) && !(shopOpen || gambleOpen))
            {
                toggleMap();
            }
        }
    }

    IEnumerator clockManager()
    {
        int lastSec = totalTime;
        clockString(totalTime);
        setMusic();
        soundtrack.Play();
        yield return new WaitForSeconds(1);
        while (running)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0) {
                running = false;
            }
            else {
                if (timeLeft < 11 && !ended && ((int)timeLeft < lastSec))
                {
                    timeLabel.color = Color.red;
                    aud.PlayOneShot(countdown);
                    canPlay = false;
                }
                lastSec = (int)timeLeft;
                clockString(lastSec);
            }
            yield return null;
        }
        endGame();
    }

    IEnumerator countdownAudio()
    {
        yield return new WaitForSeconds(1);
        if (!ended)
        canPlay = true;
    }

    void clockString(int time)
    {
        int sec = time % 60;
        int min = (time - sec) / 60;
        if (sec > 9)
            timeLabel.text = min + ":" + sec;
        else
            timeLabel.text = min + ":0" + sec;
    }

    public void setBaronKilled()
    {
        baronKilled = true;
    }

    public void increaseKilled()
    {
        peopleKilled++;
    }

    public void increaseDead()
    {
        peopleDead++;
    }

    public IEnumerator closeGate()
    {
        if (!gateClosed)
        {
            gate.GetComponent<AudioSource>().Play();
            gateClosed = true;
            Vector2 dir = new Vector2(-6, 0);
            Vector2 startPos = gate.transform.position;
            Vector2 destination = startPos + dir;
            float totalTime = 1 / 1;
            float timePassed = totalTime;
            while (timePassed > 0)
            {
                gate.transform.position = startPos + ((destination - startPos) * (1 - timePassed / totalTime));
                timePassed -= Time.deltaTime;
                yield return null;
            }
            gate.transform.position = destination;
            gate.GetComponent<AudioSource>().Stop();
        }
    }

    public void logMessage(string m, Color c)
    {
        if (m == lastMessage)
        {
            Destroy(logPanel.transform.GetChild(logPanel.transform.childCount - 1).gameObject);
            logTotal--;
        }
        GameObject t = Instantiate(logText, logPanel.transform);
        t.GetComponent<TextMeshProUGUI>().text = timeLabel.text + ": " + m;
        t.GetComponent<TextMeshProUGUI>().color = c;
        logTotal++;
        if (logTotal > 7)
        {
            Destroy(logPanel.transform.GetChild(0).gameObject);
        }
        lastMessage = m;
    }

    void shopInputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            closeShop();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            shopBuy();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        {
            if (shopItems.Length > 1)
            {
                shopSelect(1 - selectedItem);
            }
        }
    }

    public void shopSelect(int i)
    {
        selectedItem = i;
        shopPanel.transform.GetChild(0).GetChild(i).GetComponent<Image>().enabled = true;
        shopPanel.transform.GetChild(0).GetChild(1 - i).GetComponent<Image>().enabled = false;
    }

    public void shopBuy()
    {
        if (player.money >= shopItems[selectedItem].price)
        {
            player.money -= shopItems[selectedItem].price;
            npcShop.money += shopItems[selectedItem].price;
            player.boughtItem(shopItems[selectedItem].id);
            npcShop.items = new Item[shopItems.Length - 1];
            if (npcShop.items.Length == 1)
            {
                npcShop.items[0] = shopItems[1 - selectedItem];
            }
            else
            {
                npcShop.shop = false;
            }
            aud.PlayOneShot(confirm);
            closeShop(false);
        }
        else
        {
            aud.PlayOneShot(wrong);
            logMessage("Not enough money", Color.magenta);
        }
    }

    public void openShop(NpcController npc)
    {
        if (!shopOpen)
        {
            player.GetComponent<AudioSource>().Stop();
            npcShop = npc;
            shopSelect(0);
            shopOpen = true;
            shopPanel.SetActive(true);
            shopItems = npc.items;
            shopPanel.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().sprite = shopItems[0].sprite;
            shopPanel.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = shopItems[0].itemName;
            shopPanel.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>().text = shopItems[0].price.ToString();
            if (shopItems.Length > 1)
            {
                shopPanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                shopPanel.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Image>().sprite = shopItems[1].sprite;
                shopPanel.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = shopItems[1].itemName;
                shopPanel.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = shopItems[1].price.ToString();
            }
            else
            {
                shopPanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            aud.PlayOneShot(openSound);
            StartCoroutine(musicFadeOut());
        }
    }

    public void closeShop(bool sound = true)
    {
        if (shopOpen)
        {
            shopOpen = false;
            StartCoroutine(musicFadeIn());
            shopPanel.SetActive(false);
            player.setInputActive(true);
            if (sound)
            {
                aud.PlayOneShot(back);
            }
        }
    }

    void gambleInputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            gambleGuess();
        }
    }

    public void gambleGuess()
    {
        int guess = -1;
        try
        {
            guess = int.Parse(inputGamble.text);
        }
        catch (System.Exception e)
        {
            guess = -1;
        }
        if (solution == guess)
        {
            player.money += moneyWin;
            npcGamble.money -= moneyWin;
            logMessage(npcGamble.name + ": Well done, you got it", Color.magenta);
            npcGamble.dialog = npcGamble.loseDialog;
            player.playMoney();
            closeGamble();
        }
        else
        {
            aud.PlayOneShot(wrong);
            logMessage(npcGamble.name + ": Wrong. Try again tomorrow", Color.magenta);
            npcGamble.dialog = "You'll be luckier next time";
            closeGamble();
        }
        npcGamble.hideDialog();
        npcGamble.showDialog();
        npcGamble.gamble = false;
    }

    public void openGamble(NpcController npc)
    {
        if (!gambleOpen)
        {
            player.GetComponent<AudioSource>().Stop();
            npcGamble = npc;
            gambleOpen = true;
            gamblePanel.SetActive(true);
            moneyWin = npc.moneyWin;
            solution = npc.gambleSolution;

            if (!inputGamble) inputGamble = gamblePanel.GetComponentInChildren<TMP_InputField>();
            inputGamble.text = "";
            inputGamble.ActivateInputField();
            inputGamble.Select();
            gamblePanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = npcGamble.gambleText;
            aud.PlayOneShot(openSound);

            StartCoroutine(musicFadeOut());
        }
    }

    public void closeGamble()
    {
        if (gambleOpen)
        {
            gambleOpen = false;
            StartCoroutine(musicFadeIn());
            gamblePanel.SetActive(false);
            player.setInputActive(true);
        }
    }

    public void toggleMap()
    {
        if (mapOpen)
        {
            mapOpen = false;
            StartCoroutine(musicFadeIn());
            mapImage.enabled = false;
            player.setInputActive(true);
        }
        else
        {
            player.GetComponent<AudioSource>().Stop();
            player.setInputActive(false);
            mapOpen = true;
            mapImage.enabled = true;
            aud.PlayOneShot(openSound);
            StartCoroutine(musicFadeOut());
        }
    }

    public void saved()
    {
        npcSaved = true;
        endGame();
    }

    public void endGame()
    {
        if (!ended)
        {
            player.gameEnded();
            ended = true;
            ui.enabled = false;
            end.gameObject.SetActive(true);

            TextMeshProUGUI endName = end.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI endDesc = end.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();

            if (!FindObjectOfType<CharacterController>().isAlive())
            {
                PlayerPrefs.SetInt("failure", 1);
                endName.text = "Failure ending";
                endDesc.text = "You died, bringing your mission to an early grave";
                endName.color = Color.black;
                endDesc.color = Color.black;
                aud.clip = badEnding;
            }
            else if (!npcSaved)
            {
                increaseDead();
                if (peopleDead == 22)
                {
                    PlayerPrefs.SetInt("devil", 1);
                    endName.text = "Devil ending";
                    endDesc.text = "You killed everyone and let Elaine die in despair. You have become a monster";
                    endName.color = Color.red;
                    endDesc.color = Color.red;
                    aud.clip = devilEnding;
                }
                else
                {
                    PlayerPrefs.SetInt("hopeless", 1);
                    endName.text = "Hopeless ending";
                    endDesc.text = "You failed to save the life of someone who begged for your help";
                    endName.color = Color.blue;
                    endDesc.color = Color.blue;
                    aud.clip = badEnding;
                }
            }
            else
            {
                if (peopleKilled > 0 && !baronKilled || peopleKilled > 1)
                {
                    PlayerPrefs.SetInt("desperate", 1);
                    endName.text = "Desperate ending";
                    endDesc.text = "You saved Elaine, but could not contain the agitation that brought you to kill someone else";
                    endName.color = new Color(.85f, .76f, 0);
                    endDesc.color = new Color(.85f, .76f, 0);
                    aud.clip = goodEnding;
                }
                else if (baronKilled && peopleKilled == 1)
                {
                    PlayerPrefs.SetInt("vigilante", 1);
                    endName.text = "Vigilante ending";
                    endDesc.text = "You saved Elaine after freeing the town from the evil soul of the baron";
                    endName.color = Color.green;
                    endDesc.color = Color.green;
                    aud.clip = goodEnding;
                }
                else if (peopleDead == 0)
                {
                    PlayerPrefs.SetInt("angel", 1);
                    endName.text = "Angel ending";
                    endDesc.text = "Not only you saved Elaine, but you have been cautious enough to save the lives of every citizen, thus earning the title of town's guardian angel";
                    endName.color = Color.cyan;
                    endDesc.color = Color.cyan;
                    aud.clip = angelEnding;
                }
                else if (peopleKilled == 0)
                {
                    PlayerPrefs.SetInt("samaritan", 1);
                    endName.text = "Samaritan ending";
                    endDesc.text = "You saved Elaine, without killing anyone and having mercy on the corrupted soul of the baron";
                    endName.color = Color.white;
                    endDesc.color = Color.white;
                    aud.clip = goodEnding;
                }
            }
            deathsTxt.text = peopleDead.ToString();
            killsTxt.text = peopleKilled.ToString();
            baronTgl.isOn = baronKilled;
            elaineTgl.isOn = npcSaved;
            StartCoroutine(fadeOut());
        }
    }

    public void playWrong()
    {
        aud.PlayOneShot(wrong);
    }

    IEnumerator fadeOut()
    {
        TextMeshProUGUI[] texts = end.GetComponentsInChildren<TextMeshProUGUI>();
        Image[] images = end.GetComponentsInChildren<Image>();
        Color newColor;
        foreach (TextMeshProUGUI tmpro in texts)
        {
            newColor = new Color(tmpro.color.r, tmpro.color.g, tmpro.color.b, 0);
            tmpro.color = newColor;
        }
        foreach (Image img in images)
        {
            newColor = new Color(img.color.r, img.color.g, img.color.b, 0);
            img.color = newColor;
        }
        float timeLeft = 0;
        while (timeLeft <= 1)
        {
            timeLeft += Time.deltaTime / 1.5f;
            foreach (TextMeshProUGUI tmpro in texts)
            {
                newColor = new Color(tmpro.color.r, tmpro.color.g, tmpro.color.b, timeLeft * timeLeft * timeLeft);
                tmpro.color = newColor;
            }
            foreach (Image img in images)
            {
                newColor = new Color(img.color.r, img.color.g, img.color.b, timeLeft * timeLeft * timeLeft);
                img.color = newColor;
            }
            if (soundtrackPlaying)
                soundtrack.volume = 1 - timeLeft;
            yield return null;
        }
        soundtrack.Stop();
        aud.Play();
    }

    public void rewind()
    {
        SceneManager.LoadScene("Game");
    }

    public void backToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    IEnumerator musicFadeOut()
    {
        float fadeTime = .3f;
        if (soundtrackPlaying)
        {
            while (fadeTime > 0 && (shopOpen || gambleOpen || mapOpen))
            {
                fadeTime -= Time.deltaTime;
                soundtrack.volume = 3.33f * fadeTime;
                yield return null;
            }
        }
        if (shopOpen || gambleOpen || mapOpen)
        {
            soundtrack.Pause();
            Time.timeScale = 0;
        }
        else if (soundtrackPlaying)
        {
            soundtrack.volume = 1;
        }
    }

    IEnumerator musicFadeIn()
    {
        soundtrack.UnPause();
        Time.timeScale = 1;
        float fadeTime = 0;
        if (soundtrackPlaying)
        {
            while (fadeTime < .3f && !(shopOpen || gambleOpen || mapOpen))
            {
                fadeTime += Time.deltaTime;
                soundtrack.volume = 3.33f * fadeTime;
                yield return null;
            }
            soundtrack.volume = 1;
        }
    }

    public void toggleLog()
    {
        GameObject panel = logPanel.transform.parent.parent.gameObject;
        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void toggleMusic()
    {
        if (soundtrack.volume > .5f)
        {
            soundtrack.volume = 0;
            soundtrackPlaying = false;
            musicImage.sprite = musicStopped;
            PlayerPrefs.SetInt("music", -1);
        }
        else
        {
            soundtrack.volume = 1;
            soundtrackPlaying = true;
            musicImage.sprite = musicNormal;
            PlayerPrefs.SetInt("music", 0);
        }
    }

    void setMusic()
    {
        if (PlayerPrefs.GetInt("music") == -1)
        {
            toggleMusic();
        }
    }

    public void checkGambleInput()
    {
        string c = inputGamble.text;
        switch (c)
        {
            case "0":
                break;
            case "1":
                break;
            case "2":
                break;
            case "3":
                break;
            case "4":
                break;
            case "5":
                break;
            case "6":
                break;
            case "7":
                break;
            case "8":
                break;
            case "9":
                break;
            default:
                inputGamble.text = "";
                break;
        }
    }
}