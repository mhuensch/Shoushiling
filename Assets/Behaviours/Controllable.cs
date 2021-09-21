using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Targetable))]
public class Controllable : MonoBehaviour {

  public event EventHandler<Vector3> OnRallyPointSet;

  private void Awake () {
    EventHub.OnStageSelected += OnStageSelected;

    _targetable = GetComponent<Targetable>();
    _targetable.OnTargeted += OnTargeted;
    _targetable.OnTargetRemoved += OnTargetRemoved;
  }

  private void OnTargeted (object sender, Vector3 point) {
    _targeted = true;
  }

  private void OnTargetRemoved (object sender, EventArgs e) {
    _targeted = false;
  }

  private void OnStageSelected (Vector3 point) {
    if (_targeted == false) return;
    Debug.Log("Stage Selected");
    OnRallyPointSet?.Invoke(this, point);
  }

  private void OnDestroy () {
    EventHub.OnStageSelected -= OnStageSelected;
    
    _targetable.OnTargeted -= OnTargeted;
    _targetable.OnTargetRemoved -= OnTargetRemoved;
  }

  [SerializeField] private bool _targeted = false;
  private Targetable _targetable;
  
}