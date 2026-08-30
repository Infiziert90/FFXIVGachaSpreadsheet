namespace SupabaseExporter.Structures.Exports;

public class LeveIssuer
{
    /// <summary>
    /// GuildleveAssignment RowId
    /// </summary>
    public uint GuildleveAssignmentId;

    /// <summary>
    /// ENpcBase RowId
    /// </summary>
    public uint ENpcBaseId;

    /// <summary>
    /// Level RowId
    /// </summary>
    public uint LevelId;

    /// <summary>
    /// Key: GuildleveAssignmentCategory RowId
    /// </summary>
    public Dictionary<uint, LeveAssignmentCategory> Categories = [];
}

public class LeveAssignmentCategory
{
    /// <summary>
    /// GuildleveAssignmentCategory RowId
    /// </summary>
    public uint CategoryId;

    /// <summary>
    /// Key: GuildleveAssignmentCategory.Category Index
    /// </summary>
    public Dictionary<uint, LeveAssignmentCategoryType> Types = [];
}

public class LeveAssignmentCategoryType
{
    /// <summary>
    /// GuildleveAssignmentCategory.Category Index
    /// </summary>
    public uint CategoryIndex;

    /// <summary>
    /// Leve RowIds
    /// </summary>
    public HashSet<ushort> LeveIds = [];
}
