Imports System.IO
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Parameters
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry

Public Class RdfTransformation
    Inherits GH_Component

    Sub New()
        MyBase.New("RdfTransform", "RdfTransform", "Get transformation matrices from an .rdf file", "Volvox", "Diff")
    End Sub
    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return New Guid("cbf61ec3-e4a0-42ae-95ea-eed2d32dcb97")
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_FilePath, "Rdf", "R", "Rdf transformation file", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddTransformParameter("Cloud Transformation", "Tc", "Cloud transformation matrix", GH_ParamAccess.item)
        pManager.AddTransformParameter("Ifc Transformation", "Ti", "Ifc transformation matrix", GH_ParamAccess.item)
        pManager.AddTextParameter("Cloud GUID", "Gc", "Cloud GUID", GH_ParamAccess.item)
        pManager.AddTextParameter("Ifc GUID", "Gi", "Ifc GUID", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim cloudMatrix As GH_Transform = Nothing
        Dim ifcMatrix As GH_Transform = Nothing
        Dim rdf As String = Nothing

        If Not DA.GetData(0, rdf) Then Return
        Dim pcToIfc As Boolean = True

        If rdf Is Nothing Then
            cloudMatrix = New GH_Transform(Rhino.Geometry.Transform.Identity)
            ifcMatrix = New GH_Transform(Rhino.Geometry.Transform.Identity)
            Return
        End If

        If Not File.Exists(rdf) Then
            cloudMatrix = New GH_Transform(Rhino.Geometry.Transform.Identity)
            ifcMatrix = New GH_Transform(Rhino.Geometry.Transform.Identity)
            Return
        End If

        Dim mtr As New Rhino.Geometry.Transform
        Dim ifcg As String = String.Empty
        Dim pcg As String = String.Empty

        Using s As StreamReader = New StreamReader(rdf)

            Dim currType As String = String.Empty

            Dim nameA As String = String.Empty
            Dim nameB As String = String.Empty

            While Not s.EndOfStream
                Dim str As String = s.ReadLine

                If str <> String.Empty Then
                    If str(0) = "@" Then Continue While

                    If Not str.Contains(":") Then
                        If nameA <> String.Empty Then nameB = str
                        If nameA = String.Empty Then nameA = str
                    End If
                End If
            End While

            s.BaseStream.Position = 0

            While Not s.EndOfStream
                Dim str As String = s.ReadLine
                If str <> String.Empty Then

                    If str.Contains("<rel:transformParam>") Then
                        Dim mat() As String = str.Split(Chr(34))(1).Split(" ")
                        Dim counter As Integer = 0
                        For i As Integer = 0 To 3 Step 1
                            For j As Integer = 0 To 3 Step 1
                                mtr(i, j) = mat(counter)
                                counter += 1
                            Next
                        Next
                    End If

                    If str.Contains("<rel:corrSource>") Then
                        If str.Contains(nameB) Then
                            pcToIfc = True
                        Else
                            pcToIfc = False
                        End If
                    End If

                    If str.Contains(nameB) And Not str.Contains(nameA) Then
                        currType = "PC"
                    ElseIf (str.Contains(namea)) And (Not str.Contains(nameb)) Then
                        currType = "IFC"
                    End If

                    If str.Contains("<rel:globalUniqueId>") Then
                        Select Case currType
                            Case "PC"
                                pcg = str.Split(Chr(34))(1).Trim("{").Trim("}")
                            Case "IFC"
                                ifcg = str.Split(Chr(34))(1).Trim("{").Trim("}")
                        End Select
                    End If
                End If
            End While

        End Using


        If pcToIfc Then
            DA.SetData(0, mtr)
            DA.SetData(1, Transform.Identity)

            '  cloudMatrix = New GH_Transform(mtr)
            ' ifcMatrix = New GH_Transform(Transform.Identity)
        Else
            DA.SetData(0, Transform.Identity)
            DA.SetData(1, mtr)

            'ifcMatrix = New GH_Transform(mtr)
            ' cloudMatrix = New GH_Transform(Transform.Identity)
        End If


        DA.SetData(2, pcg)
        DA.SetData(3, ifcg)


    End Sub
End Class
