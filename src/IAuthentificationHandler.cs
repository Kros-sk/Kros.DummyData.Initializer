using Flurl.Http;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Interface, which describe autorization handler.
    /// </summary>
    public interface IAuthentificationHandler
    {
        /// <summary>
        /// Handles.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        Task HandleAsync(IFlurlRequest httpRequest, Request request, InitializerContext context);
    }
}
