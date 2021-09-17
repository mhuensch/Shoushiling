using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;

public class FactoryBehaviour : MonoBehaviour {

  public enum UnitTypes { None, Rock, Paper, Scissors }

  public GameObject PaperPrefab;
  public GameObject ScissorPrefab;
  public GameObject RockPrefab;

  public Button ScissorsButton;
  public Button PaperButton;
  public Button RockButton;

  public Vector3 RallyPoint;
  public GameObject Background;

  private void Start () {
    _camera = Camera.main;
    _selectable = GetComponent<SelectableBehaviour>();

    _prefabsByType = new Dictionary<UnitTypes, GameObject> {
      { UnitTypes.Paper, PaperPrefab },
      { UnitTypes.Scissors, ScissorPrefab },
      { UnitTypes.Rock, RockPrefab }
    };

    _rallyLine = this.gameObject.AddComponent<LineRenderer>();
    _rallyLine.material = GetComponent<MeshRenderer>().material;
    _rallyLine.material.renderQueue = 0;
    _rallyLine.startWidth = 0.2f;
    _rallyLine.endWidth = 0.2f;
    _rallyLine.startColor = Color.red;
    _rallyLine.endColor = Color.red;
    _rallyLine.useWorldSpace = true;

    ScissorsButton.onClick.AddListener(OnScissorsSelected);
    PaperButton.onClick.AddListener(OnPaperSelected);
    RockButton.onClick.AddListener(OnRockSelected);
  }

  private void FixedUpdate () {
    SpawnUnit();
  }

  private void Update () {
        // If the left mouse button isn't down, we don't want to process anything
    if (Input.GetMouseButtonDown(0) == false) return;

    // Find out where the mouse clicked by casting a ray.
    // NOTE: this is much more reliable than puting an OnMouseClick event handler
    // on a collider (which is intermittenyly buggy)
    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

     // Raycast to determine if the mouse is on an factory.
    RaycastHit hitData;
    if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _factoryLayer)) {
      if (hitData.transform.gameObject != this.gameObject) return;

      List<Vector3> pos = new List<Vector3> {
        this.transform.position
      };

      if (_selectable.IsSelected()) {
        Debug.Log("No Rally Point");
        _selectable.DeSelect();
        pos.Add(this.transform.position);
      } else {
        _selectable.Select();
        pos.Add(RallyPoint);
      }

      _rallyLine.SetPositions(pos.ToArray());
    } 
    else if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _stageLayer)) {
      if (_selectable.IsSelected() == false) return;

      Debug.Log("Rally Point Selected");
      _selectable.DeSelect();
      RallyPoint = hitData.point;
      _rallyLine.SetPositions(new Vector3[] { this.transform.position, RallyPoint });
      Invoke("HideRallyLine", 0.5f);
    }
  }

  private void HideRallyLine () {
    _rallyLine.SetPositions(new Vector3[] { this.transform.position, this.transform.position });
  }

  private void SpawnUnit () {
    if (_unitType == UnitTypes.None) return;

    _spawnTime += Time.deltaTime;
    if (_spawnTime < _spawnDelay) return;

    GameObject currentPrefab = _prefabsByType[_unitType];
    
    Vector3 headingDirection = (this.RallyPoint - this.transform.position).normalized;
    Vector3 spawnAt = this.transform.position + headingDirection;
    GameObject agent = Instantiate(currentPrefab, spawnAt, Quaternion.identity);

    // Debug.Log("Rally Point: " + this.RallyPoint);
    agent.GetComponent<AgentBehaviour>().RallyPoint = this.RallyPoint;

    _spawnTime -= _spawnDelay;
    _unitType = UnitTypes.None;
  }

  private void OnScissorsSelected ()  {
    _unitType = UnitTypes.Scissors;
  }

  private void OnPaperSelected ()  {
    _unitType = UnitTypes.Paper;
  }

  private void OnRockSelected ()  {
    _unitType = UnitTypes.Rock;
  }

  private Camera _camera;
  private SelectableBehaviour _selectable;
  private Dictionary<UnitTypes, GameObject> _iconsByType;
  private Dictionary<UnitTypes, GameObject> _prefabsByType;
  private LineRenderer _rallyLine;

  private float _spawnDelay = 1f;
  private float _spawnTime = 0f;

  // Adds a layer mask for layer 6 (i.e. the Factory Layer)
  private static LayerMask _factoryLayer = 1 << 6;
    // Adds a layer mask for layer 8 (i.e. the Stage Layer)
  private static LayerMask _stageLayer = 1 << 8;

  [SerializeField] // Makes field visible to Unity Editor.
  private UnitTypes _unitType;

  
}
