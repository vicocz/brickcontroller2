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
                        var deviceId = controllerAction.DeviceId;
                        deviceIds.Add(deviceId);
                    }
                }
            }

            return deviceIds;
        }

        public IReadOnlySet<string> GetSequenceNames()
        {
            var sequenceNames = new HashSet<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        if (controllerAction.ButtonType == ControllerButtonType.Sequence)
                        {
                            var sequenceName = controllerAction.SequenceName;
                            sequenceNames.Add(sequenceName);
                        }
                    }
                }
            }

            return sequenceNames;
        }
    }
}
