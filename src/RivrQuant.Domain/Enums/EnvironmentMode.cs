namespace RivrQuant.Domain.Enums;

/// <summary>
/// Represents the execution environment mode, controlling whether trades
/// are simulated or executed with real capital.
/// </summary>
public enum EnvironmentMode
{
    /// <summary>
    /// Paper trading mode using simulated funds for strategy testing
    /// without financial risk.
    /// </summary>
    Paper,

    /// <summary>
    /// Live testnet mode executing against a broker's sandbox or testnet environment
    /// with simulated market conditions but real API connectivity.
    /// </summary>
    LiveTestnet,

    /// <summary>
    /// Live production mode executing real trades with real capital
    /// against production broker endpoints.
    /// </summary>
    LiveProduction
}
