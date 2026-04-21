# ZOVserver.Basic.Titan.Streams

Hochleistungs-Serialisierungs-Suite fuer den ZOVserver. Zwei spezialisierte Stream-Implementierungen fuer das
Brawl-Stars-Protokoll - gebaut fuer Speed und minimale Speicherfussabdruecke.

## Stream-Typen im Überblick

| Stream     | Einsatzgebiet                     | Key-Features                                            |
|------------|-----------------------------------|---------------------------------------------------------|
| ByteStream | Lobby-Pakete, File-Serialisierung | Primitive, Strings, gepoolte Buffer, robustes Handling  |
| BitStream  | Battle-Sessions (In-Match)        | Bit-Level-Accuracy, gepackte Booleans, VInt-Optimierung |

## Benchmarks

Gemacht mit BenchmarkDotNet.

### ByteStream - Lobby & Data Serialization

| Methode                                   | Mean           | Error         | StdDev        |    Gen0 | Allocated | Code Size |
|:------------------------------------------|----------------|---------------|---------------|--------:|----------:|----------:|
| WriteI64_1000                             | 2,621 ns       | 0,3241 ns     | 0,1929 ns     |       - |         - |   3.180 B |
| ReadU8_1000                               | 4,183 ns       | 0,1183 ns     | 0,0782 ns     |       - |         - |   4.218 B |
| WriteVInt32_1000_Small                    | 4,741 ns       | 0,1879 ns     | 0,1118 ns     |       - |         - |   4.287 B |
| ReadI32_1000                              | 5,658 ns       | 0,4739 ns     | 0,2820 ns     |       - |         - |   5.460 B |
| ReadVInt32_1000_Small                     | 9,812 ns       | 0,8408 ns     | 0,4397 ns     |       - |         - |   5.738 B |
| WriteVInt32_1000_Large                    | 10,949 ns      | 0,2544 ns     | 0,1331 ns     |       - |         - |   4.619 B |
| WriteBytes_Small_1000                     | 11,083 ns      | 0,3007 ns     | 0,1789 ns     |       - |         - |   5.027 B |
| WriteI32_1000                             | 15,017 ns      | 26,8817 ns    | 17,7806 ns    |       - |         - |   3.993 B |
| WriteU8_1000                              | 15,054 ns      | 23,5944 ns    | 15,6062 ns    |       - |         - |   4.030 B |
| WriteBoolean_1000                         | 18,380 ns      | 10,5779 ns    | 6,2947 ns     |       - |         - |   4.108 B |
| ReadBytes_Small_1000                      | 21,757 ns      | 0,3763 ns     | 0,2239 ns     |       - |         - |   6.276 B |
| Scenario_Endianness_Mixed                 | 22,369 ns      | 0,9911 ns     | 0,5184 ns     |       - |         - |   5.255 B |
| WriteVInt64_1000                          | 22,759 ns      | 3,1547 ns     | 1,8773 ns     |       - |         - |   5.054 B |
| ReadBoolean_1000                          | 23,656 ns      | 23,4441 ns    | 15,5069 ns    |       - |         - |   4.508 B |
| WriteString_Short_1000                    | 63,044 ns      | 1,1100 ns     | 0,5806 ns     |  0,0070 |      40 B |   9.978 B |
| ReadString_Short_1000                     | 94,487 ns      | 3,7842 ns     | 1,9792 ns     |  0,0150 |      80 B |  10.276 B |
| Scenario_SerializeDeserialize_1000Objects | 112,289 ns     | 1,4525 ns     | 0,9607 ns     |  0,0150 |      80 B |  17.661 B |
| WriteMixedTypes_100                       | 152,731 ns     | 70,1803 ns    | 36,7057 ns    |  0,0100 |      73 B |  13.377 B |
| WriteI128_100                             | 166,232 ns     | 220,5943 ns   | 145,9095 ns   |       - |         - |   5.357 B |
| WriteBytesWithoutLength_Medium_100        | 177,557 ns     | 1,6210 ns     | 0,9646 ns     |       - |       2 B |     802 B |
| Scenario_NetworkPacket_1000               | 179,995 ns     | 2,5770 ns     | 1,7046 ns     |  0,0270 |     144 B |  14.898 B |
| WriteBytes_Medium_100                     | 198,273 ns     | 82,0500 ns    | 54,2710 ns    |       - |       2 B |   5.852 B |
| WriteString_Long_100                      | 15.305,624 ns  | 104,3698 ns   | 62,1088 ns    |  0,0100 |     103 B |   9.592 B |
| WriteCompressedString_100                 | 33.267,427 ns  | 767,9910 ns   | 457,0192 ns   |  5,8400 |  30.681 B |   6.154 B |
| ReadCompressedString_100                  | 63.202,208 ns  | 2.957,7974 ns | 1.956,3999 ns | 28,8200 | 151.266 B |   8.493 B |
| WriteBytes_Large_10                       | 316.658,832 ns | 2.067,9133 ns | 1.367,7966 ns |       - |   1.682 B |   5.981 B |

### BitStream - Battle-Engine Precision

| Methode                               | Mean      | Error     | StdDev    |   Gen0 | Allocated |
|:--------------------------------------|-----------|-----------|-----------|-------:|----------:|
| WriteBoolean_1000                     | 14,17 ns  | 0,448 ns  | 0,266 ns  |      - |         - |
| WritePositiveInt_4bits_1000           | 15,91 ns  | 0,814 ns  | 0,539 ns  |      - |         - |
| WritePositiveInt_8bits_1000           | 16,50 ns  | 0,391 ns  | 0,204 ns  |      - |         - |
| WritePositiveInt_16bits_1000          | 18,97 ns  | 8,466 ns  | 5,038 ns  |      - |         - |
| WriteInt_1bit_1000                    | 23,23 ns  | 15,915 ns | 10,527 ns |      - |         - |
| ReadPositiveInt_4bits_1000            | 24,29 ns  | 6,650 ns  | 3,957 ns  |      - |       4 B |
| ReadBoolean_1000                      | 25,83 ns  | 3,261 ns  | 1,941 ns  |      - |       4 B |
| WriteInt_15bits_1000                  | 26,41 ns  | 6,221 ns  | 4,115 ns  |      - |         - |
| ReadPositiveInt_8bits_1000            | 26,67 ns  | 11,196 ns | 7,406 ns  |      - |       4 B |
| WriteIntMax65535_1000                 | 26,78 ns  | 0,286 ns  | 0,149 ns  |      - |         - |
| ReadPositiveInt_16bits_1000           | 27,65 ns  | 14,341 ns | 9,486 ns  |      - |       4 B |
| WritePositiveVIntMax255OftenZero_1000 | 31,18 ns  | 0,901 ns  | 0,596 ns  |      - |         - |
| ReadPositiveVIntMax255OftenZero_1000  | 38,42 ns  | 5,806 ns  | 3,037 ns  |      - |       4 B |
| Scenario_ByteBoundaryStress_10000     | 48,02 ns  | 0,724 ns  | 0,431 ns  | 0,0004 |       2 B |
| ReadPositiveVInt_1000_Small           | 51,36 ns  | 1,095 ns  | 0,652 ns  |      - |       4 B |
| ReadPositiveVInt_1000_Large           | 64,39 ns  | 2,037 ns  | 1,347 ns  |      - |       4 B |
| Scenario_AlternatingBitSizes_1000     | 91,38 ns  | 3,338 ns  | 2,208 ns  | 0,0010 |       6 B |
| Scenario_CompressedBitStream_1000     | 104,26 ns | 2,411 ns  | 1,595 ns  |      - |       2 B |
| Scenario_VIntOptimization_1000        | 134,35 ns | 1,963 ns  | 1,298 ns  |      - |       2 B |

## Quality Assurance

Getestet im Projekt `ZOVserver.Basic.Titan.Streams.Tests`.

- 70 Unit Tests ausgefuehrt (100% Success)
- Komplette Coverage fuer Boundary Conditions, Buffer Expansions und protokollspezifische Edge Cases

## Technische Details

### Optimierte Serialisierung

- Zero-Allocation Primitives: Alle Basistypen laufen ueber `Span<T>` und direkten Speicherzugriff. Keine
  Heap-Allokationen.
- Aggressive Inlining: Methoden nutzen `MethodImplOptions.AggressiveInlining` zur Reduzierung `BitStream` des Call-Stack-Overheads.
- Bit-Level Packing: Die `BitStream`-Engine arbeitet mit manuellem Bit-Shifting und Maskierung.

### Memory Management

- Buffer Pooling: Integration mit `ArrayPool<byte>` fuer grosse Buffer im `ByteStream`. Reduziert GC-Druck unter Last.
- Predictive Expansion: Wachstumsstrategie mit minimalen Re-Allokationen beim Skalieren.

## License

MIT License - siehe LICENSE Datei im Repository.

## Disclaimer

Dieses Projekt ist nicht offiziell mit Supercell verbunden, wird von Supercell nicht unterstuetzt und erhebt keinen Anspruch auf Markenrechte. Brawl Stars ist eine eingetragene Marke von Supercell. Dies ist ein reines Community-Projekt fuer Reverse-Engineering- und Forschungszwecke.
