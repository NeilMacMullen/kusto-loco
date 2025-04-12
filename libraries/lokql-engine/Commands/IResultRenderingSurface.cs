using KustoLoco.Core;

namespace Lokql.Engine.Commands;

/// <summary>
/// Interface to describe a rendering surface.
/// </summary>
/// <remarks>
/// THis is intended to provide some common interface that allows us to perform
/// rendering operations both in the WPF UI and the CLI application
/// </remarks>
public interface IResultRenderingSurface
{
    Task RenderToDisplay(KustoQueryResult result);
    byte[] RenderToImage(KustoQueryResult result,double pWidth, double pHeight);
  
}
