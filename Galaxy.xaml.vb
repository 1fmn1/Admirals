Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports Microsoft.VisualStudio.Controls
Imports System.Windows.Media.Animation
Imports System.Diagnostics
Imports Microsoft.Sample.Controls
Imports System.Globalization
Imports Microsoft.Win32
Imports System.Net
Imports Newtonsoft.Json
'Imports System.Drawing
Imports System.IO
Imports Windows.UI
Imports System.ComponentModel
Imports System.Text.RegularExpressions

Public Class Window1

    Public Property People As List(Of Person)
    Const fileNotable As String = "Notable PLayers.txt"
    Dim wc As New WebClient
    Dim a As String
    Dim WithEvents timer1 As New Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 60)}
    Dim tskBar As Shell.TaskbarManager
    Dim MovingShipscounter As Integer
    Dim pattern As String = "mid=\d*"
    Public MyShips As New CollectionViewSource
    Public MyBuildings As New CollectionViewSource
    'Dim Stars As New Canvas
    'Dim ObjectImages As New List(Of Image)
    'Dim img(10) As BitmapImage
    Dim mousepos As Point
    Private _speed As Integer
    Private _attack As Boolean
    Private _target As Integer
    Private Navigating As Boolean
    Private zoom As MapZoom
    Private pan As Pan
    Private rectZoom As RectangleSelectionGesture
    Private autoScroll As AutoScroll
    Private grid As VirtualCanvas
    Private _showGridLines As Boolean
    Private _animateStatus As Boolean = True
    Private _tileWidth As Double = 50
    Private _tileHeight As Double = 50
    Private _tileMargin As Double = 10
    Private _totalVisuals As Integer = 0
    Private rows As Integer = 30
    Private cols As Integer = 30
    Private selectedShipID As Integer = -1
    Private selectedPlanetID As Integer = -1
    Private selectedBuildingID As Integer = -1



    Public Sub New()

        InitializeComponent()
        ShipsView.DataContext = GWFleet
        PlanetsView.DataContext = GWPlanets
        BuildingsView.DataContext = GWBuildings
        txtLog.DataContext = logString
        'Me.Height = 650
        'Me.Width = 1100
        'wc.Headers.Item(HttpRequestHeader.Cookie) = File.ReadAllText(fileCookies)
        txtLogGlobal = txtLog
        grid = Graph
        grid.SmallScrollIncrement = New Size(_tileWidth + _tileMargin, _tileHeight + _tileMargin)
        Dim target As Canvas = grid.ContentCanvas
        zoom = New MapZoom(target)
        zoom.Zoom = 0.5
        zoom.Offset = New Point(0, 0)
        Dim v As Object = Scroller.GetValue(ScrollViewer.CanContentScrollProperty)
        pan = New Pan(target, zoom)
        rectZoom = New RectangleSelectionGesture(target, zoom, ModifierKeys.Control)
        rectZoom.ZoomSelection = True
        autoScroll = New AutoScroll(target, zoom)
        AddHandler zoom.ZoomChanged, New EventHandler(AddressOf OnZoomChanged)
        'AddHandler grid.VisualsChanged, New EventHandler(Of VisualChangeEventArgs)(AddressOf OnVisualsChanged)
        ' AddHandler ZoomSlider.ValueChanged, New EventHandler(AddressOf OnZoomSliderValueChanged)
        AddHandler grid.Scale.Changed, New EventHandler(AddressOf OnScaleChanged)
        AddHandler grid.Translate.Changed, New EventHandler(AddressOf OnScaleChanged)
        grid.Background = New ImageBrush(New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/stars.jpg")))
        'Dim a As Point = GetCrossPoint(-4, 3, 4, 1, 3, -3, -2, 6)
        ShipTypes.Add("x_wing", 4)
        ShipTypes.Add("corvette", 5)
        ShipTypes.Add("nova-courier", 3)
        ShipTypes.Add("Neutral_Fury", 6)
        ShipTypes.Add("Neutral_Infiltrator", 7)
        ShipTypes.Add("Neutral_Zygerrian", 8)

        BuildingTypes.Add("Headquaters", 1)
        BuildingTypes.Add("Starport", 2)
        MyShips.Source = GWFleet
        MyBuildings.Source = GWBuildings
        AddHandler MyShips.Filter, New FilterEventHandler(AddressOf ShowOnlyMyFilter)
        AddHandler MyBuildings.Filter, New FilterEventHandler(AddressOf ShowOnlyMyFilter)
        MyShipsView.DataContext = MyShips
        MyBuildingsView.DataContext = MyBuildings
        'New SolidColorBrush(Color.FromRgb(&HD0, &HD0, &HD0))
        'grid.ContentCanvas.Background = New ImageBrush(New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/stars.jpg")))
        ' AllocateNodes()
        ' Этот вызов является обязательным для конструктора.
        'DataContext = Stars
        ' Добавить код инициализации после вызова InitializeComponent().
        'img(0) = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/stars.jpg"))
        'With Stars
        '    .Width = img(0).PixelWidth
        '    .Height = (img(0).PixelHeight)
        '    .Background = New ImageBrush(img(0))
        'End With
    End Sub


    Private Sub OnSaveLog(ByVal sender As Object, ByVal e As RoutedEventArgs)
        MessageBox.Show("You need to build the assembly with 'DEBUG_DUMP' to get this feature")
    End Sub

    Private Sub OnScaleChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim t As Double = CSharpImpl.__Assign(_gridLines.StrokeThickness, 0.3 / grid.Scale.ScaleX)
        grid.Backdrop.BorderThickness = New Thickness(t)
    End Sub

    Private lastTick As Integer = Environment.TickCount
    Private addedPerSecond As Integer = 0
    Private removedPerSecond As Integer = 0

    'Private Sub OnVisualsChanged(ByVal sender As Object, ByVal e As VisualChangeEventArgs)
    '    If _animateStatus Then
    '        StatusText.Text = String.Format(CultureInfo.InvariantCulture, "{0} live visuals of {1} total", grid.LiveVisualCount, _totalVisuals)
    '        Dim tick As Integer = Environment.TickCount

    '        If e.Added <> 0 OrElse e.Removed <> 0 Then
    '            addedPerSecond += e.Added
    '            removedPerSecond += e.Removed

    '            If tick > lastTick + 100 Then
    '                Created.BeginAnimation(Rectangle.WidthProperty, New DoubleAnimation(Math.Min(addedPerSecond, 450), New Duration(TimeSpan.FromMilliseconds(100))))
    '                CreatedLabel.Text = addedPerSecond.ToString(CultureInfo.InvariantCulture) & " created"
    '                addedPerSecond = 0
    '                Destroyed.BeginAnimation(Rectangle.WidthProperty, New DoubleAnimation(Math.Min(removedPerSecond, 450), New Duration(TimeSpan.FromMilliseconds(100))))
    '                DestroyedLabel.Text = removedPerSecond.ToString(CultureInfo.InvariantCulture) & " disposed"
    '                removedPerSecond = 0
    '            End If
    '        End If

    '        If tick > lastTick + 1000 Then
    '            lastTick = tick
    '        End If
    '    End If
    'End Sub

    'Private Sub OnAnimateStatus(ByVal sender As Object, ByVal e As RoutedEventArgs)
    '    Dim item As MenuItem = CType(sender, MenuItem)
    '    _animateStatus = CSharpImpl.__Assign(item.IsChecked, Not item.IsChecked)
    '    StatusText.Text = ""
    '    Created.BeginAnimation(Rectangle.WidthProperty, Nothing)
    '    Created.Width = 0
    '    CreatedLabel.Text = ""
    '    Destroyed.BeginAnimation(Rectangle.WidthProperty, Nothing)
    '    Destroyed.Width = 0
    '    DestroyedLabel.Text = ""
    'End Sub

    Delegate Sub BooleanEventHandler(ByVal arg As Boolean)

    Private Sub ShowQuadTree(ByVal arg As Boolean)
        MessageBox.Show("You need to build the assembly with 'DEBUG_DUMP' to get this feature")
    End Sub

    Private Sub OnRowColChange(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim item As MenuItem = TryCast(sender, MenuItem)
        Dim d As Integer = Integer.Parse(CStr(item.Tag), CultureInfo.InvariantCulture)
        rows = CSharpImpl.__Assign(cols, d)
        AllocateNodes()
    End Sub

    Private Sub OnShowGridLines(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim item As MenuItem = CType(sender, MenuItem)
        Me.ShowGridLines = CSharpImpl.__Assign(item.IsChecked, Not item.IsChecked)
    End Sub

    Private _gridLines As Polyline = New Polyline()

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702")>
    Public Property ShowGridLines As Boolean
        Get
            Return _showGridLines
        End Get
        Set(ByVal value As Boolean)
            _showGridLines = value

            If value Then
                Dim width As Double = _tileWidth + _tileMargin
                Dim height As Double = _tileHeight + _tileMargin
                Dim numTileToAccumulate As Double = 16
                Dim gridCell As Polyline = _gridLines
                gridCell.Margin = New Thickness(_tileMargin)
                gridCell.Stroke = Brushes.LightSalmon
                gridCell.StrokeThickness = 0.3
                gridCell.Points = New PointCollection(New Point() {New Point(0, height - 0.3), New Point(width - 0.3, height - 0.3), New Point(width - 0.3, 0)})
                Dim gridLines As VisualBrush = New VisualBrush(gridCell)
                gridLines.TileMode = TileMode.Tile
                gridLines.Viewport = New Rect(0, 0, 1.0 / numTileToAccumulate, 1.0 / numTileToAccumulate)
                gridLines.AlignmentX = AlignmentX.Center
                gridLines.AlignmentY = AlignmentY.Center
                Dim outerVB As VisualBrush = New VisualBrush()
                Dim outerRect As Rectangle = New Rectangle()
                outerRect.Width = 10.0
                outerRect.Height = 10.0
                outerRect.Fill = gridLines
                outerVB.Visual = outerRect
                outerVB.Viewport = New Rect(0, 0, width * numTileToAccumulate, height * numTileToAccumulate)
                outerVB.ViewportUnits = BrushMappingMode.Absolute
                outerVB.TileMode = TileMode.Tile
                grid.Backdrop.Background = outerVB
                Dim border As Border = grid.Backdrop
                border.BorderBrush = Brushes.Blue
                border.BorderThickness = New Thickness(0.3)
                grid.InvalidateVisual()
            Else
                grid.Backdrop.Background = Nothing
            End If
        End Set
    End Property


    Private Sub OnZoom(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim item As MenuItem = CType(sender, MenuItem)
        Dim tag As String = TryCast(item.Tag, String)

        If tag = "Fit" Then
            Dim scaleX As Double = grid.ViewportWidth / grid.Extent.Width
            Dim scaleY As Double = grid.ViewportHeight / grid.Extent.Height
            zoom.Zoom = Math.Min(scaleX, scaleY)
            zoom.Offset = New Point(0, 0)
        Else
            Dim zoomPercent As Double

            If Double.TryParse(tag, zoomPercent) Then
                zoom.Zoom = zoomPercent / 100
                If zoom.Zoom < 0.25 Then
                    zoom.Zoom = 0.25
                End If
                If zoom.Zoom > 5 Then
                    zoom.Zoom = 5
                End If
            End If
        End If
    End Sub

    Private Sub OnZoomChanged(ByVal sender As Object, ByVal e As EventArgs)
        If zoom.Zoom < 0.25 Then
            zoom.Zoom = 0.25
        End If
        If zoom.Zoom > 5 Then
            zoom.Zoom = 5
        End If
        If ZoomSlider.Value <> zoom.Zoom Then
            ZoomSlider.Value = zoom.Zoom
        End If
    End Sub

    Private Sub OnZoomSliderValueChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Double)) Handles ZoomSlider.ValueChanged
        If zoom IsNot Nothing Then

            If zoom.Zoom <> e.NewValue Then
                zoom.Zoom = e.NewValue
            End If
        End If
    End Sub
    Public Function AddBuildingToMap(ByVal gdata As GalaxyData) As Canvas
        Dim ellipse1 As New Ellipse
        Dim ellipse2 As New Rectangle
        Dim story As Animation.DoubleAnimation
        Dim canvas As New Canvas
        Dim a As New BitmapImage
        Dim clan_color As String

        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        ellipse2.Width = 10
        ellipse2.Height = 10
        ellipse2.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Planets/{gdata.image}.png"))}
        'ellipse2.Fill = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        ellipse1.Stroke = Brushes.Transparent
        ellipse1.StrokeThickness = 2
        ellipse1.StrokeDashArray.Add(3)
        ellipse1.StrokeDashArray.Add(2)
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = -15
        story.Duration = TimeSpan.FromSeconds(2)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse1.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        Canvas.SetZIndex(ellipse2, 1)
        Canvas.SetZIndex(ellipse1, 2)
        If gdata.image = "starport" Then
            Canvas.SetTop(ellipse2, -5)
            Canvas.SetTop(ellipse1, -5)
        Else
            Canvas.SetTop(ellipse2, 5)
            Canvas.SetTop(ellipse1, 5)
        End If

        ellipse1.Width = 10
        ellipse1.Height = 10
        ellipse1.Fill = Brushes.Transparent
        ellipse1.Name = "b" & gdata.id
        Canvas.SetZIndex(canvas, 3)
        canvas.Height = 20
        canvas.Width = 20
        canvas.Children.Add(ellipse2)
        canvas.Children.Add(ellipse1)
        Return canvas
    End Function

    Public Function AddShipToMap(ByVal gdata As GalaxyData, ByVal si As Integer) As Canvas
        Dim ellipse As New Ellipse
        Dim ellipse2 As New Ellipse
        Dim clanRect As New Rectangle
        Dim canvas As New Canvas
        Dim label As TextBlock = New TextBlock()
        Dim labelMembers As TextBlock = New TextBlock()
        Dim clan_color As String
        Dim story As Animation.DoubleAnimation
        clanRect.Width = 10
        clanRect.Height = 10
        Try
            clanRect.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Clans/{gdata.clan_id}.png"))}
        Catch
            clanRect.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Clans/1.png"))}
        End Try
        Canvas.SetBottom(clanRect, 25)
        Canvas.SetLeft(clanRect, 27)
        Canvas.SetZIndex(clanRect, 1)
        If si < 20 Then
            clanRect.Visibility = Visibility.Hidden
        End If
        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If

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
        Canvas.SetBottom(labelMembers, 8)
        Canvas.SetLeft(labelMembers, 29)
        Canvas.SetZIndex(labelMembers, 1)
        If si < 20 Or gdata.members_count = 0 Then
            labelMembers.Visibility = Visibility.Hidden
        End If

        label.FontSize = 8
        label.Text = gdata.name
        label.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        label.Background = Brushes.Transparent
        label.Width = 40
        label.Height = 12
        label.Visibility = Visibility.Hidden
        label.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        label.TextAlignment = 1
        Canvas.SetBottom(label, 8)
        Canvas.SetLeft(label, -32)
        Canvas.SetZIndex(label, 1)
        ellipse.Width = si
        ellipse.Height = si
        ellipse.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Ships/{gdata.image}.png"))}
        ellipse2.Stroke = Brushes.Transparent
        ellipse2.StrokeThickness = 2
        ellipse2.StrokeDashArray.Add(10)
        ellipse2.StrokeDashArray.Add(5)
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = -30
        story.Duration = TimeSpan.FromSeconds(2)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse2.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        Canvas.SetLeft(ellipse, si / 2)
        Canvas.SetBottom(ellipse, si / 2)
        Canvas.SetZIndex(ellipse, 2)
        ellipse.Name = "s" & gdata.id
        ellipse2.Name = "m" & gdata.id

        Dim myLinearGradientBrush As RadialGradientBrush = New RadialGradientBrush()
        myLinearGradientBrush.GradientOrigin = New Point(0.5, 0.5)
        myLinearGradientBrush.Center = New Point(0.5, 0.5)
        myLinearGradientBrush.RadiusX = 1
        myLinearGradientBrush.RadiusY = 1
        If gdata.members_count > 0 Then
            myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#AA" & clan_color.Replace("#", "")), 0))
        End If
        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#01" & clan_color.Replace("#", "")), 0.5))
        ellipse2.Fill = myLinearGradientBrush
        ellipse2.Width = si * 2
        ellipse2.Height = si * 2
        'Canvas.SetLeft(ellipse2, 0)
        'Canvas.SetBottom(ellipse2, 0)
        Canvas.SetZIndex(ellipse2, 1)


        canvas.Height = si * 2
        canvas.Width = si * 2
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(ellipse)
        canvas.Children.Add(ellipse2)
        canvas.Children.Add(label)
        canvas.Children.Add(clanRect)
        canvas.Children.Add(labelMembers)
        'Canvas.SetLeft(canvas, point.X - canvas.Width / 2)
        'Canvas.SetBottom(canvas, point.Y - canvas.Height / 2)
        Canvas.SetZIndex(canvas, 2)
        ' Canvas.SetZIndex(ellipse, 10)
        Return canvas
    End Function
    Public Function AddPlanetsToMap(gdata As GalaxyData) As Canvas
        Dim ellipse As New Ellipse
        Dim ellipse2 As New Ellipse
        Dim canvas As New Canvas
        Dim label As TextBlock = New TextBlock()
        Dim clan_color As String
        Dim labelMembers As TextBlock = New TextBlock()
        Dim clanRect As New Rectangle
        Dim story As Animation.DoubleAnimation

        clanRect.Width = 20
        clanRect.Height = 20
        Try
            clanRect.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Clans/{gdata.clan_id}.png"))}
        Catch
            clanRect.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Clans/1.png"))}
            Debug.Print("пизда")
        End Try
        Canvas.SetBottom(clanRect, 40)
        Canvas.SetLeft(clanRect, 15)
        Canvas.SetZIndex(clanRect, 1)

        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
            clanRect.Visibility = Visibility.Hidden
        End If



        labelMembers.FontSize = 14
        labelMembers.Text = gdata.citizens_count
        labelMembers.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
        labelMembers.Background = Brushes.Transparent
        labelMembers.Width = 20
        labelMembers.Height = 20
        ' labelMembers.Visibility = Visibility.Hidden
        labelMembers.HorizontalAlignment = HorizontalAlignment.Left
        'label.VerticalAlignment = VerticalAlignment.Top
        labelMembers.TextAlignment = TextAlignment.Left
        Canvas.SetBottom(labelMembers, 40)
        Canvas.SetLeft(labelMembers, 5)
        Canvas.SetZIndex(labelMembers, 1)
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
        Canvas.SetBottom(label, 10)
        Canvas.SetLeft(label, -20)
        Canvas.SetZIndex(label, 1)


        'ellipse.Fill = brush
        ellipse.Width = 25
        ellipse.Height = 25
        ellipse.Fill = New ImageBrush With {.ImageSource = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Planets/{gdata.image}.png"))}
        Canvas.SetLeft(ellipse, 100 / 2 - 12.5)
        Canvas.SetBottom(ellipse, 100 / 2 - 12.5)
        Canvas.SetZIndex(ellipse, 2)


        Dim myLinearGradientBrush As RadialGradientBrush = New RadialGradientBrush()
        myLinearGradientBrush.GradientOrigin = New Point(0.5, 0.5)
        myLinearGradientBrush.Center = New Point(0.5, 0.5)
        myLinearGradientBrush.RadiusX = 0.5
        myLinearGradientBrush.RadiusY = 0.5

        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#90" & clan_color.Replace("#", "")), 0))

        myLinearGradientBrush.GradientStops.Add(
    New GradientStop(ColorConverter.ConvertFromString("#00" & clan_color.Replace("#", "")), 0.9))

        ellipse2.Width = 100
        ellipse2.Height = 100
        ellipse2.Fill = myLinearGradientBrush
        Canvas.SetLeft(ellipse2, 0)
        Canvas.SetBottom(ellipse2, 0)
        Canvas.SetZIndex(ellipse2, 1)
        Canvas.SetZIndex(ellipse2, 0)
        ellipse2.Stroke = Brushes.Transparent
        ellipse2.StrokeThickness = 2
        ellipse2.StrokeDashArray.Add(35)
        ellipse2.StrokeDashArray.Add(15)
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = -100
        story.Duration = TimeSpan.FromSeconds(3)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        ellipse2.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        ellipse.Name = "p" & gdata.id
        ellipse2.Name = "l" & gdata.id

        canvas.Height = 100
        canvas.Width = 100
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(ellipse)
        canvas.Children.Add(ellipse2)
        canvas.Children.Add(label)
        canvas.Children.Add(clanRect)
        canvas.Children.Add(labelMembers)
        'canvas.SetLeft(canvas, Point.X - canvas.Width / 2)
        'canvas.SetBottom(canvas, Point.Y - canvas.Height / 2)
        Canvas.SetZIndex(canvas, 1)
        ' Canvas.SetZIndex(ellipse, 10)
        Return canvas
    End Function

    Private Sub AllocateNodes()
        'Dim r As Random = New Random(Environment.TickCount)
        grid.VirtualChildren.Clear()
        grid.AddVirtualChild(New TestShape(DrawPath, New Rect(New Point(0, 0), New Size(1700, 1300)), -1, -1))
        For Each gwpack As GalaxyData In GWMap.galaxy_data


            If gwpack.type = 1 Then
                Dim shape As TestShape = New TestShape(AddPlanetsToMap(gwpack),
                   New Rect(New Point(gwpack.x + 1700 / 2 - 50, -gwpack.y + 1300 / 2 - 50), New Size(100, 100)), gwpack.type, gwpack.id)
                grid.AddVirtualChild(shape)
            End If

            If gwpack.type = 2 Then
                If gwpack.state = 2 Then
                    Dim shape As TestShape = New TestShape(AddShipToMap(gwpack, 20),
                                       New Rect(New Point(gwpack.x + 1700 / 2 - 20, -gwpack.y + 1300 / 2 - 20), New Size(20, 20)), gwpack.type, gwpack.id)
                    grid.AddVirtualChild(shape)
                    Dim line As Canvas = DrawLine(gwpack)
                    Dim shapeLine As TestShape = New TestShape(line, New Rect(New Point(gwpack.x + 1700 / 2, -gwpack.y + 1300 / 2), New Size(50, 50)), 10, gwpack.id)
                    grid.AddVirtualChild(shape)
                    grid.AddVirtualChild(shapeLine)
                Else
                    If gwpack.planet_id IsNot Nothing AndAlso gwpack.planet_id > 0 Then
                        Dim P As Point
                        For Each it As GalaxyData In GWMap.galaxy_data
                            If it.type = 1 AndAlso it.id = gwpack.planet_id Then
                                P = New Point(it.x + 1700 / 2 + 30, -it.y + 1300 / 2 + 30)
                            End If
                        Next
                        Dim shape As TestShape = New TestShape(AddShipToMap(gwpack, 10),
                                   New Rect(RotXY(P, gwpack.pos_order * 2 * Math.PI / 25), New Size(20, 20)), gwpack.type, gwpack.id)
                        grid.AddVirtualChild(shape)
                    Else
                        Dim shape As TestShape = New TestShape(AddShipToMap(gwpack, 20),
                                                           New Rect(New Point(gwpack.x + 1700 / 2 - 20, -gwpack.y + 1300 / 2 - 20), New Size(40, 40)), gwpack.type, gwpack.id)
                        grid.AddVirtualChild(shape)
                    End If
                End If
            End If
            If gwpack.type = 5 And gwpack.state < 99 Then
                For Each gwitem As GalaxyData In GWPlanets
                    If gwpack.planet_id = gwitem.id Then
                        gwpack.x = gwitem.x
                        gwpack.y = gwitem.y
                    End If
                Next
                Dim shape As TestShape = New TestShape(AddBuildingToMap(gwpack),
   New Rect(New Point(gwpack.x + 1700 / 2 + 12, -gwpack.y + 1300 / 2 - 5), New Size(10, 10)), gwpack.type, gwpack.id)
                grid.AddVirtualChild(shape)
            End If
        Next
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        RefreshGWData()
        AllocateNodes()
        For Each item As ShipsData In ShipsView.Items
            If item.id = selectedShipID Then
                ShipsView.SelectedItem = item
            End If
            If item.state = 2 Then
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = item.id AndAlso child.type = 10 Then
                        child.RenewCross(item, ShipsView.SelectedItem)
                    End If
                Next
            End If
        Next

        For Each item As GalaxyData In PlanetsView.Items
            If item.id = selectedPlanetID Then
                PlanetsView.SelectedItem = item
            End If
        Next


        For Each item As GalaxyData In BuildingsView.Items
            If item.id = selectedBuildingID Then
                BuildingsView.SelectedItem = item
            End If
        Next


        For Each child As TestShape In grid.VirtualChildren
            If child.ID = selectedShipID And child.type = 2 Then
                child.DrawSelect()

            End If
            If child.ID = selectedPlanetID And child.type = 1 Then
                child.DrawSelect()
            End If
            If child.ID = selectedBuildingID And child.type = 5 Then
                child.DrawSelect()
            End If
        Next
        If selectedShipID > -1 AndAlso ShipsView.Items.Count - 1 <= selectedShipID Then
            ShipsView.SelectedIndex = selectedShipID
        End If
        If selectedPlanetID > -1 AndAlso PlanetsView.Items.Count - 1 <= selectedPlanetID Then
            PlanetsView.SelectedIndex = selectedPlanetID
        End If
        If selectedBuildingID > -1 AndAlso BuildingsView.Items.Count - 1 <= selectedBuildingID Then
            BuildingsView.SelectedIndex = selectedBuildingID
        End If
    End Sub
    Private Sub Window1_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AddNotablePlayers()
        Dim profiles As String
        For Each profile As String In IO.Directory.GetFiles("Profiles/", "*.admiral", IO.SearchOption.TopDirectoryOnly)
            profiles = File.ReadAllText(profile)
            If profile = "" Or profiles = "" Then Continue For
            profile = CheckNotable(Mid(Regex.Match(profiles, pattern).Value, 5))
            If AdmiralsList.ContainsKey(profile) Then Continue For
            AdmiralsList.Add(profile, profiles)
        Next
        Combobx = BoxAdmirals
        Combobx.DataContext = AdmiralsList.Keys
        Logger.init()
        RefreshGWData()
        timer1.IsEnabled = True
        AllocateNodes()
        BoxRespawn.DataContext = GWPlanets
        BoxBuyShip.DataContext = GWBuildings
        BoxBuyBuilding.DataContext = GWPlanets
        BoxBuyBType.DataContext = BuildingTypes.Keys
        BoxEnterShip.DataContext = GWFleet
        BoxEnterBuilding.DataContext = GWBuildings
        BoxRenameShip.DataContext = GWFleet
        BoxBuyType.DataContext = ShipTypes.Keys
        If Combobx.Items.Count > 0 Then
            Combobx.SelectedItem = AdmiralsList.Keys(0)
            'BoxRespawn.SelectedItem = 1
            'BoxBuyShip.SelectedIndex = 1
            'BoxEnterShip.SelectedIndex = 1
            'BoxEnterBuilding.SelectedIndex = 1
            'BoxRenameShip.SelectedIndex = 1

        End If
        ' Dim oldsize As New Size
        ' oldsize = Me.RenderSize
        'Threading.Thread.Sleep(600)
        'Me.RenderSize = oldsize
    End Sub

    Sub AddNotablePlayers()
        Dim helpstring() As String
        helpstring = File.ReadAllLines(fileNotable)
        For Each a As String In helpstring
            If a.Length > 0 Then
                NotablePlayers.Add(a.Split("=")(0), a.Split("=")(1))
            End If
        Next
    End Sub

    Sub RefreshGWData()
        'Dim GWFleet As New GWFleetClass
        Dim LastEvents As New GWClass
        GWMap = Nothing
        GWFleet.Clear()
        GWPlanets.Clear()
        GWBuildings.Clear()
        Dim gwplayers As Integer = 0
        Dim MovingShipscounter As Integer = 0
        'wc.Headers.Item(HttpRequestHeader.Cookie) = "mid=12824; ac=3a8e7b72a0861827fe3d19d9bb51d46a; _ga=GA1.2.836802751.1488024943; b=b; PHPSESSID=s4iemv426vuj62l4ap2hgmkhn2; _gid=GA1.2.538733330.1534191728; camera=-266.5%3B-315"
        wc.Headers.Item(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36 OPR/54.0.2952.71"
        a = wc.DownloadString("http://galaxy.xjedi.com/srv/conn.php?act=client_start")
        GWMap = JsonConvert.DeserializeObject(Of GWClass)(a)
        For Each MapObject As GalaxyData In GWMap.galaxy_data
            If MapObject.type = 2 Then
                GWFleet.Add(New ShipsData(MapObject))
                gwplayers += MapObject.members_count
                If MapObject.state = 2 Then
                    MovingShipscounter += 1
                End If
            End If
            If MapObject.type = 1 Then
                GWPlanets.Add(MapObject)
                'gwplayers += MapObject.members_count
            End If
            If MapObject.type = 3 Then
                GWClans.Add(MapObject)
            End If

            If MapObject.type = 4 Then
                GWPLayer = MapObject
                UpdatePLayerTab()
            End If

            If MapObject.type = 5 AndAlso MapObject.state < 99 Then

                GWBuildings.Add(MapObject)
                If MapObject.image = "starport" Then
                    GWStarports.Add(MapObject)
                End If
            End If
            If MapObject.act = "setParam" Then
                If MapObject.name = "lastEventID" Then
                    GWMap.LastEventIDCheck = MapObject.value
                End If
            End If

        Next

        'ShipsView.Items
        If LastEventId > 0 And GWMap.LastEventIDCheck - LastEventId < 1000 Then
            Do While Not (GWMap.LastEventIDCheck = LastEventId)
                System.Threading.Thread.Sleep(500)
                'Application.DoEvents()
                a = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=galaxy_update&lastEventID={LastEventId}&lastLogEventID=0&currentAnnouncementID=0")
                LastEvents = JsonConvert.DeserializeObject(Of GWClass)(a)
                If LastEvents IsNot Nothing And LastEvents.galaxy_data.Length > 0 Then
                    Logger.GetPacket(LastEvents.galaxy_data)
                End If
            Loop
        Else
            LastEventId = GWMap.LastEventIDCheck
            Logger.LogWrite("Too much events happened since last time.")
        End If
        'Dim Ura As Uri
        If MovingShipscounter > 0 And MovingShipscounter < 10 Then
            TaskbarItemInfo.Overlay = CType(New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Icons/{MovingShipscounter}.png")), ImageSource)
        Else
            TaskbarItemInfo.Overlay = Nothing
        End If
        Me.Title = $"Admirals {Version}  {GWFleet.Count} ships in game with {gwplayers} players. Last event: {LastEventId}. Press F1 to open log."
        MyShips.View.Refresh()
        MyBuildings.View.Refresh()
    End Sub

    Private Sub ShipsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ShipsView.SelectionChanged
        If ShipsView.SelectedItem Is Nothing Then Exit Sub
        Dim gdata As ShipsData = ShipsView.SelectedItem
        If gdata.id = selectedShipID Then Exit Sub

        For Each child As TestShape In grid.VirtualChildren
            If child.ID = selectedShipID Then
                child.UnDrawSelect()
                ' zoom.ScrollIntoView(child.Bounds)
                '    Dim a As Object
                '    rectZoom.S = child.Bounds
                '    rectZoom.ZoomSelection = True
            End If
        Next
        For Each child As TestShape In grid.VirtualChildren
            If child.ID = gdata.id Then
                child.DrawSelect()
                zoom.ScrollIntoView(child.Bounds)
                Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                a.Inflate(New Size(200, 200))
                zoom.ZoomToRect(a)
                selectedShipID = gdata.id
            End If
        Next
    End Sub


    Private Sub PlanetsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles PlanetsView.SelectionChanged
        If PlanetsView.SelectedItem Is Nothing Then Exit Sub
        Dim gdata As GalaxyData = PlanetsView.SelectedItem
        If gdata.id = selectedPlanetID Then Exit Sub

        For Each child As TestShape In grid.VirtualChildren
            If child.ID = selectedPlanetID Then
                child.UnDrawSelect()
                ' zoom.ScrollIntoView(child.Bounds)
                '    Dim a As Object
                '    rectZoom.S = child.Bounds
                '    rectZoom.ZoomSelection = True
            End If
        Next
        For Each child As TestShape In grid.VirtualChildren
            If child.ID = gdata.id Then
                child.DrawSelect()
                zoom.ScrollIntoView(child.Bounds)
                Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                a.Inflate(New Size(200, 200))
                zoom.ZoomToRect(a)
                selectedPlanetID = gdata.id
            End If
        Next
    End Sub

    Private Sub BuildingsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BuildingsView.SelectionChanged
        If BuildingsView.SelectedItem Is Nothing Then Exit Sub
        Dim gdata As GalaxyData = BuildingsView.SelectedItem
        If gdata.id = selectedBuildingID Then Exit Sub

        For Each child As TestShape In grid.VirtualChildren
            If child.ID = selectedBuildingID Then
                child.UnDrawSelect()
                ' zoom.ScrollIntoView(child.Bounds)
                '    Dim a As Object
                '    rectZoom.S = child.Bounds
                '    rectZoom.ZoomSelection = True
            End If
        Next
        For Each child As TestShape In grid.VirtualChildren
            If child.ID = gdata.id Then
                child.DrawSelect()
                zoom.ScrollIntoView(child.Bounds)
                Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                a.Inflate(New Size(200, 200))
                zoom.ZoomToRect(a)
                selectedBuildingID = gdata.id
            End If
        Next
    End Sub
    Sub ExitProgram()
        End
    End Sub
    Private Sub Window1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Me.WindowState = WindowState.Minimized
        e.Cancel = True
    End Sub

    Private Sub Graph_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles Graph.MouseUp

        If Navigating = True Then
            If e.ChangedButton = MouseButton.Left Then
                Dim x As Integer, y As Integer, s As String = "", gwPack As GWClass
                If _attack AndAlso e.ChangedButton = MouseButton.Left Then
                    Dim a1 As Ellipse
                    Dim b As String
                    Dim c As Integer
                    a1 = TryCast(e.OriginalSource, Ellipse)
                    If a1 Is Nothing Then Exit Sub
                    If a1.Name Is Nothing Then Exit Sub
                    If a1.Name.Length = 0 Then Exit Sub
                    b = a1.Name.Chars(0)
                    c = Integer.Parse(Mid(a1.Name, 2))
                    If b = "s" Or b = "m" Then
                        s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedShipID}&target_type_id={2}&target_id={c}")
                        Logger.LogWrite($"Setting Ship's [{GWMap.GetShipName(selectedShipID)}] target to Ship [{GWMap.GetShipName(c)}] ")

                    ElseIf b = "p" Or b = "l" Then
                        s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedShipID}&target_type_id={1}&target_id={c}")
                        Logger.LogWrite($"Setting Ship's [{GWMap.GetShipName(selectedShipID)}] target to planet [{GWMap.GetPlanetName(c)}] ")
                    ElseIf b = "b" Then
                        s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedShipID}&target_type_id={5}&target_id={c}")
                        Logger.LogWrite($"Setting Ship's [{GWMap.GetShipName(selectedShipID)}] target to building [{GWMap.GetBuildingName(c)}] ")
                    End If
                Else
                    x = mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom - 1700 / 2
                    y = -(mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom - 1300 / 2)
                    s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedShipID}&target_x={x}&target_y={y}")
                    Logger.LogWrite($"Setting Ship's [{GWMap.GetShipName(selectedShipID)}] target point to [({x};{y})]")
                End If
                gwPack = JsonConvert.DeserializeObject(Of GWClass)(s)
                Logger.LogWrite($"Code: [{gwPack.code}]")
                If gwPack.code = "OK" Then
                    For Each MapObject As GalaxyData In gwPack.galaxy_data
                        If MapObject.type = 2 Then

                            For Each item As GalaxyData In GWMap.galaxy_data
                                If item.id = MapObject.id AndAlso item.type = MapObject.type Then
                                    item.planet_id = MapObject.planet_id
                                    item.x = MapObject.x
                                    item.y = MapObject.y
                                    item.target_x = MapObject.target_x
                                    item.target_y = MapObject.target_y
                                    item.state = MapObject.state
                                End If
                            Next
                            For Each item As ShipsData In GWFleet
                                If item.id = MapObject.id AndAlso item.type = MapObject.type Then
                                    item.planet_id = MapObject.planet_id
                                    item.x = MapObject.x
                                    item.y = MapObject.y
                                    item.target_x = MapObject.target_x
                                    item.target_y = MapObject.target_y
                                    item.state = MapObject.state
                                End If
                            Next
                        End If
                    Next

                End If
                Dim a As TestShape
                a = grid.VirtualChildren(0)
                Navigating = False
                _attack = False
                a.FinishPath()
                AllocateNodes()
                'wc.DownloadString($"act=ship_set_target&ship_id={}&target_type_id={}&target_id={}")
                Exit Sub
            End If
        End If
        Try
            Dim a As Ellipse
            Dim b As String
            Dim c As Integer
            a = TryCast(e.OriginalSource, Ellipse)
            If a Is Nothing Then Exit Sub
            If e.ChangedButton = MouseButton.Left Then
                If a.Name Is Nothing Then Exit Sub
                If a.Name.Length = 0 Then Exit Sub
                b = a.Name.Chars(0)
                c = Integer.Parse(Mid(a.Name, 2))
                If b = "s" Or b = "m" Then

                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = selectedShipID And child.type = 2 Then
                            child.UnDrawSelect()
                            ' zoom.ScrollIntoView(child.Bounds)
                            '    Dim a As Object
                            '    rectZoom.S = child.Bounds
                            '    rectZoom.ZoomSelection = True
                        End If
                    Next
                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = c AndAlso child.type = 2 Then
                            child.DrawSelect()
                            selectedShipID = c
                        End If
                    Next
                    For Each item As ShipsData In ShipsView.Items
                        If item.id = selectedShipID Then
                            ShipsView.SelectedItem = item
                            ShipsView.ScrollIntoView(item)
                        End If
                        If item.state = 2 Then
                            For Each child As TestShape In grid.VirtualChildren
                                If child.ID = item.id AndAlso child.type = 10 Then
                                    child.RenewCross(item, ShipsView.SelectedItem)
                                End If
                            Next
                        End If
                    Next
                End If
                If b = "p" Or b = "l" Then

                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = selectedPlanetID And child.type = 1 Then
                            child.UnDrawSelect()
                            ' zoom.ScrollIntoView(child.Bounds)
                            '    Dim a As Object
                            '    rectZoom.S = child.Bounds
                            '    rectZoom.ZoomSelection = True
                        End If
                    Next
                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = c AndAlso child.type = 1 Then
                            child.DrawSelect()
                            selectedPlanetID = c
                        End If
                    Next
                    For Each item As GalaxyData In PlanetsView.Items
                        If item.id = selectedPlanetID Then
                            PlanetsView.SelectedItem = item
                            PlanetsView.ScrollIntoView(item)
                        End If
                    Next
                End If
                If b = "b" Then

                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = selectedBuildingID And child.type = 5 Then
                            child.UnDrawSelect()
                            ' zoom.ScrollIntoView(child.Bounds)
                            '    Dim a As Object
                            '    rectZoom.S = child.Bounds
                            '    rectZoom.ZoomSelection = True
                        End If
                    Next
                    For Each child As TestShape In grid.VirtualChildren
                        If child.ID = c AndAlso child.type = 5 Then
                            child.DrawSelect()
                            selectedBuildingID = c
                        End If
                    Next
                    For Each item As GalaxyData In BuildingsView.Items
                        If item.id = selectedBuildingID Then
                            BuildingsView.SelectedItem = item
                            BuildingsView.ScrollIntoView(item)
                        End If
                    Next
                End If
            End If
        Catch
            Debug.Print("хуй")
        End Try
    End Sub

    Private Function RotXY(ByVal P0 As Point, ByVal alpha As Double) As Point
        Dim c As Double, s As Double, rx As Double, ry As Double, NewPoint As New Point
        Dim x As Double, y As Double
        x = P0.X - 40
        y = P0.Y - 40
        rx = P0.X - x - 20
        ry = P0.Y - y - 20
        c = Math.Cos(alpha + Math.PI)
        s = Math.Sin(alpha + Math.PI)
        NewPoint.X = x + rx * c - ry * s
        NewPoint.Y = y + rx * s + ry * c
        Return NewPoint
    End Function

    Private Sub Window1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.F1 Then
            Process.Start(fileLog)
        End If
    End Sub
    Public Function DrawLine(gdata As GalaxyData) As Canvas
        Dim a As New Line
        Dim clan_color As String
        Dim canvas As New Canvas
        Dim b As New Ellipse
        Dim story As New Animation.DoubleAnimation
        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        With a
            .X1 = 0
            .Y1 = 0
            .X2 = -gdata.x + gdata.target_x
            .Y2 = +gdata.y - gdata.target_y
            .Stroke = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
            .StrokeThickness = 4
            .StrokeDashArray.Add(7)
            .StrokeDashArray.Add(3)
            .StrokeDashCap = PenLineCap.Triangle
            '.SnapsToDevicePixels = True
            '.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased)
        End With
        story.From = 0
        story.To = -10
        story.Duration = TimeSpan.FromSeconds(2)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        a.BeginAnimation(Line.StrokeDashOffsetProperty, story)


        With b
            .Height = 10
            .Width = 10
            .Fill = Brushes.AntiqueWhite
            .Visibility = Visibility.Hidden
        End With
        'a.StrokeEndLineCap = 20
        canvas.Height = Math.Abs(a.X2 - a.X1)
        canvas.Width = Math.Abs(a.Y2 - a.Y1)
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(a)
        canvas.Children.Add(b)
        ' canvas.Children.Add(aMask)
        Canvas.SetZIndex(a, 2)
        Canvas.SetZIndex(b, 3)
        Return canvas
    End Function
    Private Function DrawPath() As Canvas
        Dim can As New Canvas
        Dim a As New Line
        Dim b As New TextBlock
        Dim c As New Ellipse
        With a
            .X1 = 0
            .Y1 = 0
            .X2 = 0
            .Y2 = 0
            .Stroke = Brushes.Crimson
            .StrokeThickness = 4
            .StrokeDashArray.Add(7)
            .StrokeDashArray.Add(3)
            .StrokeDashCap = PenLineCap.Triangle
            .Visibility = Visibility.Hidden
            '.SnapsToDevicePixels = True
            '.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased)
        End With

        With b
            .Height = 100
            .Width = 100
            .Background = Brushes.Transparent
            .Text = String.Format(strTimeCords, a.X2, a.Y2, 0, 0, 0)
            .Visibility = Visibility.Hidden
            .FontSize = 12
            .Foreground = Brushes.Crimson
        End With
        Canvas.SetBottom(b, a.X2 - 10)
        Canvas.SetLeft(b, a.X2 - 10)
        Canvas.SetZIndex(b, 1)


        With c
            .Height = 20
            .Width = 20
            .Stroke = Brushes.Crimson
            .StrokeThickness = 2
            .StrokeDashArray.Add(10)
            .StrokeDashArray.Add(5)
            .Visibility = Visibility.Hidden
        End With
        Canvas.SetBottom(c, a.X2 - 10)
        Canvas.SetLeft(c, a.X2 - 10)
        Canvas.SetZIndex(c, 1)
        Dim story As Animation.DoubleAnimation
        story = New Animation.DoubleAnimation
        story.From = 0
        story.To = 30
        story.Duration = TimeSpan.FromSeconds(2)
        story.RepeatBehavior = Animation.RepeatBehavior.Forever
        c.BeginAnimation(Line.StrokeDashOffsetProperty, story)
        can.Height = Math.Abs(a.X2 - a.X1)
        can.Width = Math.Abs(a.Y2 - a.Y1)
        can.Background = Brushes.Transparent
        can.Children.Add(a)
        can.Children.Add(b)
        can.Children.Add(c)
        ' canvas.Children.Add(aMask)
        Canvas.SetZIndex(a, 1)
        Return can
    End Function

    Private Sub Graph_MouseMove(sender As Object, e As MouseEventArgs) Handles Graph.MouseMove
        mousepos = e.GetPosition(Graph)
        If Navigating Then
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            a.UpdateCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack)

        End If
    End Sub

    Private Sub Graph_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If _attack = False AndAlso e.KeyboardDevice.Modifiers = ModifierKeys.Shift AndAlso e.Key = Key.A Then
            _attack = True
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            For Each item As ShipsData In ShipsView.Items
                If item.id = selectedShipID Then
                    _speed = item.speed
                    a.StartPath(New Point(item.x + 1700 / 2, -item.y + 1300 / 2), _attack)
                    a.UpdateCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack)
                End If
            Next
        End If
        If e.Key = Key.LeftShift And Navigating = False Then
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            Navigating = True
            For Each item As ShipsData In ShipsView.Items
                If item.id = selectedShipID Then
                    _speed = item.speed
                    a.StartPath(New Point(item.x + 1700 / 2, -item.y + 1300 / 2), _attack)
                    a.UpdateCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack)
                End If
            Next
        End If
    End Sub

    Private Sub Graph_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.LeftShift And Navigating = True Then
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            Navigating = False
            _attack = False
            a.FinishPath()
        End If
    End Sub

    Private Sub BoxAdmirals_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles BoxAdmirals.SelectionChanged
        If BoxAdmirals.SelectedItem IsNot Nothing Then
            wc.Headers.Item(HttpRequestHeader.Cookie) = AdmiralsList(BoxAdmirals.SelectedItem)
            Timer1_Tick(Nothing, Nothing)
        End If
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        'act=commit_suicide
        'act=character_exit
        'act=character_enter&target_type_id=2&target_id=698
        'act=rename_submit&obj_type_id=2&obj_id=706&new_name=Suicide+Watch
        'http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id=89&ship_spec_id=3&pay_sum=350
        Dim a As Button, s As String = "", gwpack As GWClass
        a = TryCast(sender, Button)
        If a Is Nothing Then Exit Sub
        Select Case a.Content
            Case "Respawn"
                Dim gdata As GalaxyData
                gdata = BoxRespawn.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Attempting to respawn on {gdata}...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=set_home&planet_id={gdata.id}")
            Case "Enter ship"
                Dim gdata As ShipsData
                gdata = BoxEnterShip.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Entering {gdata}...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_enter&target_type_id={2}&target_id={gdata.id}")
            Case "Enter building"
                Dim gdata As GalaxyData
                gdata = BoxEnterBuilding.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Entering {gdata}...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_enter&target_type_id={5}&target_id={gdata.id}")
            Case "Leave"
                Logger.LogWrite($"Attempting to leave ship\building...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_exit")

            Case "Buy ship"
                Dim gdata As GalaxyData
                Dim price As Integer
                gdata = BoxBuyShip.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Attempting to buy ship type [{BoxBuyType.SelectedItem}] on starport [{gdata}]...")
                If BoxBuyType.SelectedItem Is Nothing Then Exit Sub
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum=10")
                gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
                Dim msg As MsgBoxResult
                msg = MsgBox($"Are you sure want to buy {BoxBuyType.SelectedItem} for {gwpack.cost} CUAG?", vbOKCancel, "Admirals")
                If msg = MsgBoxResult.Ok Then
                    s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum={gwpack.cost}")
                Else
                    Logger.LogWrite($"Canceled")
                    Exit Sub
                End If

            Case "Buy building"
                Dim gdata As GalaxyData
                Dim price As Integer
                gdata = BoxBuyBuilding.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Attempting to buy building type [{BoxBuyBType.SelectedItem}] on planet [{gdata}]...")
                If BoxBuyBType.SelectedItem Is Nothing Then Exit Sub
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={BuildingTypes(BoxBuyBType.SelectedItem)}&pay_sum=100")
                gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
                Dim msg As MsgBoxResult
                msg = MsgBox($"Are you sure want to buy {BoxBuyBType.SelectedItem} for {gwpack.cost} CUAG?", vbOKCancel, "Admirals")
                If msg = MsgBoxResult.Ok Then
                    s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={BuildingTypes(BoxBuyBType.SelectedItem)}&pay_sum={gwpack.cost}")
                Else
                    Logger.LogWrite($"Canceled")
                    Exit Sub
                End If

            Case "Rename"
                Dim gdata As ShipsData
                gdata = BoxRenameShip.SelectedItem
                If gdata Is Nothing Then Exit Sub
                Logger.LogWrite($"Attempting to rename ship {gdata} to [{txtShipName.Text}]...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=rename_submit&obj_type_id=2&obj_id={gdata.id}&new_name={Uri.EscapeUriString(txtShipName.Text)}")

            Case "Die!"
                Logger.LogWrite($"Attempting to kill {BoxAdmirals.SelectedItem}...")
                s = wc.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=commit_suicide")
        End Select

        gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
        If gwpack.code = "ERROR" Then
            Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
        Else
            Logger.LogWrite($"Code: [{gwpack.code}]")
        End If
        'If gwpack.code = "OK" Then
        '    For Each MapObject As GalaxyData In gwpack.galaxy_data
        '        If MapObject.type = 2 Then

        '            For Each item As GalaxyData In GWMap.galaxy_data
        '                If item.id = MapObject.id AndAlso item.type = MapObject.type Then
        '                    item.planet_id = MapObject.planet_id
        '                    item.x = MapObject.x
        '                    item.name = MapObject.name
        '                    item.y = MapObject.y
        '                    item.target_x = MapObject.target_x
        '                    item.target_y = MapObject.target_y
        '                    item.state = MapObject.state
        '                    item.members_count = MapObject.members_count
        '                End If
        '            Next
        '            For Each item As ShipsData In GWFleet
        '                If item.id = MapObject.id AndAlso item.type = MapObject.type Then
        '                    item.planet_id = MapObject.planet_id
        '                    item.x = MapObject.x
        '                    item.y = MapObject.y
        '                    item.name = MapObject.name
        '                    item.target_x = MapObject.target_x
        '                    item.target_y = MapObject.target_y
        '                    item.state = MapObject.state
        '                    item.members_count = MapObject.members_count
        '                End If
        '            Next
        '        End If
        '    Next
        '    AllocateNodes()
        'End If
    End Sub
    Public Sub UpdatePLayerTab()
        txtClan.Text = GWMap.GetClanFullName(GWPLayer.clan_id)
        txtClan.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(GWMap.GetClanColor(GWPLayer.clan_id)))
        txtHealth.Text = GWPLayer.health
        Select Case GWPLayer.obj_type_id
            Case 2
                txtLocation.Text = GWMap.GetPlanetName(GWPLayer.obj_id)
            Case 5
                txtLocation.Text = GWMap.GetBuildingName(GWPLayer.obj_id)
            Case 1
                txtLocation.Text = GWMap.GetShipName(GWPLayer.obj_id)
        End Select
        txtMoney.Text = GWPLayer.money & " CUAG"
        If GWPLayer.state = 1 Then
            txtState.Text = "Alive"
            txtState.Foreground = Brushes.Green
        Else
            txtState.Text = "Dead"
            txtState.Foreground = Brushes.Black
        End If
    End Sub

    Private Sub txtLocation_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles txtLocation.MouseUp
        Select Case GWPLayer.obj_type_id
            Case 1
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = selectedPlanetID Then
                        child.UnDrawSelect()
                        ' zoom.ScrollIntoView(child.Bounds)
                        '    Dim a As Object
                        '    rectZoom.S = child.Bounds
                        '    rectZoom.ZoomSelection = True
                    End If
                Next
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = GWPLayer.obj_id Then
                        child.DrawSelect()
                        zoom.ScrollIntoView(child.Bounds)
                        Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                        a.Inflate(New Size(200, 200))
                        zoom.ZoomToRect(a)
                        selectedPlanetID = GWPLayer.obj_id
                    End If
                Next



            Case 2
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = selectedShipID Then
                        child.UnDrawSelect()
                        ' zoom.ScrollIntoView(child.Bounds)
                        '    Dim a As Object
                        '    rectZoom.S = child.Bounds
                        '    rectZoom.ZoomSelection = True
                    End If
                Next
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = GWPLayer.obj_id Then
                        child.DrawSelect()
                        zoom.ScrollIntoView(child.Bounds)
                        Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                        a.Inflate(New Size(200, 200))
                        zoom.ZoomToRect(a)
                        selectedShipID = GWPLayer.obj_id
                    End If
                Next



            Case 5
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = selectedBuildingID Then
                        child.UnDrawSelect()
                        ' zoom.ScrollIntoView(child.Bounds)
                        '    Dim a As Object
                        '    rectZoom.S = child.Bounds
                        '    rectZoom.ZoomSelection = True
                    End If
                Next
                For Each child As TestShape In grid.VirtualChildren
                    If child.ID = GWPLayer.obj_id Then
                        child.DrawSelect()
                        zoom.ScrollIntoView(child.Bounds)
                        Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                        a.Inflate(New Size(200, 200))
                        zoom.ZoomToRect(a)
                        selectedBuildingID = GWPLayer.obj_id
                    End If
                Next
        End Select
    End Sub

    Public Sub ShowOnlyMyFilter(ByVal sender As Object, ByVal e As FilterEventArgs)
        Dim product As ShipsData = TryCast(e.Item, ShipsData)

        If product IsNot Nothing Then

            If product.owner_id = GWPLayer.id Then
                e.Accepted = True
            Else
                e.Accepted = False
            End If
        Else
            Dim product1 As GalaxyData = TryCast(e.Item, GalaxyData)
            If product1 IsNot Nothing Then
                If product1.owner_character_id = GWPLayer.id Then
                    e.Accepted = True
                Else
                    e.Accepted = False
                End If
            End If
        End If


    End Sub

    Private Sub MyShipsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MyShipsView.SelectionChanged
        If MyShipsView.SelectedItem IsNot Nothing Then ShipsView.SelectedItem = MyShipsView.SelectedItem
    End Sub

    Private Sub MyBuildingsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MyBuildingsView.SelectionChanged
        If MyBuildingsView.SelectedItem IsNot Nothing Then BuildingsView.SelectedItem = MyBuildingsView.SelectedItem
    End Sub
End Class



