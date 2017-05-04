Imports System.Drawing
Imports System.IO
Imports E57LibWriter
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class E57_CloudExporter
    Implements IDisposable

    Public Event FinishedLoading()
    Public Event Message(sender As E57_CloudExporter, e As E57MessageEventArgs)
    Private own As GH_Component = Nothing

    Dim T As System.Threading.Thread = Nothing
    Friend Property IsRunning As Boolean = False

    Dim doc As e57Document = Nothing
    Friend spher As Boolean = False

    Friend fpath As String = String.Empty
    Friend fguid As String = String.Empty
    Friend fmeta As String = String.Empty
    Friend fpclo As New List(Of PointCloud)
    Friend spose As New List(Of Plane)
    Friend sguid As New List(Of String)
    Friend sname As New List(Of String)
    Friend sdesc As New List(Of String)
    Friend svend As New List(Of String)
    Friend smode As New List(Of String)
    Friend sseri As New List(Of String)
    Friend shard As New List(Of String)
    Friend ssoft As New List(Of String)
    Friend sfirm As New List(Of String)
    Friend stemp As New List(Of Double)
    Friend shumi As New List(Of Double)
    Friend spres As New List(Of Double)
    Friend sstim As New List(Of Date)
    Friend setim As New List(Of Date)
    Friend ssboo As New List(Of Boolean)
    Friend seboo As New List(Of Boolean)

    Sub New(Owner As GH_Component)
        own = Owner
        fpclo.Clear()
        spose.Clear()
        sguid.Clear()
        sname.Clear()
        sdesc.Clear()
        svend.Clear()
        smode.Clear()
        sseri.Clear()
        shard.Clear()
        ssoft.Clear()
        sfirm.Clear()
        stemp.Clear()
        shumi.Clear()
        spres.Clear()
        sstim.Clear()
        setim.Clear()
        seboo.Clear()
        ssboo.Clear()

    End Sub

    Public Sub Start()
        Me.IsRunning = True
        T = New Threading.Thread(AddressOf Save)
        T.Start()
    End Sub

    Public Sub SimpleStart()
        Me.IsRunning = True
        T = New Threading.Thread(AddressOf SimpleSave)
        T.Start()
    End Sub

    Public Sub Join()
        If T IsNot Nothing Then
            T.Join()
        End If
        Me.IsRunning = False
    End Sub

    Public Sub Abort()
        If T IsNot Nothing Then
            T.Abort()
        End If
        Me.IsRunning = False
    End Sub

    Private Sub Save()

        If Not Directory.Exists(Path.GetDirectoryName(Me.fpath)) Then
            Dim dir As DirectoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(Me.fpath))
        End If

        doc = New e57Document()

        doc.Guid = fguid
        doc.CoordinateMetadata = fmeta
        doc.Date = DateTime.Now
        doc.LibraryVersion = "Volvox Grasshopper plugin"

        For i As Integer = 0 To fpclo.Count - 1 Step 1

            Dim sc As New Scan(doc)

            With sc
                .AtmosphericPressure = spres(i)
                .Temperature = stemp(i)
                .RelativeHumidity = shumi(i)
                .Description = sdesc(i)
                .Guid = sguid(i)
                .Name = sname(i)
                .Pose = PlaneToPose(spose(i))
                .SensorModel = smode(i)
                .SensorSerialNumber = sseri(i)
                .SensorSoftwareVersion = ssoft(i)
                .SensorHardwareVersion = shard(i)
                .SensorFirmwareVersion = sfirm(i)
                .SensorVendor = svend(i)
                .StartDate = sstim(i)
                .EndDate = setim(i)
                .IsStartAtomicClockReferenced = ssboo(i)
                .IsEndAtomicClockReferenced = seboo(i)
            End With

            Dim pc As PointCloud = fpclo(i)

            Dim xl As New List(Of Double)
            Dim yl As New List(Of Double)
            Dim zl As New List(Of Double)

            Dim rl As List(Of Int64) = Nothing
            Dim gl As List(Of Int64) = Nothing
            Dim bl As List(Of Int64) = Nothing

            If pc.ContainsColors Then
                rl = New List(Of Int64)
                gl = New List(Of Int64)
                bl = New List(Of Int64)
            End If


            If spher Then
                Dim thist As Transform = Rhino.Geometry.Transform.PlaneToPlane(spose(i), Plane.WorldXY)
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim p As Point3d = pc(j).Location
                    p.Transform(thist)
                    Dim coords() As Double = E57LibCommon.CartesianToSpherical(p.X, p.Y, p.Z)
                    xl.Add(coords(0))
                    yl.Add(coords(1))
                    zl.Add(coords(2))
                Next
                sc.AppendData(E57LibCommon.ElementName.SphericalAzimuth, xl)
                sc.AppendData(E57LibCommon.ElementName.SphericalElevation, yl)
                sc.AppendData(E57LibCommon.ElementName.SphericalRange, zl)
            Else
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim p As Point3d = pc(j).Location
                    xl.Add(p.X)
                    yl.Add(p.Y)
                    zl.Add(p.Z)
                Next
                sc.AppendData(E57LibCommon.ElementName.CartesianX, xl)
                sc.AppendData(E57LibCommon.ElementName.CartesianY, yl)
                sc.AppendData(E57LibCommon.ElementName.CartesianZ, zl)
            End If

            If pc.ContainsColors Then
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim col As Color = pc(j).Color
                    rl.Add(col.R)
                    gl.Add(col.G)
                    bl.Add(col.B)
                Next
                sc.AppendData(E57LibCommon.ElementName.ColorRed, rl)
                sc.AppendData(E57LibCommon.ElementName.ColorGreen, gl)
                sc.AppendData(E57LibCommon.ElementName.ColorBlue, bl)
            End If

            doc.Scans.Add(sc)
        Next

        If Not doc.Save(Me.fpath) Then
            Me.own.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Save failed")
        End If
        doc.Scans.Clear()
        doc = Nothing

        Me.IsRunning = False
        Threading.Thread.Sleep(100)
        RaiseEvent FinishedLoading()

    End Sub

    Private Sub SimpleSave()

        If Not Directory.Exists(Path.GetDirectoryName(Me.fpath)) Then
            Dim dir As DirectoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(Me.fpath))
        End If

        doc = New e57Document()

        doc.Guid = fguid
        doc.CoordinateMetadata = fmeta
        doc.Date = DateTime.Now
        doc.LibraryVersion = "Volvox Grasshopper plugin"

        For i As Integer = 0 To fpclo.Count - 1 Step 1

            Dim sc As New Scan(doc)

            Dim pc As PointCloud = fpclo(i)

            Dim xl As New List(Of Double)
            Dim yl As New List(Of Double)
            Dim zl As New List(Of Double)

            Dim rl As List(Of Int64) = Nothing
            Dim gl As List(Of Int64) = Nothing
            Dim bl As List(Of Int64) = Nothing

            If pc.ContainsColors Then
                rl = New List(Of Int64)
                gl = New List(Of Int64)
                bl = New List(Of Int64)
            End If

            sc.Pose = PlaneToPose(spose(i))

            If spher Then
                Dim thist As Transform = Rhino.Geometry.Transform.PlaneToPlane(spose(i), Plane.WorldXY)
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim p As Point3d = pc(j).Location
                    p.Transform(thist)
                    Dim coords() As Double = E57LibCommon.CartesianToSpherical(p.X, p.Y, p.Z)
                    xl.Add(coords(0))
                    yl.Add(coords(1))
                    zl.Add(coords(2))
                Next
                sc.AppendData(E57LibCommon.ElementName.SphericalAzimuth, xl)
                sc.AppendData(E57LibCommon.ElementName.SphericalElevation, yl)
                sc.AppendData(E57LibCommon.ElementName.SphericalRange, zl)
            Else
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim p As Point3d = pc(j).Location
                    xl.Add(p.X)
                    yl.Add(p.Y)
                    zl.Add(p.Z)
                Next
                sc.AppendData(E57LibCommon.ElementName.CartesianX, xl)
                sc.AppendData(E57LibCommon.ElementName.CartesianY, yl)
                sc.AppendData(E57LibCommon.ElementName.CartesianZ, zl)
            End If

            If pc.ContainsColors Then
                For j As Integer = 0 To pc.Count - 1 Step 1
                    Dim col As Color = pc(j).Color
                    rl.Add(col.R)
                    gl.Add(col.G)
                    bl.Add(col.B)
                Next
                sc.AppendData(E57LibCommon.ElementName.ColorRed, rl)
                sc.AppendData(E57LibCommon.ElementName.ColorGreen, gl)
                sc.AppendData(E57LibCommon.ElementName.ColorBlue, bl)
            End If

            doc.Scans.Add(sc)
        Next

        If Not doc.Save(Me.fpath) Then
            Me.own.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Save failed")
        End If
        doc.Scans.Clear()
        doc = Nothing

        Me.IsRunning = False
        Threading.Thread.Sleep(100)
        RaiseEvent FinishedLoading()

    End Sub

    Private Function PlaneToPose(P As Plane) As Double()
        Dim q As Quaternion = PlaneToQuaternion(P)

        Dim pose(6) As Double
        pose(0) = q.A
        pose(1) = q.B
        pose(2) = q.C
        pose(3) = q.D

        pose(4) = P.Origin.X
        pose(5) = P.Origin.Y
        pose(6) = P.Origin.Z

        Return pose
    End Function


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then

                Me.Abort()

                If doc IsNot Nothing Then
                    doc.Scans.Clear()
                    doc = Nothing
                End If

                fpclo.Clear()
                spose.Clear()
                sguid.Clear()
                sname.Clear()
                sdesc.Clear()
                svend.Clear()
                smode.Clear()
                sseri.Clear()
                shard.Clear()
                ssoft.Clear()
                sfirm.Clear()
                stemp.Clear()
                shumi.Clear()
                spres.Clear()
                sstim.Clear()
                setim.Clear()
                seboo.Clear()
                ssboo.Clear()

            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
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
