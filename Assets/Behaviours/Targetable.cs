using System;
using UnityEngine;
using System.Reflection; 

public class Targetable : MonoBehaviour {

  public event EventHandler<Vector3> OnTargeted;
  public event EventHandler OnTargetRemoved;

  private void Awake () {
    EventHub.OnTargetableTargeted += OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget += OnTargetableRemoveTarget;
  }

  private void Start () {
    Debug.Log($"Targetable: {this.name}");
    EventHub.TargetableCreated(this);
  }

  private void OnTargetableTargeted (Targetable target, Vector3 point) {
    if (target.gameObject != this.gameObject) return;

    OnTargeted?.Invoke(this, point);
  }

  private void OnTargetableRemoveTarget (Targetable target) {
    if (target.gameObject != this.gameObject) return;

    OnTargetRemoved?.Invoke(this, null);
  }

  private void OnDestroy () {
    EventHub.OnTargetableTargeted -= OnTargetableTargeted;
    EventHub.OnTargetableRemoveTarget -= OnTargetableRemoveTarget;
    EventHub.TargetableDestroyed(this);
  }
}