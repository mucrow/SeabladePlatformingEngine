﻿using UnityEngine;

namespace Seablade.SLR {
  [RequireComponent(typeof(BoxCollider2D))]
  public class SLRController2D: MonoBehaviour {
    // TODO consider renaming to obstacle layer. i think only the Obstacle layer is assigned to
    // this LayerMask
    [SerializeField] LayerMask _collisionMask;

    const float _skinWidth = 0.015f;

    [SerializeField] int _horizontalRayCount = 4;
    [SerializeField] int _verticalRayCount = 4;

    float _maxClimbAngle = 80f;
    float _maxDescendAngle = 75f;

    float _horizontalRaySpacing;
    float _verticalRaySpacing;

    BoxCollider2D _collider;
    RaycastOrigins _raycastOrigins;
    // TODO i wish this was called CollisionInfo
    public CollisionInfo Collisions;

    void Start() {
      _collider = GetComponent<BoxCollider2D>();
      CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
      UpdateRaycastOrigins();
      Collisions.Reset();
      Collisions.VelocityOld = velocity;

      if (velocity.y < 0) {
        // TODO the position in code of DescendSlope relative to ClimbSlope is kinda weird? it's
        //      probably just cause we won't really be colliding by default when we should be
        //      descending
        DescendSlope(ref velocity);
      }
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
          // TODO only calculate slopeAngle if i == 0 maybe
          float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
          if (i == 0 && slopeAngle <= _maxClimbAngle) {
            if (Collisions.DescendingSlope) {
              Collisions.DescendingSlope = false;
              velocity = Collisions.VelocityOld;
            }
            float distanceToSlopeStart = 0f;
            // TODO i think epsilon comparison is a good idea here
            if (slopeAngle != Collisions.SlopeAngleOld) {
              distanceToSlopeStart = hit.distance - _skinWidth;
              // only climb slope with the velocity we'll have once we reach the slope
              velocity.x -= distanceToSlopeStart * directionX;
            }
            // TODO lets not use refs
            ClimbSlope(ref velocity, slopeAngle);
            // add the distance to the slope back in after we're done climbing
            velocity.x += distanceToSlopeStart * directionX;
          }

          // SebLague: "only check the remaining rays for collisions if we are not climbing a slope"
          // it is not clear what issue this fixes, if any
          if (!Collisions.ClimbingSlope || slopeAngle > _maxClimbAngle) {
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

            // fixes jitter when reaching a wall while climbing a slope
            if (Collisions.ClimbingSlope) {
              // SLR NOTE 00000: please see comments below
              // SebLague: "don't use slopeAngle here. it has to be Collisions.SlopeAngle"
              // TODO that's gross , what's the difference ? should slopeAngle even been in scope?
              // TODO SebLague makes it sound like ClimbSlope is ruining velocity.y and we're
              // repairing it here. maybe the solution is to have better seperation of cases in
              // this function
              velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
            }

            // TODO consider if-statement here instead
            Collisions.Left = directionX == -1;
            Collisions.Right = directionX == 1;
          }
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

          // fixes jitter when reaching a ceiling while climbing a slope
          if (Collisions.ClimbingSlope) {
            // see SLR NOTE 00000
            velocity.x = velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
          }

          // TODO consider if-statement here instead
          Collisions.Below = directionY == -1;
          Collisions.Above = directionY == 1;
        }
      }

      // Fixes getting stuck for 1 frame when slope angle changes
      if (Collisions.ClimbingSlope) {
        float directionX = Mathf.Sign(velocity.x);
        rayLength = Mathf.Abs(velocity.x) + _skinWidth;
        // TODO float comparison, also could do `directionX < 0f` here
        Vector2 rayOrigin = ((directionX == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight) + Vector2.up * velocity.y;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);

        if (hit) {
          float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
          // TODO float comparison
          if (slopeAngle != Collisions.SlopeAngle) {
            velocity.x = (hit.distance - _skinWidth) * directionX;
            Collisions.SlopeAngle = slopeAngle;
          }
        }
      }
    }

    void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
      float moveDistance = Mathf.Abs(velocity.x);
      float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

      // check that we're not jumping
      if (velocity.y <= climbVelocityY) {
        velocity.y = climbVelocityY;
        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
        // TODO also check if ClimbingSlope gets set properly on gentle slopes...
        // TODO oof so mutable (fixes "cannot jump on slopes" bug)
        Collisions.Below = true;
        Collisions.ClimbingSlope = true;
        Collisions.SlopeAngle = slopeAngle;
      }
    }

    void DescendSlope(ref Vector3 velocity) {
      float directionX = Mathf.Sign(velocity.x);
      // TODO float comparison, also could do `directionX < 0f` here
      Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;
      Debug.DrawRay(rayOrigin, directionX * Vector3.right * 100f, Color.red);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, _collisionMask);

      if (hit) {
        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        if (slopeAngle != 0 && slopeAngle <= _maxDescendAngle) {
          // if we're trying to move downward on the slope...
          if (Mathf.Sign(hit.normal.x) == directionX) {
            if (hit.distance - _skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
              float moveDistance = Mathf.Abs(velocity.x);
              float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
              velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
              velocity.y -= descendVelocityY;

              Collisions.SlopeAngle = slopeAngle;
              Collisions.DescendingSlope = true;
              Collisions.Below = true;
            }
          }
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

    // TODO these field names are pretty inconsistent
    public struct CollisionInfo {
      public bool Above;
      public bool Below;
      public bool Left;
      public bool Right;

      public bool ClimbingSlope;
      public bool DescendingSlope;
      public float SlopeAngle, SlopeAngleOld;
      public Vector3 VelocityOld;

      public void Reset() {
        Above = false;
        Below = false;
        Left = false;
        Right = false;
        ClimbingSlope = false;
        DescendingSlope = false;

        // TODO ugh...maybe this method should not be called Reset() but rather Update() or
        // FixedUpdate()
        SlopeAngleOld = SlopeAngle;
        SlopeAngle = 0f;
      }
    }
  }
}
