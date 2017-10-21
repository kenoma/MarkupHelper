using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Service
{
    public static class MarkupRepositoryBinding
    {
        public static readonly Binding MarkupRepository = new NetTcpBinding(SecurityMode.None)
        {
            MaxReceivedMessageSize = int.MaxValue,
            ReaderQuotas =
                {
                    MaxArrayLength = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                },
            OpenTimeout = TimeSpan.FromMinutes(3),
            ReceiveTimeout = TimeSpan.FromHours(12),
            SendTimeout = TimeSpan.FromMinutes(3),
            CloseTimeout = TimeSpan.MaxValue
        };
    }
}
