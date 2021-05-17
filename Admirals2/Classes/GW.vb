
Imports System.ComponentModel
Imports Microsoft.Sample.Controls

Public Class GalaxyData
    Implements INotifyPropertyChanged
    Public Property act As String
    Public Property name As String
        Get
            Return mname
        End Get
        Set(value As String)
            If value IsNot Nothing Then
                mname = value.Replace("&#39;", "'")
            End If
        End Set
    End Property
    Public Property value As Integer
    Public Property type As Integer?
    Public Property id As Integer?
    Public Property prefix As String
    Public Property construct_duration As Integer
    Public Property htmlcolor2 As String
    Public Property ftype As String
    Public Property ptype As String
    Public Property x As Double
        Get
            Return _x
        End Get
        Set(value As Double)
            _x = value
            NotifyPropertyChanged("x")
        End Set
    End Property
    Private _x As Double
    Public Property y As Double
        Get
            Return _y
        End Get
        Set(value As Double)
            _y = value
            NotifyPropertyChanged("y")
        End Set
    End Property
    Private _y As Double
    Public Property clan_id As Integer?
    Public Property image As String
    Public Property image_size As Integer?
    Public Property state As Integer?
    Public Property state_time As Integer?
        Get
            Return mstate_time
        End Get
        Set(value As Integer?)
            mstate_time = value
            If type <> 4 Then Exit Property
            Dim t As Long = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            Dim timesp As TimeSpan
            t = (mstate_time - t)
            If state_data = 0 Then
                timesp = TimeSpan.FromSeconds(t)
                If timesp.Days > 0 Then
                    state_time_string = timesp.ToString("d' days 'h' hours 'm' min'")
                ElseIf timesp.Hours > 0 Then

                    state_time_string = timesp.ToString("h' hours 'm' min'")
                Else
                    state_time_string = timesp.ToString("m' min'")
                End If
            Else
                state_time_string = "Alive"
            End If
        End Set
    End Property
    Private mstate_time As Integer
    Public Property citizens_count As Integer?
    Public Property citizens_total_count As Integer?
    Public Property spec_id As Integer?
    Public Property level As Integer?
    Public Property owner_character_id As Integer?
    Public Property planet_id As Integer?
    Public Property hitpoints As Integer?
    Public Property members_count As Integer?
    Public Property battle_id As Integer?
    Public Property destroyed_by_id As Integer?
    Public Property speed As Integer?
    Public Property capacity As Integer?
    Public Property target_type_id As Integer?
    Public Property target_id As Integer?
    Public Property target_x As Double?
    Public Property target_y As Double?
    Public Property pos_order As Integer?
    Public Property state_data As String
    Public Property state_time_string As String
        Get
            Return mstate_time_string
        End Get
        Set(value As String)
            mstate_time_string = value
            NotifyPropertyChanged("state_time_string")
        End Set
    End Property
    Private mstate_time_string
    Public Property obj_type_id As Integer?
    Public Property obj_id As Integer?
    Public Property health As String
    Public Property last_activity As String
    Public Property rank As Object
    Public Property money_id As Integer?
    Public Property money As Integer?
    Public Property permission As Boolean
    Public Property construct_order As UShort

    Public Property attacker_id As Integer
    Public Property timestart As Long
    Public Property timeleft As Long
        Get
            Return mtimeleft
        End Get
        Set(value As Long)
            mtimeleft = value
            NotifyPropertyChanged("timeleft")
        End Set
    End Property
    Public WithEvents cnvShape As TestShape
    Public WithEvents cnvPath As TestShape
    Private mname As String, mtimeleft As Long
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
    End Sub
    Public Overrides Function ToString() As String
        Return $"{name}{vbTab}[{id}]"
    End Function
End Class

'Public Class GWShip
'    Inherits GalaxyData
'    Implements IVirtualChild

'    Public ReadOnly Property Bounds As Rect Implements IVirtualChild.Bounds
'        Get
'            Throw New NotImplementedException()
'        End Get
'    End Property

'    Public ReadOnly Property Visual As UIElement Implements IVirtualChild.Visual
'        Get
'            Throw New NotImplementedException()
'        End Get
'    End Property

'    Public Event BoundsChanged As EventHandler Implements IVirtualChild.BoundsChanged

'    Public Sub DisposeVisual() Implements IVirtualChild.DisposeVisual
'        Throw New NotImplementedException()
'    End Sub

'    Public Function CreateVisual(parent As VirtualCanvas) As UIElement Implements IVirtualChild.CreateVisual
'        Throw New NotImplementedException()
'    End Function
'End Class

Public Class PlayerData
    Public Property member_name As String
    Public Property ja_activity As String
    Public Property access As Integer
    Public Property access_code As String
    Public Property member_id As Integer
End Class

Public Class ShipsData
    Inherits GalaxyData
    Implements INotifyPropertyChanged
    Public Property owner_id As Integer
    Public Overloads Property owner_character_id As String
    Public Property planet_name As String
    Public Property arrive_timespan As TimeSpan
    Public Overloads Property x As Double?
        Get
            Return _x
        End Get
        Set(value As Double?)
            _x = value
            NotifyPropertyChanged("x")
        End Set
    End Property
    Private _x As Double
    Public Overloads Property y As Double?
        Get
            Return _y
        End Get
        Set(value As Double?)
            _y = value
            NotifyPropertyChanged("y")
        End Set
    End Property
    Private _y As Double
    Public Property arrive_time As String
        Get
            Return _arrivetime
        End Get
        Set(value As String)
            _arrivetime = value
            NotifyPropertyChanged("arrive_time")
        End Set
    End Property
    Private _arrivetime As String
    Public Property target_name As String
    Public Shadows Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Sub New()
        name = "Preview"
        id = -1
        owner_id = 12824
        owner_character_id = CheckNotable(owner_id)
        x = 0
        y = 0
        clan_id = 3
        type = 2
        state = 1
        target_x = 0
        target_y = 0
        speed = 10
        members_count = 2
        planet_name = "Preview"
        image = "x-wing"
    End Sub
    Sub New(ByVal pack As GalaxyData)
        name = pack.name
        id = pack.id
        clan_id = pack.clan_id
        owner_id = pack.owner_character_id
        owner_character_id = CheckNotable(pack.owner_character_id)
        x = pack.x
        y = pack.y
        type = pack.type
        state = pack.state
        construct_order = pack.construct_order
        target_x = pack.target_x
        target_y = pack.target_y
        target_id = pack.target_id
        target_type_id = pack.target_type_id
        speed = pack.speed
        members_count = pack.members_count
        planet_id = pack.planet_id
        planet_name = GWMap.GetPlanetName(pack.planet_id)
        image = pack.image
        capacity = pack.capacity
        cnvShape = pack.cnvShape
        members_count = pack.members_count
        If state = 2 And target_x IsNot Nothing Then
            arrive_timespan = TimeSpan.FromMinutes(Math.Round(CDbl(Math.Sqrt(Math.Pow(x - target_x, 2) + Math.Pow(y - target_y, 2)) * 60 / speed)))
            If arrive_timespan.Days > 0 Then
                arrive_time = arrive_timespan.ToString("d' days 'h' hours 'm' min'")
            ElseIf arrive_timespan.Hours > 0 Then

                arrive_time = arrive_timespan.ToString("h' hours 'm' min'")
            Else
                arrive_time = arrive_timespan.ToString("m' min'")
            End If
        End If
        If target_type_id IsNot Nothing AndAlso target_id IsNot Nothing Then
            If target_type_id = 1 Then
                target_name = GWMap.GetPlanetName(target_id)
            End If
            If target_type_id = 2 Then
                target_name = GWMap.GetShipName(target_id)
            End If
            If target_type_id = 5 Then
                target_name = GWMap.GetBuildingName(target_id)
            End If
            If target_type_id = 0 Then
                target_name = "Space"
            End If
        Else
            target_name = "Space"
        End If
        'image = New TransformedBitmap(New BitmapImage(New Uri($"pack://siteoforigin:,,,/Assets/Ships/{pack.image}.png")), New ScaleTransform(0.15, 0.15))
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
    End Sub
    Public Sub Renew()
        Try
            If state = 2 And target_x IsNot Nothing Then

                arrive_timespan = TimeSpan.FromMinutes(Math.Round(CDbl(Math.Sqrt(Math.Pow(x - target_x, 2) + Math.Pow(y - target_y, 2)) * 60 / speed)))
                If arrive_timespan.Days > 0 Then
                    arrive_time = arrive_timespan.ToString("d' days 'h' hours 'm' min'")
                ElseIf arrive_timespan.Hours > 0 Then

                    arrive_time = arrive_timespan.ToString("h' hours 'm' min'")
                Else
                    arrive_time = arrive_timespan.ToString("m' min'")
                End If
            End If
        Catch
        End Try
    End Sub
    Public Overrides Function ToString() As String
        Return $"{name}{vbTab}[{id}]"
    End Function
End Class

Public Class GWClass
    Public Property code As String
    Public Property time As Double
    Public Property galaxy_data As GalaxyData()
    Public Property player_data As PlayerData
    Public Property [error] As String
    Public Property cost As Integer?
    Public LastEventIDCheck As Long
    Function GetBuilding(BuildingID As Integer) As GalaxyData
        For Each elem In galaxy_data
            If elem.type = 5 Then
                If elem.id = BuildingID Then Return elem
            End If
        Next
        Return Nothing
    End Function
    Function GetShip(ShipID As Integer) As GalaxyData
        For Each elem In galaxy_data
            If elem.type = 2 Then
                If elem.id = ShipID Then Return elem
            End If
        Next
        Return Nothing
    End Function

    Function GetShipType(ShipID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 2 Then
                If elem.id = ShipID Then Return elem.image
            End If
        Next
        Return "Ship"
    End Function

    Function GetShipOwner(ShipID As Integer) As Integer
        For Each elem In galaxy_data
            If elem.type = 2 Then
                If elem.id = ShipID Then Return elem.owner_character_id
            End If
        Next
        Return 0
    End Function

    Function GetShipName(ShipID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 2 Then
                If elem.id = ShipID Then Return elem.name
            End If
        Next
        Return "-"
    End Function

    Function GetPlanetName(PlanetID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 1 Then
                If elem.id = PlanetID Then Return elem.name
            End If
        Next
        Return "space"
    End Function

    Function GetPlanetFromBuilding(id As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 5 Then
                If elem.id = id Then Return GetPlanetName(elem.planet_id)

            End If
        Next
        Return 0
    End Function

    Function GetClanPrefix(ClanID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 3 Then
                If elem.id = ClanID Then
                    Return elem.prefix '.Replace("#", "")
                End If
            End If
        Next
        Return 0
    End Function

    Function GetClanColor(ClanID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 3 Then
                If elem.id = ClanID Then
                    Return elem.htmlcolor2 '.Replace("#", "")
                End If
            End If
        Next
        Return 0
    End Function

    Function GetClanFullName(ClanID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 3 Then
                If elem.id = ClanID Then
                    Return elem.name '.Replace("#", "")
                End If
            End If
        Next
        Return 0
    End Function

    Function GetBuildingName(ClanID As Integer) As String
        For Each elem In galaxy_data
            If elem.type = 5 Then
                If elem.id = ClanID Then
                    Return elem.name & " on " & GetPlanetName(elem.planet_id) '.Replace("#", "")
                End If
            End If
        Next
        Return 0
    End Function
End Class


Public Class GalaxyDataEqualityComparer
    Implements IEqualityComparer(Of GalaxyData)

    Public Function Equals(ByVal b1 As GalaxyData, ByVal b2 As GalaxyData) As Boolean Implements IEqualityComparer(Of GalaxyData).Equals
        If b2 Is Nothing AndAlso b1 Is Nothing Then
            Return True
        ElseIf b1 Is Nothing OrElse b2 Is Nothing Then
            Return False
        ElseIf b1.type = b2.type AndAlso b1.id = b2.id AndAlso b1.name = b2.name Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetHashCode(ByVal bx As GalaxyData) As Integer Implements IEqualityComparer(Of GalaxyData).GetHashCode
        Dim hCode As Integer = bx.type Xor bx.id
        Return hCode.GetHashCode()
    End Function
End Class