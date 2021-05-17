Imports Microsoft.Sample.Controls
Imports HtmlAgilityPack
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports Newtonsoft.Json

Public Class BuildingInfoControl

    Property clan_id As Integer
    Property owner_id As Integer
    Property ships As New ObservableCollection(Of GalaxyData)
    Property team As New ObservableCollection(Of GalaxyData)
    Property buildings As New ObservableCollection(Of GalaxyData)
    Property guys0 As New ObservableCollection(Of GalaxyData)
    Property guys1 As New ObservableCollection(Of GalaxyData)
    Property imagestring As String
    Property image As New BitmapImage
    WithEvents timer1 As New Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 1, 0)}
    Private _data As GalaxyData
    Public Property data() As GalaxyData
        Get
            Return _data
        End Get
        Set(ByVal value As GalaxyData)
            _data = value
        End Set
    End Property
    Public Event SelectionChanged(gdata As GalaxyData)
    Async Sub Update(gdata As GalaxyData)
        Dim web As New HtmlWeb
        Dim doc As New HtmlDocument
        Dim elem As HtmlNodeCollection
        tabMain.SelectedIndex = 0
        tabPerm.SelectedIndex = 0
        lstClans0.DataContext = Nothing
        lstGuys0.DataContext = Nothing
        lstClans1.DataContext = Nothing
        lstGuys1.DataContext = Nothing
        Dim s As String
        Do While WC.IsBusy
            Await Task.Delay(200)
        Loop
        s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=building_page&id={gdata.id}")
        doc.LoadHtml(s)
        _data = gdata
        Me.Visibility = Visibility.Visible
        Me.DataContext = _data
        imagestring = doc.DocumentNode.SelectSingleNode("//img").Attributes("src").Value
        If IsNothing(image.UriSource) Then
            Try
                image = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/{imagestring}.png"))
            Catch ex As Exception
                image = New BitmapImage
                image.BeginInit()
                image.UriSource = New Uri($"http://galaxy.xjedi.com/{imagestring}", UriKind.Absolute)
                image.CacheOption = BitmapCacheOption.OnLoad
                image.EndInit()
                'SaveImage(image, imagestring)
            End Try
            ShipHolder.Fill = New ImageBrush(image)
        ElseIf image.UriSource.OriginalString Like $"*{gdata.image}*" = False Then
            Try
                image = New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/{imagestring}.png"))
            Catch ex As Exception
                image = New BitmapImage
                image.BeginInit()
                image.UriSource = New Uri($"http://galaxy.xjedi.com/{imagestring}", UriKind.Absolute)
                image.CacheOption = BitmapCacheOption.OnLoad
                image.EndInit()
                'SaveImage(image, imagestring)
            End Try
            ShipHolder.Fill = New ImageBrush(image)
        End If
        elem = doc.DocumentNode.SelectNodes("//span[@data-type='2']")
        ships.Clear()
        If elem IsNot Nothing Then
            For Each e As HtmlNode In elem
                ships.Add(GWMap.GetShip(e.Attributes("data-id").Value))
            Next
        End If
        elem = doc.DocumentNode.SelectNodes("//div[@id='building-char-list']/div/ul/li/span/span[@class='clickable clan-color']")
        team.Clear()
        If elem IsNot Nothing Then
            For Each e As HtmlNode In elem
                team.Add(New GalaxyData() With {
                     .id = e.Attributes("data-id").Value,
                     .clan_id = e.Attributes("data-clan_id").Value,
                     .name = CheckNotable(e.Attributes("data-id").Value),
                     .type = 4,
                     .htmlcolor2 = GWMap.GetClanColor(e.Attributes("data-id").Value),
                     .prefix = GWMap.GetClanPrefix(e.Attributes("data-clan_id").Value)})

            Next
        End If
        timer1.Start()
        lstShips.DataContext = ships
        lstTeam.DataContext = team
        Try
            gdata.ptype = "http://galaxy.xjedi.com/" & doc.DocumentNode.SelectSingleNode("//img[@id='building_image']").Attributes("src").Value
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
        ' Добавить код инициализации после вызова InitializeComponent().
    End Sub

    Private Async Sub Rectangle_MouseUP(sender As Object, e As MouseButtonEventArgs)
        Dim a As Rectangle, s As String = "", gwpack As GWClass, playerdata As GalaxyData
        a = sender
        If a Is Nothing Then Exit Sub
        If a.Name = "editName" Then
            Dim canva As Canvas
            canva = Me.Parent
            canva.Children.Add(New RenameControl(_data))
        End If
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
        If a.Name = "Buyship" Then
            Dim canva As Canvas
            canva = Me.Parent
            canva.Children.Add(New ShopControl(_data))
        End If
        'Case "Buy ship"
        'Dim price As Integer
        'Logger.LogWrite($"Attempting to buy ship type [{BoxBuyType.SelectedItem}] on starport [{_data}]...")
        'If BoxBuyType.SelectedItem Is Nothing Then Exit Sub
        '        s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum=10")
        '        gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
        '        Dim msg As MsgBoxResult
        '        msg = MsgBox($"Are you sure want to buy {BoxBuyType.SelectedItem} for {gwpack.cost} CUAG?", vbOKCancel, "Admirals")
        '        If msg = MsgBoxResult.Ok Then
        '            s = WC.DownloadString($"http://galaxy.xjedi.com/srv/conn.php?act=ship_buy&starport_id={gdata.id}&ship_spec_id={ShipTypes(BoxBuyType.SelectedItem)}&pay_sum={gwpack.cost}")
        '        Else
        '            Logger.LogWrite($"Canceled")
        '            Exit Sub
        '        End If
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



    Private Async Sub tabSelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles tabPerm.SelectionChanged
        If tabPerm.SelectedIndex = 1 Then
            Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            doc.LoadHtml(Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=perm/enter_permissions_window&obj_type_id={_data.type}&obj_id={_data.id}"))
            elem = doc.DocumentNode.SelectNodes("//span[@class='clickable clan-color']")
            guys0.Clear()
            lstClans0.DataContext = Nothing
            lstGuys0.DataContext = Nothing
            lstClans1.DataContext = Nothing
            lstGuys1.DataContext = Nothing
            If elem IsNot Nothing Then
                lstGuys0.DataContext = guys0
                lstClans0.DataContext = GWClans
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
                For Each item As GalaxyData In GWAdmirals
                    Dim a As GalaxyData
                    a = guys0.FirstOrDefault(Function(c) c.id = item.id)
                    If a Is Nothing Then
                        guys0.Add(item)
                        guys0.Last.permission = False
                    End If
                Next
            End If
            elem = doc.DocumentNode.SelectNodes("//span[@data-type='3']")
            If elem IsNot Nothing Then
                lstGuys1.DataContext = guys0
                lstClans1.DataContext = GWClans
                For Each gdata As GalaxyData In GWClans
                    gdata.permission = False
                Next
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
            lstClans0.DataContext = Nothing
            lstGuys0.DataContext = Nothing
            lstClans1.DataContext = Nothing
            lstGuys1.DataContext = Nothing
            Dim doc As New HtmlDocument
            Dim elem As HtmlNodeCollection
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            doc.LoadHtml(Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=perm%2Fship_order_permissions_window&obj_type_id={_data.type}&obj_id={_data.id}"))
            elem = doc.DocumentNode.SelectNodes("//span[@class='clickable clan-color']")
            guys0.Clear()
            If elem IsNot Nothing Then
                lstGuys1.DataContext = guys0
                lstClans1.DataContext = GWClans
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
                For Each item As GalaxyData In GWAdmirals
                    Dim a As GalaxyData
                    a = guys0.FirstOrDefault(Function(c) c.id = item.id)
                    If a Is Nothing Then
                        guys0.Add(item)
                        guys0.Last.permission = False
                    End If
                Next
            End If
            elem = doc.DocumentNode.SelectNodes("//span[@data-type='3']")

            If elem IsNot Nothing Then
                lstGuys1.DataContext = guys0
                lstClans1.DataContext = GWClans
                For Each gdata As GalaxyData In GWClans
                    gdata.permission = False
                Next
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

    Private Sub lstShips_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstShips.SelectionChanged
        If lstShips.SelectedItem Is Nothing Then Exit Sub
        RaiseEvent SelectionChanged(lstShips.SelectedItem)
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
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=remove_permission&permission_type=2&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_name={b.id}")
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
            Logger.LogWrite($"Trying to allow buy ships for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=set_permission&permission_type=20&obj_type_id={_data.type}&obj_id={_data.id}&data_int={0}&subj_type_id={b.type}&subj_id={b.id}")
        Else
            Logger.LogWrite($"Trying to disallow buy ships for {b.name}")
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=remove_permission&permission_type=20&obj_type_id={_data.type}&obj_id={_data.id}&subj_type_id={b.type}&subj_id={b.id}")
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

    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick

    End Sub

    'Private Sub tabPerm_IsVisibleChanged(sender As Object, e As DependencyPropertyChangedEventArgs) Handles tabPerm.IsVisibleChanged
    '    'tabSelectionChanged(Nothing, Nothing)
    'End Sub

    'Private Sub tabMain_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles tabMain.SelectionChanged
    '    If tabMain.SelectedIndex > 0 Then
    '        tabSelectionChanged(Nothing, Nothing)
    '    End If
    'End Sub
End Class
