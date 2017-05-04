Imports System.IO
Imports System.Text
Imports System.Xml

Public Module XmlWrite

    Public Function GetXml(ByRef e57Doc As e57Document) As String
        'Create StringWriter
        'Dim stringBuilder As New StringBuilder()
        'Using stringWriter As New StringWriter(stringBuilder)
        Dim Mstream As New MemoryStream()

        'Create XMLWriterSettings.
        Dim settings As XmlWriterSettings = New XmlWriterSettings()
        settings.Indent = True

        ' Create XmlWriter.
        Using w As XmlWriter = XmlWriter.Create(Mstream, settings)
            ' Start Root
            XStartRoot(w)
            '   Write Document Metadata
            XWriteDocMetaDataElement(w, e57Doc)

            '   Start Data3D
            XStartData3D(w)

            For Each scan As Scan In e57Doc.Scans
                XStartVectorChild(w, scan)
                If scan.OriginalGuids.Count > 0 Then XOriginalGuids(w, scan)
                If scan.HasCartesian Then XCartesianBounds(w, scan)
                If scan.HasSpherical Then XSphericalBounds(w, scan)
                If scan.HasIntensity Then XIntensityLimits(w, scan)
                If scan.HasColor Then XColorLimits(w, scan)
                If scan.HasPose Then XPose(w, scan)
                XPoints(w, scan)
                XEndParent(w)
            Next

            '   End Data3D
            XEndParent(w)

            w.WriteStartElement("images2D")
            w.WriteAttributeString("type", "Vector")
            w.WriteAttributeString("allowHeterogeneousChildren", "1")
            w.WriteEndElement()

            ' End E57Root
            XEndParent(w)
        End Using

        Dim xmlString As String = UTF8Encoding.UTF8.GetString(Mstream.ToArray())
        Mstream.Dispose()

        Dim _byteOrderMarkUtf8 As String = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetPreamble())
        If xmlString.StartsWith(_byteOrderMarkUtf8) Then
            xmlString = xmlString.Remove(0, _byteOrderMarkUtf8.Length)
        End If

        Return xmlString
    End Function


End Module



