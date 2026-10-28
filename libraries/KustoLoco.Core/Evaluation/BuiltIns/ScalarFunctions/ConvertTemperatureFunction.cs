using System;
using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class TemperatureConversionHelper
{
	private static readonly Dictionary<string, (Func<double, double> ToKelvin, Func<double, double> FromKelvin)>
		Units = new(StringComparer.OrdinalIgnoreCase)
		{
			["Kelvin"] = (v => v, k => k),
			["DegreeCelsius"] = (v => v + 273.15, k => k - 273.15),
			["MillidegreeCelsius"] = (v => v / 1000 + 273.15, k => (k - 273.15) * 1000),
			["DegreeFahrenheit"] = (v => (v + 459.67) * 5 / 9, k => k * 9 / 5 - 459.67),
			["DegreeRankine"] = (v => v * 5 / 9, k => k * 9 / 5),
			["DegreeDelisle"] = (v => 373.15 - v * 2 / 3, k => (373.15 - k) * 3 / 2),
			["DegreeNewton"] = (v => v * 100 / 33 + 273.15, k => (k - 273.15) * 33 / 100),
			["DegreeReaumur"] = (v => v * 5 / 4 + 273.15, k => (k - 273.15) * 4 / 5),
			["DegreeRoemer"] = (v => (v - 7.5) * 40 / 21 + 273.15, k => (k - 273.15) * 21 / 40 + 7.5),
			["SolarTemperature"] = (v => v * 5778, k => k / 5778),
		};

	public static double? Convert(double value, string from, string to)
	{
		if (from is null || to is null)
			return null;
		if (!Units.TryGetValue(from.Trim(), out var f) || !Units.TryGetValue(to.Trim(), out var t))
			return null;
		return t.FromKelvin(f.ToKelvin(value));
	}
}

[KustoImplementation(Keyword = "Functions.ConvertTemperature")]
public partial class ConvertTemperatureFunction
{
	public double? Impl(double value, string from, string to)
		=> TemperatureConversionHelper.Convert(value, from, to);
}
