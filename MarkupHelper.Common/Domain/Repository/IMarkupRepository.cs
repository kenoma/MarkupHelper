using MarkupHelper.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Domain.Repository
{
    [ServiceContract]
    public interface IMarkupRepository
    {
        [OperationContract]
        UserModel GetUser(string token);

        [OperationContract]
        string[] GetTagsList(UserModel user);

        [OperationContract]
        Content GetUnmarkedContent(UserModel user);

        [OperationContract]
        bool SubmitContentTag(UserModel user, Content group, string tag);

        [OperationContract]
        int CalculateUserScore(UserModel user);
    }
}
