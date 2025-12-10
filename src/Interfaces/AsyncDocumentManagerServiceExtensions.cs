using System;
using System.Linq;
using System.Threading.Tasks;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAsyncDocumentManagerService"/>.
    /// </summary>
    public static class AsyncDocumentManagerServiceExtensions
    {
        /// <summary>
        /// Finds an asynchronous document by its ID.
        /// </summary>
        /// <param name="service">The document manager service to search in.</param>
        /// <param name="id">The ID of the document to find.</param>
        /// <returns>The found asynchronous document, or <c>null</c> if no document with the specified ID is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public static IAsyncDocument? FindDocumentById(this IAsyncDocumentManagerService service, object id)
        {
            Throw.IfNull(service);
            return service.Documents.FirstOrDefault(x => Equals(x.Id, id));
        }

        /// <summary>
        /// Finds an asynchronous document by its ID, or creates a new document if it does not exist.
        /// </summary>
        /// <param name="service">The document manager service to search in.</param>
        /// <param name="id">The ID of the document to find or create.</param>
        /// <param name="createDocumentCallback">The callback function to create a new document if none is found.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the found or newly created asynchronous document.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="createDocumentCallback"/> is null.</exception>
        public static async ValueTask<IAsyncDocument> FindDocumentByIdOrCreateAsync(this IAsyncDocumentManagerService service, object id, Func<IAsyncDocumentManagerService, ValueTask<IAsyncDocument>> createDocumentCallback)
        {
            Throw.IfNull(service);
            Throw.IfNull(createDocumentCallback);
            IAsyncDocument? document = service.FindDocumentById(id);
            if (document == null)
            {
                document = await createDocumentCallback(service).ConfigureAwait(false);
                document.Id = id;
            }
            return document;
        }
    }
}
