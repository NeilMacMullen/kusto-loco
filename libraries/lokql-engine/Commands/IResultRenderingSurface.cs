using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public interface IResultRenderingSurface
{
    Task RenderToDisplay(KustoQueryResult result);
    Task<byte[]> RenderToImage(KustoQueryResult result,double pWidth, double pHeight);
}
