using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DataReaderTest
{
    class Program
    {
        static void Main(string[] args)
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "mindshopper.database.windows.net";
            builder.UserID = "mindshopper";
            builder.Password = "8799LipYAA9oksRLG6ia";
            builder.InitialCatalog = "recommender";


            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT ItemId, ItemName, CategoryCode, Category, SalesValue, ItemRank ");
                    sb.Append("FROM [dim].[Item]");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Item item = new Item(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDecimal(4), reader.GetInt32(5));
                                Console.WriteLine(item.ToString());
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nDone. Press enter.");
            Console.ReadLine();
        }

        private void method()
        {
            string serviceUrl = "http://url/";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceUrl);

            // Add an Accept header for JSON format.  
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            // Add api-key header
            client.DefaultRequestHeaders.Add("x-api-key", "y83LOw2Clk2lBkyUCdEeJaK56fBhCEnU6qIcVMoY");
            var response = client.GetStringAsync(serviceUrl).Result;
                
        }
    }
}
