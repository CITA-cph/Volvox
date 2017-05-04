<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Control_Display
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.DynamicBut = New System.Windows.Forms.Button()
        Me.MinusBut = New System.Windows.Forms.Button()
        Me.PlusBut = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'DynamicBut
        '
        Me.DynamicBut.BackColor = System.Drawing.Color.White
        Me.DynamicBut.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.DynamicBut.FlatAppearance.CheckedBackColor = System.Drawing.Color.White
        Me.DynamicBut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.DynamicBut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.DynamicBut.Location = New System.Drawing.Point(0, 1)
        Me.DynamicBut.Name = "DynamicBut"
        Me.DynamicBut.Size = New System.Drawing.Size(120, 28)
        Me.DynamicBut.TabIndex = 0
        Me.DynamicBut.Text = "Dynamic display"
        Me.DynamicBut.UseVisualStyleBackColor = False
        '
        'MinusBut
        '
        Me.MinusBut.BackColor = System.Drawing.Color.White
        Me.MinusBut.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.MinusBut.FlatAppearance.CheckedBackColor = System.Drawing.Color.White
        Me.MinusBut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.MinusBut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.MinusBut.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MinusBut.Location = New System.Drawing.Point(123, 1)
        Me.MinusBut.Name = "MinusBut"
        Me.MinusBut.Size = New System.Drawing.Size(28, 28)
        Me.MinusBut.TabIndex = 1
        Me.MinusBut.Text = "-"
        Me.MinusBut.UseVisualStyleBackColor = False
        '
        'PlusBut
        '
        Me.PlusBut.BackColor = System.Drawing.Color.White
        Me.PlusBut.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(189, Byte), Integer))
        Me.PlusBut.FlatAppearance.CheckedBackColor = System.Drawing.Color.White
        Me.PlusBut.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.PlusBut.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.PlusBut.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PlusBut.Location = New System.Drawing.Point(150, 1)
        Me.PlusBut.Name = "PlusBut"
        Me.PlusBut.Size = New System.Drawing.Size(28, 28)
        Me.PlusBut.TabIndex = 2
        Me.PlusBut.Text = "+"
        Me.PlusBut.UseVisualStyleBackColor = False
        '
        'DisplayControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.PlusBut)
        Me.Controls.Add(Me.MinusBut)
        Me.Controls.Add(Me.DynamicBut)
        Me.Name = "DisplayControl"
        Me.Size = New System.Drawing.Size(180, 30)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents DynamicBut As Windows.Forms.Button
    Friend WithEvents MinusBut As Windows.Forms.Button
    Friend WithEvents PlusBut As Windows.Forms.Button
End Class
