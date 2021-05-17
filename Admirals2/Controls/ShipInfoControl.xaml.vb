Imports Microsoft.Sample.Controls
Imports HtmlAgilityPack
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports Newtonsoft.Json
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Threading
Imports System.Net

Public Class ShipInfoControl

    Property clan_id As Integer
    Property owner_id As Integer

    Property construct_duration As Integer
    Property construct_time As Integer
    Property pay_sum As Integer

    Property cost As UShort
    Property arrive_timespan As TimeSpan
    Property team As New ObservableCollection(Of GalaxyData)
    Property guys0 As New ObservableCollection(Of GalaxyData)
    Property guys1 As New ObservableCollection(Of GalaxyData)
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
    WithEvents timer1 As New Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 1)}
    Public Event SelectionChanged(gd As GalaxyData)
    Private Sub lstTeam_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstTeam.SelectionChanged
        RaiseEvent SelectionChanged(lstTeam.SelectedItem)
    End Sub
    Private _data As GalaxyData
    Public Property data() As GalaxyData
        Get
            Return _data
        End Get
        Set(ByVal value As GalaxyData)
            _data = value
        End Set
    End Property

    Async Sub Update(gdata As ShipsData)
        Dim a As New Regex("ship\.[A-Za-z_]* = [0-9]*;")
        Dim b As New Regex("[0-9]+")
        Dim a2 As New Regex("hurry_cost = [0-9]+ \*")
        Dim m As MatchCollection
        Try
            shipConstruct.Visibility = Visibility.Hidden
            lstClans0.DataContext = Nothing
            lstGuys0.DataContext = Nothing
            lstClans1.DataContext = Nothing
            lstGuys1.DataContext = Nothing
            txtArrive.Text = ""
            tabMain.SelectedIndex = 0
            tabPerm.SelectedIndex = 0
            ShipHolder.Opacity = 1
            Dim web As New HtmlWeb
            Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Dim s As String
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36 OPR/64.0.3417.150"
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=ship_page&id={gdata.id}")
            'doc = web.Load($"http://galaxy.xjedi.com/srv/conn.php?page=ship_page&id={gdata.id}")
            doc.LoadHtml(s)
            If gdata.state = 0 Then
                shipConstruct.Visibility = Visibility.Visible
                m = a.Matches(doc.ParsedText)
                Dim c As Match = b.Match(m(0).Value)
                gdata.state_time = CInt(c.Value)
                c = b.Match(m(1).Value)
                construct_duration = CInt(c.Value)
                c = a2.Match(doc.ParsedText)
                If c.Value = "" Then
                    cost = 0
                Else
                    c = b.Match(c.Value)
                    cost = c.Value
                End If
                ShipHolder.Opacity = 0.2
                ConstructProgress.Maximum = construct_duration
                ConstructProgress.Value = gdata.state_time
                construct_time = construct_duration - gdata.state_time
                Construct_timeleft.Text = TimeSpan.FromSeconds(construct_time).ToString
                pay_sum = Math.Round(construct_time / construct_duration * (cost * (1.0 + 0.4 * gdata.construct_order)))
                If pay_sum < 10 Then pay_sum = 10
                txtPay_sum.Text = pay_sum & " CUAG"
            End If
            If gdata.state = 2 Then
                arrive_timespan = gdata.arrive_timespan
                If arrive_timespan.Days >= 1 Then
                    txtArrive.Text = arrive_timespan.ToString("d' days 'h' hours 'm' min'")
                ElseIf arrive_timespan.Hours >= 1 Then

                    txtArrive.Text = arrive_timespan.ToString("h' hours 'm' min'")
                Else
                    txtArrive.Text = arrive_timespan.ToString("m' min'")
                End If
            End If
            _data = gdata
            Me.Visibility = Visibility.Visible
            Me.DataContext = gdata
            elem = doc.DocumentNode.SelectNodes("//div[@id='ptabs-chars']//span[@class='clickable clan-color']")
            team.Clear()
            timer1.Start()
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

            lstTeam.DataContext = team
        Catch err As Exception
            Debug.Print("ShipInfo error:" & err.Message & err.TargetSite.Name)
        End Try
    End Sub

    Private Async Sub Rectangle_MouseUP(sender As Object, e As MouseButtonEventArgs)
        Dim a As Rectangle, s As String = "", gwpack As GWClass, playerdata As GalaxyData
        a = sender
        If a Is Nothing Then Exit Sub
        If a.Name = "editName" Then
            Dim canva As Canvas
            canva = Me.Parent
            canva.Children.Add(New RenameControl(_data))
        Else
            playerdata = a.DataContext
            Logger.LogWrite($"Dropping {playerdata.name} from {_data.name}... ")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=character_exit&character_id={playerdata.id}")
        End If
        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If
        Catch
        End Try
    End Sub
    Private Async Sub Button_Click(sender As Object, e As RoutedEventArgs)
        'act=commit_suicide
        'act=character_exit
        'act=character_enter&target_type_id=2&target_id=698
        'act=rename_submit&obj_type_id=2&obj_id=706&new_name=Suicide+Watch
        'http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id=89&ship_spec_id=3&pay_sum=350
        Dim a As Button, s As String = "", gwpack As GWClass
        a = sender
        If a Is Nothing Then Exit Sub
        If a.Name = "Enter" Then
            Logger.LogWrite($"Entering ship {_data.name}...")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=character_enter&target_type_id={_data.type}&target_id={_data.id}")
        End If
        If a.Name = "Leave" Then
            Logger.LogWrite($"Attempting to leave ship\building...")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=character_exit")
        End If
        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If
        Catch
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
        ClanHolder.Stroke = Nothing
        AddHandler Enter.Click, New RoutedEventHandler(AddressOf Button_Click)
        AddHandler Leave.Click, New RoutedEventHandler(AddressOf Button_Click)
        AddHandler editName.MouseUp, New MouseButtonEventHandler(AddressOf Rectangle_MouseUP)
        ' Добавить код инициализации после вызова InitializeComponent().
    End Sub
    Public Shared Sub OnGdataChanged()
    End Sub

    Private Async Sub tabSelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles tabPerm.SelectionChanged
        If tabPerm.SelectedIndex = 1 Then
            Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Dim str_web As String
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            str_web = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=perm/enter_permissions_window&obj_type_id={_data.type}&obj_id={_data.id}")
            doc.LoadHtml(str_web)
            lstGuys0.Visibility = Visibility.Visible
            lstClans0.Visibility = Visibility.Visible
            If str_web Like "ERROR: access denied" Then
                lstGuys0.Visibility = Visibility.Hidden
                lstClans0.Visibility = Visibility.Hidden
            End If
            elem = doc.DocumentNode.SelectNodes("//span[@class='clickable clan-color']")
                guys0.Clear()
                If elem IsNot Nothing Then
                    For Each ex As HtmlNode In elem
                        guys0.Add(New GalaxyData() With {
                     .id = ex.Attributes("data-id").Value,
                     .clan_id = ex.Attributes("data-clan_id").Value,
                     .name = CheckNotable(ex.Attributes("data-id").Value),
                     .type = 4,
                     .htmlcolor2 = GWMap.GetClanColor(ex.Attributes("data-id").Value),
                     .prefix = GWMap.GetClanPrefix(ex.Attributes("data-clan_id").Value),
                     .permission = True})
                    Next
                End If
                For Each item As GalaxyData In GWAdmirals
                    Dim a As GalaxyData
                    a = guys0.FirstOrDefault(Function(c) c.id = item.id)
                    If a Is Nothing Then
                        guys0.Add(item)
                        guys0.Last.permission = False
                    End If
                Next
                lstGuys0.DataContext = guys0
                lstClans0.DataContext = GWClans
                For Each gdata As GalaxyData In GWClans
                    gdata.permission = False
                Next
                elem = doc.DocumentNode.SelectNodes("//span[@data-type='3']")
                If elem IsNot Nothing Then
                    For Each ex As HtmlNode In elem
                        Dim s As Integer = ex.Attributes("data-id").Value
                        For Each gdata As GalaxyData In GWClans
                            If gdata.id = s Then
                                gdata.permission = True
                            End If
                        Next
                    Next
                End If
            ElseIf tabPerm.SelectedIndex = 2 Then
                Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Dim str_web As String
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            str_web = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=perm%2Fcontrol_permissions_window&obj_type_id={_data.type}&obj_id={_data.id}")
            lstGuys1.Visibility = Visibility.Visible
            lstClans1.Visibility = Visibility.Visible
            If str_web Like "ERROR: access denied" Then
                lstGuys1.Visibility = Visibility.Hidden
                lstClans1.Visibility = Visibility.Hidden
            End If
            doc.LoadHtml(str_web)
            elem = doc.DocumentNode.SelectNodes("//span[@class='clickable clan-color']")
            guys0.Clear()
            If elem IsNot Nothing Then
                For Each ex As HtmlNode In elem
                    guys0.Add(New GalaxyData() With {
                     .id = ex.Attributes("data-id").Value,
                     .clan_id = ex.Attributes("data-clan_id").Value,
                     .name = CheckNotable(ex.Attributes("data-id").Value),
                     .type = 4,
                     .htmlcolor2 = GWMap.GetClanColor(ex.Attributes("data-id").Value),
                     .prefix = GWMap.GetClanPrefix(ex.Attributes("data-clan_id").Value),
                     .permission = True})
                Next
            End If

            For Each item As GalaxyData In GWAdmirals
                Dim a As GalaxyData
                a = guys0.FirstOrDefault(Function(c) c.id = item.id)
                If a Is Nothing Then
                    guys0.Add(item)
                    guys0.Last.permission = False
                End If
            Next
            elem = doc.DocumentNode.SelectNodes("//span[@data-type='3']")
            lstGuys1.DataContext = guys0
            lstClans1.DataContext = GWClans
            For Each gdata As GalaxyData In GWClans
                gdata.permission = False
            Next
            If elem IsNot Nothing Then
                For Each ex As HtmlNode In elem
                    Dim s As Integer = ex.Attributes("data-id").Value
                    For Each gdata As GalaxyData In GWClans
                        If gdata.id = s Then
                            gdata.permission = True
                        End If
                    Next
                Next
            End If
        End If
    End Sub

    Private Async Sub CheckBox_Checked(sender As Object, e As RoutedEventArgs)
        Dim a As CheckBox = sender
        Dim b As GalaxyData
        Dim s As String
        Dim gwpack As GWClass
        b = a.DataContext
        If b.permission Then
            Logger.LogWrite($"Trying to allow enter for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=set_permission&permission_type=2&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_id={b.id}")
        Else
            Logger.LogWrite($"Trying to disallow enter for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=remove_permission&permission_type=2&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_id={b.id}")
        End If


        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If
        Catch
        End Try
    End Sub
    Private Async Sub CheckBox2_Checked(sender As Object, e As RoutedEventArgs)
        Dim a As CheckBox = sender
        Dim b As GalaxyData
        Dim s As String
        Dim gwpack As GWClass
        b = a.DataContext
        If b.permission Then
            Logger.LogWrite($"Trying to allow control for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=set_permission&permission_type=1&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_id={b.id}")
        Else
            Logger.LogWrite($"Trying to disallow control for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=remove_permission&permission_type=1&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_id={b.id}")
        End If


        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If
        Catch
        End Try
    End Sub

    Private Async Sub Ellipse_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Dim a As Ellipse, s As String, gwpack As GWClass, msg As MsgBoxResult
        a = sender
        If a.Name = "HurryUp" Then
            Logger.LogWrite($"Hurry up ship {_data.name}...")
            msg = MsgBox($"Are you sure want to hurry up ship for {pay_sum} CUAG", MsgBoxStyle.OkCancel, "Admirals")
            If msg = MsgBoxResult.Ok Then
                Do While WC.IsBusy
                    Await Task.Delay(200)
                Loop
                s = Await WC.DownloadStringTaskAsync(New Uri($"http://galaxy.xjedi.com/srv/conn.php?act=ship_hurry_construction&ship_id={_data.id}&pay_sum={pay_sum}"))
                Try
                    gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
                    If gwpack.code = "ERROR" Then
                        Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
                    ElseIf gwpack.code = "OK" Then
                        Logger.LogWrite($"Code: [{gwpack.code}]")
                    End If
                Catch
                End Try
            Else
                Logger.LogWrite($"Canceled")
            End If
        End If
    End Sub

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
        If _data.state = 0 AndAlso _data.construct_order = 0 Then
            _data.state_time += 1
            ConstructProgress.Value = _data.state_time
            construct_time = construct_duration - _data.state_time
            Construct_timeleft.Text = TimeSpan.FromSeconds(construct_time).ToString
            pay_sum = Math.Round(construct_time / construct_duration * (cost * (1.0 + 0.4 * _data.construct_order)))
            If pay_sum < 10 Then pay_sum = 10
            txtPay_sum.Text = pay_sum & " CUAG"
        End If
        If _data.state = 2 Then
            arrive_timespan = arrive_timespan.Subtract(timer1.Interval)
            If arrive_timespan.Days >= 1 Then
                txtArrive.Text = arrive_timespan.ToString("d' days 'h' hours 'm' min'")
            ElseIf arrive_timespan.Hours >= 1 Then

                txtArrive.Text = arrive_timespan.ToString("h' hours 'm' min'")
            Else
                txtArrive.Text = arrive_timespan.ToString("m' min'")
            End If
        End If
    End Sub
End Class
