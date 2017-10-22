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

[assembly:InternalsVisibleTo("MarkupHelper.UnitTest")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MarkupHelper.Service
{
    class Program
    {

        static void Main(string[] args)
        {
            var kernel = new StandardKernel(new Bindings());

            //var _database = kernel.Get<IMongoDatabase>();
            //_database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(new UserModel { Id=Guid.NewGuid(), Token="B" });
            //_database.GetCollection<Group>(nameof(Group)).InsertMany(new[] { new Group { Id=Guid.NewGuid(), VkId=2 } });
            //_database.GetCollection<GroupTag>(nameof(GroupTag)).InsertOne(new GroupTag { Id = Guid.NewGuid(), Tag = "31a" });
            //_database.GetCollection<GroupTag>(nameof(GroupTag)).InsertOne(new GroupTag { Id = Guid.NewGuid(), Tag = "41b" });
            //_database.GetCollection<GroupTag>(nameof(GroupTag)).InsertOne(new GroupTag { Id = Guid.NewGuid(), Tag = "51c" });

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
        }
    }
}
