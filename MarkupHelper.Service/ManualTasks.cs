using CsvHelper;
using CsvHelper.Configuration;
using MarkupHelper.Common.Domain.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        class ContentTagCsv
        {
            public string Category { get; set; }
            public string Tag { get; set; }
            public int Level { get; set; }
        }

        internal void LoadTags(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Fail.");
                return;
            }

            using (var file = File.OpenText(filename))
            using (var csv = new CsvReader(file, new Configuration { Delimiter = ",", CultureInfo = CultureInfo.InvariantCulture }))
            {
                foreach (var rec in csv.GetRecords<ContentTagCsv>())
                {
                    if (!_database.GetCollection<ContentTag>(nameof(ContentTag)).AsQueryable().Any(z =>
                    z.Tag == rec.Tag &&
                    z.Category == rec.Category &&
                    z.Level == rec.Level))
                    {
                        var tmp = new ContentTag {
                            Id = Guid.NewGuid(),
                            Tag = rec.Tag,
                            Category = rec.Category,
                            Level = rec.Level
                        };
                        _database.GetCollection<ContentTag>(nameof(ContentTag)).InsertOne(tmp);
                        Console.WriteLine($"Inserted tag {tmp.Id}:{tmp.Tag}");
                    }
                    else
                    {
                        Console.WriteLine($"Tag {{{rec}}} already added ");
                    }
                }
            }
            Console.WriteLine("Done");
        }

        internal void LoadGroups(string fname, int take)
        {
            if (!File.Exists(fname))
            {
                Console.WriteLine("Fail.");
                return;
            }

            foreach (var line in File.ReadAllLines(fname).Select(z => new Uri(z)))
                if (take > 0)
                {
                    var collection = _database.GetCollection<Content>(nameof(Content));
                    if (!collection.AsQueryable().Any(z => z.PostAddress == line))
                    {
                        var tmp = new Content { Id = Guid.NewGuid(), PostAddress = line };
                        collection.InsertOne(tmp);
                        take--;
                        Console.WriteLine($"Inserted group {tmp.Id}:{tmp.PostAddress}. Remains {take}");
                    }
                    else
                    {
                        Console.WriteLine($"Group {{{line}}} already added ");
                    }
                }
            Console.WriteLine("Done");
        }

        internal string CreateNewUser(int level)
        {
            var user = new UserModel {
                Id = Guid.NewGuid(),
                Level = level,
                Token = Guid.NewGuid().ToString()
            };
            _database.GetCollection<UserModel>(nameof(UserModel)).InsertOne(user);
            return user.Token;
        }
    }
}
