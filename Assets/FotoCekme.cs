using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FotoCekme : MonoBehaviour
{
    public Image fotoImage;
    public RawImage kameraPreview; // Kamera önizlemesi için RawImage nesnesi
    public Button kameraBaslatBtn;
    public Button cekveYollaBtn;
    private WebCamTexture kamera;
    public GameObject lutfenBekleyinizImage;
    public GameObject FalOkuPanel;
    // Kamera önizlemesini baþlatmak için buton tetikleyici
    public void BaslatKameraOnizleme()
    {
        kameraPreview.gameObject.SetActive(true);

        string[] kameraIsimleri = WebCamTexture.devices.Length > 0
            ? new string[] { WebCamTexture.devices[0].name }
            : new string[] { }; // Kamera bulunmuyorsa boþ bir dizi oluþtur

        if (kameraIsimleri.Length > 0)
        {
            kamera = new WebCamTexture(kameraIsimleri[0]);
            kamera.Play();
            kameraPreview.texture = kamera;
            kameraBaslatBtn.interactable = false;
            cekveYollaBtn.interactable = true; 
        }
        else
        {
            Debug.LogWarning("Kamera bulunamadý.");
        }
    }

    public void LutfenBekleyinizImage()
    {
        lutfenBekleyinizImage.transform.DOScale(new Vector3(14.91f, 14.91f, 14.91f),.35f).SetEase(Ease.InSine).SetDelay(1.5f);

    }
    // Fotoðraf çekme iþlemi
    public void CekFoto()
    {
        kameraPreview.gameObject.SetActive(false);
        cekveYollaBtn.interactable = false;
        if (kamera != null)
        {
            StartCoroutine(CekVeGoster());
        }
        else
        {
            Debug.LogWarning("Kamera baþlatýlmamýþ.");
        }
    }

    private IEnumerator CekVeGoster()
    {
        if (kamera == null)
        {
            yield break;
        }

        // Bir frame bekleyin
        yield return new WaitForEndOfFrame();

        // Kameradan fotoðrafý alýn
        Texture2D fotoTexture = new Texture2D(kamera.width, kamera.height);
        fotoTexture.SetPixels(kamera.GetPixels());
        fotoTexture.Apply();

        // Kamerayý kapatýn
        kamera.Stop();

        // Image nesnesine fotoðrafý yükleyin ve boyutlarý ayarlayýn
        fotoImage.sprite = Sprite.Create(fotoTexture, new Rect(0, 0, fotoTexture.width, fotoTexture.height), Vector2.one * 0.5f);

        // Kamera önizlemesini kapatýn
        kameraPreview.texture = null;

        GetComponent<KahveFallari>().YeniFalOlustur();
        LutfenBekleyinizImage();
        FalOkuPanel.transform.gameObject.SetActive(true);
        FalOkuPanel.transform.DOScale(Vector3.one,.35f).SetEase(Ease.InSine).SetDelay(10f);
    }
}
