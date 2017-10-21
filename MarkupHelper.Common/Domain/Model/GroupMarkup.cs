using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Domain.Model
{
    [DataContract]
    public class GroupMarkup : IAggregateRoot
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string GroupTag { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public Guid GroupId { get; set; }
    }
}
