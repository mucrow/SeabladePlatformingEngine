using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seablade.SLF {
  [RequireComponent(typeof(BoxCollider2D))]
  public class SLFController2D: MonoBehaviour {
    // TODO consider renaming to obstacle layer. i think only the Obstacle layer is assigned to
    // this LayerMask
    [SerializeField] LayerMask _collisionMask;

    const float _skinWidth = 0.015f;

    [SerializeField] int _horizontalRayCount = 4;
    [SerializeField] int _verticalRayCount = 4;

    float _horizontalRaySpacing;
    float _verticalRaySpacing;

    BoxCollider2D _collider;
    RaycastOrigins _raycastOrigins;

    void Start() {
      _collider = GetComponent<BoxCollider2D>();
      CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
      UpdateRaycastOrigins();

      // TODO for both of these if-blocks:
      // - int to float conversion in condition
      // - let's not use refs for that velocity argument
      // - should we really ignore collision checks when there's no input? what about moving walls and stuff?
      if (velocity.x != 0) {
        HorizontalCollisions(ref velocity);
      }
      if (velocity.y != 0) {
        VerticalCollisions(ref velocity);
      }

      // TODO rigidbody velocity instead
      transform.Translate(velocity);
    }

    // TODO refactor this to share logic with VerticalCollisions (HorizontalCollisions was
    // copy-pasted from VerticalCollisions)
    //
    // TODO let's not use refs
    void HorizontalCollisions(ref Vector3 velocity) {
      float directionX = Mathf.Sign(velocity.x);
      float rayLength = Mathf.Abs(velocity.x) + _skinWidth;

      for (int i = 0; i < _horizontalRayCount; ++i) {
        // TODO int to float conversion
        // TODO use directionX < 0 maybe
        Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
        rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);

        Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

        if (hit) {
          // move to contact position ?
          //
          // TODO i think this is still handling things as velocity, which works good for
          // translating the code to integrate with unity physics2D (it would be bad if moving into
          // contact position was handled by setting position directly because this is what causes
          // rigidbody2D to be unaware of collisions.
          //
          // worth noting that this code theoretically won't be needed since moving to contact
          // position should be handled by
          velocity.x = (hit.distance - _skinWidth) * directionX;
          // if we're raycasting to the right, and the player is facing some stairs to an upper
          // floor, the ray length should continually update to match the minimum hit distance so
          // the player "stops at the lower stair" on the staircase
          rayLength = hit.distance;
        }
      }
    }

    // TODO let's not use refs
    void VerticalCollisions(ref Vector3 velocity) {
      float directionY = Mathf.Sign(velocity.y);
      float rayLength = Mathf.Abs(velocity.y) + _skinWidth;

      for (int i = 0; i < _verticalRayCount; ++i) {
        // TODO int to float conversion
        // TODO use directionY < 0 maybe
        Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
        // TODO data flow issue
        //   SebLague: "we want to cast these rays from the point where we _will_ be on the X-axis"
        //   i should restructure this code so that this is clearer. we should probably be working
        //   with [temporarily?] translated ray origins in the first place ? not sure
        rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _collisionMask);

        Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

        if (hit) {
          // move to contact position ?
          //
          // TODO i think this is still handling things as velocity, which works good for
          // translating the code to integrate with unity physics2D (it would be bad if moving into
          // contact position was handled by setting position directly because this is what causes
          // rigidbody2D to be unaware of collisions.
          //
          // worth noting that this code theoretically won't be needed since moving to contact
          // position should be handled by
          velocity.y = (hit.distance - _skinWidth) * directionY;
          // if we're raycasting downward, and the player is standing on some stairs, the ray
          // length should continually update to match the minimum hit distance so the player
          // "lands on the higher stair" on the staircase
          rayLength = hit.distance;
        }
      }
    }

    void UpdateRaycastOrigins() {
      Bounds bounds = _collider.bounds;
      bounds.Expand(_skinWidth * -2f);

      _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
      _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
      _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
      _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing() {
      Bounds bounds = _collider.bounds;
      bounds.Expand(_skinWidth * -2f);

      _horizontalRayCount = Mathf.Clamp(_horizontalRayCount, 2, int.MaxValue);
      _verticalRayCount = Mathf.Clamp(_verticalRayCount, 2, int.MaxValue);

      _horizontalRaySpacing = bounds.size.y / (_horizontalRayCount - 1);
      _verticalRaySpacing = bounds.size.x / (_verticalRayCount - 1);
    }

    struct RaycastOrigins {
      public Vector2 TopLeft;
      public Vector2 TopRight;
      public Vector2 BottomLeft;
      public Vector2 BottomRight;
    }
  }
}
