Imports System.Globalization
Imports System.Xml
Imports E57LibWriter

Public Module E57Xml

    Public Function e57XmlStartRoot(ByRef writer As XmlWriter) As Boolean
        ' Set e57Root (Can be ended with an xmlEndParentElement)
        writer.WriteStartElement("e57Root", "http://www.astm.org/COMMIT/E57/2010-e57-v1.0")
        writer.WriteAttributeString("type", "Structure")
        Return True
    End Function

    Public Function e57XmlStartData3D(ByRef writer As XmlWriter) As Boolean
        ' Set Data3D Header (Can be ended with an xmlEndParentElement)
        writer.WriteStartElement("data3D")
        writer.WriteAttributeString("type", "Vector")
        writer.WriteAttributeString("allowHeterogeneousChildren", "1")
        Return True
    End Function


    Public Function e57XmlWriteDocMetaDataElement(ByRef writer As XmlWriter, e57Doc As e57Document) As Boolean
        ' Format Name
        e57XmlWriteCStringElement(writer, "formatName", e57Doc.FormatName)
        ' Guid
        e57XmlWriteCStringElement(writer, "guid", e57Doc.Guid)
        ' versionMajor
        e57XmlWriteStringElement(writer, "versionMajor", "Integer", e57Doc.VersionMajor) '.ToString)
        ' versionMinor
        e57XmlWriteStringElement(writer, "versionMinor", "Integer", e57Doc.VersionMinor) '.ToString)
        ' e57LibraryVersion
        e57XmlWriteCStringElement(writer, "e57LibraryVersion", e57Doc.LibraryVersion)
        ' coordinateMetadata
        e57XmlWriteCStringElement(writer, "coordinateMetadata", e57Doc.CoordinateMetadata)
        ' creationDateTime (start Parent)
        e57XmlStartParentElement(writer, "creationDateTime", "Structure")
        e57XmlWriteStringElement(writer, "dateTimeValue", "Float", timeToGPStime(e57Doc.Date))
        e57XmlWriteStringElement(writer, "isAtomicClockReferenced", "Integer", Math.Abs(CInt(e57Doc.IsAtomicClockReferenced))) '.ToString)
        e57XmlEndParentElement(writer)
        Return True
    End Function


    Public Function e57XmlStartVectorChild(ByRef writer As XmlWriter, scan As Scan) As Boolean
        ' vectorChild
        e57XmlStartParentElement(writer, "vectorChild", "Structure")
        '   guid
        e57XmlWriteCStringElement(writer, "guid", scan.GUID)
        '   name
        e57XmlWriteCStringElement(writer, "name", scan.Name)
        '   description
        e57XmlWriteCStringElement(writer, "description", scan.Description)
        '   sensorVendor
        e57XmlWriteCStringElement(writer, "sensorVendor", scan.SensorVendor)
        '   sensorModel
        e57XmlWriteCStringElement(writer, "sensorModel", scan.SensorModel)
        '   sensorSerialNumber
        e57XmlWriteCStringElement(writer, "sensorSerialNumber", scan.SensorSerialNumber)
        '   sensorSoftwareVersion
        e57XmlWriteCStringElement(writer, "sensorSoftwareVersion", scan.SensorSoftwareVersion)
        '   sensorFirmwareVersion
        e57XmlWriteCStringElement(writer, "sensorFirmwareVersion", scan.SensorFirmwareVersion)
        Return True
    End Function

    Public Function e57XmlWriteCartesianBoundsElement(ByRef writer As XmlWriter, scan As Scan) As Boolean
        '   cartesianBounds (start)
        e57XmlStartParentElement(writer, "cartesianBounds", "Structure")
        '       xMinimum
        e57XmlWriteStringElement(writer, "xMinimum", "Float", scan.CartesianBounds(0).ToString("R", CultureInfo.InvariantCulture))
        '       xMaximum
        e57XmlWriteStringElement(writer, "xMaximum", "Float", scan.CartesianBounds(1).ToString("R", CultureInfo.InvariantCulture))
        '       yMinimum
        e57XmlWriteStringElement(writer, "yMinimum", "Float", scan.CartesianBounds(2).ToString("R", CultureInfo.InvariantCulture))
        '       yMaximum
        e57XmlWriteStringElement(writer, "yMaximum", "Float", scan.CartesianBounds(3).ToString("R", CultureInfo.InvariantCulture))
        '       zMinimum
        e57XmlWriteStringElement(writer, "zMinimum", "Float", scan.CartesianBounds(4).ToString("R", CultureInfo.InvariantCulture))
        '       zMaximum
        e57XmlWriteStringElement(writer, "zMaximum", "Float", scan.CartesianBounds(5).ToString("R", CultureInfo.InvariantCulture))
        '   cartesianBounds (end)
        e57XmlEndParentElement(writer)
        Return True
    End Function

    Public Function e57XmlWritePoseElement(ByRef writer As XmlWriter, scan As Scan) As Boolean
        '   pose (start)
        e57XmlStartParentElement(writer, "pose", "Structure")
        '       rotation (start)
        e57XmlStartParentElement(writer, "rotation", "Structure")
        '           w
        e57XmlWriteStringElement(writer, "w", "Float", scan.Pose(0).ToString("R", CultureInfo.InvariantCulture))
        '           x
        e57XmlWriteStringElement(writer, "x", "Float", scan.Pose(1).ToString("R", CultureInfo.InvariantCulture))
        '           y
        e57XmlWriteStringElement(writer, "y", "Float", scan.Pose(2).ToString("R", CultureInfo.InvariantCulture))
        '           z
        e57XmlWriteStringElement(writer, "z", "Float", scan.Pose(3).ToString("R", CultureInfo.InvariantCulture))
        '       rotation (end)
        e57XmlEndParentElement(writer)
        '       translation (start)
        e57XmlStartParentElement(writer, "translation", "Structure")
        '           x
        e57XmlWriteStringElement(writer, "x", "Float", scan.Pose(4).ToString("R", CultureInfo.InvariantCulture))
        '           y
        e57XmlWriteStringElement(writer, "y", "Float", scan.Pose(5).ToString("R", CultureInfo.InvariantCulture))
        '           z
        e57XmlWriteStringElement(writer, "z", "Float", scan.Pose(6).ToString("R", CultureInfo.InvariantCulture))
        '       translation (end)
        e57XmlEndParentElement(writer)
        '   pose (end)
        e57XmlEndParentElement(writer)
        Return True
    End Function

    Public Function e57XmlWritePointsElement(ByRef writer As XmlWriter, scan As Scan) As Boolean
        '   points (start) (Data)
        e57XmlStartParentElement(writer, "points", "CompressedVector")
        writer.WriteAttributeString("fileOffset", scan.Data.FileOffset)
        writer.WriteAttributeString("recordCount", scan.Data.RecordCount)

        '       prototype (start)
        e57XmlStartParentElement(writer, "prototype", "Structure")
        For i As Integer = 0 To scan.Data.Prototype.Count - 1 Step 1
            Select Case scan.Data.Prototype(i).ElementType
                Case ElementType.Double
                    Dim name As String = scan.Data.Prototype(i).ElementName
                    e57XmlWriteAttributeElement(writer, name, "Float", "precision", "double")
                Case ElementType.Single
                    Dim name As String = scan.Data.Prototype(i).ElementName
                    e57XmlWriteAttributeElement(writer, name, "Float", "precision", "single")
                Case ElementType.ScaledInteger
                    Dim name As String = scan.Data.Prototype(i).ElementName
                    writer.WriteStartElement(name)
                    writer.WriteAttributeString("type", "ScaledInteger")
                    writer.WriteAttributeString("minimum", "00") 'scan.Data.Prototype(i).Min)
                    writer.WriteAttributeString("maximum", "11") 'scan.Data.Prototype(i).Max)
                    writer.WriteAttributeString("scale", "22") 'scan.Data.Prototype(i).Scale)
                    writer.WriteEndElement()
            End Select
        Next
        '       prototype (end)
        e57XmlEndParentElement(writer)
        '       codecs
        e57XmlWriteAttributeElement(writer, "codecs", "Vector", "allowHeterogeneousChildren", "1")
        '   points (end)
        e57XmlEndParentElement(writer)
        Return True
    End Function


    Public Function e57XmlStartParentElement(ByRef writer As XmlWriter, name As String, type As String) As Boolean
        ' Start Parent Element (Can be ended with an xmlEndParentElement
        writer.WriteStartElement(name)
        writer.WriteAttributeString("type", type)
        Return True
    End Function

    Public Function e57XmlEndParentElement(ByRef writer As XmlWriter) As Boolean
        'End Element
        writer.WriteEndElement()
        Return True
    End Function


    Public Function e57XmlWriteStringElement(ByRef writer As XmlWriter, name As String, type As String, data As String, Optional attributeName As String = Nothing, Optional attributeValue As String = Nothing) As Boolean
        'Write a String Element with start and end formatting
        writer.WriteStartElement(name)
        writer.WriteAttributeString("type", type)
        If attributeName IsNot Nothing And attributeValue IsNot Nothing Then
            writer.WriteAttributeString("precision", "single")
        End If
        writer.WriteString(data)
        writer.WriteEndElement()
        Return True
    End Function

    Public Function e57XmlWriteCStringElement(ByRef writer As XmlWriter, name As String, data As String) As Boolean
        'Write a CString Element with start and end formatting
        writer.WriteStartElement(name)
        writer.WriteAttributeString("type", "String")
        writer.WriteCData(data)
        writer.WriteEndElement()
        Return True
    End Function

    Public Function e57XmlWriteAttributeElement(ByRef writer As XmlWriter, name As String, type As String, attributeName As String, attributeValue As String) As Boolean
        'Write a Attribute Element with start and end formatting
        writer.WriteStartElement(name)
        writer.WriteAttributeString("type", type)
        writer.WriteAttributeString(attributeName, attributeValue)
        writer.WriteEndElement()
        Return True
    End Function


    Public Function timeToGPStime(time As DateTime) As String
        time = Date.Now
        Dim gpsStartDate As New DateTime(1980, 1, 1, 0, 0, 0)
        Dim gpsTime As Double = (time - gpsStartDate).TotalSeconds
        Return gpsTime.ToString("R", CultureInfo.InvariantCulture)
    End Function


    '---------------------------------------------------------------------------------------------
    ' Todos



    '   indexBounds (start)
    'e57XmlStartParentElement(writer, "indexBounds", "Structure")
    '       rowMinimum
    'e57XmlWriteStringElement(writer, "rowMinimum", "Integer", "0")
    '       rowMaximum
    'e57XmlWriteStringElement(writer, "rowMaximum", "Integer", "647")
    '       columnMinimum
    'e57XmlWriteStringElement(writer, "columnMinimum", "Integer", "0")
    '       columnMaximum
    'e57XmlWriteStringElement(writer, "columnMaximum", "Integer", "1555")
    '       returnMinimum
    'e57XmlWriteStringElement(writer, "returnMinimum", "Integer", "0")
    '       returnMaximum
    'e57XmlWriteStringElement(writer, "returnMaximum", "Integer", "1111")
    '   indexBounds (end)
    'e57XmlEndParentElement(writer)

    '---write function (with cases)---
    '<intensityLimits type = "Structure" >
    '       <intensityMaximum type="ScaledInteger" minimum="0" maximum="2047" scale="4.8851978505129456e-004">2047</intensityMaximum>
    '       <intensityMinimum type = "ScaledInteger" minimum="0" maximum="2047" scale="4.8851978505129456e-004"/>
    '</intensityLimits>

    'Public Function intensityLimits(ByRef writer As XmlWriter, scan As Scan) As Boolean
    '   ' Start intensityLimits
    '  e57XmlStartParentElement(writer, "intensityLimits", "Structure")
    'For i As Integer = 0 To scan.IntensityLimits.Count - 1 Step 1
    'Select Case scan.IntensityLimits.ElementType
    'Case ElementType.Double
    'Dim name As String = scan.IntensityLimits.ElementName
    '               e57XmlWriteStringElement(writer, "intensityLimits", "Float", scan.IntensityLimits.Value)
    'Case ElementType.Integer
    'Dim name As String = scan.IntensityLimits.ElementName
    '               e57XmlWriteAttributeElement(writer, "intensityLimits", "Float", "precision", "single")
    'Case ElementType.ScaledInteger
    'Dim name As String = scan.IntensityLimits.ElementName
    '               writer.WriteStartElement("name")
    '              writer.WriteAttributeString("type", "ScaledInteger")
    '             writer.WriteAttributeString("minimum", scan.IntensityLimits.Min)
    '                writer.WriteAttributeString("maximum", scan.IntensityLimits.Max)
    '                writer.WriteAttributeString("scale", scan.IntensityLimits.Scale)
    '                writer.WriteString(scan.IntensityLimits.Value)
    '                writer.WriteEndElement()
    '        End Select
    '    Next
    '    ' End intensityLimits
    '    e57XmlEndParentElement(writer)
    '    Return True
    'End Function





    'Float/ScaledInteger/Integer

    '   intensityLimits (start)
    'e57XmlStartParentElement(writer, "intensityLimits", "Structure")
    '       intensityMinimum
    'writer.WriteStartElement("intensityMinimum")
    'writer.WriteAttributeString("type", "ScaledInteger")
    'writer.WriteAttributeString("minimum", "0")
    'writer.WriteAttributeString("maximum", "2047")
    'writer.WriteAttributeString("scale", "4.8851978505129456e-004")
    'writer.WriteString("0")
    'writer.WriteEndElement()
    '       intensityMaximum
    'writer.WriteStartElement("intensityMaximum")
    'writer.WriteAttributeString("type", "ScaledInteger")
    'writer.WriteAttributeString("minimum", "0")
    'writer.WriteAttributeString("maximum", "2047")
    'writer.WriteAttributeString("scale", "4.8851978505129456e-004")
    'writer.WriteString("0")
    'writer.WriteEndElement()
    '   intensityLimits (end)
    'e57XmlEndParentElement(writer)

End Module
