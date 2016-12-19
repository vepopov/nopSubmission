using FluentValidation.Attributes;
using Nop.Core;
using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Zingasoft.Submission.Domain
{
    [Validator(typeof(SubmissionRecordValidator))]
    public class SubmissionRecord: BaseEntity
    {
        public int SubmissionRecordId { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.EmailLabel")]
        public string Email { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.FullNameLabel")]
        public string FullName { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.SubjectLabel")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.PhoneLabel")]
        public string Phone { get; set; }

        [NopResourceDisplayName("Plugins.Zingasoft.Submission.EnquiryLabel")]
        public string Enquiry { get; set; }

        /// <summary>
        /// Correspodns to the SubmissionType enumeration
        /// </summary>
        public int RecordType { get; set; }

        public string DownloadLink { get; set; }

        public bool SuccessfullySaved { get; set; } = false;

        public string SaveResult { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
