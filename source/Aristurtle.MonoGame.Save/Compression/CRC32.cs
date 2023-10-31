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

///////////////////////////////////////////////////////////////////////////////
/// This is a C# port from the code provided at 
/// https://www.w3.org/TR/2003/REC-PNG-20031110/#D-CRCAppendix
///////////////////////////////////////////////////////////////////////////////

namespace Aristurtle.MonoGame.Save.Compression;

internal class CRC32
{
    private const uint DEFAULT = 0xFFFFFFFF;
    private static readonly uint[] _crcTable;

    private uint _value;
    internal uint CurrentValue => _value ^ 0xFFFFFFFF;

    internal CRC32() => _value = DEFAULT;
    internal CRC32(uint initial) => _value = initial;
    internal CRC32(ReadOnlySpan<byte> initial) : this() => _ = Update(initial);

    static CRC32()
    {

        //  Make the table for fast crc
        _crcTable = new uint[256];

        uint c;
        for (uint n = 0; n < 256; n++)
        {
            c = n;
            for (int k = 0; k < 8; k++)
            {
                if ((c & 1) != 0)
                {
                    c = 0xEDB88320 ^ (c >> 1);
                }
                else
                {
                    c >>= 1;
                }
            }

            _crcTable[n] = c;
        }
    }

    internal void Reset() => _value = DEFAULT;

    internal uint Update(ReadOnlySpan<byte> buffer)
    {
        for (int n = 0; n < buffer.Length; n++)
        {
            _value = _crcTable[(_value ^ buffer[n]) & 0xFF] ^ (_value >> 8);
        }

        return _value ^ 0xFFFFFFFF;
    }

    internal static uint Calculate(ReadOnlySpan<byte> buffer) => new CRC32().Update(buffer);
}