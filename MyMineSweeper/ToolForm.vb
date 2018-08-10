Public Class ToolForm
    Private Sub ToolForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HScrollBar1.Value = Form1.mw
        HScrollBar2.Value = Form1.mh
        HScrollBar3.Maximum = Form1.mw * Form1.mh
        HScrollBar3.Value = Form1.mn

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Form1.Myinit()
    End Sub

    Private Sub HScrollBar3_ValueChanged(sender As Object, e As EventArgs) Handles HScrollBar3.ValueChanged
        Label5.Text = HScrollBar3.Value
        Form1.mn = HScrollBar3.Value
    End Sub

    Private Sub HScrollBar2_ValueChanged(sender As Object, e As EventArgs) Handles HScrollBar2.ValueChanged
        Label4.Text = HScrollBar2.Value
        HScrollBar3.Maximum = HScrollBar1.Value * HScrollBar2.Value
        Form1.mh = HScrollBar2.Value
    End Sub

    Private Sub HScrollBar1_ValueChanged(sender As Object, e As EventArgs) Handles HScrollBar1.ValueChanged
        Label3.Text = HScrollBar1.Value
        HScrollBar3.Maximum = HScrollBar1.Value * HScrollBar2.Value
        Form1.mw = HScrollBar1.Value
    End Sub
End Class