using System;
using UnityEngine;
using UnityEngine.InputSystem;
using OperationSindoor.Core;

namespace OperationSindoor.Input
{
    /// <summary>
    /// Production-grade flight input reader bridging Unity's New Input System to the flight simulation.
    /// Supports InputActionAsset callbacks with an automatic direct-polling fallback for instant testing.
    /// Incorporates control-stick filtering, inertia smoothing, deadzones, and throttle accumulation.
    /// </summary>
    [AddComponentMenu("Operation Sindoor/Input/Flight Input Reader")]
    public class FlightInputReader : MonoBehaviour, IPlayerInputProvider
    {
        [Header("Action Asset (Optional - Auto-detects if null)")]
        [Tooltip("The FlightControls.inputactions asset. If null, direct hardware polling will be used.")]
        [SerializeField] private InputActionAsset inputActionsAsset;

        [Header("Flight Stick Sensitivity & Tuning")]
        [Tooltip("Pitch sensitivity multiplier.")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float pitchSensitivity = 1.0f;

        [Tooltip("Invert flight pitch (default false: S/Pull Stick = Pitch Up (+1), W/Push Stick = Pitch Down (-1)).")]
        [SerializeField] private bool invertPitch = false;

        [Tooltip("Roll sensitivity multiplier.")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float rollSensitivity = 1.0f;

        [Tooltip("Yaw (rudder) sensitivity multiplier.")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float yawSensitivity = 1.0f;

        [Tooltip("Control surface smoothing speed. Higher = snappier, Lower = more aerodynamic stick inertia.")]
        [Range(1f, 30f)]
        [SerializeField] private float stickSmoothing = 12f;

        [Header("Throttle Control Configuration")]
        [Tooltip("Rate at which throttle increases/decreases per second while holding throttle keys (0 to 1).")]
        [Range(0.1f, 1.0f)]
        [SerializeField] private float throttleRampSpeed = 0.5f;

        [Tooltip("Initial throttle setting when spawning (0.0 = idle, 0.7 = military cruise, 1.0 = full dry power).")]
        [Range(0f, 1f)]
        [SerializeField] private float initialThrottle = 0.65f;

        // Current snapshot
        private FlightInputData currentInputData;
        public FlightInputData CurrentInput => currentInputData;

        // Interface events
        public event Action OnFirePrimaryTriggered;
        public event Action OnFireMissileTriggered;
        public event Action OnTargetLockTriggered;
        public event Action OnCountermeasuresTriggered;
        public event Action OnSwitchCameraTriggered;
        public event Action OnPauseTriggered;

        // Internal input action references
        private InputAction pitchAction;
        private InputAction rollAction;
        private InputAction yawAction;
        private InputAction throttleAction;
        private InputAction airbrakeAction;
        private InputAction afterburnerAction;
        private InputAction firePrimaryAction;
        private InputAction fireMissileAction;
        private InputAction targetLockAction;
        private InputAction countermeasuresAction;
        private InputAction switchCameraAction;
        private InputAction pauseAction;

        private bool actionsInitialized = false;

        // Smoothed axes
        private float targetPitch;
        private float targetRoll;
        private float targetYaw;
        private float accumulatedThrottle;
        private bool isAfterburnerActive;

        private void Awake()
        {
            accumulatedThrottle = Mathf.Clamp01(initialThrottle);
            InitializeInputActions();
        }

        private void OnEnable()
        {
            EnableInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void InitializeInputActions()
        {
            if (inputActionsAsset != null)
            {
                var flightMap = inputActionsAsset.FindActionMap("Flight");
                if (flightMap != null)
                {
                    pitchAction = flightMap.FindAction("Pitch");
                    rollAction = flightMap.FindAction("Roll");
                    yawAction = flightMap.FindAction("Yaw");
                    throttleAction = flightMap.FindAction("Throttle");
                    airbrakeAction = flightMap.FindAction("Airbrake");
                    afterburnerAction = flightMap.FindAction("Afterburner");
                    firePrimaryAction = flightMap.FindAction("FirePrimary");
                    fireMissileAction = flightMap.FindAction("FireMissile");
                    targetLockAction = flightMap.FindAction("TargetLock");
                    countermeasuresAction = flightMap.FindAction("Countermeasures");
                    switchCameraAction = flightMap.FindAction("SwitchCamera");
                    pauseAction = flightMap.FindAction("Pause");

                    BindActionEvents();
                    actionsInitialized = true;
                }
            }
        }

        private void BindActionEvents()
        {
            if (firePrimaryAction != null)
            {
                firePrimaryAction.performed += ctx => OnFirePrimaryTriggered?.Invoke();
            }
            if (fireMissileAction != null)
            {
                fireMissileAction.performed += ctx => OnFireMissileTriggered?.Invoke();
            }
            if (targetLockAction != null)
            {
                targetLockAction.performed += ctx => OnTargetLockTriggered?.Invoke();
            }
            if (countermeasuresAction != null)
            {
                countermeasuresAction.performed += ctx => OnCountermeasuresTriggered?.Invoke();
            }
            if (switchCameraAction != null)
            {
                switchCameraAction.performed += ctx => OnSwitchCameraTriggered?.Invoke();
            }
            if (pauseAction != null)
            {
                pauseAction.performed += ctx => OnPauseTriggered?.Invoke();
            }
        }

        private void EnableInputActions()
        {
            if (inputActionsAsset != null)
            {
                inputActionsAsset.Enable();
            }
        }

        private void DisableInputActions()
        {
            if (inputActionsAsset != null)
            {
                inputActionsAsset.Disable();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            ReadInputs(dt);
        }

        private void ReadInputs(float dt)
        {
            float rawPitch = 0f;
            float rawRoll = 0f;
            float rawYaw = 0f;
            float throttleDelta = 0f;
            bool airbrake = false;
            bool afterburnerToggle = false;
            bool firePrimary = false;
            bool fireMissile = false;
            bool targetLock = false;
            bool countermeasures = false;
            bool switchCamera = false;
            bool pause = false;

            if (actionsInitialized && inputActionsAsset.enabled)
            {
                // Read from New Input System actions
                if (pitchAction != null) rawPitch = pitchAction.ReadValue<float>();
                if (rollAction != null) rawRoll = rollAction.ReadValue<float>();
                if (yawAction != null) rawYaw = yawAction.ReadValue<float>();
                if (throttleAction != null) throttleDelta = throttleAction.ReadValue<float>();
                if (airbrakeAction != null) airbrake = airbrakeAction.IsPressed();
                if (afterburnerAction != null) afterburnerToggle = afterburnerAction.WasPressedThisFrame();
                if (firePrimaryAction != null) firePrimary = firePrimaryAction.IsPressed();
                if (fireMissileAction != null) fireMissile = fireMissileAction.WasPressedThisFrame();
                if (targetLockAction != null) targetLock = targetLockAction.WasPressedThisFrame();
                if (countermeasuresAction != null) countermeasures = countermeasuresAction.WasPressedThisFrame();
                if (switchCameraAction != null) switchCamera = switchCameraAction.WasPressedThisFrame();
                if (pauseAction != null) pause = pauseAction.WasPressedThisFrame();
            }
            else
            {
                // Standalone Hardware Polling Fallback
                ReadHardwareDirect(ref rawPitch, ref rawRoll, ref rawYaw, ref throttleDelta,
                    ref airbrake, ref afterburnerToggle, ref firePrimary, ref fireMissile,
                    ref targetLock, ref countermeasures, ref switchCamera, ref pause);
            }

            // Afterburner toggling
            if (afterburnerToggle)
            {
                isAfterburnerActive = !isAfterburnerActive;
            }

            // Pitch inversion & sensitivity
            targetPitch = (invertPitch ? -rawPitch : rawPitch) * pitchSensitivity;
            targetRoll = rawRoll * rollSensitivity;
            targetYaw = rawYaw * yawSensitivity;

            // Smooth stick deflection
            currentInputData.Pitch = Mathf.MoveTowards(currentInputData.Pitch, targetPitch, stickSmoothing * dt);
            currentInputData.Roll = Mathf.MoveTowards(currentInputData.Roll, targetRoll, stickSmoothing * dt);
            currentInputData.Yaw = Mathf.MoveTowards(currentInputData.Yaw, targetYaw, stickSmoothing * dt);

            // Throttle integration
            accumulatedThrottle = Mathf.Clamp01(accumulatedThrottle + (throttleDelta * throttleRampSpeed * dt));
            currentInputData.ThrottleDelta = throttleDelta;
            currentInputData.RawThrottle = accumulatedThrottle;

            // Discrete / binary states
            currentInputData.Airbrake = airbrake;
            currentInputData.Afterburner = isAfterburnerActive;
            currentInputData.FirePrimary = firePrimary;
            currentInputData.FireMissile = fireMissile;
            currentInputData.TargetLock = targetLock;
            currentInputData.Countermeasures = countermeasures;
            currentInputData.SwitchCamera = switchCamera;
            currentInputData.Pause = pause;
        }

        private void ReadHardwareDirect(
            ref float rawPitch, ref float rawRoll, ref float rawYaw, ref float throttleDelta,
            ref bool airbrake, ref bool afterburnerToggle, ref bool firePrimary, ref bool fireMissile,
            ref bool targetLock, ref bool countermeasures, ref bool switchCamera, ref bool pause)
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var gamepad = Gamepad.current;

            if (keyboard != null)
            {
                // Pitch: S = up (+1), W = down (-1)
                if (keyboard.sKey.isPressed) rawPitch += 1f;
                if (keyboard.wKey.isPressed) rawPitch -= 1f;

                // Roll: D = right (+1), A = left (-1)
                if (keyboard.dKey.isPressed) rawRoll += 1f;
                if (keyboard.aKey.isPressed) rawRoll -= 1f;

                // Yaw: E = right (+1), Q = left (-1)
                if (keyboard.eKey.isPressed) rawYaw += 1f;
                if (keyboard.qKey.isPressed) rawYaw -= 1f;

                // Throttle: LeftShift = up (+1), LeftCtrl = down (-1)
                if (keyboard.leftShiftKey.isPressed) throttleDelta += 1f;
                if (keyboard.leftCtrlKey.isPressed) throttleDelta -= 1f;

                airbrake = keyboard.spaceKey.isPressed;
                if (keyboard.tabKey.wasPressedThisFrame) afterburnerToggle = true;
                if (keyboard.fKey.wasPressedThisFrame) fireMissile = true;
                if (keyboard.rKey.wasPressedThisFrame) targetLock = true;
                if (keyboard.xKey.wasPressedThisFrame) countermeasures = true;
                if (keyboard.cKey.wasPressedThisFrame) switchCamera = true;
                if (keyboard.escapeKey.wasPressedThisFrame) pause = true;
            }

            if (mouse != null)
            {
                if (mouse.leftButton.isPressed) firePrimary = true;
                if (mouse.rightButton.wasPressedThisFrame) fireMissile = true;
                if (mouse.middleButton.wasPressedThisFrame) targetLock = true;
            }

            if (gamepad != null)
            {
                Vector2 leftStick = gamepad.leftStick.ReadValue();
                rawPitch += -leftStick.y; // Pull back to climb
                rawRoll += leftStick.x;

                Vector2 rightStick = gamepad.rightStick.ReadValue();
                rawYaw += rightStick.x;

                if (gamepad.rightTrigger.isPressed) firePrimary = true;
                if (gamepad.rightShoulder.wasPressedThisFrame) fireMissile = true;
                if (gamepad.buttonNorth.wasPressedThisFrame) targetLock = true; // Triangle / Y
                if (gamepad.buttonEast.isPressed) airbrake = true;             // Circle / B
                if (gamepad.leftStickButton.wasPressedThisFrame) afterburnerToggle = true; // L3
                if (gamepad.dpad.down.wasPressedThisFrame) countermeasures = true;
                if (gamepad.dpad.up.wasPressedThisFrame) switchCamera = true;
                if (gamepad.startButton.wasPressedThisFrame) pause = true;
            }
        }
    }
}
