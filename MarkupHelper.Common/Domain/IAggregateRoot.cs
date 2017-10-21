using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkupHelper.Common.Domain
{
    public interface IAggregateRoot
    {
        Guid Id { get; set; }
    }
}
