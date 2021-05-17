Imports Microsoft.Sample.Controls
Imports HtmlAgilityPack
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports Newtonsoft.Json

Public Class PlanetInfoControl
    Public Event SelectionChanged(gdata As GalaxyData)
    Property clan_id As Integer
    Property owner_id As Integer
    Property ships As New ObservableCollection(Of GalaxyData)
    Property team As New ObservableCollection(Of GalaxyData)
    Property buildings As New ObservableCollection(Of GalaxyData)

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
        Dim web As New HtmlWeb
        Dim doc As New HtmlDocument
        Dim elem As HtmlNodeCollection
        Do While WC.IsBusy
            Await Task.Delay(200)
        Loop
        doc.LoadHtml(Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?page=planet_page&id={gdata.id}"))
        _data = gdata
        Me.Visibility = Visibility.Visible
        Me.DataContext = _data
        elem = doc.DocumentNode.SelectNodes("//div[@id='ptabs-ships']//span[@data-type='2']")
        ships.Clear()
        If elem IsNot Nothing Then
            For Each e As HtmlNode In elem
                ships.Add(GWMap.GetShip(e.Attributes("data-id").Value))
            Next
        End If
        For i = ships.Count - 1 To 0 Step -1
            If ships(i) Is Nothing Then
                ships.RemoveAt(i)
            End If
        Next
        elem = doc.DocumentNode.SelectNodes("//span[@class='clickable clan-color']")
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
        lstShips.DataContext = ships
        lstTeam.DataContext = team
        buildings.Clear()
        elem = doc.DocumentNode.SelectNodes("//span[@data-type='5']")
        If elem IsNot Nothing Then
            For Each e As HtmlNode In elem
                buildings.Add(GWMap.GetBuilding(e.Attributes("data-id").Value))
            Next
        End If
        lstBuildings.DataContext = buildings
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
        AddHandler Build.Click, New RoutedEventHandler(AddressOf Button_Click)
        ' Добавить код инициализации после вызова InitializeComponent().
    End Sub
    Private Async Sub Button_Click(sender As Object, e As RoutedEventArgs)

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
        If a.Name = "Build" Then
            Dim canva As Canvas
            canva = Me.Parent
            canva.Children.Add(New ShopBControl(_data))
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

    Private Sub lstbuildings_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstBuildings.SelectionChanged
        If lstBuildings.SelectedItem Is Nothing Then Exit Sub
        RaiseEvent SelectionChanged(lstBuildings.SelectedItem)
    End Sub

    Private Sub lstShips_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstShips.SelectionChanged
        If lstShips.SelectedItem Is Nothing Then Exit Sub
        RaiseEvent SelectionChanged(lstShips.SelectedItem)
    End Sub
End Class

<ValueConversion(GetType(Integer), GetType(SolidColorBrush))>
Public Class MyClanSolidColorConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageName As Integer = CInt(value)
        Dim clan_color As String
        Try
            clan_color = GWMap.GetClanColor(imageName)
            If clan_color = "0" Then
                clan_color = "#b4b4b4"
            End If
            Dim myLinearGradientBrush As SolidColorBrush = New SolidColorBrush()

            myLinearGradientBrush.Color = ColorConverter.ConvertFromString("#FF" & clan_color.Replace("#", ""))
            Return myLinearGradientBrush
        Catch
            Return Nothing
        End Try
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class

<ValueConversion(GetType(Integer), GetType(String))>
Public Class MyIDToNameConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim id As Integer = CInt(value)
        Try
            Return CheckNotable(id)
        Catch
            Return id
        End Try
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class
