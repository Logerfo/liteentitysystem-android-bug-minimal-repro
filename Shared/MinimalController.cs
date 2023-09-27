using System.Diagnostics.CodeAnalysis;
using LiteEntitySystem;

namespace Shared;

public class MinimalController : HumanControllerLogic<PacketInput>
{
    public MinimalController(EntityParams entityParams) : base(entityParams)
    {
    }

    public override void ReadInput(in PacketInput input)
    {
    }

    public override void GenerateInput([UnscopedRef] out PacketInput input)
    {
        input = default;
    }
}