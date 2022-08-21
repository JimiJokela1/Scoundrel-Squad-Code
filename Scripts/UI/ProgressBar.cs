using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Transform emptyBlockParent;
    public Transform filledBlockParent;

    public GameObject emptyBlockPrefab;
    public GameObject filledBlockPrefab;

    protected Image[] emptyBlocks;
    protected Image[] filledBlocks;

    private bool initialized = false;
    
    public void Init(int maxBlockCount)
    {
        if(initialized)return;
        initialized = true;

        emptyBlocks = new Image[maxBlockCount];
        filledBlocks = new Image[maxBlockCount];

        for (int i = 0; i < maxBlockCount; i++)
        {
            GameObject emptyBlock = Instantiate(emptyBlockPrefab, emptyBlockParent);
            emptyBlock.SetActive(false);
            emptyBlocks[i] = emptyBlock.GetComponent<Image>();

            GameObject filledBlock = Instantiate(filledBlockPrefab, filledBlockParent);
            filledBlock.SetActive(false);
            filledBlocks[i] = filledBlock.GetComponent<Image>();
        }
    }

    public void SetEmptyBlockAmount(int blocks)
    {
        foreach(Image emptyBlock in emptyBlocks)
        {
            emptyBlock.gameObject.SetActive(false);
        }

        for (int i = 0; i < blocks; i++)
        {
            emptyBlocks[i].gameObject.SetActive(true);
        }
    }

    public void SetFilledBlockAmount(int blocks)
    {
        foreach(Image filledBlock in filledBlocks)
        {
            filledBlock.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < blocks; i++)
        {
            filledBlocks[i].gameObject.SetActive(true);
        }
    }
}
