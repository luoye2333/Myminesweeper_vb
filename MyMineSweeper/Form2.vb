Imports System.Drawing
Public Class Form2

    Private Sub Form2_Resize(sender As Object, e As EventArgs) Handles Me.Resize

        Me.Height = Me.Width

        Dim g As Graphics = Me.CreateGraphics
        g.Clear(Color.White)
        Dim sf As StringFormat = New StringFormat With {
        .Alignment = StringAlignment.Center,
        .LineAlignment = StringAlignment.Center}

        g.DrawString("1", New Font("宋体", Me.Width - 100, 0, GraphicsUnit.Pixel), Brushes.Black, ClientRectangle, sf)
    End Sub

End Class