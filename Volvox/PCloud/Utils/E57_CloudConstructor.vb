Imports System.Drawing
Imports E57LibReader
Imports E57LibCommon
Imports Rhino.Geometry
Imports Volvox_Cloud
Imports E57LibReader.ScanData

Public Class E57_CloudConstructor
    Implements IDisposable

    Private FilePath As String = String.Empty
    Private LoadPercent As Double = 1
    Private LoadIndices As New List(Of Integer)
    Private LoadFinished As Boolean = False

    Private OutClouds() As GH_Cloud = Nothing
    Private OutPlanes() As Plane = Nothing

    Private T As Threading.Thread = Nothing
    Private Doc As e57Document = Nothing

    Public Event Message(sender As E57_CloudConstructor, e As E57MessageEventArgs)
    Public Event FinishedLoading(sender As E57_CloudConstructor, e As E57MessageEventArgs)

    Sub New(FilePath As String)
        Me.FilePath = FilePath
        T = New Threading.Thread(AddressOf ReadFile)

        Doc = Nothing
        Doc = New e57Document(FilePath, False)

        ReDim OutClouds(Doc.ScanCount - 1)
        ReDim OutPlanes(Doc.ScanCount - 1)

        For Each int As Integer In Indices
            If int > Doc.Scans.Count - 1 Then
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.ErrorMessage, "Supplied index too high."))
                Return
            End If
        Next

    End Sub

#Region "Properties"
    Public Property Percent As Double
        Get
            Return LoadPercent
        End Get
        Set(value As Double)
            LoadPercent = value
        End Set
    End Property

    Public Property Indices As List(Of Integer)
        Get
            Return LoadIndices
        End Get
        Set(value As List(Of Integer))
            If value.Contains(-1) Then
                LoadIndices.Clear()
                For i As Integer = 0 To Doc.ScanCount - 1 Step 1
                    LoadIndices.Add(i)
                Next
            Else
                LoadIndices.AddRange(value)
            End If
        End Set
    End Property

    Public ReadOnly Property Finished As Boolean
        Get
            Return LoadFinished
        End Get
    End Property

    Public ReadOnly Property Clouds As GH_Cloud()
        Get
            Return OutClouds
        End Get
    End Property

    Public ReadOnly Property Planes As Plane()
        Get
            Return OutPlanes
        End Get
    End Property

#End Region
    Sub Start(CheckFile As Boolean)
        T.Start(CheckFile)
    End Sub

    Sub Join()
        T.Join()
    End Sub

    Sub ReadFile(Check As Boolean)
        LoadFinished = False

        If Check Then
            RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CustomMessage, "Checking file..."))
            If Not Doc.CheckFile() Then
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CustomMessage, "File is corrupted."))
                Exit Sub
            Else
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CustomMessage, "Check done."))
            End If
        End If

        ReDim OutClouds(Doc.ScanCount - 1)
        ReDim OutPlanes(Doc.ScanCount - 1)

        Dim counter As Integer = 0

        For Each int As Integer In Indices
            counter += 1
            Select Case Indices.Count
                Case 1
                    RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CurrentCloudReport, "Loading 1 cloud"))
                Case Else
                    RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CurrentCloudReport, "Loading " & counter & "/" & Indices.Count & " cloud"))
            End Select

            If Not ConstructCloud(int) Then
                Exit For
            End If
            RaiseEvent FinishedLoading(Me, Nothing)
        Next

        LoadFinished = True
        RaiseEvent FinishedLoading(Me, Nothing)
    End Sub

    Private PercentStopwatch As New System.Diagnostics.Stopwatch
    Private LastPercentTime As Long

    Sub SendSubPercentMessage(sender As Object, e As BytesReadEventArgs)
        If PercentStopwatch.ElapsedMilliseconds - LastPercentTime > 500 Or (PercentStopwatch.ElapsedMilliseconds < 500 And e.Percent = 0) Then
            RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.SubPercentReport, e.Percent))
            LastPercentTime = PercentStopwatch.ElapsedMilliseconds
        End If
    End Sub

    Function ConstructCloud(int As Integer) As Boolean
        If int > Doc.Scans.Count - 1 Then
            RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.ErrorMessage, "Supplied index too high"))
            Return False
        End If

        Dim thisScan As Scan = Doc.Scans(int)
        AddHandler thisScan.BytesRead, AddressOf SendSubPercentMessage
        PercentStopwatch.Restart()
        thisScan.ReadScanData(LoadPercent)
        PercentStopwatch.Stop()
        RemoveHandler thisScan.BytesRead, AddressOf SendSubPercentMessage
        Dim data As ScanData = thisScan.Data
        Dim pc As New PointCloud

        RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CustomMessage, "Creating Cloud"))

        'coordinates
        If data.HasCartesian Then

            Dim xl As New List(Of Double)
            Dim yl As New List(Of Double)
            Dim zl As New List(Of Double)

            data.GetList(ElementName.CartesianX, xl)
            data.GetList(ElementName.CartesianY, yl)
            data.GetList(ElementName.CartesianZ, zl)

            If (xl.Count <> yl.Count) Or (xl.Count <> zl.Count) Then
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.ErrorMessage, "File might be corrupted"))
                Return False
            End If

            For i As Integer = 0 To xl.Count - 1 Step 1
                pc.Add(New Point3d(xl(i), yl(i), zl(i)))
            Next

        ElseIf data.HasSpherical Then

            Dim azimuth As New List(Of Double)
            Dim range As New List(Of Double)
            Dim elevation As New List(Of Double)

            data.GetList(ElementName.SphericalAzimuth, azimuth)
            data.GetList(ElementName.SphericalRange, range)
            data.GetList(ElementName.SphericalElevation, elevation)

            If (azimuth.Count <> range.Count) Or (azimuth.Count <> elevation.Count) Then
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.ErrorMessage, "File might be corrupted"))
                Return False
            End If

            For i As Integer = 0 To azimuth.Count - 1 Step 1
                Dim sp() As Double = E57LibCommon.Maths.SphericalToCartesian(range(i), azimuth(i), elevation(i))
                pc.Add(New Point3d(sp(0), sp(1), sp(2)))
            Next

        End If

        'color 
        If data.HasColor Then

            Dim redl As New List(Of Double)
            Dim greenl As New List(Of Double)
            Dim bluel As New List(Of Double)

            data.GetList(ElementName.ColorRed, redl)
            data.GetList(ElementName.ColorGreen, greenl)
            data.GetList(ElementName.ColorBlue, bluel)

            If (redl.Count <> greenl.Count) Or (redl.Count <> bluel.Count) Then
                RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.ErrorMessage, "File might be corrupted"))
                Return False
            End If

            Dim redint As New Interval(thisScan.ColorLimits(0), thisScan.ColorLimits(1))
            Dim greenint As New Interval(thisScan.ColorLimits(2), thisScan.ColorLimits(3))
            Dim blueint As New Interval(thisScan.ColorLimits(4), thisScan.ColorLimits(5))

            For i As Integer = 0 To redl.Count - 1 Step 1
                Dim thiscolor As Color = Color.FromArgb(redint.NormalizedParameterAt(redl(i)) * 255,
                                                            greenint.NormalizedParameterAt(greenl(i)) * 255,
                                                            blueint.NormalizedParameterAt(bluel(i)) * 255)

                pc(i).Color = thiscolor
            Next

        End If

        'intensity 
        If data.HasIntensity Then
            Dim intens As New List(Of Double)
            If data.GetList(ElementName.Intensity, intens) Then
                pc.UserDictionary.Set(ElementName.Intensity, intens)
                If data.HasColor = 0 Then
                    Dim intensitv As New Interval(thisScan.IntensityLimits(0), thisScan.IntensityLimits(1))

                    For i As Integer = 0 To intens.Count - 1 Step 1
                        Dim thiscolor As Color = Color.FromArgb(intensitv.NormalizedParameterAt(intens(i)) * 255,
                                                                    intensitv.NormalizedParameterAt(intens(i)) * 255,
                                                                    intensitv.NormalizedParameterAt(intens(i)) * 255)
                        pc(i).Color = thiscolor
                    Next
                End If
            End If
        End If

        'time stamp 
        If data.HasTimeStamp Then
            Dim ts As New List(Of Double)
            If data.GetList(ElementName.TimeStamp, ts) Then
                pc.UserDictionary.Set(ElementName.TimeStamp, ts)
            End If
        End If

        AddToDictionary(pc, ElementName.CartesianInvalidState, data)
        AddToDictionary(pc, ElementName.ColumnIndex, data)
        AddToDictionary(pc, ElementName.IsColorInvalid, data)
        AddToDictionary(pc, ElementName.IsIntensityInvalid, data)
        AddToDictionary(pc, ElementName.IsTimeStampInvalid, data)
        AddToDictionary(pc, ElementName.ReturnCount, data)
        AddToDictionary(pc, ElementName.ReturnIndex, data)
        AddToDictionary(pc, ElementName.RowIndex, data)

        Dim trans() As Double = thisScan.Translation
        Dim rot() As Double = thisScan.Rotation

        Dim rq As New Quaternion(rot(0), rot(1), rot(2), rot(3))

        Dim ang As Double
        Dim axi As New Vector3d
        rq.GetRotation(ang, axi)

        Dim thistrans As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Translation(trans(0), trans(1), trans(2))
        Dim thisrotation As Rhino.Geometry.Transform = Rhino.Geometry.Transform.Rotation(ang, axi, New Point3d(0, 0, 0))
        If Not thisrotation.IsValid Then thisrotation = Transform.Identity

        Dim pos As New Plane(Plane.WorldXY)

        pos.Transform(thisrotation)
        pos.Transform(thistrans)

        OutPlanes(int) = (pos)

        'pc.Transform(thisrotation) 'SCANNER POSITION AMBIGUITY BUG =======================================================
        'pc.Transform(thistrans)    'SCANNER POSITION AMBIGUITY BUG =======================================================

        Dim ghc As New GH_Cloud(pc)

        OutClouds(int) = (ghc)

        RaiseEvent Message(Me, New E57MessageEventArgs(E57MessageEventArgs.MessageType.CustomMessage, ""))
        data.Clear()
        data = Nothing
        Return True
    End Function

    Function AddToDictionary(ByRef Cloud As PointCloud, ElementName As String, ByRef Data As ScanData) As Boolean
        If Data.HasData(ElementName) = ElementType.Empty Then Return False

        Select Case Data.TypeOfElement(ElementName)
            Case ElementType.Double
                Dim templ As New List(Of Double)
                Data.GetList(ElementName, templ)
                Cloud.UserDictionary.Set(ElementName, templ)
            Case ElementType.Integer
                Dim templ As New List(Of Int64)
                Data.GetList(ElementName, templ)
                Dim smaller As New List(Of Integer)
                For Each number As Int64 In templ
                    smaller.Add(number)
                Next
                Cloud.UserDictionary.Set(ElementName, smaller)
        End Select

        Return True
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then

                If T IsNot Nothing Then
                    T.Abort()
                End If

                OutClouds = Nothing
                OutPlanes = Nothing
            End If
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class


