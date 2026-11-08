using BrickController2.CreationManagement;

namespace BrickController2.BusinessLogic
{
    public interface ISequencePlayer
    {
        void StartPlayer();
        void StopPlayer();

        void ToggleSequence(string deviceId, int channel, bool invert, Sequence sequence);

        /// <summary>
        /// Starts the sequence unless one is already running for the given device channel.
        /// </summary>
        void StartSequence(string deviceId, int channel, bool invert, Sequence sequence);

        /// <summary>
        /// Stops the sequence running for the given device channel and resets its output; no-op if none is running.
        /// </summary>
        void StopSequence(string deviceId, int channel);
    }
}
