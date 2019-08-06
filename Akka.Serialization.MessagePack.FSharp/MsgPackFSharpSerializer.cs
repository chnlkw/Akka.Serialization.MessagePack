//-----------------------------------------------------------------------
// <copyright file="MsgPackFSharpSerializer.cs" company="Akka.NET Project">
//     Copyright (C) 2017 Akka.NET Contrib <https://github.com/AkkaNetContrib/Akka.Serialization.MessagePack>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Serialization.MessagePack.Resolvers;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using MessagePack.FSharp;
using Microsoft.FSharp.Reflection;

namespace Akka.Serialization.MessagePack.FSharp
{
    public sealed class MsgPackFSharpSerializer : Serializer
    {
        internal static AsyncLocal<ActorSystem> LocalSystem = new AsyncLocal<ActorSystem>();
        private readonly MsgPackSerializerSettings _settings;

        static MsgPackFSharpSerializer()
        {
            CompositeResolver.RegisterAndSetAsDefault(
#if SERIALIZATION
                SerializableResolver.Instance,
#endif
                AkkaResolver.Instance,
                ImmutableCollectionResolver.Instance,
                FSharpResolver.Instance,
                TypelessContractlessStandardResolver.Instance
                );
        }

        public MsgPackFSharpSerializer(ExtendedActorSystem system) : this(system, MsgPackSerializerSettings.Default)
        {
        }

        public MsgPackFSharpSerializer(ExtendedActorSystem system, Config config) 
            : this(system, MsgPackSerializerSettings.Create(config))
        {
        }

        public MsgPackFSharpSerializer(ExtendedActorSystem system, MsgPackSerializerSettings settings) : base(system)
        {
            LocalSystem.Value = system;
            _settings = settings;
        }

        public override byte[] ToBinary(object obj)
        {
            Type t = obj.GetType(); 

            if (_settings.EnableLz4Compression)
            {
                return LZ4MessagePackSerializer.NonGeneric.Serialize(t, obj);
            }
            else
            {
                return MessagePackSerializer.NonGeneric.Serialize(t, obj);
            }
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            if (_settings.EnableLz4Compression)
            {
                return LZ4MessagePackSerializer.NonGeneric.Deserialize(type, bytes);
            }
            else
            {
                return MessagePackSerializer.NonGeneric.Deserialize(type, bytes);
            }
        }

        public override int Identifier => 150;

        public override bool IncludeManifest => true;
    }
}
