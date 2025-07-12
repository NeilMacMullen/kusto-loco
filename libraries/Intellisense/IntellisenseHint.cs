namespace Intellisense;

/// <summary>
/// Visualisation Hints for the Completion Window
/// </summary>
/// <remarks>
/// It's important not to change the names of these as the current
/// mapping scheme assumes that the enum name is the same as an
/// svg file in the Assets/ComponentIcons folder
/// </remarks>
public enum IntellisenseHint
{
    None,
    Operator,
    Function,
    Table,
    Column,
    Command,
    File,
    Folder = 100000000,
    Csv,
    Tsv,
    Parquet,
    Txt,
    Json,
    Xls,
    Xlsx,
    Pptx,
    Html,
    Ppt,
    Xlsb,
    Dfr,
    Csl,

}
