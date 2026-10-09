namespace MPC.Tooling;

public enum ToolHand
{
    Neutral = 0,
    Left = 1,
    Right = 2
}

public sealed record CuttingGrade
{
    public string ManufacturerDesignation { get; }

    public string Substrate { get; }

    public string? CoatingProcess { get; }

    public string? Coating { get; }

    public CuttingGrade(
        string manufacturerDesignation,
        string substrate,
        string? coatingProcess = null,
        string? coating = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manufacturerDesignation);
        ArgumentException.ThrowIfNullOrWhiteSpace(substrate);

        ManufacturerDesignation = manufacturerDesignation.Trim();
        Substrate = substrate.Trim();
        CoatingProcess = NormalizeOptional(coatingProcess);
        Coating = NormalizeOptional(coating);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record CuttingInsert
{
    public ToolProductIdentity Identity { get; }

    public ToolInterface Interface { get; }

    public CuttingGrade Grade { get; }

    public string? ChipBreakerDesignation { get; }

    public ToolHand Hand { get; }

    public CuttingInsert(
        ToolProductIdentity identity,
        ToolInterface @interface,
        CuttingGrade grade,
        string? chipBreakerDesignation,
        ToolHand hand)
    {
        if (!Enum.IsDefined(hand))
        {
            throw new ArgumentOutOfRangeException(
                nameof(hand),
                hand,
                "Insert hand is not defined.");
        }

        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Interface = @interface ?? throw new ArgumentNullException(nameof(@interface));
        Grade = grade ?? throw new ArgumentNullException(nameof(grade));
        ChipBreakerDesignation = string.IsNullOrWhiteSpace(chipBreakerDesignation)
            ? null
            : chipBreakerDesignation.Trim();
        Hand = hand;
    }
}

public sealed record ToolHolder
{
    public ToolProductIdentity Identity { get; }

    public ToolInterface RequiredInsertInterface { get; }

    public string MasterInsertIdentification { get; }

    public ToolHand Hand { get; }

    public ToolHolder(
        ToolProductIdentity identity,
        ToolInterface requiredInsertInterface,
        string masterInsertIdentification,
        ToolHand hand)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(masterInsertIdentification);

        if (!Enum.IsDefined(hand))
        {
            throw new ArgumentOutOfRangeException(
                nameof(hand),
                hand,
                "Holder hand is not defined.");
        }

        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        RequiredInsertInterface = requiredInsertInterface ??
            throw new ArgumentNullException(nameof(requiredInsertInterface));
        MasterInsertIdentification = masterInsertIdentification.Trim();
        Hand = hand;
    }
}
