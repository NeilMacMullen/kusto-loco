using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public interface IResultRenderingSurface
{
    Task RenderToSurface(KustoQueryResult result);
    Task<byte[]> GetImage(double pWidth, double pHeight);
}

