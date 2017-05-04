Imports System.Globalization
Imports System.Xml
Imports E57LibCommon.Enums
Imports E57LibCommon.Maths

Public Module E57Xml

    Friend Function Check(V As String) As Boolean
        If V = Nothing Then Return False
        If V = String.Empty Then Return False
        If V = "" Then Return False
        Return True
    End Function

    Friend Function Check(V As Double) As Boolean
        If Double.IsNaN(V) Then Return False
        Return True
    End Function

    Friend Function Check(V As Date) As Boolean
        If V = Nothing Then Return False
        Return True
    End Function

    Friend Function XStartRoot(ByRef w As XmlWriter) As Boolean
        ' Set e57Root (Can be ended with an xmlEndParentElement)
        w.WriteStartElement("e57Root", "http://www.astm.org/COMMIT/E57/2010-e57-v1.0")
        w.WriteAttributeString("type", "Structure")
        Return True
    End Function

    Friend Function XStartData3D(ByRef w As XmlWriter) As Boolean
        ' Set Data3D Header (Can be ended with an xmlEndParentElement)
        w.WriteStartElement("data3D")
        w.WriteAttributeString("type", "Vector")
        w.WriteAttributeString("allowHeterogeneousChildren", "1")
        Return True
    End Function

    Friend Function XWriteDocMetaDataElement(ByRef w As XmlWriter, doc As e57Document) As Boolean

        'required
        XCString(w, "formatName", doc.FormatName)
        If Not Check(doc.Guid) Then doc.GenerateGuid()
        XCString(w, "guid", doc.Guid)
        XInteger(w, "versionMajor",,, doc.VersionMajor)
        XInteger(w, "versionMinor",,, doc.VersionMinor)

        'optional
        If Check(doc.LibraryVersion) Then XCString(w, "e57LibraryVersion", doc.LibraryVersion)

        If Check(doc.Date) Then
            XStartParent(w, "creationDateTime", "Structure")
            XFloat(w, "dateTimeValue",,,, (GPSTime(doc.Date)))
            XInteger(w, "isAtomicClockReferenced",,, Math.Abs(CInt(doc.IsAtomicClockReferenced)))
            XEndParent(w)
        End If

        If Check(doc.CoordinateMetadata) Then XCString(w, "coordinateMetadata", doc.CoordinateMetadata)

        Return True
    End Function

    Friend Function XStartVectorChild(ByRef w As XmlWriter, s As Scan) As Boolean
        XStartParent(w, "vectorChild", "Structure")
        If Not Check(s.Guid) Then s.GenerateGuid()
        XCString(w, "guid", s.Guid)

        If Check(s.Name) Then XCString(w, "name", s.Name)
        If Check(s.Description) Then XCString(w, "description", s.Description)
        If Check(s.SensorVendor) Then XCString(w, "sensorVendor", s.SensorVendor)
        If Check(s.SensorModel) Then XCString(w, "sensorModel", s.SensorModel)
        If Check(s.SensorSerialNumber) Then XCString(w, "sensorSerialNumber", s.SensorSerialNumber)
        If Check(s.SensorSoftwareVersion) Then XCString(w, "sensorSoftwareVersion", s.SensorSoftwareVersion)
        If Check(s.SensorFirmwareVersion) Then XCString(w, "sensorFirmwareVersion", s.SensorFirmwareVersion)

        If Check(s.StartDate) Then
            XStartParent(w, "acquisitionStart", "Structure")
            XFloat(w, "dateTimeValue",,,, (GPSTime(s.StartDate)))
            XInteger(w, "isAtomicClockReferenced",,, Math.Abs(CInt(s.IsStartAtomicClockReferenced)))
            XEndParent(w)
        End If

        If Check(s.EndDate) Then
            XStartParent(w, "acquisitionEnd", "Structure")
            XFloat(w, "dateTimeValue",,,, GPSTime(s.EndDate))
            XInteger(w, "isAtomicClockReferenced",,, Math.Abs(CInt(s.IsEndAtomicClockReferenced)))
            XEndParent(w)
        End If

        If Check(s.Temperature) Then XFloat(w, "temperature",,,, s.Temperature)
        If Check(s.RelativeHumidity) Then XFloat(w, "relativeHumidity",,,, s.RelativeHumidity)
        If Check(s.AtmosphericPressure) Then XFloat(w, "atmosphericPressure",,,, s.AtmosphericPressure)

        Return True
    End Function


    Friend Function XOriginalGuids(ByRef w As XmlWriter, s As Scan) As Boolean
        If s.OriginalGuids.Count = 0 Then Return True

        XStartParent(w, "originalGuids", "Vector")
        For i As Integer = 0 To s.OriginalGuids.Count - 1 Step 1
            If Check(s.OriginalGuids(i)) Then XCString(w, "guid", s.OriginalGuids(i))
        Next
        XEndParent(w)

        Return True
    End Function

    Friend Function XCartesianBounds(ByRef w As XmlWriter, s As Scan) As Boolean
        '   cartesianBounds (start)
        XStartParent(w, "cartesianBounds", "Structure")

        XFloat(w, "xMinimum",,,, s.CartesianBounds(0))
        XFloat(w, "xMaximum",,,, s.CartesianBounds(1))
        XFloat(w, "yMinimum",,,, s.CartesianBounds(2))
        XFloat(w, "yMaximum",,,, s.CartesianBounds(3))
        XFloat(w, "zMinimum",,,, s.CartesianBounds(4))
        XFloat(w, "zMaximum",,,, s.CartesianBounds(5))

        '   cartesianBounds (end)
        XEndParent(w)
        Return True
    End Function

    Friend Function XSphericalBounds(ByRef w As XmlWriter, s As Scan) As Boolean
        XStartParent(w, "sphericalBounds", "Structure")

        XFloat(w, "rangeMinimum",,,, s.SphericalBounds(0))
        XFloat(w, "rangeMaximum",,,, s.SphericalBounds(1))
        XFloat(w, "elevationMinimum",,,, s.SphericalBounds(2))
        XFloat(w, "elevationMaximum",,,, s.SphericalBounds(3))
        XFloat(w, "azimuthStart",,,, s.SphericalBounds(4))
        XFloat(w, "azimuthEnd",,,, s.SphericalBounds(5))

        XEndParent(w)
        Return True
    End Function

    Friend Function XResolveType(w As XmlWriter, s As Scan, Etype As ElementType, Ename As String, Evalue As Object) As Boolean

        Select Case Etype
            Case ElementType.Integer
                XInteger(w, Ename,,, Evalue)
            Case ElementType.Double
                XFloat(w, Ename,,,, Evalue)
            Case ElementType.Single
                XFloat(w, Ename, True,,, Evalue)
        End Select

        Return True
    End Function

    Friend Function XColorLimits(ByRef w As XmlWriter, s As Scan) As Boolean
        XStartParent(w, "colorLimits", "Structure")

        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorRedMinimum", s.ColorLimits(0))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorRedMaximum", s.ColorLimits(1))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorGreenMinimum", s.ColorLimits(2))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorGreenMaximum", s.ColorLimits(3))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorBlueMinimum", s.ColorLimits(4))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.ColorBlue).ElementType, "colorBlueMaximum", s.ColorLimits(5))

        XEndParent(w)
        Return True
    End Function

    Friend Function XIntensityLimits(ByRef w As XmlWriter, s As Scan) As Boolean
        XStartParent(w, "intensityLimits", "Structure")

        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.Intensity).ElementType, "intensityMinimum", s.IntensityLimits(0))
        XResolveType(w, s, s.Data.Prototype.GetElement(ElementName.Intensity).ElementType, "intensityMaximum", s.IntensityLimits(1))

        XEndParent(w)
        Return True
    End Function

    Friend Function XPose(ByRef w As XmlWriter, s As Scan) As Boolean

        XStartParent(w, "pose", "Structure")

        XStartParent(w, "rotation", "Structure")
        XFloat(w, "w",,,, s.Pose(0))
        XFloat(w, "x",,,, s.Pose(1))
        XFloat(w, "y",,,, s.Pose(2))
        XFloat(w, "z",,,, s.Pose(3))
        XEndParent(w)

        XStartParent(w, "translation", "Structure")
        XFloat(w, "x",,,, s.Pose(4))
        XFloat(w, "y",,,, s.Pose(5))
        XFloat(w, "z",,,, s.Pose(6))
        XEndParent(w)

        XEndParent(w)

        Return True
    End Function

    Friend Function XPoints(ByRef w As XmlWriter, s As Scan) As Boolean
        '   points (start) (Data)
        XStartParent(w, "points", "CompressedVector")
        w.WriteAttributeString("fileOffset", s.CompressedVectorOffset)
        w.WriteAttributeString("recordCount", s.Data.RecordCount)

        '       prototype (start)
        XStartParent(w, "prototype", "Structure")
        For i As Integer = 0 To s.Data.Prototype.Count - 1 Step 1
            Select Case s.Data.Prototype(i).ElementType
                Case ElementType.Double
                    XFloat(w, s.Data.Prototype(i).ElementName, False)
                Case ElementType.Single
                    XFloat(w, s.Data.Prototype(i).ElementName, True)
                Case ElementType.Integer
                    Dim this As IntegerElement = s.Data.Prototype(i)
                    XInteger(w, this.ElementName, this.Minimum, this.Maximum, this.Value)
            End Select
        Next
        '       prototype (end)
        XEndParent(w)
        '       codecs
        XAttribute(w, "codecs", "Vector", "allowHeterogeneousChildren", "1")
        '   points (end)
        XEndParent(w)
        Return True
    End Function

    Friend Function XStartParent(ByRef w As XmlWriter, name As String, type As String) As Boolean
        w.WriteStartElement(name)
        w.WriteAttributeString("type", type)
        Return True
    End Function

    Friend Function XEndParent(ByRef w As XmlWriter) As Boolean
        w.WriteEndElement()
        Return True
    End Function

    Friend Function XFloat(ByRef w As XmlWriter, name As String, Optional SinglePrecision As Boolean = False, Optional minimum As Double = Double.MinValue, Optional maximum As Double = Double.MaxValue, Optional Value As Double = 0) As Boolean

        w.WriteStartElement(name)

        w.WriteAttributeString("type", "Float")

        If SinglePrecision Then
            w.WriteAttributeString("precision", "single")
            If minimum <> Double.MinValue And maximum <> Double.MaxValue Then
                w.WriteAttributeString("minimum", Single.MinValue)
                w.WriteAttributeString("maximum", Single.MaxValue)
            Else
                w.WriteAttributeString("minimum", minimum)
                w.WriteAttributeString("maximum", maximum)
            End If
        Else
            If minimum <> Double.MinValue And maximum <> Double.MaxValue Then
                w.WriteAttributeString("minimum", minimum)
                w.WriteAttributeString("maximum", maximum)
            End If

            If Value <> 0 Then
                w.WriteString(Value.ToString("R", CultureInfo.InvariantCulture))
            End If
        End If

        w.WriteEndElement()

        Return True
    End Function

    Friend Function XInteger(ByRef w As XmlWriter, name As String, Optional minimum As Int64 = Int64.MinValue, Optional maximum As Int64 = Int64.MaxValue, Optional Value As Int64 = 0) As Boolean
        w.WriteStartElement(name)
        w.WriteAttributeString("type", "Integer")
        If minimum <> Int64.MinValue And maximum <> Int64.MaxValue Then
            w.WriteAttributeString("minimum", minimum)
            w.WriteAttributeString("maximum", maximum)
        End If
        If Value <> 0 Then
            w.WriteString(Value)
        End If
        w.WriteEndElement()
        Return True
    End Function

    Friend Function XCString(ByRef w As XmlWriter, name As String, data As String) As Boolean
        'Write a CString Element with start and end formatting
        w.WriteStartElement(name)
        w.WriteAttributeString("type", "String")
        w.WriteCData(data)
        w.WriteEndElement()
        Return True
    End Function

    Friend Function XAttribute(ByRef w As XmlWriter, name As String, type As String, attributeName As String, attributeValue As String) As Boolean
        w.WriteStartElement(name)
        w.WriteAttributeString("type", type)
        w.WriteAttributeString(attributeName, attributeValue)
        w.WriteEndElement()
        Return True
    End Function

End Module
