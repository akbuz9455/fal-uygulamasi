using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BurcCanvasManager : MonoBehaviour
{
    public GameObject BurcMessageObj;
    public TextMeshProUGUI BaslikBurcMessage;
    public TextMeshProUGUI IcerikBurcMessage;

    void Start()
    {
        
    }
    public void SahneyiYukle(string sahneAdi)
    {
        SceneManager.LoadScene(sahneAdi);
    }

    public void OpenBurcMessagePanel()
    {

        BurcMessageObj.transform.DOScale(Vector3.one * 9, .45f).SetEase(Ease.OutSine);
    }

    public void CloseBurcMessagePanel()
    {

        BurcMessageObj.transform.DOScale(Vector3.zero, .35f).SetEase(Ease.InSine);
    }
}
