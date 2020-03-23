<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.tbResult = New System.Windows.Forms.TextBox()
        Me.tbDifferentialPressure = New System.Windows.Forms.TextBox()
        Me.tbVolumeRatio = New System.Windows.Forms.TextBox()
        Me.SuspendLayout
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(241, 173)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(95, 23)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "ExecuteTest"
        Me.Button1.UseVisualStyleBackColor = true
        '
        'tbResult
        '
        Me.tbResult.Enabled = false
        Me.tbResult.Location = New System.Drawing.Point(241, 202)
        Me.tbResult.Name = "tbResult"
        Me.tbResult.Size = New System.Drawing.Size(95, 20)
        Me.tbResult.TabIndex = 1
        '
        'tbDifferentialPressure
        '
        Me.tbDifferentialPressure.Enabled = false
        Me.tbDifferentialPressure.Location = New System.Drawing.Point(241, 228)
        Me.tbDifferentialPressure.Name = "tbDifferentialPressure"
        Me.tbDifferentialPressure.Size = New System.Drawing.Size(95, 20)
        Me.tbDifferentialPressure.TabIndex = 2
        '
        'tbVolumeRatio
        '
        Me.tbVolumeRatio.Enabled = false
        Me.tbVolumeRatio.Location = New System.Drawing.Point(241, 254)
        Me.tbVolumeRatio.Name = "tbVolumeRatio"
        Me.tbVolumeRatio.Size = New System.Drawing.Size(95, 20)
        Me.tbVolumeRatio.TabIndex = 3
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.tbVolumeRatio)
        Me.Controls.Add(Me.tbDifferentialPressure)
        Me.Controls.Add(Me.tbResult)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents tbResult As TextBox
    Friend WithEvents tbDifferentialPressure As TextBox
    Friend WithEvents tbVolumeRatio As TextBox
End Class
