using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EInputButton
{
    JUMP,
    SPRINT,
    ATTACK,
    SECONDARYATTACK,
    MOBILITY,
    SPECIAL,
    INTERACT
}

/// <summary>
/// Input structure sent over network to the server.
/// </summary>
public struct NetworkInput : INetworkInput
{
    public Vector2 LookInput;
    public Vector2 MoveInput;
    public NetworkButtons Buttons;
}

/// <summary>
/// PlayerInput handles accumulating player input from Unity and passes the accumulated input to Fusion.
/// This version of PlayerInput showcases usage of IBeforeUpdate and IAfterTick callbacks.
/// </summary>
public sealed class PlayerInput : NetworkBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    [Tooltip("Sets how fast the mouse moves the camera")]
    [Range(0.01f, 5.0f)]
    [SerializeField] private float MouseSensitivity = 2f;

    [Tooltip("Sets how fast the mouse moves the camera")]
    [Range(0.01f, 5.0f)]
    [SerializeField] private float ControllerSensitivity = 2f;

    //Inputs need to be scaled by different constants to feel about the same, without normalizing the input
    private float mouseInputScaling = 0.1f;
    private float controllerInputScaling = 1.0f;

    //Our Custom Action Map and If using mouse or controller
    [SerializeField] private bool isUsingMouse = true;
    private PlayerInputActions playerInputActionMap;

    //Photon Networked Input Fields
    public NetworkInput input;
    private Vector2Accumulator lookRotationAccumulator = new Vector2Accumulator(0.02f, true);
    private bool resetInput;
    private NetworkRunner networkRunner;

    private void Awake()
    {
        playerInputActionMap = new PlayerInputActions();
        Cursor.lockState = CursorLockMode.Locked;
        UpdateInputs();
    }

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            this.enabled = false;
            return;
        }
    }
    public void UpdateInputs()
    {
        playerInputActionMap.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 2)*2.5f;
        ControllerSensitivity = PlayerPrefs.GetFloat("MouseSens", 2)*2.5f;
    }

    private void OnEnable()
    {
        //Ensure runner reference isnt null, mostly used for first player to join
        networkRunner = NetworkRunner.GetRunnerForGameObject(gameObject);

        //Enable Input actions
        playerInputActionMap.Player.Enable();

        //Add callbacks for detecting input device changes
        foreach (var action in playerInputActionMap.Player.Get().actions)
        {
            action.performed += CheckInputDevice;
        }

        //Add callbacks so we can pass input on the OnInput section
        if (networkRunner is not null)
        {
            networkRunner.AddCallbacks(this);
        }
    }

    private void OnDisable()
    {
        //Remove input device detection callbacks to prevent memory leak
        foreach (var action in playerInputActionMap.Player.Get().actions)
        {
            action.performed -= CheckInputDevice;
        }

        //Disable Input Actions
        playerInputActionMap.Player.Disable();

        //Remove Network Callbacks
        if (networkRunner is not null)
        {
            networkRunner.RemoveCallbacks(this);
        }
    }


    private void CheckInputDevice(InputAction.CallbackContext context)
    {
        //If youre using a Mouse/Keyboard and swapping to a Gamepad
        if (isUsingMouse && context.control.device is Gamepad)
        {
            isUsingMouse = false;
            Debug.Log("Swapped to Gamepad");
        }
        //If youre using a Gamepad and swapping to a Mouse/Keyboard
        else if (!isUsingMouse && (context.control.device is Mouse || context.control.device is Keyboard))
        {
            isUsingMouse = true;
            Debug.Log("Swapped to Mouse");
        }
        //If youre using a Mouse/Keyboard and not swapping to a new device
        else if (isUsingMouse && (context.control.device is Mouse || context.control.device is Keyboard))
        {
            return;
        }
        //If youre using a Gamepad and not swapping to a new device
        else if (!isUsingMouse && context.control.device is Gamepad)
        {
            return;
        }
        //If an unhandled device is used
        else
        {
            Debug.LogError($"Player Input Detected Input from unsupported device: {context.control.device}, IsUsingMouse:{isUsingMouse}");
        }
    }

    void IBeforeUpdate.BeforeUpdate()
    {
        //Reset accumulated input if neccicary
        if (resetInput)
        {
            resetInput = false;
            input = default;
        }

        //get the current player instance referencing the playerInputActionMap
        var playerActions = playerInputActionMap.Player;

        //Temp Buttons var to tell what was pressed this frame
        NetworkButtons buttonsPressedThisFrame = default;

        //read stick inputs
        input.MoveInput = playerActions.Move.ReadValue<Vector2>();

        //Get Look Input but seperate out mouse vs controller for sensitivity adjustment
        if (isUsingMouse)
        {
            Vector2 mouseDelta = playerActions.Look.ReadValue<Vector2>();
            lookRotationAccumulator.Accumulate(new Vector2(mouseDelta.x, mouseDelta.y) * mouseInputScaling * MouseSensitivity);
        }
        else
        {
            Vector2 controllerDelta = playerActions.Look.ReadValue<Vector2>();
            lookRotationAccumulator.Accumulate(new Vector2(controllerDelta.x, controllerDelta.y) * controllerInputScaling * ControllerSensitivity);
        }


        //Read Button Inputs
        buttonsPressedThisFrame.Set(EInputButton.JUMP, playerActions.Jump.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.SPRINT, playerActions.Sprint.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.ATTACK, playerActions.Attack.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.SECONDARYATTACK, playerActions.SecondaryAttack.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.MOBILITY, playerActions.Mobility.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.SPECIAL, playerActions.Special.IsPressed());
        buttonsPressedThisFrame.Set(EInputButton.INTERACT, playerActions.Interact.IsPressed());

        //add the buttons from this frames input to the accumulated inputs using a bitwise or opperator to combine the inputs
        input.Buttons = new NetworkButtons(input.Buttons.Bits | buttonsPressedThisFrame.Bits);
    }

    // Fusion polls accumulated input to keep movement server authoritative.
    // //This callback can be executed multiple times in a row if there is a performance spike.
    public void OnInput(NetworkRunner runner, Fusion.NetworkInput networkInput)
    {
        // Mouse movement (delta values) is aligned to engine update.
        // To get perfectly smooth interpolated look, we need to align the mouse input with Fusion ticks.
        input.LookInput = lookRotationAccumulator.ConsumeTickAligned(runner);

        //Set the collected input values to the server
        networkInput.Set(input);

        //Resets the input after the network tick gets the input
        resetInput = true;
    }

    #region unusedInterfaceCallbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    #endregion


}

