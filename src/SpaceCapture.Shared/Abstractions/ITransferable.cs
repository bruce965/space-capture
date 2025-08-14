namespace SpaceCapture.Shared.Abstractions;

/// <summary>
/// Supports copying data from another object, avoiding allocations when possible.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ITransferable<TSelf>
    where TSelf : notnull
{
    void CopyFrom(TSelf other);
}
