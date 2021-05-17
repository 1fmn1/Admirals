Imports Newtonsoft.Json

Public Class ShopBControl
    Dim gdata As GalaxyData
    Sub New()

        ' Этот вызов является обязательным для конструктора.
        InitializeComponent()

        ' Добавить код инициализации после вызова InitializeComponent().

    End Sub
    Sub New(mgdata As GalaxyData)
        gdata = mgdata
        ' Этот вызов является обязательным для конструктора.
        InitializeComponent()
        If gdata.type = 2 Then
            txtCaption.Text = $"Rename ship {gdata.name}"
        End If
        If gdata.type = 5 Then
            txtCaption.Text = $"Order ship at {gdata.name}"
        End If
        If gdata.type = 1 Then
            txtCaption.Text = $"Rename planet {gdata.name}"
        End If
        ' Добавить код инициализации после вызова InitializeComponent().

    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim canva As Canvas
        canva = Me.Parent
        canva.Children.Remove(Me)
    End Sub
    Private Async Sub Button_Click_Confirm(sender As Object, e As RoutedEventArgs)
        If IsNothing(gdata) Then Exit Sub
        'Logger.LogWrite($"Attempting to rename  {gdata.name} to [{txtName.Text}]...")
        Dim s As String = "", gwpack As GWClass
        Dim can As Canvas, tabIt As TabItem, But As Button, price As TextBox
        But = sender
        can = But.Parent
        tabIt = can.Parent
        price = can.Children.Item(2)
        Logger.LogWrite($"Attempting to buy [{tabIt.Name}] for [{price.Text}] CUAG  on planet [{gdata}]...")
        Do While WC.IsBusy
            Await Task.Delay(200)
        Loop
        s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={tabIt.Header}&pay_sum={price.Text}")
        gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
        If gwpack.code <> "OK" Then
            Dim msg As MsgBoxResult
            msg = MsgBox($"Minimum price for {tabIt.Name} is {gwpack.cost} CUAG. Do you want to buy?", vbOKCancel, "Admirals")
            If msg = MsgBoxResult.Ok Then
                Do While WC.IsBusy
                    Await Task.Delay(200)
                Loop
                s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=building_buy&planet_id={gdata.id}&building_spec_id={tabIt.Header}&pay_sum={gwpack.cost}")
            Else
                Logger.LogWrite($"Canceled")
                Exit Sub
            End If
        End If
        Try
            gwpack = JsonConvert.DeserializeObject(Of GWClass)(s)
            If gwpack.code = "ERROR" Then
                Logger.LogWrite($"Code: [{gwpack.code}] : {gwpack.error}")
            ElseIf gwpack.code = "OK" Then
                Logger.LogWrite($"Code: [{gwpack.code}]")
            End If

        Catch err As Exception
            Logger.LogWrite($"Internal Error: {err.Message}")
        End Try
        'Dim canva As Canvas
        'canva = Me.Parent
        'canva.Children.Remove(Me)
    End Sub
End Class
