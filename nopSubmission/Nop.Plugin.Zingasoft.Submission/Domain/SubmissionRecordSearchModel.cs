using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Zingasoft.Submission.Domain
{
    public partial class SubmissionRecordSearchModel : BaseNopModel
    {
        public SubmissionRecordSearchModel()
        {
            AvailableSubmissionTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.EmailLabel")]
        [AllowHtml]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.FullNameLabel")]
        [AllowHtml]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.Admin.Type")]
        public int? SearchSubmissionTypeEnumCode { get; set; }

        public IList<SelectListItem> AvailableSubmissionTypes { get; set; }
    }
}
