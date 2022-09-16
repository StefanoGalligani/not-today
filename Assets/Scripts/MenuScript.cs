using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public GameObject endingPanel;
    public GameObject creditsPanel;

    public TextMeshProUGUI rewTotal;

    public string[] names;
    public Toggle[] toggles;

    public AudioClip open;
    public AudioClip close;

    public Sprite musicNormal;
    public Sprite musicStopped;
    public AudioSource soundtrack;
    public Image musicImage;

    AudioSource aud;

    private void Start()
    {
        aud = GetComponent<AudioSource>();
        setMusic();
    }

    public void makeBold(TextMeshProUGUI text)
    {
        text.fontStyle = (TMPro.FontStyles)FontStyle.Bold;
    }

    public void removeBold(TextMeshProUGUI text)
    {
        text.fontStyle = (TMPro.FontStyles)FontStyle.Normal;
    }

    public void clickedButton(int b)
    {
        switch (b)
        {
            case 0:
                if (PlayerPrefs.GetInt("intro") == 1)
                    SceneManager.LoadScene("Game");
                else
                    SceneManager.LoadScene("Intro");
                break;
            case 1:
                setEndings();
                rewTotal.text = PlayerPrefs.GetInt("rewinds").ToString();
                endingPanel.SetActive(true);
                aud.PlayOneShot(open);
                break;
            case 2:
                creditsPanel.SetActive(true);
                aud.PlayOneShot(open);
                break;
            case 3:
                Application.Quit();
                break;
            case 4:
                PlayerPrefs.SetInt("intro", 1);
                SceneManager.LoadScene("Intro");
                break;
        }
    }

    public void hidePanel(GameObject p)
    {
        p.SetActive(false);
        aud.PlayOneShot(close);
    }

    bool getEndingReached(string name)
    {
        if (PlayerPrefs.GetInt(name) > 0)
        {
            return true;
        }
        return false;
    }

    void setEndings()
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (getEndingReached(names[i]))
            {
                toggles[i].isOn = true;
            }
        }
    }

    public void toggleMusic()
    {
        if (soundtrack.volume > .5f)
        {
            soundtrack.volume = 0;
            musicImage.sprite = musicStopped;
            PlayerPrefs.SetInt("music", -1);
        }
        else
        {
            soundtrack.volume = 1;
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
}
