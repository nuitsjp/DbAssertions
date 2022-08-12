using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DbAssertions.Test.SqlServer;

public class AdventureWorks : IDisposable
{
    private static readonly string ImageName = "nuitsjp/adventureworks";
    private static readonly string TagName = "latest";
    public static readonly string SaPassword = "P@ssw0rd!";
    private static readonly string SqlServerPort = "1433";

    private readonly DockerClient _dockerClient;
    private readonly string _containerId;
    private AdventureWorks(DockerClient dockerClient, string containerId, int port)
    {
        _dockerClient = dockerClient;
        _containerId = containerId;
        Port = port;
    }

    public int Port { get; }

    public static AdventureWorks Start()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        try
        {
            DockerClient dockerClient = new DockerClientConfiguration().CreateClient();


            dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = ImageName,
                    Tag = TagName,
                },
                null,
                new Progress<JSONMessage>()).GetAwaiter().GetResult();

            var port = ((IPEndPoint) tcpListener.LocalEndpoint).Port;
            var containerId = dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = ImageName,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,
                Env = new[] { "ACCEPT_EULA=Y", $"SA_PASSWORD={SaPassword}" },
                ExposedPorts = new Dictionary<string, EmptyStruct>() {
                    { SqlServerPort, new EmptyStruct() }
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        {
                            SqlServerPort, new List<PortBinding> {
                                new() { HostPort = port.ToString() }
                            }
                        }
                    }
                }

            }).GetAwaiter().GetResult().ID;

            dockerClient.Containers.StartContainerAsync(containerId, new())
                .GetAwaiter().GetResult();

            return new AdventureWorks(dockerClient, containerId, port);
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    public void Dispose()
    {
        _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters { Force = true })
            .GetAwaiter().GetResult();
    }
}