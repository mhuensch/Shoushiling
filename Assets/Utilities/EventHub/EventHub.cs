using UnityEngine;

public class EventHub {

  public enum AgentTypes { None, Rock, Paper, Scissors, Bomb, Scout }
  public enum Players { Human, Bot } // TODO: dynamically determine this based on who's playing

  // Global Notifications for Targetable Game Objects
  public delegate void TargetableCreatedDelegate(Targetable target);
  public static event TargetableCreatedDelegate OnTargetableCreated;
  public static void TargetableCreated (Targetable target) { OnTargetableCreated?.Invoke(target); }

  public delegate void TargetableDestroyedDelegate(Targetable targetable);
  public static event TargetableDestroyedDelegate OnTargetableDestroyed;
  public static void TargetableDestroyed (Targetable targetable) { OnTargetableDestroyed?.Invoke(targetable); }

  public delegate void TargetableTargetedDelegate(Targetable target, Vector3 point);
  public static event TargetableTargetedDelegate OnTargetableTargeted;
  public static void TargetableTargeted (Targetable target, Vector3 point) { OnTargetableTargeted?.Invoke(target, point); }

  public delegate void TargetableRemoveTargetDelegate(Targetable target);
  public static event TargetableRemoveTargetDelegate OnTargetableRemoveTarget;
  public static void TargetableRemoveTarget (Targetable target) { OnTargetableRemoveTarget?.Invoke(target); }


  // Global Notifications for Spawner Game Objects
  public delegate void SpawnTypeSelectedDelegate(AgentTypes type);
  public static event SpawnTypeSelectedDelegate OnSpawnTypeSelected;
  public static void SpawnTypeSelected (AgentTypes type) { OnSpawnTypeSelected?.Invoke(type); }

  public delegate void SpawningDelegate(AgentTypes type, float cooldown);
  public static event SpawningDelegate OnSpawning;
  public static void Spawning (AgentTypes type, float cooldown) { OnSpawning?.Invoke(type, cooldown); }

  public delegate void SpawnedDelegate(Spawnable spawn, Vector3 rallyPoint);
  public static event SpawnedDelegate OnSpawned;
  public static void Spawned (Spawnable spawn, Vector3 rallyPoint) { OnSpawned?.Invoke(spawn, rallyPoint); }


  // Global Notifications for Stage Game Object
  public delegate void StageSelectedDelegate(Vector3 point);
  public static event StageSelectedDelegate OnStageSelected;
  public static void StageSelected (Vector3 point) { OnStageSelected?.Invoke(point); }

  public delegate void StageDragStartedDelegate(Vector3 point);
  public static event StageDragStartedDelegate OnStageDragStarted;
  public static void StageDragStarted (Vector3 point) { OnStageDragStarted?.Invoke(point); }

  public delegate void StageDragUpdatedDelegate(Vector3 point);
  public static event StageDragUpdatedDelegate OnStageDragUpdated;
  public static void StageDragUpdated (Vector3 point) { OnStageDragUpdated?.Invoke(point); }

  public delegate void StageDragStoppedDelegate(Vector3 point);
  public static event StageDragStoppedDelegate OnStageDragStopped;
  public static void StageDragStopped (Vector3 point) { OnStageDragStopped?.Invoke(point); }

}
