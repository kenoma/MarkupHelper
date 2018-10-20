using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Domain.Model
{
    [DataContract]
    public class ContentTag : IAggregateRoot
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public int Level { get; set; }
    }
}
