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

| Method                                    | Mean           | Error         | StdDev        | Median         | Gen0    | Code Size | Gen1   | Allocated |
|------------------------------------------ |---------------:|--------------:|--------------:|---------------:|--------:|----------:|-------:|----------:|
| Allocation_ReuseStream                    |      69.059 ns |     3.4960 ns |     2.3124 ns |      68.208 ns |  0.0070 |  10,110 B |      - |      40 B |
| Allocation_NewStreamEachWrite             |     106.958 ns |     2.8631 ns |     1.7038 ns |     106.040 ns |  0.0150 |  10,520 B |      - |      80 B |
|                                           |                |               |               |                |         |           |        |           |
| ReadI32_1000                              |       5.465 ns |     0.2386 ns |     0.1420 ns |       5.405 ns |       - |   5,454 B |      - |         - |
| ReadVInt32_1000_Small                     |       9.163 ns |     0.1129 ns |     0.0672 ns |       9.159 ns |       - |   5,724 B |      - |         - |
| ReadU8_1000                               |      18.206 ns |    33.2434 ns |    21.9884 ns |       4.680 ns |       - |   4,383 B |      - |         - |
| ReadBytes_Small_1000                      |      21.760 ns |     0.3368 ns |     0.2228 ns |      21.703 ns |       - |   6,261 B |      - |         - |
| ReadBoolean_1000                          |      26.823 ns |    32.4536 ns |    21.4661 ns |      24.987 ns |       - |   4,544 B |      - |         - |
| ReadString_Short_1000                     |      88.327 ns |     2.8268 ns |     1.4785 ns |      87.993 ns |  0.0150 |  10,283 B |      - |      80 B |
| ReadCompressedString_100                  |  60,897.951 ns |   419.7148 ns |   249.7655 ns |  60,863.242 ns | 28.8200 |   8,472 B | 1.3700 |  151264 B |
|                                           |                |               |               |                |         |           |        |           |
| Scenario_ExpandBuffer_FromSmall           |       2.655 ns |     0.0489 ns |     0.0291 ns |       2.660 ns |  0.0006 |   1,707 B |      - |       3 B |
| Scenario_Endianness_Mixed                 |      22.435 ns |     0.3730 ns |     0.2467 ns |      22.396 ns |       - |   5,212 B |      - |         - |
| Scenario_SerializeDeserialize_1000Objects |     116.050 ns |     1.4074 ns |     0.9309 ns |     115.987 ns |  0.0150 |  17,734 B |      - |      80 B |
| Scenario_NetworkPacket_1000               |     183.515 ns |     4.5485 ns |     3.0086 ns |     182.385 ns |  0.0270 |  14,825 B |      - |     144 B |
|                                           |                |               |               |                |         |           |        |           |
| WriteVInt32_1000_Small                    |       4.699 ns |     0.1242 ns |     0.0739 ns |       4.680 ns |       - |   4,318 B |      - |         - |
| WriteVInt32_1000_Large                    |      10.915 ns |     0.2907 ns |     0.1923 ns |      10.886 ns |       - |   4,619 B |      - |         - |
| WriteBytes_Small_1000                     |      10.933 ns |     0.1768 ns |     0.1170 ns |      10.891 ns |       - |   5,027 B |      - |         - |
| WriteI64_1000                             |      15.425 ns |    28.8608 ns |    19.0896 ns |       2.779 ns |       - |   4,026 B |      - |         - |
| WriteU8_1000                              |      15.827 ns |    22.8727 ns |    15.1289 ns |      11.844 ns |       - |   3,707 B |      - |         - |
| WriteBoolean_1000                         |      17.852 ns |    10.5039 ns |     6.2507 ns |      20.445 ns |       - |   4,072 B |      - |         - |
| WriteVInt64_1000                          |      20.465 ns |     0.3053 ns |     0.2020 ns |      20.439 ns |       - |   5,001 B |      - |         - |
| WriteI32_1000                             |      24.156 ns |    29.2263 ns |    19.3314 ns |      34.172 ns |       - |   4,004 B |      - |         - |
| WriteString_Short_1000                    |      61.659 ns |     1.9685 ns |     1.1714 ns |      61.218 ns |  0.0070 |   9,976 B |      - |      40 B |
| WriteBytesWithoutLength_Medium_100        |     168.878 ns |     1.2716 ns |     0.7567 ns |     168.618 ns |       - |     802 B |      - |         - |
| WriteI128_100                             |     173.517 ns |   199.6993 ns |   132.0887 ns |     226.535 ns |       - |   5,315 B |      - |         - |
| WriteBytes_Medium_100                     |     198.127 ns |    74.5056 ns |    44.3371 ns |     218.822 ns |       - |   5,884 B |      - |         - |
| WriteMixedTypes_100                       |     296.617 ns |   392.9726 ns |   259.9271 ns |     141.370 ns |  0.0100 |  13,486 B |      - |      72 B |
| WriteString_Long_100                      |  15,443.646 ns |   387.0201 ns |   230.3095 ns |  15,358.529 ns |       - |   9,583 B |      - |         - |
| WriteCompressedString_100                 |  32,582.903 ns |   109.1516 ns |    57.0884 ns |  32,600.576 ns |  5.8400 |   6,154 B |      - |   30680 B |
| WriteBytes_Large_10                       | 313,067.893 ns | 4,553.9210 ns | 2,709.9653 ns | 311,569.660 ns |       - |   5,981 B |      - |       4 B |

### BitStream - Battle-Engine Precision

| Method                                | Mean      | Error     | StdDev    | Median    | Gen0   | Allocated |
|-------------------------------------- |----------:|----------:|----------:|----------:|-------:|----------:|
| ReadPositiveInt_4bits_1000            |  24.52 ns |  0.573 ns |  0.300 ns |  24.39 ns |      - |       4 B |
| ReadPositiveInt_8bits_1000            |  25.15 ns |  0.281 ns |  0.147 ns |  25.15 ns |      - |       4 B |
| ReadBoolean_1000                      |  25.63 ns |  3.404 ns |  2.251 ns |  25.45 ns |      - |       4 B |
| ReadPositiveInt_16bits_1000           |  27.41 ns | 14.645 ns |  9.687 ns |  30.80 ns |      - |       4 B |
| ReadPositiveVInt_1000_Small           |  32.27 ns | 11.166 ns |  7.385 ns |  36.01 ns |      - |       4 B |
| ReadPositiveVIntMax255OftenZero_1000  |  34.35 ns | 14.443 ns |  9.553 ns |  33.92 ns |      - |       4 B |
| ReadPositiveVInt_1000_Large           |  42.29 ns |  9.212 ns |  6.093 ns |  38.27 ns |      - |       4 B |
|                                       |           |           |           |           |        |           |
| Scenario_ByteBoundaryStress_10000     |  44.07 ns |  0.684 ns |  0.452 ns |  43.88 ns | 0.0004 |       2 B |
| Scenario_CompressedBitStream_1000     |  76.60 ns |  0.392 ns |  0.205 ns |  76.65 ns |      - |       2 B |
| Scenario_AlternatingBitSizes_1000     |  91.74 ns |  7.019 ns |  4.643 ns |  89.87 ns | 0.0010 |       6 B |
| Scenario_VIntOptimization_1000        |  92.58 ns |  0.305 ns |  0.182 ns |  92.58 ns |      - |       2 B |
|                                       |           |           |           |           |        |           |
| WriteBoolean_1000                     |  14.44 ns |  0.179 ns |  0.107 ns |  14.47 ns |      - |         - |
| WritePositiveInt_4bits_1000           |  15.55 ns |  0.139 ns |  0.073 ns |  15.54 ns |      - |         - |
| WritePositiveInt_8bits_1000           |  16.51 ns |  0.349 ns |  0.207 ns |  16.54 ns |      - |         - |
| WritePositiveInt_16bits_1000          |  20.17 ns |  7.241 ns |  4.309 ns |  22.20 ns |      - |         - |
| WriteInt_1bit_1000                    |  23.23 ns | 16.683 ns | 11.035 ns |  16.18 ns |      - |         - |
| WritePositiveVIntMax255OftenZero_1000 |  26.45 ns | 16.782 ns | 11.100 ns |  19.48 ns |      - |         - |
| WriteIntMax65535_1000                 |  27.13 ns |  0.415 ns |  0.275 ns |  27.10 ns |      - |         - |
| WritePositiveVInt_1000_Small          |  27.89 ns | 11.070 ns |  7.322 ns |  22.90 ns |      - |         - |
| WritePositiveVIntMax255_1000          |  28.78 ns | 11.150 ns |  7.375 ns |  26.34 ns |      - |         - |
| WritePositiveInt_27bits_1000          |  29.42 ns |  9.411 ns |  6.225 ns |  24.81 ns |      - |         - |
| WritePositiveVIntMax65535_1000        |  33.40 ns | 12.607 ns |  8.339 ns |  38.86 ns |      - |         - |
| WriteInt_15bits_1000                  |  34.43 ns | 23.788 ns | 15.734 ns |  33.77 ns |      - |         - |
| WritePositiveVInt_1000_Large          |  35.18 ns |  0.448 ns |  0.267 ns |  35.23 ns |      - |         - |
| WriteMixedTypes_100                   | 137.10 ns |  3.340 ns |  2.209 ns | 136.94 ns |      - |         - |

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
