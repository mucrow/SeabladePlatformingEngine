using UnityEngine;

namespace Seablade.SLR {
  [RequireComponent(typeof(SLRController2D))]
  public class SLRPlayer: MonoBehaviour {
    // See TRANSLATION NOTE 00000
    SLRInput _slrInput = new SLRInput();

    [SerializeField] float _jumpHeight = 4f;
    [SerializeField] float _timeToJumpApex = 0.4f;
    float _accelerationTimeAirborne = 0.2f;
    float _accelerationTimeGrounded = 0.1f;
    [SerializeField] float _moveSpeed = 6f;

    float _gravity;
    float _jumpVelocity;
    Vector3 _velocity;
    float _velocityXSmoothing;

    SLRController2D _controller;

    void Start() {
      _controller = GetComponent<SLRController2D>();

      // TODO pull this out into a named function. calculating gravity and jump velocity from jump
      // height and time to jump apex
      _gravity = (-2f * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
      _jumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
      Debug.Log("Gravity: " + _gravity + " Jump Velocity: " + _jumpVelocity);
    }

    void FixedUpdate() {
      if (_controller.Collisions.Above || _controller.Collisions.Below) {
        _velocity.y = 0;
      }

      // See TRANSLATION NOTE 00000
      Vector2 input = new Vector2(_slrInput.GetAxisRawHorizontal(), _slrInput.GetAxisRawVertical());

      if (_slrInput.Jump.Down && _controller.Collisions.Below) {
        _velocity.y = _jumpVelocity;
      }

      float targetVelocityX = input.x * _moveSpeed;
      _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, _controller.Collisions.Below ? _accelerationTimeGrounded : _accelerationTimeAirborne);
      _velocity.y += _gravity * Time.fixedDeltaTime;
      // TODO pull Time.fixedDeltaTime into a variable (below is the second usage)
      _controller.Move(_velocity * Time.fixedDeltaTime);

      // See TRANSLATION NOTE 00000
      _slrInput.FinishFrame();
    }

    //---------------------------------------------------------------------------------------------
    // TRANSLATION NOTE 00001 (SLF -> SLR): every method below this comment only exists in SLR.
    //---------------------------------------------------------------------------------------------

    // TRANSLATION NOTE 00000 (SLF -> SLR): because SLF runs its logic in Update as opposed to
    // FixedUpdate, it can safely poll input directly within its platforming logic. it is necessary
    // to implement a level of input buffering for SLR (even if it is just a buffer of 1 frame).
    void Update() {
      _slrInput.Poll();
    }
  }
}
