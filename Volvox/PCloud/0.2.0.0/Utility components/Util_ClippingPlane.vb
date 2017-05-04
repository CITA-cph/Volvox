Imports System.Drawing
Imports GH_IO.Serialization
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry

Public Class Util_ClippingPlane
    Inherits GH_Component
    Sub New()
        MyBase.New("ClippingPlane", "ClipPlane", "Preview geometry with a clipping plane.", "Volvox", "Util")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_UtilClippingPlane
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_ClippingPlane
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddBrepParameter("Convex Brep", "B", "Brep to get planes from", GH_ParamAccess.tree)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddPlaneParameter("Planes", "P", "Planes", GH_ParamAccess.list)
    End Sub

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetInt32("Count", planes.Count)
        For i As Integer = 0 To planes.Count - 1 Step 1
            Dim p As Plane = planes(i)
            writer.SetPlane("Plane" & i, New GH_IO.Types.GH_Plane(p.OriginX, p.OriginY, p.OriginZ, p.XAxis.X, p.XAxis.Y, p.XAxis.Z, p.YAxis.X, p.YAxis.Y, p.YAxis.Z))
            writer.SetGuid("Guid" & i, guids(i))
        Next

        Return MyBase.Write(writer)
    End Function

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        planes.Clear()
        guids.Clear()

        Dim count As Integer = reader.GetInt32("Count")

        For i As Integer = 0 To count - 1 Step 1
            Dim p As GH_IO.Types.GH_Plane = reader.GetPlane("Plane" & i)
            planes.Add(New Plane(New Point3d(p.Origin.x, p.Origin.y, p.Origin.z), New Vector3d(p.XAxis.x, p.XAxis.y, p.XAxis.z), New Vector3d(p.YAxis.x, p.YAxis.y, p.YAxis.z)))
            guids.Add(reader.GetGuid("Guid" & i))
        Next

        For Each g As Guid In guids
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
        Next
        planes.Clear()
        guids.Clear()

        Return MyBase.Read(reader)
    End Function

    Public Overrides Sub AddedToDocument(document As GH_Document)
        For Each g As Guid In guids
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
        Next
        planes.Clear()
        guids.Clear()
        MyBase.AddedToDocument(document)
    End Sub

    Public Overrides Sub RemovedFromDocument(document As GH_Document)
        For Each g As Guid In guids
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
        Next
        planes.Clear()
        guids.Clear()
        MyBase.AddedToDocument(document)
    End Sub

    Sub Disable(sender As Object, e As GH_ObjectChangedEventArgs) Handles Me.ObjectChanged
        Select Case e.Type
            Case GH_ObjectEventType.Enabled

                For Each g As Guid In guids
                    Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
                Next

                If Not ((Me.Locked) Or (Me.Hidden)) Then

                    Dim npl As New List(Of Plane)
                    Dim ngui As New List(Of Guid)

                    Dim views As New List(Of Guid)
                    For Each v As Rhino.Display.RhinoView In Rhino.RhinoDoc.ActiveDoc.Views
                        views.Add(v.ActiveViewportID)
                    Next

                    Dim atts As New Rhino.DocObjects.ObjectAttributes
                    atts.Mode = Rhino.DocObjects.ObjectMode.Normal
                    atts.Name = "VolvoxClippingPlane"

                    For Each pl As Plane In planes
                        Dim rad As Double = Grasshopper.CentralSettings.PreviewPlaneRadius
                        Dim orig As Point3d = pl.Origin
                        Dim radp As Point3d = pl.PointAt(-rad, -rad, -Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 100)
                        pl.Origin = radp
                        ngui.Add(Rhino.RhinoDoc.ActiveDoc.Objects.AddClippingPlane(pl, rad * 2, rad * 2, views, atts))
                        pl.Origin = orig
                        npl.Add(pl)
                    Next

                    planes.Clear()
                    guids.Clear()
                    planes.AddRange(npl)
                    guids.AddRange(ngui)

                End If

            Case GH_ObjectEventType.Preview

                For Each g As Guid In guids
                    Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
                Next

                If Not ((Me.Locked) Or (Me.Hidden)) Then

                    Dim npl As New List(Of Plane)
                    Dim ngui As New List(Of Guid)

                    Dim views As New List(Of Guid)
                    For Each v As Rhino.Display.RhinoView In Rhino.RhinoDoc.ActiveDoc.Views
                        views.Add(v.ActiveViewportID)
                    Next

                    Dim atts As New Rhino.DocObjects.ObjectAttributes
                    atts.Mode = Rhino.DocObjects.ObjectMode.Normal
                    atts.Name = "VolvoxClippingPlane"

                    For Each pl As Plane In planes
                        Dim rad As Double = Grasshopper.CentralSettings.PreviewPlaneRadius
                        Dim orig As Point3d = pl.Origin
                        Dim radp As Point3d = pl.PointAt(-rad, -rad, -Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 100)
                        pl.Origin = radp
                        ngui.Add(Rhino.RhinoDoc.ActiveDoc.Objects.AddClippingPlane(pl, rad * 2, rad * 2, views, atts))
                        pl.Origin = orig
                        npl.Add(pl)
                    Next

                    planes.Clear()
                    guids.Clear()
                    planes.AddRange(npl)
                    guids.AddRange(ngui)

                End If

        End Select

    End Sub

    Dim guids As New List(Of Guid)
    Dim planes As New List(Of Plane)

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        If Me.Locked Or Me.Hidden Then
            DA.SetDataList(0, planes)
            Return
        End If

        For Each g As Guid In guids
            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, True)
        Next

        planes.Clear()
        guids.Clear()

        Dim struct As New GH_Structure(Of GH_Brep)
        If Not DA.GetDataTree(0, struct) Then Return

        If struct IsNot Nothing Then
            If struct.AllData(True).Count > 0 Then
                Dim p As GH_Brep = struct.AllData(True).First

                Dim views As New List(Of Guid)
                For Each v As Rhino.Display.RhinoView In Rhino.RhinoDoc.ActiveDoc.Views
                    views.Add(v.ActiveViewportID)
                Next

                Dim atts As New Rhino.DocObjects.ObjectAttributes
                atts.Mode = Rhino.DocObjects.ObjectMode.Normal
                atts.Name = "VolvoxClippingPlane"

                For Each f As BrepFace In p.Value.Faces
                    Dim pl As New Plane()
                    If f.TryGetPlane(pl, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) Then
                        Dim rad As Double = Grasshopper.CentralSettings.PreviewPlaneRadius
                        Dim orig As Point3d = pl.Origin
                        Dim radp As Point3d = pl.PointAt(-rad, -rad, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 100)
                        pl.Origin = radp
                        pl.Flip()
                        guids.Add(Rhino.RhinoDoc.ActiveDoc.Objects.AddClippingPlane(pl, rad * 2, rad * 2, views, atts))
                        pl.Origin = orig
                        planes.Add(pl)
                    End If
                Next

            End If
        End If

        DA.SetDataList(0, planes)

    End Sub



End Class
