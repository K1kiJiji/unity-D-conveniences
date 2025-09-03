using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Project.Scripts.Conveniences
{
    public class Transitions : MonoBehaviour
    {
        private static IEnumerator LerpFadeCore(float limit, float start, float end, System.Action<float> setValue, bool unscaled)
        {
            if (limit <= 0f)
            {
                setValue(end);
                yield break;
            }

            setValue(start);

            float duration = 0f;
            while (duration < limit)
            {
                duration += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                setValue(Mathf.Lerp(start, end, duration / limit));
                yield return null;
            }

            setValue(end);
        }

        private static IEnumerator LerpFadeGraphic(Graphic graphic, float limit, float start, float end, bool unscaled)
        {
            return LerpFadeCore(limit, start, end, setValue: alpha =>
            {
                var color = graphic.color;
                color.a = alpha;
                graphic.color = color;
            }, 
            unscaled);
        }

        private static IEnumerator LerpFadeCanvasGroup(CanvasGroup canvasGroup, float limit, float start, float end, bool unscaled, bool setInteractable)
        {
            if (setInteractable)
            {
                bool visibleStart = start >= 0.99f;
                canvasGroup.interactable = visibleStart;
                canvasGroup.blocksRaycasts = visibleStart;
            }

            yield return LerpFadeCore(limit, start, end, setValue: alpha => canvasGroup.alpha = alpha, unscaled);

            if (setInteractable)
            {
                bool visibleEnd = end >= 0.99f;
                canvasGroup.interactable = visibleEnd;
                canvasGroup.blocksRaycasts = visibleEnd;
            }
        }


        private static float EaseShift(float duration, float peak)
        {
            duration = Mathf.Clamp01(duration);
            float durationPeak = Mathf.Clamp(peak, 0.01f, 0.99f);

            if (duration < durationPeak)
            {
                float lerp = duration / durationPeak;
                return lerp * lerp * durationPeak;
            }

            else
            {
                float lerp = (1f - duration) / (1f - durationPeak);
                return 1f - lerp * lerp * (1f - durationPeak);
            }
        }

        private static IEnumerator EaseShiftFadeCore(float limit, float peak, float start, float end, System.Action<float> setValue, bool unscaled)
        {
            if (limit <= 0f)
            {
                setValue(end);
                yield break;
            }

            float duration = 0f;
            while (duration < limit)
            {
                duration += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                float lerp = Mathf.Clamp01(duration / limit);
                float easeShift = EaseShift(lerp, peak);
                setValue(Mathf.Lerp(start, end, easeShift));
                yield return null;
            }

            setValue(end);
        }

        private static IEnumerator EaseShiftFadeGraphic(Graphic graphic, float limit, float peak, float start, float end, bool unscaled)
        {
            return EaseShiftFadeCore(limit, peak, start, end, setValue: alpha =>
                {
                    var color = graphic.color;
                    color.a = alpha;
                    graphic.color = color;
                }, 
                unscaled);
        }

        private static IEnumerator EaseShiftFadeCanvasGroup(CanvasGroup canvasGroup, float limit, float peak, float start, float end, bool unscaled, bool setInteractable)
        {
            if (setInteractable)
            {
                bool visibleStart = start >= 0.99f;
                canvasGroup.interactable = visibleStart;
                canvasGroup.blocksRaycasts = visibleStart;
            }

            yield return EaseShiftFadeCore(limit, peak, start, end, setValue: alpha => canvasGroup.alpha = alpha, unscaled);

            if (setInteractable)
            {
                bool visibleEnd = end >= 0.99f;
                canvasGroup.interactable = visibleEnd;
                canvasGroup.blocksRaycasts = visibleEnd;
            }
        }


        private static IEnumerator Wait(float hold, bool unscaled)
        {
            if (hold <= 0f)
            {
                yield break;
            }

            float tick = 0f;
            while (tick < hold)
            {
                tick += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }


        public static IEnumerator LerpFadeIn(Graphic graphic, float duration, float endOpacity, bool unscaled) => LerpFadeGraphic(graphic, duration, 0f, endOpacity, unscaled);

        public static IEnumerator LerpFadeOut(Graphic graphic, float duration, float startOpacity, bool unscaled) => LerpFadeGraphic(graphic, duration, startOpacity, 0f, unscaled);

        public static IEnumerator LearpFadeInOut(Graphic graphic, float second, float inDuration, float outDuration, float opacity, bool unscaled)
        {
            yield return LerpFadeIn(graphic, inDuration, opacity, unscaled);
            yield return Wait(second, unscaled);
            yield return LerpFadeOut(graphic, outDuration, opacity, unscaled);
        }

        public static IEnumerator LerpFadeIn(CanvasGroup canvasGroup, float duration, float endOpacity, bool unscaled, bool setInteractable) => LerpFadeCanvasGroup(canvasGroup, duration, 0f, endOpacity, unscaled, setInteractable);

        public static IEnumerator LerpFadeOut(CanvasGroup canvasGroup, float duration, float startOpacity, bool unscaled, bool setInteractable) => LerpFadeCanvasGroup(canvasGroup, duration, startOpacity, 0f, unscaled, setInteractable);

        public static IEnumerator LearpFadeInOut(CanvasGroup canvasGroup, float second, float inDuration, float outDuration, float opacity, bool unscaled, bool setInteractable)
        {
            yield return LerpFadeIn(canvasGroup, inDuration, opacity, unscaled, setInteractable);
            yield return Wait(second, unscaled);
            yield return LerpFadeOut(canvasGroup, outDuration, opacity, unscaled, setInteractable);
        }


        public static IEnumerator EaseShiftFadeIn(Graphic graphic, float duration, float peak, float endOpacity, bool unscaled) => EaseShiftFadeGraphic(graphic, duration, peak, 0f, endOpacity, unscaled);

        public static IEnumerator EaseShiftFadeOut(Graphic graphic, float duration, float peak, float startOpacity, bool unscaled) => EaseShiftFadeGraphic(graphic, duration, peak, startOpacity, 0f, unscaled);

        public  static IEnumerator EaseShiftFadeInOut(Graphic graphic, float peak, float second, float inDuration, float outDuration, float opacity, bool unscaled)
        {
            yield return EaseShiftFadeIn(graphic, inDuration, peak, opacity,unscaled);
            yield return Wait(second, unscaled);
            yield return EaseShiftFadeOut(graphic, outDuration, peak, opacity, unscaled);
        }

        public static IEnumerator EaseShiftFadeIn(CanvasGroup canvasgroup, float duration, float peak, float endOpacity, bool unscaled, bool setInteractable) => EaseShiftFadeCanvasGroup(canvasgroup, duration, peak, 0f, endOpacity, unscaled, setInteractable);

        public static IEnumerator EaseShiftFadeOut(CanvasGroup canvasgroup, float duration, float peak, float startOpacity, bool unscaled, bool setInteractable) => EaseShiftFadeCanvasGroup(canvasgroup, duration, peak, startOpacity, 0f, unscaled, setInteractable);

        public  static IEnumerator EaseShiftFadeInOut(CanvasGroup canvasgroup, float peak, float second, float inDuration, float outDuration, float opacity, bool unscaled, bool setInteractable)
        {
            yield return EaseShiftFadeIn(canvasgroup, inDuration, peak, opacity, unscaled, setInteractable);
            yield return Wait(second, unscaled);
            yield return EaseShiftFadeOut(canvasgroup, outDuration, peak, opacity, unscaled, setInteractable);
        }
    }
}