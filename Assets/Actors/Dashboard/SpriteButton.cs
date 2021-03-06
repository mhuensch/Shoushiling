using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpriteButton : MonoBehaviour {
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

    Addressables.LoadAssetAsync<Sprite[]>($"{_agentType} Icon").Completed += OnSpritesLoaded;
  }

  private void OnSpritesLoaded (AsyncOperationHandle<Sprite[]> obj) {
    _sprites = obj.Result;
  }

  private void OnAgentBuilding (EventHub.AgentTypes type, float cooldown) {
    _button.interactable = false;
  }

	public Sprite[] sprites;
	public float spritePerFrame = 1f;
	public bool loop = true;
	public bool destroyOnEnd = false;

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
  private Button _button;

}
