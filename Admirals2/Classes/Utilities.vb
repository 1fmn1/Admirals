Imports System.Collections.ObjectModel
Imports System.Net
Imports System.IO
Imports Newtonsoft.Json
Imports Microsoft.Sample.Controls
Imports HtmlAgilityPack

Module Utilities
    'Dim wc As New WebClien
    Public Const Version As String = "v. 1.18.5"
    Public Const fileLog As String = "Log.txt"
    Public Const strTimeCords As String = "x={0:N}
y={1:N}
{2}d {3}h {4}min"
    Public Property PreviewSave As Boolean
    Public Property logString As String
    Public MyBrushes As New Dictionary(Of String, ImageBrush)
    Public AdmiralsList As New Dictionary(Of String, String)
    Public BoxxAdmirals As ComboBox
    Public NotablePlayers As New Dictionary(Of Integer, String)
    Public GWJSon As String
    Public GWMap As New GWClass
    Public Logger As New LoggerClass
    Public LastEventId As Long
    Public GWFleet As New ObservableCollection(Of ShipsData)
    Public GWPlanets As New ObservableCollection(Of GalaxyData)
    Public GWBuildings As New ObservableCollection(Of GalaxyData)
    Public GWStarports As New ObservableCollection(Of GalaxyData)
    Public GWAdmirals As New ObservableCollection(Of GalaxyData)
    Public GWBattles As New ObservableCollection(Of GalaxyData)
    Public GWClans As New ObservableCollection(Of GalaxyData)
    Public GWPLayer As New GalaxyData
    Public WC As New WebClient
    Public txtLogGlobal As TextBlock
    Public rnd As New Random
    Public ShipTypes As New Dictionary(Of String, ShipTypeClass)
    Public BuildingTypes As New Dictionary(Of String, Integer)
    Public StateTypes As New Dictionary(Of Integer, String)
    Public myOptions As New OptionsClass
    Public GWScripts As New GWScriptHolder


    Public ShipsVieww As ListView
    Public PlanetsVieww As ListView
    Public BuildingsVieww As ListView
    Public ScriptsVieww As ListView
    Public BattlesVieww As ListView
    Public ShipInfoo As ShipInfoControl
    Public PlanetInfoo As PlanetInfoControl
    Public BuildingInfoo As BuildingInfoControl
    Public BattleInfoo As BattleInfoControl


    Public selectedID(6) As Integer
    Public MouseOverList As New ObservableCollection(Of TestShape)
    Public cursorLeft As Double
    Public cursorTop As Double
    Public zoom As MapZoom
    Public Scrollerr As ScrollViewer
    Public SelectedObject As TestShape
    Public MouseOverObject As TestShape
    Public grid As Microsoft.Sample.Controls.VirtualCanvas

    'Public pMenu As New ContextMenu
    'Public sMenu As New ContextMenu
    Public mapMenu As New ContextMenu

    Public Function CheckNotable(id As Integer) As String
        If NotablePlayers.ContainsKey(id) Then
            Return NotablePlayers(id)
        Else
            Dim s As String
            If id = 0 Then Return 0
            s = WC.DownloadString($"https://xjedi.com/forum/index.php?showuser={id}")
            Dim doc As New HtmlDocument
            Dim elem As HtmlNode
            doc.LoadHtml(s)
            elem = doc.DocumentNode.SelectSingleNode("//div[@class='nickname']")
            File.AppendAllText("notable players.txt", $"{vbCrLf}{id}={elem.InnerText}")
            NotablePlayers.Add(id, elem.InnerText)
            Return NotablePlayers(id)
        End If
    End Function

    Public Class CatchPoint
        Sub New(p As Point, a As Integer, b As Integer)
            raznica = a
            timespent = b
            point = p
        End Sub
        Public Property point As Point
        Public Property raznica As Integer
        Public Property timespent As Integer
    End Class

    Public Function GetCatchPoint(Myship As ShipsData, Mytarget As ShipsData) As Point
        Dim V2 As Vector, time As Double, t2 As Integer, s2 As Double, p As Point, Path2 As Vector, s1 As Double, t1 As Integer
        Dim Dots As New List(Of CatchPoint)
        V2 = New Vector(Mytarget.target_x - Mytarget.x, Mytarget.target_y - Mytarget.y)

        s2 = Math.Sqrt(Math.Pow((Mytarget.target_x - Mytarget.x), 2) + Math.Pow((Mytarget.target_y - Mytarget.y), 2))
        time = s2 / Mytarget.speed
        V2 = V2 / (time * 30)
        p = New Point(Mytarget.x, Mytarget.y)
        Path2 = New Vector(0, 0)
        Do While Path2.Length < s2
            p = p + V2
            Path2.X = p.X - Mytarget.x
            Path2.Y = p.Y - Mytarget.y
            s1 = Math.Sqrt(Math.Pow((p.X - Myship.x), 2) + Math.Pow((p.Y - Myship.y), 2))
            t1 = Math.Abs(Int(3600 * s1 / Myship.speed))
            t2 = Math.Abs(Int(3600 * Path2.Length / Mytarget.speed))
            If t1 > t2 Then Continue Do
            Dots.Add(New CatchPoint(p, t2 - t1, t2))
        Loop
        If Dots.Count = 0 Then
            Return p
        End If
        Dots.Sort(Function(a, b) a.timespent < b.timespent)
        'For i = Dots.Count - 1 To 5 Step -1
        '    Dots.RemoveAt(i)
        'Next
        'Dots.Sort(Function(a, b) a.raznica < b.raznica)
        'Dim px As Integer, py As Integer
        'px = Math.Round(Dots(0).point.X)
        'py = Math.Round(Dots(0).point.Y)
        Return Dots(0).point
    End Function
    'Public Function GetCrossPoint(X10 As Double, Y10 As Double, X11 As Double, Y11 As Double, V1 As Integer, X20 As Double, Y20 As Double, V2 As Integer) As Point
    '    Dim S1 As Double, t1 As Double, V2x As Double, V2y As Double, T As Double, V1x As Double, V1y As Double
    '    S1 = Math.Sqrt(Math.Pow((X11 - X10), 2) + Math.Pow((Y11 - Y10), 2))
    '    t1 = S1 / V1
    '    V1y = (Y11 - Y10) / t1
    '    V1x = (X11 - X10) / t1
    '    ''V2x* T + X20 = V1x * T + X10
    '    ''V2y* T + y20 = V1y * T + Y10
    '    ''V2y = Math.Sqrt(Math.Pow(V2, 2) - Math.Pow(V2x, 2))
    '    'Dim z As Double, w As Double, c As Double
    '    'z = Y10 - Y20
    '    'w = X20 - X10
    '    'c = X20 * V1y + V1x * z - V1y * X10
    '    'Dim V2X1 As Double, V2X2 As Double, V2Y2 As Double, V2Y1 As Double, D As Double
    '    'D = Math.Pow((2 * z * c), 2) - 4 * (Math.Pow(z, 2) + Math.Pow(w, 2)) *
    '    '    (Math.Pow(c, 2) - Math.Pow(w, 2) * Math.Pow(V2, 2))

    '    'V2X1 = (2 * z * c + Math.Sqrt(D)) / (2 * (Math.Pow(z, 2) + Math.Pow(w, 2)))
    '    'V2X2 = (2 * z * c - Math.Sqrt(D)) / (2 * (Math.Pow(z, 2) + Math.Pow(w, 2)))

    '    'V2Y1 = Math.Sqrt(Math.Pow(V2, 2) - Math.Pow(V2X1, 2))
    '    'V2Y2 = Math.Sqrt(Math.Pow(V2, 2) - Math.Pow(V2X2, 2))

    '    'T = (Y10 - Y20) / (V2Y1 * ((Y11 - Y10) / t1))
    '    'If T < 0 Then
    '    '    T = (Y10 - Y20) / (V2Y2 * ((Y11 - Y10) / t1))
    '    '    Return New Point(V2X2 * T + X20, V2Y2 * T + Y20)
    '    'Else
    '    '    Return New Point(V2X1 * T + X20, V2Y1 * T + Y20)
    '    'End If

    '    Dim x As Double, y As Double, r As Double
    '    For T = 0 To 150 Step 0.01
    '        x = V1x * T + X10
    '        y = V1y * T + Y10
    '        V2x = (x - X20) / T
    '        V2y = (y - Y20) / T
    '        r = Math.Sqrt(Math.Pow(V2x, 2) + Math.Pow(V2y, 2))
    '        If x > X11 AndAlso x > X10 Then
    '            Return New Point(X11, Y11)
    '        End If
    '        If x < X11 AndAlso x < X10 Then
    '            Return New Point(X11, Y11)
    '        End If
    '        If r > V2 - 0.2 AndAlso r < V2 Then
    '            Return New Point(x, y)
    '        End If
    '    Next
    '    'V2* T + Math.Sqrt(Math.Pow(X20, 2) + Math.Pow((Y20, 2))
    'End Function

    Public Class OptionsClass
        Public Property Primary As Double
        Public Property Secondary As Double
        Public Property Window As Size
    End Class


    Public Class CSharpImpl
        <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
        Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class

    Public Class ShipTypeClass
        Property type_name As String
        Property type_id As String
        Property cost As String
        Property construction_time As Integer

        Property speed As UShort
        Public Overrides Function ToString() As String
            Return cost
        End Function

    End Class

    Public Sub SaveMap()
        Dim writer As New StreamWriter($"Screenshots\{DateTime.Now.ToShortDateString.Replace(".", "-")}--{DateTime.Now.ToShortTimeString.Replace(":", "-")}.asave", False)
        writer.Write(GWJSon)
        writer.Close()
        writer.Dispose()
        Logger.LogWrite("Current GW map was saved.")
    End Sub
    Public Sub SaveImage(ByVal image As BitmapImage, ByVal localFilePath As String)
        AddHandler image.DownloadCompleted, Function(sender, args)
                                                Dim encoder = New PngBitmapEncoder()
                                                encoder.Frames.Add(BitmapFrame.Create(CType(sender, BitmapImage)))
                                                'If IO.Directory.Exists(localFilePath) Then

                                                'Else
                                                '    IO.Directory.CreateDirectory(localFilePath)
                                                'End If

                                                Using filestream = New FileStream(System.AppDomain.CurrentDomain.BaseDirectory & "\Assets\" & localFilePath & ".png", FileMode.Create)
                                                    encoder.Save(filestream)
                                                End Using
                                            End Function
    End Sub
End Module
