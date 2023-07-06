using UnityEngine;
using UnityEngine.Serialization;

namespace Seablade.DevUtils {
  public class DetectProjectSettings: MonoBehaviour {
    [SerializeField] bool _physics2DAutoSyncTransforms;
    [SerializeField] SimulationMode2D _physics2DSimulationMode;

    void Start() {
      if (Physics2D.autoSyncTransforms != _physics2DAutoSyncTransforms) {
        HandleConfigProblem(
          "This object requires the Physics2D auto sync transforms flag to be " +
          formatFlag(_physics2DAutoSyncTransforms) + " in the project settings. Destroying self."
        );
      }
      else if (Physics2D.simulationMode != _physics2DSimulationMode) {
        HandleConfigProblem(
          "This object requires the Physics2D simulation mode to be set to \"" +
          _physics2DSimulationMode.ToString() + "\" in the project settings. Destroying self."
        );
      }
    }

    void HandleConfigProblem(string message) {
      Debug.LogError(message);
      Destroy(gameObject);
    }

    string formatFlag(bool value) {
      return value ? "enabled" : "disabled";
    }
  }
}
