Imports System.IO

Namespace Binary

    Public Module EasyBinary

        Public Function ReadAndSkip(ByRef Stream As FileStream, Position As Long, Count As UInt64) As Byte()
            Dim sign As Long = Convert.ToInt64(Count)
            Dim b(sign - 1) As Byte
            Stream.Position = Position

            Dim counter As Long = 0

            While counter < sign

                Dim var1 As Long = NextChecksum(Stream.Position) - Stream.Position
                Dim var2 As Long = sign - counter

                Dim Upcoming As Long = Math.Min(var1, var2)

                Stream.Read(b, counter, Upcoming)

                If var1 < var2 Then

                    'skip checksum 
                    Dim buff(3) As Byte
                    Stream.Read(buff, 0, 4)

                End If

                counter += Upcoming

            End While

            Return b
        End Function

        Public Function NextChecksum(CurrentPosition As Int64) As Int64
            Return CurrentPosition + (1024 - (CurrentPosition Mod 1024)) - 4
        End Function

        Private Function GetByteStream(values As List(Of Single)) As Byte()
            Return values.SelectMany(Function(value) BitConverter.GetBytes(value)).ToArray()
        End Function

        Private Function GetByteStream(values As List(Of Double)) As Byte()
            Return values.SelectMany(Function(value) BitConverter.GetBytes(value)).ToArray()
        End Function

        Private Function GetByteStream(values As List(Of UInt64)) As Byte()
            Return values.SelectMany(Function(value) BitConverter.GetBytes(value)).ToArray()
        End Function

        Public Function ReadSingleBytestream(ByRef Stream As FileStream, Position As Int64, Count As UInt16) As List(Of Double)
            Dim b() As Byte = ReadAndSkip(Stream, Position, Count)

            Dim l As New List(Of Double)

            For i As Integer = 0 To b.Length - 1 Step 4
                l.Add(CDbl(BitConverter.ToSingle(b, i)))
            Next

            b = Nothing

            Return l
        End Function

        Public Function ReadDoubleBytestream(ByRef Stream As FileStream, Position As Int64, Count As UInt16) As List(Of Double)
            Dim b() As Byte = ReadAndSkip(Stream, Position, Count)

            Dim l As New List(Of Double)

            For i As Integer = 0 To b.Length - 1 Step 8
                l.Add(CDbl(BitConverter.ToDouble(b, i)))
            Next

            b = Nothing
            Return l
        End Function

        Public Function ReadIntegerBytestream(ByRef Stream As FileStream, Position As Int64, Count As UInt16, Minimum As Int64, Maximum As Int64) As List(Of Int64)
            Dim b() As Byte = ReadAndSkip(Stream, Position, Count)

            Dim l As New List(Of Int64)
            Dim bpi As Integer = BitsPerInteger(Minimum, Maximum)
            Dim ba As New BitArray(b)
            b = Nothing

            For i As Integer = 0 To ba.Length - bpi Step bpi
                Dim thisInt As Int64 = 0

                For j As Integer = 0 To 63 Step 1
                    If j < bpi Then
                        If ba(j + i) Then thisInt += 2 ^ j
                    Else
                        Exit For
                    End If
                Next

                l.Add(thisInt + Minimum)
            Next

            ba = Nothing
            Return l
        End Function

        Public Function ReadScaledIntegerBytestream(ByRef Stream As FileStream, Position As Int64, Count As UInt16, Minimum As Int64, Maximum As Int64, Scale As Double, Offset As Double) As List(Of Double)
            Dim b2() As Byte = ReadAndSkip(Stream, Position, Count)

            Dim l As New List(Of Double)
            Dim bpi As Integer = BitsPerInteger(Minimum, Maximum)

            Dim ff As Double = Math.Round(((Math.Ceiling((Count * 8) / bpi)) * bpi) / 8)

            Dim b(ff - 1) As Byte

            b2.CopyTo(b, 0)

            Dim ba As New BitArray(b)
            b = Nothing

            For i As Integer = 0 To ba.Length - bpi Step bpi
                Dim thisInt As Int64 = 0

                For j As Integer = 0 To 63 Step 1
                    If j < bpi Then
                        If ba(j + i) Then thisInt += 2 ^ j
                    Else
                        Exit For
                    End If
                Next

                l.Add(ScaledIntegerToDouble(thisInt, Scale, Offset))
            Next

            'For i As Integer = 0 To ba.Length - 1 Step bpi
            '    Dim thisInt As Int64 = 0

            '    'Dim b64(63) As Boolean

            '    For j As Integer = 0 To bpi - 1 Step 1
            '        If j + i < ba.Count - 1 Then
            '            If ba(j + i) Then
            '                thisInt += (2 ^ j)
            '            End If
            '        End If
            '    Next

            '    'b64 = b64.Reverse.ToArray

            '    'Dim thisints(0) As Int64
            '    'b64.CopyTo(thisints, 0)

            '    'b64 = Nothing

            'Next

            ba = Nothing

            Return l
        End Function

        Function BitsPerInteger(Minimum As Int64, Maximum As Int64) As Int64
            Return Math.Ceiling(Math.Log((Maximum - Minimum + 1), 2))
        End Function

        'I'm not so sure if it's the proper way...
        Function ScaledIntegerToDouble(Raw As Int64, Scale As Double, Offset As Double) As Double
            Return Raw * Scale + Offset
        End Function

        Sub WriteArrayValues(ByRef Arr() As Byte, Start As Integer, Count As Integer, Value As Byte)
            For i As Integer = Start To Count - 1 Step 1
                Arr(i) = Value
            Next
        End Sub

    End Module

    Public Module DecodingModules

        ''' <summary>
        ''' Table 35
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="sectionId"></param>
        ''' <param name="reserved"></param>
        ''' <param name="sectionLength"></param>
        ''' <param name="dataStartOffset"></param>
        ''' <param name="indexStartOffset"></param>
        Public Sub ReadCompressedVectorHeader(BytesToRead() As Byte,
    ByRef sectionId As Byte,
    ByRef reserved() As Byte,
    ByRef sectionLength As UInt64,
    ByRef dataStartOffset As UInt64,
    ByRef indexStartOffset As UInt64)

            sectionId = BytesToRead(0)

            If reserved IsNot Nothing Then
                ReDim reserved(6)
                Array.Copy(BytesToRead, 1, reserved, 0, 7)
            End If

            sectionLength = BitConverter.ToUInt64(BytesToRead, 8)
            dataStartOffset = BitConverter.ToUInt64(BytesToRead, 16)
            indexStartOffset = BitConverter.ToUInt64(BytesToRead, 24)

        End Sub

        Public Function WriteCompressedVectorHeader(sectionLength As UInt64, dataStartOffset As UInt64, indexStartOffset As UInt64) As Byte()
            Dim b(31) As Byte
            b(0) = 1

            WriteArrayValues(b, 1, 7, 0)
            BitConverter.GetBytes(sectionLength).CopyTo(b, 8)
            BitConverter.GetBytes(dataStartOffset).CopyTo(b, 16)
            BitConverter.GetBytes(indexStartOffset).CopyTo(b, 24)

            Return b
        End Function

        ''' <summary>
        ''' Table 36
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="packetType"></param>
        ''' <param name="reserved"></param>
        ''' <param name="packetLengthMinus1"></param>
        ''' <param name="entryCount"></param>
        ''' <param name="indexLevel"></param>
        ''' <param name="reserved2"></param>
        Public Sub ReadIndexPacketHeader(BytesToRead() As Byte,
    ByRef packetType As Byte,
    ByRef reserved As Byte,
    ByRef packetLengthMinus1 As UShort,
    ByRef entryCount As UShort,
    ByRef indexLevel As Byte,
    ByRef reserved2() As Byte)

            packetType = BytesToRead(0)
            reserved = BytesToRead(1)
            packetLengthMinus1 = BitConverter.ToUInt16(BytesToRead, 2)
            entryCount = BitConverter.ToUInt16(BytesToRead, 4)
            indexLevel = BytesToRead(6)

            If reserved2 IsNot Nothing Then
                ReDim reserved2(7)
                Array.Copy(BytesToRead, 7, reserved2, 0, 8)
            End If

        End Sub

        Public Function WriteIndexPacketHeader(packetLengthMinus1 As UShort, entryCount As UShort, indexLevel As Byte) As Byte()
            Dim b(15) As Byte

            WriteArrayValues(b, 0, 2, 0)
            BitConverter.GetBytes(packetLengthMinus1).CopyTo(b, 2)
            BitConverter.GetBytes(entryCount).CopyTo(b, 4)
            b(6) = indexLevel
            WriteArrayValues(b, 7, 8, 0)

            Return b
        End Function

        ''' <summary>
        ''' Table 37
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="chunkRecordIndex"></param>
        ''' <param name="packetOffset"></param>
        Public Sub ReadPacketAddressEntries(BytesToRead() As Byte,
    ByRef chunkRecordIndex As UInt64,
    ByRef packetOffset As UInt64)

            chunkRecordIndex = BitConverter.ToUInt64(BytesToRead, 0)
            packetOffset = BitConverter.ToUInt64(BytesToRead, 8)

        End Sub

        Public Function WritePacketAddressEntries(chunkRecordIndex As UInt64, packetOffset As UInt64) As Byte()
            Dim b(15) As Byte
            BitConverter.GetBytes(chunkRecordIndex).CopyTo(b, 0)
            BitConverter.GetBytes(packetOffset).CopyTo(b, 8)
            Return b
        End Function

        ''' <summary>
        ''' Table 38
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="packetType"></param>
        ''' <param name="packetFlags"></param>
        ''' <param name="packetLengthMinus1"></param>
        ''' <param name="bytestreamCount"></param>        
        Public Sub ReadDataPacketHeader(BytesToRead() As Byte,
        ByRef packetType As Byte,
        ByRef packetFlags As Byte,
        ByRef packetLengthMinus1 As UInt16,
        ByRef bytestreamCount As UInt16)

            packetType = BytesToRead(0)
            packetFlags = BytesToRead(1)
            packetLengthMinus1 = BitConverter.ToUInt16(BytesToRead, 2)
            bytestreamCount = BitConverter.ToUInt16(BytesToRead, 4)

        End Sub

        Public Function WriteDataPacketHeader(packetFlags As Byte, packetLengthMinus1 As UInt16, bytestreamCount As UInt16) As Byte()
            Dim b(5) As Byte
            b(0) = 0
            b(1) = packetFlags

            BitConverter.GetBytes(packetLengthMinus1).CopyTo(b, 2)
            BitConverter.GetBytes(bytestreamCount).CopyTo(b, 4)

            Return b
        End Function

        ''' <summary>
        ''' Table 39
        ''' </summary>
        ''' <param name="ByteToRead"></param>
        ''' <param name="compressorRestart"></param>
        ''' <param name="reserved">Not supported, set to nothing.</param>
        Public Sub ReadPacketFlags(ByteToRead As Byte,
    ByRef compressorRestart As Boolean,
    ByRef reserved As Byte())

            Dim ToBits As New BitArray(ByteToRead)
            compressorRestart = ToBits.Get(0)
        End Sub


        ''' <summary>
        ''' TODO there might be a problem with bit order here 
        ''' </summary>
        ''' <param name="compressorRestart"></param>
        ''' <returns></returns>
        Public Function WritePacketFlags(compressorRestart As Boolean) As Byte
            Dim ba As New BitArray(8, 0)
            ba(0) = compressorRestart

            Dim b(0) As Byte
            ba.CopyTo(b, 0)
            Return b(0)
        End Function

        ''' <summary>
        ''' Table 40
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="packetType"></param>
        ''' <param name="reserved"></param>
        ''' <param name="packetLengthMinus1"></param>
        Public Sub ReadIgnoredPacketHeader(BytesToRead() As Byte,
    ByRef packetType As Byte,
    ByRef reserved As Byte,
    ByRef packetLengthMinus1 As UInt16)

            packetType = BytesToRead(0)
            reserved = BytesToRead(1)
            packetLengthMinus1 = BitConverter.ToUInt16(BytesToRead, 2)
        End Sub

        Public Function WriteIgnoredPacketHeader(packetLengthMinus1 As UShort) As Byte()
            Dim b(3) As Byte
            BitConverter.GetBytes(packetLengthMinus1).CopyTo(b, 2)
            Return b
        End Function

        ''' <summary>
        ''' Table 41 and 42
        ''' </summary>
        ''' <param name="BytesToRead"></param>
        ''' <param name="stringType"></param>
        ''' <param name="length"></param>
        ''' <param name="stringData"></param>
        Public Sub ReadString(BytesToRead() As Byte,
    ByRef stringType As Boolean,
    ByRef length As UInt64,
    ByRef stringData As String)

            If stringType <> Nothing Then
                Dim ToBits As New BitArray(BytesToRead(0))
                stringType = ToBits.Get(0)
            End If

            Dim countBytes As Integer = 0
            Dim firstByte As Integer = 0

            If stringType Then
                countBytes = BytesToRead.Length - 8
                firstByte = 8
            Else
                countBytes = BytesToRead.Length - 1
                firstByte = 1
            End If

            stringData = System.Text.Encoding.UTF8.GetString(BytesToRead, firstByte, countBytes)
            length = stringData.Length
        End Sub

        Public Function WriteString(Text As String) As Byte()
            Dim numOfBytes As UInt64 = System.Text.Encoding.UTF8.GetByteCount(Text)
            Dim s() As Byte = System.Text.Encoding.UTF8.GetBytes(Text)
            Dim t As Boolean = False

            Dim ba As BitArray = Nothing

            If numOfBytes <= 127 Then
                Dim as8 As Byte = numOfBytes
                Dim asa(0) As Byte
                asa(0) = as8
                ba = New BitArray(asa)
                Dim bb As New BitArray(asa)
                ba(0) = 0

                For i As Integer = 1 To ba.Count - 1 Step 1
                    ba(i) = bb(i - 1)
                Next
            Else
                Dim asa() As Byte = BitConverter.GetBytes(numOfBytes)
                ba = New BitArray(asa)
                Dim bb As New BitArray(asa)
                ba(0) = 1

                For i As Integer = 1 To ba.Count - 1 Step 1
                    ba(i) = bb(i - 1)
                Next
            End If

            Dim babytes As UInt64 = ba.Count / 8
            Dim arrlen As UInt64 = babytes + s.Length

            Dim arr(arrlen - 1) As Byte
            ba.CopyTo(arr, 0)
            s.CopyTo(arr, ba.Length)

            Return arr
        End Function

    End Module

End Namespace