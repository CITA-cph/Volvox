Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports Rhino.Display
Imports Rhino.Geometry
Imports Volvox_Cloud

Public Class EfficientRdfAssoc
    Inherits GH_Component

    Sub New()
        MyBase.New("RdfAssociate", "RdfAssoc", "Associate points to IFC elements", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("a34c8cfd-d36b-4ee2-9884-9d6afc0e3679")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Clouds", GH_ParamAccess.list)
        pManager.AddParameter(New Param_FilePath, "Rdf", "R", "Association .rdf file", GH_ParamAccess.item)
        pManager.AddTransformParameter("Transformation", "Tc", "Cloud transformation", GH_ParamAccess.item)
        pManager.AddColourParameter("Color", "C", "Optional color. If not set, every resulting cloud will be colored randomly", GH_ParamAccess.item)
        Params.Input(2).Optional = True
        Params.Input(3).Optional = True
        pManager.AddBooleanParameter("All", "A", "Show unassociated points", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Clouds", "C", "Clouds", GH_ParamAccess.list)
        pManager.AddTextParameter("Names", "N", "Names", GH_ParamAccess.list)
        pManager.AddTextParameter("GUIDs", "G", "GUIDs", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Scan hits", "H", "Possible scan hits", GH_ParamAccess.list)
        pManager.AddParameter(New Param_Cloud, "Rest", "R", "Unassociated points", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim colrand As Boolean = False

        Dim optcol As New Color
        If Not DA.GetData(3, optcol) Then
            colrand = True
        End If

        Dim trans As Rhino.Geometry.Transform = Nothing
        DA.GetData(2, trans)

        Dim points As New List(Of PointCloud)
        If Not DA.GetDataList(0, points) Then Return

        Dim rdf As String = String.Empty
        If Not DA.GetData(1, rdf) Then Return

        Dim ptTrans As New Rhino.Geometry.Transform
        If trans = Nothing Then ptTrans = Rhino.Geometry.Transform.Identity

        ptTrans = trans

        'Dim merged As New PointCloud

        Dim pts As New List(Of GH_Cloud)
        Dim ints As New List(Of Integer)
        Dim gs As New List(Of String)
        Dim nam As New List(Of String)
        Dim poss As New List(Of Integer)

        Dim rnd As New Random(2)

        Dim map As New List(Of Integer())
        Dim glob As Integer = 0

        For i As Integer = 0 To points.Count - 1 Step 1
            For j As Integer = 0 To points(i).Count - 1 Step 1
                Dim thismap(1) As Integer
                thismap(0) = i
                thismap(1) = j
                map.Add(thismap)
            Next
        Next

        Dim flags(map.Count - 1) As Boolean

        Using str As StreamReader = New StreamReader(rdf)

            Dim thisname As String = String.Empty
            Dim thisguid As String = String.Empty
            Dim thisints As New List(Of Integer)
            Dim thishits As Integer = 0

            While Not str.EndOfStream
                Dim s As String = str.ReadLine

                If s.Contains("SBS_PC_") Then
                    thisname = s.Trim("<").Trim(">")
                    thisguid = String.Empty
                    thisints = New List(Of Integer)
                End If

                If s.Contains("<rel:subsetRepOf>") Then
                    thisguid = GetGuid(s)
                End If

                If s.Contains("<rel:pointSubsetContains>") Then
                    thisints = GetInts(s)
                End If

                If s.Contains("<rel:possibleScanHits>") Then
                    thishits = GetHits(s)
                End If

                If (thisname <> String.Empty) And (thisguid <> String.Empty) And (thisints.Count > 0) And (thishits > 0) Then

                    Dim tempc As New PointCloud

                    Dim col As New ColorHSL

                    Select Case colrand
                        Case True
                            col = New ColorHSL(rnd.NextDouble, 1, 0.5)
                        Case False
                            col = New ColorHSL(optcol)
                    End Select

                    For Each int As Integer In thisints
                        flags(int) = True
                        Dim thismap As Integer() = map(int)
                        tempc.Add(points(thismap(0))(thismap(1)).Location)
                    Next

                    Dim mybb As BoundingBox = tempc.GetBoundingBox(False)
                    Dim bbh As Double = mybb.GetCorners(0).DistanceTo(mybb.GetCorners(4))
                    Dim pt0 As Point3d = mybb.GetCorners(0)
                    Dim denom As Double = (1 / bbh)
                    Dim denom2 As Double = 1 / 5
                    Dim l As Double = col.L

                    For i As Integer = 0 To tempc.Count - 1 Step 1
                        Dim thisd As Double = tempc(i).Location.Z - pt0.Z
                        col.L = thisd * denom
                        col.L *= denom2
                        col.L += 0.4
                        tempc(i).Color = col
                        col.L = l
                    Next


                    tempc.Transform(ptTrans)

                    pts.Add(New GH_Cloud(tempc))
                    nam.Add(thisname)
                    poss.Add(thishits)
                    gs.Add(thisguid)

                    thisname = String.Empty
                    thisguid = String.Empty
                    thisints.Clear()
                    thishits = 0
                End If

            End While

        End Using

        DA.SetDataList(3, poss)
        DA.SetDataList(0, pts)
        DA.SetDataList(2, gs)
        DA.SetDataList(1, nam)

        Dim dorest As Boolean = False

        If DA.GetData(4, dorest) Then
            If dorest Then
                Dim rest As New PointCloud

                Dim bb As BoundingBox = BoundingBox.Empty

                For Each pclo As GH_Cloud In pts
                    bb.Union(pclo.Value.GetBoundingBox(False))
                Next

                Dim itv As New Interval(bb.GetCorners(0).Z, bb.GetCorners(4).Z)

                For Each int As Integer In ints


                    If Not flags(int) Then
                        Dim restmap As Integer() = map(int)
                        rest.Add(points(restmap(0))(restmap(1)).Location)
                        Dim p As Point3d = rest(rest.Count - 1).Location
                        Dim v As Double = 64 + itv.NormalizedParameterAt(p.Z) * 127
                        rest(rest.Count - 1).Color = Color.FromArgb(v, v, v)
                    End If
                Next

                rest.Transform(ptTrans)

                DA.SetData(4, New GH_Cloud(rest))
            End If
        End If


    End Sub


    Function GetGuid(s As String) As String
        '  <rel:subsetRepOf> "11dNe1_A9DlhgcQ6g03dGj"^^<xsd:string> .
        s = s.Split(Chr(34))(1)
        Dim ss() As String = s.Split(" ")

        Return ss(0)
    End Function

    Function GetInts(s As String) As List(Of Integer)
        Dim l As New List(Of Integer)

        '< rel:pointSubsetContains> "2713631 2713930-2713972 2714378 2714460 2714558"

        s = s.Split(Chr(34))(1)
        Dim ss() As String = s.Split(" ")

        For Each str As String In ss
            If str <> String.Empty Then
                Dim test As Integer
                If Integer.TryParse(str, test) Then
                    l.Add(test)
                Else
                    Dim d() As String = str.Split("-")

                    Dim n1 As Integer
                    Dim n2 As Integer

                    For i As Integer = 0 To d.Length - 1 Step 1
                        If i = 0 Then
                            n1 = Integer.Parse(d(i))
                        Else
                            If Integer.TryParse(d(i), n2) Then Exit For
                        End If
                    Next

                    For i As Integer = n1 To n2 Step 1
                        l.Add(i)
                    Next

                End If
            End If
        Next

        Return l
    End Function

    Function GetHits(s As String) As Integer
        '    <rel:possibleScanHits> "1886"^^<xsd:nonNegativeInteger> ;

        Return Integer.Parse(s.Split(Chr(34))(1))

    End Function

End Class
