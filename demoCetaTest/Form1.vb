Imports System.ComponentModel

Public Class Form1
    Private _oCeta815 As Ceta815

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If _oCeta815.ExecuteTest() Then
            tbDifferentialPressure.Text = _oCeta815.DifferentialPressure
            tbVolumeRatio.Text = _oCeta815.VolumeRatio
            tbResult.Text = _oCeta815.Result
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _oCeta815 = New Ceta815("COM1", 115200)
        If Not _oCeta815.Init() Then
            MsgBox("Initialization of Ceta815 failed!")
        End If
    End Sub
End Class
