using System;
using Fusion;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HandPose
{
    Default,
    Grab,
    GrabItem
}

public enum HandFingerPose
{
    ButtonPress,
    GripPress,
    TriggerPress
}

public class HandController : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int Normal = Animator.StringToHash("normal");
    private static readonly int Grab = Animator.StringToHash("grab");
    private static readonly int Spinning = Animator.StringToHash("spinning");
    
    [SerializeField] private Controller controller;
    public Handedness handedness;
    private RayInteractor rayInteractor;
    private GrabInteractor grabInteractor;

    
    public Handedness mainHand = Handedness.Right; // TODO: 다른 곳에서 X손잡이 설정
    private static readonly int Index = Animator.StringToHash("index");
    private static readonly int Middle = Animator.StringToHash("middle");
    private static readonly int Thumb = Animator.StringToHash("thumb");

    [Header("Inputs")]
    private InputAction leftGripPressAction;
    private InputAction rightGripPressAction;
    private InputAction leftTriggerPressAction;
    private InputAction rightTriggerPressAction;
    
    private InputAction leftGripAction;
    private InputAction rightGripAction;
    private InputAction leftTriggerAction;
    private InputAction rightTriggerAction;
    
    private InputAction rightPrimaryButtonAction;
    private InputAction rightPrimaryTouchAction;
    private InputAction leftPrimaryButtonAction;
    private InputAction leftPrimaryTouchAction;
    
    private InputAction rightSecondaryButtonAction;
    private InputAction rightSecondaryTouchAction;
    private InputAction leftSecondaryButtonAction;
    private InputAction leftSecondaryTouchAction;
    
    private InputAction leftThumbstickAction;
    public InputAction rightThumbstickAction;
    
    public event Action<bool> OnRightTriggerPressed;
    public event Action OnRightSecondaryButtonPressed;

    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag(handedness == Handedness.Right ? "rightController" : "leftController").GetComponent<Controller>();
        rayInteractor = controller.GetComponentInChildren<RayInteractor>();
        grabInteractor = controller.GetComponentInChildren<GrabInteractor>();
        
        rayInteractor.gameObject.SetActive(false);
        grabInteractor.gameObject.SetActive(false);
        
        rayInteractor.WhenStateChanged += HandleRayStateChanged;
        grabInteractor.WhenStateChanged += HandleGrabStateChanged;
        
        if (handedness == Handedness.Left)
        {
            leftGripPressAction = InputSystem.actions.FindAction("LeftHand/GripPress");
            leftTriggerPressAction = InputSystem.actions.FindAction("LeftHand/TriggerPress");
            leftGripAction = InputSystem.actions.FindAction("LeftHand/Grip");
            leftTriggerAction = InputSystem.actions.FindAction("LeftHand/Trigger");
            leftPrimaryButtonAction = InputSystem.actions.FindAction("LeftHand/PrimaryButton");
            leftPrimaryTouchAction = InputSystem.actions.FindAction("LeftHand/PrimaryTouch");
            leftSecondaryButtonAction = InputSystem.actions.FindAction("LeftHand/SecondaryButton");
            leftSecondaryTouchAction = InputSystem.actions.FindAction("LeftHand/SecondaryTouch");
            leftThumbstickAction = InputSystem.actions.FindAction("LeftHand/Primary2DAxis");
        }
        if (handedness == Handedness.Right)
        {
            rightGripPressAction = InputSystem.actions.FindAction("RightHand/GripPress");
            rightTriggerPressAction = InputSystem.actions.FindAction("RightHand/TriggerPress");
            rightGripAction = InputSystem.actions.FindAction("RightHand/Grip");
            rightTriggerAction = InputSystem.actions.FindAction("RightHand/Trigger");
            rightPrimaryButtonAction = InputSystem.actions.FindAction("RightHand/PrimaryButton");
            rightPrimaryTouchAction = InputSystem.actions.FindAction("RightHand/PrimaryTouch");
            rightSecondaryButtonAction = InputSystem.actions.FindAction("RightHand/SecondaryButton");
            rightSecondaryTouchAction = InputSystem.actions.FindAction("RightHand/SecondaryTouch");
            
            rightTriggerPressAction.started += _ => { OnRightTriggerPressed?.Invoke(true); };
            rightTriggerPressAction.canceled += _ => { OnRightTriggerPressed?.Invoke(false); };
            rightSecondaryButtonAction.started += _ => { OnRightSecondaryButtonPressed?.Invoke(); };
            rightThumbstickAction = InputSystem.actions.FindAction("RightHand/Primary2DAxis");
        }
    }

    private void Update()
    {
        UpdateHand();
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasStateAuthority)
        {
            rayInteractor.WhenStateChanged -= HandleRayStateChanged;
            grabInteractor.WhenStateChanged -= HandleGrabStateChanged;
        }
    }
    
#region Interactions
    private void HandleRayStateChanged(InteractorStateChangeArgs args)
    {
        
    }
    
    private void HandleGrabStateChanged(InteractorStateChangeArgs args)
    {
        switch (args.NewState)
        {
            case InteractorState.Select:
                if (grabInteractor.Interactable != null)
                {
                    GrabHand();
                }
                break;
            case InteractorState.Hover:
                
                // TODO: Might have to check which interactable was unselected
                if (args.PreviousState == InteractorState.Select)
                {
                    ReleaseHand();
                }
                break;
        }
    }
    
    private void GrabHand()
    {
        Debug.Log($"Grab Hand {grabInteractor.Interactable.gameObject.name}");
        ChangeHandGrabPose(HandPose.Grab);
    }

    private void ReleaseHand()
    {
        ChangeHandGrabPose(HandPose.Default);
    }
    
    
#endregion
    
#region Hand Animations
    private void UpdateHand()
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        
        CheckButtonPress();
        
        if (handedness == Handedness.Left)
        {
            ChangeHandFingerPose(HandFingerPose.GripPress, leftGripAction.ReadValue<float>());
            ChangeHandFingerPose(HandFingerPose.TriggerPress, leftTriggerAction.ReadValue<float>());
        }
        
        if (handedness == Handedness.Right)
        {
            ChangeHandFingerPose(HandFingerPose.GripPress, rightGripAction.ReadValue<float>());
            ChangeHandFingerPose(HandFingerPose.TriggerPress, rightTriggerAction.ReadValue<float>());
        }
    }
    
    private void CheckButtonPress()
    {
        var touchValue = 0f;
        bool anyPressed;
        
        if (handedness == Handedness.Left)
        {
            anyPressed = leftPrimaryButtonAction?.IsPressed() ?? false;
            anyPressed = anyPressed || (leftSecondaryButtonAction?.IsPressed() ?? false);
            touchValue = Mathf.Max(touchValue, leftPrimaryTouchAction?.ReadValue<float>() ?? 0);
            touchValue = Mathf.Max(touchValue, leftSecondaryTouchAction?.ReadValue<float>() ?? 0);
        }
        else
        {
            anyPressed = rightPrimaryButtonAction?.IsPressed()  ?? false;;
            anyPressed = anyPressed || (rightSecondaryButtonAction?.IsPressed() ?? false);
            touchValue = Mathf.Max(touchValue, rightPrimaryTouchAction?.ReadValue<float>() ?? 0);
            touchValue = Mathf.Max(touchValue, rightSecondaryTouchAction?.ReadValue<float>() ?? 0);
        }

        touchValue *= 0.75f;
        var pressValue = anyPressed ? 1 : 0;
        ChangeHandFingerPose(HandFingerPose.ButtonPress, Mathf.Max(pressValue, touchValue));
    }

    private void ChangeHandFingerPose(HandFingerPose pose, float value)
    {
        switch (pose)
        {
            case HandFingerPose.ButtonPress:
                animator.SetFloat(Thumb, value);
                break;
            case HandFingerPose.GripPress:
                animator.SetFloat(Middle, value);
                break;
            case HandFingerPose.TriggerPress:
                animator.SetFloat(Index, value);
                break;
        }
    }

    private void ChangeHandGrabPose(HandPose pose)
    {
        if (pose == HandPose.Default)
        {
            animator.SetLayerWeight(1, 1);
            animator.SetLayerWeight(2, 1);
            animator.SetLayerWeight(3, 1);
        }
        else
        {
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(2, 0);
            animator.SetLayerWeight(3, 0);
        }
        
        animator.SetBool(Normal, pose == HandPose.Default);
        animator.SetBool(Grab, pose == HandPose.Grab);
        animator.SetBool(Spinning, pose == HandPose.GrabItem);
    }
#endregion
}
