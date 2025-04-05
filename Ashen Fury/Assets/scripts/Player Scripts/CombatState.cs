using UnityEngine;
using UnityEngine.InputSystem;

public class CombatState : State
{
    float gravityValue;
    Vector3 currentVelocity;
    bool grounded;
    bool sheathWeapon;
    float playerSpeed;
    bool attack;
    Vector3 cVelocity;
    bool dodge;

    public CombatState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
        dodgeAction = character.playerInput.actions["Dodge"];
    }

    public override void Enter()
    {
        base.Enter();

        sheathWeapon = false;
        input = Vector2.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;
        attack = false;
        dodge = false;

        velocity = character.playerVelocity;
        playerSpeed = character.playerSpeed;
        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if (drawWeaponAction.triggered) sheathWeapon = true;
        if (attackAction.triggered) attack = true;
        if (dodgeAction.triggered) dodge = true;

        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);
        
        // Fix: Project camera forward direction onto horizontal plane
        Vector3 cameraForward = character.cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Vector3 cameraRight = character.cameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        
        velocity = velocity.x * cameraRight + velocity.z * cameraForward;
        velocity.y = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);

        /*if (sheathWeapon)
        {
            character.animator.SetTrigger("sheathWeapon");
            stateMachine.ChangeState(character.standing);
        }*/

        if (attack)
        {
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                // Force immediate rotation toward the enemy (ignoring current facing direction)
                Vector3 directionToEnemy = nearestEnemy.position - character.transform.position;
                directionToEnemy.y = 0; // Keep rotation horizontal
                character.transform.rotation = Quaternion.LookRotation(directionToEnemy);
            }

            character.animator.SetTrigger("attack");
            stateMachine.ChangeState(character.attacking);
        }

        if (dodge) stateMachine.ChangeState(character.dodging);
    }

    // Finds the closest enemy within range, regardless of direction
    private Transform FindNearestEnemy()
    {
        float detectionRadius = 5f; // Adjust based on your game's scale
        Collider[] hitColliders = Physics.OverlapSphere(character.transform.position, detectionRadius);
        Transform nearestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy")) // Adjust tag if needed
            {
                float distance = Vector3.Distance(character.transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = hitCollider.transform;
                }
            }
        }
        return nearestEnemy;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;

        if (grounded && gravityVelocity.y < 0) gravityVelocity.y = 0f;

        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);
        character.controller.Move(currentVelocity * Time.deltaTime * playerSpeed + gravityVelocity * Time.deltaTime);

        // Only rotate when moving (not attacking)
        if (velocity.sqrMagnitude > 0 && !attack)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
        }
    }

    public override void Exit()
    {
        base.Exit();
        gravityVelocity.y = 0f;
        character.playerVelocity = new Vector3(input.x, 0, input.y);

        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.LookRotation(velocity);
        }
    }
}