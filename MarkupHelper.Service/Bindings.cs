using MarkupHelper.Common.Domain.Repository;
using MarkupHelper.Service.Repository;
using MongoDB.Driver;
using Ninject;
using Ninject.Modules;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Service
{
    class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<MongoClient>().ToSelf().WithConstructorArgument("connectionString", config.Default.MongodbConnectionString);
            Bind<IMongoDatabase>().ToMethod(ctx => ctx.Kernel.Get<MongoClient>().GetDatabase(config.Default.TargetDatabase));
            Bind<IMarkupRepository>().To<MongodbRepository>();
            Bind<ILogger>().ToMethod(z => new LoggerConfiguration()
                .Enrich.WithProcessName()
                .Enrich.WithProcessId()
                .MinimumLevel.Verbose()
                .WriteTo.Seq(config.Default.SeqServer, compact: true)
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log", "Service-{Date}.log"), fileSizeLimitBytes: 1024 * 1024 * 50, shared: true)
                .CreateLogger());
        }
    }
}