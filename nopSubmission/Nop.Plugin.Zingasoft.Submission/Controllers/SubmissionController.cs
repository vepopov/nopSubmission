using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Plugin.Zingasoft.Submission.Domain;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security.Captcha;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Nop.Plugin.Zingasoft.Submission.Controllers
{
    public class SubmissionController : BasePluginController
    {
        private IRepository<SubmissionRecord> _submissionRepo;
        private IDownloadService _downloadService;
        private ISettingService _settings;
        private IWorkContext _workContext;
        private ILocalizationService _localizationService;
        private CaptchaSettings _captchaSettings;
        private IEmailSender _emailSender;
        private ILogger _logger;
        IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public SubmissionController(IRepository<SubmissionRecord> submissionRepo, IDownloadService downloadService,
            ISettingService settings, IWorkContext workContext, ILocalizationService localizationService, CaptchaSettings captchaSettings,
            IEmailSender emailSender, ILogger logger, IEmailAccountService emailAccountService, EmailAccountSettings emailAccountSettings)
        {
            _submissionRepo = submissionRepo;
            _downloadService = downloadService;
            _settings = settings;
            _workContext = workContext;
            _localizationService = localizationService;
            _captchaSettings = captchaSettings;
            _emailSender = emailSender;
            _logger = logger;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
        }

        public ActionResult Manage()
        {
            var model = new SubmissionRecordSearchModel();

            //Submission types
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = null });
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.Enquiry"), Value = "0" });
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.JobApplication"), Value = "1" });
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.PartnerOffer"), Value = "2" });
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.VendorOffer"), Value = "3" });
            model.AvailableSubmissionTypes.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.Other"), Value = "3" });

            return View(model);
        }

        public ActionResult NewEntry(int type)
        {
            var model = new SubmissionRecord
            {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                RecordType = type,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };

            return View(model);
        }

        private void ProcessFileUpload(SubmissionRecord sr, out string errorMessage, out string fileName, out int downloadId)
        {

            Stream stream = null;
            fileName = String.Empty;
            downloadId = 0;
            var contentType = String.Empty;
            errorMessage = String.Empty;

            //Check in IE if we have submitted file
            if (String.IsNullOrEmpty(Request["qqfile"]) && Request.Files.Count > 0)
            {
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            //check in Webkit, Mozilla if we have submitted file
            else if (Request.InputStream != null && Request["qqfile"] != null)
            {
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            //check if we do not have any file submitted for upload -> exit this method
            if (stream == null || stream.Length == 0)
            {
                return;
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);

            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            //verify if file size does not exceed maximum defined in the admin area
            int maxFileSize = EngineContext.Current.Resolve<SubmissionAdminSettings>().SubmissionFileMaxSizeInMB;
            if (fileBinary.Length > (maxFileSize * 1024 * 1024))
            {
                errorMessage = String.Format(_localizationService.GetResource("Plugins.Zingasoft.Submission.Error.MaxSizeExeeded"), maxFileSize);
                return;
            }

            //verify if file type exists amongst the allowed file types defined in the admin area
            string allowedFileTypes = EngineContext.Current.Resolve<SubmissionAdminSettings>().SubmissionAllowedFiletypes;
            string[] splitFileTypes = allowedFileTypes.Split(',');
            bool isAllowed = false;
            foreach (var item in splitFileTypes)
            {
                if (item.Trim().ToLower().CompareTo(fileExtension.ToLower()) == 0)
                {
                    isAllowed = true;
                    break;
                }
            }
            if (isAllowed == false)
            {
                errorMessage = String.Format(_localizationService.GetResource("Plugins.Zingasoft.Submission.Error.InvalidFiletype"), allowedFileTypes);
                return;
            }

            //if we have passed all of the above verifications -> we can save the download in the DB
            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = String.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //assign the download link to the submissionrecord object
            sr.DownloadLink = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid });
            downloadId = download.Id;
        }

        [HttpPost]
        [CaptchaValidator]
        public ActionResult NewEntry(SubmissionRecord sr, bool captchaValid)
        {
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                string fileUploadErrorMessage = String.Empty;
                string filename = String.Empty;
                int downloadId = 0;
                ProcessFileUpload(sr, out fileUploadErrorMessage, out filename, out downloadId);

                //check if the file upload process went OK
                if (String.IsNullOrEmpty(fileUploadErrorMessage))
                {
                    string to = String.Empty;
                    switch (sr.RecordType)
                    {
                        case (int)SubmissionType.Enquiry:
                            to = EngineContext.Current.Resolve<SubmissionAdminSettings>().EnquiryEmail;
                            break;
                        case (int)SubmissionType.JobApplication:
                            to = EngineContext.Current.Resolve<SubmissionAdminSettings>().JobApplicationEmail;
                            break;
                        case (int)SubmissionType.PartnerOffer:
                            to = EngineContext.Current.Resolve<SubmissionAdminSettings>().PartnerOfferEmail;
                            break;
                        case (int)SubmissionType.VendorOffer:
                            to = EngineContext.Current.Resolve<SubmissionAdminSettings>().VendorOfferEmail;
                            break;
                        case (int)SubmissionType.Other:
                            to = EngineContext.Current.Resolve<SubmissionAdminSettings>().OtherEmail;
                            break;
                        default:
                            break;
                    }

                    //if we have a subscriber => we send the email
                    if (String.IsNullOrEmpty(to) == false)
                    {
                        string[] toList = to.Split(';');
                        List<string> cc = null;
                        if (toList.Length > 1)
                        {
                            to = toList[0];

                            cc = new List<string>();
                            for (int i = 1; i < toList.Length - 1; i++)
                            {
                                cc.Add(toList[i]);
                            }
                            
                        }
                        string email = sr.Email.Trim();
                        string fullName = sr.FullName;
                        string subject = sr.Subject;

                        var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                        if (emailAccount == null)
                            emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
                        if (emailAccount == null)
                            throw new Exception("No email account could be loaded");

                        StringBuilder sb = new StringBuilder();
                        sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.EmailNotificationIntro"));
                        sb.Append("<br/><br/>");
                        sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.EmailLabel"));
                        sb.Append(": ");
                        sb.Append(Core.Html.HtmlHelper.FormatText(sr.Email, false, true, false, false, false, false));
                        sb.Append("<br/>");
                        sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.FullNameLabel"));
                        sb.Append(": ");
                        sb.Append(Core.Html.HtmlHelper.FormatText(sr.FullName, false, true, false, false, false, false));
                        sb.Append("<br/>");
                        sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.SubjectLabel"));
                        sb.Append(": ");
                        sb.Append(Core.Html.HtmlHelper.FormatText(sr.Subject, false, true, false, false, false, false));
                        sb.Append("<br/>");
                        if (String.IsNullOrEmpty(sr.Phone) == false)
                        {
                            sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.PhoneLabel"));
                            sb.Append(": ");
                            sb.Append(Core.Html.HtmlHelper.FormatText(sr.Phone, false, true, false, false, false, false));
                            sb.Append("<br/>");
                        }
                        sb.Append(_localizationService.GetResource("Plugins.Zingasoft.Submission.EnquiryLabel"));
                        sb.Append(": ");
                        sb.Append(Core.Html.HtmlHelper.FormatText(sr.Enquiry, false, true, false, false, false, false));

                        string body = sb.ToString();
                        try
                        {
                            _emailSender.SendEmail(emailAccount,
                                sr.Subject,
                                body,
                               sr.Email,
                               sr.FullName,
                               to,
                               to,
                               to,
                               to,
                               null,
                               cc,
                               Url.Content(String.Format("~{0}", sr.DownloadLink)),
                               filename,
                               downloadId);
                        }
                        catch (Exception exc)
                        {
                            _logger.Error(string.Format("Error sending e-mail. {0}", exc.Message), exc);
                        }
                    }

                    _submissionRepo.Insert(sr);

                    sr.SuccessfullySaved = true;
                    sr.SaveResult = _localizationService.GetResource("Plugins.Zingasoft.Submission.Info.SubmissionSucceeded");

                    return View(sr);
                }
                else
                {
                    ModelState.AddModelError("", fileUploadErrorMessage);
                }
            }

            return View(sr);
        }

        [HttpPost]
        public ActionResult SubmissionList(DataSourceRequest command, SubmissionRecordSearchModel model)
        {
            var query = _submissionRepo.Table;
            if (String.IsNullOrEmpty(model.SearchEmail) == false)
            {
                query = query.Where(rec => rec.Email.ToLower().Contains(model.SearchEmail.ToLower()));
            }

            if (String.IsNullOrEmpty(model.SearchName) == false)
            {
                query = query.Where(rec => rec.FullName.ToLower().Contains(model.SearchName.ToLower()));
            }

            if (model.SearchSubmissionTypeEnumCode.HasValue && model.SearchSubmissionTypeEnumCode.Value >= 0)
            {
                query = query.Where(rec => rec.RecordType == model.SearchSubmissionTypeEnumCode.Value);
            }

            var submissions = query.ToList();

            var gridModel = new DataSourceResult
            {
                Data = submissions,
                Total = submissions.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult SubmissionUpdate(SubmissionRecord submissionUpdate)
        {
            var submissionRec = _submissionRepo.GetById(submissionUpdate.SubmissionRecordId);
            submissionRec.Email = submissionUpdate.Email;
            submissionRec.FullName = submissionUpdate.FullName;
            submissionRec.Subject = submissionUpdate.Subject;
            submissionRec.Phone = submissionUpdate.Phone;
            submissionRec.Enquiry = submissionUpdate.Enquiry;
            submissionRec.DownloadLink = submissionUpdate.DownloadLink;
            submissionRec.RecordType = submissionUpdate.RecordType;

            _submissionRepo.Update(submissionRec);

            return new NullJsonResult();
        }


        [HttpPost]
        public ActionResult SubmissionDelete(DataSourceRequest command, int submissionRecordId)
        {
            var submissionRec = _submissionRepo.GetById(submissionRecordId);

            //delete download file if there is such
            if (String.IsNullOrEmpty(submissionRec.DownloadLink) == false)
            {
                int startIndex = submissionRec.DownloadLink.IndexOf("downloadId=");
                int length = "downloadId=".Length;

                string downloadGuid = submissionRec.DownloadLink.Substring(startIndex + length);

                var download = _downloadService.GetDownloadByGuid(new Guid(downloadGuid));
                _downloadService.DeleteDownload(download);
            }

            _submissionRepo.Delete(submissionRec);

            return new NullJsonResult();
        }

        private bool isValidEmail(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);

            if (re.IsMatch(inputEmail))
                return true;
            else
                return false;
        }

        [HttpPost]
        public ActionResult SaveSettings(string newMaxFileSize, string newAllowedFiletypes,
            string newEnquiryEmail, string newJobApplicationEmail, string newPartnerOfferEmail,
            string newVendorOfferEmail, string newOtherEmail)
        {
            //validate maxfilesize
            int maxFileSize = 0;
            if (int.TryParse(newMaxFileSize, out maxFileSize) == false)
            {
                return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.Error.InvalidMaxSizeNum"));
            }
            if (maxFileSize <= 0)
            {
                return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.Error.NonpositiveMaxFileSize"));
            }


            //validate filtypes
            string[] splitVal = newAllowedFiletypes.Split(',');
            foreach (var item in splitVal)
            {
                if (item.Trim().StartsWith(".") == false)
                {
                    return Content(String.Format(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.Error.NonpositiveMaxFileSize"),
                        item.Trim()));
                }
            }

            //validate emails
            if (String.IsNullOrEmpty(newEnquiryEmail) == false)
            {
                string[] newEnquireEmailList = newEnquiryEmail.Split(';');

                foreach (var email in newEnquireEmailList)
                {
                    if (isValidEmail(email) == false)
                        return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail"));
                }

            }
            if (String.IsNullOrEmpty(newJobApplicationEmail) == false)
            {
                string[] newJobApplicationEmailList = newJobApplicationEmail.Split(';');
                foreach (var email in newJobApplicationEmailList)
                {
                    if (isValidEmail(email) == false)
                        return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail"));
                }
                
            }
            if (String.IsNullOrEmpty(newPartnerOfferEmail) == false)
            {
                string[] newPartnerOfferEmailList = newPartnerOfferEmail.Split(';');
                foreach (var email in newPartnerOfferEmailList)
                {
                    if (isValidEmail(email) == false)
                        return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail"));
                }
            }
            if (String.IsNullOrEmpty(newVendorOfferEmail) == false)
            {
                string[] newVendorOfferEmailList = newVendorOfferEmail.Split(';');
                foreach (var email in newVendorOfferEmailList)
                {
                    if (isValidEmail(email) == false)
                        return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail"));
                }
            }
            if (String.IsNullOrEmpty(newOtherEmail) == false)
            {
                string[] newOtherEmailList = newOtherEmail.Split(';');
                foreach (var email in newOtherEmailList)
                {
                    if (isValidEmail(email) == false)
                        return Content(_localizationService.GetResource("Plugins.Zingasoft.Submission.Admin.IvnalidSuscriberEmail"));
                }
            }

            //if everything is OK -> save new settings
            _settings.SaveSetting(new SubmissionAdminSettings
            {
                SubmissionFileMaxSizeInMB = maxFileSize,
                SubmissionAllowedFiletypes = newAllowedFiletypes,
                EnquiryEmail = newEnquiryEmail,
                JobApplicationEmail = newJobApplicationEmail,
                PartnerOfferEmail = newPartnerOfferEmail,
                VendorOfferEmail = newVendorOfferEmail,
                OtherEmail = newOtherEmail
            });

            return Content("success");
        }
    }
}
