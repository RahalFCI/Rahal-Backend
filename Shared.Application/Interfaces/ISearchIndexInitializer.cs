namespace Shared.Application.Interfaces
{
    public interface ISearchIndexInitializer
    {
        string IndexName { get; }

        Task ConfigureAsync(object client, CancellationToken cancellationToken = default);
    }
}
