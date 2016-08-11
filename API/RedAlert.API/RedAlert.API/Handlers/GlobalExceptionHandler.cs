using NLog;
using System.Diagnostics;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace RedAlert.API.Handlers
{
    /// <summary>
    /// Exception handler for Web API.
    /// </summary>
    /// <seealso cref="System.Web.Http.ExceptionHandling.ExceptionHandler" />
    public class GlobalExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// When overridden in a derived class, handles the exception synchronously.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        public override void Handle(ExceptionHandlerContext context)
        {
            var logger = LogManager.GetLogger("RedAlert");
            logger.Error(context.Exception);
            Trace.TraceError(ex.ToString());

            context.Result = new InternalServerErrorResult(context.Request);
        }
    }
}