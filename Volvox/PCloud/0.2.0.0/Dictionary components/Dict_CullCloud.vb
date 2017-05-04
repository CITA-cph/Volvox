Imports System.Drawing
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Dict_CullCloud

    Inherits GH_Component

    Sub New()
        MyBase.New("Cull Cloud", "CullC", "Cull cloud points according to user data values.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictCullCloud
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_CullCloud
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to manipulate", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
        pManager.AddIntervalParameter("Domain", "D", "Values to leave", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Modified cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim strdc As String = Nothing
        Dim pc As PointCloud = Nothing
        Dim itv As New Interval

        If Not DA.GetData(0, pc) Then Return
        If Not DA.GetData(1, strdc) Then Return
        If Not DA.GetData(2, itv) Then Return

        Dim pc2 As New PointCloud

        Select Case pc.UserDictionary(strdc).GetType
            Case GetType(List(Of Double)), GetType(Double())

                Dim thisdict As New List(Of Double)
                thisdict.AddRange(pc.UserDictionary(strdc))
                Dim thisdictAfter As New List(Of Double)

                'We should have the user dictionary persist in the new point cloud. So a new dictionary should be created with the same key and with the values
                'for the points that persists. (We already iterate throught the dictionary, so it is just creating one and attaching these.)
                Select Case itv.T0 < itv.T1
                    Case True
                        For i As Integer = 0 To pc.Count - 1 Step 1
                            If itv.IncludesParameter(thisdict(i)) Then
                                thisdictAfter.Add(thisdict(i))
                                Dim thisi As PointCloudItem = pc(i)
                                If pc.ContainsColors And Not pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Color)
                                ElseIf Not pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal)
                                ElseIf pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal, thisi.Color)
                                Else
                                    pc2.Add(thisi.Location)
                                End If
                            End If
                        Next
                    Case False
                        For i As Integer = 0 To pc.Count - 1 Step 1
                            If Not itv.IncludesParameter(thisdict(i)) Then
                                Dim thisi As PointCloudItem = pc(i)
                                thisdictAfter.Add(thisdict(i))
                                If pc.ContainsColors And Not pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Color)
                                ElseIf Not pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal)
                                ElseIf pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal, thisi.Color)
                                Else
                                    pc2.Add(thisi.Location)
                                End If
                            End If
                        Next
                End Select

                pc2.UserDictionary.Set(strdc, thisdictAfter)

            Case GetType(List(Of Integer)), GetType(Integer())

                Dim thisdict As New List(Of Integer)
                thisdict.AddRange(pc.UserDictionary(strdc))
                Dim thisdictAfter As New List(Of Integer)

                'We should have the user dictionary persist in the new point cloud. So a new dictionary should be created with the same key and with the values
                'for the points that persists. (We already iterate throught the dictionary, so it is just creating one and attaching these.)
                Select Case itv.T0 < itv.T1
                    Case True
                        For i As Integer = 0 To pc.Count - 1 Step 1
                            If itv.IncludesParameter(thisdict(i)) Then
                                thisdictAfter.Add(thisdict(i))
                                Dim thisi As PointCloudItem = pc(i)
                                If pc.ContainsColors And Not pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Color)
                                ElseIf Not pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal)
                                ElseIf pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal, thisi.Color)
                                Else
                                    pc2.Add(thisi.Location)
                                End If
                            End If
                        Next
                    Case False
                        For i As Integer = 0 To pc.Count - 1 Step 1
                            If Not itv.IncludesParameter(thisdict(i)) Then
                                Dim thisi As PointCloudItem = pc(i)
                                thisdictAfter.Add(thisdict(i))
                                If pc.ContainsColors And Not pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Color)
                                ElseIf Not pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal)
                                ElseIf pc.ContainsColors And pc.ContainsNormals Then
                                    pc2.Add(thisi.Location, thisi.Normal, thisi.Color)
                                Else
                                    pc2.Add(thisi.Location)
                                End If
                            End If
                        Next
                End Select

                pc2.UserDictionary.Set(strdc, thisdictAfter)

        End Select

        DA.SetData(0, pc2)

    End Sub

End Class
