using MarkupHelper.Common.Domain.Model;
using MarkupHelper.Common.Domain.Repository;
using MarkupHelper.Common.Service;
using MarkupHelper.Service.Repository;
using MongoDB.Driver;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MarkupHelper.UnitTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MarkupHelper.Service
{
    class Program
    {

        static void Main(string[] args)
        {
            var kernel = new StandardKernel(new Bindings());

            var manual = kernel.Get<ManualTasks>();
            Console.WriteLine("Enter cmd:");
            var cmd = Console.ReadLine();
            while (!string.IsNullOrWhiteSpace(cmd))
            {
                switch (cmd)
                {
                    case "start":
                        var serviceAddress = new Uri(config.Default.ServiceEndpoint);
                        using (ServiceHost serviceHost = new ServiceHost(typeof(MongodbRepository), serviceAddress))
                        {
                            serviceHost.Description.Behaviors.Add(new NinjectBehavior(kernel, typeof(IMarkupRepository)));
                            var endpoint = serviceHost.AddServiceEndpoint(
                                typeof(IMarkupRepository),
                                MarkupRepositoryBinding.MarkupRepository,
                                serviceAddress);

                            serviceHost.Open();
                            Console.WriteLine("Press any key to close app");
                            Console.ReadLine();
                        }
                        break;
                    case "lcat":
                        Console.Write("Enter categories filename:");
                        var fname = Console.ReadLine();
                        manual.LoadTags(fname);
                        break;
                    case "lgps":
                        Console.Write("Enter groups filename:");
                        fname = Console.ReadLine();
                        Console.Write("Enter groups count:");
                        var count = Console.ReadLine();
                        manual.LoadGroups(fname, int.TryParse(count, out int take) ? take : 0);
                        break;
                    case "cuser":
                        Console.Write("Enter user level:");
                        var level = Convert.ToInt32( Console.ReadLine());
                        var token = manual.CreateNewUser(level);
                        Console.WriteLine($"New user token is {token}");
                        break;
                }
                Console.WriteLine("Enter cmd:");
                cmd = Console.ReadLine();
            }

            

        }
    }
}
