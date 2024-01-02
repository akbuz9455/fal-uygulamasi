using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FotoCekme : MonoBehaviour
{
    public Image fotoImage;
    public RawImage kameraPreview; // Kamera �nizlemesi i�in RawImage nesnesi
    public Button kameraBaslatBtn;
    public Button cekveYollaBtn;
    private WebCamTexture kamera;
    public GameObject lutfenBekleyinizImage;
    public GameObject FalOkuPanel;
    // Kamera �nizlemesini ba�latmak i�in buton tetikleyici
    public void BaslatKameraOnizleme()
    {
        kameraPreview.gameObject.SetActive(true);

        string[] kameraIsimleri = WebCamTexture.devices.Length > 0
            ? new string[] { WebCamTexture.devices[0].name }
            : new string[] { }; // Kamera bulunmuyorsa bo� bir dizi olu�tur

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
            Debug.LogWarning("Kamera bulunamad�.");
        }
    }

    public void LutfenBekleyinizImage()
    {
        lutfenBekleyinizImage.transform.DOScale(new Vector3(14.91f, 14.91f, 14.91f),.35f).SetEase(Ease.InSine).SetDelay(1.5f);

    }
    // Foto�raf �ekme i�lemi
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
            Debug.LogWarning("Kamera ba�lat�lmam��.");
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

        // Kameradan foto�raf� al�n
        Texture2D fotoTexture = new Texture2D(kamera.width, kamera.height);
        fotoTexture.SetPixels(kamera.GetPixels());
        fotoTexture.Apply();

        // Kameray� kapat�n
        kamera.Stop();

        // Image nesnesine foto�raf� y�kleyin ve boyutlar� ayarlay�n
        fotoImage.sprite = Sprite.Create(fotoTexture, new Rect(0, 0, fotoTexture.width, fotoTexture.height), Vector2.one * 0.5f);

        // Kamera �nizlemesini kapat�n
        kameraPreview.texture = null;

        GetComponent<KahveFallari>().YeniFalOlustur();
        LutfenBekleyinizImage();
        FalOkuPanel.transform.gameObject.SetActive(true);
        FalOkuPanel.transform.DOScale(Vector3.one,.35f).SetEase(Ease.InSine).SetDelay(10f);
    }
}
