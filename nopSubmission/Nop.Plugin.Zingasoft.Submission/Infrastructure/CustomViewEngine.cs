using Nop.Web.Framework.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Zingasoft.Submission.Infrastructure
{
    public class CustomViewEngine: ThemeableRazorViewEngine
    {
        public CustomViewEngine()
        {
            ViewLocationFormats = new[] { "~/Plugins/Zingasoft.Submission/Views/{0}.cshtml" };
            PartialViewLocationFormats = new[] { "~/Plugins/Zingasoft.Submission/Views/{0}.cshtml" };
        }
    }
}
