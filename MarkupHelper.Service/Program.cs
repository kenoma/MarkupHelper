using MarkupHelper.Common.Domain.Repository;
using MarkupHelper.Common.Service;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Service
{
    class Program
    {

        static void Main(string[] args)
        {
            var kernel = new StandardKernel(new Bindings());
            var serviceAddress = new Uri(config.Default.ServiceEndpoint);

            using (ServiceHost serviceHost = new ServiceHost(typeof(IMarkupRepository), serviceAddress))
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
        }
    }
}
