using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// A stream that combines multiple byte arrays into one single stream
/// </summary>
public class MultiByteReadonlyStream : Stream
{
    int dataArrayIndex = 0;
    long byteSubIndex = 0;

    long position = 0;
    int length = 0;

    List<byte[]> dataArrays;

    public MultiByteReadonlyStream(List<byte[]> dataArrays)
    {
        this.dataArrays = dataArrays;
        for (int i = 0; i < dataArrays.Count; i++)
        {
            length += dataArrays[i].Length;
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => length;

    public override long Position
    {
        get => position;
        set
        {
            position = value;
            byteSubIndex = position;
            dataArrayIndex = 0;

            for (int i = 0; i < dataArrays.Count; i++)
            {
                if (byteSubIndex >= dataArrays[i].Length)
                {
                    byteSubIndex -= dataArrays[i].Length;
                    dataArrayIndex++;
                }
                else
                {
                    break;
                }
            }
        }
    }



    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (Position >= Length)
        {
            return 0;
        }

        int countRead = 0;
        int countLeft = count;

        while (countRead < count && position < Length)
        {
            var currentDataToRead = dataArrays[dataArrayIndex].Length - byteSubIndex;

            if (currentDataToRead > countLeft)
            {
                currentDataToRead = countLeft;
            }

            Buffer.BlockCopy(dataArrays[dataArrayIndex], (int)byteSubIndex, buffer, offset + countRead, (int)currentDataToRead);
            countRead += (int)currentDataToRead;
            countLeft -= (int)currentDataToRead;
            position += currentDataToRead;

            if (currentDataToRead == dataArrays[dataArrayIndex].Length - byteSubIndex)
            {
                dataArrayIndex++;
                byteSubIndex = 0;
            }
            else
            {
                byteSubIndex += currentDataToRead;
            }
        }

        return countRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                return Position;
            case SeekOrigin.Current:
                Position += offset;
                return Position;
            case SeekOrigin.End:
                Position = Length - offset - 1;
                return Position;
            default:
                return Position;
        }
    }

    public override void SetLength(long value)
    {
        Position = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}