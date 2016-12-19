using Nop.Core.Data;
using Nop.Core.Plugins;
using Nop.Plugin.Zingasoft.Submission.Data;
using Nop.Plugin.Zingasoft.Submission.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Zingasoft.Submission
{
    public class SubmissionPlugin : BasePlugin, IAdminMenuPlugin
    {
        private SubmissionRecordObjectContext _context;
        private IRepository<SubmissionRecord> _submissionReop;
        private ISettingService _commonSettings;
        private IDownloadService _downloadService;
        private ILocalizationService _localizationService;

        public SubmissionPlugin(SubmissionRecordObjectContext context, IRepository<SubmissionRecord> submissionReop, ISettingService commonSettings, 
            IDownloadService downloadService, ILocalizationService localizationService)
        {
            _context = context;
            _submissionReop = submissionReop;
            _commonSettings = commonSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
        }
        public bool Authenticate()
        {
            return true;
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            SiteMapNode node = new SiteMapNode
            {
                Visible = true,
                Title = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.MenuTitle"),
                Url = "/Submission/Manage"
            };

            rootNode.ChildNodes.Add(node);
        }

        public override void Install()
        {
            _context.Install();

            _commonSettings.SaveSetting(new SubmissionAdminSettings
            {
                SubmissionFileMaxSizeInMB = 3,
                SubmissionAllowedFiletypes = ".pdf, .doc, .docx, .ppt, .pptx, .jpg, .jpeg, .png"
            });

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.MenuTitle", "Submission Form");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameLabel", "Full Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameLabel.Hint", "Please, provide your full name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameRequired", "You need to provide your full name!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameTooLong", 
                String.Format("Your full name line cannot exceed {0} characters!", Constants.FullNameMaxLength));

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EmailLabel", "Email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EmailLabel.Hint", "Please, provide your email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EmailRequired", "You need to provide your email!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EmailFormat", "You need to provide a valid email address!");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectLabel", "Subject");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectLabel.Hint", "Please, provide subject");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectRequired", "You need to provide subject!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectTooLong",
                 String.Format("Your subject line cannot exceed {0} characters!", Constants.SubjectMaxLength));

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryPlaceholder", "Pleace write with cyrillic letters...");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryLabel", "Enquiry");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryLabel.Hint", "Please, provide detailed enquiry");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryRequired", "You need to provide detailed enquiry!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryTooLong",
                 String.Format("Your enquiry line cannot exceed {0} characters!", Constants.EnquiryMaxLength));

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneLabel", "Phone");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneLabel.Hint", "Please, provide your phone number.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneTooLong",
                 String.Format("Your phone number cannot exceed {0} characters!", Constants.PhoneMaxLength));

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionHeader", "Submission Form Records");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionSettings", "Submission Form Settings");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionFilter", "Submission Form Filter");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.MaxFileSize", "Max file size in MB");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.AllowedFiletypes", "Allowed filetypes");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Save", "Save");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.FullName", "Full Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Subject", "Subject");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Phone", "Phone");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Enquiry", "Enquiry");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Type", "Type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.DownloadLink", "Download Link");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Attachment", "Attachment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.JobApplication", "Job Application");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.PartnerOffer", "Partner Offer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.VendorOffer", "Vendor Offer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Other", "Other");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.Title", "Submission Form");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.AttachFileHint", "Attach a file (max filesize in MB: {0})");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.Send", "Send");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Error.MaxSizeExeeded", "Submitted file exeeds the maximum allowed file size of {0} MB");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Error.InvalidFiletype", "Submitted file type is not allowed. Allowed file types are {0}");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Info.SubmissionSucceeded", "Your information was successfully submitted!");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.InvalidMaxSizeNum", "Invalid number for maximum file size!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.NonpositiveMaxFileSize", "Max file size cannot be less than or equal to 0!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.InvalidFiletype", "Invalid filetype {0}!");

            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.EnquiryEmailSubscriber", "Email to be notified upon recieveing submission form of type enquiry");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.JobApplicationEmailSubscriber", "Email to be notified upon recieveing submission form of type job application");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.ParnterOfferEmailSubscriber", "Email to be notified upon recieveing submission form of type partner offer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.VendorOfferEmailSubscriber", "Email to be notified upon recieveing submission form of type vendor offer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.OtherEmailSubscriber", "Email to be notified upon recieveing submission form of type other");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail", "You have entered an invalid email in one of the above fields!");
            this.AddOrUpdatePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.EmailNotificationIntro", "A new submission form was receieved in the system. Details are below.");

            base.Install();
        }

        public override void Uninstall()
        {
            //Delete all attached files before removing Submission table from DB
            List<SubmissionRecord> records = _submissionReop.Table.ToList();
            foreach (var item in records)
            {
                if (String.IsNullOrEmpty(item.DownloadLink) == false)
                {
                    int startIndex = item.DownloadLink.IndexOf("downloadId=");
                    int length = "downloadId=".Length;

                    string downloadGuid = item.DownloadLink.Substring(startIndex + length);

                    var download = _downloadService.GetDownloadByGuid(new Guid(downloadGuid));
                    _downloadService.DeleteDownload(download);
                }
            }

            _context.Uninstall();

            _commonSettings.DeleteSetting<SubmissionAdminSettings>();

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.MenuTitle");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameLabel");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameRequired");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.FullNameTooLong");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EmailLabel");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EmailLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EmailRequired");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EmailFormat");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectLabel");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectRequired");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.SubjectTooLong");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryPlaceholder");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryLabel");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryRequired");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.EnquiryTooLong");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneLabel");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.PhoneTooLong");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionHeader");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionSettings");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.SubmissionFilter");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.MaxFileSize");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.AllowedFiletypes");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Save");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.FullName");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Subject");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Phone");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Enquiry");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Type");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.DownloadLink");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Attachment");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.JobApplication");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.PartnerOffer");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.VendorOffer");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Other");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.Title");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.AttachFileHint");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.NewEntry.Send");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Error.MaxSizeExeeded");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Error.InvalidFiletype");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Info.SubmissionSucceeded");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.InvalidMaxSizeNum");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.NonpositiveMaxFileSize");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.Error.InvalidFiletype");

            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.EnquiryEmailSubscriber");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.JobApplicationEmailSubscriber");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.ParnterOfferEmailSubscriber");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.VendorOfferEmailSubscriber");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.OtherEmailSubscriber");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail");
            this.DeletePluginLocaleResource("Plugins.Zingasoft.Submission.Admin.EmailNotificationIntro");

            base.Uninstall();
        }
    }
}
