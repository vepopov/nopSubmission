using Nop.Plugin.Zingasoft.Submission.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Zingasoft.Submission.Data
{
    public class SubmissionRecordMap: EntityTypeConfiguration<SubmissionRecord>
    {
        public SubmissionRecordMap()
        {
            ToTable("Submission");

            HasKey(m => m.SubmissionRecordId);

            Property(m => m.Email);
            Property(m => m.FullName);
            Property(m => m.Subject);
            Property(m => m.Phone);
            Property(m => m.Enquiry);            
            Property(m => m.RecordType);
            Property(m => m.DownloadLink);

            Ignore(m => m.SuccessfullySaved);
            Ignore(m => m.SaveResult);
            Ignore(m => m.DisplayCaptcha);
        }
    }
}
