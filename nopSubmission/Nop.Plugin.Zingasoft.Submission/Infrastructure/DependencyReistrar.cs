using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Zingasoft.Submission.Data;
using Nop.Plugin.Zingasoft.Submission.Domain;
using Nop.Web.Framework.Mvc;
using Nop.Core.Configuration;

namespace Nop.Plugin.Zingasoft.Submission.Infrastructure
{
    public class DependencyReistrar: IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_submission";
         
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //data context
            this.RegisterPluginDataContext<SubmissionRecordObjectContext>(builder, CONTEXT_NAME);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<SubmissionRecord>>()
                .As<IRepository<SubmissionRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
