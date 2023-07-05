using UnityEngine;

namespace Seablade.DevUtils {
  public class DetectPhysics2DSimulationMode: MonoBehaviour {
    [SerializeField] SimulationMode2D _requiredSimulationMode;

    void Start() {
      if (Physics2D.simulationMode != _requiredSimulationMode) {
        Debug.LogError((
          "This object requires the Physics2D simulation mode to be set to \"" +
          _requiredSimulationMode.ToString() + "\" in the project settings. Destroying self."
        ));
        Destroy(gameObject);
      }
    }
  }
}
