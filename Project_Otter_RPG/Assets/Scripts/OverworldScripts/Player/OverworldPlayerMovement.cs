using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class OverworldPlayerMovement : MonoBehaviour
{
    private InputEventManager IEM;
    private OverworldEventManager OEM;
    private CinemachineCamera cam;

    public enum PlayerState
    {
        IDLE, WALK, FALLING
    }

    // values
    private float verticalInputRaw;
    private float horizontalInputRaw;
    private CharacterController characterController;
    private bool commitedToVertical;

    [Header("References")]
    [SerializeField] private SpriteInstance SI;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private PlayerState playerState;

    [Header("Invalid Direction Check Settings")]
    [SerializeField] private float invalidDirectionCheckMagnitude;
    [SerializeField] private float startingRaycastHeight;
    [SerializeField] private float raycastLength;
    [SerializeField] private LayerMask invalidFloorMask;

    [Header("Gravity")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float fallHeight = 1f;
    private bool fallChecked = false;

    [Tooltip("Set this to be a small negative so Harte \"sticks\" to the ground. Leaving it at -2 is fine.")]
    [SerializeField] private float groundedVelocity = -2f;

    private float verticalVelocity;

    private PlayerActions playerActions;
    [SerializeField] GameObject MenuCanvas;

    private void Start()
    {
        IEM = InputEventManager.Instance;
        if (IEM != null)
        {
            IEM.onVerticalInput.AddListener(ReadVerticalInput);
            IEM.onHorizontalInput.AddListener(ReadHorizontalInput);
        }

        OEM = OverworldEventManager.Instance;
        if (OEM != null)
        {
            OEM.onCameraSwitch.AddListener(SwitchCamera);
        }

        SetPlayerState(PlayerState.IDLE);
        cam = GameObject.Find("Main Camera").GetComponent<CinemachineCamera>();
    }

    private void OnDestroy() // i stg if i forget to remove listeners properly again in this project im gonna explode
    {
        if (IEM != null)
        {
            IEM.onVerticalInput.RemoveListener(ReadVerticalInput);
            IEM.onHorizontalInput.RemoveListener(ReadHorizontalInput);
        }

        if(OEM != null)
        {
            OEM.onCameraSwitch.RemoveListener(SwitchCamera);
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        playerActions = new PlayerActions();
        playerActions.Overworld.Menu.performed += MenuAccess;

    }

    void ReadVerticalInput(float value) { verticalInputRaw = value; }
    void ReadHorizontalInput(float value) { horizontalInputRaw = value; }

    private void SwitchCamera(CinemachineCamera newCam)
    {
        cam = newCam;
    }

    private void Movement()
    {
        if (cam == null) Debug.Log("camera null");

        if (IEM != null)
        {
            if (IEM.CanInput == false)
            {
                verticalInputRaw = 0;
                horizontalInputRaw = 0;
                SetPlayerState(PlayerState.IDLE);
                Debug.LogWarning("Input disabled");
                return;
            }
        }
        else
            Debug.LogWarning("IEM null");

        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();
        Vector3 horizontalDir = horizontalInputRaw * camRight + verticalInputRaw * camForward;
        horizontalDir.Normalize();

        if (horizontalDir != Vector3.zero)
        {
            bool horizontalHeld = horizontalInputRaw != 0;
            bool verticalHeld = verticalInputRaw != 0;

            if (commitedToVertical && verticalHeld)
            {
                Vector3 faceDir = verticalInputRaw > 0 ? camForward : -camForward;
                transform.rotation = Quaternion.LookRotation(faceDir);
            }
            else if (!commitedToVertical && horizontalHeld)
            {
                Vector3 faceDir = horizontalInputRaw > 0 ? camRight : -camRight;
                transform.rotation = Quaternion.LookRotation(faceDir);
            }
            else
            {
                if (verticalHeld && horizontalHeld) commitedToVertical = true;
                else if (verticalHeld) commitedToVertical = true;
                else if (horizontalHeld) commitedToVertical = false;
            }
        }

        // check if the player is trying to move into an invalid direction (maybe into a ledge or into surfaces like water)
        bool negXCheck = Physics.Raycast(transform.position + Vector3.left * invalidDirectionCheckMagnitude + Vector3.up * startingRaycastHeight, Vector3.down, out RaycastHit negXHit, raycastLength);
        bool posXCheck = Physics.Raycast(transform.position + Vector3.right * invalidDirectionCheckMagnitude + Vector3.up * startingRaycastHeight, Vector3.down, out RaycastHit posXHit, raycastLength);
        bool negZCheck = Physics.Raycast(transform.position + Vector3.back * invalidDirectionCheckMagnitude + Vector3.up * startingRaycastHeight, Vector3.down, out RaycastHit negZHit, raycastLength);
        bool posZCheck = Physics.Raycast(transform.position + Vector3.forward * invalidDirectionCheckMagnitude + Vector3.up * startingRaycastHeight, Vector3.down, out RaycastHit posZHit, raycastLength);

        if (horizontalDir.x > 0)
        { 
            if(!posXCheck || CheckInvalidSurface(posXHit.collider.gameObject.layer)) horizontalDir.x = 0;
        }

        if (horizontalDir.x < 0)
        { 
            if(!negXCheck || CheckInvalidSurface(negXHit.collider.gameObject.layer)) horizontalDir.x = 0;
        }

        if (horizontalDir.z > 0)
        {
            if(!posZCheck || CheckInvalidSurface(posZHit.collider.gameObject.layer)) horizontalDir.z = 0;
        }

        if (horizontalDir.z < 0)
        { 
            if(!negZCheck || CheckInvalidSurface(negZHit.collider.gameObject.layer)) horizontalDir.z = 0;
        }

        if (horizontalDir != Vector3.zero)
        {
            bool diagonalCheck = Physics.Raycast(transform.position + horizontalDir.normalized * invalidDirectionCheckMagnitude + Vector3.up * startingRaycastHeight, Vector3.down, out RaycastHit diagHit, raycastLength);
            if (!diagonalCheck || (diagonalCheck && CheckInvalidSurface(diagHit.collider.gameObject.layer)))
            {
                horizontalDir = Vector3.zero;
            }
        }

        verticalVelocity = characterController.isGrounded ? groundedVelocity : verticalVelocity - gravity * Time.deltaTime;
        characterController.Move((horizontalDir * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        if (!characterController.isGrounded)
        {
            if(!fallChecked)
            {
                fallChecked = true;
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit floorHit, 100f))
                {
                    if (transform.position.y - floorHit.point.y > fallHeight) 
                        SetPlayerState(PlayerState.FALLING);
                }
            }
        }
        else
        {
            fallChecked = false;
            SetPlayerState(horizontalDir != Vector3.zero ? PlayerState.WALK : PlayerState.IDLE);
        } 
    }

    private bool CheckInvalidSurface(int layer)
    {
        if (((1 << layer) & invalidFloorMask) != 0)
        {
            return true;
        }
        return false;                                                                                                           
    }

    private void SetPlayerState(PlayerState state)
    {
        if (state == playerState) return;
        playerState = state;
        SI.Play(playerState.ToString().ToLower());
    }

    private void Update()
    {
        Movement();
    }

    private void MenuAccess(InputAction.CallbackContext context)
    {
        InventoryMenu InvMenu = MenuCanvas.GetComponent<InventoryMenu>();
        InvMenu.ChangeDisplayMode();

        if (InvMenu.GetCanSee())
        {
            IEM.onVerticalInput.RemoveListener(ReadVerticalInput);
            IEM.onHorizontalInput.RemoveListener(ReadHorizontalInput);

            GameObject buttonCont = InvMenu.GetButtonContainer();
            int buttonIterator = 0;
            UnityEngine.UI.Button targetButton = null;

            foreach (Transform child in buttonCont.transform)
            {
                Navigation navigation = new Navigation();
                if (buttonIterator > 0)
                {
                    Debug.Log("MADE IT INTO NAVIGATION IF STATEMENT!");

                    navigation.mode = Navigation.Mode.Explicit;
                    navigation.selectOnDown = child.GetComponent<UnityEngine.UI.Button>();
                    targetButton.GetComponent<UnityEngine.UI.Button>().navigation = navigation;

                    navigation.mode = Navigation.Mode.Explicit;
                    navigation.selectOnUp = targetButton.GetComponent<UnityEngine.UI.Button>();
                    child.GetComponent<UnityEngine.UI.Button>().navigation = navigation;

                    Debug.Log("Button Iterator Value: " + buttonIterator.ToString());
                }
                targetButton = child.GetComponent<UnityEngine.UI.Button>();
                buttonIterator++;
            }
        }
        else
        {
            IEM.onVerticalInput.AddListener(ReadVerticalInput);
            IEM.onHorizontalInput.AddListener(ReadHorizontalInput);
        }
    }

    private void OnEnable()
    {
        playerActions.Overworld.Menu.Enable();
    }

    private void OnDisable()
    {
        playerActions.Overworld.Menu.Disable();
    }
}
