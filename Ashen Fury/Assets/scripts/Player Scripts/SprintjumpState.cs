using UnityEngine;
public class SprintJumpState:State
{
    float timePassed;
    float jumpTime;
    float gravityValue;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;
    Vector3 cVelocity;
    Vector3 input;
    Vector3 velocity;
    Vector3 gravityVelocity;

    public SprintJumpState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }
 
    public override void Enter()
    {
        base.Enter();
        character.animator.applyRootMotion = true;
        timePassed = 0f;
        character.animator.SetTrigger("sprintJump");
 
        jumpTime = 1f;
        playerSpeed = character.sprintSpeed;
        gravityValue = character.gravityValue;
        currentVelocity = character.playerVelocity;
        grounded = character.controller.isGrounded;
    }
 
    public override void Exit()
    {
        base.Exit();
        character.animator.applyRootMotion = false;
    }
 
    public override void LogicUpdate()
    {
        
        base.LogicUpdate();
        if (timePassed> jumpTime)
        {
            character.animator.SetTrigger("move");
            stateMachine.ChangeState(character.sprinting);
        }
        timePassed += Time.deltaTime;
    }
 
    public override void HandleInput()
    {
        base.HandleInput();
        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);
        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;
    }
 
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);
        character.controller.Move(currentVelocity * Time.deltaTime * playerSpeed + gravityVelocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
        }
    }
}