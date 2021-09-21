using System;
using System.Collections;
using UnityEngine;

public static class InvokeAfterExtension
{
  /// <summary>
  /// Invokes an action after a certain time, using a coroutine.
  /// </summary>
  /// <param name="behaviour">Behaviour to use as runner for the internal coroutine</param>
  /// <param name="afterTime">How long before the action should be invoked</param>
  /// <param name="action">Action to invoke</param>
  /// <returns>The Coroutine containing the waiting and action, so you can stop it if neccessary</returns>
  public static Coroutine InvokeAfter(this MonoBehaviour behaviour, float afterTime, Action action) {
      if (behaviour != null) {
          return behaviour.StartCoroutine(InvokeAfter(action, afterTime, behaviour));
      }
      return null;
  }

  private static IEnumerator InvokeAfter(Action action, float time, MonoBehaviour behaviour) {
      yield return new WaitForSeconds(time);
      if (behaviour != null) {
          action();
      }
  }
}