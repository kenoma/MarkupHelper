using MarkupHelper.Common.Domain.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Service
{
    class ManualTasks
    {
        private IMongoDatabase _database;

        public ManualTasks(IMongoDatabase database)
        {
            _database = database;
        }

        internal void LoadCategories(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Fail.");
                return;
            }

            foreach (var line in File.ReadAllLines(filename).Union(ContentTag.PredefinedEmotions))
            {
                if (!_database.GetCollection<ContentTag>(nameof(ContentTag)).AsQueryable().Any(z => z.Tag == line))
                {
                    var tmp = new ContentTag { Id = Guid.NewGuid(), Tag = line };
                    _database.GetCollection<ContentTag>(nameof(ContentTag)).InsertOne(tmp);
                    Console.WriteLine($"Inserted tag {tmp.Id}:{tmp.Tag}");
                }
                else
                {
                    Console.WriteLine($"Tag {{{line}}} already added ");
                }
            }
            Console.WriteLine("Done");
            //
            //
            //
            //_database.GetCollection<GroupTag>(nameof(GroupTag)).InsertOne(new GroupTag { Id = Guid.NewGuid(), Tag = "41b" });
            //_database.GetCollection<GroupTag>(nameof(GroupTag)).InsertOne(new GroupTag { Id = Guid.NewGuid(), Tag = "51c" });
        }

        internal void LoadGroups(string fname, int take)
        {
            if (!File.Exists(fname))
            {
                Console.WriteLine("Fail.");
                return;
            }

            foreach (var line in File.ReadAllLines(fname))
                if (take > 0)
                {
                    var collection = _database.GetCollection<Content>(nameof(Content));
                    if (!collection.AsQueryable().Any(z => z.VkContentId == line))
                    {
                        var tmp = new Content { Id = Guid.NewGuid(), VkContentId = line };
                        collection.InsertOne(tmp);
                        take--;
                        Console.WriteLine($"Inserted group {tmp.Id}:{tmp.VkContentId}. Remains {take}");
                    }
                    else
                    {
                        Console.WriteLine($"Group {{{line}}} already added ");
                    }
                }
            Console.WriteLine("Done");
        }

        internal string CreateNewUser()
        {
            UserModel user = new UserModel { Id = Guid.NewGuid(), Token = Guid.NewGuid().ToString() };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(user);
            return user.Token;
        }
    }
}
