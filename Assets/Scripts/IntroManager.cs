using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public Image black;

    void Start()
    {
        StartCoroutine(fadeIn());
    }

    IEnumerator fadeIn()
    {
        float timeLeft = 1;
        yield return new WaitForSeconds(1f);
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            Color newColor = new Color(black.color.r, black.color.g, black.color.b, timeLeft);
            black.color = newColor;
            yield return null;
        }
        yield return new WaitForSeconds(25);
        StartCoroutine(fadeOut());
    }

    IEnumerator fadeOut()
    {
        float timeLeft = 0;
        yield return new WaitForSeconds(.5f);
        while (timeLeft < 1)
        {
            timeLeft += Time.deltaTime;
            Color newColor = new Color(black.color.r, black.color.g, black.color.b, timeLeft);
            black.color = newColor;
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        endIntro();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            endIntro();
        }
    }

    void endIntro()
    {
        if (PlayerPrefs.GetInt("intro") == 0)
        {
            PlayerPrefs.SetInt("intro", 1);
            SceneManager.LoadScene("Game");
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
