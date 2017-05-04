Imports System.IO
Imports E57LibCommon.Binary

Module Binary

    Public Function WriteWithChecksum(ByRef Stream As FileStream, ByRef Bytes() As Byte)
        Dim Count As Int64 = 0
        Dim LogicalLength As UInt64 = 0
        While Count < Bytes.Length
            Dim CurrentPosition As Int64 = Stream.Position
            Dim NextChecksum As Long = E57LibCommon.Binary.NextChecksum(CurrentPosition)
            Dim BytesToWrite As Int64 = Math.Min(NextChecksum - CurrentPosition, Bytes.Length - Count)

            Stream.Write(Bytes, Count, BytesToWrite)
            LogicalLength += BytesToWrite
            Count += BytesToWrite

            If NextChecksum = CurrentPosition + BytesToWrite And Count < Bytes.Length Then
                Stream.Write(ZeroArray(4), 0, 4)
            End If
        End While

        Return LogicalLength
    End Function

End Module
