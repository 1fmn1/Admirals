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
Imports System.Collections.ObjectModel

Public Class Window1
    Const fileNotable As String = "Notable PLayers.txt"
    Dim a As String
    Dim WithEvents timer1 As New Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 8)}
    Dim WithEvents timer2 As New Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 5)}
    'Dim tskBar As Shell.TaskbarManager
    Dim MovingShipscounter As Integer
    Dim pattern As String = "mid=\d*"
    Public MyShips As New CollectionViewSource
    Public MyBuildings As New CollectionViewSource
    'Dim Stars As New Canvas
    'Dim ObjectImages As New List(Of Image)
    'Dim img(10) As BitmapImage
    Dim mousepos As Point
    Private logvisible As Boolean = True
    Private _speed As Integer
    Private _attack As Boolean
    Private _target As Integer
    Private Navigating As Boolean
    Private pan As Pan
    Private rectZoom As RectangleSelectionGesture
    Private autoScroll As AutoScroll
    Private _showGridLines As Boolean
    Private _animateStatus As Boolean = True
    Private _tileWidth As Double = 50
    Private _tileHeight As Double = 50
    Private _tileMargin As Double = 10
    Private _totalVisuals As Integer = 0
    Private rows As Integer = 30
    Private cols As Integer = 30



    Public Sub New(filename As String)

        ' Этот вызов является обязательным для конструктора.
        InitializeComponent()

        ' Добавить код инициализации после вызова InitializeComponent().
        Try
            PreviewSave = True
            Dim reader As New StreamReader(filename)
            GWJSon = reader.ReadToEnd()

        Catch ex As Exception
            MsgBox("Fatal Error: " & ex.Message)
        End Try
    End Sub
    Public Sub New()
        InitializeComponent()

    End Sub

    Private Async Sub Window1_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded


        Timeline.DesiredFrameRateProperty.OverrideMetadata(GetType(Timeline), New FrameworkPropertyMetadata With {
                                                                                        .DefaultValue = 40})
        Try
            GWScripts.Load()
            ShipsView.DataContext = GWFleet
            PlanetsView.DataContext = GWPlanets
            BuildingsView.DataContext = GWBuildings
            ScriptsView.DataContext = GWScripts.MyScripts
            BattlesView.DataContext = GWBattles

            ShipsVieww = ShipsView
            PlanetsVieww = PlanetsView
            BuildingsVieww = BuildingsView
            ScriptsVieww = ScriptsView
            ShipInfoo = ShipInfo
            PlanetInfoo = PlanetInfo
            BuildingInfoo = BuildingInfo
            BattleInfoo = BattleInfo
            txtLog.DataContext = logString
            txtLogGlobal = txtLog
            grid = Graph
            Scrollerr = Scroller
            txtState.DataContext = GWPLayer
            grid.SmallScrollIncrement = New Size(_tileWidth + _tileMargin, _tileHeight + _tileMargin)
            grid.ContentCanvas.CacheMode = Nothing
            Dim target As Canvas = grid.ContentCanvas
            zoom = New MapZoom(target)
            zoom.Zoom = My.Settings.Zoom
            zoom.Offset = My.Settings.Offset
            Dim v As Object = Scroller.GetValue(ScrollViewer.CanContentScrollProperty)
            pan = New Pan(target, zoom)
            rectZoom = New RectangleSelectionGesture(target, zoom, ModifierKeys.Control)
            rectZoom.ZoomSelection = False
            autoScroll = New AutoScroll(target, zoom)
            AddHandler zoom.ZoomChanged, New EventHandler(AddressOf OnZoomChanged)
            AddHandler grid.Scale.Changed, New EventHandler(AddressOf OnScaleChanged)
            AddHandler grid.Translate.Changed, New EventHandler(AddressOf OnScaleChanged)
            AddHandler ShipsView.SelectionChanged, New SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler PlanetsView.SelectionChanged, New SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler BuildingsView.SelectionChanged, New SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler BattlesView.SelectionChanged, New SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler PlanetInfo.SelectionChanged, New PlanetInfoControl.SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler BuildingInfo.SelectionChanged, New BuildingInfoControl.SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            AddHandler BattleInfo.SelectionChanged, New BattleInfoControl.SelectionChangedEventHandler(AddressOf OnSelectionChanged)
            Try
                Dim b As New BitmapImage
                b.BeginInit()
                b.UriSource = (New Uri($"pack://siteoforigin:,,,/Assets/stars.png"))
                b.CacheOption = BitmapCacheOption.OnLoad
                b.EndInit()

                grid.Background = New ImageBrush(b)
            Catch
                Dim myimg As New BitmapImage
                myimg.BeginInit()
                myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/stars.jpg", UriKind.Absolute)
                myimg.CacheOption = BitmapCacheOption.OnLoad
                myimg.EndInit()
                SaveImage(myimg, "stars")

                grid.Background = New ImageBrush With {.ImageSource = myimg}
            End Try
            ShipTypes.Add("x_wing", New ShipTypeClass With {.type_name = "x_wing", .type_id = 4, .speed = 9, .cost = 400, .construction_time = 5400})
            ShipTypes.Add("corvette", New ShipTypeClass With {.type_name = "corvette", .type_id = 5, .speed = 7, .cost = 2400, .construction_time = 22000})
            ShipTypes.Add("nova-courier", New ShipTypeClass With {.type_name = "nova-courier", .type_id = 3, .speed = 8, .cost = 700, .construction_time = 10000})
            ShipTypes.Add("Neutral_Fury", New ShipTypeClass With {.type_name = "Neutral_Fury", .type_id = 6, .speed = 6, .cost = 200, .construction_time = 22000})
            ShipTypes.Add("Neutral_Infiltrator", New ShipTypeClass With {.type_name = "Neutral_Infiltrator", .type_id = 7, .speed = 7, .cost = 400, .construction_time = 12000})
            ShipTypes.Add("Neutral_Zygerrian", New ShipTypeClass With {.type_name = "Neutral_Zygerrian", .type_id = 8, .speed = 5, .cost = 600, .construction_time = 16000})
            BuildingTypes.Add("Headquaters", 1)
            BuildingTypes.Add("Starport", 2)
            StateTypes.Add(1, "idle")
            StateTypes.Add(2, "moving")
            StateTypes.Add(0, "building")
            StateTypes.Add(3, "in battle")
            StateTypes.Add(4, "in battle")
            StateTypes.Add(5, "in battle")
            StateTypes.Add(99, "destroyed")



            Dim files() As String = System.IO.Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory & "\Assets\", "*.png", System.IO.SearchOption.AllDirectories)
            For Each fil In files
                MyBrushes.Add(IO.Path.GetFileNameWithoutExtension(fil), New ImageBrush With {.ImageSource = New BitmapImage(New Uri(fil))})
            Next

            BuildingInfo.Visibility = Visibility.Collapsed
            ShipInfo.Visibility = Visibility.Collapsed
            PlanetInfo.Visibility = Visibility.Collapsed
            BattleInfo.Visibility = Visibility.Collapsed
            For Each keyval As KeyValuePair(Of String, ImageBrush) In MyBrushes
                keyval.Value.Freeze()
            Next
        Catch ex As Exception
            MsgBox("Fatal Error: " & ex.Message & " Try deleting Assets directory.")
        End Try
        Try
            AddNotablePlayers()
            Dim profiles As String
            For Each profile As String In IO.Directory.GetFiles("Profiles/", "*.admiral", IO.SearchOption.TopDirectoryOnly)
                profiles = File.ReadAllText(profile)
                If profile = "" Or profiles = "" Then Continue For
                profile = CheckNotable(Mid(Regex.Match(profiles, pattern).Value, 5))
                If AdmiralsList.ContainsKey(profile) Then Continue For
                AdmiralsList.Add(profile, profiles)
                GWAdmirals.Add(New GalaxyData With {
                            .id = Mid(Regex.Match(profiles, pattern).Value, 5),
                            .name = Trim(profile),
                            .permission = False,
                            .type = 4,
                            .clan_id = 113
                            })
            Next
            If GWAdmirals.Count = 0 Then
                profiles = "mid=2"
                File.WriteAllText("Profiles/default.admiral", profiles)
                AdmiralsList.Add("default", profiles)
                GWAdmirals.Add(New GalaxyData With {
                            .id = Mid(Regex.Match(profiles, pattern).Value, 5),
                            .name = "default",
                            .permission = False,
                            .type = 4,
                            .clan_id = 113
                            })
            End If
            BoxxAdmirals = BoxAdmirals
            BoxxAdmirals.DataContext = AdmiralsList.Keys
            CreateMenus()
            'Graph.ContextMenu = mapMenu
            Logger.init(Me)
            'ARefreshGWData()
            If PreviewSave = True Then
                DrawMap(GWJSon)
                Me.WindowState = WindowState.Maximized
                Dim scaleX As Double = grid.ViewportWidth / grid.Extent.Width
                Dim scaleY As Double = grid.ViewportHeight / grid.Extent.Height
                zoom.Zoom = Math.Min(scaleX, scaleY)
                zoom.Offset = New Point(0, 0)
                Logger.LogWrite("Offline Mode.")
            End If
            timer1.IsEnabled = True
            timer2.IsEnabled = True
            'AllocateNodes()
            BoxRespawn.DataContext = GWPlanets
            If BoxxAdmirals.Items.Count > 0 Then
                BoxxAdmirals.SelectedItem = AdmiralsList.Keys(0)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    'Private Sub OnInfoSelectionChanged(gdata As GalaxyData)
    '    Select Case gdata.type
    '        Case 1

    '        Case 2
    '            For Each item As ShipsData In ShipsView.Items
    '                If item.id = gdata.id Then
    '                    ShipsView.SelectedItem = item
    '                End If
    '            Next
    '        Case 5
    '            For Each item As GalaxyData In BuildingsView.Items
    '                If item.id = gdata.id Then
    '                    BuildingsView.SelectedItem = item
    '                End If
    '            Next
    '    End Select
    'End Sub

    Private Sub OnSaveLog(ByVal sender As Object, ByVal e As RoutedEventArgs)
        MessageBox.Show("You need to build the assembly with 'DEBUG_DUMP' to get this feature")
    End Sub

    Private Sub OnScaleChanged(ByVal sender As Object, ByVal e As EventArgs)
        'Dim t As Double = CSharpImpl.__Assign(_gridLines.StrokeThickness, 0.3 / grid.Scale.ScaleX)
        'grid.Backdrop.BorderThickness = New Thickness(t)
    End Sub

    Private lastTick As Integer = Environment.TickCount
    Private addedPerSecond As Integer = 0
    Private removedPerSecond As Integer = 0

    Delegate Sub BooleanEventHandler(ByVal arg As Boolean)

    Private Sub ShowQuadTree(ByVal arg As Boolean)
        MessageBox.Show("You need to build the assembly with 'DEBUG_DUMP' to get this feature")
    End Sub

    Private Sub OnRowColChange(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim item As MenuItem = TryCast(sender, MenuItem)
        Dim d As Integer = Integer.Parse(CStr(item.Tag), CultureInfo.InvariantCulture)
        rows = CSharpImpl.__Assign(cols, d)
        'AllocateNodes()
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
                'Dim border As Canvas = grid.Backdrop
                'border.BorderBrush = Brushes.Blue
                'border.BorderThickness = New Thickness(0.3)
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

    Private Async Sub Timer2_Tick(sender As Object, e As EventArgs) Handles timer2.Tick
        If PreviewSave = True Then Exit Sub
        For Each gdata As ShipsData In GWFleet
            If gdata.state = 2 Then
                Dim a As Vector, time As Double, s1 As Double
                a = New Vector(gdata.target_x - gdata.x, gdata.target_y - gdata.y)
                s1 = Math.Sqrt(Math.Pow((gdata.target_x - gdata.x), 2) + Math.Pow((gdata.target_y - gdata.y), 2))
                time = s1 / gdata.speed
                a = a / (time * 60 * 12)
                gdata.x += a.X
                gdata.y += a.Y
                gdata.Renew()
                Try
                    If IsNothing(gdata.cnvShape) = False AndAlso IsNothing(gdata.cnvShape.Visual) = False Then
                        Dim w As Double = gdata.cnvShape.Bounds.Width
                        gdata.cnvShape.Bounds = New Rect(New Point(gdata.x + 1700 / 2 - w / 2, -gdata.y + 1300 / 2 - w / 2), New Size(w, w))
                        Canvas.SetTop(gdata.cnvShape.Visual, gdata.cnvShape.Bounds.Y)
                        Canvas.SetLeft(gdata.cnvShape.Visual, gdata.cnvShape.Bounds.X)
                    End If
                    If IsNothing(gdata.cnvPath) = False AndAlso IsNothing(gdata.cnvPath.Visual) = False Then
                        'gdata.cnvPath.Bounds = New Rect(New Point(gdata.x + 1700 / 2, -gdata.y + 1300 / 2), New Size(1700, 1300))
                        'Canvas.SetTop(gdata.cnvPath.Visual, gdata.cnvPath.Bounds.Y)
                        'Canvas.SetLeft(gdata.cnvPath.Visual, gdata.cnvPath.Bounds.X)
                        Dim b As Canvas, c As Line
                        b = gdata.cnvPath.Visual
                        c = b.Children(0)
                        c.X2 = -gdata.x + gdata.target_x
                        c.Y2 = +gdata.y - gdata.target_y
                    End If
                Catch ex As Exception
                    Debug.Print(ex.Source.ToString & ex.Message)
                End Try
                'Canvas.SetLeft(gdata.cnvShape.Visual, gdata.x + 1700 / 2 - 15)
                'Canvas.SetTop(gdata.cnvShape.Visual, -gdata.y + 1300 / 2 - 15)
                'Canvas.SetLeft(gdata.cnvShape.Visual, +a.X)
                'Canvas.SetBottom(gdata.cnvShape.Visual, +a.Y)
            End If
        Next
        For Each gdata As GalaxyData In GWMap.galaxy_data
            If gdata.state = 2 AndAlso gdata.type = 2 Then
                Dim a As Vector, time As Double, s1 As Double
                a = New Vector(gdata.target_x - gdata.x, gdata.target_y - gdata.y)
                s1 = Math.Sqrt(Math.Pow((gdata.target_x - gdata.x), 2) + Math.Pow((gdata.target_y - gdata.y), 2))
                time = s1 / gdata.speed
                a = a / (time * 60 * 12)
                gdata.x += a.X
                gdata.y += a.Y
            End If
        Next
    End Sub

    Private Async Sub Timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        Dim _pack As String, _isoutdated As Boolean
        'GWScripts.Add(ShipsView.Items(0), 0, "Medivh")
        If PreviewSave = True Then Exit Sub
        Try
            If LastEventId > 0 Then

                _pack = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=galaxy_update&lastEventID={LastEventId}&lastLogEventID=0&currentAnnouncementID=0")
                Dim _lastevents As GWClass
                _lastevents = JsonConvert.DeserializeObject(Of GWClass)(_pack)
                For Each MapObject As GalaxyData In _lastevents.galaxy_data
                    If MapObject.act = "setParam" Then
                        If MapObject.name = "lastEventID" Then
                            _isoutdated = True
                        End If
                    End If
                Next
                If _lastevents IsNot Nothing And _lastevents.galaxy_data.Length > 0 Then
                    For Each MapObject As GalaxyData In _lastevents.galaxy_data
                        If MapObject.act = "setParam" Then
                            If MapObject.name = "lastEventID" Then
                                If LastEventId < CInt(MapObject.value) Then
                                    _isoutdated = True
                                End If
                            End If
                        End If
                    Next
                End If
            Else
                _isoutdated = True
            End If
            If _isoutdated Then
                Await Task.Delay(500)
                ARefreshGWData()
                'AllocateNodes()
                For Each item As ShipsData In ShipsView.Items
                    If item.id = selectedID(item.type) Then
                        ShipsView.SelectedItem = item
                    End If
                    If item.state = 2 Then
                        For Each child As TestShape In grid.VirtualChildren
                            If child.ID = item.id AndAlso child.type = 10 Then
                                child.RenewCrossPoints(item, ShipsView.SelectedItem)
                            End If
                        Next
                    End If
                Next

                For Each item As GalaxyData In PlanetsView.Items
                    If item.id = selectedID(item.type) Then
                        PlanetsView.SelectedItem = item
                    End If
                Next


                For Each item As GalaxyData In BuildingsView.Items
                    If item.id = selectedID(item.type) Then
                        BuildingsView.SelectedItem = item
                    End If
                Next
                If PlanetInfo.Visibility = Visibility.Visible Then
                    PlanetInfo.Update(PlanetsView.SelectedItem)
                End If
                If ShipInfo.Visibility = Visibility.Visible Then
                    ShipInfo.Update(ShipsView.SelectedItem)
                End If
                If BuildingInfo.Visibility = Visibility.Visible Then
                    BuildingInfo.Update(BuildingsView.SelectedItem)
                End If
                If BattleInfo.Visibility = Visibility.Visible Then
                    BattleInfo.Update(BattlesView.SelectedItem)
                End If
            End If
            GWScripts.Process()
        Catch err As Exception
            Logger.LogWrite($"Internal Error: {err.Message}")
        End Try
    End Sub

    Sub AddNotablePlayers()
        Dim helpstring() As String
        Try
            helpstring = File.ReadAllLines(fileNotable)
            For Each a As String In helpstring
                If a.Length > 0 Then
                    NotablePlayers.Add(a.Split("=")(0), a.Split("=")(1))
                End If
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Async Sub ARefreshGWData()
        If PreviewSave = True Then Exit Sub
        WC.Headers.Item(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36 OPR/54.0.2952.71"
        Do While WC.IsBusy
            Await Task.Delay(400)
        Loop
        GWJSon = Await WC.DownloadStringTaskAsync("http://galaxy.xjedi.com/srv/conn.php?act=client_start")
        DrawMap(GWJSon)
    End Sub

    Private Async Sub DrawMap(a As String)
        Dim MObjectID As Integer
        Dim MObjectType As Integer
        Dim sObjectID As Integer
        Dim sObjectType As Integer
        Dim LastEvents As New GWClass
        Dim gwplayers As Integer = 0
        Dim MovingShipscounter As Integer = 0
        'wc.Headers.Item(HttpRequestHeader.Cookie) = "mid=12824; ac=3a8e7b72a0861827fe3d19d9bb51d46a; _ga=GA1.2.836802751.1488024943; b=b; PHPSESSID=s4iemv426vuj62l4ap2hgmkhn2; _gid=GA1.2.538733330.1534191728; camera=-266.5%3B-315"
        If IsNothing(MouseOverObject) = False Then
            MObjectID = MouseOverObject.MapObject.id
            MObjectType = MouseOverObject.MapObject.type
        End If
        If IsNothing(SelectedObject) = False Then
            sObjectID = SelectedObject.MapObject.id
            sObjectType = SelectedObject.MapObject.type
        End If
        GWMap = Nothing
        GWFleet.Clear()
        GWPlanets.Clear()
        GWBuildings.Clear()
        GWStarports.Clear()
        GWBattles.Clear()
        GWClans.Clear()
        GWClans.Add(New GalaxyData With {.name = "Neutral", .id = 1, .type = 3})
        GWMap = JsonConvert.DeserializeObject(Of GWClass)(a)
        'Dim r As Random = New Random(Environment.TickCount)

        '++++++++++++++++++++++++++++++++++++GRAPHICS++++++++++++++++++++++++++++++++
        grid.VirtualChildren.Clear()
        grid.AddVirtualChild(New TestShape(DrawMouseDirection, New Rect(New Point(0, 0), New Size(1700, 1300)), -1, -1))
        For Each MapObject As GalaxyData In GWMap.galaxy_data

            '=======================Ships=======================
            If MapObject.type = 2 Then
                MapObject.cnvShape = New TestShape(MapObject)
                AddHandler MapObject.cnvShape.Object_Click, New MouseButtonEventHandler(AddressOf ellipse_click)
                AddHandler MapObject.cnvShape.Object_MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
                AddHandler MapObject.cnvShape.Object_MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
                grid.AddVirtualChild(MapObject.cnvShape)
                If MapObject.state = 2 Then
                    Dim line As Canvas = DrawShipDirection(MapObject)
                    MapObject.cnvPath = New TestShape(line, New Rect(New Point(0, 0), New Size(1700, 1300)), 10, MapObject.id)
                    grid.AddVirtualChild(MapObject.cnvPath)
                End If
                GWFleet.Add(New ShipsData(MapObject))
                gwplayers += MapObject.members_count
                If MapObject.state = 2 Then
                    MovingShipscounter += 1
                End If
                '=======================================================
                '=======================Planets=======================
            ElseIf MapObject.type = 1 Then
                MapObject.cnvShape = New TestShape(MapObject)
                AddHandler MapObject.cnvShape.Object_Click, New MouseButtonEventHandler(AddressOf ellipse_click)
                AddHandler MapObject.cnvShape.Object_MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
                AddHandler MapObject.cnvShape.Object_MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
                grid.AddVirtualChild(MapObject.cnvShape)
                GWPlanets.Add(MapObject)
                '=======================================================
                '=======================Clans=======================
            ElseIf MapObject.type = 3 Then
                GWClans.Add(MapObject)
                '=======================================================
                '=======================Players=======================
            ElseIf MapObject.type = 4 Then
                GWPLayer = MapObject
                MyShips.Source = GWFleet
                MyBuildings.Source = GWBuildings
                MyShips.LiveFilteringProperties.Add("Owner_id")
                MyShips.IsLiveFilteringRequested = True
                AddHandler MyShips.Filter, New FilterEventHandler(AddressOf ShowOnlyMyFilter)
                AddHandler MyBuildings.Filter, New FilterEventHandler(AddressOf ShowOnlyMyFilter)
                MyShipsView.DataContext = MyShips
                MyBuildingsView.DataContext = MyBuildings
                UpdatePlayerTab()
                '=======================================================
                '=======================Buildings=======================
            ElseIf MapObject.type = 5 AndAlso MapObject.state < 99 Then
                For Each gwitem As GalaxyData In GWPlanets
                    If MapObject.planet_id = gwitem.id Then
                        MapObject.x = gwitem.x
                        MapObject.y = gwitem.y
                    End If
                Next

                MapObject.cnvShape = New TestShape(MapObject)
                AddHandler MapObject.cnvShape.Object_Click, New MouseButtonEventHandler(AddressOf ellipse_click)
                AddHandler MapObject.cnvShape.Object_MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
                AddHandler MapObject.cnvShape.Object_MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
                grid.AddVirtualChild(MapObject.cnvShape)
                GWBuildings.Add(MapObject)
                If MapObject.image = "starport" Then
                    MapObject.health = 80000
                    GWStarports.Add(MapObject)
                Else
                    MapObject.health = 40000
                End If
                '=======================================================
                '=======================Battles=======================
            ElseIf MapObject.type = 6 Then
                MapObject.cnvShape = New TestShape(MapObject)
                AddHandler MapObject.cnvShape.Object_Click, New MouseButtonEventHandler(AddressOf ellipse_click)
                AddHandler MapObject.cnvShape.Object_MouseEnter, New MouseEventHandler(AddressOf ellipse_mouseenter)
                AddHandler MapObject.cnvShape.Object_MouseLeave, New MouseEventHandler(AddressOf ellipse_mouseleave)
                grid.AddVirtualChild(MapObject.cnvShape)
                If GWBattles.Contains(MapObject, New GalaxyDataEqualityComparer) Then Continue For
                GWBattles.Add(MapObject)
                '=======================================================
                '=======================Params=======================
            ElseIf MapObject.act = "setParam" Then
                If MapObject.name = "lastEventID" Then
                    GWMap.LastEventIDCheck = MapObject.value
                End If
            End If
            '=======================================================
        Next

        If PreviewSave = False Then

            If LastEventId > 0 And GWMap.LastEventIDCheck - LastEventId < 1000 Then
                Do While Not (GWMap.LastEventIDCheck <= LastEventId)
                    'Application.DoEvents()
                    Do While WC.IsBusy
                        Await Task.Delay(400)
                    Loop
                    a = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=galaxy_update&lastEventID={LastEventId}&lastLogEventID=0&currentAnnouncementID=0")
                    LastEvents = JsonConvert.DeserializeObject(Of GWClass)(a)
                    If LastEvents IsNot Nothing And LastEvents.galaxy_data.Length > 0 Then
                        Logger.GetPacket(LastEvents.galaxy_data)
                    End If
                Loop
            Else
                LastEventId = GWMap.LastEventIDCheck
                Logger.LogWrite("Too much events happened since last time.")
            End If
        End If
        'Dim Ura As Uri
        Dim root As New ContentControl
        Dim bmp As RenderTargetBitmap = New RenderTargetBitmap(20, 20, 96, 96, PixelFormats.Default)
        root.ContentTemplate = Resources("OverlayIcon")
        root.Content = MovingShipscounter
        root.Arrange(New Rect(0, 0, 20, 20))
        bmp.Render(root)
        TaskbarItemInfo.Overlay = bmp

        If MObjectType > 0 AndAlso IsNothing(MouseOverObject) = False Then
            Try
                MouseOverObject = GWMap.galaxy_data.Where(Function(c) IsNothing(c.type) = False).First(Function(c) c.id = MObjectID AndAlso c.type = MObjectType).cnvShape
                MouseOverObject.DrawSelect()
            Catch ex As Exception
                Logger.LogWrite($"Error: Selected object no longer exist.")
            End Try
        End If
        If sObjectType > 0 AndAlso IsNothing(SelectedObject) = False Then
            Try
                SelectedObject = GWMap.galaxy_data.Where(Function(c) IsNothing(c.type) = False).First(Function(c) c.id = MObjectID AndAlso c.type = sObjectType).cnvShape
                SelectedObject.DrawSelect()
            Catch ex As Exception
                Logger.LogWrite($"Error: Selected object no longer exist.")
            End Try
        End If


        Dim dick As New Dictionary(Of Short, Short)
        For Each st As GalaxyData In GWStarports
            If st.id = 3 Or st.id = 14 Or st.id = 12 Or st.id = 13 Then Continue For
            dick.Add(st.id, 0)
        Next
        For Each item As ShipsData In GWFleet.Where(Function(c) IsNothing(c.state) = False AndAlso c.state = 0 AndAlso c.type = 2)
            If dick.ContainsKey(item.target_id) = False Then Continue For
            item.construct_order = dick(item.target_id)
            dick(item.target_id) += 1
        Next

        Me.Title = $"Admirals {Version}  {GWFleet.Count} ships in game with {gwplayers} players. Last event: {LastEventId}. Press F1 to open log."
    End Sub
    Private Overloads Sub OnSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim MyView As ListView = CType(sender, ListView)
        If MyView.SelectedItem Is Nothing Then Exit Sub
        Dim gdata As GalaxyData = MyView.SelectedItem
        If IsNothing(SelectedObject) = False Then
            If gdata.type = SelectedObject.MapObject.type AndAlso gdata.id = SelectedObject.MapObject.id Then Exit Sub
        End If
        'For Each child As TestShape In grid.VirtualChildren
        '    If child.ID = selectedID(gdata.type) AndAlso child.type = gdata.type Then
        '        child.UnDrawSelect()
        '    End If
        'Next
        If IsNothing(SelectedObject) = False Then
            SelectedObject.UnDrawSelect()
        End If
        BuildingInfo.Visibility = Visibility.Collapsed
        ShipInfo.Visibility = Visibility.Collapsed
        PlanetInfo.Visibility = Visibility.Collapsed
        BattleInfo.Visibility = Visibility.Collapsed
        SelectedObject = gdata.cnvShape
        SelectedObject.DrawSelect()
        'zoom.ScrollIntoView(child.Bounds)
        Dim a As New Rect(SelectedObject.Bounds.Location, SelectedObject.Bounds.Size)
        a.Inflate(New Size(200, 200))
        zoom.ZoomToRect(a)
        selectedID(gdata.type) = gdata.id
        If PreviewSave Then Exit Sub
        Select Case gdata.type
            Case 1
                PlanetInfo.Update(gdata)
            Case 2
                ShipInfo.Update(GWFleet.First(Function(c) c.id = gdata.id))
            Case 5
                BuildingInfo.Update(gdata)
            Case 6
                BattleInfo.Update(gdata)
        End Select
    End Sub

    Public Overloads Sub OnSelectionChanged(gdata As GalaxyData)
        Try
            If IsNothing(SelectedObject) = False Then
                If gdata.type = SelectedObject.MapObject.type AndAlso gdata.id = SelectedObject.MapObject.id Then Exit Sub
            End If
            'For Each child As TestShape In grid.VirtualChildren
            '    If child.ID = selectedID(gdata.type) AndAlso child.type = gdata.type Then
            '        child.UnDrawSelect()
            '    End If
            'Next
            If IsNothing(SelectedObject) = False Then
                SelectedObject.UnDrawSelect()
            End If
            BuildingInfo.Visibility = Visibility.Collapsed
            ShipInfo.Visibility = Visibility.Collapsed
            PlanetInfo.Visibility = Visibility.Collapsed
            BattleInfo.Visibility = Visibility.Collapsed
            Try
                SelectedObject = gdata.cnvShape
                SelectedObject.DrawSelect()
                Dim a As New Rect(SelectedObject.Bounds.Location, SelectedObject.Bounds.Size)
                a.Inflate(New Size(200, 200))
                zoom.ZoomToRect(a)
                selectedID(gdata.type) = gdata.id
            Catch ex As Exception
                Logger.LogWrite("Error: Object does not exist on map.")
            End Try
            'zoom.ScrollIntoView(child.Bounds)

            If PreviewSave Then Exit Sub
                Select Case gdata.type
                    Case 1
                        PlanetInfo.Update(gdata)
                    Case 2
                        ShipInfo.Update(GWFleet.First(Function(c) c.id = gdata.id))
                    Case 5
                        BuildingInfo.Update(gdata)
                    Case 6
                        BattleInfo.Update(gdata)
                End Select
            Catch ex As Exception
                Logger.LogWrite("Error: OnSelection.")
        End Try

    End Sub


    Sub ExitProgram()
        GWScripts.Save()
        If Me.WindowState <> WindowState.Minimized Then
            If Me.Height < 100 Then
                My.Settings.Height = 100
            End If
            If Me.Height < 100 Then
                My.Settings.Width = 100
            End If
            My.Settings.Zoom = zoom.Zoom
            My.Settings.Offset = zoom.Offset
            My.Settings.Save()
        End If
        End
    End Sub


    Private Sub ellipse_mouseenter(sender As Object, e As MouseEventArgs)
        'MouseOverObject = sender
        'MouseOverObject.DrawSelect()

        'Dim P As New Point(mousepos.X / zoom.Zoom - 10 + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom - 10 + Scroller.VerticalOffset / zoom.Zoom)
        'Dim a As New List(Of TestShape)

        'For Each b In Graph.GetChildrenIntersecting(New Rect(P, New Size(20, 20)))
        '    b = TryCast(b, TestShape)
        '    If IsNothing(b) = False Then a.Add(b)
        'Next
        'For i = a.Count - 1 To 0 Step -1
        '    If IsNothing(a(i).MapObject) Then a.RemoveAt(i) : Continue For
        '    If a(i).MapObject.type <> 2 Then a.RemoveAt(i)
        'Next
        ''hitresultslist.Clear()
        ''VisualTreeHelper.HitTest(Graph, Nothing, New HitTestResultCallback(AddressOf MyHitTestResult),
        ''New PointHitTestParameters(P))
        'If a.Count = 0 Then Exit Sub
        'a.Sort(Function(x, y) Math.Sqrt(Math.Pow((x.Bounds.X - P.X), 2) + Math.Pow((x.Bounds.Y - P.Y), 2)) <
        '    Math.Sqrt(Math.Pow((y.Bounds.X - P.X), 2) + Math.Pow((y.Bounds.Y - P.Y), 2)))
        'Debug.Print($"{a.Count} {a.First.MapObject.name}")
        'a.First.DrawSelect()
        ''CType(sender, Ellipse).Stroke = Brushes.White
        ''CType(sender, Ellipse).Stroke.Opacity += 0.5
    End Sub

    Private Sub ellipse_mouseleave(sender As Object, e As MouseEventArgs)
        'MouseOverObject = sender.UnDrawSelect()
        'Dim a As TestShape = sender
        'a.UnDrawSelect()
        'CType(sender, Ellipse).Stroke.Opacity -= 0.5
        'CType(sender, Ellipse).Stroke = Brushes.Transparent
    End Sub

    Private Sub ellipse_click(sender As Object, e As MouseButtonEventArgs)

    End Sub
    Private Sub Window1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        SaveMap()
        ExitProgram()
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

    Private Function RotXYBattle(ByVal P0 As Point, ByVal alpha As Double) As Point
        Dim c As Double, s As Double, rx As Double, ry As Double, NewPoint As New Point
        Dim x As Double, y As Double
        x = P0.X - 15
        y = P0.Y - 15
        rx = P0.X - x - 7
        ry = P0.Y - y - 7
        c = Math.Cos(alpha)
        s = Math.Sin(alpha)
        NewPoint.X = x + rx * c - ry * s
        NewPoint.Y = y + rx * s + ry * c
        Return NewPoint
    End Function

    Private Sub Window1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Key = Key.F1 Then
            Process.Start(fileLog)
        End If
    End Sub
    Public Function DrawShipDirection(gdata As GalaxyData) As Canvas
        Dim a As New Line With {.IsHitTestVisible = False}
        Dim clan_color As String
        Dim canvas As New Canvas With {.IsHitTestVisible = False}
        Dim b As New Ellipse With {.IsHitTestVisible = False}
        Dim story As New Animation.DoubleAnimation
        clan_color = GWMap.GetClanColor(gdata.clan_id)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        With a
            .X1 = 1700 / 2 + gdata.x
            .Y1 = 1300 / 2 - gdata.y
            .X2 = +gdata.target_x + 1700 / 2
            .Y2 = -gdata.target_y + 1300 / 2
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

        'MapObject.cnvPath = New TestShape(line, New Rect(New Point(MapObject.x + 1700 / 2, -MapObject.y + 1300 / 2), New Size(50, 50)), 10, MapObject.id)
        With b
            .Height = 10
            .Width = 10
            .Fill = a.Stroke
            .Visibility = Visibility.Hidden
        End With

        canvas.Height = Math.Abs(a.X2 - a.X1)
        canvas.Width = Math.Abs(a.Y2 - a.Y1)
        canvas.Background = Brushes.Transparent
        canvas.Children.Add(a)
        canvas.Children.Add(b)
        ' canvas.Children.Add(aMask)
        Canvas.SetZIndex(a, 100)
        Canvas.SetZIndex(b, 2)
        Return canvas
    End Function
    Private Function DrawMouseDirection() As Canvas
        Dim can As New Canvas With {.IsHitTestVisible = False}
        Dim a As New Line With {.IsHitTestVisible = False}
        Dim b As New TextBlock With {.IsHitTestVisible = False}
        Dim c As New Ellipse With {.IsHitTestVisible = False}
        With a
            .X1 = 0
            .Y1 = 0
            .X2 = 0
            .Y2 = 0
            .Stroke = Brushes.WhiteSmoke
            .StrokeThickness = 4
            .StrokeDashArray.Add(7)
            .StrokeDashArray.Add(3)
            .StrokeDashCap = PenLineCap.Triangle
            .Visibility = Visibility.Hidden
        End With

        With b
            .Height = 100
            .Width = 100
            .Background = Brushes.Transparent
            .Text = String.Format(strTimeCords, a.X2, a.Y2, 0, 0, 0)
            .Visibility = Visibility.Hidden
            .FontSize = 12
            .Foreground = Brushes.WhiteSmoke
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
        Canvas.SetZIndex(a, -100)
        Return can
    End Function

    Private Sub Graph_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        If _attack = False AndAlso e.KeyboardDevice.Modifiers = ModifierKeys.Shift AndAlso e.Key = Key.A Then
            _attack = True
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            For Each item As ShipsData In ShipsView.Items
                If item.id = selectedID(2) Then
                    _speed = item.speed
                    a.StartPath(New Point(item.x + 1700 / 2, -item.y + 1300 / 2), _attack)
                    a.DrawCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack)
                End If
            Next
        ElseIf e.Key = Key.LeftShift And Navigating = False Then
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            Navigating = True
            For Each item As ShipsData In ShipsView.Items
                If item.id = selectedID(2) Then
                    _speed = item.speed
                    a.StartPath(New Point(item.x + 1700 / 2, -item.y + 1300 / 2), _attack)
                    a.DrawCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack)
                End If
            Next
        ElseIf e.KeyboardDevice.Modifiers = ModifierKeys.Shift AndAlso e.Key = Key.Oem3 Then
            ShowLog()
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
            WC.Headers.Item(HttpRequestHeader.Cookie) = AdmiralsList(BoxAdmirals.SelectedItem)
            ARefreshGWData()
            'AllocateNodes()
        End If
    End Sub

    Private Async Sub Button_Click(sender As Object, e As RoutedEventArgs)
        'act=commit_suicide
        'act=character_exit
        If PreviewSave Then Exit Sub
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
                s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=set_home&planet_id={gdata.id}")
            'Case "Enter ship"
            '    Dim gdata As ShipsData
            '    gdata = BoxEnterShip.SelectedItem
            '    If gdata Is Nothing Then Exit Sub
            '    Logger.LogWrite($"Entering {gdata}...")
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_enter&target_type_id={2}&target_id={gdata.id}")
            'Case "Enter building"
            '    Dim gdata As GalaxyData
            '    gdata = BoxEnterBuilding.SelectedItem
            '    If gdata Is Nothing Then Exit Sub
            '    Logger.LogWrite($"Entering {gdata}...")
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_enter&target_type_id={5}&target_id={gdata.id}")
            'Case "Leave"
            '    Logger.LogWrite($"Attempting to leave ship\building...")
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=character_exit")

            'Case "Buy ship"
            '    Dim gdata As GalaxyData
            '    Dim price As Integer
            '    gdata = BoxBuyShip.SelectedItem
            '    If gdata Is Nothing Then Exit Sub
            '    Logger.LogWrite($"Attempting to buy ship type [{BoxBuyType.SelectedItem}] on starport [{gdata}]...")
            '    If BoxBuyType.SelectedItem Is Nothing Then Exit Sub
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum=10")
            '    gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            '    Dim msg As MsgBoxResult
            '    msg = MsgBox($"Are you sure want to buy {BoxBuyType.SelectedItem} for {gwpack.cost} CUAG?", vbOKCancel, "Admirals")
            '    If msg = MsgBoxResult.Ok Then
            '        s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum={gwpack.cost}")
            '    Else
            '        Logger.LogWrite($"Canceled")
            '        Exit Sub
            '    End If

            'Case "Buy building"
            '    Dim gdata As GalaxyData
            '    Dim price As Integer
            '    gdata = BoxBuyBuilding.SelectedItem
            '    If gdata Is Nothing Then Exit Sub
            '    Logger.LogWrite($"Attempting to buy building type [{BoxBuyBType.SelectedItem}] on planet [{gdata}]...")
            '    If BoxBuyBType.SelectedItem Is Nothing Then Exit Sub
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={BuildingTypes(BoxBuyBType.SelectedItem)}&pay_sum=100")
            '    gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            '    Dim msg As MsgBoxResult
            '    msg = MsgBox($"Are you sure want to buy {BoxBuyBType.SelectedItem} for {gwpack.cost} CUAG?", vbOKCancel, "Admirals")
            '    If msg = MsgBoxResult.Ok Then
            '        s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={BuildingTypes(BoxBuyBType.SelectedItem)}&pay_sum={gwpack.cost}")
            '    Else
            '        Logger.LogWrite($"Canceled")
            '        Exit Sub
            '    End If

            'Case "Rename"
            '    Dim gdata As ShipsData
            '    gdata = BoxRenameShip.SelectedItem
            '    If gdata Is Nothing Then Exit Sub
            '    Logger.LogWrite($"Attempting to rename ship {gdata} to [{txtShipName.Text}]...")
            '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=rename_submit&obj_type_id=2&obj_id={gdata.id}&new_name={Uri.EscapeUriString(txtShipName.Text)}")

            Case "Die!"
                Logger.LogWrite($"Attempting to kill {BoxAdmirals.SelectedItem}...")
                s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=commit_suicide")
        End Select
        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If

        Catch
            Logger.LogWrite($"Code: [OK] : Respawned.")
            Timer1_Tick(Nothing, Nothing)
            Exit Sub
        End Try






        '=================================================================================        =================================================================================

        '=================================================================================        =================================================================================


        '=================================================================================        =================================================================================

    End Sub
    Public Sub UpdatePlayerTab()
        txtClan.Text = GWMap.GetClanFullName(GWPLayer.clan_id)
        txtClan.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(GWMap.GetClanColor(GWPLayer.clan_id)))
        txtHealth.Value = CInt(GWPLayer.health)
        Dim a As Hyperlink
        Select Case GWPLayer.obj_type_id
            Case 2
                a = New Hyperlink With {.NavigateUri = New Uri("GWMap.GetShipName(GWPLayer.obj_id)", UriKind.Relative)}
                a.Inlines.Add(GWMap.GetShipName(GWPLayer.obj_id))
                AddHandler a.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
                txtLocation.Text = ""
                txtLocation.Inlines.Add(a)
            Case 5
                a = New Hyperlink With {.NavigateUri = New Uri("GWMap.GetShipName(GWPLayer.obj_id)", UriKind.Relative)}
                a.Inlines.Add(GWMap.GetBuildingName(GWPLayer.obj_id))
                a.Foreground = Brushes.Aquamarine
                AddHandler a.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
                txtLocation.Text = ""
                txtLocation.Inlines.Add(a)
            Case 1
                a = New Hyperlink With {.NavigateUri = New Uri("GWMap.GetShipName(GWPLayer.obj_id)", UriKind.Relative)}
                a.Inlines.Add(GWMap.GetPlanetName(GWPLayer.obj_id))
                a.Foreground = Brushes.Aquamarine
                AddHandler a.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
                txtLocation.Text = ""
                txtLocation.Inlines.Add(a)
        End Select
        txtMoney.Text = GWPLayer.money & " CUAG"
        If GWPLayer.state = 1 Then
            txtState.Text = "Alive"
            txtState.Foreground = Brushes.LightGreen
        ElseIf GWPLayer.state = 0 Then
            txtState.Text = $"Ready for respawn"
            txtState.Foreground = Brushes.LightGreen
            txtHealth.Value = 0
        Else
            txtState.Text = $"Respawn in {GWPLayer.state_time_string}"
            txtState.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFD9804"))
            txtHealth.Value = 0
        End If
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
    Sub rnavigate(sender As Object, arg As RequestNavigateEventArgs)

        For Each child As TestShape In grid.VirtualChildren
            If child.ID = selectedID(GWPLayer.obj_type_id) AndAlso child.type = GWPLayer.obj_type_id Then
                child.UnDrawSelect()
                ' zoom.ScrollIntoView(child.Bounds)
                '    Dim a As Object
                '    rectZoom.S = child.Bounds
                '    rectZoom.ZoomSelection = True
            End If
            If child.ID = GWPLayer.obj_id AndAlso child.type = GWPLayer.obj_type_id Then
                child.DrawSelect()
                zoom.ScrollIntoView(child.Bounds)
                Dim a As New Rect(child.Bounds.Location, child.Bounds.Size)
                a.Inflate(New Size(200, 200))
                zoom.ZoomToRect(a)
                selectedID(GWPLayer.obj_type_id) = GWPLayer.obj_id
            End If
        Next
        selectedID(GWPLayer.obj_type_id) = GWPLayer.obj_id
    End Sub

    Private Sub MyShipsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MyShipsView.SelectionChanged
        If MyShipsView.SelectedItem IsNot Nothing Then ShipsView.SelectedItem = MyShipsView.SelectedItem
    End Sub

    Private Sub MyBuildingsView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MyBuildingsView.SelectionChanged
        If MyBuildingsView.SelectedItem IsNot Nothing Then BuildingsView.SelectedItem = MyBuildingsView.SelectedItem
    End Sub

    Private Sub BtnClose_()
        Me.WindowState = WindowState.Minimized
    End Sub

    Private Sub Window1_Is() Handles Me.StateChanged
        If Me.WindowState = WindowState.Minimized Then
            MainGrid.ColumnDefinitions.Item(2).Width = New GridLength(0)
        Else
            MainGrid.ColumnDefinitions.Item(2).Width = New GridLength(1, GridUnitType.Star)
            My.Settings.Save()
        End If
    End Sub
    '================================================================================================================================
    '================                               Context Menus
    '================================================================================================================
    Private Sub CreateMenus()
        mapMenu.Items.Add(Menu_move_to)

    End Sub

    Function Menu_move_to() As MenuItem
        Dim img As New Image With {.Source = New BitmapImage(New Uri("Style/script.png", UriKind.Relative))}
        Dim a As New MenuItem With {.Header = "Move to"}
        a.Icon = img
        a.FontSize = 14
        AddHandler a.Click, New RoutedEventHandler(AddressOf mapMenu_Clicked)
        Return a
    End Function

    Private Sub mapMenu_Clicked(sender As Object, e As RoutedEventArgs)
        Dim a As MenuItem, b As ContextMenu, c As Integer, a1 As Rect, b1 As String
        If IsNothing(MouseOverObject) Then
            GWScripts.Add(SelectedObject.MapObject, 0, 0, 2, BoxAdmirals.SelectedItem, -1,
                      cursorLeft / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom - 1700 / 2, -(cursorTop / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom - 1300 / 2))
        Else
            GWScripts.Add(SelectedObject.MapObject, 0, 0, 2, BoxAdmirals.SelectedItem, -1,
          MouseOverObject.MapObject.x, MouseOverObject.MapObject.y)
        End If
    End Sub

    Private Sub onSRemoveClick(sender As Object, e As RoutedEventArgs)
        Dim a As MenuItem, b As ContextMenu, a1 As ListView
        a = sender
        b = a.Parent
        a1 = b.PlacementTarget
        GWScripts.Remove(a1.SelectedItem)
    End Sub

    Private Sub Graph_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles Graph.ContextMenuOpening
        cursorLeft = e.CursorLeft
        cursorTop = e.CursorTop
    End Sub

    Public Sub ShowLog()
        If logvisible = False Then
            Dim a As New DoubleAnimation
            'txtlog2.Visibility = Visibility.Visible
            a.IsAdditive = False
            a.To = 400
            a.From = 0
            a.Duration = TimeSpan.FromSeconds(0.3)
            a.AccelerationRatio = 0.5
            txtlog2.BeginAnimation(Border.HeightProperty, a)
            logvisible = True
        Else
            Dim a As New DoubleAnimation
            a.To = 0
            a.From = 400
            a.Duration = TimeSpan.FromSeconds(0.3)
            a.IsAdditive = False
            a.AccelerationRatio = 0.5
            txtlog2.BeginAnimation(Border.HeightProperty, a)
            'txtlog2.Visibility = Visibility.Hidden
            logvisible = False
        End If
    End Sub

    Private Async Sub Graph_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles Graph.MouseUp



        If Navigating = True Then
            If PreviewSave Then Exit Sub
            Dim a As TestShape
            a = grid.VirtualChildren(0)
            a.FinishPath()
            If e.ChangedButton = MouseButton.Left Then
                Dim x As Integer, y As Integer, s As String = "", gwPack As GWClass
                If _attack AndAlso e.ChangedButton = MouseButton.Left Then
                    If IsNothing(MouseOverObject) Then Exit Sub
                    Dim c As Integer
                    c = MouseOverObject.MapObject.id
                    Select Case MouseOverObject.MapObject.type
                        Case 1
                            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_type_id={MouseOverObject.MapObject.type}&target_id={c}")
                            Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target To planet [{GWMap.GetPlanetName(c)}] ")
                        Case 2
                            If SelectedObject.MapObject.planet_id > 0 AndAlso SelectedObject.MapObject.planet_id = MouseOverObject.MapObject.planet_id Then
                                s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_attack&source_ship_id={selectedID(2)}&target_obj_type={MouseOverObject.MapObject.type}&target_obj_id={c}")
                                Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to [{GWMap.GetShipType(c)}][{GWMap.GetShipName(c)}] ")
                            Else
                                s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_type_id={MouseOverObject.MapObject.type}&target_id={c}")
                                Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to [{GWMap.GetShipType(c)}][{GWMap.GetShipName(c)}] ")
                            End If
                        Case 5
                            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_attack&source_ship_id={selectedID(2)}&target_obj_type={MouseOverObject.MapObject.type}&target_obj_id={c}")
                            Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to building [{GWMap.GetBuildingName(c)}] ")
                        Case 6
                            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_type_id={MouseOverObject.MapObject.type}&target_id={c}")
                            Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to [Battle {c}][{MouseOverObject.MapObject.name}] ")
                    End Select

                    'Dim a1 As Ellipse
                    'Dim b As String
                    'Dim c As Integer
                    'a1 = TryCast(e.OriginalSource, Ellipse)
                    'If a1 Is Nothing Then Exit Sub
                    'If a1.Name Is Nothing Then Exit Sub
                    'If a1.Name.Length = 0 Then Exit Sub
                    'b = a1.Name.Chars(0)
                    'c = Integer.Parse(Mid(a1.Name, 2))
                    'If b = "s" Or b = "m" Then
                    '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_type_id={2}&target_id={c}")
                    '    Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to [{GWMap.GetShipType(c)}][{GWMap.GetShipName(c)}] ")

                    'ElseIf b = "p" Or b = "l" Then
                    '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_type_id={1}&target_id={c}")
                    '    Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target To planet [{GWMap.GetPlanetName(c)}] ")
                    'ElseIf b = "b" Then
                    '    s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_attack&source_ship_id={selectedID(2)}&target_obj_type={5}&target_obj_id={c}")
                    '    Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target to building [{GWMap.GetBuildingName(c)}] ")
                    'End If
                Else
                    x = mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom - 1700 / 2
                    y = -(mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom - 1300 / 2)
                    s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={selectedID(2)}&target_x={x}&target_y={y}")
                    Logger.LogWrite($"Setting Ship's[{selectedID(2)}] [{GWMap.GetShipName(selectedID(2))}] target point to [{x};{y}]")
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
                Navigating = False
                _attack = False
                'AllocateNodes()
                'wc.DownloadString($"act=ship_set_target&ship_id={}&target_type_id={}&target_id={}")
                Exit Sub
            End If
        Else
            If e.ChangedButton = MouseButton.Left Then
                ' Try
                If IsNothing(MouseOverObject) Then Exit Sub
                If IsNothing(SelectedObject) = False Then
                    SelectedObject.UnDrawSelect()
                End If
                MouseOverObject.DrawSelect()
                SelectedObject = MouseOverObject
                selectedID(MouseOverObject.MapObject.type) = MouseOverObject.MapObject.id
                BuildingInfo.Visibility = Visibility.Collapsed
                ShipInfo.Visibility = Visibility.Collapsed
                PlanetInfo.Visibility = Visibility.Collapsed
                BattleInfo.Visibility = Visibility.Collapsed
                Select Case MouseOverObject.MapObject.type
                    Case 1
                        PlanetsView.SelectedItem = GWPlanets.First(Function(x) x.id = SelectedObject.MapObject.id)
                        PlanetsView.ScrollIntoView(PlanetsView.SelectedItem)
                        If PreviewSave Then Exit Sub
                        PlanetInfo.Update(PlanetsView.SelectedItem)
                    Case 2
                        ShipsView.SelectedItem = GWFleet.First(Function(x) x.id = SelectedObject.MapObject.id)
                        ShipsView.ScrollIntoView(ShipsView.SelectedItem)
                        For Each item As ShipsData In ShipsView.Items
                            If item.state = 2 Then
                                For Each child As TestShape In grid.VirtualChildren
                                    If child.ID = item.id AndAlso child.type = 10 Then
                                        child.RenewCrossPoints(item, ShipsView.SelectedItem)
                                    End If
                                Next
                            End If
                        Next
                        If PreviewSave Then Exit Sub
                        ShipInfo.Update(ShipsView.SelectedItem)
                    Case 5
                        BuildingsView.SelectedItem = GWBuildings.First(Function(x) x.id = MouseOverObject.MapObject.id)
                        BuildingsView.ScrollIntoView(BuildingsView.SelectedItem)
                        If PreviewSave Then Exit Sub
                        BuildingInfo.Update(BuildingsView.SelectedItem)
                    Case 6
                        If PreviewSave Then Exit Sub
                        BattleInfo.Update(GWBattles.First(Function(x) x.id = MouseOverObject.MapObject.id))
                End Select
            ElseIf e.ChangedButton = MouseButton.Right Then
                If PreviewSave Then Exit Sub
                If IsNothing(MouseOverObject) = False Then
                    'Dim a As New List(Of MenuItem)
                    'For Each i As TestShape In MouseOverList
                    '    a.AddRange(i.MenuItems)
                    '    If a.Count > 10 Then Exit For
                    'Next
                    'Graph.ContextMenu.CustomPopupPlacementCallback = New Primitives.CustomPopupPlacement(New Point(50, 50), 1)
                    Graph.ContextMenu = New ContextMenu
                    Graph.ContextMenu.ItemsSource = MouseOverObject.MenuItems
                    Graph.ContextMenu.Visibility = Visibility.Visible
                    'MouseOverObject.ShowContextMenu()
                    'MouseOverObject.ShowContextMenu(New ObservableCollection(Of MenuItem)(a))
                Else
                    Graph.ContextMenu = mapMenu
                End If
            End If
        End If
    End Sub
    Private Sub Graph_MouseMove(sender As Object, e As MouseEventArgs) Handles Graph.MouseMove
        mousepos = e.GetPosition(Graph)


        If Navigating Then
            Dim f As String
            For Each r As TestShape In MouseOverList
                f = f & $" {r.MapObject.name}" & vbCrLf
            Next
            Dim ab As TestShape
            ab = grid.VirtualChildren(0)
            ab.DrawCoords(New Point(mousepos.X / zoom.Zoom + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom + Scroller.VerticalOffset / zoom.Zoom), _speed, _attack, f)
        End If

        Dim P As New Point(mousepos.X / zoom.Zoom - 3 + Scroller.HorizontalOffset / zoom.Zoom, mousepos.Y / zoom.Zoom - 3 + Scroller.VerticalOffset / zoom.Zoom)
        Dim a As New List(Of TestShape)

        For Each b In Graph.GetChildrenIntersecting(New Rect(P, New Size(6, 6)))
            b = TryCast(b, TestShape)
            If IsNothing(b) = False Then a.Add(b)
        Next
        For i = a.Count - 1 To 0 Step -1
            If IsNothing(a(i).MapObject) Then a.RemoveAt(i) : Continue For
            If a(i).MapObject.type > 6 Or a(i).MapObject.type < 1 Then a.RemoveAt(i)
        Next
        'MouseOverList = a
        If a.Count = 0 Then
            If IsNothing(MouseOverObject) = False Then
                If IsNothing(SelectedObject) = False AndAlso SelectedObject.MapObject.type = MouseOverObject.MapObject.type AndAlso SelectedObject.MapObject.id = MouseOverObject.MapObject.id Then
                Else
                    MouseOverObject.UnDrawSelect()
                End If
                MouseOverObject = Nothing
            End If
            Exit Sub
        End If
        a.Sort(Function(x, y) Math.Sqrt(Math.Pow((x.Bounds.X + x.Bounds.Width / 2 - P.X), 2) + Math.Pow((x.Bounds.Y + x.Bounds.Height / 2 - P.Y), 2)) <
            Math.Sqrt(Math.Pow((y.Bounds.X + y.Bounds.Width / 2 - P.X), 2) + Math.Pow((y.Bounds.Y + y.Bounds.Height / 2 - P.Y), 2)))
        'Debug.Print($"{a.Count} {a.First.MapObject.name}")


        If IsNothing(MouseOverObject) Then
            If a.Count > 1 AndAlso a.First.MapObject.type = 1 Then
                MouseOverObject = a(1)
            Else
                MouseOverObject = a.First
            End If
            MouseOverObject.DrawSelect()
        Else
            If a.Count > 1 AndAlso a.First.MapObject.type = 1 Then

                If a(1).MapObject.id = MouseOverObject.MapObject.id AndAlso a(1).MapObject.type = MouseOverObject.MapObject.type Then

                Else
                    If IsNothing(SelectedObject) = False AndAlso SelectedObject.MapObject.id = MouseOverObject.MapObject.id AndAlso SelectedObject.MapObject.type = MouseOverObject.MapObject.type Then
                    Else
                        MouseOverObject.UnDrawSelect()
                    End If
                    MouseOverObject = a(1)
                    MouseOverObject.DrawSelect()
                End If
            Else
                If a.First.MapObject.id = MouseOverObject.MapObject.id AndAlso a.First.MapObject.type = MouseOverObject.MapObject.type Then

                Else
                    If IsNothing(SelectedObject) = False AndAlso SelectedObject.MapObject.id = MouseOverObject.MapObject.id AndAlso SelectedObject.MapObject.type = MouseOverObject.MapObject.type Then
                    Else
                        MouseOverObject.UnDrawSelect()
                    End If
                    MouseOverObject = a.First
                    MouseOverObject.DrawSelect()
                End If
            End If
            MouseOverList.Clear()
            Dim l As Short
            For Each item In a
                MouseOverList.Add(item)
                l += 1
                If l > 2 Then Exit For
            Next
        End If
    End Sub

    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Dim menu As MenuItem
        menu = sender
        If menu.Header = "Load GW map" Then
            Dim dia As New OpenFileDialog With {.InitialDirectory = Environment.CurrentDirectory & "\Screenshots", .DefaultExt = "asave", .Filter = "GW map save|*.asave"}
            dia.ShowDialog()
            If dia.FileName = String.Empty Then Exit Sub
            Dim reader As New StreamReader(dia.FileName)
            Dim a As String
            GWJSon = reader.ReadToEnd()
            Logger.LogWrite("Offline Mode ON")
            DrawMap(GWJSon)
            PreviewSave = True
        ElseIf menu.Header = "Save GW map" Then
            If PreviewSave = True Then Exit Sub
            SaveMap()
            Logger.LogWrite("GW map saved.")
        ElseIf menu.Header = "Live GW map" Then
            PreviewSave = False
            Logger.LogWrite("Offline Mode OFF")
        End If
    End Sub
End Class