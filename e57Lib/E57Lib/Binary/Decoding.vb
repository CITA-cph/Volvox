Imports System.IO
Imports E57LibCommon

Public Module Binary

    Public Function ReadAndSkip(ByRef Stream As FileStream, Position As Long, Count As UInt64, Optional SenderName As String = "") As Byte()
        Dim sign As Long = Convert.ToInt64(Count)
        Dim b(sign - 1) As Byte
        Stream.Position = Position

        Dim counter As Long = 0

        While counter < sign
            Dim var1 As Long = NextChecksum(Stream.Position) - Stream.Position
            Dim var2 As Long = sign - counter

            Dim Upcoming As Long = Math.Min(var1, var2)

            'debugbin.Add(SenderName, DateTime.Now, Stream.Position, Upcoming, "Read start")
            Stream.Read(b, counter, Upcoming)
            'debugbin.Add(SenderName, DateTime.Now, Stream.Position, 0, "Read end")

            If var1 < var2 Then
                'skip checksum (checksum check performed on the whole file at once is faster than checking it here)
                Dim buff(3) As Byte
                'debugbin.Add(SenderName, DateTime.Now, Stream.Position, 4, "Checksum skip")
                Stream.Read(buff, 0, 4)
                'debugbin.Add(SenderName, DateTime.Now, Stream.Position, 0, "Checksum end")
            End If

            counter += Upcoming
        End While

        Return b
    End Function

    Function ReadXmlPart(FilePath As String, PageSize As UInt64, PageCount As UInt64, xmlOffset As UInt64, xmlLength As UInt64) As String

        Dim xmls As String = String.Empty

        Using ThisStream As FileStream = New FileStream(FilePath, FileMode.Open, FileAccess.Read)
            Dim xmlall() As Byte = Binary.ReadAndSkip(ThisStream, xmlOffset, xmlLength, "Xml part read")
            xmls = System.Text.Encoding.UTF8.GetString(xmlall, 0, xmlLength)
        End Using

        Return xmls
    End Function

End Module

