<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ProgressForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.ProgSimple = New Volvox.ProgressSimple()
        Me.SuspendLayout()
        '
        'ProgSimple
        '
        Me.ProgSimple.BackColor = System.Drawing.Color.Transparent
        Me.ProgSimple.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ProgSimple.Location = New System.Drawing.Point(0, 0)
        Me.ProgSimple.Name = "ProgSimple"
        Me.ProgSimple.Percent = 0
        Me.ProgSimple.Size = New System.Drawing.Size(383, 42)
        Me.ProgSimple.TabIndex = 1
        '
        'ProgressForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(383, 42)
        Me.Controls.Add(Me.ProgSimple)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "ProgressForm"
        Me.Text = "Cloud Engine Progress"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ProgSimple As ProgressSimple
End Class
