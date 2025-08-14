namespace SpaceCapture.Shared.Abstractions;

/// <inheritdoc cref="ICloneable"/>
/// <typeparam name="TSelf"></typeparam>
public interface ICloneable<TSelf> : ICloneable
    where TSelf : notnull
{
    /// <inheritdoc cref="ICloneable.Clone"/>
    new TSelf Clone();

    object ICloneable.Clone() => Clone();
}
