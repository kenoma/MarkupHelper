using FluentAssertions;
using MarkupHelper.Common.Domain.Model;
using MarkupHelper.Service.Repository;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.UnitTest.Repository
{
    class MongodbRepositoryTest
    {
        private IMongoDatabase _database;
        private ILogger _log;
        private string tmpDb;
        private MongoClient _mongoClient;

        [SetUp]
        public void Setup()
        {
            tmpDb = $"tmpDb{Environment.TickCount}";
            _mongoClient = new MongoClient("mongodb://localhost:27017/");
            _database = _mongoClient.GetDatabase(tmpDb);
            _log = Substitute.For<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mongoClient.DropDatabase(tmpDb);
        }

        [Test]
        public void CalculateUserScore_NoDataFromUser_0()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="A", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="B", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userAnother }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }


        [Test]
        public void CalculateUserScore_DataFromOnlyUser_0()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="B", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void CalculateUserScore_OneMatch_1()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="B", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void CalculateUserScore_UsersPlacedDifferentTags_0()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="B", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="C", UserId = userAnother },
                new ContentMarkup { GroupId = Guid.NewGuid(), ContentTag="D", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void GetTagsList_GetTags()
        {
            var user = new UserModel { Id = Guid.NewGuid(), Token = "Asdasd" };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(user);
            var tags = Enumerable.Range(0, 100).Select(z => new ContentTag { Tag = Path.GetRandomFileName(), Id = Guid.NewGuid() }).ToArray();
            _database.GetCollection<ContentTag>(nameof(ContentTag)).InsertMany(tags);
            var repo = Create();

            repo.GetTagsList(user).Should().Equal(tags, (x, y) => y.Tag == x);
        }

        [Test]
        public void GetUnmarkedGroup_GetUnAttendedGroup()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var groupRecords = new List<Content>
            {
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() }
            };
            var markRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = groupRecords[0].Id, ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = groupRecords[1].Id, ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = groupRecords[2].Id, ContentTag="A", UserId = userTarget.Id }

            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(markRecords);
            _database.GetCollection<Content>(nameof(Content)).InsertMany(groupRecords);
            var repo = Create();

            repo.GetUnmarkedContent(userTarget).Should().Equals(groupRecords[3]);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void GetUnmarkedGroup_AllGroupsDone()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var groupRecords = new List<Content>
            {
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() },
                new Content{ Id=Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() }
            };
            var markRecords = new List<ContentMarkup>
            {
                new ContentMarkup { GroupId = groupRecords[0].Id, ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = groupRecords[1].Id, ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = groupRecords[2].Id, ContentTag="A", UserId = userTarget.Id },
                new ContentMarkup { GroupId = groupRecords[3].Id, ContentTag="A", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).InsertMany(markRecords);
            _database.GetCollection<Content>(nameof(Content)).InsertMany(groupRecords);
            var repo = Create();

            repo.GetUnmarkedContent(userTarget).Should().BeNull();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void SubmitGroupTag_Accept()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var group = new Content { Id = Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() };
            var mark = Path.GetRandomFileName();
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<Content>(nameof(Content)).InsertMany(new[] { group });
            _database.GetCollection<ContentTag>(nameof(ContentTag)).InsertOne(new ContentTag { Id = Guid.NewGuid(), Tag = mark });
            var repo = Create();

            repo.SubmitContentTag(userTarget,group, mark).Should().BeTrue();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).AsQueryable()
                .Any(z => z.GroupId == group.Id && z.ContentTag == mark && z.UserId == userTarget.Id).Should().BeTrue();
        }

        [Test]
        public void SubmitGroupTag_NoGroup_false()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var group = new Content { Id = Guid.NewGuid(), VkContentId = Environment.TickCount.ToString() };
            var mark = Path.GetRandomFileName();
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            var repo = Create();

            repo.SubmitContentTag(userTarget, group, mark).Should().BeFalse();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
            _database.GetCollection<ContentMarkup>(nameof(ContentMarkup)).AsQueryable()
                .Any(z => z.GroupId == group.Id && z.ContentTag == mark && z.UserId == userTarget.Id).Should().BeFalse();
        }

        private MongodbRepository Create()
        {
            return new MongodbRepository(_database, _log);
        }
    }
}
