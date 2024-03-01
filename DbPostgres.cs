using dotenv.net;
using Npgsql;
using System.Text.Json;

namespace Paschoalotto_RPA
{
    public class DbPostgres
    {
        private string connectionString;
        private static string TABLE_RESULTS_WPM = "results_wpm";

        public DbPostgres()
        {
            // Carrega as variáveis de ambiente do arquivo .env
            LoadEnvVariables();
            connectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};Database={Environment.GetEnvironmentVariable("POSTGRES_DATABASE")};Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")}";
        }

        public void InsertResults(JsonElement json)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                CreateTableResultsWpm(connection);

                try
                {
                    // Insere os dados do JSON na tabela
                    using (var command = new NpgsqlCommand($@"
                    INSERT INTO {TABLE_RESULTS_WPM} (wpm, keystrokes, accuracy, correct_word, wrong_word)
                    VALUES (
                            {json.GetProperty("wpm")}, 
                            {json.GetProperty("keystrokes")}, 
                            {json.GetProperty("accuracy")}, 
                            {json.GetProperty("correct_word")}, 
                            {json.GetProperty("wrong_word")});", 
                    connection))
                    {

                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Resultado do 10 Fast Fingers inserido no database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Não foi possível inserir o resultado do WPM no database: " + ex.Message);
                }
            }
        }

        private static void CreateTableResultsWpm(NpgsqlConnection connection)
        {
            try
            {
                // Cria a tabela se ela não existir
                using (var command = new NpgsqlCommand($@"
                    CREATE TABLE IF NOT EXISTS {TABLE_RESULTS_WPM} (
                        id SERIAL PRIMARY KEY,
                        wpm INT,
                        keystrokes INT,
                        accuracy REAL,
                        correct_word INT,
                        wrong_word INT,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar a tabela '" + TABLE_RESULTS_WPM + "': " + ex.Message);
            }
        }
        private static void LoadEnvVariables()
        {
            DotEnv.Load();
        }
    }
}
