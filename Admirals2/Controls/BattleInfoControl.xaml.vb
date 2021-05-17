Imports Microsoft.Sample.Controls
Imports HtmlAgilityPack
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports Newtonsoft.Json
Imports System.IO
Imports System.Windows.Threading

Public Class BattleInfoControl

    Property clan_id As Integer
    Property owner_id As Integer

    Property image As New BitmapImage
    Property ship1name As String
    Property ship2name As String
    Property team As New ObservableCollection(Of GalaxyData)
    Property team2 As New ObservableCollection(Of GalaxyData)
    'Public Shared Property GdataProperty As DependencyProperty = DependencyProperty.Register("gdata1234",
    '                   GetType(ShipsData), GetType(ShipInfoControl),
    '                   New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.AffectsMeasure Or
    '                                FrameworkPropertyMetadataOptions.AffectsRender))
    'Public Property Gdata As ShipsData
    '    Get
    '        Return CType(GetValue(GdataProperty), ShipsData)
    '    End Get
    '    Set(ByVal value As ShipsData)
    '        SetValue(GdataProperty, value)

    '    End Set
    'End Property
    Property timesp As TimeSpan
    WithEvents timer1 As New DispatcherTimer With {.Interval = New TimeSpan(0, 0, 1)}
    Public Event SelectionChanged(gd As GalaxyData)
    'Private Sub lstTeam_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstTeam.SelectionChanged
    '    RaiseEvent SelectionChanged(lstTeam.SelectedItem)
    'End Sub
    Private _data As GalaxyData
    Public Property data() As GalaxyData
        Get
            Return _data
        End Get
        Set(ByVal value As GalaxyData)
            _data = value
        End Set
    End Property

    Async Sub Update(gdata As GalaxyData)
        Try
            tabMain.SelectedIndex = 0
            Dim web As New HtmlWeb
            Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Dim eShips As HtmlNodeCollection
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            doc.LoadHtml(Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=battle_page&id={gdata.id}"))
            _data = gdata
            Me.Visibility = Visibility.Visible
            Me.DataContext = gdata
            textIP.Text = ""
            textIP.Inlines.Clear()
            Dim a As New Hyperlink
            a.Inlines.Add(doc.DocumentNode.SelectSingleNode("//h2").InnerText)
            a.NavigateUri = New Uri("img", UriKind.Relative)
            AddHandler a.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
            textIP.Inlines.Add(a)
            Dim t As Long = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            Dim mstate_time As Long
            Dim state_time_string As String
            mstate_time = CLng(Trim(doc.DocumentNode.SelectSingleNode("//div[@class='time']").Attributes("data-time").Value))
            t = (mstate_time - t)
            timesp = TimeSpan.FromSeconds(t)
            If timesp.Days > 0 Then
                state_time_string = timesp.ToString("d' days 'h' hours 'm' min'")
            ElseIf timesp.Hours > 0 Then

                state_time_string = timesp.ToString("h' hours 'm' min'")
            Else
                state_time_string = timesp.ToString("m' min 's' sec'")
            End If
            timer1.Start()
            textTimeleft.Text = "Time left: " & state_time_string
            txtDate.Text = DateTimeOffset.FromUnixTimeSeconds(mstate_time).LocalDateTime.ToString("f")
            gdata.image = doc.DocumentNode.SelectSingleNode("//img").Attributes("src").Value
            If IsNothing(image.UriSource) Then
                image = New BitmapImage(New Uri($"http://galaxy.xjedi.com/{gdata.image}"))
                ShipHolder.Fill = New ImageBrush(image)
            ElseIf image.UriSource.OriginalString Like $"*{gdata.image}*" = False Then
                image = New BitmapImage(New Uri($"http://galaxy.xjedi.com/{gdata.image}"))
                ShipHolder.Fill = New ImageBrush(image)
            End If
            eShips = doc.DocumentNode.SelectNodes("//ul")
            Dim tr As Boolean
            For Each el As HtmlNode In eShips
                If tr = False Then
                    Dim ship As GalaxyData
                    Dim ty As Integer
                    Dim hypername As Hyperlink
                    ty = CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-type").Value))
                    If ty = 2 Then
                        ship = GWMap.GetShip(CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-id").Value)))

                    ElseIf ty = 5 Then
                        ship = GWMap.GetBuilding(CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-id").Value)))
                    End If
                    Ship1Image.Source = MyBrushes(ship.image).ImageSource
                    Clan1Image.Source = MyBrushes(ship.clan_id).ImageSource
                    hypername = New Hyperlink With {.NavigateUri = New Uri($"{ship.type}/{ship.id}", UriKind.Relative)}
                    hypername.Inlines.Add(ship.name)
                    hypername.Foreground = Brushes.Aquamarine
                    ship1name = Trim(el.SelectSingleNode("li[@class='header']").InnerText)
                    Dim clan_color As String
                    clan_color = GWMap.GetClanColor(el.SelectSingleNode("li[@class='header']/span/span").Attributes("data-clan_id").Value)
                    If clan_color = "0" Then
                        clan_color = "#b4b4b4"
                    End If

                    AddHandler hypername.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
                    Team1header.Text = ""
                    Team1header.Inlines.Add(hypername)
                    Team1header.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
                    elem = el.SelectNodes("li/span[@class='clickable clan-color']")
                    team.Clear()
                    If IsNothing(elem) Then Exit Sub
                    For Each e As HtmlNode In elem
                        team.Add(New GalaxyData() With {
                             .id = e.Attributes("data-id").Value,
                             .clan_id = e.Attributes("data-clan_id").Value,
                             .name = CheckNotable(e.Attributes("data-id").Value),
                             .type = 4,
                             .htmlcolor2 = GWMap.GetClanColor(e.Attributes("data-id").Value),
                             .prefix = GWMap.GetClanPrefix(e.Attributes("data-clan_id").Value)})

                    Next
                    tr = True
                Else
                    Dim ship As GalaxyData
                    Dim ty As Integer
                    Dim hypername As Hyperlink
                    ty = CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-type").Value))
                    If ty = 2 Then
                        ship = GWMap.GetShip(CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-id").Value)))
                    ElseIf ty = 5 Then
                        ship = GWMap.GetBuilding(CInt(Trim(el.SelectSingleNode("li[@class='header']/span").Attributes("data-id").Value)))
                    End If
                    ship2name = Trim(el.SelectSingleNode("li[@class='header']").InnerText)
                    Dim clan_color As String
                    clan_color = GWMap.GetClanColor(el.SelectSingleNode("li[@class='header']/span/span").Attributes("data-clan_id").Value)
                    If clan_color = "0" Then
                        clan_color = "#b4b4b4"
                    End If
                    Ship2Image.Source = MyBrushes(ship.image).ImageSource
                    Clan2Image.Source = MyBrushes(ship.clan_id).ImageSource
                    hypername = New Hyperlink With {.NavigateUri = New Uri($"{ship.type}/{ship.id}", UriKind.Relative)}
                    AddHandler hypername.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
                    hypername.Inlines.Add(ship.name)
                    hypername.Foreground = Brushes.Aquamarine
                    Team2header.Text = ""
                    Team2header.Inlines.Add(hypername)
                    Team2header.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
                    elem = el.SelectNodes("li/span[@class='clickable clan-color']")
                    team2.Clear()
                    If IsNothing(elem) Then Exit Sub
                    For Each e As HtmlNode In elem
                        team2.Add(New GalaxyData() With {
                             .id = e.Attributes("data-id").Value,
                             .clan_id = e.Attributes("data-clan_id").Value,
                             .name = CheckNotable(e.Attributes("data-id").Value),
                             .type = 4,
                             .htmlcolor2 = GWMap.GetClanColor(e.Attributes("data-id").Value),
                             .prefix = GWMap.GetClanPrefix(e.Attributes("data-clan_id").Value)})

                    Next
                End If
            Next
            lstTeam1.DataContext = team
            lstTeam2.DataContext = team2
        Catch err As Exception
            Debug.Print("ShipInfo error:" & err.Message & err.TargetSite.Name)
        End Try

    End Sub

    Sub rnavigate(sender As Object, arg As RequestNavigateEventArgs)
        Try
            Dim a As Hyperlink
            a = sender
            If a.NavigateUri.OriginalString Like "#/#*" Then
                Dim b() As String
                b = a.NavigateUri.OriginalString.Split("/")
                RaiseEvent SelectionChanged(GWMap.galaxy_data.First(Function(c) IsNothing(c.type) = False AndAlso c.type = Integer.Parse(b(0)) AndAlso c.id = Integer.Parse(b(1))))
            Else
                Clipboard.SetText(New TextRange(a.Inlines(0).ContentStart, a.Inlines(0).ContentEnd).Text)
            End If
        Catch ex As Exception
            Logger.LogWrite("Battle control error." & ex.Message)
        End Try
    End Sub

    Private Class GWUser
        Public Property clan_id As Byte
        Public Property name As String
        Public Property prefix As String
        Public Property player_id As UShort
    End Class
    Public Sub New()

        ' Этот вызов является обязательным для конструктора.
        InitializeComponent()
        ShipHolder.Stroke = Nothing
        ' Добавить код инициализации после вызова InitializeComponent().
    End Sub

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        timesp = timesp.Subtract(timer1.Interval)
        If timesp.Days >= 1 Then
            textTimeleft.Text = "Time left: " & timesp.ToString("d' days 'h' hours 'm' min'")
        ElseIf timesp.Hours >= 1 Then
            textTimeleft.Text = "Time left: " & timesp.ToString("h' hours 'm' min'")
        Else
            textTimeleft.Text = "Time left: " & timesp.ToString("m' min 's' sec'")
        End If
    End Sub
End Class