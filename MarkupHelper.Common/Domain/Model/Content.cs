﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Domain.Model
{
    [DataContract]
    public class Content : IAggregateRoot
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Uri PostAddress { get; set; }
    }
}
