using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KahveFallari : MonoBehaviour
{
    public List<string> kahveFalicerik;
    public TextMeshProUGUI kahveFaliText;

    private void Start()
    {

    }
    public void YeniFalOlustur()
    {
            string falicerik = kahveFalicerik[Random.Range(0, kahveFalicerik.Count)];

            // �zel i�areti kullanarak metni b�l�n
            string[] satirlar = falicerik.Split('@');

            // Metni d�zenleyerek yeni sat�rlara ge�in
            string tamMetin = string.Join("\n", satirlar);

            // Metni metin g�stereci �zerinde g�r�nt�leyin
            kahveFaliText.SetText(tamMetin);
        }

    

}
