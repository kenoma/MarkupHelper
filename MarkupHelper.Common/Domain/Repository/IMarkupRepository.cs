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
        Group GetUnmarkedGroup(UserModel user);

        [OperationContract]
        bool SubmitGroupTag(UserModel user, Group group, string tag);

        [OperationContract]
        int CalculateUserScore(UserModel user);
    }
}
