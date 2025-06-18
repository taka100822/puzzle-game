using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CandyMove : MonoBehaviour
{    
     //GameControllerスクリプトを使うので、指定する。
    private GameController gameControllerCS;

    //自身の入っている配列の座標
    public int column;//列
    public int row;//行
    private Vector2 originp = Vector2.zero; //キャンディーが動く前の原点とする座標
    private Vector2 nowp; //現在の座標
    private Vector2 distance; //原点と現在の座標の距離

    //隣のキャンディー
    private GameObject neighborCandy;

    //３つ並んでいるとき知らせる
    public bool isMatching;

    //移動前の座標
    public Vector2 myPreviousPos;

    //移動中はキャンディーを操作できないように
    public bool isMoving;
    public bool isClick = false; //クリックしたか
    private float time = 0; //動かしている時間
    private bool isStop;
    [Header("キャンディーの基本操作時間")]private float BasicOperationTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        gameControllerCS = FindObjectOfType<GameController>();
        //自分の位置を座標配列の番号（Index)にあてておく。
        column = (int)transform.position.x;
        row = (int)transform.position.y;
        //スタート位置を記録する。
        myPreviousPos = new Vector2(column,row);
    }

    // Update is called once per frame
    void Update()
    {
        // クリックしている時間を計測
        timeCount();
        
        if(isClick && time < BasicOperationTime && gameControllerCS.count < gameControllerCS.maxCount){
            if(originp == Vector2.zero)
            {
                // 原点を設定
                originp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            // 現在の座標を取得
            nowp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 距離を計算
            distance = nowp - originp;
            // 距離の絶対値が1以上ならキャンディーを動かす
            if(Mathf.Abs(distance.x) > 1 || Mathf.Abs(distance.y) > 1)
            {
                moveCandies();
                originp = Vector2.zero; // 原点をリセット
            }
        }

        //現在の座標と、column、rowの値が異なるとき。
        if (transform.position.x!=column || transform.position.y!=row)
        {
            //column,rowの位置に徐々に移動する。
            transform.position = Vector2.Lerp(transform.position, new Vector2(column, row), 0.3f);
            //現在の位置と、目的地(column,row)との距離を測る。
            Vector2 dif = (Vector2)transform.position - new Vector2(column, row);
            //目的地との距離が0.1fより小さくなったら。
            if (Mathf.Abs(dif.magnitude)<0.1f)
            {
                transform.position = new Vector2(column, row);
                //自身をCandyArray配列に格納する。
                SetCandyToArray();
            }
        }
        else if (row>0 && gameControllerCS.candyArray[column,row-1]==null)
        {
            FallCandy();
        }

        // 基本操作時間を越せばキャンディー移動を受け付けない
        // さらにキャンディーがそろっているか確認するフェーズへ
        if(time > BasicOperationTime && !gameControllerCS.isChecked)
        {
            DoCheckMatching();
        }
    }

    //指をおいたとき
    private void OnMouseDown()
    {
        isClick = true;
        gameControllerCS.isChecked = false;
    }

    //指を離したとき
    private void OnMouseUp()
    {
        isClick = false;
        DoCheckMatching();
    }

    void moveCandies()
    {
        //すべてのキャンディーを操作できないようにする
        gameControllerCS.StopCandies();
        //右にスワイプしていたなら。（Mathf.Absとは絶対値を示す）
        if (distance.x>=0 && Mathf.Abs(distance.x)>Mathf.Abs(distance.y))
        {
            //自身が一番右にいない場合、となりのキャンディーと位置を交換する
            if (column<5)
            {
                //右隣りのキャンディー情報をneighborCandyに代入
                neighborCandy = gameControllerCS.candyArray[column + 1, row];
                //隣のキャンディーを１列左へ。
                neighborCandy.GetComponent<CandyMove>().column -= 1;
                //自身は１列右へ。
                column += 1;
            }
        }
        //左にスワイプしていたなら。
        if (distance.x < 0 && Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            //自身が一番左にいない場合、となりのキャンディーと位置を交換する
            if (column > 0)
            {
                //左隣りのキャンディー情報を取得
                neighborCandy = gameControllerCS.candyArray[column - 1, row];
                //隣のキャンディーを１列右へ。
                neighborCandy.GetComponent<CandyMove>().column += 1;
                //自身は１列左へ。
                column -= 1;
            }
        }
        //上にスワイプしていたなら。
        if (distance.y >= 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //自身が一番上にいない場合、となりのキャンディーと位置を交換する
            if (row < 4 )
            {
                //上のキャンディー情報を取得
                neighborCandy = gameControllerCS.candyArray[column, row+1];
                //隣のキャンディーを１行下へ。
                neighborCandy.GetComponent<CandyMove>().row -= 1;
                //自身は１行上へ。
                row += 1;
            }
        }
        //下にスワイプしていたなら。
        if (distance.y < 0 && Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
            //自身が一番下にいない場合、となりのキャンディーと位置を交換する
            if (row > 0)
            {
                //下のキャンディー情報を取得
                neighborCandy = gameControllerCS.candyArray[column, row - 1];
                //隣のキャンディーを１行上へ。
                neighborCandy.GetComponent<CandyMove>().row += 1;
                //自身は１行下へ。
                row -= 1;
            }
        }
    }

    void DoCheckMatching()
    {
        if(gameControllerCS.count < gameControllerCS.maxCount){
            gameControllerCS.count++;
        }
        // 残り操作回数を更新
        if(gameControllerCS.maxCount - gameControllerCS.count > 1){
            gameControllerCS.RemainingTimesText.text =  (gameControllerCS.maxCount - gameControllerCS.count).ToString() + " more moves";
        }
        // ラスト1回のときは複数形movesではなく単数形moveにする
        else{
            gameControllerCS.RemainingTimesText.text =  (gameControllerCS.maxCount - gameControllerCS.count).ToString() + " more move";
        }

        gameControllerCS.CheckMatching();
        if(gameControllerCS.maxCount == gameControllerCS.count){
            // finishテキストを出現させる
            gameControllerCS.FinishText.text =  "Finish!!";
            // エフェクト出現
            //Instantiate(gameControllerCS.finishPrefab, gameControllerCS.FinishText.rectTransform.position, Quaternion.identity);
        }
    }

    //CandyArray配列に、自身を格納する。
    public void SetCandyToArray()
    {
        gameControllerCS.candyArray[column, row] = gameObject;
    }

    void FallCandy()
    {
        //自分のいた配列を空にする
        gameControllerCS.candyArray[column, row] = null;
        //自分を下に移動させる
        row -= 1;
    }

    public void BackToPreviousPos()
    {
        column = (int)myPreviousPos.x;
        row = (int)myPreviousPos.y;   
    }

    public void timeCount()
    {
        if (Input.GetMouseButtonDown(0)) // マウスボタンを押した瞬間
        {
            if (!isStop)
            {
                isStop = true;
                time = 0;  // タイマーをリセット
            }
        }
        if (Input.GetMouseButtonUp(0)) // マウスボタンを離した瞬間
        {
            if (isStop)
            {
                isStop = false;
                time = 0;
            }
        }
        if (isStop)
        {
            time += Time.deltaTime;  // マウスが押されている間、時間を加算
        }
    }
}
