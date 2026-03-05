namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the type of portfolio exposure measurement.
/// Each type captures a different dimension of portfolio risk and concentration.
/// </summary>
public enum ExposureType
{
    /// <summary>
    /// Net exposure, calculated as the difference between long and short notional values.
    /// Indicates directional market bias.
    /// </summary>
    Net,

    /// <summary>
    /// Gross exposure, calculated as the sum of absolute long and short notional values.
    /// Indicates total market participation regardless of direction.
    /// </summary>
    Gross,

    /// <summary>
    /// Beta-weighted exposure, measuring the portfolio's sensitivity to overall market movements.
    /// </summary>
    Beta,

    /// <summary>
    /// Sector-level exposure, measuring concentration within specific industry sectors.
    /// </summary>
    Sector,

    /// <summary>
    /// Factor-level exposure, measuring sensitivity to systematic risk factors
    /// such as value, momentum, quality, and size.
    /// </summary>
    Factor
}
