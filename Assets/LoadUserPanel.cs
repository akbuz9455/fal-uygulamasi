using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class LoadUserPanel : MonoBehaviour
{
    public List<GameObject> items;
    public Image BGImage;

    public void ScaleAndDisappear()
    {
        float i = 0.5f;       
        
        foreach (GameObject item in items)
        {
            item.transform.DOScale(Vector3.zero, 0.5f).SetDelay(i).SetEase(Ease.OutQuad).OnComplete(() => item.SetActive(false));
            i += 0.5f;
        }

        Invoke("BgClose",(0.5f*items.Count+0.5f));
    }

    private void BgClose()
    {
        BGImage.DOFillAmount(0, 1f).SetEase(Ease.InOutSine).OnComplete(() => BGImage.gameObject.SetActive(false));
    }
}