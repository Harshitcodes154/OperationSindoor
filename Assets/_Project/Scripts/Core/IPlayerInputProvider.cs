using System;

namespace OperationSindoor.Core
{
    /// <summary>
    /// Contract defining an input source for an aircraft.
    /// Both human player input (via FlightInputReader) and AI flight autopilots
    /// implement this interface, allowing identical aircraft physics controllers
    /// to be piloted transparently by either player or AI bots.
    /// </summary>
    public interface IPlayerInputProvider
    {
        /// <summary>
        /// Current continuous state of flight control surfaces, throttle, and active weapon triggers.
        /// </summary>
        FlightInputData CurrentInput { get; }

        /// <summary>
        /// Event fired when primary fire is triggered.
        /// </summary>
        event Action OnFirePrimaryTriggered;

        /// <summary>
        /// Event fired when a missile release is requested.
        /// </summary>
        event Action OnFireMissileTriggered;

        /// <summary>
        /// Event fired when cycling or acquiring target locks.
        /// </summary>
        event Action OnTargetLockTriggered;

        /// <summary>
        /// Event fired when deploying defensive countermeasures (flares/chaff).
        /// </summary>
        event Action OnCountermeasuresTriggered;

        /// <summary>
        /// Event fired when toggling between chase and cockpit camera perspectives.
        /// </summary>
        event Action OnSwitchCameraTriggered;

        /// <summary>
        /// Event fired when pausing/unpausing gameplay.
        /// </summary>
        event Action OnPauseTriggered;
    }
}
