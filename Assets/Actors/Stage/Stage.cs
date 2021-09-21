using System;
using UnityEngine;

public class Stage : MonoBehaviour {

  private void Start () {
    _camera = Camera.main;
  }

  private void Update () {
    // Find out where the mouse clicked by casting a ray.
    // NOTE: this is much more reliable than puting an OnMouseClick event handler
    // on a collider (which is intermittenyly buggy)
    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

    // If the ray didn't hit anything with a collider, we should exit the selection process.
    RaycastHit hitData;
    if (Physics.Raycast(ray, out hitData, Mathf.Infinity) == false) return;
    if (hitData.collider.gameObject != gameObject) return;
    
    // We need to dermine if this is a dragging interaction or a simple click/touch
    float distance = Vector3.Distance(_lastMouse, hitData.point);
    if (_lastMouse != Vector3.zero && distance > _dragSensitivity && _dragging == false) {
      _dragging = true;
      EventHub.StageDragStarted(_lastMouse);
    }
    
    // If the input is a down action, store the mouse position to compare for dragging calc.
    if (Input.GetMouseButtonDown(0)) {
      _lastMouse = hitData.point;
    }

    // Notifiy listeners of the new hit point if we are dragging.
    if (Input.GetMouseButton(0) && _dragging) {
      EventHub.StageDragUpdated(hitData.point);
    }

    // TODO: make this faster so quick drag group selects don't miss.
    // Once the click/touch is over, determine if this was a quick actio or a drag,
    // and notify all listeners of the result.
    if (Input.GetMouseButtonUp(0)) {
      if (_dragging == false) {
        EventHub.StageSelected(hitData.point);
      } else {
        _dragging = false;
        EventHub.StageDragStopped(hitData.point);
      }
      
      _lastMouse = Vector3.zero;
    }

  }

  [SerializeField] private float _dragSensitivity = 1f;

  private Camera _camera;
  private Vector3 _lastMouse;
  private bool _dragging = false;

}