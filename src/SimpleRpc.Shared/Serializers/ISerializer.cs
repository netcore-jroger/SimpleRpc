// Copyright (c) JRoger. All Rights Reserved.

namespace SimpleRpc.Shared.Serializers
{
    public interface ISerializer
    {
        byte[] ToBytes<T>(T input);

        T FromBytes<T>(byte[] input);
    }
}
