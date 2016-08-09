using NLog;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    /// <summary>
    /// Base class for all controllers.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class BaseController : Controller
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        public BaseController()
        {
            Logger = LogManager.GetLogger("RedAlert");
        }
    }
}