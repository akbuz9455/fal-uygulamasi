using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurcYorum : MonoBehaviour
{
    public List<string> burcYorumlari;
    public string burcAdi;
    public GameObject mainCanvas;


    public void ShowCommend()
    {

        mainCanvas.GetComponent<BurcCanvasManager>().BaslikBurcMessage.SetText(burcAdi);
        mainCanvas.GetComponent<BurcCanvasManager>().IcerikBurcMessage.SetText(burcYorumlari[burcYorumlari.Count%DateTime.Now.Day]+"   "+DateTime.Now.ToShortDateString());
    }

}