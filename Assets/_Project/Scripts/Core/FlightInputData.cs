using System;

namespace OperationSindoor.Core
{
    /// <summary>
    /// Immutable/serializable telemetry data packet representing the raw or smoothed
    /// flight input state for an aircraft at any given frame.
    /// Decouples physical input hardware (keyboard, gamepad, HOTAS) from aircraft flight dynamics.
    /// </summary>
    [Serializable]
    public struct FlightInputData
    {
        /// <summary>
        /// Pitch input: -1.0f (Nose Down / Push Forward) to +1.0f (Nose Up / Pull Back).
        /// </summary>
        public float Pitch;

        /// <summary>
        /// Roll input: -1.0f (Bank Left) to +1.0f (Bank Right).
        /// </summary>
        public float Roll;

        /// <summary>
        /// Yaw input: -1.0f (Rudder Left) to +1.0f (Rudder Right).
        /// </summary>
        public float Yaw;

        /// <summary>
        /// Relative change in throttle for incremental keys: -1.0f (Decrease) to +1.0f (Increase).
        /// </summary>
        public float ThrottleDelta;

        /// <summary>
        /// Absolute throttle input (0.0f to 1.0f) for direct analog controls or integrated accumulator.
        /// </summary>
        public float RawThrottle;

        /// <summary>
        /// True when the airbrake is engaged to shed airspeed rapidly.
        /// </summary>
        public bool Airbrake;

        /// <summary>
        /// True when afterburner is active (boosts thrust beyond dry military power).
        /// </summary>
        public bool Afterburner;

        /// <summary>
        /// True while holding primary autocannon fire button.
        /// </summary>
        public bool FirePrimary;

        /// <summary>
        /// True on frame when missile launch is triggered.
        /// </summary>
        public bool FireMissile;

        /// <summary>
        /// True on frame when target cycle/lock is requested.
        /// </summary>
        public bool TargetLock;

        /// <summary>
        /// True on frame when flare/chaff countermeasure deployment is requested.
        /// </summary>
        public bool Countermeasures;

        /// <summary>
        /// True on frame when switching between chase and cockpit cameras.
        /// </summary>
        public bool SwitchCamera;

        /// <summary>
        /// True on frame when pausing the game.
        /// </summary>
        public bool Pause;

        public static FlightInputData Zero => new FlightInputData
        {
            Pitch = 0f,
            Roll = 0f,
            Yaw = 0f,
            ThrottleDelta = 0f,
            RawThrottle = 0f,
            Airbrake = false,
            Afterburner = false,
            FirePrimary = false,
            FireMissile = false,
            TargetLock = false,
            Countermeasures = false,
            SwitchCamera = false,
            Pause = false
        };
    }
}
