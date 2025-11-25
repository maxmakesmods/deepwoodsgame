using K4os.Compression.LZ4;
using LiteNetLib.Layers;
using System;
using System.Diagnostics;
using System.Net;

namespace DeepWoods.Network.LAN
{
    internal class DeepWoodsPacketLayer : PacketLayerBase
    {
        private readonly XorEncryptLayer encryptLayer;

        public DeepWoodsPacketLayer(string encryptionKey)
            : base(0)
        {
            encryptLayer = new(encryptionKey);
        }

        public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
        {
            int initialLength = length;

            // first compression
            byte[] compressed = new byte[LZ4Codec.MaximumOutputSize(length) + sizeof(ushort)];
            BitConverter.TryWriteBytes(compressed, (ushort)length);
            int compressedLength = LZ4Codec.Encode(data, offset, length, compressed, sizeof(ushort), compressed.Length - sizeof(ushort), LZ4Level.L00_FAST);

            data = compressed;
            offset = 0;
            length = compressedLength + sizeof(ushort);

            // then encryption
            encryptLayer.ProcessOutBoundPacket(ref endPoint, ref data, ref offset, ref length);

            //Debug.WriteLine($"ProcessOutBoundPacket: {initialLength} -> {length} ({length * 100 / initialLength}%)");
        }

        public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
        {
            int initialLength = length;

            // first decryption
            encryptLayer.ProcessInboundPacket(ref endPoint, ref data, ref length);

            if (length < sizeof(ushort))
            {
                Debug.WriteLine($"length < sizeof(ushort)");
                length = 0;
                return;
            }

            // then decompression
            ushort allegedDecompressedLength = BitConverter.ToUInt16(data);

            byte[] decompressed = new byte[allegedDecompressedLength];
            int decompressedLength = LZ4Codec.Decode(data, sizeof(ushort), length - sizeof(ushort), decompressed, 0, decompressed.Length);

            if (decompressedLength <= 0)
            {
                Debug.WriteLine($"decompressedLength <= 0");
                length = 0;
                return;
            }

            data = decompressed;
            length = decompressedLength;

            //Debug.WriteLine($"ProcessInboundPacket: {initialLength} -> {length}");
        }
    }
}
