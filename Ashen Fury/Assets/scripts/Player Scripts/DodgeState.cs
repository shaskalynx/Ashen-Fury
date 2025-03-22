using UnityEngine;

public class DodgeState : State
{
    private float dodgeDuration;
    private float dodgeTime;
    private float dodgeSpeed;
    private Vector3 dodgeDirection;

    public DodgeState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        character.animator.applyRootMotion = false; // Ensure root motion is off

        dodgeDuration = character.dodgeDuration;
        dodgeSpeed = character.dodgeSpeed * 2f; // Increase speed for a noticeable dodge distance
        dodgeTime = 0;

        // Capture input direction at the start of dodge
        Vector2 input = moveAction.ReadValue<Vector2>();
        dodgeDirection = new Vector3(input.x, 0, input.y);

        if (dodgeDirection == Vector3.zero)
        {
            dodgeDirection = character.transform.forward; // Default to forward if no input is given
        }
        else
        {
            dodgeDirection = character.cameraTransform.forward * dodgeDirection.z + character.cameraTransform.right * dodgeDirection.x;
            dodgeDirection.y = 0;
            dodgeDirection.Normalize();
        }

        // Trigger the dodge animation
        character.animator.SetTrigger("dodge");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        dodgeTime += Time.deltaTime;
        if (dodgeTime >= dodgeDuration)
        {
            stateMachine.ChangeState(character.combatting);
            character.animator.SetTrigger("move");
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Apply smooth dodge movement
        if (dodgeTime < dodgeDuration)
        {
            character.controller.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
        }
    }
}
