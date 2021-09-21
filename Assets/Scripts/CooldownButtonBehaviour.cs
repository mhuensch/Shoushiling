using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownButtonBehaviour : MonoBehaviour {
  private void Awake () {
    _button = this.GetComponent<Button>();
    _button.onClick.AddListener(OnButtonClicked);
    EventHub.OnSpawning += OnSpawning;
  }

  private void OnButtonClicked () {
    EventHub.SpawnTypeSelected(_agentType);
  }

  private void OnSpawning (EventHub.AgentTypes type, float cooldown) {
    _button.interactable = false;
    _cooldown = cooldown;
    _cooldownImage.fillAmount = 0;
  }

  private void Update () {
    _button.interactable = _cooldownImage.fillAmount == 1;
    _cooldownImage.fillAmount += 1 / _cooldown * Time.deltaTime;
  }

  private void Destroy () {
    _button.onClick.RemoveListener(OnButtonClicked);
    EventHub.OnSpawning -= OnSpawning;
  }

  [SerializeField] private Image _cooldownImage;
  [SerializeField] private EventHub.AgentTypes _agentType;
  
  private float _cooldown;
  private Button _button;
  
}
