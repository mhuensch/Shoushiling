using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleTargeting : MonoBehaviour {

  public void Awake () {
    EventHub.OnTargetableTargeted += OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget += OnTargetableRemoveTarget;
  }

  public void Start () {
    _camera = Camera.main;
  }

  private void OnTargetableTargeted (Targetable target, Vector3 point) {
    Debug.Log($"Adding: {target}");
    _targeted = target;
  }

  private void OnTargetableRemoveTarget (Targetable target) {
    Debug.Log($"Removing: {target}");
    if (_targeted != target) return;
    _targeted = null;
  }

  private void Update () {
    // At first, it might seem like you want these actions to be part of FixedUpdate.
    // However, because this involves updates to the UI, we'll put them here in the normal update method.

    // If the left mouse button isn't down, we don't want to process anything
    if (Input.GetMouseButtonUp(0) == false) return;
    // Debug.Log("Right Mouse Clicked");

    // Find out where the mouse clicked by casting a ray.
    // NOTE: this is much more reliable than puting an OnMouseClick event handler
    // on a collider (which is intermittenyly buggy)
    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

    // If the ray didn't hit anything with a collider, we should exit the selection process.
    RaycastHit hitData;
    if (Physics.Raycast(ray, out hitData, Mathf.Infinity) == false) return;
    
    // Get the object that was hit so we can determine how to interact with it.
    GameObject hitObject = hitData.collider.gameObject;
    Debug.Log($"Left Clicked: {hitObject.name} {hitData.point}");

    // Confirm that the object hit is a targetable or leave the method.
    Targetable target = hitObject.GetComponent<Targetable>();
    if (target == null) return;

    // Raise an event so any listeners know something was targeted.
    if (_lastTargeted == target && Time.time - _lastTargetedOn < _doubleClickSensitivity) {
      // WHEN IT'S A DOUBLE CLICK ON THE SAME TARGET
      EventHub.TargetableDoubleTarget(target);
    }
    else if (_targeted == target) {
      // WHEN IT'S THE SAME TARGET
      EventHub.TargetableRemoveTarget(_targeted);
    }
    else {
      // WHEN IT'S A DIFFERENT TARGET
      if (_targeted != null) EventHub.TargetableRemoveTarget(_targeted);
      EventHub.TargetableTargeted(target, hitData.point);
    } 
 
    _lastTargeted = target;
    _lastTargetedOn = Time.time;
  }


  private void OnDestroy () {
    EventHub.OnTargetableTargeted -= OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget -= OnTargetableRemoveTarget;
  }

  [SerializeField] private float _doubleClickSensitivity = 0.25f;

  private Camera _camera;
  private Targetable _targeted;

  private float _lastTargetedOn;
  private Targetable _lastTargeted;
  
}
