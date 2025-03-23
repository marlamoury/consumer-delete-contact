using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.Delete.Contact.Infrastructure.Persistence
{
    public interface IContatoRepository
    {
        Task DeleteContatoAsync(int id);
    }
}
