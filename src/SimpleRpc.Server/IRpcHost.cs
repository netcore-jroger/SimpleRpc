// Copyright (c) JRoger. All Rights Reserved.

using System.Threading.Tasks;

namespace SimpleRpc.Server;

public interface IRpcHost
{
    Task StartAsync();

    Task StopAsync();
}
