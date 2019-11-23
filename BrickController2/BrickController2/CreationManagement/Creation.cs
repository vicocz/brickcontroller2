using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrickController2.CreationManagement
{
    public class Creation : NotifyPropertyChangedSource
    {
        private string _name;
        private ObservableCollection<ControllerProfile> _controllerProfiles = new ObservableCollection<ControllerProfile>();

        [PrimaryKey, AutoIncrement]
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

        public override string ToString()
        {
            return Name;
        }

        public Creation Clone()
        {
            var copy = new Creation
            {
                Name = _name
            };
            foreach (var profile in _controllerProfiles ?? Enumerable.Empty<ControllerProfile>())
            {
                copy._controllerProfiles.Add(profile.Clone());
            }

            return copy;
        }

        public IEnumerable<string> GetDeviceIds()
        {
            var deviceIds = new List<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        var deviceId = controllerAction.DeviceId;
                        if (!deviceIds.Contains(deviceId))
                        {
                            deviceIds.Add(deviceId);
                        }
                    }
                }
            }

            return deviceIds;
        }
    }
}
