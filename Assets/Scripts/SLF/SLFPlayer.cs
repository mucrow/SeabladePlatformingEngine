using UnityEngine;

namespace Seablade.SLF {
  [RequireComponent(typeof(SLFController2D))]
  public class SLFPlayer: MonoBehaviour {
    [SerializeField] float _jumpHeight = 4f;
    [SerializeField] float _timeToJumpApex = 0.4f;
    float _accelerationTimeAirborne = 0.2f;
    float _accelerationTimeGrounded = 0.1f;
    [SerializeField] float _moveSpeed = 6f;

    float _gravity;
    float _jumpVelocity;
    Vector3 _velocity;
    float _velocityXSmoothing;

    SLFController2D _controller;

    void Start() {
      _controller = GetComponent<SLFController2D>();

      // TODO pull this out into a named function. calculating gravity and jump velocity from jump
      // height and time to jump apex
      _gravity = (-2f * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
      _jumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
      Debug.Log("Gravity: " + _gravity + " Jump Velocity: " + _jumpVelocity);
    }

    void Update() {
      if (_controller.Collisions.Above || _controller.Collisions.Below) {
        _velocity.y = 0;
      }

      Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

      if (Input.GetKeyDown(KeyCode.Space) && _controller.Collisions.Below) {
        _velocity.y = _jumpVelocity;
      }

      float targetVelocityX = input.x * _moveSpeed;
      _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, _controller.Collisions.Below ? _accelerationTimeGrounded : _accelerationTimeAirborne);
      _velocity.y += _gravity * Time.deltaTime;
      // TODO this (and perhaps other parts of Update()) should be in FixedUpdate()
      // TODO pull Time.deltaTime into a variable (below is the second usage)
      _controller.Move(_velocity * Time.deltaTime);
    }
  }
}
