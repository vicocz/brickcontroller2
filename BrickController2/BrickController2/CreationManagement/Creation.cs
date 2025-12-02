using BrickController2.BusinessLogic;
using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class Creation : NotifyPropertyChangedSource, IShareable
    {
        private string _name = string.Empty;
        private ObservableCollection<ControllerProfile> _controllerProfiles = new ObservableCollection<ControllerProfile>();
        private CreationValidationResult _lastValidation;

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerProfile> ControllerProfiles
        {
            get { return _controllerProfiles; }
            set { _controllerProfiles = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Keeps track of the last validation result for this creation.
        /// </summary>
        [JsonIgnore]
        public CreationValidationResult ValidationResult
        {
            get { return _lastValidation; }
            set
            {
                if (_lastValidation != value)
                {
                    _lastValidation = value;
                    RaisePropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public static string Type => "bc2c";

        public override string ToString()
        {
            return Name;
        }

        public IReadOnlySet<string> GetDeviceIds()
        {
            var deviceIds = new HashSet<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        deviceIds.Add(controllerAction.DeviceId);
                    }
                }
            }

            return deviceIds;
        }

        public IEnumerable<string> GetSequenceNames()
        {
            var sequenceNames = new List<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        if (controllerAction.ButtonType == ControllerButtonType.Sequence)
                        {
                            var sequenceName = controllerAction.SequenceName;
                            if (!sequenceNames.Contains(sequenceName))
                            {
                                sequenceNames.Add(sequenceName);
                            }
                        }
                    }
                }
            }

            return sequenceNames;
        }
    }
}
