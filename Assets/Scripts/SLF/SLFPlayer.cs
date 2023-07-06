using UnityEngine;

namespace Seablade.SLF {
  [RequireComponent(typeof(SLFController2D))]
  public class SLFPlayer: MonoBehaviour {
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _moveSpeed = 6f;

    SLFController2D _controller;
    Vector3 _velocity;

    void Start() {
      _controller = GetComponent<SLFController2D>();
    }

    void Update() {
      if (_controller.Collisions.Above || _controller.Collisions.Below) {
        _velocity.y = 0;
      }

      Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

      _velocity.x = input.x * _moveSpeed;
      _velocity.y += _gravity * Time.deltaTime;
      // TODO this (and perhaps other parts of Update()) should be in FixedUpdate()
      // TODO pull Time.deltaTime into a variable (below is the second usage)
      _controller.Move(_velocity * Time.deltaTime);
    }
  }
}
