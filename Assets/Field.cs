using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Field : MonoBehaviour
{
    enum GameState
    {
        None,       // 初期状態
        Ready,      // ゲーム開始準備OK
        PlayNow,    // ゲーム中
        GameOver,   // ゲームオーバー
        GameClear,  // ゲームクリア
    }

    // 生成するセルのプレファブです。
    [SerializeField]
    private GameObject CellPrefab;

    // フィールドに存在するすべてのセルです。
    public Cell[,] mCells { get; private set; }
    private GameState mState = GameState.None;
    private int mRow = 9;
    private int mCol = 9;
    private int mBombCount = 10;
    private int mFlagCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // フィールドの初期化(爆弾位置)を行います。
    public void Initialize()
    {
        Debug.Log("Initialize");
        mState = GameState.None;
        mCells = new Cell[mRow, mCol];

        // フィールドサイズの調整
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(mCol * Cell.Size, mRow * Cell.Size);

        // 生成されたランダム位置に爆弾フラグ設定
        for (int r = 0; r < mCells.GetLength(0); r++) {
            for (int c = 0; c < mCells.GetLength(1); c++) {
                mCells[r, c] = GenerateCell(r, c);
            }
        }

        Reset();
    }

    public void Reset()
    {
        if (mState == GameState.Ready) return;
        // Debug.Log("Reset " + mState);
        mFlagCount = 0;

        // ランダム生成された爆弾位置の配列
        var randomBombFlags = Enumerable.Concat(
            Enumerable.Repeat(true, mBombCount),
            Enumerable.Repeat(false, mRow * mCol - mBombCount)
            ).OrderBy(_ => Guid.NewGuid()).ToArray();

        // 生成されたランダム位置に爆弾フラグ設定
        int i = 0;
        for (int r = 0; r < mCells.GetLength(0); r++) {
            for (int c = 0; c < mCells.GetLength(1); c++) {
                mCells[r, c].Reset(randomBombFlags[i++]);
            }
        }

        // 隣接する爆弾を数えておく
        for (int r = 0; r < mCells.GetLength(0); r++) {
            for (int c = 0; c < mCells.GetLength(1); c++) {
                int cnt = GetNeighbourCells(r, c).Count(cell => cell.mIsBomb);
                mCells[r, c].SetNeighbourBombCount(cnt);
            }
        }

        var timer = GameObject.Find("Timer").GetComponent<Timer>();
        timer.Reset();
        DrawBombCount();
        DrawMessage("");
        mState = GameState.Ready;
    }

    private void DrawBombCount()
    {
        GameObject counter = GameObject.Find("BombCounter");
        if (!counter) return;
        Text t = counter.GetComponent<Text>();
        if (!t) return;
        int bombRest = mBombCount - mFlagCount;
        t.text = bombRest.ToString();
    }

    // セルを盤面に初期化・生成します。
    private Cell GenerateCell(int r, int c)
    {
        // ゲームオブジェクトを画面に配置します。
        var go = Instantiate(CellPrefab, this.GetComponent<RectTransform>());
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(Cell.Size / 2 + Cell.Size * r, Cell.Size / 2 + Cell.Size * c);

        // セルクラスを初期化します。
        var cell = go.GetComponent<Cell>();
        cell.Initialize(r, c);
        return cell;
    }

    // 隣接するセルをすべて取得します。
    private Cell[] GetNeighbourCells(int r, int c)
    {
        var cells = new List<Cell>();

        var isTop = r == 0;
        var isButtom = r == mCells.GetLength(0) - 1;
        var isLeft = c == 0;
        var isRight = c == mCells.GetLength(1) - 1;

        if (!isTop && !isLeft) cells.Add(mCells[r - 1, c - 1]);
        if (!isTop) cells.Add(mCells[r - 1, c]);
        if (!isTop && !isRight) cells.Add(mCells[r - 1, c + 1]);
        if (!isRight) cells.Add(mCells[r, c + 1]);
        if (!isButtom && !isRight) cells.Add(mCells[r + 1, c + 1]);
        if (!isButtom) cells.Add(mCells[r + 1, c]);
        if (!isButtom && !isLeft) cells.Add(mCells[r + 1, c - 1]);
        if (!isLeft) cells.Add(mCells[r, c - 1]);

        return cells.ToArray();
    }

    // クリックを監視してマークします
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;
        if (mState != GameState.Ready && mState != GameState.PlayNow) return;

        var collider = Physics2D.OverlapPoint(Input.mousePosition);
        if (!collider) return;
        var go = collider.transform.gameObject;
        var cell = go.GetComponent<Cell>();
        if (!cell) return;
        if (cell.mState == CellState.Opened) return;

        if (mState == GameState.Ready) {
            // ゲーム開始
            mState = GameState.PlayNow;
            var timer = GameObject.Find("Timer").GetComponent<Timer>();
            timer.OnStart();
        }

        // 左クリック
        if (Input.GetMouseButtonDown(0)) {
            if (!OpenCell(cell)) return;
        }  else { // 右クリック
            CellState preState = cell.mState;
            cell.ChangeMark();
            // 残りの爆弾数を更新
            if (preState != cell.mState && (preState == CellState.Flag || cell.mState == CellState.Flag)) {
                if (preState == CellState.Flag) --mFlagCount;
                else if (cell.mState == CellState.Flag) ++mFlagCount;
                DrawBombCount();
            }
            if (cell.mState != CellState.Flag) return;
        }

        // 展開に成功した場合、爆弾以外のすべてのセルが展開済の場合クリア
        if (IsGameClear()) {
            GameClear();
        }
    }

    private bool OpenCell(Cell cell)
    {
        if (cell.mState != CellState.Closing) return false;

        // Debug.Log("OpenCell (" + cell.row + ", " + cell.col + ") " + cell.State);
        cell.Open();

        // 爆弾だった場合ゲームオーバー
        if (cell.mIsBomb) {
            GameOver();
            return false;
        }

        if (cell.mNeighbourBombCount == 0) {
            OpenNeighbourCell(cell.mRow, cell.mCol);
        }
        return true;
    }

    private bool IsGameClear()
    {
        for (int r = 0; r < mCells.GetLength(0); r++) {
            for (int c = 0; c < mCells.GetLength(1); c++) {
                if (mCells[r, c].mState == CellState.Opened) continue;
                if (mCells[r, c].mState != CellState.Flag) return false;
                if (!mCells[r, c].mIsBomb) return false;
            }
        }
        return true;
    }

    // あるセルの隣接するセルすべてを展開します。
    private void OpenNeighbourCell(int r, int c)
    {
        // Debug.Log("*** OpenNeighbourCell (" + r + ", " + c + ") ***");
        foreach (var cell in GetNeighbourCells(r, c)) {
            // 展開済のセルには何もしません
            if (cell.mState == CellState.Closing) {
                OpenCell(cell);
            }
        }
    }

    // ゲームオーバー時の処理
    private void GameOver()
    {
        Debug.Log("ゲームオーバー");
        var timer = GameObject.Find("Timer").GetComponent<Timer>();
        timer.OnFinish();
        var reset = GameObject.Find("Reset").GetComponent<Reset>();
        reset.OnGameOver();
        DrawMessage("Game Over");
        mState = GameState.GameOver;
    }

    // ゲームクリア時の処理
    private void GameClear()
    {
        Debug.Log("ゲームクリア");
        var timer = GameObject.Find("Timer").GetComponent<Timer>();
        timer.OnFinish();
        var reset = GameObject.Find("Reset").GetComponent<Reset>();
        reset.OnGameClear();
        DrawMessage("Clear !!");
        mState = GameState.GameClear;
    }

    private void DrawMessage(string str)
    {
        GameObject msg = GameObject.Find("Message");
        if (!msg) return;
        Text t = msg.GetComponent<Text>();
        if (!t) return;
        t.text = str;
    }
}
