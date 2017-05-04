Imports System.IO
Imports E57LibCommon

Public Class e57Document

    ''' <summary>
    ''' Shall be "ASTM E57 3D Imaging Data File”.", according to the standard. Consider twice any modifications.
    ''' </summary>
    ''' <returns></returns>
    Public Property FormatName As String = "ASTM E57 3D Imaging Data File"

    ''' <summary>
    ''' This value is required, use e57Document.GenerateGuid or provide your own Guid generated in any other way.
    ''' </summary>
    ''' <returns></returns>
    Public Property Guid As String = String.Empty

    ''' <summary>
    ''' Shall be 1, according to the standard. Consider twice any modifications.
    ''' </summary>
    ''' <returns></returns>
    Public Property VersionMajor As Integer = 1

    ''' <summary>
    ''' Shall be 0, according to the standard. Consider twice any modifications.
    ''' </summary>
    ''' <returns></returns>
    Public Property VersionMinor As Integer = 0

    Public Property LibraryVersion As String = String.Empty
    Public Property CoordinateMetadata As String = String.Empty

    Public Property [Date] As Date = Nothing
    Public Property IsAtomicClockReferenced As Boolean = False

    Private MyData3D As New List(Of Scan)

    Public Sub New()

    End Sub

    Public Property Scans As List(Of Scan)
        Get
            Return MyData3D
        End Get
        Set(value As List(Of Scan))
            MyData3D = value
        End Set
    End Property

    Public Function Save(Filename As String) As Boolean

        Using w As FileStream = New FileStream(Filename, FileMode.Create, FileAccess.ReadWrite)

            'for now we write a zeroed array as a file header placeholder
            w.Write(WriteFileHeader(0, 0, 0), 0, 48)

            For Each s As Scan In MyData3D
                s.Encode(w)
                s.Data.Clear()
            Next

            Dim bts() As Byte = (System.Text.Encoding.UTF8.GetBytes(GetXml(Me)))
            Dim xmloff As UInt64 = w.Position
            WriteWithChecksum(w, bts)

            w.Write(ZeroArray((Math.Ceiling(w.Length / 1024) * 1024) - w.Length), 0, (Math.Ceiling(w.Length / 1024) * 1024) - w.Length)

            w.Position = 0
            w.Write(WriteFileHeader(w.Length, xmloff, bts.Length), 0, 48)

            'Compute checksums          
            For i As Int64 = 0 To w.Length - 1 Step 1024
                w.Position = i
                Dim buff(1019) As Byte
                w.Read(buff, 0, 1020)
                w.Write(ChecksumToBytes(GenerateChecksum(buff)), 0, 4)
            Next

        End Using

        Return True
    End Function

    Public Sub GenerateGuid()
        Me.Guid = System.Guid.NewGuid.ToString
    End Sub


    'PSEUDOCODE for using the library

    'dim pcl as new list(of pointcloud)                     (assuming there are some point clouds here)

    'dim doc as new e57document 
    'doc.whatever1 = "goo1"                                 (setting doc properties)
    'doc.whatever2 = "goo2"                                 (setting doc properties)
    'doc.whatever3 = "goo3"                                 (setting doc properties)

    '   for each pc as pointcloud in pcl 
    '      dim nscan as new scan
    '      nscan.whatever1 = "goo1"                         (setting scan properties)
    '      nscan.whatever2 = "goo2"                         (setting scan properties)
    '      nscan.whatever3 = "goo3"                         (setting scan properties)
    '      putNumbersIndata(nscan.data, pc, "cartesianX")   (this function determins where to put the data)
    '      putNumbersIndata(nscan.data, pc, "cartesianY")   (strictly Rhino here)
    '      putNumbersIndata(nscan.data, pc, "cartesianZ")   (strictly Rhino here)
    '      nscan.data.encode
    '      doc.scans.add(nscan)
    '   next

    'doc.save(filename)

End Class
