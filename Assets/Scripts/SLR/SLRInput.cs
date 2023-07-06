using UnityEngine;

namespace Seablade.SLR {
  public class SLRInput {
    public Key Jump = new Key(KeyCode.Space);
    public Key MoveLeft = new Key(KeyCode.A);
    public Key MoveRight = new Key(KeyCode.D);
    public Key MoveDown = new Key(KeyCode.S);
    public Key MoveUp = new Key(KeyCode.W);

    public void Poll() {
      Jump.Poll();
      MoveLeft.Poll();
      MoveRight.Poll();
      MoveDown.Poll();
      MoveUp.Poll();
    }

    public void FinishFrame() {
      Jump.FinishFrame();
      MoveLeft.FinishFrame();
      MoveRight.FinishFrame();
      MoveDown.FinishFrame();
      MoveUp.FinishFrame();
    }

    public float GetAxisRawHorizontal() {
      return GetAxisRaw(MoveLeft, MoveRight);
    }

    public float GetAxisRawVertical() {
      return GetAxisRaw(MoveDown, MoveUp);
    }

    float GetAxisRaw(Key negative, Key positive) {
      float ret = 0f;
      if (negative.State) {
        ret -= 1f;
      }
      if (positive.State) {
        ret += 1f;
      }
      return ret;
    }

    public struct Key {
      KeyCode _keyCode;

      public bool State;
      public bool Down;
      public bool Up;

      public Key(KeyCode keyCode) {
        _keyCode = keyCode;
        State = false;
        Down = false;
        Up = false;
      }

      public void Poll() {
        State = Input.GetKey(_keyCode);
        if (Input.GetKeyDown(_keyCode)) {
          Down = true;
        }
        if (Input.GetKeyUp(_keyCode)) {
          Up = true;
        }
      }

      public void FinishFrame() {
        Down = false;
        Up = false;
      }
    }
  }
}
