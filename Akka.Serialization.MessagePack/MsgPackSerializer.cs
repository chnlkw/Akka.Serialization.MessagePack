//-----------------------------------------------------------------------
// <copyright file="MsgPackSerializer.cs" company="Akka.NET Project">
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

namespace Akka.Serialization.MessagePack
{
    public sealed class MsgPackSerializer : Serializer
    {
        internal static AsyncLocal<ActorSystem> LocalSystem = new AsyncLocal<ActorSystem>();
        private readonly MsgPackSerializerSettings _settings;

        static MsgPackSerializer()
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

        public MsgPackSerializer(ExtendedActorSystem system) : this(system, MsgPackSerializerSettings.Default)
        {
        }

        public MsgPackSerializer(ExtendedActorSystem system, Config config) 
            : this(system, MsgPackSerializerSettings.Create(config))
        {
        }

        public MsgPackSerializer(ExtendedActorSystem system, MsgPackSerializerSettings settings) : base(system)
        {
            LocalSystem.Value = system;
            _settings = settings;
        }

        private Type GetTypeForSerializer(object obj)
        {
            var t = obj.GetType();
            if (FSharpType.IsUnion(t, null))
            {
#if NETSTANDARD2_0
                if (FSharpType.IsUnion(t.BaseType, null))
                {
                    return t.BaseType;
                }
#else
                throw new NotSupportedException(string.Format("Cannot get the BaseType of {0}", t));
#endif
            }

            return t;
        }

        public override byte[] ToBinary(object obj)
        {
            if (_settings.EnableLz4Compression)
            {
                return LZ4MessagePackSerializer.NonGeneric.Serialize(GetTypeForSerializer(obj), obj);
            }
            else
            {
                var t = obj.GetType();
                return MessagePackSerializer.NonGeneric.Serialize(GetTypeForSerializer(obj), obj);
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
