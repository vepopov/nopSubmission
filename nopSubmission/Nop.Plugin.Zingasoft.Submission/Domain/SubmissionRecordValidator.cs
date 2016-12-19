using FluentValidation;
using Nop.Plugin.Zingasoft.Submission.Data;
using Nop.Services.Localization;

namespace Nop.Plugin.Zingasoft.Submission.Domain
{
    public class SubmissionRecordValidator : AbstractValidator<SubmissionRecord>
    {
        public SubmissionRecordValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.FullNameRequired"))
                .Length(0,Constants.FullNameMaxLength).WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.FullNameTooLong"));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.EmailRequired"))
                .EmailAddress().WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.EmailFormat"));

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.SubjectRequired"))
                .Length(0,Constants.SubjectMaxLength).WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.SubjectTooLong"));

            RuleFor(x => x.Enquiry)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.EnquiryRequired"))
                .Length(0,Constants.EnquiryMaxLength).WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.EnquiryTooLong"));

            RuleFor(x => x.Phone)
               .Length(0,Constants.PhoneMaxLength).WithMessage(localizationService.GetResource("Plugins.Zingasoft.Submission.PhoneTooLong"));
        }
    }
}