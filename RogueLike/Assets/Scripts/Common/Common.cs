using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.UIElements;




#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

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


    public delegate IEnumerator WaitForClickAndCast(GameObject spellPrefab, int SpellManaCost, AudioClip SpellPfSFX);

    // Debug.Log("Waiting for target click...");

    // yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

    // Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    // clickPos.z = 0f;

    // if (spellPrefab != null && CurrentMana > 0f && CurrentMana >= SpellManaCost)
    // {
    //     CurrentMana -= SpellManaCost;

    //     RectTransform manaRect = ManaBar.GetComponent<RectTransform>();
    //     manaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentMana);

    //     ManaFloat.text = $"{CurrentMana.ToString("F0")}MP";

    //     GameObject spell = Instantiate(spellPrefab, clickPos, quaternion.identity);
    //     sFXManager.PlaySFX(SpellPfSFX);
    //     Destroy(spell, 0.8f);
    // }
    // else
    // {
    //     sFXManager.PlaySFX(sFXManager.Denied);
    //     Debug.Log("Not enough Mana!");
    //     yield return null;
    // }

    // EndCasting();


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
            const float epsilon = 0.1f;

            if (Mathf.Abs(x) < epsilon && Mathf.Abs(y) < epsilon) return Direction.None;
            if (x > epsilon && Mathf.Abs(y) < epsilon) return Direction.Right;
            if (x < -epsilon && Mathf.Abs(y) < epsilon) return Direction.Left;
            if (Mathf.Abs(x) < epsilon && y > epsilon) return Direction.Up;
            if (Mathf.Abs(x) < epsilon && y < -epsilon) return Direction.Down;
            if (x > epsilon && y > epsilon) return Direction.UpRight;
            if (x < -epsilon && y > epsilon) return Direction.UpLeft;
            if (x > epsilon && y < -epsilon) return Direction.BottomRight;
            if (x < -epsilon && y < -epsilon) return Direction.BottomLeft;

            return Direction.None;
        }


        public Quaternion GetRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Quaternion.Euler(0, 0, -90).normalized;
                case Direction.Right: return Quaternion.Euler(0, 0, -180).normalized;
                case Direction.Down: return Quaternion.Euler(0, 0, 90).normalized;
                case Direction.Left: return Quaternion.Euler(0, 0, 180).normalized;
                case Direction.UpRight: return Quaternion.Euler(0, 0, -135).normalized;
                case Direction.UpLeft: return Quaternion.Euler(0, 0, -45).normalized;
                case Direction.BottomRight: return Quaternion.Euler(0, 0, -225).normalized;
                case Direction.BottomLeft: return Quaternion.Euler(0, 0, 50).normalized;
                default: return Quaternion.identity;
            }
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
