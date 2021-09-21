using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Targetable))]
public class Outlineable : MonoBehaviour {
  // HACK: I'm not sure what's the best way to apply an outline to game objects at runtime.
  // This works for now, but it requires the outline shader to be set as the default on the prefab,
  // and assumes you're OK with the unhighlighted shader to be the default URP lit shader.

  private void Awake () {
    _targetable = GetComponent<Targetable>();
    _targetable.OnTargeted += OnTargeted;
    _targetable.OnTargetRemoved += OnTargetRemoved;
  }

  private void Start () {
    _material = GetComponentsInChildren<Renderer>()
      .Select(renderer => renderer.material)
      .FirstOrDefault(material => material != null);

    if (_material == null) Debug.LogError("No default outline material found.");

    _originalOutlineShader = _material.shader;
    _shaderWithoutOutline = Shader.Find("Universal Render Pipeline/Lit");
    
    _material.shader = _shaderWithoutOutline;
  }

  private void OnTargeted (Vector3 point) {
    if (_material == null) return;

    _material.shader = _originalOutlineShader;
  }

  private void OnTargetRemoved () {
    if (_material == null) return;

    _material.shader = _shaderWithoutOutline;
  }

  private void OnDestroy () {
    _targetable.OnTargeted -= OnTargeted;
    _targetable.OnTargetRemoved -= OnTargetRemoved;

  }

  private Shader _shaderWithoutOutline;
  private Shader _originalOutlineShader;

  private Targetable _targetable;
  private Material _material;
}