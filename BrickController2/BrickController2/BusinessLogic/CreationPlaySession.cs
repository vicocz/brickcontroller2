using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.CreationManagement;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;

namespace BrickController2.BusinessLogic;

/// <summary>One player screen owns independent control state and sequence players for each creation.</summary>
public sealed class CreationPlaySession
{
    private readonly IInputDeviceManagerService _inputs;
    private readonly Func<IPlayLogic> _createPlayer;
    private readonly object _gate = new();
    private readonly List<Binding> _bindings = [];
    private readonly HashSet<string> _ambiguousAssignments = [];
    private bool _running;
    private bool _listening;

    public CreationPlaySession(IInputDeviceManagerService inputs, Func<IPlayLogic> createPlayer)
    {
        _inputs = inputs;
        _createPlayer = createPlayer;
    }

    public event EventHandler? Changed;
    public IReadOnlyList<Creation> Creations => _bindings.Select(b => b.Creation).ToArray();
    public string GetStatus(Func<string, string> translate)
    {
        lock (_gate)
            return string.Join("\n", _bindings.Select(b =>
                $"{b.Creation.Name}: {b.ControllerName ?? b.Creation.ControllerAssignmentName ?? translate("AnyController")} — " +
                translate(b.Creation.ControllerAssignmentId == "none" ? "ControllerDisabled" :
                b.Creation.ControllerAssignmentId == null || b.RuntimeId != null ? "ControllerReady" : "ControllerUnavailable")));
    }

    public void ChangeProfile(Creation creation, ControllerProfile profile)
    {
        lock (_gate)
        {
            var b = _bindings.Single(b => b.Creation == creation);
            b.Player.StopPlay();
            b.Player.ActiveProfile = profile;
            if (_running && (b.RuntimeId != null || creation.ControllerAssignmentId == null)) b.Player.StartPlay();
        }
    }

    public void Configure(IEnumerable<Creation> creations, Creation primary, IPlayLogic primaryPlayer)
    {
        _bindings.Clear();
        _ambiguousAssignments.Clear();
        foreach (var creation in creations)
        {
            var player = creation == primary ? primaryPlayer : _createPlayer();
            player.ActiveProfile ??= creation.ControllerProfiles.First();
            player.UseCreationControllerAssignment = creation.ControllerAssignmentId != null;
            _bindings.Add(new Binding(creation, player));
        }
    }

    public void Listen()
    {
        if (_listening) return;
        _listening = true;
        _inputs.InputDevicesChangedEvent += DevicesChanged;
        _inputs.InputDeviceEvent += Input;
        Refresh();
    }

    public void Close()
    {
        if (!_listening) return;
        _inputs.InputDevicesChangedEvent -= DevicesChanged;
        _inputs.InputDeviceEvent -= Input;
        _listening = false;
        Stop();
    }

    public void Start()
    {
        lock (_gate)
        {
            if (_running) return;
            _running = true;
            foreach (var b in _bindings)
                if (b.RuntimeId != null || b.Creation.ControllerAssignmentId == null) b.Player.StartPlay();
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            _running = false;
            foreach (var b in _bindings) b.Player.StopPlay();
        }
    }

    /// <summary>Never guess when two connected devices have the same descriptor.</summary>
    public static string? Resolve(string? assignmentId, IEnumerable<IInputDevice> devices)
    {
        if (string.IsNullOrWhiteSpace(assignmentId) || assignmentId == "none") return null;
        var matches = devices.Where(d => !string.IsNullOrWhiteSpace(d.RuntimeId) &&
            (d.AssignmentId == assignmentId || d.RuntimeId == assignmentId)).ToArray();
        return matches.Length == 1 ? matches[0].RuntimeId : null;
    }

    private void Refresh()
    {
        // Snapshot before taking our lock: device notifications hold the input manager lock.
        var devices = _inputs.GetInputDevices();
        lock (_gate)
        {
            foreach (var group in devices.Where(d => d.AssignmentId != null).GroupBy(d => d.AssignmentId!))
                if (group.Count() > 1) _ambiguousAssignments.Add(group.Key);
            var resolved = _bindings.ToDictionary(b => b, b =>
                b.Creation.ControllerAssignmentId != null && _ambiguousAssignments.Contains(b.Creation.ControllerAssignmentId)
                    ? null : Resolve(b.Creation.ControllerAssignmentId, devices));
            foreach (var b in _bindings)
            {
                var runtimeId = resolved[b];
                if (runtimeId != null && resolved.Values.Count(id => id == runtimeId) > 1) runtimeId = null;
                b.ControllerName = devices.FirstOrDefault(d => d.RuntimeId == runtimeId)?.Name;
                if (b.RuntimeId == runtimeId) continue;
                b.Player.StopPlay();
                b.RuntimeId = runtimeId;
                if (_running && runtimeId != null) b.Player.StartPlay();
            }
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void DevicesChanged(object? sender, InputDeviceChangedEventArgs e) => Refresh();

    private void Input(object? sender, InputDeviceEventArgs e)
    {
        lock (_gate)
        {
            if (!_running) return;
            foreach (var b in _bindings)
                if (b.RuntimeId == e.RuntimeId ||
                    (_bindings.Count == 1 && b.Creation.ControllerAssignmentId == null))
                    b.Player.ProcessGameControllerEvent(e);
        }
    }

    private sealed class Binding(Creation creation, IPlayLogic player)
    {
        public Creation Creation { get; } = creation;
        public IPlayLogic Player { get; } = player;
        public string? RuntimeId { get; set; }
        public string? ControllerName { get; set; }
    }
}
