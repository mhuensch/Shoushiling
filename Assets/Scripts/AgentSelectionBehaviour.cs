using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentSelectionBehaviour : MonoBehaviour {

  public void Awake () {
    EventHub.OnAgentCreated += OnAgentCreated;
    EventHub.OnAgentDestroyed += OnAgentDestroyed;
  }

  public void Start () {
    _camera = Camera.main;
    dragSelectionImage.transform.gameObject.SetActive(true);
    DrawDragSelect();
  }

  private void OnAgentCreated (AgentBehaviour agent) {
    _agents.Add(agent);
  }

  private void OnAgentDestroyed (AgentBehaviour agent) {
    _agents.Remove(agent);
  }

  private void Update () {
    // At first, it might seem like you want these methods to be part of FixedUpdate.
    // However, because this involves updates to the UI, we'll put them here in the normal update method.
    UpdateClickSelect();
    UpdateDragSelect();
  }

  private void UpdateClickSelect () {
    // If the left mouse button isn't down, we don't want to process anything
    if (Input.GetMouseButtonDown(0) == false) return;

    // Find out where the mouse clicked by casting a ray.
    // NOTE: this is much more reliable than puting an OnMouseClick event handler
    // on a collider (which is intermittenyly buggy)
    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

    // Raycast to determine if the mouse is on an agent.
    RaycastHit hitData;
    if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _agentLayer)) {
      // Confirm that the object we hit has an agent behaviour.
      AgentBehaviour agent = hitData.transform.gameObject.GetComponent<AgentBehaviour>();
      if (agent == null) {
        Debug.LogWarning("An item was clicked on the agent layer that does not implement the Agent Behaviour.");
        return;
      }

      ToggleAgentSelect(agent);
    }
    else if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _stageLayer)) {
      if (_selected.Count <= 0) return;

      _selected.ForEach(agent => EventHub.AgentRallyPoint(agent, hitData.point));
      _selected.Clear();
    }
  }

  private void UpdateDragSelect () {
    if (Input.GetMouseButtonDown(0)) {
      _dragStartPosition = Input.mousePosition;
      _selectionBox = new Rect();
    }
    
    if (Input.GetMouseButton(0)) {
      _dragEndPosition = Input.mousePosition;
      DrawDragSelect();
      DrawSelection();
    }

    if (Input.GetMouseButtonUp(0)) {
      SelectUnits();

      // Clear the draw box.
      _dragStartPosition = Vector2.zero;
      _dragEndPosition = Vector2.zero;
      DrawDragSelect();
    }
  }

  private void DrawDragSelect () {
    Vector2 boxStart = _dragStartPosition;
    Vector2 boxEnd = _dragEndPosition;
    Vector2 boxCenter = (boxStart + boxEnd) / 2;

    dragSelectionImage.position = boxCenter;

    Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
    dragSelectionImage.sizeDelta = boxSize; 
  }


  void DrawSelection () {
    // To start we'll assume a left to right, top to bottom drag
    _selectionBox.xMin = _dragStartPosition.x;
    _selectionBox.xMax = Input.mousePosition.x;
    _selectionBox.yMin = Input.mousePosition.y;
    _selectionBox.yMax = _dragStartPosition.y;

    // Then we'll modify accordingly for other use cases, like draging right to left ...
    if (Input.mousePosition.x < _dragStartPosition.x) {
      _selectionBox.xMin = Input.mousePosition.x;
      _selectionBox.xMax = _dragStartPosition.x;
    }

    // Or dragging bottom to top.
    if (Input.mousePosition.y  > _dragStartPosition.y) {
      _selectionBox.yMin = _dragStartPosition.y;
      _selectionBox.yMax = Input.mousePosition.y;
    }
  }

  void SelectUnits () {
    _agents.ForEach(agent => {
      Vector3 agentScreenPoint = _camera.WorldToScreenPoint(agent.transform.position);
      if (_selectionBox.Contains(agentScreenPoint) == false) return;
      ToggleAgentSelect(agent);
    });
  }

  private void ToggleAgentSelect (AgentBehaviour agent) {
    // Toggle the add or remove of the agent as needed.
    if (_selected.Contains(agent)) {
      DeselectAgent(agent);
    }
    else {
      SelectAgent(agent);
    }
  }

  private void SelectAgent (AgentBehaviour agent) {
    Debug.Log($"Adding: {agent}");
    _selected.Add(agent);
    EventHub.AgentSelected(agent);
  }

  private void DeselectAgent (AgentBehaviour agent) {
    Debug.Log($"Removing: {agent}");
    _selected.Remove(agent);
    EventHub.AgentDeSelected(agent);
  }

  private void OnDestroy () {
    // Remove any event hub listeners as needed.
    EventHub.OnAgentCreated -= OnAgentCreated;
    EventHub.OnAgentDestroyed -= OnAgentDestroyed;
  }


  [SerializeField] RectTransform dragSelectionImage;
  private Vector2 _dragStartPosition = Vector2.zero;
  private Vector2 _dragEndPosition = Vector2.zero;
  private Rect _selectionBox = new Rect();

  private Camera _camera;
  private List<AgentBehaviour> _agents = new List<AgentBehaviour>();
  private List<AgentBehaviour> _selected = new List<AgentBehaviour>();
  

  // Adds a layer mask for layer 7 (i.e. the Agent Layer)
  private static LayerMask _agentLayer = 1 << 7;
  // Adds a layer mask for layer 8 (i.e. the Stage Layer)
  private static LayerMask _stageLayer = 1 << 8;
}
