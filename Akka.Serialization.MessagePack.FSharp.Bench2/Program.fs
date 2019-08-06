// Learn more about F# at http://fsharp.org

namespace Akka.Serialization.MessagePack.Benchmarks

open System
open BenchmarkDotNet.Attributes
open Akka.Configuration
open Akka
open Akka.Actor
open Akka.Serialization.MessagePack
open Akka.Serialization

[<MemoryDiagnoser>]
type SerializationBenchmark() =
    let config = ConfigurationFactory.ParseString("akka.suppress-json-serializer-warning=true");
    let system = ActorSystem.Create("SerializationBenchmarks", config);

    let msgPackSerializer = new MsgPackSerializer(system.AsInstanceOf<ExtendedActorSystem>());
    let hyperionSerializer = new HyperionSerializer(system.AsInstanceOf<ExtendedActorSystem>(), 
        new HyperionSerializerSettings(false, false, typeof(SimpleTypesProvider)));
    let newtonSoftJsonSerializer = new NewtonSoftJsonSerializer(system.AsInstanceOf<ExtendedActorSystem>());

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
