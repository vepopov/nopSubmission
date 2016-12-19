using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Zingasoft.Submission.Domain
{
    public class SubmissionAdminSettings: ISettings
    {
        /// <summary>
        /// Max file size (in MB) allowed for the submission form attachments
        /// </summary>
        public int SubmissionFileMaxSizeInMB { get; set; }

        /// <summary>
        /// File types allowed for the submission form attachments
        /// </summary>
        public string SubmissionAllowedFiletypes { get; set; }

        public string EnquiryEmail { get; set; }

        public string JobApplicationEmail { get; set; }

        public string PartnerOfferEmail { get; set; }

        public string VendorOfferEmail { get; set; }

        public string OtherEmail { get; set; }

    }
}
