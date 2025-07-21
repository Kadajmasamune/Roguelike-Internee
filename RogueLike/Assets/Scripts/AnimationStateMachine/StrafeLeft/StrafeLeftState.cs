using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/StrafeState")]
public class StrafeState : AnimState
{
    public enum StrafeDirection
    {
        Left,
        Right
    }

    public StrafeDirection strafeDirection;
    public float dotThreshold = 0.5f; // Slightly lower to account for isometric angles

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (!flags.IsLockedOn || flags.IsRolling || flags.IsAttacking)
            return false;

        if (velocity.magnitude < 0.01f)
            return false;

        Vector2 facingDir = DirectionToVector(entity.CurrentDirection);
        if (facingDir == Vector2.zero)
            return false;

        // Left = -perpendicular, Right = +perpendicular
        Vector2 strafeDir = strafeDirection == StrafeDirection.Left
            ? GetLeftVector(facingDir)
            : GetRightVector(facingDir);

        float dot = Vector2.Dot(velocity.normalized, strafeDir);
        return dot >= dotThreshold;
    }

    private Vector2 DirectionToVector(Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Vector2(0, 1),
            Direction.Down => new Vector2(0, -1),
            Direction.Left => new Vector2(-1, 0),
            Direction.Right => new Vector2(1, 0),
            Direction.UpLeft => new Vector2(-1, 1).normalized,
            Direction.UpRight => new Vector2(1, 1).normalized,
            Direction.BottomLeft => new Vector2(-1, -1).normalized,
            Direction.BottomRight => new Vector2(1, -1).normalized,
            _ => Vector2.zero
        };
    }

    private Vector2 GetLeftVector(Vector2 forward)
    {
        // 90-degree counterclockwise rotation in 2D
        return new Vector2(-forward.y, forward.x).normalized;
    }

    private Vector2 GetRightVector(Vector2 forward)
    {
        // 90-degree clockwise rotation in 2D
        return new Vector2(forward.y, -forward.x).normalized;
    }
}
