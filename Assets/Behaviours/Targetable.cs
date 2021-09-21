using System;
using UnityEngine;
using System.Reflection; 

public class Targetable : MonoBehaviour {

  public event Action<Vector3> OnTargeted;
  public event Action OnTargetRemoved;
  public event Action OnDoubleTarget;

  private void Awake () {
    EventHub.OnTargetableTargeted += OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget += OnTargetableRemoveTarget;
    EventHub.OnTargetableDoubleTarget += OnTargetableDoubleTarget;
  }

  private void Start () {
    Debug.Log($"Targetable: {this.name}");
    EventHub.TargetableCreated(this);
  }

  private void OnTargetableTargeted (Targetable target, Vector3 point) {
    if (target.gameObject != this.gameObject) return;

    OnTargeted?.Invoke(point);
  }

  private void OnTargetableRemoveTarget (Targetable target) {
    if (target.gameObject != this.gameObject) return;

    OnTargetRemoved?.Invoke();
  }

  private void OnTargetableDoubleTarget (Targetable target) {
    if (target.gameObject != this.gameObject) return;

    OnDoubleTarget?.Invoke();
  }

  private void OnDestroy () {
    EventHub.OnTargetableTargeted -= OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget -= OnTargetableRemoveTarget;
    EventHub.OnTargetableDoubleTarget -= OnTargetableDoubleTarget;
    EventHub.TargetableDestroyed(this);
  }
}