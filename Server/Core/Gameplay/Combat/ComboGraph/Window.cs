namespace Server.Gameplay;

public readonly struct TimeWindow
{
    public float Start { get; }
    public float End { get; }

    public TimeWindow(float start, float end)
    {
        if (end < start)
            throw new ArgumentException("End must be >= Start.", nameof(end));

        Start = start;
        End = end;
    }

    public bool Contains(float t) => t >= Start && t <= End;
    public bool IsEmpty => End <= Start;
}

public readonly struct DamageWindow
{
    public TimeWindow Window { get; }

    public DamageWindow(TimeWindow window)
    {
        Window = window;
    }

    public bool IsActive(float t) => Window.Contains(t);
}

public readonly struct InputWindow
{
    public TimeWindow Window { get; }

    public InputWindow(TimeWindow window)
    {
        Window = window;
    }

    public bool IsOpen(float t) => Window.Contains(t);
}

public readonly struct CancelWindow
{
    public TimeWindow Window { get; }

    public CancelWindow(TimeWindow window)
    {
        Window = window;
    }

    public bool CanCancel(float t) => Window.Contains(t);
}

public sealed class ComboTransition
{
    /// <summary>Unique id for debugging / tooling.</summary>
    public string Id { get; }

    /// <summary>Node you are transitioning to.</summary>
    public ComboNode Target { get; }

    /// <summary>Input required to trigger this transition (e.g. E, Ctrl+W, etc).</summary>
    public InputSlots RequiredInput { get; }

    /// <summary>
    /// Optional per-transition input window.
    /// If null, the node's default input window is used.
    /// </summary>
    public InputWindow? InputWindowOverride { get; }

    public ComboTransition(
        string id,
        ComboNode target,
        InputSlots requiredInput,
        InputWindow? inputWindowOverride = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        RequiredInput = requiredInput;
        InputWindowOverride = inputWindowOverride;
    }

    public bool IsInputSatisfied(InputSlots currentInput)
        => (currentInput & RequiredInput) == RequiredInput;

    public bool IsWindowOpen(float timeInNode, InputWindow defaultWindow)
    {
        var win = InputWindowOverride ?? defaultWindow;
        return win.IsOpen(timeInNode);
    }
}

public sealed class ComboNode
{
    /// <summary>Unique node id, e.g. "light_1", "dash_start", "up_slash".</summary>
    public string Id { get; }

    /// <summary>
    /// Reference to your AttackMoveVault row.
    /// Could be StorageId or some logical key.
    /// </summary>
    public string AttackMoveId { get; }

    /// <summary>
    /// When damage is applied during this node (can be multiple hits).
    /// Times are relative to node start (seconds).
    /// </summary>
    public IReadOnlyList<DamageWindow> DamageWindows { get; }

    /// <summary>
    /// When next attack input is accepted.
    /// </summary>
    public InputWindow InputWindow { get; }

    /// <summary>
    /// When this node can be cancelled into something else (dodge, block, etc.).
    /// </summary>
    public CancelWindow CancelWindow { get; }

    /// <summary>Outgoing transitions.</summary>
    public IReadOnlyList<ComboTransition> Transitions => _transitions;

    private readonly List<ComboTransition> _transitions = new();

    public ComboNode(
        string id,
        string attackMoveId,
        IEnumerable<DamageWindow> damageWindows,
        InputWindow inputWindow,
        CancelWindow cancelWindow)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        AttackMoveId = attackMoveId ?? throw new ArgumentNullException(nameof(attackMoveId));
        DamageWindows = (damageWindows ?? Array.Empty<DamageWindow>()).ToArray();
        InputWindow = inputWindow;
        CancelWindow = cancelWindow;
    }

    public void AddTransition(ComboTransition transition)
    {
        if (transition == null)
            throw new ArgumentNullException(nameof(transition));
        _transitions.Add(transition);
    }

    /// <summary>
    /// Find the first valid transition given current time in this node and current input.
    /// If multiple are valid, the first added wins (you can add priority logic later).
    /// </summary>
    public ComboTransition? FindNextTransition(float timeInNode, InputSlots input)
    {
        foreach (var t in _transitions)
        {
            if (!t.IsInputSatisfied(input))
                continue;

            if (!t.IsWindowOpen(timeInNode, InputWindow))
                continue;

            return t;
        }

        return null;
    }

    public bool IsDamageActive(float timeInNode)
        => DamageWindows.Any(dw => dw.IsActive(timeInNode));

    public bool CanCancel(float timeInNode)
        => CancelWindow.CanCancel(timeInNode);
}

public sealed class ComboGraphSettings
{
    /// <summary>
    /// If no valid transition is taken and TimeInState exceeds this,
    /// reset back to Entry node.
    /// </summary>
    public float ResetToEntryAfterSeconds { get; }

    public ComboGraphSettings(float resetToEntryAfterSeconds)
    {
        ResetToEntryAfterSeconds = Math.Max(0f, resetToEntryAfterSeconds);
    }
}
