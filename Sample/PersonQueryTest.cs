using System.Data;
using System.Net;
using Dapper;
using DbAssertions;
using DbAssertions.SqlServer;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;

namespace Sample
{
    public class PersonQueryTest : IDisposable
    {
        private const string ImageName = "nuitsjp/adventureworks";
        private const string TagName = "latest";
        private const string SaPassword = "P@ssw0rd!";
        private const string SqlServerPort = "1433";

        private readonly DockerClient _dockerClient;
        private readonly string _containerId;

        public PersonQueryTest()
        {
            _dockerClient = new DockerClientConfiguration().CreateClient();
            _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = ImageName,
                    Tag = TagName,
                },
                null,
                new Progress<JSONMessage>()).GetAwaiter().GetResult();

            _containerId = _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = ImageName,
                Env = new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={SaPassword}" },
                ExposedPorts = new Dictionary<string, EmptyStruct>() {
                    { SqlServerPort, new EmptyStruct() }
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        {
                            SqlServerPort, new List<PortBinding> {
                                new() { HostPort = SqlServerPort }
                            }
                        }
                    }
                }
            }).GetAwaiter().GetResult().ID;

            _dockerClient.Containers.StartContainerAsync(_containerId, new()).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task UpdateTitle()
        {
            // Initialize
            using var connection = OpenConnection();
            await connection.ExecuteAsync(await File.ReadAllTextAsync(@"Sql\Initialize.sql"));
            var setupCompletion = connection.ExecuteScalar<DateTime>("select GETDATE()");

            // Run
            await connection.ExecuteAsync(await File.ReadAllTextAsync(@"Sql\UpdateTitle.sql"));

            // Assertions
            var database = new SqlDatabase(BuildConnectionString());
            database.Should().BeExpected(new DirectoryInfo("Expected\\Expected"), setupCompletion);
        }

        public void Dispose()
        {
            _dockerClient.Containers
                .RemoveContainerAsync(_containerId, new ContainerRemoveParameters { Force = true })
                .GetAwaiter().GetResult();
        }

        private static string BuildConnectionString()
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = "localhost",
                InitialCatalog = "AdventureWorks",
                UserID = "sa",
                Password = SaPassword,
                Encrypt = false
            }.ToString();
        }

        /// <summary>
        /// Immediately after docker startup, connection errors occur. Therefore, retry processing is performed.
        /// </summary>
        /// <returns></returns>
        private static IDbConnection OpenConnection()
        {
            var connectionString = BuildConnectionString();


            IDbConnection? connection = null;
            for (var i = 0; ; i++)
            {
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                }
                catch
                {
                    connection?.Dispose();
                    if (i == 100)
                    {
                        throw;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
            }
        }
    }
}