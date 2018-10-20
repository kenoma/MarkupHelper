using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using MarkupHelper.Common.Domain.Model;
using MarkupHelper.Common.Domain.Repository;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Serilog;

namespace MarkupHelper.Service.Repository
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = false)]
    internal class MongodbRepository : IMarkupRepository
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
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));

                var usertags = from mg in rcollection.AsQueryable()
                               where mg.UserId == user.Id
                               select new { T = mg.ContentTag, G = mg.ContentId };

                var nonusertags = from mg in rcollection.AsQueryable()
                                  where mg.UserId != user.Id
                                  select new { T = mg.ContentTag, G = mg.ContentId };

                var union = usertags.ToArray().Intersect(nonusertags.ToArray());

                return union.Count();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return 0;
            }
        }

        public ContentTag[] GetTagsList(UserModel user)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                var rcollection = _database.GetCollection<ContentTag>(nameof(ContentTag));

                var allTags = from a in rcollection.AsQueryable()
                              where a.Level == user.Level
                              select a;

                return allTags.ToArray();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
                return new ContentTag[0];
            }
        }

        public Content GetUnmarkedContent(UserModel user)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                var rnd = new Random(Environment.TickCount);
                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));
                var gcollection = _database.GetCollection<Content>(nameof(Content));

                var qrbl = rcollection.AsQueryable();
                var forbiddenGroups = new HashSet<Guid>(from mg in qrbl
                                                        group mg by mg.ContentId into g
                                                        where g.Count() > config.Default.GroupMarkupsLimit
                                                        select g.Key);

                var userGroups = (from mg in qrbl
                                  where mg.UserId == user.Id
                                  select mg.ContentId).Distinct().ToArray();

                var allGroups = (from a in gcollection.AsQueryable()
                                 select a).ToArray();

                return allGroups.OrderBy(z => rnd.NextDouble()).Where(z => !forbiddenGroups.Contains(z.Id)).FirstOrDefault(z => !userGroups.Contains(z.Id));
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

        public double PercentageDone(UserModel user)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
            {
                throw new UnauthorizedAccessException();
            }

            var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));
            var gcollection = _database.GetCollection<Content>(nameof(Content));

            double markedByUser = rcollection.AsQueryable().Where(z => z.UserId == user.Id).Select(z => z.ContentId).Distinct().Count();
            return Math.Round(100.0 * markedByUser / gcollection.Count(_ => true));
        }

        public bool SubmitContentTag(UserModel user, Content group, string category, string tag)
        {
            LogOperation(user);

            if (GetUser(user.Token).Id != user.Id)
            {
                throw new UnauthorizedAccessException();
            }

            try
            {
                var rcollection = _database.GetCollection<ContentMarkup>(nameof(ContentMarkup));
                var gcollection = _database.GetCollection<Content>(nameof(Content));
                var tcollection = _database.GetCollection<ContentTag>(nameof(ContentTag));

                if (!tcollection.AsQueryable().Any(z => z.Tag.Equals(tag)))
                {
                    tcollection.InsertOne(new ContentTag { Category = category, Tag = tag, Id = Guid.NewGuid() });
                    //return false;
                }

                if (!gcollection.AsQueryable().Any(z => z.Id == group.Id))
                {
                    return false;
                }

                if (rcollection.AsQueryable().Count(mg => mg.UserId == user.Id && mg.ContentId == group.Id) > config.Default.TagLimitPerUser)
                {
                    return false;
                }

                rcollection.InsertOne(new ContentMarkup {
                    Id = Guid.NewGuid(),
                    ContentId = group.Id,
                    UserId = user.Id,
                    ContentTag = tag,
                    Category = category,
                    Level = user.Level,
                    Timestamp = DateTime.UtcNow
                });
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
                {
                    return;
                }

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
                {
                    return;
                }

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
