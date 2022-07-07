using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoExtensions
{

    public static Coroutine InvokeSafe(this MonoBehaviour behavior, System.Action method, float delayInSeconds)
    {
        return behavior.StartCoroutine(InvokeSafeRoutine(method, delayInSeconds));
    }
    public static Coroutine InvokeRepeatingSafe(this MonoBehaviour behavior, System.Action method, WaitForSecondsRealtime waitForSecondsRealTime, WaitForSecondsRealtime waitForSecondRepeat)
    {
        Coroutine _coroutine = behavior.StartCoroutine(InvokeSafeRepeatingRoutine(method, waitForSecondsRealTime, waitForSecondRepeat));
        return _coroutine;
    }
    public static Coroutine InvokeRepeatingSafe(this MonoBehaviour behavior, System.Action method, float delay, float repeatRate)
    {
        Coroutine _coroutine = behavior.StartCoroutine(InvokeSafeRepeatingRoutine(method, new WaitForSecondsRealtime(delay), new WaitForSecondsRealtime(repeatRate)));
        return _coroutine;
    }
    public static IEnumerator InvokeRepeatingSafe2(this MonoBehaviour behavior, System.Action method, WaitForSecondsRealtime waitForSecondsRealTime, WaitForSecondsRealtime waitForSecondRepeat)
    {
        IEnumerator ie = InvokeSafeRepeatingRoutine(method, waitForSecondsRealTime, waitForSecondRepeat);
        behavior.StartCoroutine(ie);
        return ie;
    }

    private static IEnumerator InvokeSafeRepeatingRoutine(System.Action method, WaitForSecondsRealtime waitForSecondsRealTime, WaitForSecondsRealtime waitForSecondRepeat)
    {
        yield return waitForSecondsRealTime;

        while (true)
        {
            if (method != null) method.Invoke();
            yield return waitForSecondRepeat;
        }
    }

    private static IEnumerator InvokeSafeRepeatingRoutine(System.Action method, float delayInSeconds, float repeatRateInSeconds)
    {
        yield return new WaitForSecondsRealtime(delayInSeconds);

        while (true)
        {
            if (method != null) method.Invoke();
            yield return new WaitForSecondsRealtime(repeatRateInSeconds);
        }
    }

    private static IEnumerator InvokeSafeRoutine(System.Action method, float delayInSeconds)
    {
        yield return new WaitForSecondsRealtime(delayInSeconds);
        if (method != null) method.Invoke();
    }
}