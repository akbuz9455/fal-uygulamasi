using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChatCharacters : MonoBehaviour
{
    public float transitionTime = 3.0f;
    private float elapsedTime = 0.0f;
    private Color startColor = Color.black;
    private Color endColor = Color.white;

    void Start()
    {
        StartCoroutine(TransitionColor());
    }

    void Update()
    {
        
    }

    IEnumerator TransitionColor()
    {
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            // Apply the current color to the object's material or sprite renderer
            GetComponent<Image>().color = currentColor;

            yield return null;
        }
    }
}