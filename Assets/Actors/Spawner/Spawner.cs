using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Targetable))]
public class Spawner : MonoBehaviour {
  private void Awake () {
    EventHub.OnSpawnTypeSelected += OnSpawnTypeSelected;
  }

  private void Start () {
    _startPoint = transform.position;

    _rallyLine = this.gameObject.AddComponent<LineRenderer>();
    _rallyLine.material = GetComponent<MeshRenderer>().material;
    _rallyLine.material.renderQueue = 0;
    _rallyLine.startWidth = 0.1f;
    _rallyLine.endWidth = 0.1f;
    _rallyLine.startColor = Color.red;
    _rallyLine.endColor = Color.red;
    _rallyLine.useWorldSpace = true;

    _targetable = GetComponent<Targetable>();
    _targetable.OnTargeted += OnTargeted;
    _targetable.OnTargetRemoved += OnTargetRemoved;

    // TODO: move this up into the awake function? once we have a way to choose players?
    if (_player == EventHub.Players.Human) {
      Debug.Log("Adding Controlable");
      Controllable controllable = gameObject.AddComponent<Controllable>();
      controllable.OnRallyPointSet += OnRallyPointSet;
    }

    OnRallyPointSet(this, _rallyPoint);
    HideRallyLine();
  }

  private void Update () {
    // If the spawn type for this factory has not been set, exit;
    if (_spawnType == EventHub.AgentTypes.None) return;

    // Add the time passed to the countdown to build complete,
    // and if it's still less than the spawn delay time, exit.
    _spawnCountdown += Time.deltaTime;
    if (_spawnCountdown < _spawnDelay) return;

    // Create a new spawn of the type given and get its spawnable component.
    Spawnable spawned = Instantiate(_spawnPrefab, _spawnPoint, Quaternion.identity).GetComponent<Spawnable>();
    
    // If the owner of this spawner is human, add the controlable behavior to the game object.
    if (_player == EventHub.Players.Human && spawned.gameObject.GetComponent<Controllable>() == null) {
      spawned.gameObject.AddComponent<Controllable>();
    }

    // Notify all listeners that a new game object was spawned and the rally point of this factory.
    // This is mainly done to let the instantiated spawn know where to rally to without having to reference a public method.
    EventHub.Spawned(spawned, _spawnType, _rallyPoint);

    // Reset the spawn countdown and the spawn type to none.
    _spawnCountdown -= _spawnDelay;
    _spawnType = EventHub.AgentTypes.None;
  }

  private void OnSpawnTypeSelected (EventHub.AgentTypes spawnType) {
    _spawnType = spawnType;
    Addressables.LoadAssetAsync<GameObject>($"{_spawnType} Prefab").Completed += OnPrefabLoaded;
    EventHub.Spawning(_spawnType, _spawnDelay);
  }

  private void OnPrefabLoaded (AsyncOperationHandle<GameObject> obj) {
    Debug.Log("Prefab Loaded");
    _spawnPrefab = obj.Result;
  }

  private void OnTargeted (Vector3 point) {
    _specificallyTargeted = true;
    ShowRallyLine();
  }

  private void OnTargetRemoved () {
    if (_specificallyTargeted == false) return;
    _specificallyTargeted = false;
    ShowRallyLine();
    this.InvokeAfter(0.75f, HideRallyLine);
  }

  private void ShowRallyLine () {
    List<Vector3> positions = new List<Vector3> { transform.position, _rallyPoint };
    _rallyLine.SetPositions(positions.ToArray());
  }
  
  private void HideRallyLine () {
    List<Vector3> positions = new List<Vector3> { transform.position, transform.position };
    _rallyLine.SetPositions(positions.ToArray());
  }

  private void OnRallyPointSet (object source, Vector3 rallyPoint) {
    Debug.Log("Rally Point Set");
    _rallyPoint = rallyPoint;
    // Calculate the heading of the rally point (normalized) and add it to the start point
    // to get the new spawing point for this spawner
    _spawnPoint = _startPoint + (_rallyPoint - _startPoint).normalized;
    EventHub.TargetableRemoveTarget(GetComponent<Targetable>());
  }

  private void Destroy () {
    EventHub.OnSpawnTypeSelected -= OnSpawnTypeSelected;

    _targetable.OnTargeted -= OnTargeted;
    _targetable.OnTargetRemoved -= OnTargetRemoved;

    Controllable controllable = gameObject.GetComponent<Controllable>();
    if (controllable != null) controllable.OnRallyPointSet -= OnRallyPointSet;
  }


  [SerializeField] private EventHub.Players _player = EventHub.Players.Bot;

  [SerializeField] private Vector3 _startPoint;
  [SerializeField] private Vector3 _rallyPoint;
  [SerializeField] private Vector3 _spawnPoint;

  [SerializeField] private EventHub.AgentTypes _spawnType;
  [SerializeField] private float _spawnDelay = 1f;
  [SerializeField] private float _spawnCountdown = 0f;
  
  private bool _specificallyTargeted = false;

  private Targetable _targetable;
  private LineRenderer _rallyLine;
  private GameObject _spawnPrefab;
}
