using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Targetable))]
public class Spawnable : MonoBehaviour {

  private void Awake () {
    EventHub.OnSpawned += OnSpawned;
  }

  private void Start () {
    _camera = Camera.main;
    _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
  }

  private void Update () {
    UpdateMovement();
  }

  private void OnSpawned (Spawnable spawned, Vector3 rallyPoint) {
    if (spawned != this) return;

    _rallyPoint = rallyPoint;

    _controllable = GetComponent<Controllable>();
    if (_controllable == null) return;
    _controllable.OnRallyPointSet += OnRallyPointSet;
  }

  private void OnRallyPointSet (object source, Vector3 rallyPoint) {
    _rallyPoint = rallyPoint;
    Debug.Log($"OnRallyPointSet: {rallyPoint}");
    EventHub.TargetableRemoveTarget(GetComponent<Targetable>());
  }

  private void UpdateMovement () {
    // If we're offscren, we need to move back onscreen. 
    if (IsOffScreen(_camera, transform.position)) {
      // Debug.Log($"Offscreen, Returning Last Position: ${_lastOnScreenPosition}");
      _navMeshAgent.destination = _lastOnScreenPosition;
    } 
    else {
      // Debug.Log($"Onscreen, Returning Rally Position: ${RallyPoint}");
      _lastOnScreenPosition = transform.position;
      _navMeshAgent.destination = _rallyPoint;
    }
  }

  private static bool IsOffScreen (Camera camera, Vector3 position) {
    // Calculate whether or not the agents rotation point is "off screen".
    // Most of the time we should be OK if part of the agent is offscreen.
    // If not, we'll need to adjust this for the size of the mesh.
    Vector3 screenPosition = camera.WorldToScreenPoint(position);
    return Screen.safeArea.Contains(screenPosition) == false;
  }

  private void OnDestroy () {
    EventHub.OnSpawned -= OnSpawned;
    if (_controllable != null) _controllable.OnRallyPointSet -= OnRallyPointSet;
  }

  [SerializeField] private Vector3 _rallyPoint;

  private Camera _camera;
  private Controllable _controllable;
  private Vector3 _lastOnScreenPosition;
  private UnityEngine.AI.NavMeshAgent _navMeshAgent;
}