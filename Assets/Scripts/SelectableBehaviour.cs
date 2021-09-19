using UnityEngine;

public class SelectableBehaviour : MonoBehaviour {

  private void Start () {
    DeSelect();
  }

  public void Toggle () {
    if (_selected) {
      Select();
    }
    else {
      DeSelect();
    }
  }

  public void Select () {
    ChangeLuminosity(this.gameObject, 0.5f);
    _selected = true;
  }

  public void DeSelect () {
    ChangeLuminosity(this.gameObject, -0.5f);
    _selected = false;
  }

  public bool IsSelected () {
    return _selected;
  }


  private static void ChangeLuminosity (GameObject obj, float offset) {
    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
    if (renderer == null) return;

    Color start = renderer.material.color;
    
    float H, S, V;
    Color.RGBToHSV(start, out H, out S, out V);
    renderer.material.color = Color.HSVToRGB(H, S, V + offset);
  }

  private bool _selected = false;
}
