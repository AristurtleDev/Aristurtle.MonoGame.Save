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

using Aristurtle.MonoGame.Save.Compression;

namespace Aristurtle.MonoGame.Save;

/// <summary>
///     Contains methods for reading save file data written with this library.
/// </summary>
public static partial class SaveFileReader
{
    /// <summary>
    ///     Reads the save data from the file specififed.
    /// </summary>
    /// <param name="path">
    ///     The path and extension of the file to read.
    /// </param>
    /// <returns>
    ///     A <see cref="T:byte[]"/> containing the save data read from the SAVE chunk of the .png file, if a SAVE
    ///     chunk exists; otherwise <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidSignatureException">
    ///     Thrown if the header of the file being read does not match the expected signature.
    /// </exception>
    /// <exception cref="InvalidCrcException">
    ///     Thrown if the CRC checksum of the save data read from the file does not match the expected CRC
    ///     checksum stored in the file
    /// </exception>
    public static byte[] FromBin(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return FromBin(stream);
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
    public static byte[] FromBin(FileStream input)
    {
        //  Validate that this is a save file created by this library by reading the header signature of the
        //  file and compare to to our signature
        ReadOnlySpan<byte> expectedSignature = stackalloc byte[4] { 0x53, 0x41, 0x56, 0x45 };
        Span<byte> actualSignature = stackalloc byte[4];
        input.Read(actualSignature);

        if (!expectedSignature.SequenceEqual(actualSignature))
        {
            throw new InvalidSignatureException("This file does not appear to be a valid save file created by this library: Invalid Signature");
        }

        using BinaryReader reader = new BinaryReader(input);

        int len = reader.ReadInt32();
        byte[] compresssedData = reader.ReadBytes(len);
        uint expectedCrc = reader.ReadUInt32();

        uint actualCrc = CRC32.Calculate(compresssedData);

        if (expectedCrc != actualCrc)
        {
            throw new InvalidCrcException("Save data appears to be corrupted");
        }

        return Zlib.Decompress(compresssedData);
    }
}