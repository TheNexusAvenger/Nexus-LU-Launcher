using System.IO;
using System.Text;

namespace Nexus.LU.Launcher.State.Util;

public static class FileStreamExtensions
{
    /// <summary>
    /// Asserts the next byte in the stream matches the expected byte.
    /// </summary>
    /// <param name="this">File stream to read.</param>
    /// <param name="byteToAssert">Expected byte value to assert.</param>
    /// <exception cref="InvalidDataException">The expected and actual byte don't match.</exception>
    public static void AssertNextByte(this FileStream @this, byte byteToAssert)
    {
        var actualByte = @this.ReadByte();
        if (actualByte == byteToAssert) return;
        throw new InvalidDataException($"Expected byte in file did not match. (Expected {byteToAssert}, got {actualByte} actualByte)");
    }

    /// <summary>
    /// Reads a null-terminated string.
    /// </summary>
    /// <param name="this">File stream to read.</param>
    /// <returns>Null-terminated string (excluding the null terminator).</returns>
    public static string ReadNullTerminatedString(this FileStream @this)
    {
        var stringBuilder = new StringBuilder();
        while (true)
        {
            var character = @this.ReadByte();
            if (character == 0) break;
            stringBuilder.Append((char) character);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Writes a null-terminated string.
    /// </summary>
    /// <param name="this">File stream to write.</param>
    /// <param name="stringToWrite">String to write.</param>
    public static void WriteNullTerminatedString(this FileStream @this, string stringToWrite)
    {
        foreach (var character in stringToWrite.ToCharArray())
        {
            @this.WriteByte((byte) character);
        }
        @this.WriteByte(0);
    }
}