Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Documents
Imports System.Globalization
Imports System.IO

<ValueConversion(GetType(String), GetType(ImageSource))>
Public Class MyClanRectConverter
    Implements IValueConverter
    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageName As String = CStr(value)
        Try
            Return New TransformedBitmap(MyBrushes(imageName).ImageSource, New ScaleTransform(0.1, 0.1))
        Catch
            Return Nothing
        End Try
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function


    Public Sub SaveImage(ByVal image As BitmapImage, ByVal localFilePath As String)
        AddHandler image.DownloadCompleted, Function(sender, args)
                                                Dim encoder = New PngBitmapEncoder()
                                                encoder.Frames.Add(BitmapFrame.Create(CType(sender, BitmapImage)))

                                                Using filestream = New FileStream(System.AppDomain.CurrentDomain.BaseDirectory & "\Assets\" & localFilePath & ".png", FileMode.Create)
                                                    encoder.Save(filestream)
                                                End Using
                                            End Function
    End Sub
End Class


<ValueConversion(GetType(String), GetType(ImageSource))>
Public Class MyShipImageConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageName As String = CStr(value)

        Try
            Return MyBrushes(imageName & "_s").ImageSource
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/ship/{imageName}_s.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, imageName & "_s")
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            Return myimg
        End Try
    End Function

    Public Sub SaveImage(ByVal image As BitmapImage, ByVal localFilePath As String)
        AddHandler image.DownloadCompleted, Function(sender, args)
                                                Dim encoder = New PngBitmapEncoder()
                                                encoder.Frames.Add(BitmapFrame.Create(CType(sender, BitmapImage)))

                                                Using filestream = New FileStream(System.AppDomain.CurrentDomain.BaseDirectory & "\Assets\" & localFilePath & ".png", FileMode.Create)
                                                    encoder.Save(filestream)
                                                End Using
                                            End Function
    End Sub

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class


<ValueConversion(GetType(Integer), GetType(Brush))>
Public Class MyClanConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageName As Integer = value
        Dim clan_color As String
        clan_color = GWMap.GetClanColor(imageName)
        If clan_color = "0" Then
            clan_color = "#b4b4b4"
        End If
        Return New SolidColorBrush(ColorConverter.ConvertFromString(clan_color))
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class


<ValueConversion(GetType(Integer), GetType(String))>
Public Class MyPlanetNameConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim PlanetID As Integer = CInt(value)
        Try
            Return GWMap.GetPlanetName(PlanetID)
        Catch
            Return "Unknown"
        End Try
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class


<ValueConversion(GetType(String), GetType(ImageBrush))>
Public Class MyImageNameConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim imageName As String = CStr(value)

        Try
            Return MyBrushes(imageName)
        Catch
            Dim myimg As New BitmapImage
            myimg.BeginInit()
            myimg.UriSource = New Uri($"http://galaxy.xjedi.com/img/ship/{imageName}.png", UriKind.Absolute)
            myimg.CacheOption = BitmapCacheOption.OnLoad
            myimg.EndInit()
            SaveImage(myimg, imageName & "_s")
            'With {.UriCachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.CacheIfAvailable)}
            Return New ImageBrush(myimg)
        End Try
    End Function

    Public Sub SaveImage(ByVal image As BitmapImage, ByVal localFilePath As String)
        AddHandler image.DownloadCompleted, Function(sender, args)
                                                Dim encoder = New PngBitmapEncoder()
                                                encoder.Frames.Add(BitmapFrame.Create(CType(sender, BitmapImage)))

                                                Using filestream = New FileStream(System.AppDomain.CurrentDomain.BaseDirectory & "\Assets\" & localFilePath & ".png", FileMode.Create)
                                                    encoder.Save(filestream)
                                                End Using
                                            End Function
    End Sub

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class

<ValueConversion(GetType(Long), GetType(String))>
Public Class MyDateConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert

        Return DateTimeOffset.FromUnixTimeSeconds(value).LocalDateTime.ToString("f")
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class

<ValueConversion(GetType(Integer), GetType(ShipsData))>
Public Class MyShipIDConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert

        Return GWFleet.FirstOrDefault(Function(c) c.id = value)
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class
<ValueConversion(GetType(Integer), GetType(String))>
Public Class MyStateConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
        If StateTypes.ContainsKey(value) Then Return StateTypes(value)
        Return value
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class

'<ValueConversion(GetType(String), GetType(String))>
'Public Class GridLengthConverter
'    Implements IValueConverter

'    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
'        Dim a As Double = value
'        Return $"{a}*"
'        'Try
'        '    Return New GridLength(a, GridUnitType.Star)
'        'Catch
'        '    Return New GridLength(1, GridUnitType.Star)
'        'End Try
'    End Function

'    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
'        Dim a As String = value
'        Return CInt(a.Replace("*", ""))
'    End Function
'End Class

Public Class GridViewSort
    Public Shared Function GetCommand(ByVal obj As DependencyObject) As ICommand
        Return CType(obj.GetValue(CommandProperty), ICommand)
    End Function

    Public Shared Sub SetCommand(ByVal obj As DependencyObject, ByVal value As ICommand)
        obj.SetValue(CommandProperty, value)
    End Sub

    Public Shared ReadOnly CommandProperty As DependencyProperty = DependencyProperty.RegisterAttached("Command", GetType(ICommand), GetType(GridViewSort), New UIPropertyMetadata(Nothing, Function(o, e)
                                                                                                                                                                                                Dim listView As ItemsControl = TryCast(o, ItemsControl)

                                                                                                                                                                                                If listView IsNot Nothing Then

                                                                                                                                                                                                    If Not GetAutoSort(listView) Then

                                                                                                                                                                                                        If e.OldValue IsNot Nothing AndAlso e.NewValue Is Nothing Then
                                                                                                                                                                                                            listView.[RemoveHandler](GridViewColumnHeader.ClickEvent, New RoutedEventHandler(AddressOf ColumnHeader_Click))
                                                                                                                                                                                                        End If

                                                                                                                                                                                                        If e.OldValue Is Nothing AndAlso e.NewValue IsNot Nothing Then
                                                                                                                                                                                                            listView.[AddHandler](GridViewColumnHeader.ClickEvent, New RoutedEventHandler(AddressOf ColumnHeader_Click))
                                                                                                                                                                                                        End If
                                                                                                                                                                                                    End If
                                                                                                                                                                                                End If
                                                                                                                                                                                            End Function))

    Public Shared Function GetAutoSort(ByVal obj As DependencyObject) As Boolean
        Return CBool(obj.GetValue(AutoSortProperty))
    End Function

    Public Shared Sub SetAutoSort(ByVal obj As DependencyObject, ByVal value As Boolean)
        obj.SetValue(AutoSortProperty, value)
    End Sub

    Public Shared ReadOnly AutoSortProperty As DependencyProperty = DependencyProperty.RegisterAttached("AutoSort", GetType(Boolean), GetType(GridViewSort), New UIPropertyMetadata(False, Function(o, e)
                                                                                                                                                                                               Dim listView As ListView = TryCast(o, ListView)

                                                                                                                                                                                               If listView IsNot Nothing Then

                                                                                                                                                                                                   If GetCommand(listView) Is Nothing Then
                                                                                                                                                                                                       Dim oldValue As Boolean = CBool(e.OldValue)
                                                                                                                                                                                                       Dim newValue As Boolean = CBool(e.NewValue)

                                                                                                                                                                                                       If oldValue AndAlso Not newValue Then
                                                                                                                                                                                                           listView.[RemoveHandler](GridViewColumnHeader.ClickEvent, New RoutedEventHandler(AddressOf ColumnHeader_Click))
                                                                                                                                                                                                       End If

                                                                                                                                                                                                       If Not oldValue AndAlso newValue Then
                                                                                                                                                                                                           listView.[AddHandler](GridViewColumnHeader.ClickEvent, New RoutedEventHandler(AddressOf ColumnHeader_Click))
                                                                                                                                                                                                       End If
                                                                                                                                                                                                   End If
                                                                                                                                                                                               End If
                                                                                                                                                                                           End Function))

    Public Shared Function GetPropertyName(ByVal obj As DependencyObject) As String
        Return CStr(obj.GetValue(PropertyNameProperty))
    End Function

    Public Shared Sub SetPropertyName(ByVal obj As DependencyObject, ByVal value As String)
        obj.SetValue(PropertyNameProperty, value)
    End Sub

    Public Shared ReadOnly PropertyNameProperty As DependencyProperty = DependencyProperty.RegisterAttached("PropertyName", GetType(String), GetType(GridViewSort), New UIPropertyMetadata(Nothing))

    Public Shared Function GetShowSortGlyph(ByVal obj As DependencyObject) As Boolean
        Return CBool(obj.GetValue(ShowSortGlyphProperty))
    End Function

    Public Shared Sub SetShowSortGlyph(ByVal obj As DependencyObject, ByVal value As Boolean)
        obj.SetValue(ShowSortGlyphProperty, value)
    End Sub

    Public Shared ReadOnly ShowSortGlyphProperty As DependencyProperty = DependencyProperty.RegisterAttached("ShowSortGlyph", GetType(Boolean), GetType(GridViewSort), New UIPropertyMetadata(True))

    Public Shared Function GetSortGlyphAscending(ByVal obj As DependencyObject) As ImageSource
        Return CType(obj.GetValue(SortGlyphAscendingProperty), ImageSource)
    End Function

    Public Shared Sub SetSortGlyphAscending(ByVal obj As DependencyObject, ByVal value As ImageSource)
        obj.SetValue(SortGlyphAscendingProperty, value)
    End Sub

    Public Shared ReadOnly SortGlyphAscendingProperty As DependencyProperty = DependencyProperty.RegisterAttached("SortGlyphAscending", GetType(ImageSource), GetType(GridViewSort), New UIPropertyMetadata(Nothing))

    Public Shared Function GetSortGlyphDescending(ByVal obj As DependencyObject) As ImageSource
        Return CType(obj.GetValue(SortGlyphDescendingProperty), ImageSource)
    End Function

    Public Shared Sub SetSortGlyphDescending(ByVal obj As DependencyObject, ByVal value As ImageSource)
        obj.SetValue(SortGlyphDescendingProperty, value)
    End Sub

    Public Shared ReadOnly SortGlyphDescendingProperty As DependencyProperty = DependencyProperty.RegisterAttached("SortGlyphDescending", GetType(ImageSource), GetType(GridViewSort), New UIPropertyMetadata(Nothing))

    Private Shared Function GetSortedColumnHeader(ByVal obj As DependencyObject) As GridViewColumnHeader
        Return CType(obj.GetValue(SortedColumnHeaderProperty), GridViewColumnHeader)
    End Function

    Private Shared Sub SetSortedColumnHeader(ByVal obj As DependencyObject, ByVal value As GridViewColumnHeader)
        obj.SetValue(SortedColumnHeaderProperty, value)
    End Sub

    Private Shared ReadOnly SortedColumnHeaderProperty As DependencyProperty = DependencyProperty.RegisterAttached("SortedColumnHeader", GetType(GridViewColumnHeader), GetType(GridViewSort), New UIPropertyMetadata(Nothing))

    Private Shared Sub ColumnHeader_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim headerClicked As GridViewColumnHeader = TryCast(e.OriginalSource, GridViewColumnHeader)

        If headerClicked IsNot Nothing AndAlso headerClicked.Column IsNot Nothing Then
            Dim propertyName As String = GetPropertyName(headerClicked.Column)

            If Not String.IsNullOrEmpty(propertyName) Then
                Dim listView As ListView = GetAncestor(Of ListView)(headerClicked)

                If listView IsNot Nothing Then
                    Dim command As ICommand = GetCommand(listView)

                    If command IsNot Nothing Then

                        If command.CanExecute(propertyName) Then
                            command.Execute(propertyName)
                        End If
                    ElseIf GetAutoSort(listView) Then
                        ApplySort(listView.Items, propertyName, listView, headerClicked)
                    End If
                End If
            End If
        End If
    End Sub

    Public Shared Function GetAncestor(Of T As DependencyObject)(ByVal reference As DependencyObject) As T
        Dim parent As DependencyObject = VisualTreeHelper.GetParent(reference)

        While Not (TypeOf parent Is T)
            parent = VisualTreeHelper.GetParent(parent)
        End While

        If parent IsNot Nothing Then
            Return CType(parent, T)
        Else
            Return Nothing
        End If
    End Function

    Public Shared Sub ApplySort(ByVal view As ICollectionView, ByVal propertyName As String, ByVal listView As ListView, ByVal sortedColumnHeader As GridViewColumnHeader)
        Dim direction As ListSortDirection = ListSortDirection.Ascending

        If view.SortDescriptions.Count > 0 Then
            Dim currentSort As SortDescription = view.SortDescriptions(0)

            If currentSort.PropertyName = propertyName Then

                If currentSort.Direction = ListSortDirection.Ascending Then
                    direction = ListSortDirection.Descending
                Else
                    direction = ListSortDirection.Ascending
                End If
            End If

            view.SortDescriptions.Clear()
            Dim currentSortedColumnHeader As GridViewColumnHeader = GetSortedColumnHeader(listView)

            If currentSortedColumnHeader IsNot Nothing Then
                RemoveSortGlyph(currentSortedColumnHeader)
            End If
        End If
        Try
            If Not String.IsNullOrEmpty(propertyName) Then
                view.SortDescriptions.Add(New SortDescription(propertyName, direction))
                If GetShowSortGlyph(listView) Then AddSortGlyph(sortedColumnHeader, direction, If(direction = ListSortDirection.Ascending, GetSortGlyphAscending(listView), GetSortGlyphDescending(listView)))
                SetSortedColumnHeader(listView, sortedColumnHeader)
            End If
        Catch
        End Try
    End Sub

    Private Shared Sub AddSortGlyph(ByVal columnHeader As GridViewColumnHeader, ByVal direction As ListSortDirection, ByVal sortGlyph As ImageSource)
        Dim adornerLayer As AdornerLayer = AdornerLayer.GetAdornerLayer(columnHeader)
        adornerLayer.Add(New SortGlyphAdorner(columnHeader, direction, sortGlyph))
    End Sub

    Private Shared Sub RemoveSortGlyph(ByVal columnHeader As GridViewColumnHeader)
        Dim adornerLayer As AdornerLayer = AdornerLayer.GetAdornerLayer(columnHeader)
        Dim adorners As Adorner() = adornerLayer.GetAdorners(columnHeader)

        If adorners IsNot Nothing Then

            For Each adorner As Adorner In adorners
                If TypeOf adorner Is SortGlyphAdorner Then adornerLayer.Remove(adorner)
            Next
        End If
    End Sub

    Private Class SortGlyphAdorner
        Inherits Adorner

        Private _columnHeader As GridViewColumnHeader
        Private _direction As ListSortDirection
        Private _sortGlyph As ImageSource

        Public Sub New(ByVal columnHeader As GridViewColumnHeader, ByVal direction As ListSortDirection, ByVal sortGlyph As ImageSource)
            MyBase.New(columnHeader)
            _columnHeader = columnHeader
            _direction = direction
            _sortGlyph = sortGlyph
        End Sub

        Private Function GetDefaultGlyph() As Geometry
            Dim x1 As Double = _columnHeader.ActualWidth - 13
            Dim x2 As Double = x1 + 10
            Dim x3 As Double = x1 + 5
            Dim y1 As Double = _columnHeader.ActualHeight / 2 - 3
            Dim y2 As Double = y1 + 5

            If _direction = ListSortDirection.Ascending Then
                Dim tmp As Double = y1
                y1 = y2
                y2 = tmp
            End If

            Dim pathSegmentCollection As PathSegmentCollection = New PathSegmentCollection()
            pathSegmentCollection.Add(New LineSegment(New Point(x2, y1), True))
            pathSegmentCollection.Add(New LineSegment(New Point(x3, y2), True))
            Dim pathFigure As PathFigure = New PathFigure(New Point(x1, y1), pathSegmentCollection, True)
            Dim pathFigureCollection As PathFigureCollection = New PathFigureCollection()
            pathFigureCollection.Add(pathFigure)
            Dim pathGeometry As PathGeometry = New PathGeometry(pathFigureCollection)
            Return pathGeometry
        End Function

        Protected Overrides Sub OnRender(ByVal drawingContext As DrawingContext)
            MyBase.OnRender(drawingContext)

            If _sortGlyph IsNot Nothing Then
                Dim x As Double = _columnHeader.ActualWidth - 13
                Dim y As Double = _columnHeader.ActualHeight / 2 - 5
                Dim rect As Rect = New Rect(x, y, 10, 10)
                drawingContext.DrawImage(_sortGlyph, rect)
            Else
                drawingContext.DrawGeometry(Brushes.LightGray, New Pen(Brushes.Gray, 1.0), GetDefaultGlyph())
            End If
        End Sub
    End Class
End Class





