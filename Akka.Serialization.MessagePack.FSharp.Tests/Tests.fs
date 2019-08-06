module Tests

open System
open Xunit

open MessagePack
open MessagePack.Resolvers
open MessagePack.ImmutableCollection
open MessagePack.FSharp

open Akka.TestKit.Xunit2
open Akka.Serialization.Testkit.Util
open Akka.Serialization.MessagePack
open Akka.Serialization.MessagePack.FSharp

[<MessagePackObject>]
type SimpleUnion =
  | A of unit
  | B of int
  | C of int * float

[<MessagePackObject>]
type ArgumentedUnion =
  | AA of AA : unit
  | AB of AB : int
  | AC of AC1 : int * AC2 : float
  
[<Struct; MessagePackObject>]
type StructUnion =
    | E
    | F of Prop1 : int
    | G of Prop2 : int64 * Prop3: float32
  

[<MessagePackObject>]
type record = {
    [<Key(0)>]
    A : int;
    [<Key(1)>]
    B : float
}


[<MessagePackObject>]
type Tree =
  | Leaf of int
  | Node of Tree * Tree

type StringKeyUnion = | D of Prop : int

[<Fact>]
let ``DU instance type`` () =
    let a = A
    Assert.Equal(typeof<SimpleUnion>, a.GetType().BaseType)
    let aa = AA ()
    Assert.Equal(typeof<ArgumentedUnion>, aa.GetType().BaseType)

[<AbstractClass>]
type FSharpTests(serializerType : Type) =
    inherit TestKit(ConfigFactory.GetConfig(serializerType))

    let chk (x: 'a) =
        let bin = MessagePackSerializer.Serialize x
        let y : 'a = MessagePackSerializer.Deserialize bin
        Assert.Equal<'a>(x, y)
        
    let chk1 (x: 'a) =
        let bin = MessagePackSerializer.NonGeneric.Serialize(typeof<'a>, x)
        let y : 'a = MessagePackSerializer.NonGeneric.Deserialize(typeof<'a>, bin) :?> 'a
        Assert.Equal<'a>(x, y)

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
        chk <| A ()
        chk1 <| A ()
        chk <| B 100
        chk1 <| B 100

    [<Fact>]
    member __.``Can Serialize Argumented DU Using Base MsgPackFSharp`` () =
        chk  <| AA ()
        chk1 <| AA ()
        chk  <| AB 100
        chk1 <| AB 100
        chk  <| AC (100, 1.0)
        chk1 <| AC (100, 1.0)

    [<Fact>]
    member __.``Can Serialize Simple Record Using Base MsgPackFSharp`` () =
        chk <| { record.A=1; B=1.0 }
        chk1 <| { record.A=1; B=1.0 }

    [<Fact>]
    member __.``Can Serialize Simple DU`` () =

        __.check <| A ()
        __.check <| B 100
        __.check <| C(99999999, -123.43)

    [<Fact>]
    member __.``Can Serialize Argumented DU`` () =
         __.check <| AA ()
         __.check <| AA ()
         __.check <| AB 100
         __.check <| AB 100
         __.check <| AC (100, 1.0)
         __.check <| AC (100, 1.0)


    [<Fact>]
    member __.``DU string key`` () =
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

    [<Fact>]
    member __.``record `` () =
        __.check <| { record.A=1; B=1.0 }

type MsgPackFSharpTests() =
    inherit FSharpTests(typeof<MsgPackFSharpSerializer>)

type MsgPackTests_MUST_RUN_STANDALONE_WITHOUT_MsgPackFSharpTests() =
    inherit FSharpTests(typeof<MsgPackSerializer>)

//type MessagePackFSharpTests() =
//    let chk (x: 'a) =
//        let bin = MessagePackSerializer.Serialize x
//        let y : 'a = MessagePackSerializer.Deserialize bin
//        Assert.Equal<'a>(x, y)
//    let chk1 (x: 'a) =
//        let bin = MessagePackSerializer.NonGeneric.Serialize(typeof<'a>, x)
//        let y : 'a = MessagePackSerializer.NonGeneric.Deserialize(typeof<'a>, bin) :?> 'a
//        Assert.Equal<'a>(x, y)
//
//    static do
//        CompositeResolver.RegisterAndSetAsDefault(
//          ImmutableCollectionResolver.Instance,
//          FSharpResolver.Instance,
//          StandardResolver.Instance
//        )
//
//    [<Fact>]
//    member __.``Simple DU`` () =
//        chk <| B 100
//        chk1 <| B 100
//        chk A
//        chk1 A
//
//
//    [<Fact>]
//    member __.``Struct DU`` () =
//        chk E
//        chk1 E
//        chk <| F 100
//        chk1 <| F 100
//        chk <| G (99999999L, -123.43f)
//        chk1 <| G (99999999L, -123.43f)
//
//
//    [<Fact>]
//    member __.``Simple Record`` () =
//        chk <| { record.A=1; B=1.0 }
//        chk1 <| { record.A=1; B=1.0 }
