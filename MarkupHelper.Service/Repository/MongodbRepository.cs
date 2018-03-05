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
using MongoDB.Driver.Linq;
using System.ServiceModel.Channels;
using System.Runtime.CompilerServices;

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
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
                throw new UnauthorizedAccessException();

            try
            {
                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));

                var usertags = from mg in rcollection.AsQueryable()
                               where mg.UserId == user.Id
                               select new { T = mg.ContentTag, G = mg.GroupId };

                var nonusertags = from mg in rcollection.AsQueryable()
                                  where mg.UserId != user.Id
                                  select new { T = mg.ContentTag, G = mg.GroupId };

                var union = usertags.ToArray().Intersect(nonusertags.ToArray());

                return union.Count();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return 0;
            }
        }

        public string[] GetTagsList(UserModel user)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
                throw new UnauthorizedAccessException();

            try
            {
                var rcollection = _database.GetCollection<ContentTag>(nameof(ContentTag));

                var allTags = from a in rcollection.AsQueryable()
                              select a.Tag;

                return allTags.ToArray();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return new string[0];
            }
        }

        public Content GetUnmarkedContent(UserModel user)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
                throw new UnauthorizedAccessException();

            try
            {

                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));
                var gcollection = _database.GetCollection<Content>(nameof(Content));

                var qrbl = rcollection.AsQueryable();
                var forbiddenGroups = new HashSet<Guid>(from mg in qrbl
                                                        group mg by mg.GroupId into g
                                                        where g.Count() > config.Default.GroupMarkupsLimit
                                                        select g.Key);

                var userGroups = (from mg in qrbl
                                  where mg.UserId == user.Id
                                  select mg.GroupId).Distinct().ToArray();

                var allGroups = (from a in gcollection.AsQueryable()
                                 select a).ToArray();

                return allGroups.Where(z => !forbiddenGroups.Contains(z.Id)).FirstOrDefault(z => !userGroups.Contains(z.Id));
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                throw;
            }
        }

        public UserModel GetUser(string token)
        {
            LogOperation(token);
            var users = _database.GetCollection<UserModel>(nameof(UserModel));

            var user = users.AsQueryable().Single(z => z.Token == token);
            return user;
        }

        public bool SubmitContentTag(UserModel user, Content group, string tag)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
                throw new UnauthorizedAccessException();

            try
            {
                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));
                var gcollection = _database.GetCollection<Content>(nameof(Content));
                var tcollection = _database.GetCollection<ContentTag>(nameof(ContentTag));

                if (!tcollection.AsQueryable().Any(z => z.Tag.Equals(tag)))
                {
                    tcollection.InsertOne(new ContentTag { Tag = tag, Id = Guid.NewGuid() });
                    //return false;
                }

                if (!gcollection.AsQueryable().Any(z => z.Id == group.Id))
                    return false;

                if (rcollection.AsQueryable().Count(mg => mg.UserId == user.Id && mg.GroupId == group.Id) > config.Default.TagLimitPerUser)
                    return false;

                rcollection.InsertOne(new ContentMarkup { Id = Guid.NewGuid(), GroupId = group.Id, UserId = user.Id, ContentTag = tag, Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                throw;
            }
            return true;
        }

        private void LogOperation(string token, [CallerMemberName]string caller = "")
        {
            try
            {
                var context = OperationContext.Current;
                if (context == null)
                    return;
                var prop = context.IncomingMessageProperties;
                var endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                _log.Verbose("[{Token}] User validation {Method} from {@EndpointData}", token, caller, endpoint);
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }

        private void LogOperation(UserModel user, [CallerMemberName]string caller = "")
        {
            try
            {
                var context = OperationContext.Current;
                if (context == null)
                    return;
                var prop = context.IncomingMessageProperties;
                var endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                _log.Verbose("[{@User}] Request {Method} from {@EndpointData}", user, caller, endpoint);
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }
        }
    }
}
