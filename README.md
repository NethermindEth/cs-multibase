# Multiformats Multibase

[![Nethermind.Multiformats.Base](https://img.shields.io/nuget/v/Nethermind.Multiformats.Base)](https://www.nuget.org/packages/Nethermind.Multiformats.Base)

[Multibase](https://github.com/multiformats/multibase) implementation in C#.

As stated in the specs, multibase encoded strings are prefixed with an identifier for the base it's encoded in.

## Table of Contents

- [Install](#install)
- [Usage](#usage)
- [Supported base encodings](#supported-base-encodings)
- [Maintainers](#maintainers)
- [Contribute](#contribute)
- [License](#license)

## Install

  PM> Install-Package Multiformats.Base

  CLI> dotnet install Multiformats.Base

## Usage
``` csharp
using Multiformats.Base;
using System.Text;

string encoded = Multibase.Encode(MultibaseEncoding.Base32Lower, Encoding.UTF8.GetBytes("hello world"));
// bnbswy3dpeb3w64tmmq
byte[] decoded = Multibase.Decode(encoded, out MultibaseEncoding encoding);
```

## Supported base encodings

* Identity
* Base2
* Base8
* Base10
* Base16
* Base32
  * Lower / Upper
  * Padded Lower / Upper
  * Hex Lower / Upper
  * Hex Padded Lower / Upper
  * Z-Base32
* Base58
  * Bitcoin
  * Flickr
* Base64
  * Unpadded / Padded
  * UrlSafe Unpadded / Padded

