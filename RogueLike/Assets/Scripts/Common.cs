using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Common
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpRight,
        BottomRight,
        UpLeft,
        BottomLeft,
    }


    public interface IHasDirection
    {
        Direction CurrentDirection { get; }
    }

    public interface IHasVelocity
    {
        Vector2 CurrentVelocity { get; }
    }

    public interface IHasBooleans
    {
        bool IsRolling { get; }
        bool IsRunning { get; }
        bool IsAttacking { get; }
        bool IsLockedOn { get; }
        bool IsCasting { get; }
        bool IsHeavyAttacking { get; }
    }

    public class AnimHashGenerator
    {
        public void GenerateAnimHash(Dictionary<string, int> AnimationClipHashes, AnimatorController AnimController)
        {
            AnimationClipHashes.Clear();
            var clips = AnimController.animationClips;

            foreach (var clip in clips)
            {
                AnimationClipHashes[clip.name] = Animator.StringToHash(clip.name);
            }
        }
    }

    public class EntityMovement
    {
        public Direction GetDirectionFromInput(float x, float y)
        {
            if (x == 0 && y == 0) return Direction.None;
            if (x > 0 && y == 0) return Direction.Right;
            if (x < 0 && y == 0) return Direction.Left;
            if (x == 0 && y > 0) return Direction.Up;
            if (x == 0 && y < 0) return Direction.Down;
            if (x > 0 && y > 0) return Direction.UpRight;
            if (x < 0 && y > 0) return Direction.UpLeft;
            if (x > 0 && y < 0) return Direction.BottomRight;
            if (x < 0 && y < 0) return Direction.BottomLeft;

            return Direction.None;
        }

        public Vector2 DirectionToVector(Direction dir)
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
    }
}
