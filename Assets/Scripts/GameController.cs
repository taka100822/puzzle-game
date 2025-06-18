using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameController : MonoBehaviour
{
    [Header("キャンディーが消える際のエフェクト")] [SerializeField] public GameObject explosionPrefab = null;
    [Header("キャンディーの行数")] private int width = 6;
    [Header("キャンディーの列数")]private int height = 5;
    [Header("キャンディーを1個消去するともらえるスコア")] public int pointsPerCandy = 10;
    [Header("スコアテキスト")] public TMP_Text ScoreText;
    [Header("スコアメッセージ")] public GameObject ScoringMessage;
    [Header("残り操作回数テキスト")] public TMP_Text RemainingTimesText;
    [Header("フィニッシュテキスト")] public TMP_Text FinishText;
    [Header("フィニッシュテキストが出現する際のエフェクト")] [SerializeField] public GameObject finishPrefab = null;
    [Header("キャンディーが消える時にならすSE")] public AudioClip SE; 
    [Header("キャンディー")] public GameObject[] Candies;
    [Header("最大操作回数")] public int maxCount = 5;
    public int count;
    public GameObject[,] candyArray = new GameObject[6,5];
    private List<GameObject> deleteList = new List<GameObject>();
    private AudioSource audioSource = null;
    private int score; // スコア
    private int chain; //連鎖回数
    private bool isStart; // クリックを開始して押し続けているか
    public bool isChecked = false; //CheckMatching()をしたか


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        score = 0;
        count = 0;
        ScoreText.text = "SCORE: " + score.ToString();
        RemainingTimesText.text =  maxCount.ToString() + " more moves";
        FinishText.text =  "";
        CreateCandies();
    }

    void CreateCandies()
    {
        for(int i=0; i < width; i++){
            for(int j=0; j < height; j++){
                int r = Random.Range(0, Candies.Length);
                var candy = Instantiate(Candies[r]);
                candy.transform.position = new Vector2(i,j);
                candyArray[i,j] = candy;
            }
        }
        CheckStartset();
    }

    void CheckStartset()
    {
        CandyMove candyMove = FindObjectOfType<CandyMove>();
        //下の行からヨコのつながりを確認
        for (int i = 0; i < height; i++)
        {
            //右から２つ目以降は確認不要（width-2）
            for (int j = 0; j < width-2; j++)
            {
                //同じタグのキャンディーが３つ並んでいたら。Ｘ座標がｊ
                if ((candyArray[j,i].tag==candyArray[j+1,i].tag) && (candyArray[j, i].tag == candyArray[j + 2, i].tag))
                {
                    //CandyのisMatchingをtrueに
                    candyArray[j, i].GetComponent<CandyMove>().isMatching = true;
                    candyArray[j + 1, i].GetComponent<CandyMove>().isMatching = true;
                    candyArray[j + 2, i].GetComponent<CandyMove>().isMatching = true;
                }
            }
        }

        //左の列からタテのつながりを確認
        for (int i = 0; i < width; i++)
        {
            //上から２つ目以降は確認不要。height-2
            for (int j = 0; j < height-2; j++)
            {
                //Ｙ座標がｊ。
                if ((candyArray[i,j].tag==candyArray[i,j+1].tag) && (candyArray[i,j].tag==candyArray[i,j+2].tag))
                {
                    candyArray[i, j].GetComponent<CandyMove>().isMatching = true;
                    candyArray[i, j+1].GetComponent<CandyMove>().isMatching = true;
                    candyArray[i, j+2].GetComponent<CandyMove>().isMatching = true;
                }
            }
        }
        
        foreach (var item in candyArray)
        {
            if (item.GetComponent<CandyMove>().isMatching)
            {
                deleteList.Add(item);
            }
        }
        //List内にキャンディーがある場合
        if (deleteList.Count>0)
        {
            
            //該当する配列をnullにして（内部管理）、キャンディーを消去する（見た目）。
            foreach (var item in deleteList)
            {
                candyArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;
                Destroy(item);
            }
            //Listを空っぽに。
            deleteList.Clear();
            //空欄に新しいキャンディーを入れる。
            SpawnNewCandy();
        }
        else
        {
            isStart = true;
        }
    }
    
    void SpawnNewCandy()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (candyArray[i, j] == null)
                {
                    int r = Random.Range(0, Candies.Length);
                    var candy = Instantiate(Candies[r]);
                    //見た目の処理
                    candy.transform.position = new Vector2(i, j+0.3f);
                    //内部管理の処理
                    candyArray[i, j] = candy;
                }
            }
        }

        if (isStart == false)
        {
            CheckStartset();
        }
        else//isStart==trueのとき。
        {
            //新しい位置をmyPreviousPosに設定
            foreach (var item in candyArray)
            {
                int column = (int)item.transform.position.x;
                int row = (int)item.transform.position.y;
                item.GetComponent<CandyMove>().myPreviousPos = new Vector2(column,row);
            }
            //続けざまに３つそろっているかどうか判定。
            Invoke("CheckMatching",0.6f);
        }
    }

    public void CheckMatching()
    {
        if(!isChecked)
        {
            isChecked = true;
        }
        //下の行からヨコのつながりを確認
        for (int i = 0; i < height; i++)
        {
            //右から２つ目以降は確認不要
            for (int j = 0; j < width - 2; j++)
            {
                // candyArray[j, i] やその隣の要素が null でないか確認
                if (candyArray[j, i] != null && candyArray[j + 1, i] != null && candyArray[j + 2, i] != null)
                {
                    //同じタグのキャンディーが３つ並んでいたら。Ｘ座標がｊ。
                    if ((candyArray[j, i].tag == candyArray[j + 1, i].tag) && (candyArray[j, i].tag == candyArray[j + 2, i].tag))
                    {
                        //CandyのisMatchingをtrueに
                        candyArray[j, i].GetComponent<CandyMove>().isMatching = true;
                        candyArray[j + 1, i].GetComponent<CandyMove>().isMatching = true;
                        candyArray[j + 2, i].GetComponent<CandyMove>().isMatching = true;
                    }
                }
            }
        }
        //左の列からタテのつながりを確認
        for (int i = 0; i < width; i++)
        {
            //上から２つ目以降は確認不要。
            for (int j = 0; j < height - 2; j++)
            {
                // candyArray[j, i] やその隣の要素が null でないか確認
                if (candyArray[i, j] != null && candyArray[i, j + 1] != null && candyArray[i, j + 2] != null)
                {
                    //Ｙ座標がｊ。
                    if ((candyArray[i, j].tag == candyArray[i, j + 1].tag) && (candyArray[i, j].tag == candyArray[i, j + 2].tag))
                    {
                        candyArray[i, j].GetComponent<CandyMove>().isMatching = true;
                        candyArray[i, j + 1].GetComponent<CandyMove>().isMatching = true;
                        candyArray[i, j + 2].GetComponent<CandyMove>().isMatching = true;
                    }
                }
            }
        }
        //isMatching=trueのものをＬｉｓｔに入れる
        foreach (var item in candyArray)
        {
            if (item != null && item.GetComponent<CandyMove>().isMatching)
            {
                //インスペクターで指定したSEを鳴らす
                audioSource.PlayOneShot(SE);
                //３つ以上そろったとき、キャンディーを半透明にする。
                item.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
                //エフェクトを出す
                Instantiate(explosionPrefab, item.transform.position, Quaternion.identity);
                deleteList.Add(item);
            }
        }

        //List内にキャンディーがある場合
        if (deleteList.Count > 0)
        {
            //キャンディーを消去するとき、一瞬の間を持たせるためIvoke関数にする。
            chain++;
            Invoke("DeleteCandies",0.2f);
            
        }
        else//Listにキャンディーがない場合。
        {
            chain = 0;
            //再びキャンディーを操作できるようにする。
            Invoke("CanMoveCandies", 0.4f);
        }
    }

void DeleteCandies()
    {
        //List内のキャンディーを消去。かつ、その配列をnullに。
        foreach (var item in deleteList)
        {
            Destroy(item);
            candyArray[(int)item.transform.position.x, (int)item.transform.position.y] = null;
            //スコアをキャンディー１つあたり加算
            score += pointsPerCandy * chain;
        }
        //加算される得点を表示
        var scoringPopUp = Instantiate(ScoringMessage);
        if(chain == 1){
            scoringPopUp.GetComponent<TextMesh>().text = "+" + deleteList.Count * pointsPerCandy;
        }
        else if(chain > 1){
            scoringPopUp.GetComponent<TextMesh>().text = "+" + deleteList.Count * pointsPerCandy + "×" + chain + "chain";
        }
        //score表示
        ScoreText.text = "SCORE: " + score;
        //Listを空っぽに。
        deleteList.Clear();
        //キャンディーの落下を待って、空欄に新しいキャンディーを入れる。
        Invoke("SpawnNewCandy", 0.5f);
    }

    public void StopCandies()
    {
        foreach (var item in candyArray)
        {
            if(item != null){
            item.GetComponent<CandyMove>().isMoving = true;
            }
        }
    }

void CanMoveCandies()
    {
        foreach (var item in candyArray)
        {
            if(item != null){
            item.GetComponent<CandyMove>().isMoving = false;
            }
        }
    }

    public void NewGameBtn()
    {
        SceneManager.LoadScene("Game");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
