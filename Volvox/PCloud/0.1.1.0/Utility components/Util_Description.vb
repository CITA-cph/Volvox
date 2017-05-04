Imports System.Drawing
Imports GH_IO.Serialization
Imports Grasshopper
Imports Grasshopper.Kernel

Public Class Util_Description
    Inherits GH_Component

    Sub New()
        MyBase.New("About", "About", "About Volvox", "Volvox", "Volvox")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease1.Comp_Description
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Volvox
        End Get
    End Property

    Public Overrides Sub CreateAttributes()
        MyBase.Attributes = New Att_Description(Me)
    End Sub

    Public Overrides Function Write(writer As GH_IWriter) As Boolean
        writer.SetSingle("PivotX", Me.Attributes.Pivot.X)
        writer.SetSingle("PivotY", Me.Attributes.Pivot.Y)
        Return MyBase.Write(writer)
    End Function

    Public Overrides Function Read(reader As GH_IReader) As Boolean
        Me.Attributes.Pivot = New PointF(reader.GetSingle("PivotX"), reader.GetSingle("PivotY"))
        Return MyBase.Read(reader)
    End Function

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
    End Sub

End Class
