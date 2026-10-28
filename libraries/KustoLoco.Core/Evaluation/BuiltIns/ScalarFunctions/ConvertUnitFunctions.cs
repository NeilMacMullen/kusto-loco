namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ConvertAngle")]
public partial class ConvertAngleFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Angle, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertEnergy")]
public partial class ConvertEnergyFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Energy, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertForce")]
public partial class ConvertForceFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Force, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertLength")]
public partial class ConvertLengthFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Length, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertMass")]
public partial class ConvertMassFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Mass, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertSpeed")]
public partial class ConvertSpeedFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Speed, value, from, to);
}

[KustoImplementation(Keyword = "Functions.ConvertVolume")]
public partial class ConvertVolumeFunction
{
    public double? Impl(double value, string from, string to)
        => UnitConversionHelper.Convert(UnitConversionHelper.Volume, value, from, to);
}
