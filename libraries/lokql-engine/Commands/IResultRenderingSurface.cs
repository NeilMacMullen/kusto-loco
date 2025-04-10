using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public interface IResultRenderingSurface
{
    Task RenderToDisplay(KustoQueryResult result);
    byte[] RenderToImage(KustoQueryResult result,double pWidth, double pHeight);
  
}
