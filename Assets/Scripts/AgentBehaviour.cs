using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour {

  public Vector3 RallyPoint;

  private void Awake () {
    EventHub.OnAgentSelected += OnAgentSelected;
    EventHub.OnAgentDeSelected += OnAgentDeSelected;
    EventHub.OnAgentRallyPoint += OnAgentRallyPoint;
  }

  private void Start () {
    _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    _selectable = gameObject.GetComponent<SelectableBehaviour>();
    _camera = Camera.main;

    EventHub.AgentCreated(this);
  }

  private void Update () {
     MoveToRallyPoint();
  }

  private void OnAgentSelected (AgentBehaviour agent) {
    if (agent != this) return;
    // Debug.Log($"Agent Selected: {agent}");
    _selectable.Select();
  }

  private void OnAgentDeSelected (AgentBehaviour agent) {
    if (agent != this) return;
    // Debug.Log($"Agent DeSelected: {agent}");
    _selectable.DeSelect();
  }  

  private void OnAgentRallyPoint (AgentBehaviour agent, Vector3 point) {
    if (agent != this) return;
    Debug.Log($"Agent Rally: {agent} {point}");
    RallyPoint = point;
    OnAgentDeSelected(agent);
  }

  private void MoveToRallyPoint () {
    // If we're offscren, we need to move back onscreen. 
    if (IsOffScreen(_camera, transform.position)) {
      // Debug.Log($"Offscreen, Returning Last Position: ${_lastOnScreenPosition}");
      _navMeshAgent.destination = _lastOnScreenPosition;
    } 
    else {
      // Debug.Log($"Onscreen, Returning Rally Position: ${RallyPoint}");
      _lastOnScreenPosition = transform.position;
      _navMeshAgent.destination = RallyPoint;
    }
  }

  private static bool IsOffScreen (Camera camera, Vector3 position) {
    // Calculate whether or not the agents rotation point is "off screen".
    // Most of the time we should be OK if part of the agent is offscreen.
    // If not, we'll need to adjust this for the size of the mesh.
    Vector3 screenPosition = camera.WorldToScreenPoint(position);
    return Screen.safeArea.Contains(screenPosition) == false;
  }

  private static void ChangeLuminosity (AgentBehaviour agent, float offset) {
    Color original = agent.GetComponent<MeshRenderer>().material.color;
    
    float H, S, V;
    Color.RGBToHSV(original, out H, out S, out V);
    agent.GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(H, S, V + offset);
  }

  private void OnDestroy () {
    EventHub.OnAgentSelected -= OnAgentSelected;
    EventHub.OnAgentDeSelected -= OnAgentDeSelected;
    EventHub.AgentDestroyed(this);
  }


  private Vector3 _lastOnScreenPosition;
  private SelectableBehaviour _selectable;
  private UnityEngine.AI.NavMeshAgent _navMeshAgent;
  private Camera _camera;
}
