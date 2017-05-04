Imports System.Drawing
Imports System.Text.RegularExpressions
Imports Grasshopper.Kernel
Imports Rhino.Geometry

Public Class Dict_ExpressionDictionary
    Inherits GH_Component

    Sub New()
        MyBase.New("Cloud Expression", "ExpData", "Evaluate an expression and save results as user data.", "Volvox", "UserData")
    End Sub

    Public Overrides ReadOnly Property ComponentGuid As Guid
        Get
            Return GuidsRelease2.Comp_DictExpression
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    Protected Overrides ReadOnly Property Icon As Bitmap
        Get
            Return My.Resources.Icon_CloudExpression
        End Get
    End Property

    Protected Overrides Sub RegisterInputParams(pManager As GH_InputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Cloud to operate on", GH_ParamAccess.item)
        pManager.AddTextParameter("Key", "K", "Key", GH_ParamAccess.item)
        pManager.AddTextParameter("Expression", "E", "Expression to evaluate", GH_ParamAccess.item)
    End Sub

    Protected Overrides Sub RegisterOutputParams(pManager As GH_OutputParamManager)
        pManager.AddParameter(New Param_Cloud, "Cloud", "C", "Modified cloud", GH_ParamAccess.item)
    End Sub

    Dim ExpSolver As Math_EvalProvider = Nothing
    Dim GlobalCloud As PointCloud = Nothing
    Dim ProcCount As Integer = Environment.ProcessorCount
    Dim Results() As Double = Nothing
    Dim PrevEquation As String = String.Empty

    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        Dim strex As String = Nothing
        Dim strdc As String = Nothing
        Dim pc As PointCloud = Nothing

        If Not DA.GetData(0, pc) Then Return
        pc = pc.Duplicate
        GlobalCloud = pc
        ReDim Results(pc.Count - 1)

        If Not DA.GetData(1, strdc) Then Return
        If Not DA.GetData(2, strex) Then Return

        If strdc.Length < 1 Then Return
        If strex.Length < 1 Then Return
        strex = strex.ToLower

        If strex <> PrevEquation Then
            ExpSolver = Nothing
            ExpSolver = New Math_EvalProvider
            ExpSolver.Compile(strex)
            PrevEquation = strex
        End If

        Dim ThreadList As New List(Of Threading.Thread)

        For i As Integer = 0 To ProcCount - 1 Step 1
            Dim nt As New Threading.Thread(AddressOf EvalThread)
            nt.IsBackground = True
            ThreadList.Add(nt)
        Next

        For i As Integer = 0 To ProcCount - 1 Step 1
            ThreadList(i).Start(i)
        Next

        For Each t As Threading.Thread In ThreadList
            t.Join()
        Next

        ThreadList.Clear()

        GlobalCloud.UserDictionary.Set(strdc, Results)
        DA.SetData(0, GlobalCloud)
        ' GlobalCloud = Nothing
        Results = Nothing

    End Sub

    Sub EvalThread(MyIndex As Integer)

        Dim i0 As Integer = MyIndex * Math.Ceiling(GlobalCloud.Count / ProcCount)
        Dim i1 As Integer = Math.Min((MyIndex + 1) * Math.Ceiling(GlobalCloud.Count / ProcCount) - 1, GlobalCloud.Count - 1)

        Dim totc As Integer = GlobalCloud.Count

        For i As Integer = i0 To i1 Step 1
            Results(i) = ExpSolver.EvaluateExpression(GlobalCloud.Item(i))
        Next

    End Sub



End Class
