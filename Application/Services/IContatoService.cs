using Consumer.Delete.Contact.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.Delete.Contact.Application.Services
{
    public interface IContatoService
    {
        Task DeletarContatoAsync(int id);
    }
}
