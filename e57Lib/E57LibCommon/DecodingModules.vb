Imports System.IO

Public Module DecodingModules

    ''' <summary>
    ''' Gets first 48 bytes of a file
    ''' </summary>
    ''' <param name="FilePath"></param>
    ''' <returns></returns>
    Friend Function GetFileHeaderBytes(FilePath As String) As Byte()
        Dim HeaderBytes(47) As Byte
        Using ThisStream As FileStream = New FileStream(FilePath, FileMode.Open, FileAccess.Read)
            ThisStream.Read(HeaderBytes, 0, 48)
        End Using
        Return HeaderBytes
    End Function

    ''' <summary>
    ''' Reads file header
    ''' </summary>
    ''' <param name="FilePath"></param>
    ''' <param name="FileSignature"></param>
    ''' <param name="VersionMajor"></param>
    ''' <param name="VersionMinor"></param>
    ''' <param name="FileLength"></param>
    ''' <param name="xmlOffset"></param>
    ''' <param name="xmlLength"></param>
    ''' <param name="PageSize"></param>
    ''' <returns></returns>
    Public Function ReadFileHeader(FilePath As String,
                          ByRef FileSignature As String,
                          ByRef VersionMajor As UInt32,
                          ByRef VersionMinor As UInt32,
                          ByRef FileLength As UInt64,
                          ByRef xmlOffset As UInt64,
                          ByRef xmlLength As UInt64,
                          ByRef PageSize As UInt64) As String

        Dim HeaderBytes() As Byte = GetFileHeaderBytes(FilePath)

        FileSignature = System.Text.Encoding.ASCII.GetString(HeaderBytes, 0, 8)
        VersionMajor = BitConverter.ToUInt32(HeaderBytes, 8)
        VersionMinor = BitConverter.ToUInt32(HeaderBytes, 12)
        FileLength = BitConverter.ToUInt64(HeaderBytes, 16)
        xmlOffset = BitConverter.ToUInt64(HeaderBytes, 24)
        xmlLength = BitConverter.ToUInt64(HeaderBytes, 32)
        PageSize = BitConverter.ToUInt64(HeaderBytes, 40)

        'debugbin.Add("Header bytes read", DateTime.Now, 0, 48, "File length is " & FileLength)

        Dim Outstring As String = String.Empty

        Outstring &= "fileSignature: " & FileSignature & vbCrLf
        Outstring &= "versionMajor: " & VersionMajor & vbCrLf
        Outstring &= "versionMinor: " & VersionMinor & vbCrLf
        Outstring &= "fileLength: " & FileLength & vbCrLf
        Outstring &= "xmlOffset: " & xmlOffset & vbCrLf
        Outstring &= "xmlLength: " & xmlLength & vbCrLf
        Outstring &= "pageSize: " & PageSize & vbCrLf

        Return Outstring

    End Function

    Public Function WriteFileHeader(FileLength As UInt64,
                           xmlOffset As UInt64,
                           xmlLength As UInt64) As Byte()

        Dim HeaderBytes(47) As Byte

        Dim VersionMajor As UInt32 = 1
        Dim VersionMinor As UInt32 = 0

        Dim PageSize As UInt64 = 1024

        System.Text.Encoding.ASCII.GetBytes("ASTM-E57").CopyTo(HeaderBytes, 0)
        BitConverter.GetBytes(VersionMajor).CopyTo(HeaderBytes, 8)
        BitConverter.GetBytes(VersionMinor).CopyTo(HeaderBytes, 12)
        BitConverter.GetBytes(FileLength).CopyTo(HeaderBytes, 16)
        BitConverter.GetBytes(xmlOffset).CopyTo(HeaderBytes, 24)
        BitConverter.GetBytes(xmlLength).CopyTo(HeaderBytes, 32)
        BitConverter.GetBytes(PageSize).CopyTo(HeaderBytes, 40)

        Return HeaderBytes

    End Function

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
    Public Sub ReadPacketAddressEntry(BytesToRead() As Byte,
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
        b(0) = 1
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
