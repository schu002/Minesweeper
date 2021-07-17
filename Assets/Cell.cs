using System;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    // セル1辺の大きさ
    public static int Size = 32;

    // セルの状態
    public CellState mState { get; private set; }

    // このセルに爆弾があるかどうか
    public bool mIsBomb { get; private set; }

    // 隣接する爆弾の数です
    public int mNeighbourBombCount { get; private set; }

    public int mRow { get; private set; }
    public int mCol { get; private set; }

    private static Color bgcolor = new Color(0.7f, 0.7f, 0.7f);

    void Start()
    {
        // bgcolor = Color.gray; // new Color(0.6f, 0.6f, 0.6f);
    }

    // セルを初期化します
    public void Initialize(int r, int c)
    {
        mRow = r;
        mCol = c;
        mState = CellState.Closing;
        mIsBomb = false;
        mNeighbourBombCount = 0;
        // Debug.Log("Init (" + row + ", " + col + ") " + State);
    }

    // セルを初期化します
    public void Reset(bool isBomb)
    {
        mState = CellState.Closing;
        mIsBomb = isBomb;
        mNeighbourBombCount = 0;
        this.GetComponent<Image>().color = bgcolor;
        this.GetComponentInChildren<Text>().text = "";
        // if (isBomb) this.GetComponentInChildren<Text>().text = "Ｘ";
        // if (row == 6 && col == 6) Debug.Log("Reset (" + row + ", " + col + ") " + State);
    }

    public void SetNeighbourBombCount(int bombCount)
    {
        if (!mIsBomb && bombCount > 0) {
            mNeighbourBombCount = bombCount;
        }
    }

    // セルを展開します
    public void Open()
    {
        // すでに展開済の場合は何もしません
        // フラグが立ててあるセルも展開しないようにします
        if (mState == CellState.Opened || mState == CellState.Flag) {
            this.GetComponentInChildren<Image>().color = Color.white;
            return;
        }

        // Textコンポーネントを取得しておく
        var text = this.GetComponentInChildren<Text>();
        this.GetComponent<Image>().color = Color.white;
        if (mIsBomb) { // 爆弾セルを開いたのでゲームオーバー
            text.text = "Ｘ";
            return;
        }

        // Debug.Log("Cell::Open (" + row + ", " + col + ") " + State + " Bomb " + this.NeighbourBombCount);
        // セルを展開済の状態に更新し、白色にします
        mState = CellState.Opened;
        text.text = (mNeighbourBombCount == 0)? "" : mNeighbourBombCount.ToString();
        this.GetComponent<Image>().color = Color.white;
    }

    // セルにマークを付けます
    public void ChangeMark()
    {
        var text = this.GetComponentInChildren<Text>();
        switch (mState)
        {
            case CellState.Closing:
                mState = CellState.Flag;
                // フラグは適当な文字で代替してます
                text.text = "●"; 
                break;
            case CellState.Flag:
                mState = CellState.Question;
                text.text = " ？";
                break;
            case CellState.Question:
                mState = CellState.Closing;
                text.text = string.Empty;
                break;
            default:
                break;
        }
    }
}

// フィールド上のセルがどのような状態にあるかを表します
public enum CellState
{
    Closing = 0,    // セルが閉じられた状態
    Opened = 1,     // セルが展開された状態
    Flag = 2,       // セルにフラグが立てられた状態
    Question = 3    // セルに？が付けられた状態
}
