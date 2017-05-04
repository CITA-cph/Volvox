Imports System.Drawing
Imports Grasshopper.Kernel
Imports Volvox_Instr
Imports Rhino.Geometry

Public Class Eng_BoxCrop
    Inherits GH_Component

    Sub New()
        MyBase.New("Box Crop", "BCrop", "Cull points outside of the box.", "Volvox", "Engine")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_BoxCrop
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_boxcrop
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
        pManager.AddBrepParameter("Box", "B", "Cropping box", GH_ParamAccess.list)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
        pManager.AddParameter(New Param_Instr)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cboxlist As New List(Of Brep)
        If Not DA.GetDataList(0, cboxlist) Then Return

        Dim outlist As New List(Of Box)
        For Each cbox As Brep In cboxlist

            Dim bf As BrepFace = cbox.Faces(0)

            Dim thisplane As New Plane
            If Not bf.TryGetPlane(thisplane, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) Then
                Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Are you sure it's a box ?")
                Exit Sub
            End If

            If cbox.DuplicateVertices.Count <> 8 Then
                Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Are you sure it's a box ?")
                Exit Sub
            End If

            outlist.Add(New Box(thisplane, cbox))

        Next

        DA.SetData(0, New Instr_BoxCrop(outlist))

    End Sub

End Class
