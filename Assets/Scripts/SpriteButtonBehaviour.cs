using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteButtonBehaviour : MonoBehaviour {
  [SerializeField] private Image _spriteImage;
  private Sprite[] _sprites;
  public int _frameRate = 30;
  private float  _timePerFrame = 0f;
  private float  _elapsedTime = 0f;
  private int _currentFrame = 0;

  private void Start () {
    _button = this.GetComponent<Button>();
    _spriteImage = this.GetComponent<Image>();

    _timePerFrame = 1f / _frameRate;

    _spritesByType = new Dictionary<EventHub.AgentTypes, string> {
      { EventHub.AgentTypes.Paper, "Wind Icon" },
      { EventHub.AgentTypes.Scissors, "Fire Icon" },
      { EventHub.AgentTypes.Rock, "Ice Icon" },
      { EventHub.AgentTypes.Bomb, "Bomb Icon" },
      { EventHub.AgentTypes.Scout, "Angel Icon" }
    };

    string resourceName = _spritesByType[_agentType];
    Debug.Log(resourceName);
    _sprites = Resources.LoadAll<Sprite>(resourceName);
    Debug.Log($"Sprite Count: {_sprites.Length}");

  }

  private void OnAgentBuilding (EventHub.AgentTypes type, float cooldown) {
    _button.interactable = false;
  }

	public Sprite[] sprites;
	public float spritePerFrame = 1f;
	public bool loop = true;
	public bool destroyOnEnd = false;

	private int index = 0;
	private float frame = 0f;

  private void Update () {
    _elapsedTime  += Time.deltaTime;
    if (_elapsedTime >= _timePerFrame) {
      
      _elapsedTime = 0f;
      ++_currentFrame;
      SetSprite();

      if (_currentFrame >= _sprites.Length) {
        _currentFrame = 0;
      }

    }
  }

  private void SetSprite () {
    if (_currentFrame >= 0 && _currentFrame < _sprites.Length) {
      _spriteImage.sprite = _sprites[_currentFrame];
    }
  }

  [SerializeField] private EventHub.AgentTypes _agentType;
  
  private Dictionary<EventHub.AgentTypes, string> _spritesByType;
  private Button _button;
  
}
