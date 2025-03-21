using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;





[System.Serializable]
public class Card
{

    public string name;
    public string rank;
    public string suit;
    public int value;
    public Sprite sprite;
    public Card(string rank, string suit, int value)
    {
        this.name = suit.ToUpper() + rank;
        this.rank = rank;
        this.suit = suit;
        this.value = value;
        this.sprite = Resources.Load<Sprite>("Cards/" + suit.ToUpper() + rank);
    }
}
public class GameController : MonoBehaviour
{
    public Color highColor, outLineHighColor, lowColor, outLineLowColor;
    public GameObject cardPrefab;
    public List<Card> decks = new List<Card>();
    private string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    private string[] suits = { "C", "D", "H", "S" };
    [Header("Panel")]

    public GameObject startPanel, endPanel;

    [Header("Button")]

    public Button startButton;
    public void OnClickStartGame(){
        startPanel.LeanMoveLocalY(Screen.height * -2f, 0.3f)
        .setFrom(0)
        .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            startPanel.SetActive(false);
            StartGame();
        });
    }
    public Button exitButton;
    public Button highButton;
    public Button lowButton;

    [Header("TEXT")]

    public Text resultText;
    public Text scoreText, highScoreText;
    public Text scoreTextEnd, highScoreTextEnd;
    public Text playerChoseText, correctText;


    private int highScore = 0;
    private int score = 0;

    [Header("Positions")]
    public GameObject firstCardPos;
    public GameObject secondCardPos;
    public GameObject deckPos;

    public void Shuffle()
    {
        for (int i = 0; i < decks.Count; i++)
        {
            Card temp = decks[i];
            int randomIndex = UnityEngine.Random.Range(i, decks.Count);
            decks[i] = decks[randomIndex];
            decks[randomIndex] = temp;
        }
    }



    public void StartGame()
    {
        score = 0;
        highButton.onClick.AddListener(() =>
        {
            StartCoroutine(OnClickGuest(true));
        });
        lowButton.onClick.AddListener(() =>
        {
            StartCoroutine(OnClickGuest(false));
        });

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateTextUI();

        StartCoroutine(GenerateCards());
    }


    void Update(){
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            print(pos);
        }
    }

    void Start()
    {
        // highButton.onClick.AddListener(() =>
        // {
        //     StartCoroutine(OnClickGuest(true));
        // });
        // lowButton.onClick.AddListener(() =>
        // {
        //     StartCoroutine(OnClickGuest(false));
        // });

        // highScore = PlayerPrefs.GetInt("HighScore", 0);
        // UpdateTextUI();

        //StartGame();
        startButton.onClick.AddListener(OnClickStartGame);
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

    }


    IEnumerator GenerateCards()
    {

        foreach (string suit in suits)
        {
            for (int i = 0; i < ranks.Length; i++)
            {
                int value = i + 2;
                if (i >= 9)
                {
                    value = 10;
                    if(ranks[i] == "J")
                    {
                        value = 11;
                    }else if(ranks[i] == "Q")
                    {
                        value = 12;
                    }else if(ranks[i] == "K")
                    {
                        value = 13;
                    }else if(ranks[i] == "A")
                    {
                        value = 14;
                    }
                }
                decks.Add(new Card(ranks[i], suit, value));
            }
        }

        Shuffle();
        if (decks[0] != null)
        {
            yield return MoveCardFromDeck(decks[0], firstCardPos);
        }

        highButton.interactable = true;
        lowButton.interactable = true;
        //yield return MoveCardFromDeck(decks[1], secondCardPos);


    }

    IEnumerator MoveCardFromDeck(Card card, GameObject to)
    {
        // print("MoveCardFromDeckToFirst");
        GameObject cardObject = Instantiate(cardPrefab, deckPos.transform.localPosition, Quaternion.identity);
        cardObject.GetComponent<Image>().sprite = card.sprite;
        cardObject.transform.SetParent(GameObject.Find("CardsCanvas").transform, false);
        cardObject.transform.localScale = deckPos.transform.localScale;
        yield return new WaitForSeconds(0.3f);

        StartCoroutine(FlipCard(cardObject, false));
        yield return new WaitForSeconds(0.2f);
        cardObject.LeanScale(to.transform.localScale, 0.3f).setEase(LeanTweenType.easeInOutExpo);
        cardObject.LeanRotate(to.transform.rotation.eulerAngles, 0.3f).setEase(LeanTweenType.easeInOutExpo);
        cardObject.LeanMoveLocal(to.transform.localPosition, 0.3f).setEase(LeanTweenType.easeInOutExpo);

        //decks.RemoveAt(0);
    }


    IEnumerator OnClickGuest(bool isHigh)
    {
        highButton.interactable = false;
        lowButton.interactable = false;

        AlertAni(isHigh ? highButton.gameObject : lowButton.gameObject);
        AlertAni(playerChoseText.gameObject);
        string chose = isHigh ? "HIGH" : "LOW";
        playerChoseText.text = " Player Chose:  " + chose;
        yield return MoveCardFromDeck(decks[1], secondCardPos);
        yield return new WaitForSeconds(0.5f);
        if (isHigh)
        {
            if (decks[0].value <= decks[1].value)
            {
                IsWin(true);
            }
            else
            {
                IsWin(false);
            }
        }
        else if (!isHigh)
        {
            if (decks[0].value > decks[1].value)
            {
                IsWin(true);
            }
            else
            {
                IsWin(false);
            }
        }
    }


    public void IsWin(bool who)
    {
        switch (who)
        {
            case true:
                print("Win");

                score += 10;
                StartCoroutine(RemoveCardFromScreen(true));
                break;
            case false:
                print("Lose");
                StartCoroutine(RemoveCardFromScreen(false));
                break;
        }
        UpdateTextUI();

    }


    IEnumerator RemoveCardFromScreen(bool isPlayerWin)
    {
        print("RemoveCardFromScreen");
        if (decks[0].value > decks[1].value)
        {
            resultText.color = lowColor;
            resultText.gameObject.GetComponent<Outline>().effectColor = outLineLowColor;
            yield return ResultTextAni("LOW", resultText);
        }
        else
        {
            resultText.color = highColor;
            resultText.gameObject.GetComponent<Outline>().effectColor = outLineHighColor;
            yield return ResultTextAni("HIGH", resultText);
        }
        yield return new WaitForSeconds(0.2f);
        decks.RemoveAt(0);
        decks.RemoveAt(1);

        yield return new WaitForSeconds(0.8f);
        if (isPlayerWin)
        {
            correctText.color = highColor;
            correctText.gameObject.GetComponent<Outline>().effectColor = outLineHighColor;
            yield return ResultTextAni("Correct", correctText);
            AlertAni(scoreText.gameObject);
            if (decks.Count > 2)
            {
                yield return new WaitForSeconds(1f);
                List<GameObject> cards = new List<GameObject>();
                for (int i = 0; i < GameObject.Find("CardsCanvas").transform.childCount; i++)
                {
                    cards.Add(GameObject.Find("CardsCanvas").transform.GetChild(i).gameObject);
                }
                cards.ForEach(c => { Destroy(c); });
                yield return new WaitForSeconds(1f);
                yield return MoveCardFromDeck(decks[0], firstCardPos);


                highButton.interactable = true;
                lowButton.interactable = true;
            }
            else
            {
                yield return GenerateCards();
            }

        }
        else
        {
            yield return new WaitForSeconds(1f);
            List<GameObject> cards = new List<GameObject>();
            for (int i = 0; i < GameObject.Find("CardsCanvas").transform.childCount; i++)
            {
                cards.Add(GameObject.Find("CardsCanvas").transform.GetChild(i).gameObject);
            }
            cards.ForEach(c => { Destroy(c); });
            correctText.color = lowColor;
            correctText.gameObject.GetComponent<Outline>().effectColor = outLineLowColor;
            yield return ResultTextAni("Wrong", correctText);
            yield return EndGame();
            print("GameOver");
        }
        resultText.gameObject.SetActive(false);
        correctText.gameObject.SetActive(false);
        correctText.gameObject.LeanScale(Vector3.one * 3, 0.3f).setFrom(Vector3.one).setEase(LeanTweenType.easeInOutQuad);
        resultText.gameObject.LeanScale(Vector3.one * 3, 0.3f).setFrom(Vector3.one).setEase(LeanTweenType.easeInOutQuad);
    }


    IEnumerator FlipCard(GameObject card, bool isFlip)
    {
        if (card != null) LeanTween.scaleX(card, 0f, 0.3f).setEase(LeanTweenType.easeInOutQuad);

        yield return new WaitForSeconds(0.2f);

        card.transform.Find("back").gameObject.SetActive(isFlip);

        LeanTween.scaleX(card, 1f, 0.3f).setEase(LeanTweenType.easeInOutQuad);
    }


    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateTextUI()
    {
        scoreText.text = "Score: " + score.ToString();
        highScoreText.text = "HighScore: " + highScore.ToString();
    }


    IEnumerator ResultTextAni(string result, Text text)
    {
        text.gameObject.SetActive(true);
        text.text = result;
        text.gameObject.LeanScale(Vector3.one, 0.3f).setFrom(Vector3.one * 3).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.4f);
    }

    void AlertAni(GameObject game)
    {
        game.LeanScale(Vector3.one * 1.1f, 0.3f).setFrom(Vector3.one).setEase(LeanTweenType.easeInOutQuad).setOnComplete(
            () =>
            {
                game.LeanScale(Vector3.one, 0.3f).setFrom(Vector3.one * 1.1f).setEase(LeanTweenType.easeInOutQuad);
            }
        );
    }


    void HighScoreCheck()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }


    public IEnumerator EndGame()
    {
        HighScoreCheck();
        scoreTextEnd.text = "Score: " + score.ToString();
        highScoreTextEnd.text = "HighScore: " + highScore.ToString();
        endPanel.LeanMoveLocalY(0, 0.3f)
        .setFrom(Screen.height * -2f)
        .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            endPanel.transform.GetChild(0).gameObject.LeanScaleX(1, 0.3f).setFrom(0).setEase(LeanTweenType.easeInOutQuad);
        });
        yield return new WaitForSeconds(1f);
    }


    public void OnClickReplayGame()
    {
        decks.Clear();
        endPanel.transform.GetChild(0).gameObject.LeanScaleX(0, 0.3f).setFrom(1).setEase(LeanTweenType.easeInOutQuad)
        .setOnComplete(() =>
        {
            endPanel.LeanMoveLocalY(Screen.height * -2f, 0.3f)
       .setFrom(0)
       .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
       {
           StartGame();
       });
        });
    }
}
