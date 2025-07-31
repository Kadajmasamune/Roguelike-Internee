using System.Collections;
using Signals;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{

    private TextMesh damageText;
    private MeshRenderer meshRenderer;

    public void Awake()
    {
        damageText = GetComponent<TextMesh>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Setup(int damage, float fadeInTime, float fadeOutTime)
    {
        damageText.text = damage.ToString();
        meshRenderer.sortingLayerName = "DamagePopUp";
        StartCoroutine(PopupRoutine(fadeInTime, fadeOutTime));
    }

    private IEnumerator PopupRoutine(float fadeInTime, float fadeOutTime)
    {
        float fadeInElapsed = 0f;
        Color c = damageText.color;
        while (fadeInElapsed <= fadeInTime)
        {
            fadeInElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, fadeInTime, fadeInElapsed / fadeInTime);
            c.a = alpha;
            damageText.color = c;
            yield return null;
        }

        // Bob Up
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 2f, 0);  
        float bobDuration = 0.5f;
        float bobElapsed = 0f;
        while (bobElapsed < bobDuration)
        {
            bobElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, bobElapsed / bobDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Fade Out
        float fadeOutElapsed = 0f;
        while (fadeOutElapsed <= fadeOutTime)
        {
            fadeOutElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fadeOutTime, 0f, fadeOutElapsed / fadeOutTime);
            c.a = alpha;
            damageText.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
