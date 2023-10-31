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

public static partial class SaveFileWriter
{
    /// <summary>
    ///     Creates a new save file containg the given <paramref name="data"/>
    /// </summary>
    /// <param name="path">
    ///     The path and extension of the file to create by this method.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to wriate to the save file created by this
    ///     method.
    /// </param>
    public static void ToBin(string path, ReadOnlySpan<byte> data)
    {
        using FileStream fileStream = File.Create(path);
        ToBin(fileStream, data);
    }


    /// <summary>
    ///     Creates a new save file containg the given <paramref name="data"/>
    /// </summary>
    /// <param name="stream">
    ///     The <see cref="System.IO.FileStream"/> to write the save data.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to wriate to the save file created by this
    ///     method.
    /// </param>
    public static void ToBin(FileStream fileStream, ReadOnlySpan<byte> data)
    {
        byte[] compressed = Zlib.Compress(data);
        int len = compressed.Length;
        uint checksum = CRC32.Calculate(compressed);

        using BinaryWriter writer = new BinaryWriter(fileStream);

        //  Write signature
        writer.Write((byte)'S');
        writer.Write((byte)'A');
        writer.Write((byte)'V');
        writer.Write((byte)'E');

        //  Write the total number of bites expected in the data part
        writer.Write(len);

        //Write the data part
        writer.Write(compressed);

        //  Write the checksum so we can validate when reading in.
        writer.Write(checksum);
    }
}