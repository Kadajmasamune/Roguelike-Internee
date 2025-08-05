using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BulletTimePostProcessingScript : MonoBehaviour
{
    [SerializeField] private Volume _BulletTimePostProcessor;


    public IEnumerator StartPostProcessingEffect(float start, float endtime)
    {
        float elapsedTime = 0f;
        float end = 1f;
        float endeffect = start;


        while (elapsedTime <= start)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float _weight = Mathf.Lerp(0f, end, elapsedTime / start);
            _BulletTimePostProcessor.weight = _weight;
            yield return null;
        }

        yield return new WaitForSeconds(endtime);

        float fadeOutElapsed = 0f;
        while (fadeOutElapsed <= endeffect)
        {
            fadeOutElapsed += Time.deltaTime;
            float _weight = Mathf.Lerp(1f, 0f, fadeOutElapsed / endeffect);
            _BulletTimePostProcessor.weight = _weight;
            yield return null;
        }


    }
}
