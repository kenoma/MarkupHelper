using MarkupHelper.Common.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkupHelper.Common.Domain.Model;
using System.ServiceModel;
using Serilog;
using MongoDB.Driver;

namespace MarkupHelper.Service.Repository
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = false)]
    class MongodbRepository : IMarkupRepository
    {
        private readonly ILogger _log;
        private readonly IMongoDatabase _database;

        public MongodbRepository(IMongoDatabase mongoDatabase, ILogger log)
        {
            _log = log;
            _database = mongoDatabase;
        }

        public int CalculateUserScore(UserModel user)
        {
            try
            {
                _log.Information($"New user created {neophyte}");

                var rcollection = _database.GetCollection<GroupMarkup>(nameof(GroupMarkup));
                var tags = from mg in rcollection.AsQueryable()
                           group mg by new { T = mg.GroupTag, G = mg.GroupId } into g
                           where g.Any(z => z.UserId != user.Id) && g.Any(z => z.UserId == user.Id)
                           select g;

                
                return tags.Count();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return 0;
            }
        }

        public string[] GetTagsList(UserModel user)
        {
            throw new NotImplementedException();
        }

        public Group GetUnmarkedGroup(UserModel user)
        {
            throw new NotImplementedException();
        }

        public UserModel GetUser(string token)
        {
            throw new NotImplementedException();
        }

        public bool SubmitGroupTag(UserModel user, Group group, string tag)
        {
            throw new NotImplementedException();
        }
    }
}
