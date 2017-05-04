Public Module Binary
    Public Function BitsPerInteger(Minimum As Int64, Maximum As Int64) As Int64
        Return Math.Ceiling(Math.Log((Maximum - Minimum + 1), 2))
    End Function

    Public Function ScaledIntegerToDouble(Raw As Int64, Scale As Double, Offset As Double) As Double
        Return Raw * Scale + Offset
    End Function

    Public Function DoubleToScaledInteger(Value As Double, Scale As Double, Offset As Double) As Int64
        Return (Value - Offset) / Scale
    End Function

    Public Function DoubleToScaledIntegerFast(Value As Double, ScaleDenom As Double, Offset As Double) As Int64
        Return (Value - Offset) * ScaleDenom
    End Function

    Public Sub WriteArrayValues(ByRef Arr() As Byte, Start As Integer, Count As Integer, Value As Byte)
        For i As Integer = Start To Count - 1 Step 1
            Arr(i) = Value
        Next
    End Sub

    Public Function ZeroArray(Count As Integer) As Byte()
        Dim arr(Count - 1) As Byte
        Return arr
    End Function

    Public Function NextChecksum(CurrentPosition As UInt64) As UInt64
        Return CurrentPosition + (1024 - (CurrentPosition Mod 1024)) - 4
    End Function

    ''' <summary>
    ''' In this case it should always be 1020 bytes 
    ''' </summary>
    ''' <param name="Bytes"></param>
    ''' <returns></returns>
    Public Function GenerateChecksum(Bytes() As Byte) As UInt32
            Return Crc32C.Crc32CAlgorithm.Compute(Bytes)
        End Function

        ''' <summary>
        ''' Get checksum UInt32 from 4 bytes. Reverses the byte order to match the file order.
        ''' </summary>
        ''' <param name="Bytes"></param>
        ''' <returns></returns>
        Public Function ChecksumFromBytes(Bytes() As Byte) As UInt32
            Return BitConverter.ToUInt32(Bytes.Reverse.ToArray, 0)
        End Function

        ''' <summary>
        ''' Gets bytes from UInt32 and reverses them to match the file order.
        ''' </summary>
        ''' <param name="Sum"></param>
        ''' <returns></returns>
        Public Function ChecksumToBytes(Sum As UInt32) As Byte()
            Return BitConverter.GetBytes(Sum).Reverse.ToArray
        End Function

    End Module
