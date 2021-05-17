Imports System.IO
Imports System.Reflection
Imports System.Windows.Shell
Imports System.Windows
Imports System.Windows.Data
Imports Newtonsoft.Json
Imports System.Text.RegularExpressions
Imports AppLimit.NetSparkle
Class Application
    Dim _sparkle As Sparkle
    Private Sub App_Startup(sender As Object, e As StartupEventArgs)
        CheckForUpdates()
        If File.Exists(AppDomain.CurrentDomain.BaseDirectory & "\Log.txt") = False Then
            File.Create(AppDomain.CurrentDomain.BaseDirectory & "\Log.txt")
        End If

        If File.Exists(AppDomain.CurrentDomain.BaseDirectory & "\notable players.txt") = False Then
            File.Create((AppDomain.CurrentDomain.BaseDirectory & "\notable players.txt"))
        End If

        If Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "\Assets") = False Then
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "\Assets")
        End If
        If Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "\Profiles") = False Then
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "\Profiles")
        End If
        If Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "\Screenshots") = False Then
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "\Screenshots")
        End If
        'If File.Exists(AppDomain.CurrentDomain.BaseDirectory & "\Newtonsoft.Json.dll") = False Then
        '    WriteResourceToFile("Newtonsoft.Json.dll", AppDomain.CurrentDomain.BaseDirectory & "\Newtonsoft.Json.dll")
        'End If
        'If File.Exists(AppDomain.CurrentDomain.BaseDirectory & "\HtmlAgilityPack.dll") = False Then
        '    WriteResourceToFile("HtmlAgilityPack.dll", AppDomain.CurrentDomain.BaseDirectory & "\HtmlAgilityPack.dll")
        'End If
        'If File.Exists(AppDomain.CurrentDomain.BaseDirectory & "\VirtualCanvas.dll") = False Then
        '    WriteResourceToFile("VirtualCanvas.dll", AppDomain.CurrentDomain.BaseDirectory & "\VirtualCanvas.dll")
        'End If
        If e.Args.Length > 0 Then
            If e.Args.First = "/exit" Then
                Dim Current = Process.GetCurrentProcess()
                For Each proc As Process In Process.GetProcessesByName(Current.ProcessName)
                    If proc.Id <> Current.Id Then
                        proc.Kill()
                    End If
                Next
                Current.Kill()
            End If
        End If

        Dim task As JumpTask = New JumpTask With
    {
        .Title = "Выход из Admirals",
        .Arguments = "/exit",
        .Description = "Выход из программы",
        .CustomCategory = "Actions",
        .IconResourcePath = Assembly.GetEntryAssembly().CodeBase,
        .ApplicationPath = Assembly.GetEntryAssembly().CodeBase
    }

        Dim jumpList As JumpList = New JumpList()
        jumpList.JumpItems.Add(task)
        jumpList.ShowFrequentCategory = False
        jumpList.ShowRecentCategory = False

        JumpList.SetJumpList(Application.Current, jumpList)
        If File.Exists("Cookies.txt") Then
            WC.Headers.Item(Net.HttpRequestHeader.Cookie) = File.ReadAllText("Cookies.txt")
        End If

        Dim fileName As String = e.Args?.FirstOrDefault()
        Dim mainWindow As Window1
        Dim a As String
        'a = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("\Admirals.exe", "")
        a = AppDomain.CurrentDomain.BaseDirectory
        Environment.CurrentDirectory = a
        If Not String.IsNullOrWhiteSpace(fileName) AndAlso fileName Like "*asave" Then
            mainWindow = New Window1(fileName)

        ElseIf Not String.IsNullOrWhiteSpace(fileName) AndAlso fileName Like "*admiral" Then
            Dim pattern As String = "mid=\d*", id As String
            Dim profiles As String, admslist As New Dictionary(Of String, String)
            For Each profile As String In IO.Directory.GetFiles("Profiles/", "*.admiral", IO.SearchOption.TopDirectoryOnly)
                profiles = File.ReadAllText(profile)
                If (profile = "" Or profiles = "") AndAlso Not (profiles Like "*mid*") Then Continue For
                id = Mid(Regex.Match(profiles, pattern).Value, 5)
                If admslist.ContainsKey(id) Then
                    IO.File.Delete(profile)
                    Continue For
                Else
                    admslist.Add(id, profile)
                End If
            Next
            profiles = File.ReadAllText(fileName)
            id = Mid(Regex.Match(profiles, pattern).Value, 5)
            If admslist.ContainsKey(id) Then
                If fileName <> a & "\Profiles\" & IO.Path.GetFileName(fileName) Then
                    IO.File.Delete(admslist(id))
                    IO.File.Copy(fileName, a & "\Profiles\" & IO.Path.GetFileName(fileName), True)
                End If
            Else
                IO.File.Copy(fileName, a & "\Profiles\" & IO.Path.GetFileName(fileName), True)
            End If
            MessageBox.Show("Profile installed successfully.")
            End
        Else
            mainWindow = New Window1()
        End If
        mainWindow.Show()
    End Sub

    ' События приложения, например, Startup, Exit и DispatcherUnhandledException,
    ' можно обрабатывать в данном файле.

    Sub CheckForUpdates()
        _sparkle = New Sparkle("https://admirals.000webhostapp.com/versioninfo.xml")
        _sparkle.ShowDiagnosticWindow = False
        _sparkle.StartLoop(True, True)
    End Sub
    Sub WriteResourceToFile(ByVal resourceName As String, ByVal fileName As String)
        Using resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)

            Using file = New FileStream(fileName, FileMode.Create, FileAccess.Write)
                resource.CopyTo(file)
            End Using
        End Using
    End Sub

    Private Sub Application_Exit(sender As Object, e As ExitEventArgs) Handles Me.[Exit]
        '_sparkle.StopLoop()
        My.Settings.Save()
    End Sub
End Class
