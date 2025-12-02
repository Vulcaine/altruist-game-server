namespace Server.Gameplay;

public sealed class ComboGraph
{
    public string Id { get; }
    public ComboNode Entry { get; }
    public ComboGraphSettings Settings { get; }

    private readonly Dictionary<string, ComboNode> _nodes = new(StringComparer.Ordinal);

    public ComboGraph(string id, ComboNode entry, ComboGraphSettings settings)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));

        RegisterNode(entry);
    }

    public void RegisterNode(ComboNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        _nodes[node.Id] = node;
    }

    public ComboNode? GetNode(string nodeId)
    {
        if (nodeId == null)
            return null;
        _nodes.TryGetValue(nodeId, out var node);
        return node;
    }

    public ComboState CreateState()
        => new ComboState(this, Entry);

    /// <summary>
    /// Simple helper that uses internal state; convenient for your `var t = graph.NextTransition(inputs)` usage.
    /// </summary>
    public ComboTransition? NextTransition(ComboState state, InputSlots input)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        return state.CurrentNode.FindNextTransition(state.TimeInCurrentNode, input);
    }
}

public sealed class ComboState
{
    public ComboGraph Graph { get; }
    public ComboNode CurrentNode { get; private set; }

    /// <summary>Time since we entered CurrentNode (seconds).</summary>
    public float TimeInCurrentNode { get; private set; }

    /// <summary>
    /// Whether damage for the current node has already been applied this frame
    /// (you can extend this if you have multi-hit windows).
    /// </summary>
    public bool DamageAppliedThisFrame { get; set; }

    private float _timeSinceAnyInput;

    internal ComboState(ComboGraph graph, ComboNode entry)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        CurrentNode = entry ?? throw new ArgumentNullException(nameof(entry));
        TimeInCurrentNode = 0f;
        _timeSinceAnyInput = 0f;
    }

    public void Tick(float dt)
    {
        TimeInCurrentNode += dt;
        _timeSinceAnyInput += dt;

        if (Graph.Settings.ResetToEntryAfterSeconds > 0f &&
            _timeSinceAnyInput >= Graph.Settings.ResetToEntryAfterSeconds)
        {
            ResetToEntry();
        }
    }

    public void RegisterInput(InputSlots input)
    {
        if (input != InputSlots.None)
            _timeSinceAnyInput = 0f;
    }

    public void ApplyTransition(ComboTransition transition)
    {
        if (transition == null)
            throw new ArgumentNullException(nameof(transition));

        CurrentNode = transition.Target;
        TimeInCurrentNode = 0f;
        DamageAppliedThisFrame = false;
    }

    public void ResetToEntry()
    {
        CurrentNode = Graph.Entry;
        TimeInCurrentNode = 0f;
        DamageAppliedThisFrame = false;
    }
}
