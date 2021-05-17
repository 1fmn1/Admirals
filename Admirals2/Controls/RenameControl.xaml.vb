Imports Newtonsoft.Json

Public Class RenameControl
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
            txtCaption.Text = $"Rename building {gdata.name}"
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
        Logger.LogWrite($"Attempting to rename  {gdata.name} to [{txtName.Text}]...")
        Dim s As String = "", gwpack As GWClass
        s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=rename_submit&obj_type_id={gdata.type}&obj_id={gdata.id}&new_name={Uri.EscapeUriString(txtName.Text)}")
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
        Dim canva As Canvas
        canva = Me.Parent
        canva.Children.Remove(Me)
    End Sub
End Class
