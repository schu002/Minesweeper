using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reset : MonoBehaviour
{
    public Sprite normal;
    public Sprite gameover;
    public Sprite finish;

    // Start is called before the first frame update
    void Start()
    {
        var img = GetComponent<Image>();
        img.enabled = true;
    }

    public void OnGameOver()
    {
        var img = GetComponent<Image>();
        img.sprite = gameover;
    }

    public void OnGameClear()
    {
        var img = GetComponent<Image>();
        img.sprite = finish;
    }

    public void OnClick()
    {
        var img = GetComponent<Image>();
        img.sprite = normal;
        var field = GameObject.Find("Field").GetComponent<Field>();
        field.Reset();
    }
}
