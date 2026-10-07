using BrickController2.DeviceManagement.Macros;
using BrickController2.Helpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BrickController2.CreationManagement
{
    public class ControllerAction : NotifyPropertyChangedSource
    {
        private string _deviceId = string.Empty;
        private int _channel;
        private ChannelOutputType _channelOutputType;
        private bool _isInvert;
        private ControllerButtonType _buttonType;
        private ControllerAxisType _axisType;
        private ControllerAxisCharacteristic _axisCharacteristic;
        private int _maxOutputPercent;
        private int _axisActiveZonePercent = 100;
        private int _axisDeadZonePercent;
        private int _maxServoAngle;
        private int _servoBaseAngle;
        private int _stepperAngle;
        private string _sequenceName = string.Empty;
        private string _macroId = string.Empty;
        private MacroChoiceValue _macroChoiceValue;

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerEvent))]
        [JsonIgnore]
        public int ControllerEventId { get; set; }

        [ManyToOne]
        [JsonIgnore]
        public ControllerEvent? ControllerEvent { get; set; }

        public string DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; RaisePropertyChanged(); }
        }

        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        public ChannelOutputType ChannelOutputType
        {
            get { return _channelOutputType; }
            set { _channelOutputType = value; RaisePropertyChanged(); }
        }

        public bool IsInvert
        {
            get { return _isInvert; }
            set { _isInvert = value; RaisePropertyChanged(); }
        }

        public ControllerButtonType ButtonType
        {
            get { return _buttonType; }
            set { _buttonType = value; RaisePropertyChanged(); }
        }

        public ControllerAxisType AxisType
        {
            get { return _axisType; }
            set { _axisType = value; RaisePropertyChanged(); }
        }

        public ControllerAxisCharacteristic AxisCharacteristic
        {
            get { return _axisCharacteristic; }
            set { _axisCharacteristic = value; RaisePropertyChanged(); }
        }

        public int MaxOutputPercent
        {
            get { return _maxOutputPercent; }
            set { _maxOutputPercent = value; RaisePropertyChanged(); }
        }

        public int AxisActiveZonePercent
        {
            get { return _axisActiveZonePercent; }
            set { _axisActiveZonePercent = value != 0 ? value : 100; RaisePropertyChanged(); }
        }

        public int AxisDeadZonePercent
        {
            get { return _axisDeadZonePercent; }
            set { _axisDeadZonePercent = value; RaisePropertyChanged(); }
        }

        public int MaxServoAngle
        {
            get { return _maxServoAngle; }
            set { _maxServoAngle = value; RaisePropertyChanged(); }
        }

        public int ServoBaseAngle
        {
            get { return _servoBaseAngle; }
            set { _servoBaseAngle = value; RaisePropertyChanged(); }
        }

        public int StepperAngle
        {
            get { return _stepperAngle; }
            set { _stepperAngle = value; RaisePropertyChanged(); }
        }

        public string SequenceName
        {
            get { return _sequenceName; }
            set { _sequenceName = value; RaisePropertyChanged(); }
        }

        public string MacroId
        {
            get { return _macroId; }
            set { _macroId = value; RaisePropertyChanged(); }
        }

        [TextBlob(nameof(MacroChoiceBlob))]
        public MacroChoiceValue MacroChoice
        {
            get { return _macroChoiceValue; }
            set { _macroChoiceValue = value; RaisePropertyChanged(); }
        }

        [JsonIgnore]
        public string? MacroChoiceBlob { get; set; }

        [Ignore]
        [JsonIgnore]
        internal bool HasChannel => Channel != NoChannel;

        [Ignore]
        [JsonIgnore]
        internal MacroScope MacroScope => HasChannel ? MacroScope.Channel : MacroScope.Device;

        public override string ToString()
        {
            return HasChannel
                ? $"{DeviceId} - {Channel}"
                : DeviceId;
        }

        public bool IsValidMacro(MacroDescriptor macro) => ButtonType == ControllerButtonType.Macro
            && macro.IsMatch(MacroId, MacroScope, MacroChoice);

        /// <summary>
        /// Determines whether this action is bound to the given device output.
        /// Channel actions are identified by device and channel;
        /// device-level macro actions (<see cref="NoChannel"/>) also by macro id.
        /// </summary>
        public bool IsBoundTo(string deviceId, int channel, string macroId)
            => DeviceId == deviceId &&
                Channel == channel &&
                (HasChannel || MacroId == macroId);

        public const int NoChannel = -1; // special case for device macro
    }
}
