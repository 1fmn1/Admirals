Imports System.IO
Imports System.Text.RegularExpressions

Public Class LoggerClass
    Dim writer As StreamWriter
    Private galaxy As Window1
    Sub GetPacket(Packet() As GalaxyData)
        writer = New StreamWriter(fileLog, append:=True)
        For Each Pack As GalaxyData In Packet
            If Pack.act = "setObj" Then
                If Pack.type = 2 Then
                    LogWriteShip(Pack)
                ElseIf Pack.type = 1 Then
                    LogWritePlanet(Pack)
                ElseIf Pack.type = 4 Then
                    LogWritePlayer(Pack)
                Else
                    LogWriteUnknown(Pack)
                End If
            ElseIf Pack.act = "setParam" Then
                If Pack.name = "lastEventID" Then
                    LastEventId = Pack.value
                    writer.WriteLine($"LastEventID: {Pack.value}    <=============================")
                End If
            End If
        Next
        writer.Close()
        writer.Dispose()
    End Sub
    Sub LogWriteUnknown(ByVal pack As GalaxyData)
        LogWrite($"  Something weird happened. Type [{pack.type}]. ID [{pack.id}].")

        writer.WriteLine($"[{DateTime.Now.ToString}] :  Something weird happened. Type [{pack.type}]. ID [{pack.id}].")
    End Sub

    Sub LogWritePlayer(ByVal pack As GalaxyData)
        LogWrite($"  Something happened to player  [{CheckNotable(pack.id)}].")

        writer.WriteLine($"[{DateTime.Now.ToString}] :  Something happened to player  [{CheckNotable(pack.id)}].")
    End Sub

    Sub LogWriteBattle(ByVal pack As GalaxyData)
        LogWrite($"  ")

        writer.WriteLine($"[{DateTime.Now.ToString}] :  ")
    End Sub

    Sub LogWritePlanet(ByVal pack As GalaxyData)
        If pack.citizens_count IsNot Nothing Then
            LogWrite($"  Someone entered or left planet [{GWMap.GetPlanetName(pack.id)}]. Players on planet: [{pack.citizens_total_count}].")

            writer.WriteLine($"[{DateTime.Now.ToString}] :  Someone entered or left planet [{GWMap.GetPlanetName(pack.id)}]. Players on planet: [{pack.citizens_total_count}].")
        Else
            LogWrite($"  Something weird happened. Type [{pack.type}]. ID [{pack.id}].")

            writer.WriteLine($"[{DateTime.Now.ToString}] :  Something weird happened. Type [{pack.type}]. ID [{pack.id}].")
        End If
    End Sub
    Sub LogWriteShip(ByVal pack As GalaxyData)
        If pack.state = 4 Then
            LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   engaged in battle.")

            writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   engaged in battle.")

        ElseIf pack.state = 99 Then
            If pack.destroyed_by_id Is Nothing Then pack.destroyed_by_id = 0
            LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  Ship[{pack.id}] [{GWMap.GetShipName(pack.id)}]   was destroyed by {GWMap.GetShipType(pack.destroyed_by_id)}[{pack.destroyed_by_id}] [{GWMap.GetShipName(pack.destroyed_by_id)}] ({CheckNotable(GWMap.GetShipOwner(pack.destroyed_by_id))}).")

            writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   was destroyed by {UCase(GWMap.GetShipType(pack.id))}[{pack.destroyed_by_id}] [{GWMap.GetShipName(pack.destroyed_by_id)}] ({CheckNotable(GWMap.GetShipOwner(pack.destroyed_by_id))}).")
        ElseIf pack.state = 0 Then
            If pack.owner_character_id IsNot Nothing Then
                LogWrite($"  ({CheckNotable(pack.owner_character_id)})  started building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{pack.name}] type {pack.image}.")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(pack.owner_character_id)})  started building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{pack.name}] type {pack.image}.")
            End If
        ElseIf pack.state Is Nothing Then
            If pack.name IsNot Nothing Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  renamed {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]  to [{pack.name}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  renamed {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] to [{pack.name}].")
            ElseIf pack.members_count IsNot Nothing Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] members count changed to [{pack.members_count}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] members count changed to [{pack.members_count}].")
            ElseIf pack.type Is Nothing Then
                LogWrite($"  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] was deleted from map.")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] was deleted from map.")
            Else
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] permissions changed.")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] permissions changed.")
            End If
        ElseIf pack.state = 1 Then
            If pack.battle_id = 0 Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   finished battle and survived.")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   finished battle and survived.")
            ElseIf pack.members_count IsNot Nothing And pack.planet_id IsNot Nothing Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  finished building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] on [{GWMap.GetPlanetName(pack.planet_id)}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  finished building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] on [{GWMap.GetPlanetName(pack.planet_id)}].")
            ElseIf pack.planet_id IsNot Nothing And pack.pos_order IsNot Nothing Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))}) {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}] arrived on [{GWMap.GetPlanetName(pack.planet_id)}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))}) {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]  arrived on [{GWMap.GetPlanetName(pack.planet_id)}].")
            Else
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   stopped at [{pack.x};{pack.y}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   stopped at [{pack.x};{pack.y}].")
            End If
        ElseIf pack.state = 2 Then
            If pack.target_x Is Nothing Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  finished building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  finished building {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}].")
            Else
                If pack.target_id IsNot Nothing AndAlso pack.target_type_id > 0 Then
                    Select Case pack.target_type_id
                        Case 1
                            LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to [{GWMap.GetPlanetName(pack.target_id)}].")

                            writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to [{GWMap.GetPlanetName(pack.target_id)}].")
                        Case 2
                            LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to {GWMap.GetShipType(pack.target_id)}[{pack.target_id}] [{GWMap.GetShipName(pack.target_id)}].")

                            writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to {GWMap.GetShipType(pack.target_id)}[{pack.target_id}] [{GWMap.GetShipName(pack.target_id)}].")
                        Case 5
                            LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to [{GWMap.GetBuildingName(pack.target_id)}].")

                            writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]  started moving to [{GWMap.GetBuildingName(pack.target_id)}].")
                    End Select

                ElseIf pack.target_type_id = 0 Then
                    LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to [{pack.target_x};{pack.target_y}].")

                    writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   started moving to [{pack.target_x};{pack.target_y}].")
                End If
            End If
        ElseIf pack.state = 3 Then
            If pack.target_type_id = 5 Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   ATTACKS building on [{GWMap.GetPlanetFromBuilding(pack.target_id)}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   ATTACKS building on [{GWMap.GetPlanetFromBuilding(pack.target_id)}].")
            ElseIf pack.target_type_id = 2 Then
                LogWrite($"  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   ATTACKS ({CheckNotable(GWMap.GetShipOwner(pack.id))}'s) {UCase(GWMap.GetShipType(pack.id))}[{pack.target_id}] [{GWMap.GetShipName(pack.target_id)}].")

                writer.WriteLine($"[{DateTime.Now.ToString}] :  ({CheckNotable(GWMap.GetShipOwner(pack.id))})  {UCase(GWMap.GetShipType(pack.id))}[{pack.id}] [{GWMap.GetShipName(pack.id)}]   ATTACKS ({CheckNotable(GWMap.GetShipOwner(pack.id))}'s) {UCase(GWMap.GetShipType(pack.id))}[{pack.target_id}] [{GWMap.GetShipName(pack.target_id)}].")
            End If
        End If
    End Sub

    Public Sub LogWrite(ByVal text As String)
        'writer = New StreamWriter(fileLog, append:=True)
        'writer.WriteLine($"[{DateTime.Now.ToString}] :  {text}")
        Dim pattern As String = "([\S]+\[[\d]+\][\s]+\[[\S]+[\s]*[\S]*[\s]*[\S]*\])"
        Dim idpattern As String = "(\[[\d]+\])"
        Dim id As String
        Dim m As Match
        Dim a As New Hyperlink
        m = Regex.Match(text, pattern)
        a.Inlines.Add(m.Value)
        id = Regex.Match(m.Value, idpattern).Value.Replace("[", "").Replace("]", "")
        a.NavigateUri = New Uri(id, UriKind.Relative)
        AddHandler a.RequestNavigate, New RequestNavigateEventHandler(AddressOf rnavigate)
        txtLogGlobal.Inlines.InsertBefore(txtLogGlobal.Inlines.FirstInline, New Run(Mid(text, m.Index + 1 + m.Length) & vbCrLf))
        txtLogGlobal.Inlines.InsertBefore(txtLogGlobal.Inlines.FirstInline, a)
        txtLogGlobal.Inlines.InsertBefore(txtLogGlobal.Inlines.FirstInline, New Run($"--> " & Mid(text, 1, m.Index)))
        'txtLogGlobal.Inlines.Add(New Run($"--> " & Mid(text, 1, m.Index)))
        'txtLogGlobal.Inlines.Add(a)
        'txtLogGlobal.Inlines.Add(New Run(Mid(text, m.Index + 1 + m.Length) & vbCrLf))
        'writer.WriteLine($"LastEventID: {LastEventId}    <=============================")
        'writer.Close()
        'writer.Dispose()
    End Sub
    Sub rnavigate(sender As Object, arg As RequestNavigateEventArgs)
        Dim a As Integer
        a = CInt(arg.Uri.OriginalString)
        For Each gdata In GWMap.galaxy_data
            If gdata.id = a AndAlso gdata.type = 2 Then
                galaxy.OnSelectionChanged(gdata)
            End If
        Next
    End Sub
    Sub init(a As Window1)
        For Each s As String In File.ReadLines(fileLog)
            If s Like "LastEventID:*" Then
                Dim RegexObj As New Regex("[^\d]")
                LastEventId = Integer.Parse(RegexObj.Replace(s, ""))
            End If
        Next
        galaxy = a
    End Sub
    'act=galaxy_update&lastEventID=130171&lastLogEventID=0&currentAnnouncementID=0
End Class
