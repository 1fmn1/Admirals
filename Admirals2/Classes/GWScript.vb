Imports System.Collections.ObjectModel
Imports System.Net
Imports Newtonsoft.Json
Imports System.IO

Public Enum ModeEnum
    Protect = 0
    Hunt = 1
    Move = 2
End Enum
Public Class GWScript
    Public Property script_id As Integer
    Public Property name As String
    Public Property id As UShort
    Public Property image As String
    Public Property Mode As ModeEnum
    Public Property target_name As String
    Public Property target_id As UShort
    Public Property target_type As UShort
    Public Property Admiral As String
    Public Property Working As UShort
        Get
            Return m_working
        End Get
        Set(value As UShort)
            m_working = value
        End Set
    End Property
    Public Property nextscript_id As Object
        Get
            Return m_nextscript_id
        End Get
        Set(value As Object)
            If IsNothing(TryCast(value, GWScript)) Then
                m_nextscript_id = value
            Else
                m_nextscript_id = CType(value, GWScript).script_id
            End If
        End Set
    End Property
    Public Property Point As Point

    'Public Overrides Function ToString() As String
    '    Return script_id
    'End Function
    Private m_working As UShort, m_nextscript_id As Integer
End Class

Public Class GWScriptHolder
    Public Sub New()
        MyScripts.DefaultIfEmpty(Nothing)
    End Sub

    Public Property MyScripts As New ObservableCollection(Of GWScript)
    Public Async Sub Process()
        For Each script As GWScript In MyScripts
            If script.Working = 1 Then
                Dim MyShip As GalaxyData
                Dim MyTarget As GalaxyData
                Dim rect As Rect
                For Each gdata As GalaxyData In GWFleet
                    If gdata.id = script.id Then
                        MyShip = gdata
                        Exit For
                    End If
                Next
                If IsNothing(MyShip) Then script.Working = 0 : Continue For
                If MyShip.state = 99 Then script.Working = 0 : Continue For
                If script.Mode = ModeEnum.Protect Then
                    If IsNothing(MyShip) Then Continue For
                    If MyShip.state = 0 Or MyShip.state > 2 Then Continue For
                    For Each gdata As GalaxyData In GWPlanets
                        If gdata.type = script.target_type AndAlso gdata.id = script.target_id Then
                            MyTarget = gdata
                            Exit For
                        End If
                    Next
                    If IsNothing(MyTarget) Then Continue For
                    If MyShip.planet_id Is Nothing OrElse MyShip.planet_id <> script.target_id Then
                        If MyShip.state = 2 AndAlso MyShip.target_type_id = 2 Then Continue For
                        If MyShip.target_id = script.target_id AndAlso MyShip.target_type_id = script.target_type Then Continue For
                        Await SendMoVeOrder(script, MyTarget)
                        Continue For
                    End If
                    For Each gdata As GalaxyData In GWFleet
                        If gdata.clan_id = MyShip.clan_id Then Continue For
                        If gdata.state = 0 Or gdata.state > 2 Then Continue For
                        If gdata.planet_id IsNot Nothing AndAlso gdata.planet_id = MyShip.planet_id Then
                            If Await SendAttackOrder(script, gdata) Then
                                Exit For
                            End If
                        Else
                            If gdata.x + 2 > MyTarget.x AndAlso gdata.x - 2 < MyTarget.x Then
                                If gdata.y + 2 > MyTarget.y AndAlso gdata.y - 2 < MyTarget.y Then
                                    If Await SendAttackOrder(script, gdata) Then
                                        'script.Working = 0
                                        'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                ElseIf script.Mode = ModeEnum.Hunt Then
                    If MyShip.state = 0 Or MyShip.state > 2 Then Continue For
                    For Each gdata As GalaxyData In GWFleet
                        If gdata.id = script.target_id AndAlso gdata.type = script.target_type Then
                            MyTarget = gdata
                            Exit For
                        End If
                    Next
                    Select Case MyTarget.state
                        Case 0
                            'If MyShip.target_id <> MyTarget.id Then
                            Continue For
                                'If Await SendAttackOrder(script, MyTarget) Then
                    'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                    'Exit For
                    'End If
                'End If
                        Case 1
                            If MyShip.target_id <> MyTarget.id Then
                                If Await SendAttackOrder(script, MyTarget) Then
                                    'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                    'Exit For
                                End If
                            End If
                        Case 2
                            Dim b As Point
                            'b = GetCrossPoint(MyTarget.x, MyTarget.y, MyTarget.target_x, MyTarget.target_y, MyTarget.speed, MyShip.x, MyShip.y, MyShip.speed)
                            b = GetCatchPoint(MyShip, MyTarget)

                            If MyShip.x - 1.2 < MyTarget.x AndAlso MyShip.x + 1.2 > MyTarget.x Then
                                If MyShip.y - 1.2 < MyTarget.y AndAlso MyShip.y + 1.2 > MyTarget.y Then
                                    If MyShip.target_id <> MyTarget.id Then
                                        If Await SendAttackOrder(script, MyTarget) Then
                                            'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                            Continue For
                                        Else
                                            Continue For
                                        End If
                                    Else
                                        Continue For
                                    End If
                                End If
                            End If
                            'If MyShip.x - 1 < b.X AndAlso MyShip.x + 1 > b.X Then
                            '    If MyShip.y - 1 < b.Y AndAlso MyShip.y + 1 > b.Y Then
                            '        If MyShip.target_id <> MyTarget.id Then
                            '            If Await SendAttackOrder(script, MyTarget) Then
                            '                'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                            '                Continue For
                            '            Else
                            '                Continue For
                            '            End If
                            '        End If
                            '    End If
                            'End If
                            If MyShip.target_x - 2 < b.X AndAlso MyShip.target_x + 2 > b.X Then
                                If MyShip.target_y - 2 < b.Y AndAlso MyShip.target_y + 2 > b.Y Then
                                    Continue For
                                End If
                            End If
                            Await SendMoVeOrder(script, b)
                            Continue For
                        Case 4
                            If MyShip.x - 1 > MyTarget.x AndAlso MyShip.x + 1 < MyTarget.x Then
                                If MyShip.y - 1 > MyTarget.y AndAlso MyShip.y + 1 < MyTarget.y Then
                                    If MyShip.target_id <> MyTarget.id Then
                                        If Await SendAttackOrder(script, MyTarget) Then
                                            'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                            Continue For
                                        End If
                                    End If
                                End If
                            End If

                        Case 99
                            Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working because target was destroyed.")
                            Finish(script)
                    End Select
                ElseIf script.Mode = ModeEnum.Move Then

                    If MyShip.state = 0 Or MyShip.state > 2 Then Continue For
                    If MyShip.state = 1 Then
                        rect = New Rect(New Point(MyShip.x - 0.25, MyShip.y - 0.25), New Size(0.5, 0.5))
                        If rect.Contains(script.Point) Then
                            Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working because target was reached.")
                            Finish(script)
                        Else
                            If Await SendMoVeOrder(script, script.Point) Then
                                'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                Continue For
                            End If
                        End If
                        'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                    End If
                    If MyShip.state = 2 Then
                        rect = New Rect(New Point(MyShip.target_x - 0.25, MyShip.target_y - 0.25), New Size(0.5, 0.5))
                        If rect.Contains(script.Point) Then Continue For

                        If Await SendMoVeOrder(script, script.Point) Then
                                'Logger.LogWrite($"Script for [{GWMap.GetShipType(script.id)}]'s [{GWMap.GetShipName(script.id)}] finished working.")
                                Continue For
                            End If
                        End If
                    End If
            End If
        Next
    End Sub

    Public Overloads Async Function SendMoVeOrder(Myscript As GWScript, Mytarget As GalaxyData) As Task(Of Boolean)
        Dim wc2 As New WebClient, s As String = "", gwPack As GWClass
        wc2.Headers.Item(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36 OPR/54.0.2952.71"
        wc2.Headers.Item(HttpRequestHeader.Cookie) = AdmiralsList(Myscript.Admiral)
        Logger.LogWrite($"Script setting [{GWMap.GetShipType(Myscript.id)}]'s [{GWMap.GetShipName(Myscript.id)}] target to [{GWMap.GetPlanetName(Mytarget.id)}] ")
        If BoxxAdmirals.SelectedItem <> Myscript.Admiral Then
            Await Task.Delay(200)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            Await wc2.DownloadStringTaskAsync("http://galaxy.xjedi.com/srv/conn.php?act=client_start")
            Await Task.Delay(500)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await wc2.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_type_id={Mytarget.type}&target_id={Mytarget.id}")
        Else
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_type_id={Mytarget.type}&target_id={Mytarget.id}")
        End If
        gwPack = JsonConvert.DeserializeObject(Of GWClass)(s)
        Logger.LogWrite($"Code: [{gwPack.code}]")
        If gwPack.code = "OK" Then
            'For Each MapObject As GalaxyData In gwPack.galaxy_data
            '    If MapObject.type = 2 Then

            '        For Each item As GalaxyData In GWMap.galaxy_data
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '        For Each item As ShipsData In GWFleet
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '    End If
            'Next
            Return True
        Else
            Return False
        End If
    End Function
    Public Overloads Async Function SendMoVeOrder(Myscript As GWScript, Mytarget As Point) As Task(Of Boolean)
        Dim wc2 As New WebClient, s As String = "", gwPack As GWClass
        wc2.Headers.Item(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36 OPR/54.0.2952.71"
        wc2.Headers.Item(HttpRequestHeader.Cookie) = AdmiralsList(Myscript.Admiral)

        Logger.LogWrite($"Script setting [{GWMap.GetShipType(Myscript.id)}]'s [{GWMap.GetShipName(Myscript.id)}] target point to [({Math.Round(Mytarget.X)};{Math.Round(Mytarget.Y)})]")

        If BoxxAdmirals.SelectedItem <> Myscript.Admiral Then
            Await Task.Delay(200)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            Await wc2.DownloadStringTaskAsync("http://galaxy.xjedi.com/srv/conn.php?act=client_start")
            Await Task.Delay(500)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await wc2.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_x={Math.Round(Mytarget.X)}&target_y={Math.Round(Mytarget.Y)}")
        Else
            Await Task.Delay(200)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_x={Math.Round(Mytarget.X)}&target_y={Math.Round(Mytarget.Y)}")
        End If

        gwPack = JsonConvert.DeserializeObject(Of GWClass)(s)
        Logger.LogWrite($"Code: [{gwPack.code}]")
        If gwPack.code = "OK" Then
            'For Each MapObject As GalaxyData In gwPack.galaxy_data
            '    If MapObject.type = 2 Then

            '        For Each item As GalaxyData In GWMap.galaxy_data
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '        For Each item As ShipsData In GWFleet
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '    End If
            'Next
            Return True
        Else
            Return False
        End If
    End Function
    Public Async Function SendAttackOrder(Myscript As GWScript, MyTarget As GalaxyData) As Task(Of Boolean)
        Dim wc2 As New WebClient, s As String = "", gwPack As GWClass
        wc2.Headers.Item(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36 OPR/54.0.2952.71"
        wc2.Headers.Item(HttpRequestHeader.Cookie) = AdmiralsList(Myscript.Admiral)
        Logger.LogWrite($"Script setting [{GWMap.GetShipType(Myscript.id)}]'s [{GWMap.GetShipName(Myscript.id)}] target to [{GWMap.GetShipType(MyTarget.id)}] [{GWMap.GetShipName(MyTarget.id)}] ")


        If BoxxAdmirals.SelectedItem <> Myscript.Admiral Then
            Await Task.Delay(200)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            Await wc2.DownloadStringTaskAsync("http://galaxy.xjedi.com/srv/conn.php?act=client_start")

            Await Task.Delay(1000)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await wc2.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_type_id={2}&target_id={MyTarget.id}")
        Else
            Await Task.Delay(500)
            Do While WC.IsBusy
                Await Task.Delay(200)
            Loop
            s = Await WC.DownloadStringTaskAsync($"http://galaxy.xjedi.com/srv/conn.php?act=ship_set_target&ship_id={Myscript.id}&target_type_id={2}&target_id={MyTarget.id}")
        End If


        gwPack = JsonConvert.DeserializeObject(Of GWClass)(s)
        Logger.LogWrite($"Code: [{gwPack.code}]")
        If gwPack.code = "OK" Then
            'For Each MapObject As GalaxyData In gwPack.galaxy_data
            '    If MapObject.type = 2 Then

            '        For Each item As GalaxyData In GWMap.galaxy_data
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '        For Each item As ShipsData In GWFleet
            '            If item.id = MapObject.id AndAlso item.type = MapObject.type Then
            '                item.planet_id = MapObject.planet_id
            '                item.x = MapObject.x
            '                item.y = MapObject.y
            '                item.target_x = MapObject.target_x
            '                item.target_y = MapObject.target_y
            '                item.state = MapObject.state
            '            End If
            '        Next
            '    End If
            'Next
            Return True
        Else
            Return False
        End If
    End Function
    Public Sub Add(Ship As GalaxyData, targetid As UInteger, targettype As UShort, Mode As UShort, Admiral As String,
                   Optional NextScript As Integer = -1, Optional targetx As Double = 0, Optional targety As Double = 0)
        Dim a As New GWScript
        If Ship Is Nothing Then Exit Sub
        a.name = Ship.name
        a.image = Ship.image
        a.target_id = targetid
        a.target_type = targettype
        If targettype = 1 Then
            a.target_name = GWMap.GetPlanetName(targetid)
        ElseIf targettype = 2 Then
            a.target_name = GWMap.GetShipName(targetid)
        ElseIf targettype = 0 Then
            a.target_name = "Point"
        End If
        a.Point = New Point(targetx, targety)
        a.id = Ship.id
        a.Working = 0
        a.Mode = Mode
        a.Admiral = Admiral
        a.nextscript_id = NextScript
        If MyScripts.Count = 0 Then a.script_id = 0 Else a.script_id = MyScripts.Last.script_id + 1
        MyScripts.Add(a)
        Save()
    End Sub

    Public Sub Remove(item As GWScript)
        MyScripts.Remove(item)
        Save()
    End Sub

    Public Sub Save()
        File.WriteAllText("Scripts.admiral", JsonConvert.SerializeObject(Me))
    End Sub

    Public Sub Load()
        Dim a As GWScriptHolder
        If File.Exists("Scripts.admiral") Then
            a = JsonConvert.DeserializeObject(Of GWScriptHolder)(File.ReadAllText("Scripts.admiral"))
            MyScripts = a.MyScripts
        End If
    End Sub
    Public Sub Finish(script As GWScript)
        script.Working = 0
        If script.nextscript_id >= 0 Then
            Dim nextscript As GWScript = (From scripts In MyScripts Where scripts.script_id = script.nextscript_id).FirstOrDefault
            If IsNothing(nextscript) = False Then
                nextscript.Working = 1
            End If
        End If
    End Sub
End Class