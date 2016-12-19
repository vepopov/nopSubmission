using Nop.Plugin.Zingasoft.Submission.Infrastructure;
using Nop.Web.Framework.Mvc.Routes;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Zingasoft.Submission
{
    public class RouteConfig : IRouteProvider
    {
        public int Priority
        {
            get
            {
                return 0;
            }
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "Plugin.Zingasoft.Submission.ManageSubmission",
                "Submission/Manage",
                new { controller = "Submission", action = "Manage"},
                new[] {"Nop.Plugins.Zingasoft.Submission.Controllers"}
                );

            routes.MapRoute(
                "Plugin.Zingasoft.Submission.SubmissionList",
                "Submission/SubmissionList",
                new { controller = "Submission", action = "SubmissionList" },
                new[] { "Nop.Plugins.Zingasoft.Submission.Controllers" }
                );

            routes.MapRoute(
                "Plugin.Zingasoft.Submission.SubmissionUpdate",
                "Submission/SubmissionUpdate",
                new { controller = "Submission", action = "SubmissionUpdate" },
                new[] { "Nop.Plugins.Zingasoft.Submission.Controllers" }
                );

            routes.MapRoute(
                "Plugin.Zingasoft.Submission.SubmissionDelete",
                "Submission/SubmissionDelete",
                new { controller = "Submission", action = "SubmissionDelete" },
                new[] { "Nop.Plugins.Zingasoft.Submission.Controllers" }
                );

            routes.MapRoute(
                "Plugin.Zingasoft.Submission.NewSubmission",
                "Submission/NewEntry",
                new { controller = "Submission", action = "NewEntry" },
                new[] { "Nop.Plugins.Zingasoft.Submission.Controllers" }
                );

            routes.MapRoute(
                "Plugin.Zingasoft.Submission.SaveSettings",
                "Submission/SaveSettings",
                new { controller = "Submission", action = "SaveSettings" },
                new[] { "Nop.Plugins.Zingasoft.Submission.Controllers" }
                );

            ViewEngines.Engines.Insert(0, new CustomViewEngine());
        }
    }
}
