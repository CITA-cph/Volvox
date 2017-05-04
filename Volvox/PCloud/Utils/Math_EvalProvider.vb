Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text
Imports Rhino.Geometry

Public Class Math_EvalProvider

    Dim mi As MethodInfo
    Dim o As Object

    Public Function Compile(Expression As String) As Boolean

        Dim cdd As CodeDomProvider = CodeDomProvider.CreateProvider("VisualBasic")

        Dim cp As CompilerParameters = New CompilerParameters

        cp.ReferencedAssemblies.Add("System.dll")
        cp.CompilerOptions = "/t:library"
        cp.GenerateInMemory = True

        Dim sb As StringBuilder = New StringBuilder("")

        sb.Append("Imports System" & vbCrLf)
        sb.Append("Class ExpressionSolver" & vbCrLf)
        sb.Append("Public Function EvaluateExpression(x as double, y as double, z as double, r as double, g as double, b as double, a as double, u as double, v as double, w as double) as double" & vbCrLf)
        sb.Append("Return " & CorrectExpression(Expression) & vbCrLf)
        sb.Append("End Function " & vbCrLf)
        sb.Append("End Class " & vbCrLf)

        Dim cr As CompilerResults = cdd.CompileAssemblyFromSource(cp, sb.ToString())

        For Each ce As CompilerError In cr.Errors
            MsgBox(ce.ToString, MsgBoxStyle.OkOnly, "Expression parser error")
        Next

        Dim a As System.Reflection.Assembly = cr.CompiledAssembly

        o = a.CreateInstance("ExpressionSolver")
        Dim t As Type = o.GetType()
        mi = t.GetMethod("EvaluateExpression")

        Return True

    End Function


    Public Function CompileEx(Expression As String) As List(Of String)

        Dim cdd As CodeDomProvider = CodeDomProvider.CreateProvider("VisualBasic")

        Dim cp As CompilerParameters = New CompilerParameters

        cp.ReferencedAssemblies.Add("System.dll")
        cp.CompilerOptions = "/t:library"
        cp.GenerateInMemory = True

        Dim sb As StringBuilder = New StringBuilder("")

        sb.Append("Imports System" & vbCrLf)
        sb.Append("Class Solver" & vbCrLf)
        sb.Append("Public Function Evaluate(x as Double, y as Double, z as Double, _" & vbCrLf & "r as Double, g as Double, b as Double, a as Double, _" & vbCrLf & "u as Double, v as Double, w as Double) as Double" & vbCrLf & vbCrLf)
        sb.Append(Expression & vbCrLf & vbCrLf)
        sb.Append("End Function " & vbCrLf)
        sb.Append("End Class ")

        Dim cr As CompilerResults = cdd.CompileAssemblyFromSource(cp, sb.ToString())

        Dim er As New List(Of String)

        For Each ce As CompilerError In cr.Errors
            er.Add(ce.ToString)
        Next

        er.Add(sb.ToString)

        Try
            Dim a As System.Reflection.Assembly = cr.CompiledAssembly
            o = a.CreateInstance("Solver")
            Dim t As Type = o.GetType()
            mi = t.GetMethod("Evaluate")
        Catch ex As Exception
            er.Add(ex.Message)
        End Try

        Return er

    End Function


    Public Function EvaluateExpression(p As PointCloudItem) As Double

        Dim params(9) As Object
        params(0) = p.Location.X
        params(1) = p.Location.Y
        params(2) = p.Location.Z
        params(3) = p.Color.R
        params(4) = p.Color.G
        params(5) = p.Color.B
        params(6) = p.Color.A
        params(7) = p.Normal.X
        params(8) = p.Normal.Y
        params(9) = p.Normal.Z

        Dim result As Object = mi.Invoke(o, params)

        Return (DirectCast(result, Double))

    End Function

    Function CorrectExpression(Expression As String) As String

        Dim repl As New List(Of String)(StuffToReplace)

        For Each s As String In repl
            Dim pattern As String = "(^|\b)" & s & "(\b|$)"
            Dim replacement As String = "math." & s
            Dim rgx As New System.Text.RegularExpressions.Regex(pattern)
            Expression = rgx.Replace(Expression, replacement)
        Next

        Return Expression

    End Function

    Function StuffToReplace() As List(Of String)
        Dim ns As New List(Of String)

        ns.Add("abs")
        ns.Add("acos")
        ns.Add("asin")
        ns.Add("atan")
        ns.Add("atan2")
        ns.Add("bigmul")
        ns.Add("ceiling")
        ns.Add("cos")
        ns.Add("cosh")
        ns.Add("divrem")
        ns.Add("e")
        ns.Add("exp")
        ns.Add("floor")
        ns.Add("ieeeremainder")
        ns.Add("log")
        ns.Add("log10")
        ns.Add("max")
        ns.Add("min")
        ns.Add("pi")
        ns.Add("pow")
        ns.Add("round")
        ns.Add("sign")
        ns.Add("sin")
        ns.Add("sinh")
        ns.Add("sqrt")
        ns.Add("tan")
        ns.Add("tanh")
        ns.Add("truncate")

        Return ns
    End Function


End Class