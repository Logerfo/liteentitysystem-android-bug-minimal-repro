using LiteEntitySystem;
using LiteEntitySystem.Extensions;

namespace Shared;

public class MinimalPawn : PawnLogic
{
    public SyncString RandomString { get; } = new();

    public MinimalPawn(EntityParams entityParams) : base(entityParams)
    {
    }
}