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


using Aristurtle.MonoGame.Save.IO.Png;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aristurtle.MonoGame.Save;

public static partial class SaveFileWriter
{
    /// <summary>
    ///     Creates a new .png file using the given <paramref name="texture"/> and embeds the given
    ///     <paramref name="data"/> into the .png as a SAVE chunk.
    /// </summary>
    /// <param name="path">
    ///     The path and extension of the file to create by this method.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to embed in the SAVE chunk of the .png
    ///     created by this method.
    /// </param>
    /// <param name="texture">
    ///     A <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> instance that represents the image data to
    ///     write to the .png created by this method.
    /// </param>
    public static void ToPng(string path, ReadOnlySpan<byte> data, Texture2D texture)
    {
        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData<Color>(pixels);
        ToPng(path, data, texture.Width, texture.Height, pixels);
    }

    /// <summary>
    ///     Creates a new .png file using the given <paramref name="texture"/> and embeds the give
    ///     <paramref name="data"/> into the .png as a SAVE chunk.
    /// </summary>
    /// <param name="stream">
    ///     The <see cref="System.IO.FileStream"/> to write the .png too.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to embed in the SAVE chunk of the .png.
    /// </param>
    /// <param name="texture">
    ///     A <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> instance that represents the image data to
    ///     write to the .png.
    /// </param>
    public static void ToPng(FileStream stream, ReadOnlySpan<byte> data, Texture2D texture)
    {
        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData<Color>(pixels);
        ToPng(stream, data, texture.Width, texture.Height, pixels);
    }

    /// <summary>
    ///     Creates a new .png file using the given <paramref name="texture"/> and embeds the give
    ///     <paramref name="data"/> into the .png as a SAVE chunk.
    /// </summary>
    /// <param name="path">
    ///     The path and extension of the file to create.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to embed in the SAVE chunk of the .png
    ///     created by this method.
    /// </param>
    /// <param name="width">
    ///     The width of the image data, in pixels.
    /// </param>
    /// <param name="height">
    ///     The height of the image data, in pixels.
    /// </param>
    /// <param name="pixels">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the <see cref="Microsoft.Xna.Framework.Color"/> data that 
    ///     represents the pixels of the image data to write to the .png file read from left-to-right top-to-bottom.
    /// </param>
    public static void ToPng(string path, ReadOnlySpan<byte> data, int width, int height, ReadOnlySpan<Color> pixels)
    {
        using FileStream stream = File.Create(path);
        ToPng(stream, data, width, height, pixels);
    }
    /// <summary>
    ///     Creates a new .png file using the given <paramref name="texture"/> and embeds the give
    ///     <paramref name="data"/> into the .png as a SAVE chunk.
    /// </summary>
    /// <param name="stream">
    ///     The <see cref="System.IO.FileStream"/> to write the .png too.
    /// </param>
    /// <param name="data">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the bytes of the data to embed in the SAVE chunk of the .png.
    /// </param>
    /// <param name="width">
    ///     The width of the image data, in pixels.
    /// </param>
    /// <param name="height">
    ///     The height of the image data, in pixels.
    /// </param>
    /// <param name="pixels">
    ///     A <see cref="ReadOnlySpan{T}"/> containing the <see cref="Microsoft.Xna.Framework.Color"/> data that 
    ///     represents the pixels of the image data to write to the .png file read from left-to-right top-to-bottom.
    /// </param>
    public static void ToPng(FileStream stream, ReadOnlySpan<byte> data, int width, int height, ReadOnlySpan<Color> pixels)
    {
        PngWriter.WriteSignature(stream);
        PngWriter.WriteIHDR(stream, width, height);
        PngWriter.WriteIDAT(stream, width, height, pixels);
        PngWriter.WriteCustomChunk(stream, "SAVE", data);
        PngWriter.WriteIEND(stream);
    }
}