using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultipleTargeting : MonoBehaviour {

  public void Awake () {
    EventHub.OnTargetableCreated += OnTargetableCreated;
    EventHub.OnTargetableDestroyed += OnTargetableDestroyed;

    EventHub.OnStageDragStarted += OnStageDragStarted;
    EventHub.OnStageDragUpdated += OnStageDragUpdated;
    EventHub.OnStageDragStopped += OnStageDragStopped;
  }

  public void Start () {
    _camera = Camera.main;
    _selector = transform.Find("Selector").GetComponent<RectTransform>();
    _selector.transform.gameObject.SetActive(true);
    DrawDragSelect();
  }

  private void OnTargetableCreated (Targetable target) {
    _targets.Add(target);
    _targets.ForEach(target => Debug.Log($"Targeting Added: {target.name}"));
  }

  private void OnTargetableDestroyed (Targetable target) {
    _targets.Remove(target);
  }

  private void OnStageDragStarted (Vector3 point) {
    Debug.Log($"Stage Drag Started: {point}");
    _dragStart = Input.mousePosition;
    _targetBox = new Rect();
  }

  private void OnStageDragUpdated (Vector3 point) {
    _dragEnd = Input.mousePosition;
    DrawDragSelect();
    DrawSelection();
  }

  private void OnStageDragStopped (Vector3 point) {
    Debug.Log($"Stage Drag Stopped: {point}");

    _targets.ForEach(target => {
      Vector3 targetPosition = _camera.WorldToScreenPoint(target.transform.position);
      if (_targetBox.Contains(targetPosition) == false) return;
      EventHub.TargetableTargeted(target, targetPosition);
    });

    // Clear the draw box.
    _dragStart = Vector2.zero;
    _dragEnd = Vector2.zero;
    DrawDragSelect();
  }

  private void DrawDragSelect () {
    Vector2 boxStart = _dragStart;
    Vector2 boxEnd = _dragEnd;
    Vector2 boxCenter = (boxStart + boxEnd) / 2;

    _selector.position = boxCenter;

    Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
    _selector.sizeDelta = boxSize; 
  }

  private void DrawSelection () {
    // To start we'll assume a left to right, top to bottom drag
    _targetBox.xMin = _dragStart.x;
    _targetBox.xMax = Input.mousePosition.x;
    _targetBox.yMin = Input.mousePosition.y;
    _targetBox.yMax = _dragStart.y;

    // Then we'll modify accordingly for other use cases, like draging right to left ...
    if (Input.mousePosition.x < _dragStart.x) {
      _targetBox.xMin = Input.mousePosition.x;
      _targetBox.xMax = _dragStart.x;
    }

    // Or dragging bottom to top.
    if (Input.mousePosition.y  > _dragStart.y) {
      _targetBox.yMin = _dragStart.y;
      _targetBox.yMax = Input.mousePosition.y;
    }
  }

  private void OnDestroy () {
    // Remove any event hub listeners as needed.
    EventHub.OnTargetableCreated -= OnTargetableCreated;
    EventHub.OnTargetableDestroyed -= OnTargetableDestroyed;

    EventHub.OnStageDragStarted -= OnStageDragStarted;
    EventHub.OnStageDragUpdated -= OnStageDragUpdated;
    EventHub.OnStageDragStopped -= OnStageDragStopped;
  }


  private Camera _camera;
  private RectTransform _selector;

  private Vector2 _dragStart = Vector2.zero;
  private Vector2 _dragEnd = Vector2.zero;
  private Rect _targetBox = new Rect();

  private List<Targetable> _targets = new List<Targetable>();
}
