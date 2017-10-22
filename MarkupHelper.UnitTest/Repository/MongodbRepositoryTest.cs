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
            var groupRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="A", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="B", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userAnother }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }


        [Test]
        public void CalculateUserScore_DataFromOnlyUser_0()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="B", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void CalculateUserScore_OneMatch_1()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="B", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void CalculateUserScore_UsersPlacedDifferentTags_0()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var userAnother = Guid.NewGuid();
            var groupRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="B", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="C", UserId = userAnother },
                new GroupMarkup { GroupId = Guid.NewGuid(), GroupTag="D", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(groupRecords);
            var repo = Create();

            repo.CalculateUserScore(userTarget).Should().Be(0);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void GetTagsList_GetTags()
        {
            var user = new UserModel { Id = Guid.NewGuid(), Token = "Asdasd" };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(user);
            var tags = Enumerable.Range(0, 100).Select(z => new GroupTag { Tag = Path.GetRandomFileName(), Id = Guid.NewGuid() }).ToArray();
            _database.GetCollection<GroupTag>(nameof(GroupTag)).InsertMany(tags);
            var repo = Create();

            repo.GetTagsList(user).Should().Equal(tags, (x, y) => y.Tag == x);
        }

        [Test]
        public void GetUnmarkedGroup_GetUnAttendedGroup()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var groupRecords = new List<Group>
            {
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount }
            };
            var markRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = groupRecords[0].Id, GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = groupRecords[1].Id, GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = groupRecords[2].Id, GroupTag="A", UserId = userTarget.Id }

            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(markRecords);
            _database.GetCollection<Group>(nameof(Group)).InsertMany(groupRecords);
            var repo = Create();

            repo.GetUnmarkedGroup(userTarget).Should().Equals(groupRecords[3]);
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void GetUnmarkedGroup_AllGroupsDone()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var groupRecords = new List<Group>
            {
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount },
                new Group{ Id=Guid.NewGuid(), VkId = Environment.TickCount }
            };
            var markRecords = new List<GroupMarkup>
            {
                new GroupMarkup { GroupId = groupRecords[0].Id, GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = groupRecords[1].Id, GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = groupRecords[2].Id, GroupTag="A", UserId = userTarget.Id },
                new GroupMarkup { GroupId = groupRecords[3].Id, GroupTag="A", UserId = userTarget.Id }
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).InsertMany(markRecords);
            _database.GetCollection<Group>(nameof(Group)).InsertMany(groupRecords);
            var repo = Create();

            repo.GetUnmarkedGroup(userTarget).Should().BeNull();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
        }

        [Test]
        public void SubmitGroupTag_Accept()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var group = new Group { Id = Guid.NewGuid(), VkId = Environment.TickCount };
            var mark = Path.GetRandomFileName();
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            _database.GetCollection<Group>(nameof(Group)).InsertMany(new[] { group });
            var repo = Create();

            repo.SubmitGroupTag(userTarget,group, mark).Should().BeTrue();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).AsQueryable()
                .Any(z => z.GroupId == group.Id && z.GroupTag == mark && z.UserId == userTarget.Id).Should().BeTrue();
        }

        [Test]
        public void SubmitGroupTag_NoGroup_false()
        {
            var userTarget = new UserModel { Id = Guid.NewGuid(), Token = Path.GetRandomFileName() };
            var group = new Group { Id = Guid.NewGuid(), VkId = Environment.TickCount };
            var mark = Path.GetRandomFileName();
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(userTarget);
            var repo = Create();

            repo.SubmitGroupTag(userTarget, group, mark).Should().BeFalse();
            _log.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
            _database.GetCollection<GroupMarkup>(nameof(GroupMarkup)).AsQueryable()
                .Any(z => z.GroupId == group.Id && z.GroupTag == mark && z.UserId == userTarget.Id).Should().BeFalse();
        }

        private MongodbRepository Create()
        {
            return new MongodbRepository(_database, _log);
        }
    }
}
