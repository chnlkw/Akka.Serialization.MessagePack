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

[<AbstractClass>]
type FsharpTests(serializerType : Type) =
    inherit TestKit(ConfigFactory.GetConfig(serializerType))

    member __.check (x:'a) =
        let serializer = base.Sys.Serialization.FindSerializerFor x
        let bytes = serializer.ToBinary x
        let y : 'a = serializer.FromBinary<'a> bytes
        Assert.Equal<'a>(x, y)

    [<Fact>]
    member __.``Can Serialize fSharp String``() =
        __.check "string"

    [<Fact>]
    member __.``Can Serialize Simple DU Using Base MsgPackFSharp`` () =
        let chk (x: 'a) =
            let bin = MessagePackSerializer.NonGeneric.Serialize(x.GetType(), x)
            let y : 'a = MessagePackSerializer.NonGeneric.Deserialize(x.GetType(), bin) :?> 'a
            Assert.Equal<'a>(x, y)
        let chk2 (x: 'a) =
            let bin = MessagePackSerializer.Typeless.Serialize(x)
            let y : 'a = MessagePackSerializer.Typeless.Deserialize(bin) :?> 'a
            Assert.Equal<'a>(x, y)
        chk2 A
        chk2 A
        chk2 <| B 100

    [<Fact>]
    member __.``Can Serialize Simple DU`` () =

        __.check A
        __.check <| B 100
        __.check <| C(99999999L, -123.43f)

type MsgPackFSharpTEsts() =
    inherit FsharpTests(typeof<MsgPackSerializer>)
