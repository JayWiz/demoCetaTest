Imports System.ComponentModel

Public Class Form1
    Private _oCeta815

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' MsgBox((ComboBox1.SelectedIndex + 1).ToString())
        _oCeta815.SendCommand(ComboBox1.SelectedIndex + 1)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ComboBox1.SelectedIndex = 0

        _oCeta815 = New Ceta815("COM1", 115200)
        _oCeta815.InitSerialConnection()
    End Sub
End Class
