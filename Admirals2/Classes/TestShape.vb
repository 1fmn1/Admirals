Imports Microsoft.Sample.Controls
Imports System.Globalization
Imports Microsoft.Win32
Imports System.Net
Imports Newtonsoft.Json

Imports System.Windows
Imports System.IO

Imports Windows.UI
Imports System.Windows.Media.Animation
Imports System.Collections.ObjectModel


Public Class TestShape
    Implements IVirtualChild
    Private _bounds As Rect
    Public Property Fill As Brush
    Public Property Stroke As Brush
    Public Property Label As String
    Public Property ID As Integer
    Public Property MapObject As GalaxyData
    Public Property type As Integer
    Private _selected As Boolean
    Private _visual As New Canvas With {.IsHitTestVisible = False}
    Private _visual2 As New Canvas With {.IsHitTestVisible = False}
    Private _points As Point()
    Private Event IVirtualChild_BoundsChanged As EventHandler Implements IVirtualChild.BoundsChanged
    Public Event Object_Click As MouseButtonEventHandler
    Public Event Object_MouseEnter As MouseEventHandler
    Public Event Object_MouseLeave As MouseEventHandler
    Private _myzindex As Short
    Public MenuItems As New ObservableCollection(Of MenuItem)
    Public Sub New(ByRef gdata As GalaxyData)
        _visual = Nothing
        MapObject = gdata
        type = gdata.type
        ID = gdata.id
        Dim si As Integer
        si = 30
        'Dim a As Ellipse
        'Try
        '    a = Visual.Children.Item(1)
        '    a.Stroke = Brushes.LightGray
        '    a.StrokeThickness = 1
        '    a.StrokeDashArray.Add(10)
        '    a.StrokeDashArray.Add(5)
        '    ' a.StrokeDashOffset = -0.5
        'Catch
        'End Try
        Select Case type
            Case 1
                _visual2 = AddPlanetsToMap(MapObject)
                _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - 25, -MapObject.y + 1300 / 2 - 25), New Size(50, 50))
                _visual2.ContextMenu = New ContextMenu
                MenuItems.Add(Menu_Header)
                MenuItems.Add(Menu_Select_Item)
                MenuItems.Add(Menu_move_to)
                MenuItems.Add(Menu_Guard_planet)

            Case 2
                If MapObject.state = 2 Then
                    _visual2 = AddShipToMap(MapObject, si)
                    _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - si / 2, -MapObject.y + 1300 / 2 - si / 2), New Size(si, si))         'Dim line As Canvas = DrawShipDirection(MapObject)

                    'MapObject.cnvPath = New TestShape(line, New Rect(New Point(MapObject.x + 1700 / 2, -MapObject.y + 1300 / 2), New Size(50, 50)), 10, MapObject.id)
                    'MapObject.cnvPath = New TestShape(line, New Rect(New Point(0, 0), New Size(1700, 1300)), 10, MapObject.id)


                ElseIf MapObject.state = 0 Then
                    Dim P As Point
                    Dim pid As Integer = GWMap.galaxy_data.First(Function(c) c.type IsNot Nothing AndAlso c.type = 5 AndAlso c.id = MapObject.target_id).planet_id
                    Dim gPlanet As GalaxyData = GWMap.galaxy_data.First(Function(c) c.type IsNot Nothing AndAlso c.type = 1 AndAlso c.id = pid)
                    P = New Point(gPlanet.x + 1700 / 2, -gPlanet.y + 1300 / 2)
                    If MapObject.pos_order = 0 Then MapObject.pos_order = MapObject.id
                    _visual2 = AddShipToMap(MapObject, si / 2)
                    _bounds = New Rect(RotXY(P, MapObject.pos_order * 2 * Math.PI / 20, 20, si / 4), New Size(20, 20))
                ElseIf MapObject.state = 1 Or MapObject.state = 99 Or MapObject.state = 3 Then
                    If MapObject.planet_id IsNot Nothing AndAlso MapObject.planet_id > 0 Then
                        Dim P As Point
                        For Each it As GalaxyData In GWMap.galaxy_data
                            If it.type = 1 AndAlso it.id = MapObject.planet_id Then
                                P = New Point(it.x + 1700 / 2, -it.y + 1300 / 2)
                            End If
                        Next
                        If MapObject.pos_order = 0 Then MapObject.pos_order = MapObject.id
                        _visual2 = AddShipToMap(MapObject, si / 2)
                        _bounds = New Rect(RotXY(P, MapObject.pos_order * 2 * Math.PI / 20, 20, si / 4), New Size(20, 20))
                    Else
                        _visual2 = AddShipToMap(MapObject, si)
                        _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - si / 2, -MapObject.y + 1300 / 2 - si / 2), New Size(si, si))
                    End If

                    If MapObject.state = 99 Then
                        _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - 10, -MapObject.y + 1300 / 2 - 10), New Size(20, 20))
                    End If

                ElseIf MapObject.state = 4 Then
                    Dim P As Point
                    For Each it As GalaxyData In GWBattles
                        If it.id = MapObject.battle_id Then
                            P = New Point(it.x + 1700 / 2, -it.y + 1300 / 2)
                            If MapObject.x < it.x And MapObject.y < it.y Then
                                MapObject.pos_order = 1
                            ElseIf MapObject.x > it.x And MapObject.y < it.y Then
                                MapObject.pos_order = 1
                            ElseIf MapObject.x < it.x And MapObject.y > it.y Then
                                MapObject.pos_order = 3
                            ElseIf MapObject.x > it.x And MapObject.y > it.y Then
                                MapObject.pos_order = 3
                            End If

                        End If
                    Next
                    If MapObject.pos_order = 0 Then MapObject.pos_order = MapObject.id
                    _visual2 = AddShipToMap(MapObject, si / 2)
                    _bounds = New Rect(RotXY(P, MapObject.pos_order * Math.PI / 2, 7.5, si / 4), New Size(20, 20))
                End If

                _visual2.ContextMenu = New ContextMenu
                MenuItems.Add(Menu_Header)
                MenuItems.Add(Menu_Select_Item)
                MenuItems.Add(Menu_move_to)
                MenuItems.Add(Menu_Catch_ship)
            Case 3

            Case 4
            Case 5
                'If MapObject.pos_order = 0 Then MapObject.pos_order = MapObject.id
                'P = New Point(MapObject.x + 1700 / 2 + 12, -MapObject.y + 1300 / 2 - 5)
                'If MapObject.pos_order = 0 Then MapObject.pos_order = MapObject.id
                _visual2 = AddBuildingToMap(MapObject)
                Dim h As Short
                If gdata.image = "starport" Then
                    h = -7
                Else
                    h = 7
                End If
                '_bounds = New Rect(RotXYBattle(P, MapObject.pos_order * 2 * Math.PI / 4), New Size(10, 10))
                _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - 5 + 15, -MapObject.y + 1300 / 2 - 5 + h), New Size(10, 10))
                '_bounds = New Rect(New Point(MapObject.x + 1700 / 2 + 12, -MapObject.y + 1300 / 2 - 5), New Size(10, 10))

                _visual2.ContextMenu = New ContextMenu
                MenuItems.Add(Menu_Header)
                MenuItems.Add(Menu_Select_Item)
            Case 6

                _visual2 = AddBattlesToMap(MapObject)
                _bounds = New Rect(New Point(MapObject.x + 1700 / 2 - 15, -MapObject.y + 1300 / 2 - 15), New Size(30, 30))

                _visual2.ContextMenu = New ContextMenu
                MenuItems.Add(Menu_Header)
                MenuItems.Add(Menu_Select_Item)
        End Select
        For Each child As UIElement In _visual2.Children
            child.IsHitTestVisible = False
        Next
        _visual2.ContextMenu = Nothing
        '_visual2.ContextMenu.ItemsSource = MenuItems
        _myzindex = Canvas.GetZIndex(_visual2)
    End Sub

    Public Sub New(ByRef Visual As Canvas, ByVal b As Rect, ByVal atype As Integer, ByVal aid As Integer)
        _visual = Nothing
        type = atype
        ID = aid
        _myzindex = Canvas.GetZIndex(Visual)
        'Dim a As Ellipse
        'Try
        '    a = Visual.Children.Item(1)
        '    a.Stroke = Brushes.LightGray
        '    a.StrokeThickness = 1
        '    a.StrokeDashArray.Add(10)
        '    a.StrokeDashArray.Add(5)
        '    ' a.StrokeDashOffset = -0.5
        'Catch
        'End Try
        _visual2 = Visual
        _bounds = b
    End Sub

    Public Sub RenewCrossPoints(g1 As ShipsData, g2 As ShipsData)
        Dim a As Ellipse, b As Point, c As Canvas
        'c = TryCast(_visual.Children.Item(0), Canvas)
        'If c Is Nothing Then Exit Sub
        Try
            a = TryCast(_visual.Children.Item(1), Ellipse)
            If IsNothing(a) Then Exit Sub
            a.Visibility = Visibility.Hidden
            'b = GetCrossPoint(g1.x, g1.y, g1.target_x, g1.target_y, g1.speed, g2.x, g2.y, g2.speed)
            b = GetCatchPoint(g2, g1)
            a.Visibility = Visibility.Visible
            'Canvas.SetTop(a, -b.Y)
            'Canvas.SetLeft(a, b.X)
            If g1.id = g2.id Then
                a.Visibility = Visibility.Hidden
            End If
            Canvas.SetTop(a, -b.Y - 5 + 650)
            Canvas.SetLeft(a, b.X - 5 + 850)
        Catch ERR As Exception
            Debug.Print(ERR.Message)
        End Try
        'Canvas.SetTop(a, -b.Y - 10 + 650)
        'Canvas.SetLeft(a, b.X - 10 + 850)
    End Sub
    Public Sub DrawSelect(Optional Opacity As Double = 1)
        Dim a As Ellipse
        Dim b As TextBlock
        _selected = True
        Try
            a = _visual.Children.Item(1)
            a.Stroke = Brushes.White
            a.Opacity = Opacity
            If a.Width > 20 Then
                b = _visual.Children.Item(2)
                b.Visibility = Visibility.Visible
                b.Opacity = Opacity
            End If
            'a.StrokeDashOffset = -0.5
            If type = 2 Then
                'Canvas.SetZIndex(_visual, 5)
            End If
        Catch
            Debug.Print("err")
        End Try
        Try
            a = _visual2.Children.Item(1)
            a.Stroke = Brushes.White
            a.Opacity = Opacity
            If a.Width > 20 Then
                b = _visual2.Children.Item(2)
                b.Visibility = Visibility.Visible
                b.Opacity = Opacity
            End If
            If type = 2 Then
                'Canvas.SetZIndex(_visual2, 5)
            End If
        Catch
            Debug.Print("err2")
        End Try
    End Sub

    Public Sub UnDrawSelect()
        Dim a As Ellipse
        Dim b As TextBlock
        _selected = False
        Try
            a = _visual.Children.Item(1)
            a.Stroke = Brushes.Transparent
            a.Opacity = 1
            If a.Width > 20 Then
                b = _visual.Children.Item(2)
                b.Visibility = Visibility.Hidden
                b.Opacity = 1
            End If
            If Me.MapObject.type = 1 Then
                b.Visibility = Visibility.Visible
            End If
            'a.StrokeDashOffset = -0.5
        Catch
            Debug.Print("err")
        End Try
        Try
            a = _visual2.Children.Item(1)
            a.Stroke = Brushes.Transparent
            a.Opacity = 1
            If a.Width > 20 Then
                b = _visual2.Children.Item(2)
                b.Visibility = Visibility.Hidden
                b.Opacity = 1
            End If
            If Me.MapObject.type = 1 Then
                b.Visibility = Visibility.Visible
            End If
            'a.StrokeDashOffset = -0.5
        Catch
            Debug.Print("err2")
        End Try
    End Sub
    Public Sub StartPath(p As Point, attack As Boolean)
        Dim a As Line
        Dim b As TextBlock
        Dim c As Ellipse
        Try
            a = _visual.Children.Item(0)
            b = _visual.Children.Item(1)
            c = _visual.Children.Item(2)
            a.Visibility = Visibility.Visible
            a.IsHitTestVisible = False
            b.IsHitTestVisible = False
            c.IsHitTestVisible = False
            a.X1 = p.X
            a.Y1 = p.Y
            a.X2 = p.X
            a.Y2 = p.Y
            If attack = True Then
                c.Visibility = Visibility.Visible
                Canvas.SetLeft(c, p.X - 10)
                Canvas.SetTop(c, p.Y - 10)
            End If

            With b
                .Visibility = Visibility.Visible
            End With
        Catch err As Exception
            Debug.Print("жопа" & err.Message)
        End Try
    End Sub

    Public Sub FinishPath()
        Dim a As Line
        Dim b As TextBlock
        Dim c As Ellipse
        Try
            a = _visual.Children.Item(0)
            b = _visual.Children.Item(1)
            c = _visual.Children.Item(2)
            a.Visibility = Visibility.Hidden
            b.Visibility = Visibility.Hidden
            c.Visibility = Visibility.Hidden
        Catch err As Exception
            Debug.Print("жопа" & err.Message)
        End Try
    End Sub

    Public Sub DrawCoords(p As Point, speed As Integer, attack As Boolean, Optional Test As String = "")
        Dim a As Line
        Dim b As TextBlock
        Dim c As Ellipse
        Try
            a = _visual.Children.Item(0)
            b = _visual.Children.Item(1)
            c = _visual.Children.Item(2)
            a.IsHitTestVisible = False
            a.X2 = p.X
            a.Y2 = p.Y
            Dim r As TimeSpan
            r = New TimeSpan(0, Math.Round(Math.Sqrt(Math.Pow(a.X2 - a.X1, 2) + Math.Pow(a.Y2 - a.Y1, 2)) * 60 / speed), 0)
            b.Text = String.Format(strTimeCords, p.X + 850, -p.Y + 650, r.Days, r.Hours, r.Minutes)
            Canvas.SetLeft(b, p.X + 20)
            Canvas.SetTop(b, p.Y + 10)
            If attack = True Then
                Canvas.SetLeft(c, p.X - 10)
                Canvas.SetTop(c, p.Y - 10)
                ' c.Visibility = Visibility.Visible
            End If
        Catch Err As Exception
            Debug.Print("жопа" & Err.Message)
        End Try
    End Sub

    Public ReadOnly Property Visual As UIElement Implements IVirtualChild.Visual
        Get
            Try
                Return _visual
            Catch
            End Try
        End Get
    End Property

    Public Function CreateVisual(ByVal parent As Microsoft.Sample.Controls.VirtualCanvas) As UIElement Implements IVirtualChild.CreateVisual
        If _visual Is Nothing Then
            _visual = _visual2

        End If

        Return _visual
    End Function

    Public Sub DisposeVisual() Implements IVirtualChild.DisposeVisual
        If _visual IsNot Nothing Then
            _visual2 = _visual
            _visual = Nothing
        End If
    End Sub

    Public Property Bounds As Rect Implements IVirtualChild.Bounds
        Get
            Return _bounds
        End Get
        Set(value As Rect)
            _bounds = value
        End Set
    End Property


    Private _parent As Microsoft.Sample.Controls.VirtualCanvas
    Private _typeface As Typeface
    Private _fontSize As Double

    Public Function MeasureText(ByVal parent As Microsoft.Sample.Controls.VirtualCanvas, ByVal label As String) As Size
        If _parent Is parent Then
            Dim fontFamily As FontFamily = CType(parent.GetValue(TextBlock.FontFamilyProperty), FontFamily)
            Dim fontStyle As FontStyle = CType(parent.GetValue(TextBlock.FontStyleProperty), FontStyle)
            Dim fontWeight As FontWeight = CType(parent.GetValue(TextBlock.FontWeightProperty), FontWeight)
            Dim fontStretch As FontStretch = CType(parent.GetValue(TextBlock.FontStretchProperty), FontStretch)
            _fontSize = CDbl(parent.GetValue(TextBlock.FontSizeProperty))
            _typeface = New Typeface(fontFamily, fontStyle, fontWeight, fontStretch)
            _parent = parent
        End If

        Dim ft As FormattedText = New FormattedText(label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, _fontSize, Brushes.Black)
        Return New Size(ft.Width, ft.Height)
    End Function
    Public Function AddBuildingToMap(ByVal gdata As GalaxyData) As Canvas
        Dim ellipse As New Ellipse
        Dim story As Animation.DoubleAnimation
        Dim canvas As New Canvas
        Dim clan_color As String
        'Dim Stack As New StackPanel
        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        Dim a As ImageBrush
        Try
            a = MyBrushes(gdata.image)
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/building/{gdata.image}_ico.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, gdata.image)
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            a = New ImageBrush With {.ImageSource = myimg}
        End Try
        'Stack.Background = a
        Dim MyRect As New Rectangle
        MyRect.Width = 10
        MyRect.Height = 10
        MyRect.OpacityMask = a
        MyRect.Fill = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        'Stack.Children.Add(MyRect)
        'ellipse2.Fill = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        ellipse.Stroke = Brushes.Transparent
        ellipse.StrokeThickness = 2
        ellipse.StrokeDashArray.Add(3)
        ellipse.StrokeDashArray.Add(1)
        ellipse.Fill = Brushes.Transparent
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = -20
        story.Duration = TimeSpan.FromSeconds(4.5)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        Canvas.SetZIndex(ellipse, 10)

        'If gdata.image = "starport" Then
        '    Canvas.SetTop(MyRect, -7)
        '    Canvas.SetTop(ellipse, -9.5)
        'Else
        '    Canvas.SetTop(MyRect, 7)
        '    Canvas.SetTop(ellipse, 4.5)
        'End If
        Canvas.SetTop(MyRect, 0)
        Canvas.SetTop(ellipse, -2.5)
        Canvas.SetLeft(ellipse, -2.5)
        If gdata.state = 0 Then
            Dim Mystory As ColorAnimation
            Mystory = New ColorAnimation
            Mystory.From = Color.FromArgb(0, 0, 255, 0)
            Mystory.To = Color.FromArgb(128, 0, 255, 0)
            Mystory.AccelerationRatio = 0.5
            'Mystory.DecelerationRatio = 0.1
            'Mystory.Duration = TimeSpan.FromSeconds(2)
            Mystory.AutoReverse = True
            Mystory.RepeatBehavior = Animation.RepeatBehavior.Forever
            Dim myl As New SolidColorBrush()
            myl.BeginAnimation(SolidColorBrush.ColorProperty, Mystory)
            'Dim myl As RadialGradientBrush = New RadialGradientBrush()
            'myl.GradientOrigin = New Point(0.5, 0.5)
            'myl.Center = New Point(0.5, 0.5)
            'myl.RadiusX = 1
            'myl.RadiusY = 1
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(255, 255, 0, 0), 0.5))
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(0, 255, 0, 0), 1))
            'myl.GradientStops(1).BeginAnimation(GradientStop.OffsetProperty, story)

            ellipse.Fill = myl
        End If
        If gdata.state = 4 Or gdata.state = 3 Then

            Dim Mystory As ColorAnimation

            Mystory = New ColorAnimation
            Mystory.From = Color.FromArgb(0, 255, 0, 0)
            Mystory.To = Color.FromArgb(128, 255, 0, 0)
            Mystory.AccelerationRatio = 0.5
            'Mystory.DecelerationRatio = 0.1
            'Mystory.Duration = TimeSpan.FromSeconds(2)
            Mystory.AutoReverse = True
            Mystory.RepeatBehavior = Animation.RepeatBehavior.Forever
            Dim myl As New SolidColorBrush()
            myl.BeginAnimation(SolidColorBrush.ColorProperty, Mystory)
            'Dim myl As RadialGradientBrush = New RadialGradientBrush()
            'myl.GradientOrigin = New Point(0.5, 0.5)
            'myl.Center = New Point(0.5, 0.5)
            'myl.RadiusX = 1
            'myl.RadiusY = 1
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(255, 255, 0, 0), 0.5))
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(0, 255, 0, 0), 1))
            'myl.GradientStops(1).BeginAnimation(GradientStop.OffsetProperty, story)

            ellipse.Fill = myl
        End If
        ellipse.Width = 15
        ellipse.Height = 15
        'ellipse.Fill = Brushes.Transparent
        ellipse.Name = "b" & gdata.id
        AddHandler ellipse.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        AddHandler ellipse.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        AddHandler ellipse.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        'Canvas.SetZIndex(canvas, 0)
        Canvas.SetZIndex(canvas, 15)
        canvas.Height = 20
        canvas.Width = 20
        canvas.Children.Add(MyRect)
        canvas.Children.Add(ellipse)
        Return canvas
    End Function

    Public Function AddShipToMap(ByVal gdata As GalaxyData, ByVal si As Integer) As Canvas
        Dim rectImg As New Rectangle With {.IsHitTestVisible = False}
        Dim ellipse2 As New Ellipse With {.IsHitTestVisible = False}
        Dim ellipse3 As New Ellipse With {.IsHitTestVisible = False}
        Dim clanRect As New Rectangle With {.IsHitTestVisible = False}
        Dim canvas As New Canvas
        Dim label As TextBlock = New TextBlock() With {.IsHitTestVisible = False, .FontWeight = FontWeights.UltraBold}
        Dim labelMembers As TextBlock = New TextBlock() With {.IsHitTestVisible = False, .FontWeight = FontWeights.UltraBold}
        Dim clan_color As String
        Dim story As Animation.DoubleAnimation
        clanRect.Width = 10
        clanRect.Height = 10
        If gdata.state = 99 Then si = si / 2
        Try
            clanRect.Fill = MyBrushes(gdata.clan_id)
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/clan/{gdata.clan_id}.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, gdata.clan_id)
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            clanRect.Fill = New ImageBrush With {.ImageSource = myimg}
        End Try
        Canvas.SetTop(clanRect, 0)
        Canvas.SetLeft(clanRect, 0)
        Canvas.SetZIndex(clanRect, 1)
        If si < 20 Or gdata.state = 99 Then
            clanRect.Visibility = Visibility.Hidden
        End If
        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        'drContext.DrawText(New FormattedText("Хуйня", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Verdana"), 12, Brushes.White), New Point(0, 0))
        'drContext.Close()
        'drContext.DrawImage(MyBrushes(gdata.image).ImageSource, New Rect(New Size(si, si)))
        labelMembers.FontSize = 12
        labelMembers.Text = gdata.members_count
        labelMembers.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        labelMembers.Background = Brushes.Transparent
        labelMembers.Width = 10
        labelMembers.Height = 15
        ' labelMembers.Visibility = Visibility.Hidden
        labelMembers.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        labelMembers.TextAlignment = TextAlignment.Left
        Canvas.SetTop(labelMembers, -3.5)
        Canvas.SetLeft(labelMembers, si - 8)
        Canvas.SetZIndex(labelMembers, 30)
        If si < 20 Or gdata.members_count = 0 Then
            labelMembers.Visibility = Visibility.Hidden
        End If

        label.FontSize = 8
        label.Text = gdata.name
        label.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        label.Background = Brushes.Transparent
        label.Height = 12
        label.Width = 80
        label.Visibility = Visibility.Hidden
        label.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        label.TextAlignment = TextAlignment.Center
        Canvas.SetTop(label, si * 0.75)
        Canvas.SetLeft(label, si * 0.5 - label.Width * 0.5)
        Canvas.SetZIndex(label, 30)
        rectImg.Width = si
        rectImg.Height = si * 0.75
        If gdata.state = 99 Then
            rectImg.Width = si
            rectImg.Height = si * 0.75
        End If
        rectImg.StrokeThickness = 2
        rectImg.StrokeDashArray.Add(3)
        rectImg.StrokeDashArray.Add(1)
        Canvas.SetLeft(rectImg, 0)
        Canvas.SetTop(rectImg, si * 0.13)
        Canvas.SetZIndex(rectImg, 15)
        'AddHandler clanRect.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        'AddHandler clanRect.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        'AddHandler clanRect.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        AddHandler rectImg.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        AddHandler rectImg.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        AddHandler rectImg.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        'AddHandler ellipse3.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        'AddHandler ellipse3.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        'AddHandler ellipse3.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        Try
            rectImg.Fill = MyBrushes(gdata.image)
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/ship/{gdata.image}.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, gdata.image)
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            rectImg.Fill = New ImageBrush With {.ImageSource = myimg}
        End Try
        ellipse2.Stroke = Brushes.Transparent
        ellipse2.StrokeThickness = 2
        story = New Animation.DoubleAnimation
        story.From = 0
        If si < 20 Or gdata.state = 99 Then
            story.To = (-si - 0.6) * Math.PI
            ellipse2.StrokeDashArray.Add(si / 2)
            ellipse2.StrokeDashArray.Add(si / 6)
        Else
            story.To = (-si + 0.1) * Math.PI
            ellipse2.StrokeDashArray.Add(si / 3)
            ellipse2.StrokeDashArray.Add(si / 9)
        End If
        story.Duration = TimeSpan.FromSeconds(5)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse2.BeginAnimation(Line.StrokeDashOffsetProperty, story)

        rectImg.Name = "s" & gdata.id
        ellipse2.Name = "m" & gdata.id
        ellipse2.IsHitTestVisible = False
        Dim myLinearGradientBrush As RadialGradientBrush = New RadialGradientBrush()
        myLinearGradientBrush.GradientOrigin = New Point(0.5, 0.5)
        myLinearGradientBrush.Center = New Point(0.5, 0.5)
        myLinearGradientBrush.RadiusX = 1
        myLinearGradientBrush.RadiusY = 1
        If gdata.members_count > 0 Then
            myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#EE" & clan_color.Replace("#", "")), 0))
        Else
            myLinearGradientBrush.GradientStops.Add(
New GradientStop(ColorConverter.ConvertFromString("#00" & clan_color.Replace("#", "")), 0))
        End If
        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#0f" & clan_color.Replace("#", "")), 0.5))
        ellipse2.Fill = myLinearGradientBrush
        ellipse2.Width = si * 1.5
        ellipse2.Height = si * 1.5
        Canvas.SetLeft(ellipse2, -0.25 * si)
        Canvas.SetTop(ellipse2, -0.25 * si)
        Canvas.SetZIndex(ellipse2, 3)
        canvas.Height = si * 1.5
        canvas.Width = si * 1.5
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(rectImg)
        canvas.Children.Add(ellipse2)
        canvas.Children.Add(label)
        canvas.Children.Add(clanRect)
        canvas.Children.Add(labelMembers)
        If gdata.state = 0 Then
            Dim Mystory As ColorAnimation
            ellipse3.Width = si * 1.5
            ellipse3.Height = si * 1.5
            Canvas.SetLeft(ellipse3, -si * 0.25)
            Canvas.SetTop(ellipse3, -si * 0.25)
            Canvas.SetZIndex(ellipse3, 0)
            Mystory = New ColorAnimation
            Mystory.From = Color.FromArgb(0, 0, 255, 0)
            Mystory.To = Color.FromArgb(128, 0, 255, 0)
            Mystory.AccelerationRatio = 0.5
            'Mystory.DecelerationRatio = 0.1
            'Mystory.Duration = TimeSpan.FromSeconds(2)
            Mystory.AutoReverse = True
            Mystory.RepeatBehavior = Animation.RepeatBehavior.Forever
            Dim myl As New SolidColorBrush()
            myl.BeginAnimation(SolidColorBrush.ColorProperty, Mystory)
            'Dim myl As RadialGradientBrush = New RadialGradientBrush()
            'myl.GradientOrigin = New Point(0.5, 0.5)
            'myl.Center = New Point(0.5, 0.5)
            'myl.RadiusX = 1
            'myl.RadiusY = 1
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(255, 255, 0, 0), 0.5))
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(0, 255, 0, 0), 1))
            'myl.GradientStops(1).BeginAnimation(GradientStop.OffsetProperty, story)

            ellipse3.Fill = myl
            canvas.Children.Add(ellipse3)
        End If
        If gdata.state = 4 Or gdata.state = 3 Then
            Dim Mystory As ColorAnimation
            ellipse3.Width = si * 1.5
            ellipse3.Height = si * 1.5
            Canvas.SetLeft(ellipse3, -si * 0.25)
            Canvas.SetTop(ellipse3, -si * 0.25)
            Canvas.SetZIndex(ellipse3, 0)
            Mystory = New ColorAnimation
            Mystory.From = Color.FromArgb(0, 255, 0, 0)
            Mystory.To = Color.FromArgb(128, 255, 0, 0)
            Mystory.AccelerationRatio = 0.5
            'Mystory.DecelerationRatio = 0.1
            'Mystory.Duration = TimeSpan.FromSeconds(2)
            Mystory.AutoReverse = True
            Mystory.RepeatBehavior = Animation.RepeatBehavior.Forever
            Dim myl As New SolidColorBrush()
            myl.BeginAnimation(SolidColorBrush.ColorProperty, Mystory)
            'Dim myl As RadialGradientBrush = New RadialGradientBrush()
            'myl.GradientOrigin = New Point(0.5, 0.5)
            'myl.Center = New Point(0.5, 0.5)
            'myl.RadiusX = 1
            'myl.RadiusY = 1
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(255, 255, 0, 0), 0.5))
            'myl.GradientStops.Add(New GradientStop(Color.FromArgb(0, 255, 0, 0), 1))
            'myl.GradientStops(1).BeginAnimation(GradientStop.OffsetProperty, story)

            ellipse3.Fill = myl
            canvas.Children.Add(ellipse3)
        End If
        'Canvas.SetLeft(canvas, point.X - canvas.Width / 2)
        'Canvas.SetBottom(canvas, point.Y - canvas.Height / 2)
        Canvas.SetZIndex(canvas, 9)
        ' Canvas.SetZIndex(ellipse, 10)
        'Dim cash As New BitmapCache(10)
        'cash.EnableClearType = True
        'cash.SnapsToDevicePixels = True
        'canvas.CacheMode = cash
        Return canvas
    End Function
    Public Function AddBattlesToMap(gdata As GalaxyData) As Canvas
        Dim ellipse As New Ellipse With {.Width = 30, .Height = 30}
        Dim ell(4) As Ellipse
        Dim text As New TextBlock
        Dim canvas As New Canvas
        Dim rnd As New Random
        'Dim explode As New DoubleAnimation With {.From = 0, .To = 1, .RepeatBehavior = RepeatBehavior.Forever}
        Dim story As New DoubleAnimation With {.From = 0, .To = 10, .RepeatBehavior = RepeatBehavior.Forever}
        Dim trans(4) As ScaleTransform
        Dim brush As ImageBrush
        brush = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://application:,,,/style/fire.png"))}
        'explode.Duration = TimeSpan.FromSeconds(2)
        For i = 0 To 4
            ell(i) = New Ellipse With {.Width = 30, .Height = 30}
            trans(i) = New ScaleTransform With {.CenterX = 15, .CenterY = 15}
            ell(i).Fill = brush
            ell(i).IsHitTestVisible = False
            Canvas.SetTop(ell(i), 0 + 3 * Math.Cos(i * 2 * Math.PI / 5) - 3 * Math.Sin(i * 2 * Math.PI / 5))
            Canvas.SetLeft(ell(i), 0 + 3 * Math.Cos(i * 2 * Math.PI / 5) + 3 * Math.Sin(i * 2 * Math.PI / 5))
            ell(i).RenderTransform = trans(i)
            With trans(i)
                .CenterX = ell(i).Height / 2
                .CenterY = ell(i).Height / 2
            End With
            Dim a As New DoubleAnimation
            With a
                a.From = 0
                a.To = 1
                a.Duration = TimeSpan.FromSeconds(i + 3 / 4)
                a.RepeatBehavior = RepeatBehavior.Forever
            End With
            trans(i).BeginAnimation(ScaleTransform.ScaleXProperty, a)
            trans(i).BeginAnimation(ScaleTransform.ScaleYProperty, a)
        Next


        story.Duration = TimeSpan.FromSeconds(5)

        ellipse.Width = 30
        ellipse.Height = 30
        ellipse.StrokeThickness = 2
        ellipse.StrokeDashArray.Add(5)
        ellipse.StrokeDashArray.Add(3)

        text.FontSize = 8
        text.Text = gdata.name & $" [{gdata.id}]"
        text.Foreground = Brushes.Gold
        text.Background = Brushes.Transparent
        text.Height = 14
        text.Width = 120
        text.Visibility = Visibility.Hidden
        text.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        text.TextAlignment = TextAlignment.Left
        Canvas.SetLeft(text, 30)
        Canvas.SetTop(text, 30)
        Canvas.SetZIndex(text, 30)

        'ellipse.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        'ellipse.ContextMenu = pMenu
        'AddHandler ellipse.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        'AddHandler ellipse.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        'AddHandler ellipse.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        'Try
        'ellipse.Fill = MyBrushes.First.Value
        'ellipse.Fill = brush
        'ellipse2.Fill = brush
        'ellipse3.Fill = brush
        'ellipse4.Fill = brush
        'ellipse5.Fill = brush

        'trans.BeginAnimation(ScaleTransform.ScaleXProperty, explode)
        'trans.BeginAnimation(ScaleTransform.ScaleYProperty, explode)
        'ellipse2.BeginAnimation(Ellipse.StretchProperty, explode)
        'ellipse3.BeginAnimation(Ellipse.StretchProperty, explode)
        'ellipse4.BeginAnimation(Ellipse.StretchProperty, explode)
        'ellipse5.BeginAnimation(Ellipse.StretchProperty, explode)

        ellipse.Name = "e" & gdata.id
        'Catch
        '    Dim myimg As New BitmapImage
        '    myimg.BeginInit()
        '    myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/planet/{gdata.image}.png", UriKind.Absolute)
        '    myimg.CacheOption = BitmapCacheOption.OnLoad
        '    myimg.EndInit()
        '    SaveImage(myimg, gdata.image)
        '    'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
        '    ellipse.Fill = New ImageBrush With {.ImageSource = myimg}
        'End Try
        Canvas.SetLeft(ellipse, 0)
        Canvas.SetTop(ellipse, 0)
        Canvas.SetZIndex(ellipse, 2)
        canvas.Height = 30
        canvas.Width = 30
        canvas.Background = Brushes.Transparent
        For i = 0 To 4
            If i = 1 Then
                canvas.Children.Add(ellipse)
                canvas.Children.Add(text)
            End If
            canvas.Children.Add(ell(i))
        Next
        Canvas.SetZIndex(canvas, 2)
        Return canvas
    End Function
    Public Function AddPlanetsToMap(gdata As GalaxyData) As Canvas
        Dim ellipse As New Ellipse
        Dim ellipse2 As New Ellipse With {.IsHitTestVisible = False}
        Dim canvas As New Canvas
        Dim label As TextBlock = New TextBlock() With {.IsHitTestVisible = False}
        Dim clan_color As String
        Dim labelMembers As TextBlock = New TextBlock() With {.IsHitTestVisible = False}
        Dim labelTotalMembers As TextBlock = New TextBlock() With {.IsHitTestVisible = False}
        Dim clanRect As New Rectangle With {.IsHitTestVisible = False}
        Dim story As Animation.DoubleAnimation

        clanRect.Width = 20
        clanRect.Height = 20
        Try
            clanRect.Fill = MyBrushes(gdata.clan_id)
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/clan/{gdata.clan_id}.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, gdata.clan_id)
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            clanRect.Fill = New ImageBrush With {.ImageSource = myimg}
        End Try
        Canvas.SetTop(clanRect, 15)
        Canvas.SetLeft(clanRect, -25)
        Canvas.SetZIndex(clanRect, 3)

        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
            clanRect.Visibility = Visibility.Hidden
        End If


        labelTotalMembers.FontSize = 10
        labelTotalMembers.Text = gdata.citizens_total_count
        labelTotalMembers.Foreground = Brushes.Gray
        labelTotalMembers.Background = Brushes.Transparent
        labelTotalMembers.Width = 20
        labelTotalMembers.Height = 20
        ' labelMembers.Visibility = Visibility.Hidden
        labelTotalMembers.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        labelTotalMembers.TextAlignment = TextAlignment.Center
        Canvas.SetTop(labelTotalMembers, 3)
        Canvas.SetLeft(labelTotalMembers, -25)
        Canvas.SetZIndex(labelTotalMembers, 3)
        labelMembers.FontSize = 14
        labelMembers.Text = gdata.citizens_count
        labelMembers.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        labelMembers.Background = Brushes.Transparent
        labelMembers.Width = 20
        labelMembers.Height = 20
        ' labelMembers.Visibility = Visibility.Hidden
        labelMembers.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        labelMembers.TextAlignment = TextAlignment.Center
        Canvas.SetTop(labelMembers, 33)
        Canvas.SetLeft(labelMembers, -25)
        Canvas.SetZIndex(labelMembers, 3)
        If gdata.citizens_count = 0 Then
            labelMembers.Visibility = Visibility.Hidden
        End If

        label.Text = gdata.name
        label.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        label.Background = Brushes.Transparent
        label.Width = 140
        label.Height = 20
        'label.HorizontalAlignment = HorizontalAlignment.Stretch
        'label.VerticalAlignment = VerticalAlignment.Top
        label.TextAlignment = TextAlignment.Center
        'label.Visibility = Visibility.Hidden
        Canvas.SetTop(label, 50)
        Canvas.SetLeft(label, -45)
        Canvas.SetZIndex(label, 1)


        'ellipse.Fill = brush
        ellipse.Width = 50
        ellipse.Height = 50
        ellipse.StrokeThickness = 2
        ellipse.StrokeDashArray.Add(3)
        ellipse.StrokeDashArray.Add(1)
        AddHandler ellipse.MouseUp, New MouseButtonEventHandler(AddressOf ellipse_click)
        AddHandler ellipse.MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
        AddHandler ellipse.MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
        Try
            ellipse.Fill = MyBrushes(gdata.image)
            'ellipse.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Planets/{gdata.image}.png"))}
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/planet/{gdata.image}.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, gdata.image)
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            ellipse.Fill = New ImageBrush With {.ImageSource = myimg}
        End Try
        Canvas.SetLeft(ellipse, 0)
        Canvas.SetTop(ellipse, 0)
        Canvas.SetZIndex(ellipse, 2)


        Dim myLinearGradientBrush As RadialGradientBrush = New RadialGradientBrush()
        myLinearGradientBrush.GradientOrigin = New Point(0.5, 0.5)
        myLinearGradientBrush.Center = New Point(0.5, 0.5)
        myLinearGradientBrush.RadiusX = 0.5
        myLinearGradientBrush.RadiusY = 0.5

        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#CC" & clan_color.Replace("#", "")), 0))

        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#00" & clan_color.Replace("#", "")), 0.9))

        ellipse2.Width = 100
        ellipse2.Height = 100
        ellipse2.Fill = myLinearGradientBrush
        Canvas.SetLeft(ellipse2, -25)
        Canvas.SetTop(ellipse2, -25)
        ' Canvas.SetZIndex(ellipse2, 1)
        Canvas.SetZIndex(ellipse2, 0)
        ellipse2.Stroke = Brushes.Transparent
        ellipse2.StrokeThickness = 2
        ellipse2.StrokeDashArray.Add(35.5)
        ellipse2.StrokeDashArray.Add(17)
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = -100 / 2 * Math.PI
        story.Duration = TimeSpan.FromSeconds(3)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse2.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        ellipse.Name = "p" & gdata.id
        ellipse2.Name = "l" & gdata.id
        ellipse2.IsHitTestVisible = False
        canvas.Height = 50
        canvas.Width = 50
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(ellipse)
        canvas.Children.Add(ellipse2)
        canvas.Children.Add(label)
        canvas.Children.Add(clanRect)
        canvas.Children.Add(labelMembers)
        canvas.Children.Add(labelTotalMembers)
        'canvas.SetLeft(canvas, Point.X - canvas.Width / 2)
        'canvas.SetBottom(canvas, Point.Y - canvas.Height / 2)
        Canvas.SetZIndex(canvas, -1)
        ' Canvas.SetZIndex(ellipse, 10)
        Return canvas
    End Function




    Private Function RotXY(ByVal P0 As Point, ByVal alpha As Double, ByVal r As Double, si As Double) As Point
        Dim c As Double, s As Double, rx As Double, ry As Double, NewPoint As New Point
        Dim x As Double, y As Double
        x = P0.X + r
        y = P0.Y + r
        'rx = x - P0.X
        'ry = y - P0.Y
        c = Math.Cos(alpha)
        s = Math.Sin(alpha)
        NewPoint.X = P0.X + r * c - r * s - si
        NewPoint.Y = P0.Y + r * c + r * s - si
        Return NewPoint
    End Function


    Private Sub ellipse_click(sender As Object, e As MouseButtonEventArgs)
        'RaiseEvent Object_Click(Me, e)
    End Sub

    Private Sub ellipse_mouseenter(sender As Object, e As MouseEventArgs)
        'RaiseEvent Object_MouseEnter(Me, e)


        'CType(sender, Ellipse).Stroke = Brushes.White
        'CType(sender, Ellipse).Stroke.Opacity += 0.5
    End Sub
    Private Sub ellipse_mouseleave(sender As Object, e As MouseEventArgs)
        'RaiseEvent Object_MouseLeave(Me, e)
        'CType(sender, Ellipse).Stroke.Opacity -= 0.5
        'CType(sender, Ellipse).Stroke = Brushes.Transparent
    End Sub

    Public Sub ShowContextMenu()
        Dim a As Canvas
        If IsNothing(_visual) = False Then
            If IsNothing(_visual.ContextMenu) = False Then
                '_visual.ContextMenu.BeginInit()
                ' _visual.ContextMenu.ItemsSource = MenuItems
                ' _visual.ContextMenu.EndInit()
                _visual.ContextMenu.Visibility = Visibility.Visible
            End If
        End If
    End Sub

    Function Menu_Header() As MenuItem
        Dim a As New MenuItem
        Dim img As New Image With {.Height = 40, .Width = 40, .Stretch = Stretch.Uniform}
        If MapObject.type = 6 Then
            img.Source = New BitmapImage(New Uri("Style/fire.png", UriKind.Relative))
        Else
            Try
                img.Source = MyBrushes(MapObject.image).ImageSource
            Catch
            End Try
        End If
        a.BeginInit()
        a.Header = MapObject.name & $"[{MapObject.id}]"
        a.Icon = img
        a.FontSize = 14
        a.FontWeight = FontWeights.ExtraBold
        a.IsEnabled = False
        a.EndInit()
        Return a
    End Function
    Function Menu_Guard_planet() As MenuItem
        Dim a As New MenuItem With {.Header = "Guard planet"}
        Dim img As New Image With {.Source = New BitmapImage(New Uri("Style/script.png", UriKind.Relative)), .Height = 40, .Width = 40, .Stretch = Stretch.Uniform}
        a.BeginInit()
        a.Icon = img
        a.FontSize = 14
        AddHandler a.Click, New RoutedEventHandler(AddressOf PMenu_Clicked)
        a.EndInit()
        Return a
    End Function

    Function Menu_Select_Item() As MenuItem
        Dim a As New MenuItem With {.Header = "Select"}
        Dim img As New Image With {.Source = New BitmapImage(New Uri("Style/target.png", UriKind.Relative)), .Height = 40, .Width = 40, .Stretch = Stretch.Uniform}
        a.Icon = img
        a.FontSize = 14
        AddHandler a.Click, New RoutedEventHandler(AddressOf Select_Clicked)
        Return a
    End Function

    Function Menu_Catch_ship() As MenuItem
        Dim a As New MenuItem With {.Header = "Catch ship"}
        Dim img As New Image With {.Source = New BitmapImage(New Uri("Style/script.png", UriKind.Relative)), .Height = 40, .Width = 40, .Stretch = Stretch.Uniform}
        a.BeginInit()
        a.Icon = img
        a.FontSize = 14
        AddHandler a.Click, New RoutedEventHandler(AddressOf SMenu_Clicked)
        a.EndInit()
        Return a
    End Function

    Function Menu_move_to() As MenuItem
        Dim img As New Image With {.Source = New BitmapImage(New Uri("Style/script.png", UriKind.Relative)), .Height = 40, .Width = 40, .Stretch = Stretch.Uniform}
        Dim a As New MenuItem With {.Header = "Move to"}
        a.BeginInit()
        a.Icon = img
        a.FontSize = 14
        AddHandler a.Click, New RoutedEventHandler(AddressOf mapMenu_Clicked)
        a.EndInit()
        Return a
    End Function

    Private Sub Select_Clicked(sender As Object, e As RoutedEventArgs)
        If IsNothing(SelectedObject) = False Then
            SelectedObject.UnDrawSelect()
        End If
        Me.DrawSelect()
        SelectedObject = Me
        selectedID(Me.MapObject.type) = Me.MapObject.id
        BuildingInfoo.Visibility = Visibility.Collapsed
        ShipInfoo.Visibility = Visibility.Collapsed
        PlanetInfoo.Visibility = Visibility.Collapsed
        BattleInfoo.Visibility = Visibility.Collapsed
        Select Case Me.MapObject.type
            Case 1
                PlanetsVieww.SelectedItem = GWPlanets.First(Function(x) x.id = SelectedObject.MapObject.id)
                PlanetsVieww.ScrollIntoView(PlanetsVieww.SelectedItem)
                PlanetInfoo.Update(PlanetsVieww.SelectedItem)
            Case 2
                ShipsVieww.SelectedItem = GWFleet.First(Function(x) x.id = SelectedObject.MapObject.id)
                ShipsVieww.ScrollIntoView(ShipsVieww.SelectedItem)
                ShipInfoo.Update(ShipsVieww.SelectedItem)
                For Each item As ShipsData In ShipsVieww.Items
                    If item.state = 2 Then
                        For Each child As TestShape In grid.VirtualChildren
                            If child.ID = item.id AndAlso child.type = 10 Then
                                child.RenewCrossPoints(item, ShipsVieww.SelectedItem)
                            End If
                        Next
                    End If
                Next
            Case 5
                BuildingsVieww.SelectedItem = GWBuildings.First(Function(x) x.id = Me.MapObject.id)
                BuildingsVieww.ScrollIntoView(BuildingsVieww.SelectedItem)
                BuildingInfoo.Update(BuildingsVieww.SelectedItem)
            Case 6
                BattleInfoo.Update(GWBattles.First(Function(x) x.id = Me.MapObject.id))
        End Select
    End Sub
    Private Sub PMenu_Clicked(sender As Object, e As RoutedEventArgs)
        'Dim a As MenuItem, b As ContextMenu, c As Integer, a1 As Ellipse, b1 As String
        'a = TryCast(sender, MenuItem)
        'b = a.Parent
        'a1 = b.PlacementTarget
        'If a1 Is Nothing Then Exit Sub
        'If a1.Name Is Nothing Then Exit Sub
        'If a1.Name.Length = 0 Then Exit Sub
        'b1 = a1.Name.Chars(0)
        'c = Integer.Parse(Mid(a1.Name, 2))
        If IsNothing(SelectedObject) Then Exit Sub
        GWScripts.Add(SelectedObject.MapObject, MapObject.id, MapObject.type, 0, BoxxAdmirals.SelectedItem)
    End Sub
    Private Sub SMenu_Clicked(sender As Object, e As RoutedEventArgs)
        'Dim a As MenuItem, b As ContextMenu, c As Integer, a1 As Ellipse, b1 As String
        'a = TryCast(sender, MenuItem)
        'b = a.Parent
        'a1 = b.PlacementTarget
        'If a1 Is Nothing Then Exit Sub
        'If a1.Name Is Nothing Then Exit Sub
        'If a1.Name.Length = 0 Then Exit Sub
        'b1 = a1.Name.Chars(0)
        'c = Integer.Parse(Mid(a1.Name, 2))
        If IsNothing(SelectedObject) Then Exit Sub
        GWScripts.Add(SelectedObject.MapObject, MapObject.id, MapObject.type, 1, BoxxAdmirals.SelectedItem)
    End Sub
    Private Sub mapMenu_Clicked(sender As Object, e As RoutedEventArgs)
        Dim a As MenuItem, b As ContextMenu, c As Integer, a1 As Rect, b1 As String
        If IsNothing(SelectedObject) Then Exit Sub
        GWScripts.Add(SelectedObject.MapObject, 0, 0, 2, BoxxAdmirals.SelectedItem, -1,
          MapObject.x, MapObject.y)
    End Sub
End Class



