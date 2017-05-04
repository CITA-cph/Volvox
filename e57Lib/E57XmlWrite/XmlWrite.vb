Imports System.IO
Imports System.Text
Imports System.Xml
Imports E57LibWriter


Public Module XmlWrite

    Public Function getXml(ByRef e57Doc As e57Document) As String
        'Create MemoryStream
        Dim Mstream As New MemoryStream()
        Using Mstream

            'Create XMLWriterSettings.
            Dim settings As XmlWriterSettings = New XmlWriterSettings()
            settings.Indent = True
            settings.CloseOutput = True

            ' Create XmlWriter.
            'Using writer As XmlWriter = XmlWriter.Create(StringWriter, settings)
            Using writer As XmlWriter = XmlWriter.Create(Mstream, settings)
                ' Write comment
                'writer.WriteComment("E57 XML Test")

                ' Start Root
                e57XmlStartRoot(writer)
                '   Write Document Metadata
                e57XmlWriteDocMetaDataElement(writer, e57Doc)

                '   Start Data3D
                e57XmlStartData3D(writer)
                '       write VectorChild for each scan
                For Each scan As Scan In e57Doc.Scans
                    '       Start VectorChild with MetaData
                    e57XmlStartVectorChild(writer, scan)
                    '       write CartesianBounds
                    e57XmlWriteCartesianBoundsElement(writer, scan)
                    '       write Scan Pose
                    e57XmlWritePoseElement(writer, scan)
                    '       write Points Element
                    e57XmlWritePointsElement(writer, scan)
                    '       End vectorChild
                    e57XmlEndParentElement(writer)
                Next

                '   End Data3D
                e57XmlEndParentElement(writer)
                ' End E57Root
                e57XmlEndParentElement(writer)

                ' Write the XML to file and close the writer.
                writer.Flush()
                writer.Close()
            End Using
        End Using

        Dim xmlString As String = UTF8Encoding.UTF8.GetString(Mstream.ToArray())
        Return xmlString
    End Function


End Module



