using UnityEngine;

public class EventHub : MonoBehaviour {

  public enum AgentTypes { None, Rock, Paper, Scissors }

  public delegate void AgentTypeSelectedDelegate(AgentTypes type);
  public static event AgentTypeSelectedDelegate OnAgentTypeSelected;
  public static void AgentTypeSelected (AgentTypes type) { OnAgentTypeSelected?.Invoke(type); }

  public delegate void AgentBuildingDelegate(AgentTypes type, float cooldown);
  public static event AgentBuildingDelegate OnAgentBuilding;
  public static void AgentBuilding (AgentTypes type, float cooldown) { OnAgentBuilding?.Invoke(type, cooldown); }

  public delegate void AgentCreatedDelegate(AgentBehaviour agent);
  public static event AgentCreatedDelegate OnAgentCreated;
  public static void AgentCreated (AgentBehaviour agent) { OnAgentCreated?.Invoke(agent); }

  public delegate void AgentDestroyedDelegate(AgentBehaviour agent);
  public static event AgentDestroyedDelegate OnAgentDestroyed;
  public static void AgentDestroyed (AgentBehaviour agent) { OnAgentDestroyed?.Invoke(agent); }

  public delegate void AgentSelectedDelegate(AgentBehaviour agent);
  public static event AgentSelectedDelegate OnAgentSelected;
  public static void AgentSelected (AgentBehaviour agent) { OnAgentSelected?.Invoke(agent); }

  public delegate void AgentDeSelectedDelegate(AgentBehaviour agent);
  public static event AgentDeSelectedDelegate OnAgentDeSelected;
  public static void AgentDeSelected (AgentBehaviour agent) { OnAgentDeSelected?.Invoke(agent); }
  
  public delegate void AgentRallyPointDelegate(AgentBehaviour agent, Vector3 point);
  public static event AgentRallyPointDelegate OnAgentRallyPoint;
  public static void AgentRallyPoint (AgentBehaviour agent, Vector3 point) { OnAgentRallyPoint?.Invoke(agent, point); }
}
