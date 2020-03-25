Public Class Form1
    Private _oCeta815 As Ceta815

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If _oCeta815.ExecuteTest() Then
            tbDifferentialPressure.Text = _oCeta815.DifferentialPressure
            tbVolumeRatio.Text = _oCeta815.VolumeRatio
            tbResult.Text = _oCeta815.Result
            If _oCeta815.Result = "PASS" Then
                tbResult.BackColor = Color.LightGreen
            Else
                tbResult.BackColor = Color.Red
            End If
            Debug.WriteLine("ExecuteTest succeeded")
        Else
            Debug.WriteLine("ExecuteTest failed")
        End IF
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _oCeta815 = New Ceta815("COM1", 115200)
        If Not _oCeta815.Init() Then
            MsgBox("Initialization of Ceta815 failed!")
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        _oCeta815.Init()
    End Sub
End Class
