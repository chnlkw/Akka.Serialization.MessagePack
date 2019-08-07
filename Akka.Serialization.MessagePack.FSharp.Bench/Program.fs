// Learn more about F# at http://fsharp.org

module Akka.Serialization.MessagePack.Benchmarks

open System
open BenchmarkDotNet.Attributes
open Akka.Configuration
open MessagePack
open MessagePack.Resolvers
open MessagePack.ImmutableCollection
open MessagePack.FSharp
open Akka
open Akka.Actor
open Akka.Serialization.MessagePack
open Akka.Serialization
open Akka.Util.Internal
open Akka.Serialization.MessagePack.FSharp
open BenchmarkDotNet
open BenchmarkDotNet.Running
open Microsoft.FSharp.Reflection

[<Struct; MessagePackObject>]
type SDU =
    | A of A : unit
    | B of B : int
    | C of C : string

[<MessagePackObject>]
type DU =
    | A
    | AU of unit
    | B of int
    | C of string

[<MessagePackObject>]
type record = {
    [<Key(0)>]
    A : int;
    [<Key(1)>]
    B : float
}

[<MemoryDiagnoser>]
type SerializationBenchmarks() =
    static do
        CompositeResolver.RegisterAndSetAsDefault(
          ImmutableCollectionResolver.Instance,
          FSharpResolver.Instance,
          StandardResolver.Instance
        )

    let config = ConfigurationFactory.ParseString("akka.suppress-json-serializer-warning=true");
    let system = ActorSystem.Create("SerializationBenchmarks", config);
    
    let v_arr = [|1;2;3;4;5|] 
    let v_a = DU.A
    let s_a = SDU.A ()
    let v_b = B 1
    let v_c = "abcde"
    let v_r = {record.A = 1; B = 1.0}
    let v_i = 1

    let serializerFactory = [|
            fun () -> new MsgPackSerializer(system.AsInstanceOf<ExtendedActorSystem>()) :> Serializer;
            fun () -> new MsgPackFSharpSerializer(system.AsInstanceOf<ExtendedActorSystem>())  :> Serializer;
            fun () -> new NewtonSoftJsonSerializer(system.AsInstanceOf<ExtendedActorSystem>()) :> Serializer
        |]

    [<Params(0,1,2)>]
    member val public serializer_id = 0 with get, set

    member val serializer : Serializer = null

    [<GlobalSetup>]
    member self.SetupData () =
        self.serializer = serializerFactory.[self.serializer_id] ()

    member self.convert (x:'a) =
        let bytes = self.serializer.ToBinary x 
        self.serializer.FromBinary<'a> bytes

//    let msgconvert (x: 'a) =
//        let bin = MessagePackSerializer.Serialize(x)
//        MessagePackSerializer.Deserialize<'a>(bin)
//
//    let msgconvert_nongeneric (x: 'a) =
//        let bin = MessagePackSerializer.NonGeneric.Serialize(typeof<'a>, x)
//        MessagePackSerializer.NonGeneric.Deserialize(typeof<'a>, bin) :?> 'a

//    [<Benchmark>]
//    member self.Direct_serialize_record () =
//        msgconvert v_r
//
//    [<Benchmark>]
//    member self.MsgPack_serialize_record () =
//        convert msgPackSerializer v_r
//        
//    [<Benchmark>]
//    member self.MsgPackFSharp_serialize_record () =
//        convert msgPackFSharpSerializer v_r
//        
//    [<Benchmark>]
//    member self.Newton_serialize_record () =
//        convert newtonSoftJsonSerializer v_r
//
//    [<Benchmark>]
//    member self.Direct_serialize_array () =
//        msgconvert v_arr
//
//    [<Benchmark>]
//    member self.MsgPack_serialize_array () =
//        convert msgPackSerializer v_arr
//        
//    [<Benchmark>]
//    member self.MsgPackFSharp_serialize_array () =
//        convert msgPackFSharpSerializer v_arr
//        
//    [<Benchmark>]
//    member self.Newton_serialize_array () =
//        convert newtonSoftJsonSerializer v_arr
//           
//    [<Benchmark>]
//    member self.Direct_serialize_du () =
//        msgconvert v_a
//
//    [<Benchmark>]
//    member self.Direct_serialize_struct_du () =
//        msgconvert s_a
//
//    [<Benchmark>]
//    member self.Direct_serialize_du_nongeneric () =
//        msgconvert_nongeneric v_a
//
    [<Benchmark>]
    member self.MsgPack_serialize_du () =
        self.convert v_a
//        
//    [<Benchmark>]
//    member self.MsgPackFSharp_serialize_du () =
//        convert msgPackFSharpSerializer v_a
//        
//    [<Benchmark>]
//    member self.Newton_serialize_du () =
//        convert newtonSoftJsonSerializer v_a
//     
//    [<Benchmark>]
//    member self.GetTypeForSerializer_du () =
//        MsgPackFSharpSerializer.GetTypeForSerializer v_a
//
//    [<Benchmark>]
//    member self.GetTypeForSerializer_int () =
//        MsgPackFSharpSerializer.GetTypeForSerializer v_i
//        
//    [<Benchmark>]
//    member self.isunion_fsharp_du () =
//        FSharpType.IsUnion <| (v_a :> obj).GetType()
//
//    [<Benchmark>]
//    member self.isunion_fsharp_int () =
//        FSharpType.IsUnion <| (v_i :> obj).GetType()
//
//    [<Benchmark>]
//    member self.basetype_fsharp_du () =
//        (v_a :> obj).GetType().BaseType
//
//    [<Benchmark>]
//    member self.basetype_fsharp_int () =
//        (v_i :> obj).GetType().BaseType


[<EntryPoint>]
let main argv =
    let result = BenchmarkRunner.Run<SerializationBenchmarks>()
    printfn "result %A" result
    0 // return an integer exit code
