using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Utility : MonoBehaviour
{
    public static void KillTween(Tween tweenToKill)
    {
        if (tweenToKill != null) tweenToKill.Kill();
    }
    public static void KillTweens(Tween tweenA, Tween tweenB, Tween tweenC = null)
    {
        if (tweenA != null) tweenA.Kill();
        if (tweenB != null) tweenB.Kill();
        if (tweenC != null) tweenC.Kill();
    }

    public static bool IsPlayer(Collider2D collision)
    {
        PlayerModel player = collision.GetComponent<PlayerModel>();
        if (player == null) return false;

        return true;
    }

    public static void UpdateTriggerColliders(Transform transform, bool enable)
    {
        Collider2D[] colliders = transform.GetComponents<Collider2D>();
        foreach (Collider2D collider2D in colliders)
        {
            if (collider2D.isTrigger) collider2D.enabled = enable;
        }
    }

    public static string GetYellowText(string textToCheck_)
    {
        TextColour[] textColours = new TextColour[1];
        textColours[0] = TextColour.Yellow;
        string textToCheck = textToCheck_;
        textToCheck = DialogueManager.CheckForColouredText(textToCheck, textColours);

        return textToCheck;
    }

    /// <summary>
    /// Selects a random item from a given list.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="list">The list to select from.</param>
    /// <returns>A random item from the list, or default(T) if the list is empty.</returns>
    public static T GetRandomItemFromList<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return default(T); // Return default if the list is empty
        }

        // Generate a random index
        // Random.Range with integer arguments is exclusive for the max value.
        // So Random.Range(0, list.Count) will return a value between 0 and list.Count-1, which are valid indices.
        int randomIndex = Random.Range(0, list.Count);

        // Return the item at the random index.
        return list[randomIndex];
    }
}
