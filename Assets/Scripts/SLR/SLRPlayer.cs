using Seablade.SLF;
using UnityEngine;

namespace Seablade.SLR {
  [RequireComponent(typeof(SLRController2D))]
  public class SLRPlayer: MonoBehaviour {
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _moveSpeed = 6f;

    SLRController2D _controller;
    Vector3 _velocity;

    void Start() {
      _controller = GetComponent<SLRController2D>();
    }

    void Update() {
      Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

      _velocity.x = input.x * _moveSpeed;
      _velocity.y += _gravity * Time.deltaTime;
      _controller.Move(_velocity * Time.deltaTime);
    }
  }
}
