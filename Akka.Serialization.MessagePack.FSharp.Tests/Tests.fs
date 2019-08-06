module Tests

open System
open Xunit
open Akka.TestKit.Xunit2
open Akka.Serialization.Testkit.Util
open Akka.Serialization.MessagePack
open MessagePack

[<MessagePackObject>]
type SimpleUnion =
  | A
  | B of int
  | C of int64 * float32
  
[<Struct; MessagePackObject>]
type StructUnion =
    | E
    | F of Prop1 : int
    | G of Prop2 : int64 * Prop3: float32
  
[<MessagePackObject>]
type Tree =
  | Leaf of int
  | Node of Tree * Tree

type StringKeyUnion = | D of Prop : int

[<AbstractClass>]
type FsharpTests(serializerType : Type) =
    inherit TestKit(ConfigFactory.GetConfig(serializerType))

    member __.check (x:'a) =
        let serializer = base.Sys.Serialization.FindSerializerForType typeof<'a>
        let bytes = serializer.ToBinary x
        let y : 'a = serializer.FromBinary<'a> bytes
        Assert.Equal<'a>(x, y)

    [<Fact>]
    member __.``Can Serialize fSharp String``() =
        __.check "string"

    [<Fact>]
    member __.``Can Serialize Simple DU Using Base MsgPackFSharp`` () =
        let chk (x: 'a) =
            let bin = MessagePackSerializer.Serialize(x)
            let y : 'a = MessagePackSerializer.Deserialize<'a>(bin)
            Assert.Equal<'a>(x, y)
        let chk1 (x: 'a) =
            let bin = MessagePackSerializer.NonGeneric.Serialize(typeof<'a>, x)
            let y : 'a = MessagePackSerializer.NonGeneric.Deserialize(typeof<'a>, bin) :?> 'a
            Assert.Equal<'a>(x, y)
        chk A
        chk1 A
        chk <| B 100
        chk1 <| B 100

    [<Fact>]
    member __.``Can Serialize Simple DU`` () =

        __.check A
        __.check <| B 100
        __.check <| C(99999999L, -123.43f)

        
    [<Fact>]
    member __.``string key`` () =
        __.check <| D 1

    [<Fact>]
    member __.``Can Serialize Tree DU`` () =

        __.check <| Leaf 1
        __.check <| Node (Leaf 2, Leaf 3)
        __.check <| Node (Node (Leaf 4, Leaf 5), Leaf 6)

    [<Fact>]
    member __.``struct `` () =
        __.check E
        __.check <| F 100
        __.check <| G (99999999L, -123.43f)

type MsgPackFSharpTests() =
    inherit FsharpTests(typeof<MsgPackSerializer>)
