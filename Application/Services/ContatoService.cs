using Consumer.Delete.Contact.Domain.Entities;
using Consumer.Delete.Contact.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace Consumer.Delete.Contact.Application.Services
{
    public class ContatoService : IContatoService
    {
        private readonly IContatoRepository _contatoRepository;

        public ContatoService(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }

        public async Task DeletarContatoAsync(int id)
        {
            await _contatoRepository.DeleteContatoAsync(id);
            Console.WriteLine($"Contato com ID {id} excluído com sucesso.");
        }
    }
}
