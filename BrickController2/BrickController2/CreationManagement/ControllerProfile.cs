using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace BrickController2.CreationManagement
{
    public class ControllerProfile : NotifyPropertyChangedSource, IShareable
    {
        private string _name;
        private ObservableCollection<ControllerEvent> _controllerEvents = new ObservableCollection<ControllerEvent>();

        [JsonIgnore]
        public static string Type => "bc2p";

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [ForeignKey(typeof(Creation))]
        [JsonIgnore]
        public int CreationId { get; set; }

        [ManyToOne]
        [JsonIgnore]
        public Creation Creation { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerEvent> ControllerEvents
        {
            get { return _controllerEvents; }
            set { _controllerEvents = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
