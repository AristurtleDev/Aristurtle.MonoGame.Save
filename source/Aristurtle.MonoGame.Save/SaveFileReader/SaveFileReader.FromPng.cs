/* ----------------------------------------------------------------------------
MIT License

Copyright (c) 2023 Christopher Whitley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
---------------------------------------------------------------------------- */

using System.Buffers.Binary;
using Aristurtle.MonoGame.Save.Compression;

namespace Aristurtle.MonoGame.Save;

public static partial class SaveFileReader
{
    /// <summary>
    ///     Reads the save data from the SAVE chunk in a .png file
    /// </summary>
    /// <param name="path">
    ///     The path and extension of the .png file to read.
    /// </param>
    /// <returns>
    ///     A <see cref="T:byte[]"/> containing the save data read from the SAVE chunk of the .png file, if a SAVE
    ///     chunk exists; otherwise <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidSignatureException">
    ///     Thrown if the header of the file being read does not match the expected signature of a .png file.
    /// </exception>
    /// <exception cref="InvalidCrcException">
    ///     Thrown if the CRC checksum of the SAVE chunk read from the .png file does not match the expected CRC
    ///     checksum stored in the SAVE chunk.
    /// </exception>
    public static byte[]? FromPng(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return FromPng(stream);
    }

    /// <summary>
    ///     Reads the save data from the SAVE chunk in a .png file
    /// </summary>
    /// <param name="input">
    ///     The input stream used to read the .png file.
    /// </param>
    /// <returns>
    ///     A <see cref="T:byte[]"/> containing the save data read from the SAVE chunk of the .png file, if a SAVE
    ///     chunk exists; otherwise <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidSignatureException">
    ///     Thrown if the header of the file being read does not match the expected signature of a .png file.
    /// </exception>
    /// <exception cref="InvalidCrcException">
    ///     Thrown if the CRC checksum of the SAVE chunk read from the .png file does not match the expected CRC
    ///     checksum stored in the SAVE chunk.
    /// </exception>
    public static byte[]? FromPng(FileStream input)
    {
        //  Validate that this is actually a png by reading the header signature of the file and compare it to
        //  the valid png signature.
        ReadOnlySpan<byte> expectedPngSignature = stackalloc byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Span<byte> actualSignature = stackalloc byte[8];
        input.Read(actualSignature);
        if (!expectedPngSignature.SequenceEqual(actualSignature))
        {
            throw new InvalidSignatureException("This file does not appear to be a valid PNG file: Invalid Signature");
        }

        Span<byte> lenBytes = stackalloc byte[4];
        Span<byte> crcBytes = stackalloc byte[4];
        ReadOnlySpan<byte> expectedSaveSignature = stackalloc byte[4] { 0x53, 0x41, 0x56, 0x45 };

        while (input.Position != input.Length)
        {
            //  Read the length of the chunk
            input.Read(lenBytes);
            int len = BinaryPrimitives.ReadInt32BigEndian(lenBytes);

            //  The CRC calculated by the writer uses the data of both the type and the actual data written
            //  for the CRC calculation.  
            byte[] typeAndData = new byte[len + 4];

            //  Read the chunk type (4-bytes)
            input.Read(typeAndData, 0, 4);

            //  Validate this is a SAVE chunk
            if (expectedSaveSignature.SequenceEqual(typeAndData[0..4]))
            {
                //  Read the data of the chunk
                input.Read(typeAndData, 4, len);

                //  Read the chunk crc
                input.Read(crcBytes);
                int expectedCrc = BinaryPrimitives.ReadInt32BigEndian(crcBytes);

                //  Calculate the CRC of the data we read
                int actualCrc = (int)CRC32.Calculate(typeAndData);

                if (expectedCrc != actualCrc)
                {
                    throw new InvalidCrcException("Save data appears to be corrupted");
                }

                return Zlib.Decompress(typeAndData[4..]);
            }
            else
            {
                //  Move to next chunk
                input.Position += len + 4;
            }
        }

        return null;
    }
}