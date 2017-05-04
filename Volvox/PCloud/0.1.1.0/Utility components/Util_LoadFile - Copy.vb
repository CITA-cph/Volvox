Imports System.Drawing
Imports System.IO
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Util_LoadFile
    Inherits GH_Component

    Dim Rnd As New Random

    Sub New()
        MyBase.New("Load Cloud", "CloudLoad", "Loads point cloud from file.", "Volvox", "Util")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_LoadFile
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Load
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Grasshopper.Kernel.Parameters.Param_FilePath, "File path", "F", "File path", GH_ParamAccess.item)
        pManager.AddTextParameter("Mask", "M", "Mask to apply", GH_ParamAccess.item)
        pManager.AddNumberParameter("Percent", "%", "Percent of points to load, 0 to 1 range", GH_ParamAccess.item, 1)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim fpath As String = String.Empty
        Dim fmask As String = String.Empty
        Dim perc As Double = 1

        If Not (DA.GetData(0, fpath)) Then Return
        If Not (DA.GetData(1, fmask)) Then Return
        If Not (DA.GetData(2, perc)) Then Return

        If Not File.Exists(fpath) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File doesn't exist.")
            Exit Sub
        End If

        If String.IsNullOrEmpty(fpath) Or String.IsNullOrEmpty(fmask) Then Return

        Dim pc As New PointCloud
        Dim parser As New Parse_Load(fmask)

        Using str As StreamReader = New StreamReader(fpath)

            Do While str.Peek() >= 0

                Dim thisline As String = str.ReadLine

                Select Case Rnd.NextDouble() + perc
                    Case Is > 1

                        Dim p As Multipoint = parser.TextToMultipoint(thisline)

                        pc.AppendNew()
                        pc(pc.Count - 1).Location = New Point3d(p.X, p.Y, p.Z)
                        If p.ContainsNormals Then pc(pc.Count - 1).Normal = New Vector3d(p.U, p.V, p.W)
                        If p.ContainsIntensity Then
                            If p.ContainsColors Then
                                Dim intens As Double = p.A / 255
                                pc(pc.Count - 1).Color = Color.FromArgb(p.R * intens, p.G * intens, p.B * intens)
                            Else
                                pc(pc.Count - 1).Color = Color.FromArgb(p.A, p.A, p.A)
                            End If
                        Else
                            If p.ContainsColors Then pc(pc.Count - 1).Color = Color.FromArgb(p.R, p.G, p.B)
                        End If

                End Select

            Loop

        End Using

        DA.SetData(0, pc)

    End Sub

End Class