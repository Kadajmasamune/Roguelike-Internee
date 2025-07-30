using System.Collections;
using Signals;
using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshProUGUI damageText;

    public void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(int damage, float fadeInTime, float fadeOutTime)
    {
        damageText.text = damage.ToString();
        StartCoroutine(PopupRoutine(fadeInTime, fadeOutTime));
    }

    private IEnumerator PopupRoutine(float fadeInTime, float fadeOutTime)
    {
        float fadeInElapsed = 0f;
        while (fadeInElapsed <= fadeInTime)
        {
            fadeInElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeInElapsed / fadeInTime);
            damageText.alpha = alpha;
            yield return null;
        }

        // Bob Up
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 30f, 0);  
        float bobDuration = 0.3f;
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
            float alpha = Mathf.Lerp(1f, 0f, fadeOutElapsed / fadeOutTime);
            damageText.alpha = alpha;
            yield return null;
        }

        Destroy(gameObject);
    }
}
