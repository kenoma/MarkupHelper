using System;
using System.Linq;
using MarkupHelper.Common.Domain.Model;
using MarkupHelper.Common.Domain.Repository;

namespace MarkupHelper.Service
{
    internal class DummyMarkupRepository : IMarkupRepository
    {

        private readonly Random _rnd = new Random(Environment.TickCount);

        public int CalculateUserScore(UserModel user)
        {
            return _rnd.Next(100);
        }

        public ContentTag[] GetTagsList(UserModel user)
        {
            var arr = new ContentTag[]
                {
                    new ContentTag{ Category="Категория 1", Level=1, Tag="Тэг 1" },
                    new ContentTag{ Category="Категория 1", Level=1, Tag="Тэг 2" },

                    new ContentTag{ Category="Категория 1", Level=2, Tag="Тэг 1" },
                    new ContentTag{ Category="Категория 1", Level=2, Tag="Тэг 2" },

                    new ContentTag{ Category="Категория 2", Level=2, Tag="Тэг 1" },
                    new ContentTag{ Category="Категория 2", Level=2, Tag="Тэг 2" },
                    new ContentTag{ Category="Категория 2", Level=2, Tag="Тэг 3" },
                    new ContentTag{ Category="Категория 2", Level=2, Tag="Тэг 4" },

                    new ContentTag{ Category="Категория 3", Level=2, Tag="Тэг 1" },
                    new ContentTag{ Category="Категория 3", Level=2, Tag="Тэг 2" },
                    new ContentTag{ Category="Категория 3", Level=2, Tag="Тэг 3" },

                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 1" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 2" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 3" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 4" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 11" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 21" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 13" },
                    new ContentTag{ Category="Категория 4", Level=2, Tag="Тэг 14" },

                    new ContentTag{ Category="Категория 5", Level=2, Tag="Тэг 14" },

                    new ContentTag{ Category="Проблема 1", Level=2, Tag="Тэг 14" },

                    new ContentTag{ Category="Категория 7", Level=2, Tag="Тэг 14" },

                    new ContentTag{ Category="Категория 8", Level=2, Tag="Тэг 14" },
                };
            return arr.Where(z => z.Level == user.Level).ToArray();
        }

        public Content GetUnmarkedContent(UserModel user)
        {
            return new Content {
                PostAddress = new Uri("https://duckduckgo.com/?q=csvhelper+c%23&atb=v80-4__&ia=qa")
            };
        }

        public UserModel GetUser(string token)
        {
            return new UserModel { Level = 2, Token = token };
        }

        public double PercentageDone(UserModel user)
        {
            return Math.Round(_rnd.NextDouble() * 100.0);
        }

        public bool SubmitContentTag(UserModel user, Content group, string category, string tag)
        {
            return _rnd.NextDouble() > 0.2;
        }
    }
}
