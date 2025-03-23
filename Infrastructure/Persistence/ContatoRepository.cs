using Dapper;
using MySql.Data.MySqlClient;
using Consumer.Delete.Contact.Infrastructure.Persistence;

namespace Consumer.Delete.Contact.Infrastructure.Persistence
{
    public class ContatoRepository : IContatoRepository
    {
        private readonly string _connectionString;

        public ContatoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task DeleteContatoAsync(int id)
        {
            const string query = @"
                DELETE FROM contatos 
                WHERE id = @Id;";

            using var connection = new MySqlConnection(_connectionString);

            try
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Contato com ID {id} deletado com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Nenhum contato encontrado com o ID {id} para exclusão.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deletar o contato com ID {id}: {ex.Message}");
            }
        }
    }
}
