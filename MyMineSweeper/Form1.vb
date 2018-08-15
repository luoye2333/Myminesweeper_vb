Imports System.Drawing
Imports System.Threading
Public Class Form1
    Friend mw, mh, mn As Integer

    Dim map(,) As Byte '计数
    Dim kmap(,) As Byte '是否揭开,有无标记
    Dim umap(,) As Boolean '有无更新，用于paint
    Dim firstclick As Boolean
    Dim mbmp, showbmp As Bitmap
    Dim lmb, rmb As Boolean

    Dim f2 As ToolForm
    Dim mremain As Integer
    Dim mtime As DateTime
    Dim maincolor, secondcolor As Color
    Dim b1, b2 As Brush
    Dim sf As StringFormat
    Dim perx, pery As Integer
    Dim die_when_wrong As Boolean = True



    Enum MapStatus
        empty = 0
        mine = 10
    End Enum
    Enum KnowMapStatus
        unknown = 0
        marked = 1
        opened = 2
    End Enum

    'attention:2个map均采用python的先行后列储存。即先索引第几行，后索引第几列
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '主色
        maincolor = Color.Black
        Dim r, g, b As Byte
        r = 255 - maincolor.R
        g = 255 - maincolor.G
        b = 255 - maincolor.B
        secondcolor = Color.FromArgb(maincolor.A, r, g, b)
        b1 = New SolidBrush(maincolor)
        b2 = New SolidBrush(secondcolor)
        sf = New StringFormat With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        perx = 40 : pery = 40 '绘图精度
        mw = 30 : mh = 16 : mn = 99
        f2 = New ToolForm
        Myinit()
    End Sub
    Private Function GetBlock(clickx As Integer, clicky As Integer) As Point
        Dim cr As Size = ClientSize
        Dim s As Point
        s.X = Int(clickx / cr.Width * mw) + 1
        s.Y = Int(clicky / cr.Height * mh) + 1
        Return s
    End Function
    Private Sub GenerateMap(firstclickblock As Point, minenumber As Integer)
        Dim count As Integer = 0
        Randomize()
        Dim r As Random = New Random
        While count < minenumber
            Dim gx, gy As Integer
            gx = r.Next(mw) + 1
            gy = r.Next(mh) + 1
            With firstclickblock
                If (gx >= .X - 1) AndAlso (gx <= .X + 1) AndAlso
                (gy >= .Y - 1) AndAlso (gy <= .Y + 1) Then Continue While
                '保证不在初始一圈内
            End With
            If map(gy, gx) = 10 Then Continue While
            '不重复
            map(gy, gx) = 10
            For px = gx - 1 To gx + 1
                For py = gy - 1 To gy + 1
                    If (px < 1) OrElse (px > mw) OrElse (py < 1) OrElse (py > mh) Then Continue For
                    '防止超出边界
                    If (px = gx) And (py = gy) Then Continue For
                    If map(py, px) = 10 Then Continue For
                    map(py, px) += 1
                Next py
            Next px
            '创建数字

            count += 1
        End While
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        mtime = mtime.AddSeconds(1)
        f2.Label1.Text = mtime.ToString("mm:ss")
    End Sub
    Private Sub ChainOpen(ByVal BlocksToOpen() As Point)
        Dim pn As Integer
        Dim bto() As Point
        Dim searched(,) As Boolean
        ReDim searched(mh, mw)
        For Each p In BlocksToOpen
            umap(p.Y, p.X) = True
            kmap(p.Y, p.X) = KnowMapStatus.opened
            If map(p.Y, p.X) = MapStatus.mine Then
                Fail(p.X, p.Y)
                Exit Sub
            End If
            If map(p.Y, p.X) = MapStatus.empty Then
                With p
                    For px = .X - 1 To .X + 1
                        For py = .Y - 1 To .Y + 1
                            If (px < 1) OrElse (px > mw) OrElse (py < 1) OrElse (py > mh) Then Continue For
                            If searched(py, px) Then Continue For
                            If (px = .X) And (py = .Y) Then Continue For
                            If kmap(py, px) = KnowMapStatus.unknown Then
                                searched(py, px) = True
                                Select Case map(py, px)
                                    Case MapStatus.empty
                                        pn += 1
                                        ReDim Preserve bto(pn - 1)
                                        bto(pn - 1) = New Point(px, py)
                                    Case Else
                                        umap(py, px) = True
                                        kmap(py, px) = KnowMapStatus.opened
                                End Select
                            End If
                        Next py
                    Next px
                End With
            End If
        Next p
        Draw()
        If pn > 0 Then
            ChainOpen(bto)
        End If
    End Sub
    Friend Sub Myinit()
        firstclick = True
        mtime = #00:00:00#
        mremain = mn
        Me.ClientSize = New Size(mw * 40, mh * 40)
        With f2
            .Visible = True
            .Left = Me.Left + Me.Width
            .Top = Me.Top
            .Height = Me.Height
            .BackColor = maincolor
            .Label2.Text = mremain
            .Label1.Text = mtime.ToString("mm:ss")
        End With
        mbmp = New Bitmap(mw * perx, mh * pery)
        ReDim map(mh, mw)
        ReDim umap(mh, mw)
        ReDim kmap(mh, mw)
        For i = 1 To mw
            For j = 1 To mh
                map(j, i) = MapStatus.empty
                umap(j, i) = True
                kmap(j, i) = KnowMapStatus.unknown
            Next j
        Next i
        '画格子线
        Dim gr As Graphics = Graphics.FromImage(mbmp)
        Dim pw As Single = mbmp.Width / mw
        Dim ph As Single = mbmp.Height / mh
        For i = 0 To mw - 1
            Dim x As Integer
            x = Int(0 + i * pw)
            gr.DrawLine(New Pen(maincolor, 1), x, 0, x, 0 + mbmp.Height)
        Next i
        For j = 0 To mh - 1
            Dim y As Integer
            y = Int(0 + j * ph)
            gr.DrawLine(New Pen(maincolor, 1), 0, y, 0 + mbmp.Width, y)
        Next j
        Draw()
        Sweepinit()
    End Sub
    Private Sub Draw()
        Dim g As Graphics = Graphics.FromImage(mbmp)
        For x = 1 To mw
            For y = 1 To mh
                If umap(y, x) Then
                    umap(y, x) = False
                    'if updated then repaint
                    Dim ax, ay As UInteger
                    ax = Int((x - 1) * perx)
                    ay = Int((y - 1) * pery)
                    Dim rect As Rectangle = New Rectangle(ax + 1, ay + 1, Int(perx) - 1, Int(pery) - 1)
                    Select Case kmap(y, x)
                        Case KnowMapStatus.unknown
                            g.FillRectangle(b1, rect)
                        Case KnowMapStatus.marked
                            g.FillRectangle(b1, rect)
                            g.DrawString("P", New Font("宋体", perx, 0, GraphicsUnit.Pixel), b2, rect, sf)
                        Case KnowMapStatus.opened
                            Select Case map(y, x)
                                Case MapStatus.empty
                                    'empty
                                    g.FillRectangle(b2, rect)
                                Case MapStatus.mine
                                    'mine
                                    g.FillRectangle(b2, rect)
                                    g.FillEllipse(b1, rect)
                                Case Else
                                    g.FillRectangle(b2, rect)
                                    g.DrawString(CStr(map(y, x)), New Font("宋体", perx, 0, GraphicsUnit.Pixel), b1, rect, sf)
                            End Select
                    End Select
                End If
            Next y
        Next x
        g.Flush()
        ShowPic()
        Refresh()
    End Sub
    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        If e.Button = MouseButtons.Left Then lmb = True
        If e.Button = MouseButtons.Right Then rmb = True
    End Sub
    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
        Dim cp As Point = GetBlock(e.X, e.Y)
        Dim ans As Byte = map(cp.Y, cp.X)
        Dim kans As Byte = kmap(cp.Y, cp.X)

        If (firstclick) AndAlso (lmb) Then
            firstclick = False
            GenerateMap(cp, mn)
            Timer1.Enabled = True
        End If

        Select Case kans
            Case KnowMapStatus.unknown
                If Not (lmb And rmb) Then
                    If lmb Then
                        Dim bto(0) As Point
                        bto(0) = cp
                        ChainOpen(bto)
                    ElseIf rmb Then
                        'mark
                        mremain -= 1
                        f2.Label2.Text = mremain
                        umap(cp.Y, cp.X) = True
                        kmap(cp.Y, cp.X) = KnowMapStatus.marked
                        Draw()
                        If mremain = 0 Then WinCheck()
                    End If
                End If
            Case KnowMapStatus.marked
                If rmb Then
                    'demark
                    mremain += 1
                    f2.Label2.Text = mremain
                    umap(cp.Y, cp.X) = True
                    kmap(cp.Y, cp.X) = KnowMapStatus.unknown
                    Draw()
                End If
            Case KnowMapStatus.opened
                If (lmb) AndAlso (rmb) Then
                    Dim mines, unknowns As Byte
                    mines = 0 : unknowns = 0
                    Dim bto() As Point
                    With cp
                        For px = .X - 1 To .X + 1
                            For py = .Y - 1 To .Y + 1
                                If (px < 1) OrElse (px > mw) OrElse (py < 1) OrElse (py > mh) Then Continue For
                                If (px = .X) And (py = .Y) Then Continue For
                                Select Case kmap(py, px)
                                    Case KnowMapStatus.marked
                                        mines += 1
                                    Case KnowMapStatus.unknown
                                        unknowns += 1
                                        ReDim Preserve bto(unknowns - 1)
                                        bto(unknowns - 1) = New Point(px, py)
                                End Select
                            Next py
                        Next px
                    End With
                    If (unknowns > 0) AndAlso ans = mines Then
                        ChainOpen(bto)
                    End If
                End If
        End Select
        lmb = False : rmb = False
    End Sub
    Private Sub Fail(fx As Integer, fy As Integer)
        Dim g As Graphics = Graphics.FromImage(mbmp)
        Dim ax, ay As UInteger
        Dim mpen As Pen = New Pen(Color.Red, perx / 20)
        If die_when_wrong Then
            For i = 1 To mw
                For j = 1 To mh
                    If (kmap(j, i) = KnowMapStatus.marked) Then
                        If (map(j, i) <> MapStatus.mine) Then
                            '标记错误
                            ax = Int((i - 1) * perx)
                            ay = Int((j - 1) * pery)
                            g.DrawLine(mpen, ax + 1, ay + 1, ax + perx - 1, ay + pery - 1)
                            g.DrawLine(mpen, ax + 1, ay + pery - 1, ax + perx - 1, ay + 1)
                        End If
                    Else
                        umap(j, i) = True
                    End If
                    '全部打开，之后对操作没有响应
                    kmap(j, i) = KnowMapStatus.opened
                Next j
            Next i
        End If
        Draw()
        ax = Int((fx - 1) * perx)
        ay = Int((fy - 1) * pery)
        g.DrawLine(mpen, ax + 1, ay + 1, ax + perx - 1, ay + pery - 1)
        g.DrawLine(mpen, ax + 1, ay + pery - 1, ax + perx - 1, ay + 1)
        g.Flush()
        ShowPic()
        Timer1.Enabled = False
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If (mbmp IsNot Nothing) And (Me.WindowState <> FormWindowState.Minimized) Then
            ShowPic()
        End If

        If f2 IsNot Nothing Then
            With f2
                .Left = Me.Left + Me.Width
                .Top = Me.Top
                .Height = Me.Height
            End With
        End If

    End Sub
    Private Sub Form1_Move(sender As Object, e As EventArgs) Handles Me.Move
        If f2 IsNot Nothing Then
            With f2
                .Left = Me.Left + Me.Width
                .Top = Me.Top
                .Height = Me.Height
            End With
        End If

    End Sub
    Private Sub WinCheck()
        For i = 1 To mw
            For j = 1 To mh
                If kmap(j, i) = KnowMapStatus.marked AndAlso map(j, i) <> MapStatus.mine Then
                    Exit Sub
                End If
            Next j
        Next i
        Timer1.Enabled = False
        Dim g As Graphics = Graphics.FromImage(mbmp)
        g.DrawString("Win!!!", New Font("Consolas", Me.Height / 3, 0, GraphicsUnit.Pixel), b1, ClientRectangle, sf)
        ShowPic()
    End Sub
    Private Sub ShowPic()
        showbmp = New Bitmap(mbmp, ClientSize.Width, ClientSize.Height)
        PictureBox1.Location = New Point(0, 0)
        PictureBox1.Size = ClientSize
        PictureBox1.Image = showbmp
    End Sub
    Dim atsw As Thread
    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.P Then
            For j = 1 To mh
                For i = 1 To mw
                    updated(j, i) = True
                Next i
            Next j
            atsw = New Thread(AddressOf AutoSweep)
            atsw.Start()
        End If
    End Sub
    Dim swmap(,) As Byte
    Dim checked(,) As Boolean
    Dim pressed(,) As Boolean
    Dim updated(,) As Boolean
    Enum SweepMapStatus
        empty = 9
        marked = 10
        unknown = 11
    End Enum
    Private Sub Sweepinit()
        ReDim swmap(mh, mw)
        ReDim checked(mh, mw)
        ReDim updated(mh, mw)
        ReDim pressed(mh, mw)
        For j = 1 To mh
            For i = 1 To mw
                checked(j, i) = False
                updated(j, i) = True
                pressed(j, i) = False
            Next i
        Next j
    End Sub
    Private Sub AutoSweep()
        getsweepmap()
        Dim sweepagain As Boolean = False
        For j = 1 To mh
            For i = 1 To mw
                If checked(j, i) Then Continue For
                Dim ans As Byte = swmap(j, i)
                If ans = SweepMapStatus.marked Then
                    '雷,不用搜索直接完成
                    checked(j, i) = True
                ElseIf ans = SweepMapStatus.empty Then
                    '空格，把周围update，否则连锁开启时后续无法跟进
                    For jy = j - 1 To j + 1
                        For ix = i - 1 To i + 1
                            If (ix < 1) OrElse (jy < 1) OrElse
                                (ix > mw) OrElse (jy > mh) Then Continue For
                            If (ix = i) AndAlso (jy = j) Then Continue For
                            If checked(jy, ix) Then Continue For
                            updated(jy, ix) = True
                        Next ix
                    Next jy
                    checked(j, i) = True
                ElseIf ans <> SweepMapStatus.unknown Then
                    '数字，则开始搜索
                    '先确定周围雷和未点开格子的数量
                    Dim nmine, nunknown As Integer
                    nmine = 0 : nunknown = 0
                    Dim ubpos() As Point
                    For jy = j - 1 To j + 1
                        For ix = i - 1 To i + 1
                            If (ix < 1) OrElse (jy < 1) OrElse
                                (ix > mw) OrElse (jy > mh) Then Continue For
                            If (ix = i) AndAlso (jy = j) Then Continue For
                            Dim ans2 As Byte = swmap(jy, ix)
                            If ans2 = SweepMapStatus.marked Then
                                nmine += 1
                            ElseIf ans2 = SweepMapStatus.unknown Then
                                nunknown += 1
                                '顺便记录未点开格子的位置
                                ReDim Preserve ubpos(nunknown)
                                ubpos(nunknown) = New Point(ix, jy)
                            End If
                        Next ix
                    Next jy

                    If (ans = nmine) And (nunknown > 0) Then
                        '雷=数字  表示可以点开周围方块
                        For l = 1 To nunknown
                            Dim ux, uy As Integer
                            ux = ubpos(l).X
                            uy = ubpos(l).Y
                            If pressed(uy, ux) Then Continue For
                            ClickBlock(ubpos(l), 1)
                            pressed(uy, ux) = True
                            updated(uy, ux) = True '点开后有新的数字出现 需要识别
                            sweepagain = True
                        Next l
                        checked(j, i) = True
                    ElseIf (ans = nmine + nunknown) And (nunknown > 0) Then
                        '把周围方块标雷
                        For l = 1 To nunknown
                            Dim ux, uy As Integer
                            ux = ubpos(l).X
                            uy = ubpos(l).Y
                            If pressed(uy, ux) Then Continue For
                            ClickBlock(ubpos(l), 2)
                            pressed(uy, ux) = True
                            updated(uy, ux) = True '标雷后可以指导其他数字 需要识别
                            sweepagain = True
                        Next l
                        checked(j, i) = True
                    End If
                End If
            Next i
        Next j
        If sweepagain Then
            Thread.Sleep(200) '延时0.2s 必须 否则等不到ui线程更新就做完了
            AutoSweep()
            Thread.Sleep(200)
            AutoSweep()
        End If

    End Sub
    Private Sub getsweepmap()
        For j = 1 To mh
            For i = 1 To mw
                If checked(j, i) Then Continue For
                If Not updated(j, i) Then Continue For
                updated(j, i) = False
                Select Case kmap(j, i)
                    Case KnowMapStatus.unknown
                        swmap(j, i) = SweepMapStatus.unknown
                    Case KnowMapStatus.marked
                        swmap(j, i) = SweepMapStatus.marked
                    Case KnowMapStatus.opened
                        If map(j, i) = 0 Then
                            swmap(j, i) = SweepMapStatus.empty
                        Else
                            swmap(j, i) = map(j, i)
                        End If
                End Select
            Next i
        Next j
    End Sub
    Private Declare Sub SetCursorPos Lib "user32" (ByVal x As Integer,
                                                   ByVal y As Integer)
    Private Declare Sub mouse_event Lib "user32" (ByVal dwFlags As Integer,
                                                  ByVal dx As Integer,
                                                  ByVal dy As Integer,
                                                  ByVal cButtons As Integer,
                                                  ByVal dwExtraInfo As Integer)
    Const MOUSEEVENTF_LEFTDOWN = &H2
    Const MOUSEEVENTF_LEFTUP = &H4
    Const MOUSEEVENTF_RIGHTDOWN = &H8
    Const MOUSEEVENTF_RIGHTUP = &H10

    Private Sub ClickBlock(ByVal b As Point, cparam As Byte)
        Dim cx, cy As Single
        Dim px, py As Single
        px = PictureBox1.Width / mw
        py = PictureBox1.Height / mh
        Dim margin, title As Integer
        margin = (Me.Width - Me.ClientSize.Width) / 2
        title = Me.Height - Me.ClientSize.Height - margin
        cx = Me.Left + margin + PictureBox1.Left + px * (b.X - 1 + 0.5)
        cy = Me.Top + title + PictureBox1.Top + py * (b.Y - 1 + 0.5)
        SetCursorPos(cx, cy)
        If cparam = 1 Then
            mouse_event(MOUSEEVENTF_LEFTDOWN, cx, cy, 0, 0)
            Thread.Sleep(20) '增加鼠标移动的视觉效果
            mouse_event(MOUSEEVENTF_LEFTUP, cx, cy, 0, 0)
        Else
            mouse_event(MOUSEEVENTF_RIGHTDOWN, cx, cy, 0, 0)
            Thread.Sleep(20)
            mouse_event(MOUSEEVENTF_RIGHTUP, cx, cy, 0, 0)
        End If
    End Sub

End Class
