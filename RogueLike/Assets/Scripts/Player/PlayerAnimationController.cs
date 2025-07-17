using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : EntityAnimationController
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        Animate(playerController.AnimationClipHashes);
    }
}