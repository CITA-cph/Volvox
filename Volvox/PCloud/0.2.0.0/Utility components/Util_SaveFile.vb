Imports System.Drawing
Imports System.IO
Imports System.Threading
Imports Grasshopper.Kernel
Imports Volvox_Cloud

Public Class Util_SaveFile
    Inherits GH_Component

    Sub New()

        MyBase.New("Save .xyz", "xyzSave", "Save cloud to .xyz file.", "Volvox", "I/O")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return Comp_IOSaveXYZ
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quinary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_Save
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to deconstruct", GH_ParamAccess.item)
        pManager.AddParameter(New Param_SaveFile)
        pManager.AddTextParameter("Mask", "M", "Mask to apply", GH_ParamAccess.item, "x y z")
        pManager.AddNumberParameter("Precision", "P", "Precision", GH_ParamAccess.item)
        pManager.AddBooleanParameter("Double-quotes", "D", "Surround values with double-quotes", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Run", "R", "Run", GH_ParamAccess.item, False)
        pManager.AddBooleanParameter("Abort", "A", "Abort", GH_ParamAccess.item, False)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
    End Sub

    Dim running As Boolean = False
    Dim GlobCloud As GH_Cloud = Nothing
    Dim FileMask As String = Nothing
    Dim FilePath As String = Nothing
    Dim FilePrec As Double = 1
    Dim DoubleQuote As Boolean = False
    Dim MyThread As Threading.Thread = Nothing
    Dim finished As Boolean = False

    Dim GlobMessage As String = New String("")

    Dim st As New System.Diagnostics.Stopwatch

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim pc As GH_Cloud = Nothing
        Dim fpath As String = String.Empty
        Dim fmask As String = String.Empty
        Dim prec As Double = 1
        Dim dq As Boolean = False

        Dim r As Boolean = False
        Dim a As Boolean = False

        If Not (DA.GetData(0, pc)) Then Return
        If Not (DA.GetData(1, fpath)) Then Return
        If Not (DA.GetData(2, fmask)) Then Return
        If Not (DA.GetData(3, prec)) Then Return
        If Not (DA.GetData(4, dq)) Then Return

        If Not (DA.GetData(5, r)) Then Return
        If Not (DA.GetData(6, a)) Then Return

        fpath = RemoveIllegal(fpath)

        If Not File.Exists(fpath) Then File.Create(fpath).Dispose()

        If r And (Not a) And (Not running) Then
            st.Start()
            FileMask = fmask
            FilePath = fpath
            FilePrec = prec
            DoubleQuote = dq
            GlobCloud = pc
            running = True
            finished = False
            MyThread = New Threading.Thread(AddressOf SavingFile)
            MyThread.Start()
        End If

        If a Then
            running = False
            finished = True
            If MyThread IsNot Nothing Then
                MyThread.Abort()
                MyThread = Nothing
            End If
            Me.Message = "Aborted"
            GlobCloud = Nothing
            st.Reset()
        End If

        If finished Then
            st.Reset()
            running = False
            MyThread = Nothing
            GlobCloud = Nothing
        End If

    End Sub

    Sub SavingFile()

        Dim nparse As New Parse_Save(FileMask, FilePrec, DoubleQuote)

        Dim totc As Double = 1 / GlobCloud.Value.Count
        Dim LastPercentReported As Integer

        Thread.Sleep(10)

        If Utils_IO.IsFileLocked(New FileInfo(FilePath)) Then
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File is in use by another process")
            Me.Message = "File in use"
            finished = True
            Exit Sub
        End If

        Try

            Using w As StreamWriter = New StreamWriter(FilePath)

                For i As Integer = 0 To GlobCloud.Value.Count - 1 Step 1

                    If LastPercentReported < ((i * totc) * 100) Then
                        LastPercentReported = Math.Ceiling((i * totc) * 100)
                        GlobMessage = LastPercentReported
                        InvokeDraw()
                    End If

                    w.WriteLine(nparse.PCloudItemToText(GlobCloud.Value, i))

                Next

            End Using

            finished = True
            GlobMessage = "Complete"
            InvokeDraw()

            Dim dlg As Action = AddressOf ExpireComponent
            Rhino.RhinoApp.MainApplicationWindow.Invoke(dlg)

        Catch ex As Exception
            Me.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message)
        End Try

    End Sub

    Private Sub ExpireComponent()
        Me.ExpireSolution(True)
    End Sub

    Friend Sub InvokeDraw()
        If Rhino.RhinoApp.MainApplicationWindow.InvokeRequired Then Rhino.RhinoApp.MainApplicationWindow.Invoke(DrawMessage)
    End Sub

    Dim DrawMessage As Action = AddressOf PaintAmountMessage

    Private Sub PaintAmountMessage()
        If GlobMessage = "Complete" Then
            Me.Message = st.ElapsedMilliseconds & " ms"
        Else
            Me.Message = GlobMessage & "%"
        End If

        Me.OnDisplayExpired(False)
    End Sub

End Class
