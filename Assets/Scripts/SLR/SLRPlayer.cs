using Seablade.SLF;
using UnityEngine;

namespace Seablade.SLR {
  [RequireComponent(typeof(SLRController2D))]
  public class SLRPlayer: MonoBehaviour {
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _moveSpeed = 6f;

    Vector3 _velocity;

    SLRController2D _controller;

    void Start() {
      _controller = GetComponent<SLRController2D>();
    }

    void FixedUpdate() {
      if (_controller.Collisions.Above || _controller.Collisions.Below) {
        _velocity.y = 0;
      }

      Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

      _velocity.x = input.x * _moveSpeed;
      _velocity.y += _gravity * Time.fixedDeltaTime;
      // TODO pull Time.fixedDeltaTime into a variable (below is the second usage)
      _controller.Move(_velocity * Time.fixedDeltaTime);
    }
  }
}
