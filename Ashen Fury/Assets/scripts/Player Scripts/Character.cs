using UnityEngine;
using UnityEngine.InputSystem;
public class Character : MonoBehaviour
{
    [Header("Controls")]
    public float playerSpeed = 5.0f;
    public float crouchSpeed = 2.0f;
    public float sprintSpeed = 7.0f;
    public float jumpHeight = 0.8f; 
    public float gravityMultiplier = 2;
    public float rotationSpeed = 5f;
    public float crouchColliderHeight = 1.35f;
    public float dodgeDuration = 0.5f; // Add dodge duration
    public float dodgeSpeed = 10.0f; // Add dodge speed
 
    [Header("Animation Smoothing")]
    [Range(0, 1)]
    public float speedDampTime = 0.1f;
    [Range(0, 1)]
    public float velocityDampTime = 0.9f;
    [Range(0, 1)]
    public float rotationDampTime = 0.2f;
    [Range(0, 1)]
    public float airControl = 0.5f;
 
    public StateMachine movementSM;
    public StandingState standing;
    public JumpingState jumping;
    public CrouchingState crouching;
    public LandingState landing;
    public SprintState sprinting;
    public SprintJumpState sprintjumping;
    public CombatState combatting;
    public AttackState attacking;
    public DodgeState dodging; // Add dodging state
 
    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector]
    public float normalColliderHeight;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector]
    public PlayerInput playerInput;
    [HideInInspector]
    public Transform cameraTransform;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Vector3 playerVelocity;

    //SOUND MANAGER
    [SerializeField] private PlayerSoundManager soundManager;
 
 
    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        //Cursor.lockState = CursorLockMode.Locked; // Lock the cursor in the middle
        Cursor.visible = false; // Hide the cursor in gameplay

        // (SOUND MANAGER) If not assigned in inspector, try to find it in children
        if (soundManager == null)
        {
            soundManager = GetComponentInChildren<PlayerSoundManager>();
        }
 
        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        jumping = new JumpingState(this, movementSM);
        crouching = new CrouchingState(this, movementSM);
        landing = new LandingState(this, movementSM);
        sprinting = new SprintState(this, movementSM);
        sprintjumping = new SprintJumpState(this, movementSM);
        combatting = new CombatState(this, movementSM);
        attacking = new AttackState(this, movementSM);
        dodging = new DodgeState(this, movementSM); // Initialize dodging state
 
        movementSM.Initialize(standing);
 
        normalColliderHeight = controller.height;
        gravityValue *= gravityMultiplier;
    }
 
    private void Update()
    {
        movementSM.currentState.HandleInput();
 
        movementSM.currentState.LogicUpdate();
    }
 
    private void FixedUpdate()
    {
        // Only update if the CharacterController is enabled
        if (controller != null && controller.enabled)
        {
            movementSM.currentState.PhysicsUpdate();
        }
    }

    // Optional: Add a method to handle death state
    public void OnDeath()
    {
        // This can be called from your HealthSystem when the player dies
        enabled = false; // This will disable the entire Character script
    }

    // SOUND MANAGER Animation Event receivers
    public void PlayAttack1()
    {
        soundManager.PlayAttack1();
    }

    public void PlayAttack2()
    {
        soundManager.PlayAttack2();
    }

    public void PlaySwordSheath()
    {
        soundManager.PlaySwordSheath();
    }

    public void PlaySwordUnsheath()
    {
        soundManager.PlaySwordUnsheath();
    }

    public void PlayJump()
    {
        soundManager.PlayJump();
    }

    public void PlayDamage()
    {
        soundManager.PlayDamage();
    }

    public void PlayDeath()
    {
        soundManager.PlayDeath();
    }

    /*public void PlayFootstep()
    {
        soundManager.PlayFootstep();
    }*/
}