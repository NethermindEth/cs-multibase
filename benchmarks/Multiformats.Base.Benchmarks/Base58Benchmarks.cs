using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Multiformats.Base.Benchmarks
{
    [MemoryDiagnoser]
    public class Base58Benchmarks
    {
        private const int Threads = 8;
        private const int OperationsPerThread = 1000;

        private byte[] _ed25519PeerId;
        private byte[] _sha256PeerId;
        private string _ed25519Text;
        private string _sha256Text;

        [GlobalSetup]
        public void Setup()
        {
            _ed25519PeerId = new byte[38];
            _ed25519PeerId[1] = 0x24;
            _ed25519PeerId[2] = 0x08;
            _ed25519PeerId[3] = 0x01;
            _ed25519PeerId[4] = 0x12;
            _ed25519PeerId[5] = 0x20;
            for (int i = 6; i < _ed25519PeerId.Length; i++)
                _ed25519PeerId[i] = (byte)(i * 37 + 11);

            _sha256PeerId = new byte[34];
            _sha256PeerId[0] = 0x12;
            _sha256PeerId[1] = 0x20;
            for (int i = 2; i < _sha256PeerId.Length; i++)
                _sha256PeerId[i] = (byte)(i * 53 + 7);

            _ed25519Text = Multibase.Base58.Encode(_ed25519PeerId);
            _sha256Text = Multibase.Base58.Encode(_sha256PeerId);
        }

        [Benchmark]
        public string EncodeEd25519() => Multibase.Base58.Encode(_ed25519PeerId);

        [Benchmark]
        public string EncodeSha256() => Multibase.Base58.Encode(_sha256PeerId);

        [Benchmark]
        public byte[] DecodeEd25519() => Multibase.Base58.Decode(_ed25519Text);

        [Benchmark]
        public byte[] DecodeSha256() => Multibase.Base58.Decode(_sha256Text);

        [Benchmark(OperationsPerInvoke = Threads * OperationsPerThread)]
        public void DecodeEd25519Threads8()
        {
            string text = _ed25519Text;
            Parallel.For(0, Threads, new ParallelOptions { MaxDegreeOfParallelism = Threads }, _ =>
            {
                for (int i = 0; i < OperationsPerThread; i++)
                    Multibase.Base58.Decode(text);
            });
        }
    }
}
