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

            // Özel iþareti kullanarak metni bölün
            string[] satirlar = falicerik.Split('@');

            // Metni düzenleyerek yeni satýrlara geçin
            string tamMetin = string.Join("\n", satirlar);

            // Metni metin göstereci üzerinde görüntüleyin
            kahveFaliText.SetText(tamMetin);
        }

    

}
