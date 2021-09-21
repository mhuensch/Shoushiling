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
    _targeted.Add(target);
  }

  private void OnTargetableRemoveTarget (Targetable target) {
    Debug.Log($"Removing: {target}");
    _targeted.Remove(target);
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
    if (_targeted.Contains(target) == false) {
      EventHub.TargetableTargeted(target, hitData.point);
    }
    else {
      EventHub.TargetableRemoveTarget(target);
    }
  }

  private void OnDestroy () {
    EventHub.OnTargetableTargeted -= OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget -= OnTargetableRemoveTarget;
  }

  private Camera _camera;
  private List<Targetable> _targeted = new List<Targetable>();
}
