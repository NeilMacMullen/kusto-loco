using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public class NullResultRenderingSurface : IResultRenderingSurface
{
    public Task RenderToDisplay(KustoQueryResult result)
    {
        return Task.CompletedTask;
    }
    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        return [];
    }

  
}
